#!/usr/bin/env python3
"""
Script to reorganize root-level entity classes into project-specific subfolders
and update namespaces accordingly
"""

import os
import shutil
from pathlib import Path
from typing import Dict, List

# Entity to Project mapping based on plan
ENTITY_TO_PROJECT = {
    # ProductionAccounting
    'ACCOUNTING_AMORTIZATION': 'ProductionAccounting',
    'ACCOUNTING_COST': 'ProductionAccounting',
    'ACCOUNTING_METHOD': 'ProductionAccounting',
    'AMORTIZATION_RECORD': 'ProductionAccounting',
    'IMPAIRMENT_RECORD': 'ProductionAccounting',
    'CEILING_TEST_CALCULATION': 'ProductionAccounting',
    'GL_ACCOUNT': 'ProductionAccounting',
    'GL_ENTRY': 'ProductionAccounting',
    'JOURNAL_ENTRY': 'ProductionAccounting',
    'JOURNAL_ENTRY_LINE': 'ProductionAccounting',
    'INVOICE': 'ProductionAccounting',
    'INVOICE_LINE_ITEM': 'ProductionAccounting',
    'INVOICE_PAYMENT': 'ProductionAccounting',
    'PURCHASE_ORDER': 'ProductionAccounting',
    'PO_LINE_ITEM': 'ProductionAccounting',
    'PO_RECEIPT': 'ProductionAccounting',
    'AP_INVOICE': 'ProductionAccounting',
    'AP_PAYMENT': 'ProductionAccounting',
    'AP_CREDIT_MEMO': 'ProductionAccounting',
    'AR_INVOICE': 'ProductionAccounting',
    'AR_PAYMENT': 'ProductionAccounting',
    'AR_CREDIT_MEMO': 'ProductionAccounting',
    'INVENTORY_ITEM': 'ProductionAccounting',
    'INVENTORY_TRANSACTION': 'ProductionAccounting',
    'INVENTORY_ADJUSTMENT': 'ProductionAccounting',
    'INVENTORY_VALUATION': 'ProductionAccounting',
    'REVENUE_TRANSACTION': 'ProductionAccounting',
    'REVENUE_ALLOCATION': 'ProductionAccounting',
    'REVENUE_DEDUCTION': 'ProductionAccounting',
    'REVENUE_DISTRIBUTION': 'ProductionAccounting',
    'COST_TRANSACTION': 'ProductionAccounting',
    'COST_ALLOCATION': 'ProductionAccounting',
    'COST_CENTER': 'ProductionAccounting',
    'AFE': 'ProductionAccounting',
    'AFE_LINE_ITEM': 'ProductionAccounting',
    'ROYALTY_OWNER': 'ProductionAccounting',
    'ROYALTY_PAYMENT_DETAIL': 'ProductionAccounting',
    'TAX_TRANSACTION': 'ProductionAccounting',
    'TAX_RETURN': 'ProductionAccounting',
    'JOINT_OPERATING_AGREEMENT': 'ProductionAccounting',
    'JOA_INTEREST': 'ProductionAccounting',
    'JOINT_INTEREST_BILL': 'ProductionAccounting',
    'JOIB_LINE_ITEM': 'ProductionAccounting',
    'JOIB_ALLOCATION': 'ProductionAccounting',
    'JIB_COST_ALLOCATION': 'ProductionAccounting',
    'PRODUCTION_ALLOCATION': 'ProductionAccounting',
    'ALLOCATION_DETAIL': 'ProductionAccounting',
    'ALLOCATION_RESULT': 'ProductionAccounting',
    'RUN_TICKET': 'ProductionAccounting',
    'TANK_INVENTORY': 'ProductionAccounting',
    'CRUDE_OIL_INVENTORY': 'ProductionAccounting',
    'CRUDE_OIL_INVENTORY_TRANSACTION': 'ProductionAccounting',
    'DEPLETION_CALCULATION': 'ProductionAccounting',
    'ASSET_RETIREMENT_OBLIGATION': 'ProductionAccounting',
    'SALES_CONTRACT': 'ProductionAccounting',
    'PRICE_INDEX': 'ProductionAccounting',
    'RECEIVABLE': 'ProductionAccounting',
    
    # ProspectIdentification
    'PROSPECT': 'ProspectIdentification',
    'PROSPECT_ANALOG': 'ProspectIdentification',
    'PROSPECT_BA': 'ProspectIdentification',
    'PROSPECT_DISCOVERY': 'ProspectIdentification',
    'PROSPECT_ECONOMIC': 'ProspectIdentification',
    'PROSPECT_FIELD': 'ProspectIdentification',
    'PROSPECT_HISTORY': 'ProspectIdentification',
    'PROSPECT_MIGRATION': 'ProspectIdentification',
    'PROSPECT_PLAY': 'ProspectIdentification',
    'PROSPECT_PORTFOLIO': 'ProspectIdentification',
    'PROSPECT_RANKING': 'ProspectIdentification',
    'PROSPECT_RESERVOIR': 'ProspectIdentification',
    'PROSPECT_RISK_ASSESSMENT': 'ProspectIdentification',
    'PROSPECT_SEIS_SURVEY': 'ProspectIdentification',
    'PROSPECT_SOURCE_ROCK': 'ProspectIdentification',
    'PROSPECT_TRAP': 'ProspectIdentification',
    'PROSPECT_VOLUME_ESTIMATE': 'ProspectIdentification',
    'PROSPECT_WELL': 'ProspectIdentification',
    'PROSPECT_WORKFLOW_STAGE': 'ProspectIdentification',
    'EXPLORATION_BUDGET': 'ProspectIdentification',
    'EXPLORATION_COSTS': 'ProspectIdentification',
    'EXPLORATION_PERMIT': 'ProspectIdentification',
    'EXPLORATION_PROGRAM': 'ProspectIdentification',
    'LEAD': 'ProspectIdentification',
    'PLAY': 'ProspectIdentification',
    'R_LEAD_STATUS': 'ProspectIdentification',
    'R_PLAY_TYPE': 'ProspectIdentification',
    
    # PipelineAnalysis
    'PIPELINE': 'PipelineAnalysis',
    'PIPELINE_ANALYSIS_RESULT': 'PipelineAnalysis',
    'PIPELINE_ANOMALY': 'PipelineAnalysis',
    'PIPELINE_COMPONENT': 'PipelineAnalysis',
    'PIPELINE_CONSTRUCTION': 'PipelineAnalysis',
    'PIPELINE_DESIGN': 'PipelineAnalysis',
    'PIPELINE_FACILITY': 'PipelineAnalysis',
    'PIPELINE_FIELD': 'PipelineAnalysis',
    'PIPELINE_INCIDENT': 'PipelineAnalysis',
    'PIPELINE_INSPECTION': 'PipelineAnalysis',
    'PIPELINE_MAINTENANCE': 'PipelineAnalysis',
    'PIPELINE_MAINTENANCE_SCHEDULE': 'PipelineAnalysis',
    'PIPELINE_MATERIAL': 'PipelineAnalysis',
    'PIPELINE_OPERATION': 'PipelineAnalysis',
    'PIPELINE_REPAIR': 'PipelineAnalysis',
    'PIPELINE_RISK_ASSESSMENT': 'PipelineAnalysis',
    'PIPELINE_ROUTE': 'PipelineAnalysis',
    'PIPELINE_SEGMENT': 'PipelineAnalysis',
    'PIPELINE_STATION': 'PipelineAnalysis',
    'PIPELINE_TEST': 'PipelineAnalysis',
    'PIPELINE_WELL': 'PipelineAnalysis',
    'R_PIPELINE_TYPE': 'PipelineAnalysis',
    'R_PIPELINE_STATUS': 'PipelineAnalysis',
    
    # GasLift
    'GAS_LIFT_DESIGN': 'GasLift',
    'GAS_LIFT_PERFORMANCE': 'GasLift',
    
    # EconomicAnalysis
    'ECONOMIC_ANALYSIS_RESULT': 'EconomicAnalysis',
    
    # NodalAnalysis
    'NODAL_ANALYSIS_RESULT': 'NodalAnalysis',
    
    # ProductionForecasting
    'PRODUCTION_FORECAST': 'ProductionForecasting',
    'PRODUCTION_FORECAST_POINT': 'ProductionForecasting',
    
    # FlashCalculations
    'FLASH_CALCULATION_RESULT': 'FlashCalculations',
    
    # HeatMap
    'HEAT_MAP_CONFIGURATION': 'HeatMap',
    
    # Decommissioning
    'ABANDONMENT_STATUS': 'Decommissioning',
    'DECOMMISSIONING_STATUS': 'Decommissioning',
    
    # DevelopmentPlanning
    'DEVELOPMENT_COSTS': 'DevelopmentPlanning',
    
    # ProductionOperations
    'PRODUCTION_COSTS': 'ProductionOperations',
    
    # LifeCycle
    'FIELD_PHASE': 'LifeCycle',
    
    # Common (shared entities)
    'LIST_OF_VALUE': 'Common',
    'PROVED_PROPERTY': 'Common',
    'UNPROVED_PROPERTY': 'Common',
    'R_RISK_LEVEL': 'Common',
    'R_WORKFLOW_STAGE': 'Common',
    'RESERVOIR_STATUS': 'Common',
    'GAS_COMPOSITION': 'Common',
    'GAS_COMPOSITION_COMPONENT': 'Common',
    'OIL_COMPOSITION': 'Common',
    'OIL_PROPERTY_RESULT': 'Common',
}

def get_project_folder(entity_name: str) -> str:
    """Get the project folder for an entity"""
    return ENTITY_TO_PROJECT.get(entity_name, 'Common')

def update_namespace_in_file(file_path: Path, new_namespace: str) -> bool:
    """Update namespace in a C# file"""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        # Find and replace namespace
        import re
        # Match: namespace Beep.OilandGas.Models.Data
        old_pattern = r'namespace\s+Beep\.OilandGas\.Models\.Data\s*'
        new_content = re.sub(old_pattern, f'namespace Beep.OilandGas.Models.Data.{new_namespace}\n', content)
        
        if new_content != content:
            with open(file_path, 'w', encoding='utf-8') as f:
                f.write(new_content)
            return True
        return False
    except Exception as e:
        print(f"  Error updating namespace in {file_path.name}: {e}")
        return False

def main():
    """Main function to reorganize entities"""
    script_dir = Path(__file__).parent
    data_dir = script_dir.parent / "Data"
    
    if not data_dir.exists():
        print(f"Error: Data directory not found: {data_dir}")
        return
    
    print("Reorganizing entities by project...")
    print(f"Data directory: {data_dir}\n")
    
    moved_count = 0
    skipped_count = 0
    error_count = 0
    
    # Get all root-level .cs files
    root_files = [f for f in data_dir.iterdir() 
                  if f.is_file() and f.suffix == '.cs' 
                  and f.name not in ['OwnershipTree.cs', 'SalesStatement.cs', 'SalesTransaction.cs', 'ExchangeStatement.cs']]
    
    print(f"Found {len(root_files)} root-level entity files\n")
    
    for entity_file in root_files:
        entity_name = entity_file.stem
        project_folder = get_project_folder(entity_name)
        
        if project_folder == 'Common' and entity_name not in ENTITY_TO_PROJECT:
            # Skip if not in mapping and not explicitly marked as Common
            print(f"  Skipping {entity_name} (not in mapping)")
            skipped_count += 1
            continue
        
        # Create target folder
        target_dir = data_dir / project_folder
        target_dir.mkdir(exist_ok=True)
        
        # Target file path
        target_file = target_dir / entity_file.name
        
        if target_file.exists():
            print(f"  Skipping {entity_name} (already exists in {project_folder}/)")
            skipped_count += 1
            continue
        
        try:
            # Move file
            shutil.move(str(entity_file), str(target_file))
            
            # Update namespace
            if update_namespace_in_file(target_file, project_folder):
                print(f"  Moved {entity_name} -> {project_folder}/ (namespace updated)")
            else:
                print(f"  Moved {entity_name} -> {project_folder}/ (namespace already correct)")
            
            moved_count += 1
        except Exception as e:
            print(f"  Error moving {entity_name}: {e}")
            error_count += 1
    
    print(f"\nReorganization complete!")
    print(f"  Moved: {moved_count} files")
    print(f"  Skipped: {skipped_count} files")
    print(f"  Errors: {error_count} files")

if __name__ == "__main__":
    main()
