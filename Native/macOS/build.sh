#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
OUT_DIR="${SCRIPT_DIR}/../../Plugins/macOS"
mkdir -p "${OUT_DIR}"

clang -dynamiclib \
  -O2 \
  -Wall \
  -Wextra \
  -o "${OUT_DIR}/libpty_bridge.dylib" \
  "${SCRIPT_DIR}/pty_bridge.c"

echo "Built: ${OUT_DIR}/libpty_bridge.dylib"
