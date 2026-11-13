#!/bin/bash
# Sync GameAssets/map + bomb into Road.Service & Fighting.Service (real folders).
# Use after git pull — especially on Windows where Mac absolute symlinks break.
set -euo pipefail
ROOT="$(cd "$(dirname "$0")" && pwd)"
cd "$ROOT"

if [ ! -f "GameAssets/map/1001/fore.map" ]; then
  echo "[X] Missing GameAssets/map — git pull latest first."
  exit 1
fi

sync_one() {
  local dest="$1"
  rm -rf "$dest"
  mkdir -p "$(dirname "$dest")"
  cp -R "GameAssets/$(basename "$dest")" "$dest"
}

echo "Syncing map/bomb → Road.Service + Fighting.Service ..."
sync_one "Road.Service/map"
sync_one "Road.Service/bomb"
sync_one "Fighting.Service/map"
sync_one "Fighting.Service/bomb"

test -f "Road.Service/map/1001/fore.map"
echo "[OK] Maps ready. Restart Road + Fighting."
