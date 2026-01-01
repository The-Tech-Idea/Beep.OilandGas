# PowerShell script to generate SQL scripts for Entity classes
# This is a helper script - actual generation will be done via direct file operations

param(
    [string]$EntityFile,
    [string]$TableName,
    [string]$DatabaseType
)

# Type mappings
$typeMappings = @{
    'Sqlserver' = @{
        'String' = 'NVARCHAR(255)'
        'String40' = 'NVARCHAR(40)'
        'String100' = 'NVARCHAR(100)'
        'String2000' = 'NVARCHAR(2000)'
        'String4000' = 'NVARCHAR(4000)'
        'DateTime' = 'DATETIME'
        'Decimal' = 'NUMERIC(18,2)'
        'Decimal6' = 'NUMERIC(18,6)'
        'Int' = 'INT'
        'BigInt' = 'BIGINT'
        'Bool' = 'BIT'
        'Guid' = 'NVARCHAR(38)'
    }
    'SQLite' = @{
        'String' = 'TEXT'
        'String40' = 'TEXT'
        'String100' = 'TEXT'
        'String2000' = 'TEXT'
        'String4000' = 'TEXT'
        'DateTime' = 'DATETIME'
        'Decimal' = 'REAL'
        'Decimal6' = 'REAL'
        'Int' = 'INTEGER'
        'BigInt' = 'INTEGER'
        'Bool' = 'INTEGER'
        'Guid' = 'TEXT'
    }
    'PostgreSQL' = @{
        'String' = 'VARCHAR(255)'
        'String40' = 'VARCHAR(40)'
        'String100' = 'VARCHAR(100)'
        'String2000' = 'VARCHAR(2000)'
        'String4000' = 'VARCHAR(2000)'
        'DateTime' = 'TIMESTAMP'
        'Decimal' = 'NUMERIC(18,2)'
        'Decimal6' = 'NUMERIC(18,6)'
        'Int' = 'INTEGER'
        'BigInt' = 'BIGINT'
        'Bool' = 'BOOLEAN'
        'Guid' = 'VARCHAR(38)'
    }
    'Oracle' = @{
        'String' = 'VARCHAR2(255)'
        'String40' = 'VARCHAR2(40)'
        'String100' = 'VARCHAR2(100)'
        'String2000' = 'VARCHAR2(2000)'
        'String4000' = 'VARCHAR2(2000)'
        'DateTime' = 'DATE'
        'Decimal' = 'NUMBER(18,2)'
        'Decimal6' = 'NUMBER(18,6)'
        'Int' = 'NUMBER(10)'
        'BigInt' = 'NUMBER(19)'
        'Bool' = 'NUMBER(1)'
        'Guid' = 'VARCHAR2(38)'
    }
    'MySQL' = @{
        'String' = 'VARCHAR(255)'
        'String40' = 'VARCHAR(40)'
        'String100' = 'VARCHAR(100)'
        'String2000' = 'VARCHAR(2000)'
        'String4000' = 'VARCHAR(2000)'
        'DateTime' = 'DATETIME'
        'Decimal' = 'DECIMAL(18,2)'
        'Decimal6' = 'DECIMAL(18,6)'
        'Int' = 'INT'
        'BigInt' = 'BIGINT'
        'Bool' = 'TINYINT(1)'
        'Guid' = 'VARCHAR(38)'
    }
    'MariaDB' = @{
        'String' = 'VARCHAR(255)'
        'String40' = 'VARCHAR(40)'
        'String100' = 'VARCHAR(100)'
        'String2000' = 'VARCHAR(2000)'
        'String4000' = 'VARCHAR(2000)'
        'DateTime' = 'DATETIME'
        'Decimal' = 'DECIMAL(18,2)'
        'Decimal6' = 'DECIMAL(18,6)'
        'Int' = 'INT'
        'BigInt' = 'BIGINT'
        'Bool' = 'TINYINT(1)'
        'Guid' = 'VARCHAR(38)'
    }
}

Write-Host "SQL Script Generator Helper"
Write-Host "This script provides type mappings for SQL generation"
