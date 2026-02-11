#!/usr/bin/env python3
import os
import sys
import time
import subprocess
from pathlib import Path

PROJECT_PATH = Path("/Users/romin.zhang/IdeaProjects/vocab-card-game")
UNITY_BIN = "/Applications/Unity/Hub/Editor/2022.3.62f3/Unity.app/Contents/MacOS/Unity"
OUTPUT_DIR = PROJECT_PATH / "Builds" / "WebGL_dev"
LOG_PATH = OUTPUT_DIR / "unity_build.log"

WATCH_DIRS = [
    PROJECT_PATH / "Assets",
    PROJECT_PATH / "Packages",
    PROJECT_PATH / "ProjectSettings",
]

EXCLUDE_DIRS = {"Library", "Builds", "Logs", "Temp", "Obj", ".git"}

POLL_SECONDS = 2
DEBOUNCE_SECONDS = 5


def iter_files():
    for base in WATCH_DIRS:
        if not base.exists():
            continue
        for path in base.rglob("*"):
            if path.is_dir():
                if path.name in EXCLUDE_DIRS:
                    # skip subtree
                    continue
            else:
                # filter out big/irrelevant files if needed
                yield path


def snapshot():
    snap = {}
    for f in iter_files():
        try:
            stat = f.stat()
            snap[str(f)] = (stat.st_mtime, stat.st_size)
        except FileNotFoundError:
            continue
    return snap


def run_build():
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    env = os.environ.copy()
    env["BUILD_OUTPUT"] = str(OUTPUT_DIR)
    cmd = [
        UNITY_BIN,
        "-batchmode",
        "-quit",
        "-projectPath",
        str(PROJECT_PATH),
        "-executeMethod",
        "VocabCardGame.Editor.BuildScript.BuildWebGL",
        "-logFile",
        str(LOG_PATH),
    ]
    print(f"[watch] build start -> {OUTPUT_DIR}")
    result = subprocess.run(cmd, env=env)
    if result.returncode == 0:
        print("[watch] build success")
    else:
        print("[watch] build failed (see unity_build.log)")


def main():
    last = snapshot()
    last_build_time = 0.0
    print("[watch] watching for changes...")

    while True:
        time.sleep(POLL_SECONDS)
        current = snapshot()
        if current != last:
            now = time.time()
            if now - last_build_time >= DEBOUNCE_SECONDS:
                run_build()
                last_build_time = now
            last = current


if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        print("[watch] stopped")
        sys.exit(0)
