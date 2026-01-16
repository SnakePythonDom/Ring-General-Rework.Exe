import sqlite3
import os
import re

def run_migrations():
    appdata = os.getenv('APPDATA')
    db_path = os.path.join(appdata, "RingGeneral", "ring_general.db")
    migrations_dir = r"C:\Users\popo2\.gemini\Ring-General-Rework.Exe\data\migrations"
    
    if not os.path.exists(db_path):
        print(f"Database not found at {db_path}")
        return

    print(f"Connecting to database: {db_path}")
    conn = sqlite3.connect(db_path)
    cursor = conn.cursor()
    
    # Disable foreign keys for population
    cursor.execute("PRAGMA foreign_keys = OFF;")
    
    # Define explicit order for reliable population
    fix_migrations = ['035_Fix_GimmickHistory_FK.sql', '036_Fix_Shows_Schema.sql']
    pop_migrations = [f for f in os.listdir(migrations_dir) if f.endswith('.sql') and re.match(r'0(19|2[0-9]|3[0-4])', f)]
    
    # Run fixes first, then population
    migration_files = fix_migrations + sorted(pop_migrations)
    
    for filename in migration_files:
        filepath = os.path.join(migrations_dir, filename)
        if not os.path.exists(filepath): continue
        print(f"Applying {filename}...")
        
        with open(filepath, 'r', encoding='utf-8') as f:
            sql = f.read()
            
            # Special handling for 020 (duplicate columns)
            if "020" in filename:
                for stmt in sql.split(';'):
                    if stmt.strip():
                        try:
                            cursor.execute(stmt)
                        except sqlite3.OperationalError as e:
                            if "duplicate column" in str(e).lower():
                                pass # Ignore
                            else:
                                print(f"Error in {filename}: {e}")
            else:
                try:
                    # executescript is faster but might need clean SQL
                    cursor.executescript(sql)
                except Exception as e:
                    print(f"Error in {filename}: {e}")
                    # Continue anyway as some might be already partially applied
        
        conn.commit()

    # Re-enable foreign keys
    cursor.execute("PRAGMA foreign_keys = ON;")
    conn.commit()
    conn.close()
    print("Database population complete.")

if __name__ == "__main__":
    run_migrations()
