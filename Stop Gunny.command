#!/bin/bash
# Double-click → tắt game server + (tuỳ chọn) Docker SQL
cd "$(dirname "$0")"
echo ""
echo "═══════════════════════════════════════"
echo "   Gunny II — Stop"
echo "═══════════════════════════════════════"
echo ""

pkill -f 'keep-gunny-road.sh' 2>/dev/null || true

for s in gunny-center gunny-fighting gunny-road gunny-request; do
  screen -S "$s" -X quit 2>/dev/null || true
done

pkill -f 'Center.Service.dll' 2>/dev/null || true
pkill -f 'Fighting.Service.dll' 2>/dev/null || true
pkill -f 'Road.Service.dll' 2>/dev/null || true
pkill -f 'Tank.Request.dll' 2>/dev/null || true

sleep 1

# Free ports just in case
for port in 828 9500 9202 9208; do
  for pid in $(lsof -nP -iTCP:"$port" -sTCP:LISTEN -t 2>/dev/null); do
    kill "$pid" 2>/dev/null || true
  done
done

echo "✔ Đã tắt Center / Fighting / Road / Request"

if command -v docker >/dev/null 2>&1 && docker info >/dev/null 2>&1; then
  if docker ps --format '{{.Names}}' | grep -qx gunny-sql; then
    docker stop gunny-sql >/dev/null 2>&1 && echo "✔ Đã stop Docker gunny-sql" || echo "… Không stop được gunny-sql"
  else
    echo "… gunny-sql không chạy"
  fi
else
  echo "… Docker không chạy — bỏ qua SQL"
fi

echo ""
echo "Xong."
read -r -p "Nhấn Enter để đóng…"
exit 0
