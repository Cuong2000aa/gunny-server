#!/bin/bash
# Double-click: mo Docker Desktop + container gunny-sql (port 1433)
set -u
export HOME="${HOME:-/Users/it}"
export PATH="/usr/local/bin:/opt/homebrew/bin:$PATH"
cd "$(dirname "$0")"

ok() { echo "[OK] $*"; }
warn() { echo "[...] $*"; }
err() { echo "[X] $*"; }

echo ""
echo "=== Gunny — Start Docker SQL ==="
echo ""

# Mo Docker Desktop (nested app tren ban macOS moi)
if ! docker info >/dev/null 2>&1; then
  warn "Dang mo Docker Desktop..."
  open "/Applications/Docker.app/Contents/MacOS/Docker Desktop.app" 2>/dev/null \
    || open /Applications/Docker.app 2>/dev/null \
    || true
  for i in $(seq 1 90); do
    if docker info >/dev/null 2>&1; then
      ok "Docker san sang"
      break
    fi
    sleep 2
    printf "."
  done
  echo ""
fi

if ! docker info >/dev/null 2>&1; then
  err "Docker chua chay. Mo Docker Desktop bang tay roi double-click lai."
  read -r -p "Nhan Enter de dong..."
  exit 1
fi
ok "Docker dang chay"

SQL_NAME="gunny-sql"
SA_PASSWORD="Gunny@2026Mac"
SQL_IMAGE="mcr.microsoft.com/azure-sql-edge:latest"

if docker ps -a --format '{{.Names}}' | grep -qx "${SQL_NAME}"; then
  if docker ps --format '{{.Names}}' | grep -qx "${SQL_NAME}"; then
    ok "SQL (${SQL_NAME}) dang chay"
  else
    warn "Start ${SQL_NAME}..."
    docker start "${SQL_NAME}"
    ok "Da start ${SQL_NAME}"
  fi
else
  warn "Tao container ${SQL_NAME} lan dau..."
  docker run -d --name "${SQL_NAME}" \
    -e "ACCEPT_EULA=Y" \
    -e "MSSQL_SA_PASSWORD=${SA_PASSWORD}" \
    -p 1433:1433 \
    "${SQL_IMAGE}"
  ok "Da tao ${SQL_NAME}"
fi

warn "Cho port 1433..."
for i in $(seq 1 60); do
  if nc -z 127.0.0.1 1433 2>/dev/null; then
    ok "SQL port 1433 OK"
    break
  fi
  sleep 2
  printf "."
done
echo ""

if ! nc -z 127.0.0.1 1433 2>/dev/null; then
  err "SQL chua lang nghe 1433. Xem: docker logs ${SQL_NAME}"
  read -r -p "Nhan Enter de dong..."
  exit 1
fi

sleep 3
docker ps --format 'table {{.Names}}\t{{.Status}}\t{{.Ports}}'
echo ""
ok "Docker SQL san sang. Co the choi: http://127.0.0.1:828/web/play.html"
read -r -p "Nhan Enter de dong..."
exit 0
