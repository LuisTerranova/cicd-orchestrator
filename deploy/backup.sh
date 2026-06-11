#!/bin/bash
set -euo pipefail

# CI/CD Orchestrator — PostgreSQL Backup Script
# Usage: ./deploy/backup.sh [output-dir]
# Default output: /tmp/orchestrator-backups/

BACKUP_DIR="${1:-/tmp/orchestrator-backups}"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
mkdir -p "$BACKUP_DIR"

# Load connection string from .env if available
if [ -f .env ]; then
    # shellcheck disable=SC1091
    source .env
fi

DB_URL="${DB_CONNECTION_STRING:-}"
if [ -z "$DB_URL" ]; then
    # Try docker-compose default
    DB_URL="Host=localhost;Database=orchestrator;Username=orchestrator;Password=dev123"
fi

BACKUP_FILE="$BACKUP_DIR/orchestrator_$TIMESTAMP.sql.gz"
LOG_FILE="$BACKUP_DIR/backup_$TIMESTAMP.log"

echo "[$(date -u '+%Y-%m-%d %H:%M:%S UTC')] Starting backup..." | tee -a "$LOG_FILE"

# Extract connection parameters from the URL-like string
if [[ "$DB_URL" == postgresql://* ]] || [[ "$DB_URL" == postgres://* ]]; then
    # postgresql://user:pass@host:port/db
    PGPASSWORD=$(echo "$DB_URL" | sed -n 's/.*:\/\/[^:]*:\([^@]*\)@.*/\1/p') \
    pg_dump \
        --dbname="$DB_URL" \
        --no-owner \
        --no-acl \
        --compress=9 \
        --file="$BACKUP_FILE" \
        2>> "$LOG_FILE"
else
    # Host=...;Database=...;Username=...;Password=...
    DB_HOST=$(echo "$DB_URL" | sed -n 's/.*Host=\([^;]*\).*/\1/p')
    DB_PORT=$(echo "$DB_URL" | sed -n 's/.*Port=\([0-9]*\).*/\1/p')
    DB_NAME=$(echo "$DB_URL" | sed -n 's/.*Database=\([^;]*\).*/\1/p')
    DB_USER=$(echo "$DB_URL" | sed -n 's/.*Username=\([^;]*\).*/\1/p')
    DB_PASS=$(echo "$DB_URL" | sed -n 's/.*Password=\([^;]*\).*/\1/p')
    DB_PORT=${DB_PORT:-5432}

    PGPASSWORD="$DB_PASS" pg_dump \
        --host="$DB_HOST" \
        --port="$DB_PORT" \
        --dbname="$DB_NAME" \
        --username="$DB_USER" \
        --no-owner \
        --no-acl \
        --compress=9 \
        --file="$BACKUP_FILE" \
        2>> "$LOG_FILE"
fi

echo "[$(date -u '+%Y-%m-%d %H:%M:%S UTC')] Backup complete: $(du -h "$BACKUP_FILE" | cut -f1)" | tee -a "$LOG_FILE"

# Prune backups older than 30 days
echo "[$(date -u '+%Y-%m-%d %H:%M:%S UTC')] Pruning backups older than 30 days..." | tee -a "$LOG_FILE"
find "$BACKUP_DIR" -name "orchestrator_*.sql.gz" -type f -mtime +30 -delete
find "$BACKUP_DIR" -name "backup_*.log" -type f -mtime +30 -delete
echo "[$(date -u '+%Y-%m-%d %H:%M:%S UTC')] Done." | tee -a "$LOG_FILE"
