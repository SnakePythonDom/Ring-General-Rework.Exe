## 2026-01-11 - Zip Bomb Prevention in Save Import
**Vulnerability:** Uncontrolled Resource Consumption (Zip Bomb) in `SaveStorageService.ImporterPack`. The application accepted zip files without verifying the uncompressed size of entries.
**Learning:** Even in desktop applications processing local user files, input validation for file archives is crucial to prevent resource exhaustion (DoS) or disk filling.
**Prevention:** Implemented a strict check on `entry.Length` before `ExtractToFile`, enforcing a 500 MB limit for imported database files.
