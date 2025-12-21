#!/bin/bash

# PPDM39 MariaDB Installation Script
# This script creates the PPDM39 database and runs all required scripts

set -e  # Exit on error

# Configuration
DATABASE_NAME=${DATABASE_NAME:-ppdm39}
DB_USER=${DB_USER:-root}
DB_HOST=${DB_HOST:-localhost}
DB_PORT=${DB_PORT:-3306}

echo "PPDM39 MariaDB Installation Script"
echo "==================================="
echo "Database: $DATABASE_NAME"
echo "User: $DB_USER"
echo "Host: $DB_HOST"
echo "Port: $DB_PORT"
echo ""

# Get the directory where the script is located
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# Check if mysql is available
if ! command -v mysql &> /dev/null; then
    echo "Error: mysql command not found. Please install MariaDB client tools."
    exit 1
fi

# Prompt for password if not set
if [ -z "$MYSQL_PWD" ]; then
    read -sp "Enter MySQL password for $DB_USER: " MYSQL_PWD
    echo ""
    export MYSQL_PWD
fi

# Function to run a SQL file
run_sql_file() {
    local file=$1
    local description=$2
    local optional=${3:-0}
    
    if [ ! -f "$file" ]; then
        if [ "$optional" = "1" ]; then
            echo "Skipping optional file: $file (not found)"
            return 0
        else
            echo "Error: Required file not found: $file"
            exit 1
        fi
    fi
    
    echo "Running: $description [$file]"
    mysql -h "$DB_HOST" -P "$DB_PORT" -u "$DB_USER" "$DATABASE_NAME" < "$file"
    if [ $? -ne 0 ]; then
        echo "Error executing $file"
        exit 1
    fi
    echo ""
}

# Create database if it doesn't exist
echo "Checking if database exists..."
DB_EXISTS=$(mysql -h "$DB_HOST" -P "$DB_PORT" -u "$DB_USER" -e "SHOW DATABASES LIKE '$DATABASE_NAME';" | grep -c "$DATABASE_NAME" || true)

if [ "$DB_EXISTS" = "0" ]; then
    echo "Creating database: $DATABASE_NAME"
    mysql -h "$DB_HOST" -P "$DB_PORT" -u "$DB_USER" -e "CREATE DATABASE $DATABASE_NAME CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"
    echo ""
fi

# Run scripts in order
run_sql_file "$SCRIPT_DIR/TAB.sql" "Creating Tables and Columns" 0
run_sql_file "$SCRIPT_DIR/PK.sql" "Creating Primary Keys" 0
run_sql_file "$SCRIPT_DIR/CK.sql" "Creating Check Constraints" 0
run_sql_file "$SCRIPT_DIR/FK.sql" "Creating Foreign Key Constraints" 0
run_sql_file "$SCRIPT_DIR/OUOM.sql" "Creating Original Units of Measure Foreign Keys" 0
run_sql_file "$SCRIPT_DIR/UOM.sql" "Creating Units of Measure Foreign Keys" 0
run_sql_file "$SCRIPT_DIR/RQUAL.sql" "Creating ROW_QUALITY Foreign Keys" 0
run_sql_file "$SCRIPT_DIR/RSRC.sql" "Creating SOURCE Foreign Keys" 0
run_sql_file "$SCRIPT_DIR/TCM.sql" "Creating Table Comments" 1
run_sql_file "$SCRIPT_DIR/CCM.sql" "Creating Column Comments" 1
run_sql_file "$SCRIPT_DIR/SYN.sql" "Creating Synonyms" 1
run_sql_file "$SCRIPT_DIR/GUID.sql" "Creating GUID Constraints" 1
run_sql_file "$SCRIPT_DIR/ACCESS_CONTROL_TAB.sql" "Creating Access Control Tables" 0
run_sql_file "$SCRIPT_DIR/ACCESS_CONTROL_PK.sql" "Creating Access Control Primary Keys" 0
run_sql_file "$SCRIPT_DIR/ACCESS_CONTROL_FK.sql" "Creating Access Control Foreign Keys" 0
run_sql_file "$SCRIPT_DIR/ACCESS_CONTROL_IX.sql" "Creating Access Control Indexes" 0

echo "Installation completed successfully!"
