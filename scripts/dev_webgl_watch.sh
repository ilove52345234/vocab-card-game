#!/usr/bin/env bash
set -euo pipefail

UNITY_BIN="/Applications/Unity/Hub/Editor/2022.3.62f3/Unity.app/Contents/MacOS/Unity"
PROJECT_PATH="/Users/romin.zhang/IdeaProjects/vocab-card-game"
BUILD_DIR="$PROJECT_PATH/Builds/WebGL_dev"
PORT="${PORT:-8000}"

mkdir -p "$BUILD_DIR"

# Initial build
BUILD_OUTPUT="$BUILD_DIR" "$UNITY_BIN" -batchmode -quit \
  -projectPath "$PROJECT_PATH" \
  -executeMethod VocabCardGame.Editor.BuildScript.BuildWebGL \
  -logFile "$BUILD_DIR/unity_build.log"

# Start server
cd "$BUILD_DIR"
/opt/homebrew/bin/python3 -m http.server "$PORT" > /tmp/vocab_webgl_server.log 2>&1 &
SERVER_PID=$!

echo "Serving at http://127.0.0.1:$PORT/index.html"

# Watch & rebuild (foreground)
/opt/homebrew/bin/python3 "$PROJECT_PATH/scripts/webgl_watch.py"

echo "Stopping server..."
kill "$SERVER_PID" 2>/dev/null || true
