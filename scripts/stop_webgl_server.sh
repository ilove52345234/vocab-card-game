#!/usr/bin/env bash
set -euo pipefail

PORT="${PORT:-8000}"

# Stop any python http.server on the port
pkill -f "http.server $PORT" >/dev/null 2>&1 || true

# Verify
if lsof -n -i :$PORT >/dev/null 2>&1; then
  echo "Port $PORT is still in use."
  exit 1
fi

echo "Stopped WebGL server on port $PORT."
