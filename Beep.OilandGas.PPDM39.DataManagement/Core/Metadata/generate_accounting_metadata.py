#!/usr/bin/env python3
"""
Generate metadata entries for new accounting tables
"""
import re

# Define metadata for each new table
tables_metadata = {
    'ACCOUNTING_METHOD': {
        'table_name': 'accounting_method',
        'entity_type': 'ACCOUNTING_METHOD',
        'primary_key': 'ACCOUNTING_METHOD_ID',
        'module': 'Accounting',
        'foreign_keys': [
            ('FIELD_ID', 'FIELD', 'FIELD_ID'),
        ]
    },
    'ACCOUNTING_COST': {
        'table_name': 'accounting_cost',
        'entity_type': 'ACCOUNTING_COST',
        'primary_key': 'ACCOUNTING_COST_ID',
        'module': 'Accounting',
        'foreign_keys': [
            ('FINANCE_ID', 'FINANCE', 'FINANCE_ID'),
            ('FIELD_ID', 'FIELD', 'FIELD_ID'),
            ('WELL_ID', 'WELL', 'WELL_ID'),
            ('POOL_ID', 'POOL', 'POOL_ID'),
        ]
    },
    'ACCOUNTING_AMORTIZATION': {
        'table_name': 'accounting_amortization',
        'entity_type': 'ACCOUNTING_AMORTIZATION',
        'primary_key': 'ACCOUNTING_AMORTIZATION_ID',
        'module': 'Accounting',
        'foreign_keys': [
            ('FIELD_ID', 'FIELD', 'FIELD_ID'),
            ('WELL_ID', 'WELL', 'WELL_ID'),
            ('POOL_ID', 'POOL', 'POOL_ID'),
        ]
    },
    'ASSET_RETIREMENT_OBLIGATION': {
        'table_name': 'asset_retirement_obligation',
        'entity_type': 'ASSET_RETIREMENT_OBLIGATION',
        'primary_key': 'ARO_ID',
        'module': 'Accounting',
        'foreign_keys': [
            ('FIELD_ID', 'FIELD', 'FIELD_ID'),
            ('WELL_ID', 'WELL', 'WELL_ID'),
            ('FACILITY_ID', 'FACILITY', 'FACILITY_ID'),
        ]
    },
    'DEPLETION_CALCULATION': {
        'table_name': 'depletion_calculation',
        'entity_type': 'DEPLETION_CALCULATION',
        'primary_key': 'DEPLETION_ID',
        'module': 'Accounting',
        'foreign_keys': [
            ('FIELD_ID', 'FIELD', 'FIELD_ID'),
            ('WELL_ID', 'WELL', 'WELL_ID'),
            ('POOL_ID', 'POOL', 'POOL_ID'),
        ]
    },
    'PRODUCTION_ALLOCATION': {
        'table_name': 'production_allocation',
        'entity_type': 'PRODUCTION_ALLOCATION',
        'primary_key': 'PRODUCTION_ALLOCATION_ID',
        'module': 'Accounting',
        'foreign_keys': [
            ('PDEN_ID', 'PDEN', 'PDEN_ID'),
            ('FIELD_ID', 'FIELD', 'FIELD_ID'),
            ('WELL_ID', 'WELL', 'WELL_ID'),
            ('POOL_ID', 'POOL', 'POOL_ID'),
        ]
    },
    'REVENUE_TRANSACTION': {
        'table_name': 'revenue_transaction',
        'entity_type': 'REVENUE_TRANSACTION',
        'primary_key': 'REVENUE_TRANSACTION_ID',
        'module': 'Accounting',
        'foreign_keys': [
            ('FINANCE_ID', 'FINANCE', 'FINANCE_ID'),
            ('FIELD_ID', 'FIELD', 'FIELD_ID'),
            ('WELL_ID', 'WELL', 'WELL_ID'),
            ('PDEN_ID', 'PDEN', 'PDEN_ID'),
        ]
    },
    'REVENUE_DEDUCTION': {
        'table_name': 'revenue_deduction',
        'entity_type': 'REVENUE_DEDUCTION',
        'primary_key': 'REVENUE_DEDUCTION_ID',
        'module': 'Accounting',
        'foreign_keys': [
            ('REVENUE_TRANSACTION_ID', 'REVENUE_TRANSACTION', 'REVENUE_TRANSACTION_ID'),
        ]
    },
    'REVENUE_DISTRIBUTION': {
        'table_name': 'revenue_distribution',
        'entity_type': 'REVENUE_DISTRIBUTION',
        'primary_key': 'REVENUE_DISTRIBUTION_ID',
        'module': 'Accounting',
        'foreign_keys': [
            ('REVENUE_TRANSACTION_ID', 'REVENUE_TRANSACTION', 'REVENUE_TRANSACTION_ID'),
            ('BUSINESS_ASSOCIATE_ID', 'BUSINESS_ASSOCIATE', 'BA_ID'),
        ]
    },
    'ROYALTY_CALCULATION': {
        'table_name': 'royalty_calculation',
        'entity_type': 'ROYALTY_CALCULATION',
        'primary_key': 'ROYALTY_CALCULATION_ID',
        'module': 'Accounting',
        'foreign_keys': [
            ('OBLIGATION_ID', 'OBLIGATION', 'OBLIGATION_ID'),
            ('REVENUE_TRANSACTION_ID', 'REVENUE_TRANSACTION', 'REVENUE_TRANSACTION_ID'),
            ('FIELD_ID', 'FIELD', 'FIELD_ID'),
            ('WELL_ID', 'WELL', 'WELL_ID'),
        ]
    },
    'JOINT_INTEREST_BILL': {
        'table_name': 'joint_interest_bill',
        'entity_type': 'JOINT_INTEREST_BILL',
        'primary_key': 'JIB_ID',
        'module': 'Accounting',
        'foreign_keys': [
            ('INTEREST_SET_ID', 'INTEREST_SET', 'INT_SET_ID'),
            ('FIELD_ID', 'FIELD', 'FIELD_ID'),
            ('OPERATOR_ID', 'BUSINESS_ASSOCIATE', 'BA_ID'),
        ]
    },
    'JIB_COST_ALLOCATION': {
        'table_name': 'jib_cost_allocation',
        'entity_type': 'JIB_COST_ALLOCATION',
        'primary_key': 'JIB_COST_ALLOCATION_ID',
        'module': 'Accounting',
        'foreign_keys': [
            ('JIB_ID', 'JOINT_INTEREST_BILL', 'JIB_ID'),
            ('FINANCE_ID', 'FINANCE', 'FINANCE_ID'),
        ]
    },
}

def generate_metadata_entry(table_key, table_info):
    """Generate C# metadata entry"""
    fk_entries = ""
    if table_info['foreign_keys']:
        fk_entries = ",\n                ForeignKeys = new List<PPDMForeignKey>\n                {\n"
        for fk_col, ref_table, ref_pk in table_info['foreign_keys']:
            fk_entries += f"""                    new PPDMForeignKey
                    {{
                        ForeignKeyColumn = "{fk_col}",
                        ReferencedTable = "{ref_table}",
                        ReferencedPrimaryKey = "{ref_pk}",
                        RelationshipType = "OneToMany"
                    }},
"""
        fk_entries += "                }"
    else:
        fk_entries = ",\n                ForeignKeys = new List<PPDMForeignKey>()"
    
    return f"""            // {table_info['table_name']}
            metadata["{table_key}"] = new PPDMTableMetadata
            {{
                TableName = "{table_info['table_name']}",
                EntityTypeName = "{table_info['entity_type']}",
                PrimaryKeyColumn = "{table_info['primary_key']}",
                Module = "{table_info['module']}",
                CommonColumns = new List<string> {{ "ACTIVE_IND", "ROW_CREATED_BY", "ROW_CREATED_DATE", "ROW_CHANGED_BY", "ROW_CHANGED_DATE", "PPDM_GUID", "ROW_EFFECTIVE_DATE", "ROW_EXPIRY_DATE", "ROW_QUALITY" }}{fk_entries}
            }};
"""

def main():
    """Generate metadata entries"""
    metadata_entries = []
    for table_key in sorted(tables_metadata.keys()):
        metadata_entries.append(generate_metadata_entry(table_key, tables_metadata[table_key]))
    
    output = "\n".join(metadata_entries)
    
    # Write to file
    output_file = "accounting_metadata_entries.txt"
    with open(output_file, 'w', encoding='utf-8') as f:
        f.write(output)
    
    print(f"Generated metadata entries in {output_file}")
    print(f"\nTotal tables: {len(tables_metadata)}")
    print("\nCopy the content and insert before 'return metadata;' in PPDM39Metadata.Generated.cs")

if __name__ == '__main__':
    main()

