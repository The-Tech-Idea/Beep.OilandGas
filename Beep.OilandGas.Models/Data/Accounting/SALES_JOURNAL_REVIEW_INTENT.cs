using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.Accounting;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Accounting;
public sealed class SALES_JOURNAL_REVIEW_INTENT:ModelEntityBase
{
    public const int MaximumCommandLength=16777216;
    [Key][Required][MaxLength(32)]public string SALES_JOURNAL_REVIEW_INTENT_ID{get;set;}=string.Empty;
    [Required][MaxLength(128)]public string OWNER_TENANT_ID{get;set;}=string.Empty;
    [Required][MaxLength(128)]public string OWNER_COMPANY_BA_ID{get;set;}=string.Empty;
    [Required][MaxLength(128)]public string BOOK_ID{get;set;}=string.Empty;
    [Required][MaxLength(16)]public string CURRENCY_CODE{get;set;}=string.Empty;
    [Required][MaxLength(128)]public string FIELD_ID{get;set;}=string.Empty;
    [Required][MaxLength(32)]public string JOURNAL_ENTRY_ID{get;set;}=string.Empty;
    [Required][MaxLength(64)]public string SOURCE_REVISION{get;set;}=string.Empty;
    [Required][MaxLength(MaximumCommandLength)]public string COMMAND_JSON{get;set;}=string.Empty;
    public CompanySalesJournalReviewCommand Restore()
    {
        if(string.IsNullOrWhiteSpace(COMMAND_JSON) || COMMAND_JSON.Length>MaximumCommandLength)throw new InvalidOperationException("The complete original review command is unavailable.");var command=JsonSerializer.Deserialize<CompanySalesJournalReviewCommand>(COMMAND_JSON) ??throw new InvalidOperationException("The original review command is malformed.");command.Require();var source=command.Review.Current;CompanyGLAccountReview.RequireAudit(this);
        if(SALES_JOURNAL_REVIEW_INTENT_ID!=command.IntentId || OWNER_TENANT_ID!=source.Company.TenantId || OWNER_COMPANY_BA_ID!=source.Company.BusinessAssociateId || BOOK_ID!=source.BookId || CURRENCY_CODE!=source.CurrencyCode || FIELD_ID!=source.FieldId || JOURNAL_ENTRY_ID!=command.Review.Original.Header.JOURNAL_ENTRY_ID || SOURCE_REVISION!=command.Review.Revision || ROW_CREATED_BY!=command.Review.ActorUserId || ACTIVE_IND!="Y" || PPDM_GUID!=Guid.ParseExact(command.IntentId,"N").ToString())throw new InvalidOperationException("The original native journal reviewer intent scope, identity, source or audit changed.");return command;
    }
    public static SALES_JOURNAL_REVIEW_INTENT Create(CompanySalesJournalReviewCommand command)
    {
        command.Require();var source=command.Review.Current;return new(){SALES_JOURNAL_REVIEW_INTENT_ID=command.IntentId,OWNER_TENANT_ID=source.Company.TenantId,OWNER_COMPANY_BA_ID=source.Company.BusinessAssociateId,BOOK_ID=source.BookId,CURRENCY_CODE=source.CurrencyCode,FIELD_ID=source.FieldId,JOURNAL_ENTRY_ID=command.Review.Original.Header.JOURNAL_ENTRY_ID,SOURCE_REVISION=command.Review.Revision,COMMAND_JSON=JsonSerializer.Serialize(command),ACTIVE_IND="Y",PPDM_GUID=Guid.ParseExact(command.IntentId,"N").ToString()};
    }
}
