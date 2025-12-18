#!/bin/bash

# PPDM39 PostgreSQL Installation Script
# This script creates the PPDM39 database and runs all required scripts

set -e  # Exit on error

# Configuration
DATABASE_NAME=${DATABASE_NAME:-ppdm39}
DB_USER=${DB_USER:-postgres}
DB_HOST=${DB_HOST:-localhost}
DB_PORT=${DB_PORT:-5432}

echo "PPDM39 PostgreSQL Installation Script"
echo "======================================"
echo "Database: $DATABASE_NAME"
echo "User: $DB_USER"
echo "Host: $DB_HOST"
echo "Port: $DB_PORT"
echo ""

# Get the directory where the script is located
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# Check if psql is available
if ! command -v psql &> /dev/null; then
    echo "Error: psql command not found. Please install PostgreSQL client tools."
    exit 1
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
    psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DATABASE_NAME" -f "$file"
    if [ $? -ne 0 ]; then
        echo "Error executing $file"
        exit 1
    fi
    echo ""
}

# Create database if it doesn't exist
echo "Checking if database exists..."
DB_EXISTS=$(psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -lqt | cut -d \| -f 1 | grep -w "$DATABASE_NAME" | wc -l)

if [ "$DB_EXISTS" = "0" ]; then
    echo "Creating database: $DATABASE_NAME"
    psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -c "CREATE DATABASE $DATABASE_NAME"
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

echo "Installation completed successfully!"
