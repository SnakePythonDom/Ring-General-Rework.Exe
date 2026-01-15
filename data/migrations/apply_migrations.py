
import sqlite3
import os

appdata = os.getenv('APPDATA')
db_path = os.path.join(appdata, "RingGeneral", "ring_general.db")
migration_20 = r"C:\Users\popo2\.gemini\Ring-General-Rework.Exe\data\migrations\020_Update_World_Schema.sql"
migration_19 = r"C:\Users\popo2\.gemini\Ring-General-Rework.Exe\data\migrations\019_Populate_World_Data.sql"

if not os.path.exists(db_path):
    print(f"Database not found at {db_path}")
    # Try the one in src just in case?
    # db_path = r"C:\Users\popo2\.gemini\Ring-General-Rework.Exe\src\RingGeneral.UI\ring_general.db"
    exit(1)

print(f"Updating database: {db_path}")

conn = sqlite3.connect(db_path)
cursor = conn.cursor()

# Disable FKs
cursor.execute("PRAGMA foreign_keys = OFF;")

# Apply 020
print("Applying 020_Update_World_Schema.sql...")
with open(migration_20, 'r', encoding='utf-8') as f:
    sql_20 = f.read()
    # Execute script (might fail if columns already exist, so wrap in try/catch blocks usually, but execution script is all or nothing)
    # We can try/except per statement?
    # The file has multiple statements separated by ;
    statements = sql_20.split(';')
    for stmt in statements:
        if stmt.strip():
            try:
                cursor.execute(stmt)
            except sqlite3.OperationalError as e:
                if "duplicate column" in str(e).lower():
                    print(f"Column already exists: {e}")
                else:
                    print(f"Error executing 020 statement: {e}")
                    raise

# Apply 019
print("Applying 019_Populate_World_Data.sql...")
with open(migration_19, 'r', encoding='utf-8') as f:
    sql_19 = f.read()
    # Replace INSERT INTO with INSERT OR REPLACE INTO for safety
    sql_19 = sql_19.replace("INSERT INTO Countries", "INSERT OR REPLACE INTO Countries")
    sql_19 = sql_19.replace("INSERT INTO Regions", "INSERT OR REPLACE INTO Regions")
    
    try:
        cursor.executescript(sql_19)
    except Exception as e:
        print(f"Error applying 019: {e}")
        # Proceed or rollback? Auto-commit is on default in python sqlite3 but executescript commits?
        # usually executescript issues a COMMIT.
        raise

# Re-enable FKs
cursor.execute("PRAGMA foreign_keys = ON;")
conn.commit()
conn.close()
print("Migrations applied successfully.")
