#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
OUT_DIR="${SCRIPT_DIR}/../../Plugins/macOS"
OUT_FILE="${OUT_DIR}/libpty_bridge.dylib"
mkdir -p "${OUT_DIR}"

clang -dynamiclib \
  -O2 \
  -Wall \
  -Wextra \
  -arch arm64 \
  -arch x86_64 \
  -mmacosx-version-min=11.0 \
  -o "${OUT_FILE}" \
  "${SCRIPT_DIR}/pty_bridge.c"

echo "Built universal macOS binary: ${OUT_FILE}"
file "${OUT_FILE}"
