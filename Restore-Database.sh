#!/bin/bash
# Restore Database/*.bak into local SQL (Docker gunny-sql or localhost:1433)
set -euo pipefail
ROOT="$(cd "$(dirname "$0")" && pwd)"
SA_PASSWORD="${SA_PASSWORD:-Gunny@2026Mac}"
SQL="${SQL_HOST:-127.0.0.1,1433}"

need() { [ -f "$1" ] || { echo "[X] Missing $1"; exit 1; }; }
need "$ROOT/Database/Game34.bak"
need "$ROOT/Database/Player34.bak"
need "$ROOT/Database/Db_Membership.bak"

echo "Copy bak into SQL container..."
docker exec -u root gunny-sql mkdir -p /var/opt/mssql/data/backup
docker exec -u root gunny-sql chown mssql:root /var/opt/mssql/data/backup
docker cp "$ROOT/Database/Game34.bak" gunny-sql:/var/opt/mssql/data/backup/Game34.bak
docker cp "$ROOT/Database/Player34.bak" gunny-sql:/var/opt/mssql/data/backup/Player34.bak
docker cp "$ROOT/Database/Db_Membership.bak" gunny-sql:/var/opt/mssql/data/backup/Db_Membership.bak

run_sql() {
  docker run --rm --network host mcr.microsoft.com/mssql-tools \
    /opt/mssql-tools/bin/sqlcmd -S "$SQL" -U sa -P "$SA_PASSWORD" -C -Q "$1"
}

echo "Restoring databases (drop if exist)..."
run_sql "
IF DB_ID('Project_Game34') IS NOT NULL BEGIN ALTER DATABASE [Project_Game34] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [Project_Game34]; END
IF DB_ID('Project_Player34') IS NOT NULL BEGIN ALTER DATABASE [Project_Player34] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [Project_Player34]; END
IF DB_ID('Db_Membership') IS NOT NULL BEGIN ALTER DATABASE [Db_Membership] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [Db_Membership]; END
RESTORE DATABASE [Project_Game34] FROM DISK='/var/opt/mssql/data/backup/Game34.bak' WITH REPLACE, MOVE 'Db_Tank' TO '/var/opt/mssql/data/Project_Game34.mdf', MOVE 'Db_Tank_log' TO '/var/opt/mssql/data/Project_Game34_log.ldf';
RESTORE DATABASE [Project_Player34] FROM DISK='/var/opt/mssql/data/backup/Player34.bak' WITH REPLACE, MOVE 'DDTMixTankAc' TO '/var/opt/mssql/data/Project_Player34.mdf', MOVE 'DDTMixTankAc_log' TO '/var/opt/mssql/data/Project_Player34_log.ldf';
RESTORE DATABASE [Db_Membership] FROM DISK='/var/opt/mssql/data/backup/Db_Membership.bak' WITH REPLACE, MOVE 'Db_Membership_Data' TO '/var/opt/mssql/data/Db_Membership.mdf', MOVE 'Db_Membership_Log' TO '/var/opt/mssql/data/Db_Membership_log.ldf';
"
echo "[OK] Restore done."
