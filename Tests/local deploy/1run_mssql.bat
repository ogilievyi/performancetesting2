docker run --name mssql --cpus="2" -e ACCEPT_EULA=Y -e SA_PASSWORD=Password+ -p 1433:1433 -d samuelmarks/mssql-server-fts-sqlpackage-linux