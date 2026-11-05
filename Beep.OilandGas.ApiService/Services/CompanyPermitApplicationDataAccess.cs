using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PermitsAndApplications.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.PermitsAndApplications.Modules;
using Beep.OilandGas.PermitsAndApplications.Data.PermitTables;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>The existing company/module and FIELD owners authorize native permit reads.</summary>
public sealed class CompanyPermitApplicationDataAccess(CompanyModuleConnection company, ModuleConnectionResolver modules,
    ICompanyFieldSelectionService selections, IDMEEditor editor, ICommonColumnHandler columns,
    IPPDM39DefaultsRepository defaults, IPPDMMetadataRepository metadata, IUserAssetAccessStore assets,
    IPPDMRowAccessPolicy fieldPolicy) : ICompanyPermitApplicationDataAccess
{
    private sealed record ReadAuthority(PermitApplicationDataScope Scope,string Grants);
    private readonly AsyncLocal<ReadAuthority?> readScope=new();
    private PermitApplicationDataScope? captured;
    private readonly AsyncLocal<PermitApplicationDataScope?> commandScope = new();
    public async Task<PermitApplicationDataScope> RequireReadScopeAsync()
    {
        var before = await company.RefreshCompanyAsync(); var account = company.Company;
        if (!before.Permissions.Contains(PermissionConstants.HSE.ViewPermits, StringComparer.Ordinal))
            throw RefusalException.Forbidden("Current company permit view permission is required.");
        FieldSelectionReview selected;
        if ((commandScope.Value??readScope.Value?.Scope) is { } command)
        {
            if (selections is not ICompanyFieldSelectionAuthority stored)
                throw new InvalidOperationException("The permit command requires current stored FIELD selection authority.");
            await stored.RequireStoredSelectionAsync(command.Field);
            FieldSelectionReviewValidator.RequireReview(command.Field, account.Scope, account.UserId);
            selected = command.Field;
        }
        else
        {
            var field = await selections.GetCurrentFieldSelectionAsync();
            FieldSelectionReviewValidator.RequireCurrent(field, account.Scope, account.UserId);
            if (!field.SelectionAvailable) throw new InvalidOperationException("An available company FIELD is required for permit data.");
            selected = field.SelectionContext!;
        }
        var connection = await modules.ResolveAsync("PERMITS");
        var fingerprint = await modules.GetMigrationBindingFingerprintAsync(["PERMITS"], connection);
        var source = editor.GetDataSource(connection) ?? throw new InvalidOperationException("The selected permit driver/datasource is unavailable.");
        var actual = source.Dataconnection?.ConnectionProp;
        if (source.DatasourceName != connection || actual is null || !string.Equals(actual.ConnectionName, connection, StringComparison.OrdinalIgnoreCase) ||
            ModuleDatabaseTarget.Key(editor, actual) != ModuleDatabaseTarget.Key(editor, connection))
            throw new InvalidOperationException("The runtime permit datasource differs from the selected module target.");
        var after = await company.RefreshCompanyAsync();
        if (company.Company.Scope != account.Scope || company.Company.UserId != account.UserId || !after.Permissions.Contains(PermissionConstants.HSE.ViewPermits, StringComparer.Ordinal))
            throw RefusalException.Forbidden("The current company permit authority changed.");
        var current = new PermitApplicationDataScope(selected, new(connection, fingerprint)); current.Require();
        if (captured is not null && captured != current) throw new InvalidOperationException("The reviewed permit company/account/FIELD or module binding changed.");
        if(readScope.Value is {} read && (read.Scope!=current || await ReadFieldGrantsAsync(current)!=read.Grants))
            throw RefusalException.Forbidden("The permit read account, FIELD grant or binding changed.");
        captured = current; return current;
    }
    private async Task<string> ReadFieldGrantsAsync(PermitApplicationDataScope scope)
    {
        var grants=(await assets.ReadAsync(scope.Field.ActorUserId,scope.Field.Database.Fingerprint))
            .Where(grant=>grant.IsActive && grant.AssetType=="FIELD" && grant.AssetId==scope.Field.FieldId && grant.FacilityType is null &&
                PPDMCompanyFieldEligibility.Rank(grant.AccessLevel)>=PPDMCompanyFieldEligibility.Rank("READ"))
            .OrderBy(grant=>grant.Id,StringComparer.Ordinal).ToArray();
        if(grants.Length==0)throw RefusalException.Forbidden("Current company FIELD read authority is required.");
        return System.Text.Json.JsonSerializer.Serialize(grants);
    }
    private async Task<string> ReadNativeFieldAuthorityAsync(PermitApplicationDataScope scope)
    {
        var type=typeof(Beep.OilandGas.PPDM39.Models.FIELD);
        var repository=new PPDMGenericRepository(editor,columns,defaults,metadata,type,scope.Field.Database.ConnectionName,type.Name,rowAccess:fieldPolicy);
        var field=await repository.GetByIdAsync(scope.Field.FieldId!)??throw RefusalException.Forbidden("The selected native FIELD is unavailable.");
        if(PPDMCompanyFieldEligibility.Rank(await fieldPolicy.GetEffectiveAccessLevelAsync(type,type.Name,field))<PPDMCompanyFieldEligibility.Rank("READ"))
            throw RefusalException.Forbidden("Current native company FIELD read entitlement is required.");
        return CompanyPPDMEvidenceReader.CanonicalRecord(field);
    }
    public async Task<T> WithReadAuthorityAsync<T>(Func<Task<T>> read)
    {
        ArgumentNullException.ThrowIfNull(read);
        // Nested source comparisons share this bounded operation or the existing command authority owner.
        // They still call RequireReadScopeAsync for fresh app permissions, stored selection and binding checks.
        if(commandScope.Value is not null || readScope.Value is not null)
        {await RequireReadScopeAsync();var nested=await read();await RequireReadScopeAsync();return nested;}
        var scope=await RequireReadScopeAsync();
        var grants=await ReadFieldGrantsAsync(scope);var field=await ReadNativeFieldAuthorityAsync(scope);
        readScope.Value=new(scope,grants);
        try
        {
            await RequireReadScopeAsync();var result=await read();await RequireReadScopeAsync();
            if(await ReadNativeFieldAuthorityAsync(scope)!=field)throw RefusalException.Conflict("The native FIELD changed during permit evidence review.");
            await RequireReadScopeAsync();return result;
        }
        finally{readScope.Value=null;}
    }
    public async Task<PermitApplicationInboxAuthority> ReviewInboxAuthorityAsync()
    {
        async Task<PermitApplicationInboxAuthority> Read()
        {
            var scope=await RequireReadScopeAsync();var before=await company.RefreshCompanyAsync();
            var type=typeof(Beep.OilandGas.PPDM39.Models.FIELD);
            var repository=new PPDMGenericRepository(editor,columns,defaults,metadata,type,scope.Field.Database.ConnectionName,type.Name,rowAccess:fieldPolicy);
            var row=await repository.GetByIdAsync(scope.Field.FieldId!)??throw RefusalException.Forbidden("The selected native FIELD is unavailable.");
            var snapshot=CompanyPPDMEvidenceReader.CanonicalRecord(row);
            var write=PPDMCompanyFieldEligibility.Rank(await fieldPolicy.GetEffectiveAccessLevelAsync(type,type.Name,row))>=PPDMCompanyFieldEligibility.Rank("WRITE");
            var after=await company.RefreshCompanyAsync();
            bool HasBefore(string permission)=>before.Permissions.Contains(permission,StringComparer.Ordinal);
            bool HasAfter(string permission)=>after.Permissions.Contains(permission,StringComparer.Ordinal);
            if(await RequireReadScopeAsync()!=scope || before.Scope!=after.Scope || before.UserId!=after.UserId ||
                HasBefore(PermissionConstants.HSE.IssuePermit)!=HasAfter(PermissionConstants.HSE.IssuePermit) ||
                HasBefore(PermissionConstants.HSE.ApprovePermit)!=HasAfter(PermissionConstants.HSE.ApprovePermit) || snapshot!=CompanyPPDMEvidenceReader.CanonicalRecord(row))
                throw RefusalException.Conflict("The permit inbox authority changed during review.");
            return new(scope,write && HasAfter(PermissionConstants.HSE.IssuePermit),write && HasAfter(PermissionConstants.HSE.ApprovePermit),ProcessDefinitionRevision.FromJson(snapshot));
        }
        var before=await Read();var after=await Read();
        if(before!=after)throw RefusalException.Conflict("The native permit inbox authority changed during review.");return before;
    }
    public async Task<PermitApplicationCreationReview> ReviewCreationAsync(PermitApplicationCreationProposal proposal)
    {
        ArgumentNullException.ThrowIfNull(proposal);proposal.Require();
        if(commandScope.Value is not null)throw new InvalidOperationException("Creation resource review must run outside the owned module transaction.");
        var scope=await RequireReadScopeAsync();
        async Task Authority()
        {
            var account=await company.RefreshCompanyAsync();
            if(await RequireReadScopeAsync()!=scope || account.Scope!=scope.Field.Company || account.UserId!=scope.Field.ActorUserId ||
                !account.Permissions.Contains(PermissionConstants.HSE.IssuePermit,StringComparer.Ordinal))
                throw RefusalException.Forbidden("Current company draft-author permission and the reviewed FIELD are required.");
        }
        async Task<string> Read(Type type,string[] required,List<AppFilter> filters)
        {
            await Authority();
            var source=editor.GetDataSource(scope.Field.Database.ConnectionName)??throw new InvalidOperationException("The reviewed PPDM datasource is unavailable.");
            var shape=source.GetEntityStructure(new TheTechIdea.Beep.DataBase.EntityStructure{EntityName=type.Name,DatasourceEntityName=type.Name},true);
            if(shape?.Fields is null || required.Any(name=>shape.Fields.Count(column=>column.FieldName==name)!=1))
                throw new NativeSchemaCompatibilityException("Install the canonical linked-resource schema with Beep Migration before reviewing permit creation.");
            var repository=new PPDMGenericRepository(editor,columns,defaults,metadata,type,scope.Field.Database.ConnectionName,type.Name,rowAccess:fieldPolicy);
            var rows=(await repository.GetAsync(filters)).ToArray();
            if(rows.Length!=1 || rows[0].GetType()!=type || type.GetProperty("ACTIVE_IND")!.GetValue(rows[0]) as string!="Y")
                throw RefusalException.Forbidden("The exact active permit resource is unavailable in the current company FIELD.");
            var row=rows[0];
            foreach(var filter in filters)
                if(type.GetProperty(filter.FieldName)?.GetValue(row) as string!=filter.FilterValue)
                    throw RefusalException.Forbidden("The provider returned another permit resource identity or FIELD.");
            var frozen=CompanyPPDMEvidenceReader.CanonicalRecord(row);
            if(PPDMCompanyFieldEligibility.Rank(await fieldPolicy.GetEffectiveAccessLevelAsync(type,type.Name,row))<PPDMCompanyFieldEligibility.Rank("WRITE"))
                throw RefusalException.Forbidden("Current native write entitlement is required for a permit creation resource.");
            await Authority();
            if(frozen!=CompanyPPDMEvidenceReader.CanonicalRecord(row))throw RefusalException.Conflict("The permit creation resources changed during review.");
            return frozen;
        }
        static AppFilter Equal(string field,string value)=>new(){FieldName=field,Operator="=",FilterValue=value};
        async Task<PermitApplicationCreationReview> Capture()
        {
            var field=await Read(typeof(Beep.OilandGas.PPDM39.Models.FIELD),["FIELD_ID","ACTIVE_IND","ROW_CHANGED_DATE"],[Equal("FIELD_ID",scope.Field.FieldId!)]);
            string? well=null,facility=null;
            if(proposal.WellId is not null)
            {
                var type=typeof(Beep.OilandGas.PPDM39.Models.WELL);
                await CompanyPPDMEvidenceReader.DefinitionAsync(metadata,type,["UWI"],("ASSIGNED_FIELD","FIELD","FIELD_ID"));
                well=await Read(type,["UWI","ASSIGNED_FIELD","OPERATOR","ACTIVE_IND","ROW_CHANGED_DATE"],
                    [Equal("UWI",proposal.WellId),Equal("ASSIGNED_FIELD",scope.Field.FieldId!)]);
            }
            if(proposal.FacilityId is not null)
            {
                var type=typeof(Beep.OilandGas.PPDM39.Models.FACILITY);
                await CompanyPPDMEvidenceReader.DefinitionAsync(metadata,type,["FACILITY_ID","FACILITY_TYPE"],("PRIMARY_FIELD_ID","FIELD","FIELD_ID"));
                facility=await Read(type,["FACILITY_ID","FACILITY_TYPE","PRIMARY_FIELD_ID","CURRENT_OPERATOR","ACTIVE_IND","ROW_CHANGED_DATE"],
                    [Equal("FACILITY_ID",proposal.FacilityId),Equal("FACILITY_TYPE",proposal.FacilityType!),Equal("PRIMARY_FIELD_ID",scope.Field.FieldId!)]);
            }
            return PermitApplicationCreationReview.Capture(scope,proposal,field,well,facility);
        }
        await Authority();var before=await Capture();var after=await Capture();await Authority();
        if(before!=after)throw RefusalException.Conflict("The permit creation resources changed during review. Read the current source again.");
        return before;
    }
    public Task<T> WithCommandAuthorityAsync<T>(PermitApplicationCommand original,Func<Func<Task>,Task<T>> execute)
    {
        ArgumentNullException.ThrowIfNull(original);ArgumentNullException.ThrowIfNull(execute);original.Require();
        async Task Resources()
        {
            if(original.CreationReview is {} reviewed && reviewed!=await ReviewCreationAsync(reviewed.Proposal))
                throw RefusalException.Conflict("The reviewed creation resources changed. Retain the original and review current evidence.");
        }
        return WithPermitAuthorityAsync(original.Review.Context,PermitApplicationCommand.IsAuthorAction(original.Action)?PermissionConstants.HSE.IssuePermit:PermissionConstants.HSE.ApprovePermit,Resources,execute);
    }
    public Task<T> WithDocumentStageAuthorityAsync<T>(PermitDocumentStageOriginal original,Func<Func<Task>,Task<T>> execute)
    {
        ArgumentNullException.ThrowIfNull(original);ArgumentNullException.ThrowIfNull(execute);original.Require();
        return WithPermitAuthorityAsync(original.Review.Context,PermissionConstants.HSE.IssuePermit,()=>Task.CompletedTask,execute);
    }
    private async Task<T> WithPermitAuthorityAsync<T>(PermitApplicationDataScope context,string permission,Func<Task> resources,Func<Func<Task>,Task<T>> execute)
    {
        if (commandScope.Value is not null || readScope.Value is not null) throw new InvalidOperationException("Permit commands cannot nest their authority boundary.");
        if (await RequireReadScopeAsync() != context) throw RefusalException.Forbidden("The permit original belongs to another current context.");
        async Task<string> Grants()
        {
            var actor = await company.RefreshCompanyAsync();
            if (actor.Scope != context.Field.Company || actor.UserId != context.Field.ActorUserId || !actor.Permissions.Contains(permission, StringComparer.Ordinal))
                throw RefusalException.Forbidden("The current company account lacks the permit command permission.");
            var grants = (await assets.ReadAsync(actor.UserId, context.Field.Database.Fingerprint)).Where(grant => grant.IsActive && grant.Inherit &&
                grant.AssetType == "FIELD" && grant.AssetId == context.Field.FieldId && grant.FacilityType is null &&
                PPDMCompanyFieldEligibility.Rank(grant.AccessLevel) >= PPDMCompanyFieldEligibility.Rank("WRITE"))
                .OrderBy(grant => grant.Id, StringComparer.Ordinal).ToArray();
            if (grants.Length == 0) throw RefusalException.Forbidden("A current inherited company FIELD write grant is required for permit changes.");
            return System.Text.Json.JsonSerializer.Serialize(grants);
        }
        async Task<string> NativeField()
        {
            var type = typeof(Beep.OilandGas.PPDM39.Models.FIELD);
            var repository = new PPDMGenericRepository(editor, columns, defaults, metadata, type, context.Field.Database.ConnectionName, "FIELD", rowAccess: fieldPolicy);
            var row = await repository.GetByIdAsync(context.Field.FieldId!) ?? throw RefusalException.Forbidden("The selected native FIELD is unavailable.");
            if (PPDMCompanyFieldEligibility.Rank(await fieldPolicy.GetEffectiveAccessLevelAsync(type, "FIELD", row)) < PPDMCompanyFieldEligibility.Rank("WRITE"))
                throw RefusalException.Forbidden("The native company FIELD entitlement does not permit this command.");
            return System.Text.Json.JsonSerializer.Serialize(row, type);
        }
        await resources();
        var grants = await Grants(); var field = await NativeField();
        async Task Guard()
        {
            if (await RequireReadScopeAsync() != context || await Grants() != grants)
                throw RefusalException.Forbidden("The permit command account, binding, FIELD selection or write grant changed.");
        }
        T result;
        commandScope.Value = context;
        try { await Guard(); result = await execute(Guard); await Guard(); }
        finally { commandScope.Value = null; }
        if (await RequireReadScopeAsync() != context || await Grants() != grants || await NativeField() != field)
            throw RefusalException.Forbidden("The permit command source authority changed before confirmation. Retain the original for recovery.");
        await resources();
        return result;
    }
    public IPPDMRowAccessPolicy ForRead(Type entityType, string tableName, string? applicationId = null)
    {
        if (entityType.Name != tableName || !PermitsModule.OwnedEntities.Contains(entityType))
            throw new InvalidOperationException("The exact registered permit entity/table is required.");
        var reference = entityType == typeof(JURISDICTION_REQUIREMENTS) || entityType == typeof(R_PERMITS_REFERENCE_CODE) || entityType == typeof(REQUIRED_FORM);
        if (entityType != typeof(PERMIT_APPLICATION) && !reference && !LeaseSummaryValidator.Identifier(applicationId))
            throw new InvalidOperationException("The native permit child requires its exact application and subtype parent.");
        return new Policy(this, entityType, tableName, applicationId, reference);
    }
    private PPDMGenericRepository Repository(Type type, PermitApplicationDataScope scope, string? applicationId = null) =>
        new(editor, columns, defaults, metadata, type, scope.Database.ConnectionName, type.Name, rowAccess: ForRead(type, type.Name, applicationId));
    private async Task RequireParentAsync(string applicationId, bool requireInjection)
    {
        var scope = await RequireReadScopeAsync();
        var parent = await Repository(typeof(PERMIT_APPLICATION), scope).GetByIdAsync(applicationId) as PERMIT_APPLICATION;
        if (parent is null || parent.PERMIT_APPLICATION_ID != applicationId || parent.FIELD_ID != scope.Field.FieldId)
            throw RefusalException.Forbidden("The permit parent is unavailable in the selected company FIELD.");
        if (requireInjection)
        {
            var injection = await Repository(typeof(INJECTION_PERMIT_APPLICATION), scope, applicationId).GetByIdAsync(applicationId) as INJECTION_PERMIT_APPLICATION;
            if (injection is null || injection.PERMIT_APPLICATION_ID != applicationId)
                throw RefusalException.Forbidden("The injection result parent differs from its recorded permit application.");
        }
        if (await RequireReadScopeAsync() != scope) throw new InvalidOperationException("The permit parent authority changed.");
    }
    private sealed class Policy(CompanyPermitApplicationDataAccess owner, Type type, string table, string? applicationId, bool reference) : IPPDMRowAccessPolicy
    {
        private void Canonical(Type entityType, string tableName)
        { if (entityType != type || tableName != table) throw new InvalidOperationException("The permit row policy belongs to another entity/table."); }
        private async Task<PermitApplicationDataScope> Authority()
        {
            var scope = await owner.RequireReadScopeAsync();
            if (applicationId is not null && type!=typeof(PERMIT_APPLICATION_COMMAND)) await owner.RequireParentAsync(applicationId, type == typeof(MIT_RESULT));
            return scope;
        }
        private static AppFilter Equal(string field, string value) => new() { FieldName = field, Operator = "=", FilterValue = value };
        public async Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type entityType, string tableName)
        {
            Canonical(entityType, tableName); var scope = await Authority();
            if (type == typeof(PERMIT_APPLICATION) || type==typeof(PERMIT_APPLICATION_COMMAND) || type==typeof(PERMIT_DOCUMENT_STAGE))
                return [new AppFilter[] { Equal("FIELD_ID", scope.Field.FieldId!),
                    Equal("OWNER_TENANT_ID", scope.Field.Company.TenantId),
                    Equal("OWNER_COMPANY_ID", scope.Field.Company.BusinessAssociateId) }.Concat(type!=typeof(PERMIT_APPLICATION)?new[]{Equal("PERMIT_APPLICATION_ID",applicationId!)}:Array.Empty<AppFilter>()).ToArray()];
            return [new AppFilter[] { reference ? Equal("ACTIVE_IND", "Y") :
                type == typeof(MIT_RESULT) ? Equal("INJECTION_PERMIT_APPLICATION_ID", applicationId!) : Equal("PERMIT_APPLICATION_ID", applicationId!) }];
        }
        public async Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type entityType, string tableName)
        { Canonical(entityType, tableName); await Authority(); return [Equal("ACTIVE_IND", "Y")]; }
        public async Task ValidateRowsAsync(Type entityType, string tableName, IReadOnlyList<object> rows)
        {
            Canonical(entityType, tableName); var scope = await Authority();
            foreach (var row in rows)
            {
                string? Value(string name) => type.GetProperty(name)?.GetValue(row) as string;
                if (!type.IsInstanceOfType(row) || Value("ACTIVE_IND") != "Y" ||
                    (type == typeof(PERMIT_APPLICATION) || type==typeof(PERMIT_APPLICATION_COMMAND) || type==typeof(PERMIT_DOCUMENT_STAGE)) && (Value("FIELD_ID") != scope.Field.FieldId ||
                        Value("OWNER_TENANT_ID") != scope.Field.Company.TenantId ||
                        Value("OWNER_COMPANY_ID") != scope.Field.Company.BusinessAssociateId) ||
                    !reference && type != typeof(PERMIT_APPLICATION) && (type == typeof(MIT_RESULT) ? Value("INJECTION_PERMIT_APPLICATION_ID") != applicationId : Value("PERMIT_APPLICATION_ID") != applicationId))
                    throw RefusalException.Forbidden("The native permit response escaped its reviewed company/FIELD or parent scope.");
            }
            if (await Authority() != scope) throw new InvalidOperationException("The permit row authority changed during the read.");
        }
        public async Task<string?> GetEffectiveAccessLevelAsync(Type entityType, string tableName, object row, bool includeInherited = true)
        { await ValidateRowsAsync(entityType, tableName, [row]); return "READ"; }
        public Task ValidateMutationAsync(Type entityType, string tableName, object entity, object? previous, PPDMRowMutation operation, string? actor) =>
            throw new InvalidOperationException("Native permit mutations require their reviewed owned command, immutable original and read-back evidence.");
    }
}
