#!/usr/bin/env bash
set -euo pipefail

UNITY_BIN="/Applications/Unity/Hub/Editor/2022.3.62f3/Unity.app/Contents/MacOS/Unity"
PROJECT_PATH="/Users/romin.zhang/IdeaProjects/vocab-card-game"
PORT="${PORT:-8000}"
SERVE="${SERVE:-1}"
WAIT_FOR_SERVER="${WAIT_FOR_SERVER:-1}"

TIMESTAMP="$(date +"%Y%m%d_%H%M%S")"
BUILD_DIR="$PROJECT_PATH/Builds/WebGL_${TIMESTAMP}"
LATEST_DIR="$PROJECT_PATH/Builds/WebGL_latest"
UNITY_LOG="$BUILD_DIR/unity_build.log"

mkdir -p "$BUILD_DIR"

BUILD_OUTPUT="$BUILD_DIR" "$UNITY_BIN" -batchmode -quit \
  -projectPath "$PROJECT_PATH" \
  -executeMethod VocabCardGame.Editor.BuildScript.BuildWebGL \
  -logFile "$UNITY_LOG"

echo "Build complete: $BUILD_DIR"
echo "Unity log: $UNITY_LOG"

mkdir -p "$LATEST_DIR"
rsync -a --delete "$BUILD_DIR/" "$LATEST_DIR/"

if [ "$SERVE" = "1" ]; then
  cd "$LATEST_DIR"
  /opt/homebrew/bin/python3 -m http.server "$PORT" --bind 127.0.0.1 &
  SERVER_PID=$!

  URL="http://localhost:${PORT}"
  echo "Serving at $URL"
  open "$URL" || true

  if [ "$WAIT_FOR_SERVER" = "1" ]; then
    wait "$SERVER_PID"
  fi
fi
