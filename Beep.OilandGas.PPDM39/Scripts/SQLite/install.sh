#!/bin/bash

# PPDM39 SQLite Installation Script
# This script creates the PPDM39 database and runs all required scripts

set -e  # Exit on error

# Configuration
DATABASE_NAME=${DATABASE_NAME:-ppdm39.db}

echo "PPDM39 SQLite Installation Script"
echo "=================================="
echo "Database: $DATABASE_NAME"
echo ""

# Get the directory where the script is located
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
DB_PATH="$SCRIPT_DIR/$DATABASE_NAME"

# Check if sqlite3 is available
if ! command -v sqlite3 &> /dev/null; then
    echo "Error: sqlite3 command not found. Please install SQLite."
    exit 1
fi

# Remove existing database if it exists (optional - comment out if you want to keep existing data)
# rm -f "$DB_PATH"

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
    sqlite3 "$DB_PATH" < "$file"
    if [ $? -ne 0 ]; then
        echo "Error executing $file"
        exit 1
    fi
    echo ""
}

# Enable foreign keys
echo "Enabling foreign key constraints..."
sqlite3 "$DB_PATH" "PRAGMA foreign_keys = ON;"

# Run scripts in order
run_sql_file "$SCRIPT_DIR/TAB.sql" "Creating Tables and Columns" 0
run_sql_file "$SCRIPT_DIR/PK.sql" "Creating Primary Keys" 0
run_sql_file "$SCRIPT_DIR/CK.sql" "Creating Check Constraints" 0
run_sql_file "$SCRIPT_DIR/FK.sql" "Creating Foreign Key Constraints" 0
run_sql_file "$SCRIPT_DIR/OUOM.sql" "Creating Original Units of Measure Foreign Keys" 0
run_sql_file "$SCRIPT_DIR/UOM.sql" "Creating Units of Measure Foreign Keys" 0
run_sql_file "$SCRIPT_DIR/RQUAL.sql" "Creating ROW_QUALITY Foreign Keys" 0
run_sql_file "$SCRIPT_DIR/RSRC.sql" "Creating SOURCE Foreign Keys" 0
run_sql_file "$SCRIPT_DIR/GUID.sql" "Creating GUID Constraints" 1

# Note: TCM, CCM, and SYN are not applicable for SQLite
echo "Skipping TCM.sql (SQLite doesn't support table comments)"
echo "Skipping CCM.sql (SQLite doesn't support column comments)"
echo "Skipping SYN.sql (SQLite doesn't support synonyms)"
echo ""

echo "Installation completed successfully!"
echo "Database created at: $DB_PATH"
