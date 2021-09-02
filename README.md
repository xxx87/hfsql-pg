# HFSQL to SQL
Extracting data from the HF database for subsequent import into the Postgres database (or another SQL database).

OLEDB DRIVER: https://drive.google.com/drive/folders/13R8Ilcz131RRnkMPM1J8AbaBExxh03RT?usp=sharing

### For start:
```
bin\Debug\net5.0-windows\HFSQL_1.exe <db_name> <table_name>
```
As a result, the folder "bin/Debug/net5.0-windows/files" will contain all the files that were written directly to the table. The "bin/Debug/net5.0-windows/scripts" folder will contain sql scripts that can be applied to Postgres.