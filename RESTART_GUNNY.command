#!/bin/bash
# Double-click or: bash /Users/it/Documents/Gunny/RESTART_GUNNY.command
export HOME=/Users/it
export PATH="$HOME/.dotnet:/usr/local/bin:/opt/homebrew/bin:$PATH"
bash /tmp/restart-gunny-road-only.sh
echo
echo "Hard refresh: http://127.0.0.1:828/web/play.html?v=$(date +%H%M)"
read -r _
