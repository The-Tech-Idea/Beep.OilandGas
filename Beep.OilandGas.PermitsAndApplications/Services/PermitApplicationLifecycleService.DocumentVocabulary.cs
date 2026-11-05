using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.PermitsAndApplications.Models;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Report;
namespace Beep.OilandGas.PermitsAndApplications.Services;
public partial class PermitApplicationLifecycleService
{
    private async Task<string> ReadDocumentVocabularyAsync(PermitDocumentUploadProposal document,IDataSource core,Func<Task> guard)
    {
        document.Require();
        async Task<string> Read(Type type,string key,string code)
        {
            await guard();await StageSchemaAsync(core,type,[key]);
            var policy=new DocumentVocabularyReads(type,key,code,guard);
            var repository=new PPDMGenericRepository(Editor,CommonColumnHandler,Defaults,Metadata,type,core.DatasourceName,type.Name,rowAccess:policy);
            var rows=(await repository.GetAsync([])).ToArray();
            if(rows.Length!=1)throw new PermitDocumentEvidenceChangedException("The exact active native document reference is unavailable or ambiguous.");
            await guard();return PermitApplicationCommandOutcome.Payload(rows[0]);
        }
        async Task<string[]> Capture()=>[
            await Read(typeof(R_DOCUMENT_TYPE),"DOCUMENT_TYPE",document.DocumentType),
            await Read(typeof(R_LANGUAGE),"LANGUAGE_ID",document.Language),
            await Read(typeof(R_DIGITAL_FORMAT),"DIGITAL_FORMAT",document.DigitalFormat)];
        var first=await Capture();var second=await Capture();await guard();
        if(!first.SequenceEqual(second,StringComparer.Ordinal))throw new PermitDocumentEvidenceChangedException("Native document vocabulary changed during review.");
        return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(first));
    }
    private sealed class DocumentVocabularyReads(Type expected,string key,string code,Func<Task> guard):IPPDMRowAccessPolicy
    {
        private void Canonical(Type type,string table)
        {if(type!=expected || table!=expected.Name)throw new UnauthorizedAccessException("Native document vocabulary lookup changed type.");}
        public async Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)
        {Canonical(type,table);await guard();return [[new(){FieldName=key,Operator="=",FilterValue=code},new(){FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"}]];}
        public async Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table)
        {Canonical(type,table);await guard();return [];}
        public async Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows)
        {
            Canonical(type,table);await guard();
            if(rows.Count>1)throw new PermitDocumentEvidenceChangedException("The native document vocabulary returned duplicate keys.");
            foreach(var row in rows)
            {
                if(row.GetType()!=expected || expected.GetProperty(key)!.GetValue(row) as string!=code || row is not IPPDMEntity {ACTIVE_IND:"Y",ROW_EXPIRY_DATE:null})
                    throw new PermitDocumentEvidenceChangedException("The native document vocabulary returned another, inactive or retired reference.");
                // Codes are vocabulary evidence, not regulatory applicability. Dated codes need an explicit company date policy.
                if(expected.GetProperty("EFFECTIVE_DATE")!.GetValue(row) is not null || expected.GetProperty("EXPIRY_DATE")!.GetValue(row) is not null)
                    throw new PermitDocumentEvidenceChangedException("Configure explicit company reference-date applicability before using dated document vocabulary.");
            }
            await guard();
        }
        public async Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool includeInherited=true)
        {await ValidateRowsAsync(type,table,[row]);return "READ";}
        public Task ValidateMutationAsync(Type type,string table,object row,object? previous,PPDMRowMutation mutation,string? actor)=>throw new UnauthorizedAccessException("Permit document operations cannot alter native reference catalogs.");
    }
}
