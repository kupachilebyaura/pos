#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
SRC_APP="$ROOT/src/app"
DEST="$ROOT/frontend-ui/src/app"

if [ ! -d "$SRC_APP" ]; then
  echo "Source app not found at $SRC_APP"
  exit 1
fi

rm -rf "$DEST"
mkdir -p "$DEST"
cp -R "$SRC_APP/"* "$DEST/"
echo "Copied src/app to frontend-ui/src/app"
