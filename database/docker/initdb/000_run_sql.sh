#!/bin/bash
set -euo pipefail

run_sql_dir() {
  local sql_dir="$1"

  if [ ! -d "$sql_dir" ]; then
    return
  fi

  shopt -s nullglob
  local sql_files=("$sql_dir"/*.sql)
  shopt -u nullglob

  if [ ${#sql_files[@]} -eq 0 ]; then
    echo "No SQL files found in $sql_dir"
    return
  fi

  for sql_file in "${sql_files[@]}"; do
    echo "Running $sql_file"
    psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" -f "$sql_file"
  done
}

run_sql_dir /docker-entrypoint-initdb.d/migrations
run_sql_dir /docker-entrypoint-initdb.d/seeds
