
import re

input_file = r"C:\Users\popo2\.gemini\Ring-General-Rework.Exe\data\migrations\019_Populate_World_Data.sql"
output_file = r"C:\Users\popo2\.gemini\Ring-General-Rework.Exe\data\migrations\019_Populate_World_Data.sql"

with open(input_file, 'r', encoding='utf-8') as f:
    content = f.read()

# Fix Countries INSERT header (already done mostly, but ensure)
# It was: INSERT INTO Countries (CountryId, Code, Name, Continent, WrestlingImportance)
# But values are: ('USA', 'États-Unis', 'North America', 100)
# Need: ('USA', 'USA', 'États-Unis', 'North America', 100)

# Regex to find Country values lines.
# Pattern: ('XXX', 'Name', 'Continent', Num)
# We accept 3 letter code.
# Be careful not to match Regions which also look similar? 
# Regions: ('USA_NY', 'New York', 'USA', 100) -> 4 fields.
# Countries: ('USA', 'États-Unis', 'North America', 100) -> 4 fields.
# BUT Regions ID is > 3 chars (usually with underscore). Countries ID is 3 chars.

def fix_country_line(match):
    # match.group(0) is the whole line segment e.g. ('USA', 'États-Unis', 'North America', 100)
    # We want to insert the code again.
    # Group 1: 'USA'
    # Group 2: , 'États-Unis', 'North America', 100)
    code = match.group(1)
    rest = match.group(2)
    return f"({code}, {code}{rest}"

# Iterate line by line to be safer or use specific blocks?
# The file has "INSERT INTO Countries" and "INSERT INTO Regions".

lines = content.splitlines()
new_lines = []
in_countries = False
in_regions = False

for line in lines:
    if "INSERT INTO Countries" in line:
        in_countries = True
        in_regions = False
        # Ensure header is correct (it might be partially fixed by previous tool)
        # We want: (CountryId, Code, Name, Continent, WrestlingImportance)
        if "(Id," in line:
             line = line.replace("(Id, Name, Continent, WrestlingImportance)", "(CountryId, Code, Name, Continent, WrestlingImportance)")
    elif "INSERT INTO Regions" in line:
        in_regions = True
        in_countries = False
        # Fix Regions Header: Id -> RegionId
        line = line.replace("(Id, Name", "(RegionId, Name")

    if in_countries and line.strip().startswith("('"):
        # Check if it looks like a country line (3 char code)
        # Pattern: ('ABC', 
        m = re.match(r"\s*\('([A-Z]{3})'(.+)", line)
        if m:
            # Check if it DOESN'T already have the double code (in case we run twice)
            # If rest starts with ", 'ABC'", it's already fixed?
            code = m.group(1)
            rest = m.group(2)
            if not rest.startswith(f", '{code}'"):
                line = re.sub(r"\('([A-Z]{3})'", r"('\1', '\1'", line, 1)
    
    new_lines.append(line)

with open(output_file, 'w', encoding='utf-8') as f:
    f.write("\n".join(new_lines))
