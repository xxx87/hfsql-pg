# HFSQL to SQL
Extracting data from the HF database for subsequent import into the Postgres database (or another SQL database).

### For start:
```
dotnet run <db_name> <table_name>
```
As a result, the folder "bin/Debug/net5.0-windows/files" will contain all the files that were written directly to the table. The "bin/Debug/net5.0-windows/scripts" folder will contain sql scripts that can be applied to Postgres.