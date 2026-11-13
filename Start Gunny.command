#!/bin/bash
# Double-click trong Finder → bật Docker SQL + Center/Fighting/Road/Request + mở play.html
set -u
cd "$(dirname "$0")"
ROOT="$(pwd)"
export PATH="$HOME/.dotnet:/usr/local/bin:/opt/homebrew/bin:$PATH"

DOTNET="${DOTNET:-$HOME/.dotnet/dotnet}"
LOG_DIR="/tmp/gunny-logs"
FIFO_DIR="/tmp/gunny-fifo"
mkdir -p "$LOG_DIR" "$FIFO_DIR"

SA_PASSWORD="Gunny@2026Mac"
SQL_NAME="gunny-sql"
SQL_IMAGE="mcr.microsoft.com/azure-sql-edge:latest"

GREEN='\033[0;32m'; YELLOW='\033[1;33m'; RED='\033[0;31m'; NC='\033[0m'
ok()   { echo -e "${GREEN}[OK]${NC} $*"; }
warn() { echo -e "${YELLOW}...${NC} $*"; }
err()  { echo -e "${RED}[X]${NC} $*"; }

echo ""
echo "═══════════════════════════════════════"
echo "   Gunny II — Start (Docker + Server)"
echo "═══════════════════════════════════════"
echo ""

# --- Docker Desktop ---
if ! docker info >/dev/null 2>&1; then
  warn "Đang mở Docker Desktop…"
  open -a Docker 2>/dev/null || true
  for i in $(seq 1 60); do
    if docker info >/dev/null 2>&1; then
      ok "Docker sẵn sàng"
      break
    fi
    sleep 2
    printf "."
  done
  echo ""
  if ! docker info >/dev/null 2>&1; then
    err "Docker chưa chạy. Mở Docker Desktop rồi double-click lại."
    read -r -p "Nhấn Enter để đóng…"
    exit 1
  fi
else
  ok "Docker đang chạy"
fi

# --- SQL container ---
# NOTE: bash 3.2 (macOS) treats $VAR + Unicode ellipsis as one name → always use ${VAR}
if docker ps -a --format '{{.Names}}' | grep -qx "${SQL_NAME}"; then
  if docker ps --format '{{.Names}}' | grep -qx "${SQL_NAME}"; then
    ok "SQL (${SQL_NAME}) dang chay"
  else
    warn "Start container ${SQL_NAME}..."
    docker start "${SQL_NAME}" >/dev/null
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

warn "Cho SQL lang nghe port 1433..."
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
  err "SQL chua san sang. Xem: docker logs ${SQL_NAME}"
  read -r -p "Nhan Enter de dong..."
  exit 1
fi
# Cho SQL engine khởi động xong (Edge cần thêm vài giây sau khi port mở)
sleep 5

# --- Deploy latest DLLs if present ---
if [ -f "$ROOT/GameServerScripts/bin/out/GameServerScripts.dll" ]; then
  cp -f "$ROOT/GameServerScripts/bin/out/GameServerScripts.dll" \
    "$ROOT/Road.Service/bin/Debug/net8.0/GameServerScripts.dll" 2>/dev/null || true
  cp -f "$ROOT/GameServerScripts/bin/out/GameServerScripts.dll" \
    "$ROOT/Fighting.Service/bin/Debug/net8.0/GameServerScripts.dll" 2>/dev/null || true
fi
if [ -f "$ROOT/Game.Logic/bin/Debug/net8.0/Game.Logic.dll" ]; then
  cp -f "$ROOT/Game.Logic/bin/Debug/net8.0/Game.Logic.dll" \
    "$ROOT/Road.Service/bin/Debug/net8.0/Game.Logic.dll" 2>/dev/null || true
  cp -f "$ROOT/Game.Logic/bin/Debug/net8.0/Game.Logic.dll" \
    "$ROOT/Fighting.Service/bin/Debug/net8.0/Game.Logic.dll" 2>/dev/null || true
fi

# --- Helpers ---
need_dll() {
  local path="$1"
  if [ ! -f "$path" ]; then
    err "Thiếu $path — cần build project trước."
    return 1
  fi
  return 0
}

start_fifo_service() {
  # name dir dll port logfile
  local name="$1" dir="$2" dll="$3" port="$4" log="$5"
  local screen_name="gunny-$name"
  local fifo="$FIFO_DIR/${name}.in"

  screen -S "$screen_name" -X quit >/dev/null 2>&1 || true
  for pid in $(lsof -nP -iTCP:"$port" -sTCP:LISTEN -t 2>/dev/null); do
    kill "$pid" 2>/dev/null || true
  done
  sleep 0.5

  # Road: dùng keeper loop (hay crash/EOF)
  if [ "$name" = "road" ]; then
    cat > /tmp/keep-gunny-road.sh <<KEEPEOF
#!/bin/bash
set -u
export PATH="\$HOME/.dotnet:/usr/local/bin:/opt/homebrew/bin:\$PATH"
ROOT="$ROOT"
DIR="\$ROOT/Road.Service"
DLL="\$DIR/bin/Debug/net8.0/Road.Service.dll"
LOG="/tmp/gunny-logs/Road.log"
FIFO="/tmp/gunny-fifo/Road.in"
mkdir -p /tmp/gunny-logs /tmp/gunny-fifo
if [ -f "\$ROOT/GameServerScripts/bin/out/GameServerScripts.dll" ]; then
  cp -f "\$ROOT/GameServerScripts/bin/out/GameServerScripts.dll" "\$DIR/bin/Debug/net8.0/GameServerScripts.dll" 2>/dev/null || true
fi
while true; do
  for pid in \$(lsof -nP -iTCP:9500 -sTCP:LISTEN -t 2>/dev/null); do kill "\$pid" 2>/dev/null || true; done
  sleep 1
  rm -f "\$FIFO"; mkfifo "\$FIFO"
  (exec 3>"\$FIFO"; while true; do sleep 3600; done) &
  KEEPER=\$!
  echo "[\$(date '+%H:%M:%S')] starting Road (keeper=\$KEEPER)" >> "\$LOG"
  cd "\$DIR"
  "\$HOME/.dotnet/dotnet" "\$DLL" < "\$FIFO" >> "\$LOG" 2>&1
  code=\$?
  echo "[\$(date '+%H:%M:%S')] Road exited code=\$code — restart in 2s" >> "\$LOG"
  kill "\$KEEPER" 2>/dev/null || true
  wait "\$KEEPER" 2>/dev/null || true
  sleep 2
done
KEEPEOF
    chmod +x /tmp/keep-gunny-road.sh
    screen -dmS gunny-road /tmp/keep-gunny-road.sh
    ok "Road (screen gunny-road, port $port)"
    return
  fi

  rm -f "$fifo"
  mkfifo "$fifo"
  # Giữ fifo mở để Console.ReadLine không EOF
  (exec 3>"$fifo"; while true; do sleep 3600; done) &

  screen -dmS "$screen_name" bash -lc "
    export PATH=\"\$HOME/.dotnet:/usr/local/bin:/opt/homebrew/bin:\$PATH\"
    cd \"$dir\"
    exec \"$DOTNET\" \"$dll\" < \"$fifo\" >> \"$log\" 2>&1
  "
  ok "$name (screen $screen_name, port $port)"
}

need_dll "$ROOT/Center.Service/bin/Debug/net8.0/Center.Service.dll" || { read -r -p "Nhấn Enter…"; exit 1; }
need_dll "$ROOT/Fighting.Service/bin/Debug/net8.0/Fighting.Service.dll" || { read -r -p "Nhấn Enter…"; exit 1; }
need_dll "$ROOT/Road.Service/bin/Debug/net8.0/Road.Service.dll" || { read -r -p "Nhấn Enter…"; exit 1; }
need_dll "$ROOT/Tank.Request/bin/Debug/net8.0/Tank.Request.dll" || { read -r -p "Nhấn Enter…"; exit 1; }

# Kill old screens
for s in gunny-center gunny-fighting gunny-road gunny-request; do
  screen -S "$s" -X quit >/dev/null 2>&1 || true
done
pkill -f 'keep-gunny-road.sh' >/dev/null 2>&1 || true
sleep 1

echo ""
warn "Khởi động services…"
: > "$LOG_DIR/Center.log"
: > "$LOG_DIR/Fighting.log"
: > "$LOG_DIR/Road.log"
: > "$LOG_DIR/Request.log"

start_fifo_service center   "$ROOT/Center.Service"   "$ROOT/Center.Service/bin/Debug/net8.0/Center.Service.dll"     9202 "$LOG_DIR/Center.log"
sleep 2
start_fifo_service fighting "$ROOT/Fighting.Service" "$ROOT/Fighting.Service/bin/Debug/net8.0/Fighting.Service.dll" 9208 "$LOG_DIR/Fighting.log"
sleep 2
start_fifo_service road     "$ROOT/Road.Service"     "$ROOT/Road.Service/bin/Debug/net8.0/Road.Service.dll"         9500 "$LOG_DIR/Road.log"
sleep 2

# Request: không cần fifo (Kestrel)
screen -S gunny-request -X quit >/dev/null 2>&1 || true
for pid in $(lsof -nP -iTCP:828 -sTCP:LISTEN -t 2>/dev/null); do kill "$pid" 2>/dev/null || true; done
sleep 0.5
screen -dmS gunny-request bash -lc "
  export PATH=\"\$HOME/.dotnet:/usr/local/bin:/opt/homebrew/bin:\$PATH\"
  cd \"$ROOT/Tank.Request\"
  exec \"$DOTNET\" bin/Debug/net8.0/Tank.Request.dll >> \"$LOG_DIR/Request.log\" 2>&1
"
ok "Request (screen gunny-request, port 828)"

warn "Chờ web :828…"
for i in $(seq 1 40); do
  if curl -sf -o /dev/null --max-time 1 "http://127.0.0.1:828/web/play.html"; then
    ok "Web OK"
    break
  fi
  sleep 1
  printf "."
done
echo ""

LAN_IP="$(ipconfig getifaddr en0 2>/dev/null || ipconfig getifaddr en1 2>/dev/null || echo 127.0.0.1)"
PLAY_LOCAL="http://127.0.0.1:828/web/play.html"
PLAY_LAN="http://${LAN_IP}:828/web/play.html"

echo ""
echo "═══════════════════════════════════════"
ok "Gunny đã sẵn sàng chơi!"
echo "  Máy này:  $PLAY_LOCAL"
echo "  LAN/ĐT:   $PLAY_LAN"
echo "  Log:      $LOG_DIR/"
echo "  Stop:     double-click \"Stop Gunny.command\""
echo "═══════════════════════════════════════"
echo ""

open "$PLAY_LOCAL" 2>/dev/null || true

read -r -p "Nhấn Enter để đóng cửa sổ này (game vẫn chạy ngầm)…"
exit 0
