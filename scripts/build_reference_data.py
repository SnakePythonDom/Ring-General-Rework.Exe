#!/usr/bin/env python3
"""Build deterministic countries/regions seed data from legacy BAKI database."""

from __future__ import annotations

import argparse
import pathlib
import re
import sqlite3
import unicodedata
from typing import Dict, Iterable, List, Optional, Sequence, Tuple

DEFAULT_COUNTRY_ID = "COUNTRY_DEFAULT"
DEFAULT_REGION_ID = "REGION_DEFAULT"
DEFAULT_COUNTRY_CODE = "WLD"
DEFAULT_COUNTRY_NAME = "World"
DEFAULT_REGION_NAME = "Global"


def normalize_country(name: str) -> str:
    if not name:
        return ""
    normalized = unicodedata.normalize("NFKC", name)
    letters_only = []
    for char in normalized:
        if char.isalpha():
            letters_only.append(char)
        else:
            letters_only.append(" ")
    collapsed = re.sub(r"\s+", " ", "".join(letters_only)).strip()
    return collapsed.casefold()


CODE_OVERRIDES = {
    normalize_country("United States"): "USA",
    normalize_country("United Kingdom"): "GBR",
}


def clean_display(name: str) -> str:
    if not name:
        return ""
    collapsed = re.sub(r"\s+", " ", name).strip()
    return collapsed


def display_from_normalized(normalized: str) -> str:
    return " ".join(word.capitalize() for word in normalized.split())


def slugify(name: str) -> str:
    normalized = normalize_country(name)
    slug = re.sub(r"\s+", "_", normalized).upper()
    return slug or "UNKNOWN"


def build_alias_map() -> Dict[str, str]:
    # Keep the alias map intentionally short; unknowns still seed as-is.
    alias_groups = {
        "United States": ["usa", "u.s.a", "u.s.", "u.s", "united states of america"],
        "United Kingdom": ["uk", "u.k.", "u.k", "great britain", "britain"],
    }
    alias_map: Dict[str, str] = {}
    for canonical, aliases in alias_groups.items():
        for alias in aliases:
            alias_map[normalize_country(alias)] = canonical
    return alias_map


def find_country_column(
    conn: sqlite3.Connection, table: str, candidates: Sequence[str]
) -> Tuple[Optional[str], Optional[str]]:
    rows = conn.execute(f"PRAGMA legacy.table_info({table})").fetchall()
    columns = [(row[1], (row[2] or "").upper()) for row in rows]
    for candidate in candidates:
        for name, col_type in columns:
            if name.lower() == candidate.lower():
                return name, col_type
    for name, col_type in columns:
        if "COUNTRY" in name.upper():
            return name, col_type
    return None, None


def load_distinct_values(
    conn: sqlite3.Connection, table: str, column: Optional[str], col_type: Optional[str]
) -> List[str]:
    if not column:
        return []
    if col_type and "INT" in col_type:
        non_numeric_count = conn.execute(
            f"""
            SELECT COUNT(*)
            FROM legacy.{table}
            WHERE {column} IS NOT NULL
              AND TRIM({column}) != ''
              AND CAST({column} AS TEXT) GLOB '*[^0-9]*'
            """
        ).fetchone()[0]
        if non_numeric_count == 0:
            cursor = conn.execute(
                f"""
                SELECT DISTINCT c.countryName
                FROM legacy.{table} t
                JOIN legacy.countries c ON c.countryID = t.{column}
                WHERE t.{column} IS NOT NULL
                  AND c.countryName IS NOT NULL
                  AND TRIM(c.countryName) != ''
                """
            )
        else:
            cursor = conn.execute(
                f"""
                SELECT DISTINCT {column}
                FROM legacy.{table}
                WHERE {column} IS NOT NULL
                  AND TRIM({column}) != ''
                """
            )
    else:
        cursor = conn.execute(
            f"""
            SELECT DISTINCT {column}
            FROM legacy.{table}
            WHERE {column} IS NOT NULL
              AND TRIM({column}) != ''
            """
        )
    return [row[0] for row in cursor.fetchall()]


def build_country_mapping(
    raw_values: Iterable[str], alias_map: Dict[str, str]
) -> Dict[str, Dict[str, str]]:
    countries_by_norm: Dict[str, str] = {}
    for raw in raw_values:
        cleaned = clean_display(raw)
        normalized = normalize_country(cleaned)
        if not normalized:
            continue
        if normalized in alias_map:
            display = alias_map[normalized]
        else:
            display = display_from_normalized(normalized)
        existing = countries_by_norm.get(normalized)
        if existing is None or display.casefold() < existing.casefold():
            countries_by_norm[normalized] = display

    used_ids = {DEFAULT_COUNTRY_ID}
    used_codes = {DEFAULT_COUNTRY_CODE}
    mapping: Dict[str, Dict[str, str]] = {}

    for normalized, display in sorted(
        countries_by_norm.items(), key=lambda item: (item[0], item[1].casefold())
    ):
        base_id = f"COUNTRY_{slugify(display)}"
        country_id = base_id
        suffix = 2
        while country_id in used_ids:
            country_id = f"{base_id}_{suffix}"
            suffix += 1
        used_ids.add(country_id)

        override = CODE_OVERRIDES.get(normalized)
        if override:
            code = override
        else:
            letters = re.sub(r"[^A-Za-z]", "", display)
            code = (letters[:3] or "UNK").upper()
        if len(code) < 3:
            code = code.ljust(3, "X")

        base_code = code
        code_suffix = 1
        while code in used_codes:
            code = f"{base_code[:2]}{code_suffix}"
            code_suffix += 1
        used_codes.add(code)

        mapping[normalized] = {"id": country_id, "name": display, "code": code}

    return mapping


def build_region_rows(
    conn: sqlite3.Connection, country_mapping: Dict[str, Dict[str, str]]
) -> List[Dict[str, str]]:
    legacy_countries = {
        row[0]: row[1]
        for row in conn.execute(
            "SELECT countryID, countryName FROM legacy.countries WHERE countryName IS NOT NULL"
        ).fetchall()
    }
    legacy_regions = conn.execute(
        "SELECT regionParent, regionName FROM legacy.regions WHERE regionName IS NOT NULL AND TRIM(regionName) != ''"
    ).fetchall()

    used_region_ids = {DEFAULT_REGION_ID}
    region_rows: List[Dict[str, str]] = []
    region_names_by_country: Dict[str, set[str]] = {}

    for normalized, data in country_mapping.items():
        region_names_by_country.setdefault(data["id"], set()).add("Other")

    for parent_id, name in legacy_regions:
        country_name = legacy_countries.get(parent_id)
        if not country_name:
            continue
        normalized = normalize_country(country_name)
        country = country_mapping.get(normalized)
        if not country:
            continue
        region_names_by_country.setdefault(country["id"], set()).add(clean_display(name))

    for country_id, region_names in sorted(region_names_by_country.items()):
        for region_name in sorted(region_names, key=lambda v: normalize_country(v)):
            base_id = f"REGION_{country_id}_{slugify(region_name)}"
            region_id = base_id
            suffix = 2
            while region_id in used_region_ids:
                region_id = f"{base_id}_{suffix}"
                suffix += 1
            used_region_ids.add(region_id)
            region_rows.append(
                {
                    "id": region_id,
                    "country_id": country_id,
                    "name": region_name,
                }
            )

    return region_rows


def sql_escape(value: str) -> str:
    return value.replace("'", "''")


def write_seed_files(
    output_dir: pathlib.Path,
    countries: Dict[str, Dict[str, str]],
    regions: List[Dict[str, str]],
    alias_map: Dict[str, str],
) -> None:
    countries_path = output_dir / "seed_countries.sql"
    regions_path = output_dir / "seed_regions.sql"
    aliases_path = output_dir / "seed_country_aliases.sql"

    with countries_path.open("w", encoding="utf-8") as handle:
        handle.write("-- Auto-generated by scripts/build_reference_data.py\n")
        handle.write("PRAGMA foreign_keys = ON;\n\n")
        handle.write("BEGIN TRANSACTION;\n\n")
        handle.write(
            "INSERT OR IGNORE INTO Countries (CountryId, Code, Name) VALUES\n"
        )
        entries = [
            f"('{DEFAULT_COUNTRY_ID}', '{DEFAULT_COUNTRY_CODE}', '{DEFAULT_COUNTRY_NAME}')"
        ]
        for data in countries.values():
            entries.append(
                f"('{data['id']}', '{data['code']}', '{sql_escape(data['name'])}')"
            )
        handle.write(",\n".join(entries))
        handle.write(";\n\nCOMMIT;\n")

    with regions_path.open("w", encoding="utf-8") as handle:
        handle.write("-- Auto-generated by scripts/build_reference_data.py\n")
        handle.write("PRAGMA foreign_keys = ON;\n\n")
        handle.write("BEGIN TRANSACTION;\n\n")
        handle.write(
            "INSERT OR IGNORE INTO Regions (RegionId, CountryId, Name) VALUES\n"
        )
        entries = [
            f"('{DEFAULT_REGION_ID}', '{DEFAULT_COUNTRY_ID}', '{DEFAULT_REGION_NAME}')"
        ]
        for region in regions:
            entries.append(
                "('{id}', '{country_id}', '{name}')".format(
                    id=region["id"],
                    country_id=region["country_id"],
                    name=sql_escape(region["name"]),
                )
            )
        handle.write(",\n".join(entries))
        handle.write(";\n\nCOMMIT;\n")

    alias_entries = set()
    for normalized, data in countries.items():
        alias_entries.add((normalized, data["id"]))

    for alias_norm, canonical_name in alias_map.items():
        canonical_norm = normalize_country(canonical_name)
        canonical = countries.get(canonical_norm)
        if canonical:
            alias_entries.add((alias_norm, canonical["id"]))

    with aliases_path.open("w", encoding="utf-8") as handle:
        handle.write("-- Auto-generated by scripts/build_reference_data.py\n")
        handle.write("PRAGMA foreign_keys = ON;\n\n")
        handle.write("BEGIN TRANSACTION;\n\n")
        handle.write(
            "CREATE TABLE IF NOT EXISTS CountryAliases (\n"
            "    AliasNorm TEXT PRIMARY KEY,\n"
            "    CountryId TEXT NOT NULL,\n"
            "    FOREIGN KEY (CountryId) REFERENCES Countries(CountryId)\n"
            ");\n\n"
        )
        handle.write(
            "INSERT OR IGNORE INTO CountryAliases (AliasNorm, CountryId) VALUES\n"
        )
        entries = [
            f"('{sql_escape(alias_norm)}', '{country_id}')"
            for alias_norm, country_id in sorted(alias_entries)
        ]
        handle.write(",\n".join(entries))
        handle.write(";\n\nCOMMIT;\n")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--legacy-db", required=True, help="Path to BAKI1.1.db")
    parser.add_argument("--output-dir", required=True, help="Output directory for SQL seeds")
    args = parser.parse_args()

    output_dir = pathlib.Path(args.output_dir)
    output_dir.mkdir(parents=True, exist_ok=True)

    conn = sqlite3.connect(":memory:")
    try:
        conn.execute("ATTACH DATABASE ? AS legacy", (args.legacy_db,))

        alias_map = build_alias_map()
        worker_column, worker_type = find_country_column(
            conn, "workers", ["basedInCountry", "basedIn", "birthPlaceCountry", "birthPlace"]
        )
        promo_column, promo_type = find_country_column(
            conn, "promotions", ["basedInCountry", "basedIn", "country"]
        )

        worker_values = load_distinct_values(conn, "workers", worker_column, worker_type)
        promo_values = load_distinct_values(conn, "promotions", promo_column, promo_type)
        raw_values = worker_values + promo_values

        country_mapping = build_country_mapping(raw_values, alias_map)
        regions = build_region_rows(conn, country_mapping)
        write_seed_files(output_dir, country_mapping, regions, alias_map)
    finally:
        conn.close()

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
