#!/usr/bin/env python3
"""
Script to update using statements in project files to reference new entity namespaces
"""

import os
import re
from pathlib import Path
from typing import Dict

# Entity to namespace mapping
ENTITY_TO_NAMESPACE = {
    # ProductionAccounting
    'ACCOUNTING_AMORTIZATION': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'ACCOUNTING_COST': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'ACCOUNTING_METHOD': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'AMORTIZATION_RECORD': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'IMPAIRMENT_RECORD': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'CEILING_TEST_CALCULATION': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'GL_ACCOUNT': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'GL_ENTRY': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'JOURNAL_ENTRY': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'JOURNAL_ENTRY_LINE': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'INVOICE': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'INVOICE_LINE_ITEM': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'INVOICE_PAYMENT': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'PURCHASE_ORDER': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'PO_LINE_ITEM': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'PO_RECEIPT': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'AP_INVOICE': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'AP_PAYMENT': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'AP_CREDIT_MEMO': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'AR_INVOICE': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'AR_PAYMENT': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'AR_CREDIT_MEMO': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'INVENTORY_ITEM': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'INVENTORY_TRANSACTION': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'INVENTORY_ADJUSTMENT': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'INVENTORY_VALUATION': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'REVENUE_TRANSACTION': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'REVENUE_ALLOCATION': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'REVENUE_DEDUCTION': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'REVENUE_DISTRIBUTION': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'COST_TRANSACTION': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'COST_ALLOCATION': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'COST_CENTER': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'AFE': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'AFE_LINE_ITEM': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'ROYALTY_OWNER': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'ROYALTY_PAYMENT_DETAIL': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'TAX_TRANSACTION': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'TAX_RETURN': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'JOINT_OPERATING_AGREEMENT': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'JOA_INTEREST': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'JOINT_INTEREST_BILL': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'JOIB_LINE_ITEM': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'JOIB_ALLOCATION': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'JIB_COST_ALLOCATION': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'PRODUCTION_ALLOCATION': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'ALLOCATION_DETAIL': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'ALLOCATION_RESULT': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'RUN_TICKET': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'TANK_INVENTORY': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'CRUDE_OIL_INVENTORY': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'CRUDE_OIL_INVENTORY_TRANSACTION': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'DEPLETION_CALCULATION': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'ASSET_RETIREMENT_OBLIGATION': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'SALES_CONTRACT': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'PRICE_INDEX': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    'RECEIVABLE': 'Beep.OilandGas.Models.Data.ProductionAccounting',
    
    # ProspectIdentification
    'PROSPECT': 'Beep.OilandGas.Models.Data.ProspectIdentification',
    'PROSPECT_ANALOG': 'Beep.OilandGas.Models.Data.ProspectIdentification',
    'PROSPECT_BA': 'Beep.OilandGas.Models.Data.ProspectIdentification',
    'PROSPECT_DISCOVERY': 'Beep.OilandGas.Models.Data.ProspectIdentification',
    'PROSPECT_ECONOMIC': 'Beep.OilandGas.Models.Data.ProspectIdentification',
    'PROSPECT_FIELD': 'Beep.OilandGas.Models.Data.ProspectIdentification',
    'PROSPECT_HISTORY': 'Beep.OilandGas.Models.Data.ProspectIdentification',
    'PROSPECT_MIGRATION': 'Beep.OilandGas.Models.Data.ProspectIdentification',
    'PROSPECT_PLAY': 'Beep.OilandGas.Models.Data.ProspectIdentification',
    'PROSPECT_PORTFOLIO': 'Beep.OilandGas.Models.Data.ProspectIdentification',
    'PROSPECT_RANKING': 'Beep.OilandGas.Models.Data.ProspectIdentification',
    'PROSPECT_RESERVOIR': 'Beep.OilandGas.Models.Data.ProspectIdentification',
    'PROSPECT_RISK_ASSESSMENT': 'Beep.OilandGas.Models.Data.ProspectIdentification',
    'PROSPECT_SEIS_SURVEY': 'Beep.OilandGas.Models.Data.ProspectIdentification',
    'PROSPECT_SOURCE_ROCK': 'Beep.OilandGas.Models.Data.ProspectIdentification',
    'PROSPECT_TRAP': 'Beep.OilandGas.Models.Data.ProspectIdentification',
    'PROSPECT_VOLUME_ESTIMATE': 'Beep.OilandGas.Models.Data.ProspectIdentification',
    'PROSPECT_WELL': 'Beep.OilandGas.Models.Data.ProspectIdentification',
    'PROSPECT_WORKFLOW_STAGE': 'Beep.OilandGas.Models.Data.ProspectIdentification',
    'EXPLORATION_BUDGET': 'Beep.OilandGas.Models.Data.ProspectIdentification',
    'EXPLORATION_COSTS': 'Beep.OilandGas.Models.Data.ProspectIdentification',
    'EXPLORATION_PERMIT': 'Beep.OilandGas.Models.Data.ProspectIdentification',
    'EXPLORATION_PROGRAM': 'Beep.OilandGas.Models.Data.ProspectIdentification',
    'LEAD': 'Beep.OilandGas.Models.Data.ProspectIdentification',
    'PLAY': 'Beep.OilandGas.Models.Data.ProspectIdentification',
    'R_LEAD_STATUS': 'Beep.OilandGas.Models.Data.ProspectIdentification',
    'R_PLAY_TYPE': 'Beep.OilandGas.Models.Data.ProspectIdentification',
    
    # PipelineAnalysis
    'PIPELINE': 'Beep.OilandGas.Models.Data.PipelineAnalysis',
    'PIPELINE_ANALYSIS_RESULT': 'Beep.OilandGas.Models.Data.PipelineAnalysis',
    'PIPELINE_ANOMALY': 'Beep.OilandGas.Models.Data.PipelineAnalysis',
    'PIPELINE_COMPONENT': 'Beep.OilandGas.Models.Data.PipelineAnalysis',
    'PIPELINE_CONSTRUCTION': 'Beep.OilandGas.Models.Data.PipelineAnalysis',
    'PIPELINE_DESIGN': 'Beep.OilandGas.Models.Data.PipelineAnalysis',
    'PIPELINE_FACILITY': 'Beep.OilandGas.Models.Data.PipelineAnalysis',
    'PIPELINE_FIELD': 'Beep.OilandGas.Models.Data.PipelineAnalysis',
    'PIPELINE_INCIDENT': 'Beep.OilandGas.Models.Data.PipelineAnalysis',
    'PIPELINE_INSPECTION': 'Beep.OilandGas.Models.Data.PipelineAnalysis',
    'PIPELINE_MAINTENANCE': 'Beep.OilandGas.Models.Data.PipelineAnalysis',
    'PIPELINE_MAINTENANCE_SCHEDULE': 'Beep.OilandGas.Models.Data.PipelineAnalysis',
    'PIPELINE_MATERIAL': 'Beep.OilandGas.Models.Data.PipelineAnalysis',
    'PIPELINE_OPERATION': 'Beep.OilandGas.Models.Data.PipelineAnalysis',
    'PIPELINE_REPAIR': 'Beep.OilandGas.Models.Data.PipelineAnalysis',
    'PIPELINE_RISK_ASSESSMENT': 'Beep.OilandGas.Models.Data.PipelineAnalysis',
    'PIPELINE_ROUTE': 'Beep.OilandGas.Models.Data.PipelineAnalysis',
    'PIPELINE_SEGMENT': 'Beep.OilandGas.Models.Data.PipelineAnalysis',
    'PIPELINE_STATION': 'Beep.OilandGas.Models.Data.PipelineAnalysis',
    'PIPELINE_TEST': 'Beep.OilandGas.Models.Data.PipelineAnalysis',
    'PIPELINE_WELL': 'Beep.OilandGas.Models.Data.PipelineAnalysis',
    'R_PIPELINE_TYPE': 'Beep.OilandGas.Models.Data.PipelineAnalysis',
    'R_PIPELINE_STATUS': 'Beep.OilandGas.Models.Data.PipelineAnalysis',
    
    # GasLift
    'GAS_LIFT_DESIGN': 'Beep.OilandGas.Models.Data.GasLift',
    'GAS_LIFT_PERFORMANCE': 'Beep.OilandGas.Models.Data.GasLift',
    
    # EconomicAnalysis
    'ECONOMIC_ANALYSIS_RESULT': 'Beep.OilandGas.Models.Data.EconomicAnalysis',
    
    # NodalAnalysis
    'NODAL_ANALYSIS_RESULT': 'Beep.OilandGas.Models.Data.NodalAnalysis',
    
    # ProductionForecasting
    'PRODUCTION_FORECAST': 'Beep.OilandGas.Models.Data.ProductionForecasting',
    'PRODUCTION_FORECAST_POINT': 'Beep.OilandGas.Models.Data.ProductionForecasting',
    
    # FlashCalculations
    'FLASH_CALCULATION_RESULT': 'Beep.OilandGas.Models.Data.FlashCalculations',
    
    # HeatMap
    'HEAT_MAP_CONFIGURATION': 'Beep.OilandGas.Models.Data.HeatMap',
    
    # Decommissioning
    'ABANDONMENT_STATUS': 'Beep.OilandGas.Models.Data.Decommissioning',
    'DECOMMISSIONING_STATUS': 'Beep.OilandGas.Models.Data.Decommissioning',
    
    # DevelopmentPlanning
    'DEVELOPMENT_COSTS': 'Beep.OilandGas.Models.Data.DevelopmentPlanning',
    
    # ProductionOperations
    'PRODUCTION_COSTS': 'Beep.OilandGas.Models.Data.ProductionOperations',
    
    # LifeCycle
    'FIELD_PHASE': 'Beep.OilandGas.Models.Data.LifeCycle',
    
    # Common
    'LIST_OF_VALUE': 'Beep.OilandGas.Models.Data.Common',
    'PROVED_PROPERTY': 'Beep.OilandGas.Models.Data.Common',
    'UNPROVED_PROPERTY': 'Beep.OilandGas.Models.Data.Common',
    'R_RISK_LEVEL': 'Beep.OilandGas.Models.Data.Common',
    'R_WORKFLOW_STAGE': 'Beep.OilandGas.Models.Data.Common',
    'RESERVOIR_STATUS': 'Beep.OilandGas.Models.Data.Common',
    'GAS_COMPOSITION': 'Beep.OilandGas.Models.Data.Common',
    'GAS_COMPOSITION_COMPONENT': 'Beep.OilandGas.Models.Data.Common',
    'OIL_COMPOSITION': 'Beep.OilandGas.Models.Data.Common',
    'OIL_PROPERTY_RESULT': 'Beep.OilandGas.Models.Data.Common',
}

def update_file_references(file_path: Path) -> int:
    """Update entity type references in a C# file"""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        original_content = content
        updates = 0
        
        # Add using statements for entities found in the file
        using_statements = set()
        
        for entity_name, namespace in ENTITY_TO_NAMESPACE.items():
            # Check if entity is referenced in the file
            if re.search(rf'\b{entity_name}\b', content):
                using_statements.add(f'using {namespace};')
        
        # Add using statements after existing using statements
        if using_statements:
            # Find the last using statement
            using_pattern = r'(using\s+[^;]+;)'
            matches = list(re.finditer(using_pattern, content))
            
            if matches:
                last_using = matches[-1]
                insert_pos = last_using.end()
                
                # Check if using statements already exist
                existing_usings = set(re.findall(r'using\s+([^;]+);', content))
                new_usings = [us for us in using_statements if us.split()[1].rstrip(';') not in existing_usings]
                
                if new_usings:
                    new_using_block = '\n' + '\n'.join(sorted(new_usings))
                    content = content[:insert_pos] + new_using_block + content[insert_pos:]
                    updates += len(new_usings)
        
        if content != original_content:
            with open(file_path, 'w', encoding='utf-8') as f:
                f.write(content)
            return updates
        
        return 0
    except Exception as e:
        print(f"  Error updating {file_path.name}: {e}")
        return 0

def main():
    """Main function to update project references"""
    script_dir = Path(__file__).parent
    workspace_root = script_dir.parent.parent
    
    print("Updating project references...")
    print(f"Workspace root: {workspace_root}\n")
    
    # Projects to update
    projects_to_update = [
        'Beep.OilandGas.ProductionAccounting',
        'Beep.OilandGas.ProspectIdentification',
        'Beep.OilandGas.PipelineAnalysis',
        'Beep.OilandGas.GasLift',
        'Beep.OilandGas.EconomicAnalysis',
        'Beep.OilandGas.NodalAnalysis',
        'Beep.OilandGas.ProductionForecasting',
        'Beep.OilandGas.FlashCalculations',
        'Beep.OilandGas.HeatMap',
        'Beep.OilandGas.Decommissioning',
        'Beep.OilandGas.DevelopmentPlanning',
        'Beep.OilandGas.ProductionOperations',
        'Beep.OilandGas.LifeCycle',
    ]
    
    total_updates = 0
    files_updated = 0
    
    for project_name in projects_to_update:
        project_dir = workspace_root / project_name
        if not project_dir.exists():
            continue
        
        print(f"Processing {project_name}...")
        project_updates = 0
        
        # Find all .cs files in project
        for cs_file in project_dir.rglob("*.cs"):
            updates = update_file_references(cs_file)
            if updates > 0:
                project_updates += updates
                files_updated += 1
        
        if project_updates > 0:
            print(f"  Updated {project_updates} using statements in {files_updated} files")
        total_updates += project_updates
    
    print(f"\nUpdate complete!")
    print(f"  Total using statements added: {total_updates}")
    print(f"  Files updated: {files_updated}")

if __name__ == "__main__":
    main()
