#!/usr/bin/env python3
"""
Generate C# model classes for new accounting tables from SQL table definitions
"""
import os
import re
from pathlib import Path

# Table definitions with field mappings
tables = {
    'ACCOUNTING_COST': {
        'fields': [
            ('ACCOUNTING_COST_ID', 'NVARCHAR(40)', 'String', False),
            ('FINANCE_ID', 'NVARCHAR(40)', 'String', True),
            ('PROPERTY_ID', 'NVARCHAR(40)', 'String', True),
            ('WELL_ID', 'NVARCHAR(40)', 'String', True),
            ('FIELD_ID', 'NVARCHAR(40)', 'String', True),
            ('POOL_ID', 'NVARCHAR(40)', 'String', True),
            ('COST_TYPE', 'NVARCHAR(40)', 'String', False),
            ('COST_CATEGORY', 'NVARCHAR(40)', 'String', True),
            ('AMOUNT', 'NUMERIC(18,2)', 'Decimal', False),
            ('COST_DATE', 'DATE', 'DateTime?', True),
            ('IS_CAPITALIZED', 'NVARCHAR(1)', 'String', True),
            ('IS_EXPENSED', 'NVARCHAR(1)', 'String', True),
            ('DRY_HOLE_FLAG', 'NVARCHAR(1)', 'String', True),
            ('DESCRIPTION', 'NVARCHAR(2000)', 'String', True),
            ('ACTIVE_IND', 'NVARCHAR(1)', 'String', True),
            ('PPDM_GUID', 'NVARCHAR(38)', 'String', True),
            ('REMARK', 'NVARCHAR(2000)', 'String', True),
            ('SOURCE', 'NVARCHAR(40)', 'String', True),
            ('ROW_CHANGED_BY', 'NVARCHAR(30)', 'String', True),
            ('ROW_CHANGED_DATE', 'DATE', 'DateTime?', True),
            ('ROW_CREATED_BY', 'NVARCHAR(30)', 'String', True),
            ('ROW_CREATED_DATE', 'DATE', 'DateTime?', True),
            ('ROW_EFFECTIVE_DATE', 'DATE', 'DateTime?', True),
            ('ROW_EXPIRY_DATE', 'DATE', 'DateTime?', True),
            ('ROW_ID', 'NVARCHAR(40)', 'String', False),
        ]
    },
    'ACCOUNTING_AMORTIZATION': {
        'fields': [
            ('ACCOUNTING_AMORTIZATION_ID', 'NVARCHAR(40)', 'String', False),
            ('PROPERTY_ID', 'NVARCHAR(40)', 'String', True),
            ('WELL_ID', 'NVARCHAR(40)', 'String', True),
            ('POOL_ID', 'NVARCHAR(40)', 'String', True),
            ('FIELD_ID', 'NVARCHAR(40)', 'String', True),
            ('PERIOD_START_DATE', 'DATE', 'DateTime', False),
            ('PERIOD_END_DATE', 'DATE', 'DateTime', False),
            ('CAPITALIZED_COST', 'NUMERIC(18,2)', 'Decimal', False),
            ('PRODUCTION_BOE', 'NUMERIC(18,2)', 'Decimal?', True),
            ('TOTAL_RESERVES_BOE', 'NUMERIC(18,2)', 'Decimal?', True),
            ('AMORTIZATION_AMOUNT', 'NUMERIC(18,2)', 'Decimal', False),
            ('AMORTIZATION_METHOD', 'NVARCHAR(40)', 'String', True),
            ('DESCRIPTION', 'NVARCHAR(2000)', 'String', True),
            ('ACTIVE_IND', 'NVARCHAR(1)', 'String', True),
            ('PPDM_GUID', 'NVARCHAR(38)', 'String', True),
            ('REMARK', 'NVARCHAR(2000)', 'String', True),
            ('SOURCE', 'NVARCHAR(40)', 'String', True),
            ('ROW_CHANGED_BY', 'NVARCHAR(30)', 'String', True),
            ('ROW_CHANGED_DATE', 'DATE', 'DateTime?', True),
            ('ROW_CREATED_BY', 'NVARCHAR(30)', 'String', True),
            ('ROW_CREATED_DATE', 'DATE', 'DateTime?', True),
            ('ROW_EFFECTIVE_DATE', 'DATE', 'DateTime?', True),
            ('ROW_EXPIRY_DATE', 'DATE', 'DateTime?', True),
            ('ROW_ID', 'NVARCHAR(40)', 'String', False),
        ]
    },
    'ASSET_RETIREMENT_OBLIGATION': {
        'fields': [
            ('ARO_ID', 'NVARCHAR(40)', 'String', False),
            ('FIELD_ID', 'NVARCHAR(40)', 'String', True),
            ('WELL_ID', 'NVARCHAR(40)', 'String', True),
            ('FACILITY_ID', 'NVARCHAR(40)', 'String', True),
            ('ESTIMATED_COST', 'NUMERIC(18,2)', 'Decimal', False),
            ('ESTIMATED_RETIREMENT_DATE', 'DATE', 'DateTime?', True),
            ('DISCOUNT_RATE', 'NUMERIC(10,4)', 'Decimal?', True),
            ('PRESENT_VALUE', 'NUMERIC(18,2)', 'Decimal?', True),
            ('ACCRETION_EXPENSE', 'NUMERIC(18,2)', 'Decimal?', True),
            ('STATUS', 'NVARCHAR(40)', 'String', True),
            ('DESCRIPTION', 'NVARCHAR(2000)', 'String', True),
            ('ACTIVE_IND', 'NVARCHAR(1)', 'String', True),
            ('PPDM_GUID', 'NVARCHAR(38)', 'String', True),
            ('REMARK', 'NVARCHAR(2000)', 'String', True),
            ('SOURCE', 'NVARCHAR(40)', 'String', True),
            ('ROW_CHANGED_BY', 'NVARCHAR(30)', 'String', True),
            ('ROW_CHANGED_DATE', 'DATE', 'DateTime?', True),
            ('ROW_CREATED_BY', 'NVARCHAR(30)', 'String', True),
            ('ROW_CREATED_DATE', 'DATE', 'DateTime?', True),
            ('ROW_EFFECTIVE_DATE', 'DATE', 'DateTime?', True),
            ('ROW_EXPIRY_DATE', 'DATE', 'DateTime?', True),
            ('ROW_ID', 'NVARCHAR(40)', 'String', False),
        ]
    },
    'DEPLETION_CALCULATION': {
        'fields': [
            ('DEPLETION_ID', 'NVARCHAR(40)', 'String', False),
            ('PROPERTY_ID', 'NVARCHAR(40)', 'String', True),
            ('WELL_ID', 'NVARCHAR(40)', 'String', True),
            ('POOL_ID', 'NVARCHAR(40)', 'String', True),
            ('FIELD_ID', 'NVARCHAR(40)', 'String', True),
            ('DEPLETION_METHOD', 'NVARCHAR(40)', 'String', False),
            ('COST_BASIS', 'NUMERIC(18,2)', 'Decimal?', True),
            ('ESTIMATED_RESERVES', 'NUMERIC(18,2)', 'Decimal?', True),
            ('PRODUCTION', 'NUMERIC(18,2)', 'Decimal?', True),
            ('DEPLETION_RATE', 'NUMERIC(10,4)', 'Decimal?', True),
            ('DEPLETION_AMOUNT', 'NUMERIC(18,2)', 'Decimal?', True),
            ('PERCENTAGE_RATE', 'NUMERIC(10,4)', 'Decimal?', True),
            ('PERIOD_START_DATE', 'DATE', 'DateTime?', True),
            ('PERIOD_END_DATE', 'DATE', 'DateTime?', True),
            ('DESCRIPTION', 'NVARCHAR(2000)', 'String', True),
            ('ACTIVE_IND', 'NVARCHAR(1)', 'String', True),
            ('PPDM_GUID', 'NVARCHAR(38)', 'String', True),
            ('REMARK', 'NVARCHAR(2000)', 'String', True),
            ('SOURCE', 'NVARCHAR(40)', 'String', True),
            ('ROW_CHANGED_BY', 'NVARCHAR(30)', 'String', True),
            ('ROW_CHANGED_DATE', 'DATE', 'DateTime?', True),
            ('ROW_CREATED_BY', 'NVARCHAR(30)', 'String', True),
            ('ROW_CREATED_DATE', 'DATE', 'DateTime?', True),
            ('ROW_EFFECTIVE_DATE', 'DATE', 'DateTime?', True),
            ('ROW_EXPIRY_DATE', 'DATE', 'DateTime?', True),
            ('ROW_ID', 'NVARCHAR(40)', 'String', False),
        ]
    },
    'PRODUCTION_ALLOCATION': {
        'fields': [
            ('PRODUCTION_ALLOCATION_ID', 'NVARCHAR(40)', 'String', False),
            ('PDEN_ID', 'NVARCHAR(40)', 'String', True),
            ('FIELD_ID', 'NVARCHAR(40)', 'String', True),
            ('WELL_ID', 'NVARCHAR(40)', 'String', True),
            ('POOL_ID', 'NVARCHAR(40)', 'String', True),
            ('ALLOCATION_DATE', 'DATE', 'DateTime', False),
            ('TOTAL_PRODUCTION', 'NUMERIC(18,2)', 'Decimal', False),
            ('ALLOCATION_METHOD', 'NVARCHAR(40)', 'String', False),
            ('ALLOCATION_RESULTS_JSON', 'NVARCHAR(MAX)', 'String', True),
            ('DESCRIPTION', 'NVARCHAR(2000)', 'String', True),
            ('ACTIVE_IND', 'NVARCHAR(1)', 'String', True),
            ('PPDM_GUID', 'NVARCHAR(38)', 'String', True),
            ('REMARK', 'NVARCHAR(2000)', 'String', True),
            ('SOURCE', 'NVARCHAR(40)', 'String', True),
            ('ROW_CHANGED_BY', 'NVARCHAR(30)', 'String', True),
            ('ROW_CHANGED_DATE', 'DATE', 'DateTime?', True),
            ('ROW_CREATED_BY', 'NVARCHAR(30)', 'String', True),
            ('ROW_CREATED_DATE', 'DATE', 'DateTime?', True),
            ('ROW_EFFECTIVE_DATE', 'DATE', 'DateTime?', True),
            ('ROW_EXPIRY_DATE', 'DATE', 'DateTime?', True),
            ('ROW_ID', 'NVARCHAR(40)', 'String', False),
        ]
    },
    'REVENUE_TRANSACTION': {
        'fields': [
            ('REVENUE_TRANSACTION_ID', 'NVARCHAR(40)', 'String', False),
            ('FINANCE_ID', 'NVARCHAR(40)', 'String', True),
            ('FIELD_ID', 'NVARCHAR(40)', 'String', True),
            ('WELL_ID', 'NVARCHAR(40)', 'String', True),
            ('PDEN_ID', 'NVARCHAR(40)', 'String', True),
            ('PRODUCTION_DATE', 'DATE', 'DateTime', False),
            ('OIL_VOLUME', 'NUMERIC(18,2)', 'Decimal?', True),
            ('GAS_VOLUME', 'NUMERIC(18,2)', 'Decimal?', True),
            ('OIL_PRICE', 'NUMERIC(18,2)', 'Decimal?', True),
            ('GAS_PRICE', 'NUMERIC(18,2)', 'Decimal?', True),
            ('GROSS_REVENUE', 'NUMERIC(18,2)', 'Decimal', False),
            ('NET_REVENUE', 'NUMERIC(18,2)', 'Decimal?', True),
            ('REVENUE_RECOGNITION_STATUS', 'NVARCHAR(40)', 'String', True),
            ('DESCRIPTION', 'NVARCHAR(2000)', 'String', True),
            ('ACTIVE_IND', 'NVARCHAR(1)', 'String', True),
            ('PPDM_GUID', 'NVARCHAR(38)', 'String', True),
            ('REMARK', 'NVARCHAR(2000)', 'String', True),
            ('SOURCE', 'NVARCHAR(40)', 'String', True),
            ('ROW_CHANGED_BY', 'NVARCHAR(30)', 'String', True),
            ('ROW_CHANGED_DATE', 'DATE', 'DateTime?', True),
            ('ROW_CREATED_BY', 'NVARCHAR(30)', 'String', True),
            ('ROW_CREATED_DATE', 'DATE', 'DateTime?', True),
            ('ROW_EFFECTIVE_DATE', 'DATE', 'DateTime?', True),
            ('ROW_EXPIRY_DATE', 'DATE', 'DateTime?', True),
            ('ROW_ID', 'NVARCHAR(40)', 'String', False),
        ]
    },
    'REVENUE_DEDUCTION': {
        'fields': [
            ('REVENUE_DEDUCTION_ID', 'NVARCHAR(40)', 'String', False),
            ('REVENUE_TRANSACTION_ID', 'NVARCHAR(40)', 'String', False),
            ('DEDUCTION_TYPE', 'NVARCHAR(40)', 'String', False),
            ('AMOUNT', 'NUMERIC(18,2)', 'Decimal', False),
            ('DESCRIPTION', 'NVARCHAR(2000)', 'String', True),
            ('ACTIVE_IND', 'NVARCHAR(1)', 'String', True),
            ('PPDM_GUID', 'NVARCHAR(38)', 'String', True),
            ('REMARK', 'NVARCHAR(2000)', 'String', True),
            ('SOURCE', 'NVARCHAR(40)', 'String', True),
            ('ROW_CHANGED_BY', 'NVARCHAR(30)', 'String', True),
            ('ROW_CHANGED_DATE', 'DATE', 'DateTime?', True),
            ('ROW_CREATED_BY', 'NVARCHAR(30)', 'String', True),
            ('ROW_CREATED_DATE', 'DATE', 'DateTime?', True),
            ('ROW_EFFECTIVE_DATE', 'DATE', 'DateTime?', True),
            ('ROW_EXPIRY_DATE', 'DATE', 'DateTime?', True),
            ('ROW_ID', 'NVARCHAR(40)', 'String', False),
        ]
    },
    'REVENUE_DISTRIBUTION': {
        'fields': [
            ('REVENUE_DISTRIBUTION_ID', 'NVARCHAR(40)', 'String', False),
            ('REVENUE_TRANSACTION_ID', 'NVARCHAR(40)', 'String', False),
            ('INT_SET_PARTNER_ID', 'NVARCHAR(40)', 'String', True),
            ('BUSINESS_ASSOCIATE_ID', 'NVARCHAR(40)', 'String', True),
            ('WORKING_INTEREST_PERCENTAGE', 'NUMERIC(10,4)', 'Decimal?', True),
            ('NET_REVENUE_INTEREST_PERCENTAGE', 'NUMERIC(10,4)', 'Decimal?', True),
            ('REVENUE_AMOUNT', 'NUMERIC(18,2)', 'Decimal', False),
            ('DESCRIPTION', 'NVARCHAR(2000)', 'String', True),
            ('ACTIVE_IND', 'NVARCHAR(1)', 'String', True),
            ('PPDM_GUID', 'NVARCHAR(38)', 'String', True),
            ('REMARK', 'NVARCHAR(2000)', 'String', True),
            ('SOURCE', 'NVARCHAR(40)', 'String', True),
            ('ROW_CHANGED_BY', 'NVARCHAR(30)', 'String', True),
            ('ROW_CHANGED_DATE', 'DATE', 'DateTime?', True),
            ('ROW_CREATED_BY', 'NVARCHAR(30)', 'String', True),
            ('ROW_CREATED_DATE', 'DATE', 'DateTime?', True),
            ('ROW_EFFECTIVE_DATE', 'DATE', 'DateTime?', True),
            ('ROW_EXPIRY_DATE', 'DATE', 'DateTime?', True),
            ('ROW_ID', 'NVARCHAR(40)', 'String', False),
        ]
    },
    'ROYALTY_CALCULATION': {
        'fields': [
            ('ROYALTY_CALCULATION_ID', 'NVARCHAR(40)', 'String', False),
            ('OBLIGATION_ID', 'NVARCHAR(40)', 'String', True),
            ('REVENUE_TRANSACTION_ID', 'NVARCHAR(40)', 'String', True),
            ('FIELD_ID', 'NVARCHAR(40)', 'String', True),
            ('WELL_ID', 'NVARCHAR(40)', 'String', True),
            ('CALCULATION_DATE', 'DATE', 'DateTime', False),
            ('PRODUCTION_VOLUME', 'NUMERIC(18,2)', 'Decimal?', True),
            ('ROYALTY_RATE', 'NUMERIC(10,4)', 'Decimal', False),
            ('ROYALTY_AMOUNT', 'NUMERIC(18,2)', 'Decimal', False),
            ('GROSS_REVENUE', 'NUMERIC(18,2)', 'Decimal?', True),
            ('DEDUCTIONS', 'NUMERIC(18,2)', 'Decimal?', True),
            ('NET_REVENUE', 'NUMERIC(18,2)', 'Decimal?', True),
            ('DESCRIPTION', 'NVARCHAR(2000)', 'String', True),
            ('ACTIVE_IND', 'NVARCHAR(1)', 'String', True),
            ('PPDM_GUID', 'NVARCHAR(38)', 'String', True),
            ('REMARK', 'NVARCHAR(2000)', 'String', True),
            ('SOURCE', 'NVARCHAR(40)', 'String', True),
            ('ROW_CHANGED_BY', 'NVARCHAR(30)', 'String', True),
            ('ROW_CHANGED_DATE', 'DATE', 'DateTime?', True),
            ('ROW_CREATED_BY', 'NVARCHAR(30)', 'String', True),
            ('ROW_CREATED_DATE', 'DATE', 'DateTime?', True),
            ('ROW_EFFECTIVE_DATE', 'DATE', 'DateTime?', True),
            ('ROW_EXPIRY_DATE', 'DATE', 'DateTime?', True),
            ('ROW_ID', 'NVARCHAR(40)', 'String', False),
        ]
    },
    'JOINT_INTEREST_BILL': {
        'fields': [
            ('JIB_ID', 'NVARCHAR(40)', 'String', False),
            ('INTEREST_SET_ID', 'NVARCHAR(40)', 'String', True),
            ('FIELD_ID', 'NVARCHAR(40)', 'String', False),
            ('OPERATOR_ID', 'NVARCHAR(40)', 'String', True),
            ('BILL_PERIOD_START_DATE', 'DATE', 'DateTime', False),
            ('BILL_PERIOD_END_DATE', 'DATE', 'DateTime', False),
            ('TOTAL_BILL_AMOUNT', 'NUMERIC(18,2)', 'Decimal', False),
            ('WORKING_INTEREST_SHARE', 'NUMERIC(10,4)', 'Decimal?', True),
            ('NET_AMOUNT_DUE', 'NUMERIC(18,2)', 'Decimal?', True),
            ('BILL_STATUS', 'NVARCHAR(40)', 'String', True),
            ('DESCRIPTION', 'NVARCHAR(2000)', 'String', True),
            ('ACTIVE_IND', 'NVARCHAR(1)', 'String', True),
            ('PPDM_GUID', 'NVARCHAR(38)', 'String', True),
            ('REMARK', 'NVARCHAR(2000)', 'String', True),
            ('SOURCE', 'NVARCHAR(40)', 'String', True),
            ('ROW_CHANGED_BY', 'NVARCHAR(30)', 'String', True),
            ('ROW_CHANGED_DATE', 'DATE', 'DateTime?', True),
            ('ROW_CREATED_BY', 'NVARCHAR(30)', 'String', True),
            ('ROW_CREATED_DATE', 'DATE', 'DateTime?', True),
            ('ROW_EFFECTIVE_DATE', 'DATE', 'DateTime?', True),
            ('ROW_EXPIRY_DATE', 'DATE', 'DateTime?', True),
            ('ROW_ID', 'NVARCHAR(40)', 'String', False),
        ]
    },
    'JIB_COST_ALLOCATION': {
        'fields': [
            ('JIB_COST_ALLOCATION_ID', 'NVARCHAR(40)', 'String', False),
            ('JIB_ID', 'NVARCHAR(40)', 'String', False),
            ('FINANCE_ID', 'NVARCHAR(40)', 'String', True),
            ('COST_CATEGORY', 'NVARCHAR(40)', 'String', True),
            ('GROSS_COST', 'NUMERIC(18,2)', 'Decimal', False),
            ('WORKING_INTEREST_SHARE', 'NUMERIC(10,4)', 'Decimal?', True),
            ('NET_COST', 'NUMERIC(18,2)', 'Decimal', False),
            ('DESCRIPTION', 'NVARCHAR(2000)', 'String', True),
            ('ACTIVE_IND', 'NVARCHAR(1)', 'String', True),
            ('PPDM_GUID', 'NVARCHAR(38)', 'String', True),
            ('REMARK', 'NVARCHAR(2000)', 'String', True),
            ('SOURCE', 'NVARCHAR(40)', 'String', True),
            ('ROW_CHANGED_BY', 'NVARCHAR(30)', 'String', True),
            ('ROW_CHANGED_DATE', 'DATE', 'DateTime?', True),
            ('ROW_CREATED_BY', 'NVARCHAR(30)', 'String', True),
            ('ROW_CREATED_DATE', 'DATE', 'DateTime?', True),
            ('ROW_EFFECTIVE_DATE', 'DATE', 'DateTime?', True),
            ('ROW_EXPIRY_DATE', 'DATE', 'DateTime?', True),
            ('ROW_ID', 'NVARCHAR(40)', 'String', False),
        ]
    },
}

def generate_property(field_name, csharp_type, is_nullable):
    """Generate a C# property"""
    var_name = f"{field_name}Value"
    nullable_marker = "?" if is_nullable and ("DateTime" in csharp_type or "Decimal" in csharp_type) else ""
    csharp_type_with_nullable = csharp_type + nullable_marker if nullable_marker else csharp_type
    
    return f"""        private {csharp_type_with_nullable} {var_name};
        public {csharp_type_with_nullable} {field_name}
        {{
            get {{ return this.{var_name}; }}
            set {{ SetProperty(ref {var_name}, value); }}
        }}"""

def generate_model(table_name, fields):
    """Generate C# model class"""
    class_name = table_name
    properties = "\n\n".join([generate_property(field[0], field[2], field[3]) for field in fields])
    
    return f"""using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.PPDM39.Models
{{
    public partial class {class_name} : Entity
    {{
{properties}
    }}
}}"""

def main():
    """Generate all model files"""
    models_dir = Path(__file__).parent.parent.parent / "Beep.OilandGas.PPDM.Models" / "39"
    
    for table_name, table_info in tables.items():
        model_content = generate_model(table_name, table_info['fields'])
        model_file = models_dir / f"{table_name}.cs"
        
        with open(model_file, 'w', encoding='utf-8') as f:
            f.write(model_content)
        
        print(f"Generated: {model_file.name}")

if __name__ == '__main__':
    main()

