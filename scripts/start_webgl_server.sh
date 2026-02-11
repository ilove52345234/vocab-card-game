#!/usr/bin/env bash
set -euo pipefail

PROJECT_PATH="/Users/romin.zhang/IdeaProjects/vocab-card-game"
BUILD_DIR="${BUILD_DIR:-$PROJECT_PATH/Builds/WebGL_latest}"
PORT="${PORT:-8000}"
PYTHON="/opt/homebrew/bin/python3"

cd "$BUILD_DIR"
"$PYTHON" -m http.server "$PORT" --bind 127.0.0.1 > /tmp/vocab_webgl_server.log 2>&1 &
SERVER_PID=$!

sleep 0.5
if curl -I --max-time 3 "http://127.0.0.1:$PORT/index.html" >/dev/null 2>&1; then
  open "http://127.0.0.1:$PORT/index.html" || true
  wait "$SERVER_PID"
else
  kill "$SERVER_PID" 2>/dev/null || true
  echo "Server failed to start; stopped."
  exit 1
fi
