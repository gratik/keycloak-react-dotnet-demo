#!/usr/bin/env bash

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
RUN_DIR="$ROOT_DIR/.run"
FRONTEND_DIR="$ROOT_DIR/frontend"
COMPOSE_FILE="$ROOT_DIR/infra/docker-compose.yml"
FRONTEND_PID_FILE="$RUN_DIR/frontend.pid"
FRONTEND_LOG_FILE="$RUN_DIR/frontend.log"

mkdir -p "$RUN_DIR"

load_env_file() {
  local file_path="$1"

  if [[ -f "$file_path" ]]; then
    set -a
    # shellcheck disable=SC1090
    source "$file_path"
    set +a
  fi
}

require_command() {
  local command_name="$1"

  if ! command -v "$command_name" >/dev/null 2>&1; then
    echo "Missing required command: $command_name" >&2
    exit 1
  fi
}

wait_for_http() {
  local url="$1"
  local expected_status="$2"
  local label="$3"
  local attempts=60
  local status

  while (( attempts > 0 )); do
    status="$(curl -s -o /dev/null -w "%{http_code}" "$url" || true)"
    if [[ "$status" == "$expected_status" ]]; then
      echo "$label is ready at $url"
      return 0
    fi

    sleep 2
    attempts=$((attempts - 1))
  done

  echo "Timed out waiting for $label at $url" >&2
  exit 1
}

start_frontend() {
  local frontend_url="$1"
  local frontend_host="127.0.0.1"

  if curl -s "$frontend_url" >/dev/null 2>&1; then
    echo "Frontend dev server already responds at $frontend_url"
    return 0
  fi

  if [[ -f "$FRONTEND_PID_FILE" ]]; then
    local existing_pid
    existing_pid="$(cat "$FRONTEND_PID_FILE")"
    if kill -0 "$existing_pid" >/dev/null 2>&1; then
      echo "Frontend dev server is already running with PID $existing_pid"
      return 0
    fi
    rm -f "$FRONTEND_PID_FILE"
  fi

  if [[ ! -d "$FRONTEND_DIR/node_modules" && "${SKIP_NPM_INSTALL:-0}" != "1" ]]; then
    echo "Installing frontend dependencies..."
    (cd "$FRONTEND_DIR" && npm install)
  fi

  echo "Starting frontend dev server..."
  (
    cd "$FRONTEND_DIR"
    nohup npm run dev -- --host "$frontend_host" >"$FRONTEND_LOG_FILE" 2>&1 &
    echo $! >"$FRONTEND_PID_FILE"
  )

  wait_for_http "$frontend_url" "200" "Frontend"
}

open_browser() {
  local url="$1"

  if [[ "${NO_OPEN:-0}" == "1" ]]; then
    echo "Skipping browser open because NO_OPEN=1"
    return 0
  fi

  if command -v open >/dev/null 2>&1; then
    open "$url"
  elif command -v xdg-open >/dev/null 2>&1; then
    xdg-open "$url" >/dev/null 2>&1 &
  else
    echo "Open your browser to: $url"
  fi
}

load_env_file "$ROOT_DIR/.env.example"
load_env_file "$ROOT_DIR/.env"
load_env_file "$FRONTEND_DIR/.env.example"
load_env_file "$FRONTEND_DIR/.env"

KEYCLOAK_PORT="${KEYCLOAK_PORT:-8080}"
API_HTTP_PORT="${API_HTTP_PORT:-8081}"
VITE_KEYCLOAK_URL="${VITE_KEYCLOAK_URL:-http://localhost:${KEYCLOAK_PORT}}"
VITE_KEYCLOAK_REALM="${VITE_KEYCLOAK_REALM:-demo-realm}"
VITE_KEYCLOAK_CLIENT_ID="${VITE_KEYCLOAK_CLIENT_ID:-react-spa}"
VITE_KEYCLOAK_REDIRECT_URI="${VITE_KEYCLOAK_REDIRECT_URI:-http://localhost:5173/auth/callback}"
VITE_API_BASE_URL="${VITE_API_BASE_URL:-http://localhost:${API_HTTP_PORT}}"
REPORTING_API_HTTP_PORT="${REPORTING_API_HTTP_PORT:-8082}"
VITE_REPORTING_API_BASE_URL="${VITE_REPORTING_API_BASE_URL:-http://localhost:${REPORTING_API_HTTP_PORT}}"

export KEYCLOAK_PORT
export API_HTTP_PORT
export REPORTING_API_HTTP_PORT
export VITE_KEYCLOAK_URL
export VITE_KEYCLOAK_REALM
export VITE_KEYCLOAK_CLIENT_ID
export VITE_KEYCLOAK_REDIRECT_URI
export VITE_API_BASE_URL
export VITE_REPORTING_API_BASE_URL

require_command docker
require_command node
require_command npm
require_command curl

if ! docker info >/dev/null 2>&1; then
  echo "Docker is not available. Start Docker Desktop and try again." >&2
  exit 1
fi

if [[ ! -f "$COMPOSE_FILE" ]]; then
  echo "Missing compose file: $COMPOSE_FILE" >&2
  exit 1
fi

if [[ ! -f "$FRONTEND_DIR/package.json" ]]; then
  echo "Missing frontend package.json" >&2
  exit 1
fi

for required_var in \
  KEYCLOAK_PORT \
  API_HTTP_PORT \
  REPORTING_API_HTTP_PORT \
  VITE_KEYCLOAK_URL \
  VITE_KEYCLOAK_REALM \
  VITE_KEYCLOAK_CLIENT_ID \
  VITE_KEYCLOAK_REDIRECT_URI \
  VITE_API_BASE_URL \
  VITE_REPORTING_API_BASE_URL; do
  if [[ -z "${!required_var}" ]]; then
    echo "Missing required configuration: $required_var" >&2
    exit 1
  fi
done

echo "Validated configuration."
echo "Keycloak URL: $VITE_KEYCLOAK_URL"
echo "API 1 URL: $VITE_API_BASE_URL"
echo "API 2 URL: $VITE_REPORTING_API_BASE_URL"
echo "Frontend URL: http://localhost:5173"

echo "Starting Docker services..."
docker compose -f "$COMPOSE_FILE" up -d --build

wait_for_http "$VITE_KEYCLOAK_URL/realms/$VITE_KEYCLOAK_REALM/.well-known/openid-configuration" "200" "Keycloak"
wait_for_http "$VITE_API_BASE_URL/api/demo/protected" "401" "API"
wait_for_http "$VITE_REPORTING_API_BASE_URL/api/reports/summary" "401" "Reporting API"

start_frontend "http://localhost:5173"

open_browser "http://localhost:5173"

echo
echo "Demo is ready."
echo "Frontend log: $FRONTEND_LOG_FILE"
echo "Use alice / Passw0rd! or bob / Passw0rd! on the Keycloak login page."
