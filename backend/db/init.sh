#!/bin/bash
# Start SQL Server in the background, then run the init script once it's ready.
set -e

echo "[db-init] Starting SQL Server..."
/opt/mssql/bin/sqlservr &
SQL_PID=$!

echo "[db-init] Waiting for SQL Server to accept connections..."
for i in $(seq 1 30); do
    /opt/mssql-tools18/bin/sqlcmd \
        -S localhost -U sa -P "${SA_PASSWORD}" \
        -Q "SELECT 1" -C -l 1 &>/dev/null && break
    echo "[db-init] Not ready yet (attempt $i/30), retrying in 2s..."
    sleep 2
done

echo "[db-init] Creating database WeatherAuthDb if it does not exist..."
/opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "${SA_PASSWORD}" -C \
    -Q "IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'WeatherAuthDb') CREATE DATABASE WeatherAuthDb;"

echo "[db-init] Applying schema & stored procedures..."
/opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "${SA_PASSWORD}" -C \
    -d WeatherAuthDb \
    -i /init/StoredProcedures.sql

echo "[db-init] Initialisation complete."

# Hand control back to SQL Server process
wait $SQL_PID
