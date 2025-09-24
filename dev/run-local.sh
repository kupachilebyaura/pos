#!/usr/bin/env bash
set -euo pipefail

# Small helper to start POS API and frontend locally.
# Usage:
#   bash dev/run-local.sh        # start API if ports free, then frontend
#   bash dev/run-local.sh --force  # kill processes listening on ports then start

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
API_PROJECT="$ROOT/POS.API"
FRONTEND_DIR="$ROOT/frontend"
PORT_HTTP=5002
PORT_HTTPS=5003
LOG=/tmp/pos-api.log
PIDFILE=/tmp/pos-api.pid
FRONT_PIDFILE=/tmp/pos-frontend.pid

echo "== POS dev runner =="
echo "Root: $ROOT"

if ! command -v dotnet >/dev/null 2>&1; then
  echo "dotnet not found in PATH. Install .NET SDK and retry."
  exit 1
fi

echo "Checking ports $PORT_HTTP and $PORT_HTTPS..."
lsof -nP -iTCP:$PORT_HTTP -sTCP:LISTEN || true
lsof -nP -iTCP:$PORT_HTTPS -sTCP:LISTEN || true

if [[ "${1:-}" == "--force" ]]; then
  echo "--force provided: killing processes listening on $PORT_HTTP and $PORT_HTTPS"
  for p in $(lsof -t -iTCP:$PORT_HTTP -sTCP:LISTEN || true); do
    echo "killing $p"
    kill -9 "$p" || true
  done
  for p in $(lsof -t -iTCP:$PORT_HTTPS -sTCP:LISTEN || true); do
    echo "killing $p"
    kill -9 "$p" || true
  done
fi

echo "Starting API (HTTP:$PORT_HTTP HTTPS:$PORT_HTTPS) in background..."
cd "$API_PROJECT"
ASPNETCORE_URLS="http://localhost:$PORT_HTTP;https://localhost:$PORT_HTTPS" \
  ASPNETCORE_ENVIRONMENT=Development \
  nohup dotnet run --project . > "$LOG" 2>&1 &
API_PID=$!
echo $API_PID > "$PIDFILE"
echo "API started (pid=$API_PID). Logs: $LOG"

echo "Waiting for /api/cash/ping to return 2xx (up to 20s)..."
for i in {1..20}; do
  sleep 1
  if curl -s -o /dev/null -w "%{http_code}" http://localhost:$PORT_HTTP/api/cash/ping 2>/dev/null | grep -q '^2'; then
    echo "API responds on http://localhost:$PORT_HTTP"
    break
  fi
  if curl -k -s -o /dev/null -w "%{http_code}" https://localhost:$PORT_HTTPS/api/cash/ping 2>/dev/null | grep -q '^2'; then
    echo "API responds on https://localhost:$PORT_HTTPS"
    break
  fi
  echo -n "."
done

echo
echo "Tailing last 20 lines of log ($LOG):"
tail -n 20 "$LOG" || true

echo "Starting static frontend on http://localhost:8000"
cd "$FRONTEND_DIR"
python3 -m http.server 8000 >/dev/null 2>&1 &
FRONT_PID=$!
echo $FRONT_PID > "$FRONT_PIDFILE"
echo "Frontend started (pid=$FRONT_PID). Open http://localhost:8000"

echo "Done. Use 'tail -f $LOG' to follow API logs. To stop, 'kill \\$(cat $PIDFILE) && kill \\$(cat $FRONT_PIDFILE)'."
