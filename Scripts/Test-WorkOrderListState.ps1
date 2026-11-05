param([string]$DataAssemblyPath)
$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path $PSScriptRoot -Parent
if (-not $DataAssemblyPath) {
    $DataAssemblyPath = Join-Path $repositoryRoot 'Beep.OilandGas.Repository.Tests/bin/company-focused/Debug/net10.0/TheTechIdea.Data.dll'
}
if (-not (Test-Path -LiteralPath $DataAssemblyPath)) { throw 'Provide an actual compiled TheTechIdea.Data assembly; this check does not replace missing projects.' }
# Use the supplied canonical Data assembly for domain types; compile current state/native owners against explicit aliases.
# Compile actual state/context sources with an explicit typed-client/auth fixture. No transport, Razor, host or OIDC claim.
$checkDirectory = Join-Path ([IO.Path]::GetTempPath()) "oilgas-work-order-list-$([Guid]::NewGuid().ToString('N'))"
[void](New-Item -ItemType Directory -Path $checkDirectory)
[xml]$project = '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings><IsPackable>false</IsPackable><NoWarn>$(NoWarn);CS0436</NoWarn></PropertyGroup><ItemGroup><FrameworkReference Include="Microsoft.AspNetCore.App"/><PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.4.0"/><PackageReference Include="xunit" Version="2.9.3"/><PackageReference Include="xunit.runner.visualstudio" Version="3.1.5"/><PackageReference Include="Moq" Version="4.20.72"/></ItemGroup><ItemGroup/></Project>'
$group = $project.SelectNodes("/Project/ItemGroup")[1]
foreach ($source in @('Beep.OilandGas.Web/Services/WorkOrderListState.cs', 'Beep.OilandGas.Web/Services/WorkOrderInspectionState.cs', 'Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/WorkOrderService.cs', 'Beep.OilandGas.LifeCycle/Services/AccessControl/UserAssetAccessService.cs', 'Beep.OilandGas.LifeCycle/Services/AccessControl/IApplicationAuthorizationReader.cs', 'Beep.OilandGas.LifeCycle/Services/AccessControl/IApplicationRolePermissionStore.cs', 'Beep.OilandGas.PPDM39.DataManagement/Core/ModuleSetup/MigrationConnectionTarget.cs', 'Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/InspectionService.cs','Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/InspectionService.MaterialAcceptance.cs','Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/InspectionService.PersonnelAcceptance.cs', 'Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/SchedulingService.cs', 'Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/ContractorManagementService.cs','Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/ContractorManagementService.PersonnelSource.cs','Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/ContractorManagementService.PersonnelAssignment.cs','Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/ContractorManagementService.PersonnelRelease.cs','Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/ContractorManagementService.PersonnelPolicy.cs','Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/ContractorManagementService.PersonnelCredentials.cs','Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/ContractorManagementService.PersonnelAcceptanceCore.cs', 'Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/WorkOrderRowValues.cs', 'Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/RecordedWorkOrderRows.cs', 'Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/RecordedWorkOrderCreation.cs', 'Beep.OilandGas.Models/Core/Interfaces/IWorkOrderService.cs', 'Beep.OilandGas.Models/Core/Interfaces/IInspectionService.cs', 'Beep.OilandGas.Models/Core/Interfaces/ISchedulingService.cs', 'Beep.OilandGas.Models/Core/Interfaces/IContractorManagementService.cs',  'Beep.OilandGas.Web/Services/WorkOrderDetailState.cs', 'Beep.OilandGas.Web.Auth.Tests/WorkOrderDetailStateTests.cs', 'Beep.OilandGas.Web.Auth.Tests/WorkOrderWorkflowTests.cs', 'Beep.OilandGas.Web.Auth.Tests/WorkOrderDatabaseBindingTests.cs', 'Beep.OilandGas.ApiService/Services/WorkOrderSourceChecks.cs', 'Beep.OilandGas.Web.Auth.Tests/WorkOrderSourceChecksTests.cs', 'Beep.OilandGas.Web.Auth.Tests/RecordedWorkOrderRowsTests.cs', 'Beep.OilandGas.Web.Auth.Tests/WorkOrderProcessReviewTests.cs', 'Beep.OilandGas.Web.Auth.Tests/RecordedWorkOrderCreationTests.cs', 'Beep.OilandGas.Web.Auth.Tests/WorkOrderInspectionSchemaTests.cs', 'Beep.OilandGas.Web.Auth.Tests/WorkOrderInspectionCommandTests.cs', 'Beep.OilandGas.Web.Auth.Tests/AssetAccessLevelChecksTests.cs', 'Beep.OilandGas.Web.Auth.Tests/UserAssetAccessAdmissionTests.cs', 'Beep.OilandGas.Web.Auth.Tests/WorkOrderGrantAdministrationTests.cs', 'Beep.OilandGas.Web.Auth.Tests/WorkOrderAccessClientTests.cs', 'Beep.OilandGas.Client/App/Services/AccessControl/AccessControlService.cs', 'Beep.OilandGas.Client/App/ClientApiTransport.cs', 'Beep.OilandGas.Client/App/AppOptions.cs', 'Beep.OilandGas.Client/App/ServiceAccessMode.cs', 'Beep.OilandGas.Client/Authentication/IAuthenticationProvider.cs', 'Beep.OilandGas.Client/Models/ApiError.cs', 'Beep.OilandGas.Client/Exceptions/ApiAuthenticationException.cs', 'Beep.OilandGas.Client/Exceptions/ApiClientException.cs', 'Beep.OilandGas.Client/Exceptions/ApiException.cs', 'Beep.OilandGas.Client/Exceptions/ApiTimeoutException.cs', 'Beep.OilandGas.Web/Services/AssetGrantAdministrationState.cs', 'Beep.OilandGas.Web/Services/AssetGrantAdministrationState.Review.cs', 'Beep.OilandGas.Web/Services/AssetGrantAdministrationState.Interests.cs', 'Beep.OilandGas.Web.Auth.Tests/WorkOrderParentAccessTests.cs', 'Beep.OilandGas.PPDM39.DataManagement/Core/ModuleSetup/ModuleSchemaBoundary.cs', 'Beep.OilandGas.PPDM39.DataManagement/Core/Metadata/WorkOrderInspectionMetadata.cs','Beep.OilandGas.PPDM39.DataManagement/Core/Metadata/WorkOrderDocumentSourceMetadata.cs', 'Beep.OilandGas.ApiService/Services/WorkOrderProcessReviews.cs', 'Beep.OilandGas.PPDM39.DataManagement/Services/WorkOrder/WorkOrderDatabaseBinding.cs', 'Beep.OilandGas.Models/Core/Refusals/RefusalException.cs', 'Beep.OilandGas.Models/Core/Refusals/RefusalKind.cs', 'Beep.OilandGas.Web/Services/WorkOrderCreateState.cs', 'Beep.OilandGas.Web.Auth.Tests/WorkOrderCreateStateTests.cs', 'Beep.OilandGas.Client/App/CompanyClientContext.cs', 'Beep.OilandGas.Web.Auth.Tests/WorkOrderListStateTests.cs')) {
    $node = $project.CreateElement('Compile'); $node.SetAttribute('Include', (Join-Path $repositoryRoot $source)); [void]$group.AppendChild($node)
}
$reference = $project.CreateElement('Reference'); $reference.SetAttribute('Include', 'TheTechIdea.Data')
$dataAlias=$project.CreateElement('Aliases');$dataAlias.InnerText='global,actualData';[void]$reference.AppendChild($dataAlias)
Set-Content -LiteralPath (Join-Path $checkDirectory 'CanonicalJournalEvidence.cs') -Encoding utf8 -Value 'extern alias actualData; global using JournalPostingEvidence = actualData::Beep.OilandGas.Models.Data.ProductionAccounting.JournalPostingEvidence;'
$hint = $project.CreateElement('HintPath'); $hint.InnerText = (Resolve-Path -LiteralPath $DataAssemblyPath).Path
[void]$reference.AppendChild($hint); [void]$group.AppendChild($reference)
foreach ($modelName in @('Beep.OilandGas.Models', 'Beep.OilandGas.PPDM.Models', 'DataManagementModels', 'DataManagementEngine', 'Beep.OilandGas.PPDM39')) {
    $actualModelPath = Join-Path $repositoryRoot "Beep.OilandGas.Repository.Tests/bin/company-focused/Debug/net10.0/$modelName.dll"
    if (-not (Test-Path -LiteralPath $actualModelPath)) { throw "Actual model dependency is missing: $modelName" }
    $modelReference = $project.CreateElement('Reference'); $modelReference.SetAttribute('Include', $modelName)
    $modelHint = $project.CreateElement('HintPath'); $modelHint.InnerText = $actualModelPath
    [void]$modelReference.AppendChild($modelHint); [void]$group.AppendChild($modelReference)
}
[IO.File]::WriteAllText((Join-Path $checkDirectory 'ActualWorkOrderContracts.cs'), @'
extern alias actualData;
global using TransitionWorkOrderRequest = actualData::Beep.OilandGas.Models.Data.WorkOrder.TransitionWorkOrderRequest;
global using WorkOrderSubType = actualData::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderSubType;
global using WorkOrderState = actualData::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderState;
global using ConditionType = actualData::Beep.OilandGas.Models.Data.WorkOrder.ConditionType;
global using InspectionCondition = actualData::Beep.OilandGas.Models.Data.WorkOrder.InspectionCondition;
global using WorkOrderWorkflow = actualData::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderWorkflow;
global using CostCode = actualData::Beep.OilandGas.Models.Data.WorkOrder.CostCode;
global using WorkOrderSummary = actualData::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderSummary;
global using WorkOrderDetailModel = actualData::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderDetailModel;
global using WorkOrderStep = actualData::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderStep;
global using WorkOrderComponentReference = actualData::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderComponentReference;
global using CreateWorkOrderRequest = actualData::Beep.OilandGas.Models.Data.WorkOrder.CreateWorkOrderRequest;
global using ScheduleResult = actualData::Beep.OilandGas.Models.Data.WorkOrder.ScheduleResult;
global using ScheduleConflict = actualData::Beep.OilandGas.Models.Data.WorkOrder.ScheduleConflict;
global using CalendarSlot = actualData::Beep.OilandGas.Models.Data.WorkOrder.CalendarSlot;
global using ScheduleWorkOrderRequest = actualData::Beep.OilandGas.Models.Data.WorkOrder.ScheduleWorkOrderRequest;
global using ContractorQualificationResult = actualData::Beep.OilandGas.Models.Data.WorkOrder.ContractorQualificationResult;
global using ContractorAssignment = actualData::Beep.OilandGas.Models.Data.WorkOrder.ContractorAssignment;
global using AssignContractorRequest = actualData::Beep.OilandGas.Models.Data.WorkOrder.AssignContractorRequest;
global using RecordInspectionResultRequest = actualData::Beep.OilandGas.Models.Data.WorkOrder.RecordInspectionResultRequest;
global using RecordedWorkOrderComponentChecks = actualData::Beep.OilandGas.Models.Data.WorkOrder.RecordedWorkOrderComponentChecks;
global using AddWorkOrderInspectionRequirementRequest = actualData::Beep.OilandGas.Models.Data.WorkOrder.AddWorkOrderInspectionRequirementRequest;
global using WorkOrderPermitPrerequisiteKinds = actualData::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderPermitPrerequisiteKinds;
global using WorkOrderInspectionRules = actualData::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules;
global using WorkOrderParentAccess = actualData::TheTechIdea.Data.OilGas.WorkOrderParentAccess;
'@, [Text.UTF8Encoding]::new($false))
$projectPath = Join-Path $checkDirectory 'WorkOrderListTests.csproj'; $project.Save($projectPath)
dotnet test $projectPath --logger 'console;verbosity=normal'
if ($LASTEXITCODE -ne 0) { throw "Work-order list check failed with exit $LASTEXITCODE." }
