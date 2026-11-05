param([Parameter(Mandatory)][string]$ModelsAssemblyPath,[string]$Filter='',[switch]$IncludeMaterialUsage,[switch]$IncludeWorkOrderBudget,[switch]$IncludeMaintenanceWellEvidence)
. (Join-Path $PSScriptRoot 'RestoredOwnerFixtureReferences.ps1')
$ErrorActionPreference='Stop'
$repositoryRoot=Split-Path $PSScriptRoot -Parent
$checkDirectory=Join-Path ([IO.Path]::GetTempPath()) ('oilgas-maintenance-start-'+[guid]::NewGuid().ToString('N'))
[void](New-Item -ItemType Directory -Path $checkDirectory)
# Actual native process/repository/transaction/router sources with the existing explicit Beep provider ledger.
# No business EF/SQL, production-source API, live provider, browser or OIDC certification.
[xml]$project='<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings><DefineConstants>WORKFLOW_INBOX_FOCUSED</DefineConstants><NoWarn>$(NoWarn);CS0436</NoWarn></PropertyGroup><ItemGroup><FrameworkReference Include="Microsoft.AspNetCore.App"/><PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.4.0"/><PackageReference Include="xunit" Version="2.9.3"/><PackageReference Include="xunit.runner.visualstudio" Version="3.1.5"/><PackageReference Include="Moq" Version="4.20.72"/></ItemGroup><ItemGroup/></Project>'
$group=$project.SelectNodes('/Project/ItemGroup')[1]
$startFixture=$project.CreateElement('Compile');$startFixture.SetAttribute('Include',(Join-Path $repositoryRoot 'Beep.OilandGas.ApiService.Tests/WorkOrderExecutionStartFixture.cs'));[void]$group.AppendChild($startFixture)
$sources=@('Beep.OilandGas.LifeCycle/Services/Processes/ProcessServiceBase.cs','Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.cs','Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.HseStart.cs','Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.InvestmentHandoff.cs','Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.InvestmentStage.cs','Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.InvestmentProgress.cs','Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.ReviewStagePersistence.cs','Beep.OilandGas.LifeCycle/Services/Processes/DevelopmentReviewDefinition.cs','Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.StartPersistence.cs','Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.CompanyStart.cs','Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.CompanyStep.cs','Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.MaintenanceStart.cs','Beep.OilandGas.LifeCycle/Services/Processes/CrossPersonaTaskRouter.cs','Beep.OilandGas.LifeCycle/Services/Processes/ICrossPersonaTaskRouter.cs','Beep.OilandGas.LifeCycle/Services/Processes/IRolePersonaReader.cs','Beep.OilandGas.LifeCycle/Services/Processes/DynamicRoutingService.cs','Beep.OilandGas.LifeCycle/Services/Processes/ProcessValidator.cs','Beep.OilandGas.PPDM39/Core/PPDMGenericRepository.cs','Beep.OilandGas.PPDM39/Core/IPPDMGenericRepository.cs','Beep.OilandGas.PPDM39/Core/PPDMDataSourceTransaction.cs','Beep.OilandGas.PPDM39/Core/BeepOperationResult.cs','Beep.OilandGas.ApiService.Tests/PersonaTaskRoutingTests.cs','Beep.OilandGas.ApiService.Tests/CompanyProcessStartTests.cs','Beep.OilandGas.ApiService.Tests/NativeProcessStartTests.cs','Beep.OilandGas.ApiService.Tests/CompanyMaintenanceProcessStartTests.cs')
foreach($table in @('PROCESS_DEFINITION','PROCESS_INSTANCE','PROCESS_STEP_INSTANCE','PROCESS_HISTORY','PROCESS_APPROVAL','CROSS_PERSONA_TASK')){$sources+=('Beep.OilandGas.LifeCycle/Data/Tables/'+$table+'.cs')}
$sources+='Beep.OilandGas.ApiService/Services/ProcessStepRoleChecks.cs'
$sources+='Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.MaintenanceAcceptance.cs'
$sources+='Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.MaintenanceAssignedStage.cs'
$sources+='Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.MaintenanceExecutionHandoff.cs'
$sources+='Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.CompletionDecision.cs'
$sources+='Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.MaintenanceReturnReview.cs'
$sources+='Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.MaintenanceReturnHandoff.cs'
$sources+='Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.ReturnAcceptance.cs'
$sources+='Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.MaintenanceClosure.cs'
$sources+='Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.MaintenanceAssetReturnSource.cs'
$sources+='Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.MaintenanceGraphDelta.cs'
$sources+='Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.MaintenanceOwnedAudit.cs'
$sources+='Beep.OilandGas.PPDM39/Core/CommonColumnHandler.cs'
$sources+='Beep.OilandGas.ApiService.Tests/CompanyMaintenanceAssignedStageTests.cs'
$sources+='Beep.OilandGas.ApiService.Tests/CompanyMaintenanceExecutionHandoffTests.cs'
$sources+='Beep.OilandGas.ApiService.Tests/CompanyWorkOrderCompletionDecisionTests.cs'
$sources+='Beep.OilandGas.ApiService.Tests/CompanyWorkOrderCompletionDecisionStatusTests.cs'
$sources+='Beep.OilandGas.ApiService.Tests/CompanyMaintenanceReturnReviewTests.cs'
$sources+='Beep.OilandGas.ApiService.Tests/CompanyMaintenanceReturnHandoffTests.cs'
$sources+='Beep.OilandGas.ApiService.Tests/CompanyWorkOrderReturnAcceptanceTests.cs'
$sources+='Beep.OilandGas.ApiService.Tests/CompanyMaintenanceClosureTests.cs'
$sources+='Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/WorkOrderExecutionPermitService.cs'
$sources+='Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/WorkOrderExecutionPermitService.Decision.cs'
$sources+='Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/WorkOrderExecutionPermitService.RecordedApproval.cs'
$sources+='Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/WorkOrderExecutionPermitService.ExecutionPrerequisites.cs'
$sources+='Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/WorkOrderExecutionPermitService.Start.cs'
$sources+='Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/WorkOrderExecutionPermitService.SubmissionInspections.cs'
$sources+='Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/WorkOrderExecutionPermitService.Submission.cs'
$sources+='Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/WorkOrderExecutionPermitService.Labor.cs'
$sources+='Beep.OilandGas.ApiService.Tests/WorkOrderExecutionPermitTests.cs'
$sources+='Beep.OilandGas.ApiService.Tests/WorkOrderPermitPrerequisiteInspectionTests.cs'
$sources+='Beep.OilandGas.ApiService.Tests/WorkOrderMaterialAcceptanceFixture.cs'
$sources+='Beep.OilandGas.ApiService.Tests/WorkOrderCompleteApprovalFixture.cs'
$sources+='Beep.OilandGas.ApiService.Tests/WorkOrderMaterialAcceptanceTests.cs'
$sources+='Beep.OilandGas.ApiService.Tests/WorkOrderPersonnelSourceTests.cs'
$sources+='Beep.OilandGas.ApiService.Tests/WorkOrderPermitDocumentSourceTests.cs'
$sources+='Beep.OilandGas.ApiService.Tests/WorkOrderPersonnelAssignmentTests.cs'
$sources+='Beep.OilandGas.ApiService.Tests/WorkOrderPersonnelRosterTests.cs'
$sources+='Beep.OilandGas.ApiService.Tests/WorkOrderPersonnelReleaseTests.cs'
$sources+='Beep.OilandGas.ApiService.Tests/CompanyPersonnelPolicyTests.cs'
$sources+='Beep.OilandGas.ApiService.Tests/CompanyPersonnelIdentityTests.cs'
$sources+='Beep.OilandGas.ApiService.Tests/CompanyPersonnelCredentialTests.cs'
$sources+='Beep.OilandGas.ApiService.Tests/WorkOrderPersonnelReviewFixture.cs'
$sources+='Beep.OilandGas.ApiService.Tests/WorkOrderPersonnelAcceptanceFixture.cs'
$sources+='Beep.OilandGas.ApiService.Tests/WorkOrderPersonnelAcceptanceTests.cs'
$sources+='Beep.OilandGas.ApiService.Tests/WorkOrderExecutionPersonnelTests.cs'
$sources+='Beep.OilandGas.ApiService.Tests/WorkOrderExecutionStartTests.cs'
$sources+='Beep.OilandGas.ApiService.Tests/WorkOrderLaborUsageTests.cs'
$sources+='Beep.OilandGas.ApiService.Tests/WorkOrderSubmissionTests.cs'
$sources+='Beep.OilandGas.ApiService.Tests/WorkOrderExecutorIdentityTests.cs'
$sources+='Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/InspectionService.DocumentSource.cs'
$sources+='Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/InspectionService.DocumentContributors.cs'
$sources+='Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/ContractorManagementService.PersonnelIdentity.cs'
$sources+='Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/ContractorManagementService.ExecutorIdentity.cs'
$sources+='Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/WorkOrderRowValues.cs'

$sources+='Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/WorkOrderDatabaseBinding.cs'
$sources+='Beep.OilandGas.Models/Core/Refusals/RefusalException.cs'
$sources+='Beep.OilandGas.Models/Core/Refusals/RefusalKind.cs'
$sources+='Beep.OilandGas.ApiService.Tests/CompanyMaintenanceAcceptanceTests.cs'
$sources+='Beep.OilandGas.ApiService/Services/CompanyProcessReadService.cs'
# Import-only copies select the canonical Data contracts while retaining the actual native implementation.
foreach($entry in @(@('Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/InspectionService.cs','InspectionService.cs'),@('Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/InspectionService.MaterialAcceptance.cs','InspectionService.MaterialAcceptance.cs'),@('Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/InspectionService.PersonnelAcceptance.cs','InspectionService.PersonnelAcceptance.cs'),@('Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/InspectionService.DocumentAcceptance.cs','InspectionService.DocumentAcceptance.cs'),@('Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/ContractorManagementService.cs','ContractorManagementService.cs'),@('Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/ContractorManagementService.PersonnelSource.cs','ContractorManagementService.PersonnelSource.cs'),@('Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/ContractorManagementService.PersonnelAssignment.cs','ContractorManagementService.PersonnelAssignment.cs'),@('Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/ContractorManagementService.PersonnelRelease.cs','ContractorManagementService.PersonnelRelease.cs'),@('Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/ContractorManagementService.PersonnelPolicy.cs','ContractorManagementService.PersonnelPolicy.cs'),@('Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/ContractorManagementService.PersonnelCredentials.cs','ContractorManagementService.PersonnelCredentials.cs'),@('Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/ContractorManagementService.PersonnelAcceptanceCore.cs','ContractorManagementService.PersonnelAcceptanceCore.cs'),@('Beep.OilandGas.Models/Core/Interfaces/IContractorManagementService.cs','IContractorManagementService.cs'),@('Beep.OilandGas.Models/Core/Interfaces/IInspectionService.cs','IInspectionService.cs'))){[IO.File]::WriteAllText((Join-Path $checkDirectory $entry[1]),[IO.File]::ReadAllText((Join-Path $repositoryRoot $entry[0])).Replace('using Beep.OilandGas.Models.Data.WorkOrder;',''),[Text.UTF8Encoding]::new($false))}
if($IncludeMaterialUsage){
    $sources+=@('Beep.OilandGas.Accounting/Services/InventoryService.NativeContext.cs','Beep.OilandGas.Accounting/Services/InventoryService.CompanySources.cs',
        'Beep.OilandGas.Accounting/Services/InventoryService.WorkOrderMaterials.cs','Beep.OilandGas.Accounting/Services/InventoryService.WorkOrderMaterialRelease.cs',
        'Beep.OilandGas.Accounting/Services/InventoryService.WorkOrderMaterialUsage.cs','Beep.OilandGas.ApiService.Tests/WorkOrderMaterialUsageTests.cs')
    $sources+='Beep.OilandGas.PPDM39.DataManagement/Core/Metadata/WorkOrderMaterialMetadata.cs'
}
if($IncludeWorkOrderBudget){
    $project.Project.PropertyGroup.DefineConstants+=";WORK_ORDER_BUDGET_FOCUSED"
    $sources+=@('Beep.OilandGas.ProductionAccounting/Services/AfeService.cs','Beep.OilandGas.ProductionAccounting/Services/AuthorizationWorkflowService.cs','Beep.OilandGas.ProductionAccounting/Services/AuthorizationWorkflowService.WorkOrderBudget.cs','Beep.OilandGas.ProductionAccounting/Services/AfeService.WorkOrderBudget.cs','Beep.OilandGas.ProductionAccounting/Services/AfeService.WorkOrderLaborCost.cs','Beep.OilandGas.ProductionAccounting/Services/AfeService.WorkOrderMaterialCost.cs',
        'Beep.OilandGas.ProductionAccounting/Core/Interfaces/IAfeService.cs','Beep.OilandGas.ProductionAccounting/Core/Interfaces/IAuthorizationWorkflowService.cs',
        'Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/COST_VARIANCE_REPORT.cs','Beep.OilandGas.ProductionAccounting/Exceptions/ProductionAccountingException.cs',
        'Beep.OilandGas.ProductionAccounting/Constants/AfeStatusCodes.cs','Beep.OilandGas.ProductionAccounting/Constants/DocumentWorkflowStatusCodes.cs',
        'Beep.OilandGas.ProductionAccounting/Constants/ApprovalWorkflowStatusCodes.cs','Beep.OilandGas.ProductionAccounting/Constants/WorkflowReferenceCodes.cs',
        'Beep.OilandGas.ApiService.Tests/WorkOrderBudgetTests.cs','Beep.OilandGas.ApiService.Tests/WorkOrderBudgetDecisionTests.cs','Beep.OilandGas.ApiService.Tests/WorkOrderLaborCostTests.cs','Beep.OilandGas.PPDM39.DataManagement/Core/Metadata/WorkOrderFinancialMetadata.cs','Beep.OilandGas.Accounting/Services/GLAccountService.cs','Beep.OilandGas.Accounting/Services/GLAccountService.CompanyBooks.cs','Beep.OilandGas.Accounting/Services/JournalEntryService.CompanyAdmission.cs')
}
if($IncludeMaintenanceWellEvidence){
    $sources+=@('Beep.OilandGas.ApiService.Tests/CompanyMaintenanceWellEvidenceTests.cs','Beep.OilandGas.ApiService.Tests/CompanyMaintenanceAssetReturnTests.cs','Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/RecordedWorkOrderRows.cs')
    $sources+=@(Get-ChildItem -LiteralPath (Join-Path $repositoryRoot 'Beep.OilandGas.PPDM39.DataManagement/Services/Well') -Filter 'WellServices*.cs' | ForEach-Object { [IO.Path]::GetRelativePath($repositoryRoot,$_.FullName) })
}
$sources+=@('Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/WorkOrderExecutionPermitService.RecordedWork.cs','Beep.OilandGas.ApiService.Tests/CompanyMaintenanceRecordedWorkTests.cs')
foreach($source in $sources){$node=$project.CreateElement('Compile');$node.SetAttribute('Include',(Join-Path $repositoryRoot $source));[void]$group.AppendChild($node)}
$dependencies=Join-Path $repositoryRoot 'Beep.OilandGas.Repository.Tests/bin/company-focused/Debug/net10.0'
$dependencyNames=@('Beep.OilandGas.Models','Beep.OilandGas.Models','Beep.OilandGas.PPDM.Models','Beep.OilandGas.PPDM39','DataManagementModels','DataManagementEngine')
if($IncludeMaterialUsage){$dependencyNames+=@('Beep.OilandGas.Accounting','Beep.OilandGas.PPDM39.DataManagement')}
if($IncludeMaintenanceWellEvidence -and -not $IncludeMaterialUsage){$dependencyNames+='Beep.OilandGas.PPDM39.DataManagement'}
if($IncludeWorkOrderBudget -and -not $IncludeMaterialUsage){$dependencyNames+='Beep.OilandGas.Accounting'}
foreach($name in $dependencyNames){
    $path=if($name -eq 'Beep.OilandGas.Models'){(Resolve-Path -LiteralPath $ModelsAssemblyPath).Path}else{Join-Path $dependencies ($name+'.dll')}
    if(-not(Test-Path -LiteralPath $path)){throw "Actual dependency missing: $name"}
    $node=$project.CreateElement('Reference');$node.SetAttribute('Include',$name);$hint=$project.CreateElement('HintPath');$hint.InnerText=$path;[void]$node.AppendChild($hint)
    if($name -in @('Beep.OilandGas.Models','Beep.OilandGas.Models','Beep.OilandGas.Accounting')){$alias=$project.CreateElement('Aliases');$alias.InnerText=if($name -eq 'Beep.OilandGas.Models'){'global,actualOwners'}elseif($name -eq 'Beep.OilandGas.Accounting'){'global,cachedAccounting'}else{'global,actualModels'};[void]$node.AppendChild($alias)}
    [void]$group.AppendChild($node)
}
@'
extern alias actualOwners;
extern alias actualModels;
global using InspectionCondition = actualOwners::Beep.OilandGas.Models.Data.WorkOrder.InspectionCondition;
global using WorkOrderDetailModel = actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderDetailModel;
global using WorkOrderSummary = actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderSummary;
global using WorkOrderComponentReference = actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderComponentReference;
global using AddWorkOrderInspectionRequirementRequest = actualOwners::Beep.OilandGas.Models.Data.WorkOrder.AddWorkOrderInspectionRequirementRequest;
global using RecordInspectionResultRequest = actualOwners::Beep.OilandGas.Models.Data.WorkOrder.RecordInspectionResultRequest;
global using WorkOrderSubType = actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderSubType;
global using ContractorAssignment = actualOwners::Beep.OilandGas.Models.Data.WorkOrder.ContractorAssignment;
global using ContractorQualificationResult = actualOwners::Beep.OilandGas.Models.Data.WorkOrder.ContractorQualificationResult;
global using WORK_ORDER_INSPECTION = actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION;
global using WORK_ORDER_EXECUTION_PERMIT = actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_EXECUTION_PERMIT;
global using WorkOrderWorkflow = actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderWorkflow;
global using WorkOrderInspectionRules = actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules;
global using ModelEntityBase = actualOwners::Beep.OilandGas.Models.Data.ModelEntityBase;
global using AFE = actualOwners::Beep.OilandGas.Models.Data.ProductionAccounting.AFE;
global using APPROVAL_WORKFLOW = actualOwners::Beep.OilandGas.Models.Data.ProductionAccounting.APPROVAL_WORKFLOW;
global using ACCOUNTING_COST = actualOwners::Beep.OilandGas.Models.Data.ProductionAccounting.ACCOUNTING_COST;
global using GL_ACCOUNT = actualOwners::Beep.OilandGas.Models.Data.ProductionAccounting.GL_ACCOUNT;
global using GL_ENTRY = actualOwners::Beep.OilandGas.Models.Data.ProductionAccounting.GL_ENTRY;
global using JOURNAL_ENTRY = actualOwners::Beep.OilandGas.Models.Data.ProductionAccounting.JOURNAL_ENTRY;
global using JOURNAL_ENTRY_LINE = actualOwners::Beep.OilandGas.Models.Data.ProductionAccounting.JOURNAL_ENTRY_LINE;
global using INVENTORY_ITEM = actualOwners::Beep.OilandGas.Models.Data.ProductionAccounting.INVENTORY_ITEM;
global using INVENTORY_TRANSACTION = actualOwners::Beep.OilandGas.Models.Data.ProductionAccounting.INVENTORY_TRANSACTION;
global using ICommonColumnHandler = actualModels::Beep.OilandGas.Models.Core.Interfaces.ICommonColumnHandler;
global using Beep.OilandGas.Models.Core.Interfaces;
global using Beep.OilandGas.PPDM.Models;
global using TheTechIdea.Beep.Report;
'@ | Set-Content (Join-Path $checkDirectory 'ActualContracts.cs') -Encoding utf8
if(-not $IncludeWorkOrderBudget){
    $actualPath=Join-Path $checkDirectory 'ActualContracts.cs';$actualText=[IO.File]::ReadAllText($actualPath)
    $actualText=[Text.RegularExpressions.Regex]::Replace($actualText,'(?m)^global using (AFE|APPROVAL_WORKFLOW|ACCOUNTING_COST|GL_ACCOUNT|GL_ENTRY|JOURNAL_ENTRY|JOURNAL_ENTRY_LINE) = actualOwners::[^\r\n]+;\r?\n?','')
    [IO.File]::WriteAllText($actualPath,$actualText,[Text.UTF8Encoding]::new($false))
}
$path=Join-Path $checkDirectory 'MaintenanceStart.csproj';Add-OilGasRestoredOwnerReferences -Project $project -RepositoryRoot (Split-Path $PSScriptRoot -Parent)
$project.Save($path)
$testArguments=@('test',$path,'--logger','console;verbosity=normal')
if(-not [string]::IsNullOrWhiteSpace($Filter)){$testArguments+=@('--filter',$Filter)}
dotnet @testArguments
if($LASTEXITCODE -ne 0){throw "Native maintenance start check failed: $checkDirectory"}
