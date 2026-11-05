using System.Reflection;
using System.Text.Json;
using TheTechIdea.Data.OilGas;
using Xunit;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class CompanyFinancialCanonicalOwnershipTests
{
    [Theory]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.CreateGLAccountRequest")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.UpdateGLAccountRequest")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.GLAccount")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.GLAccountResponse")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.CreateJournalEntryRequest")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.JournalEntryLineData")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.JournalEntry")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.JournalEntryLine")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.JournalEntryApprovalResult")]
    [InlineData("Beep.OilandGas.Models.Data.ProductionAccounting.JournalPostingEvidence")]
    [InlineData("TheTechIdea.Data.OilGas.CompanyGLAccountReview")]
    [InlineData("TheTechIdea.Data.OilGas.CompanyGLMappingReview")]
    [InlineData("TheTechIdea.Data.OilGas.CompanyGLAccountConfigurationCommand")]
    [InlineData("TheTechIdea.Data.OilGas.CompanyGLAccountConfigurationSubmission")]
    [InlineData("TheTechIdea.Data.OilGas.CompanyGLAccountConfigurationReceipt")]
    [InlineData("TheTechIdea.Data.OilGas.CompanyGLAccountConfigurationReview")]
    [InlineData("TheTechIdea.Data.OilGas.CompanyGLMappingConfigurationCommand")]
    [InlineData("TheTechIdea.Data.OilGas.CompanyGLMappingConfigurationReview")]
    [InlineData("TheTechIdea.Data.OilGas.CompanyGLMappingConfigurationSubmission")]
    [InlineData("TheTechIdea.Data.OilGas.CompanyGLMappingConfigurationReceipt")]
    [InlineData("TheTechIdea.Data.OilGas.CompanyLedgerPeriodReview")]
    [InlineData("TheTechIdea.Data.OilGas.CompanyLedgerBookFacts")]
    [InlineData("TheTechIdea.Data.OilGas.CompanyLedgerBookReview")]
    [InlineData("TheTechIdea.Data.OilGas.CompanyLedgerBookConfigurationCommand")]
    [InlineData("TheTechIdea.Data.OilGas.CompanyLedgerBookConfigurationReview")]
    [InlineData("TheTechIdea.Data.OilGas.CompanyLedgerBookConfigurationSubmission")]
    [InlineData("TheTechIdea.Data.OilGas.CompanyLedgerBookConfigurationReceipt")]
    [InlineData("TheTechIdea.Data.OilGas.CompanyFxRateFacts")]
    [InlineData("TheTechIdea.Data.OilGas.CompanyFxRateReview")]
    [InlineData("TheTechIdea.Data.OilGas.CompanyFxRateWindow")]
    [InlineData("TheTechIdea.Data.OilGas.CompanySalesPaymentFacts")]
    [InlineData("TheTechIdea.Data.OilGas.CompanySalesPaymentSource")]
    [InlineData("TheTechIdea.Data.OilGas.CompanySalesPaymentCommand")]
    [InlineData("TheTechIdea.Data.OilGas.CompanySalesPaymentReceipt")]
    [InlineData("TheTechIdea.Data.OilGas.CompanySalesPaymentReviewSummary")]
    [InlineData("TheTechIdea.Data.OilGas.CompanySalesPaymentReviewSource")]
    [InlineData("TheTechIdea.Data.OilGas.CompanySalesPaymentReviewCommand")]
    [InlineData("TheTechIdea.Data.OilGas.CompanySalesPaymentReviewProof")]
    [InlineData("TheTechIdea.Data.OilGas.CompanySalesPaymentReviewRecovery")]
    [InlineData("TheTechIdea.Data.OilGas.CompanySalesPaymentWorkflowSource")]
    [InlineData("TheTechIdea.Data.OilGas.CompanySalesPaymentProcessSourceReview")]
    [InlineData("TheTechIdea.Data.OilGas.CompanyFxRateConfigurationCommand")]
    [InlineData("TheTechIdea.Data.OilGas.CompanyFxRateConfigurationReview")]
    [InlineData("TheTechIdea.Data.OilGas.CompanyFxRateConfigurationSubmission")]
    [InlineData("TheTechIdea.Data.OilGas.CompanyFxRateReceipt")]
    [InlineData("TheTechIdea.Data.OilGas.CompanyFxConvertedComponent")]
    [InlineData("TheTechIdea.Data.OilGas.CompanyFxConversion")]
    [InlineData("TheTechIdea.Data.OilGas.CompanyLedgerPeriodCreation")]
    [InlineData("TheTechIdea.Data.OilGas.CompanyLedgerPeriodCreationSubmission")]
    [InlineData("TheTechIdea.Data.OilGas.CompanyLedgerPeriodCreationReceipt")]
    [InlineData("TheTechIdea.Data.OilGas.CompanyLedgerPeriodFacts")]
    [InlineData("TheTechIdea.Data.OilGas.CompanyLedgerPeriodSource")]
    [InlineData("TheTechIdea.Data.OilGas.CompanyLedgerPeriodConfigurationReview")]
    [InlineData("TheTechIdea.Data.OilGas.CompanySalesJournalMappings")]
    [InlineData("TheTechIdea.Data.OilGas.CompanySalesJournalSourceLine")]
    [InlineData("TheTechIdea.Data.OilGas.CompanySalesJournalSourceReview")]
    [InlineData("TheTechIdea.Data.OilGas.CompanySalesJournalPreparation")]
    [InlineData("TheTechIdea.Data.OilGas.CompanySalesJournalLineRevision")]
    [InlineData("TheTechIdea.Data.OilGas.CompanySalesJournalPreparationProof")]
    [InlineData("TheTechIdea.Data.OilGas.CompanySalesJournalPreparationOutcome")]
    [InlineData("TheTechIdea.Data.OilGas.CompanySalesJournalWorkflowSource")]
    [InlineData("TheTechIdea.Data.OilGas.CompanySalesPostedLedgerLine")]
    [InlineData("TheTechIdea.Data.OilGas.CompanySalesPostedLedgerJournal")]
    [InlineData("TheTechIdea.Data.OilGas.CompanySalesPostedLedgerReview")]
    [InlineData("TheTechIdea.Data.OilGas.CompanySalesJournalPostingHeaderProof")]
    [InlineData("TheTechIdea.Data.OilGas.CompanySalesJournalPostingOutcome")]
    [InlineData("TheTechIdea.Data.OilGas.CompanySalesJournalPostingRecovery")]
    [InlineData("TheTechIdea.Data.OilGas.CompanySalesJournalPostingSource")]
    [InlineData("TheTechIdea.Data.OilGas.CompanySalesJournalPostingCommand")]
    [InlineData("TheTechIdea.Data.OilGas.CompanySalesJournalReviewSource")]
    [InlineData("TheTechIdea.Data.OilGas.CompanySalesJournalReviewCommand")]
    [InlineData("TheTechIdea.Data.OilGas.CompanySalesJournalReviewProof")]
    [InlineData("TheTechIdea.Data.OilGas.CompanySalesJournalReviewRecovery")]
    public void SharedFinancialContractHasCanonicalOwnerAndCannotBecomeABusinessTable(string name)
    {
        var assembly=typeof(CompanyScope).Assembly;
        var contract=assembly.GetType(name,throwOnError:true)!;
        Assert.Equal(assembly,contract.Assembly);
        Assert.Equal(name,contract.FullName);
        Assert.Throws<ArgumentException>(()=>ModuleSchemaBoundary.Validate([contract]));
    }

    [Theory]
    [InlineData("Beep.OilandGas.Models.Data.ProductionAccounting.GL_ACCOUNT")]
    [InlineData("Beep.OilandGas.Models.Data.ProductionAccounting.JOURNAL_ENTRY")]
    [InlineData("Beep.OilandGas.Models.Data.ProductionAccounting.JOURNAL_ENTRY_LINE")]
    [InlineData("Beep.OilandGas.Models.Data.ProductionAccounting.GL_ENTRY")]
    [InlineData("Beep.OilandGas.Models.Data.ProductionAccounting.AR_PAYMENT")]
    [InlineData("Beep.OilandGas.Models.Data.ProductionAccounting.AR_CREDIT_MEMO")]
    [InlineData("Beep.OilandGas.Models.Data.ProductionAccounting.FX_RATE")]
    [InlineData("Beep.OilandGas.Accounting.Models.GLAccountMapping")]
    [InlineData("TheTechIdea.Data.OilGas.COMPANY_LEDGER_PERIOD")]
    [InlineData("TheTechIdea.Data.OilGas.COMPANY_LEDGER_BOOK")]
    [InlineData("TheTechIdea.Data.OilGas.SALES_JOURNAL_POSTING_OUTCOME")]
    [InlineData("TheTechIdea.Data.OilGas.SALES_JOURNAL_POSTING_INTENT")]
    [InlineData("TheTechIdea.Data.OilGas.SALES_PAYMENT_REVIEW_INTENT")]
    [InlineData("TheTechIdea.Data.OilGas.SALES_JOURNAL_REVIEW_INTENT")]
    public void ExistingFinancialEntityIsOwnedByCanonicalDataWithItsOriginalPublicName(string name)
    {
        var assembly=typeof(CompanyScope).Assembly;
        var entity=assembly.GetType(name,throwOnError:true)!;
        Assert.Equal(name,entity.FullName);
        Assert.Equal(assembly,entity.Assembly);
        Assert.Equal(assembly,entity.BaseType!.Assembly);
        Assert.Equal("Beep.OilandGas.Models.Data.ModelEntityBase",entity.BaseType.FullName);
        Assert.NotNull(Activator.CreateInstance(entity));
        ModuleSchemaBoundary.Validate([entity]);
    }

    [Fact]
    public void CompanyMappingOwnershipFieldsAreBoundedNullableAndOmittedFromLegacyJson()
    {
        var entity=typeof(CompanyScope).Assembly.GetType("Beep.OilandGas.Accounting.Models.GLAccountMapping",throwOnError:true)!;
        var row=Activator.CreateInstance(entity)!;var json=JsonSerializer.Serialize(row,entity);
        foreach(var name in new[]{"OWNER_TENANT_ID","OWNER_COMPANY_BA_ID","BOOK_ID","CURRENCY_CODE","GL_ACCOUNT_ID"})
        {
            var property=entity.GetProperty(name)!;Assert.Equal(typeof(string),property.PropertyType);Assert.Null(property.GetValue(row));
            Assert.Equal(name=="CURRENCY_CODE"?16:128,property.GetCustomAttribute<System.ComponentModel.DataAnnotations.MaxLengthAttribute>()!.Length);
            Assert.Equal(System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,property.GetCustomAttribute<System.Text.Json.Serialization.JsonIgnoreAttribute>()!.Condition);
            Assert.DoesNotContain('"'+name+'"',json);
        }
        var receipt=entity.GetProperty("CONFIGURATION_COMMAND_JSON")!;Assert.Null(receipt.GetValue(row));Assert.Equal(CompanyGLMappingConfigurationProof.MaximumLength,receipt.GetCustomAttribute<System.ComponentModel.DataAnnotations.MaxLengthAttribute>()!.Length);
        Assert.Equal(System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,receipt.GetCustomAttribute<System.Text.Json.Serialization.JsonIgnoreAttribute>()!.Condition);Assert.DoesNotContain("CONFIGURATION_COMMAND_JSON",json);
    }

#if WORKFLOW_INBOX_FOCUSED
    // This focused composition retains the pre-move model assembly as an explicit compatibility
    // reference. Compare populated entity JSON and complete public property sets, not a new golden
    // generated by the moved implementation itself.
    [Theory]
    [InlineData("GL_ACCOUNT")]
    [InlineData("JOURNAL_ENTRY")]
    [InlineData("JOURNAL_ENTRY_LINE")]
    [InlineData("GL_ENTRY")]
    [InlineData("AR_PAYMENT")]
    [InlineData("AR_CREDIT_MEMO")]
    [InlineData("FX_RATE")]
    public void PopulatedLegacyFinancialEntityJsonAndPublicPropertiesRemainCompatible(string entityName)
    {
        var name="Beep.OilandGas.Models.Data.ProductionAccounting."+entityName;
        var current=typeof(CompanyScope).Assembly.GetType(name,throwOnError:true)!;
        var legacy=Assembly.Load("Beep.OilandGas.Models").GetType(name,throwOnError:true)!;
        Assert.NotEqual(current.Assembly,legacy.Assembly);
        var currentProperties=current.GetProperties(BindingFlags.Instance|BindingFlags.Public);
        var legacyProperties=legacy.GetProperties(BindingFlags.Instance|BindingFlags.Public);
        var added=currentProperties.Where(value=>legacyProperties.All(old=>old.Name!=value.Name)).ToArray();
        var expectedAdded=entityName switch{
            "AR_PAYMENT"=>new[]{"BOOK_ID","CURRENCY_CODE","EVIDENCE_REFERENCE","EXTERNAL_REFERENCE","FIELD_ID","OWNER_COMPANY_BA_ID","OWNER_TENANT_ID","PAYER_BA_ID","PREPARATION_WORKFLOW_JSON","REVIEW_WORKFLOW_JSON","SOURCE_REVISION","STATUS"},
            "FX_RATE"=>new[]{"ALLOCATION_POLICY","AUTHORITY_REFERENCE","BOOK_ID","BOOK_NATIVE_REVISION","CONFIGURATION_JSON","EFFECTIVE_FROM","EFFECTIVE_THROUGH","OBSERVED_AT_UTC","OWNER_COMPANY_BA_ID","OWNER_TENANT_ID","PROVENANCE_REFERENCE","PROVIDER_NAME","RATE_DECIMALS","RATE_DIRECTION","RATE_VERSION"},
            "GL_ACCOUNT"=>new[]{"BOOK_ID","CONFIGURATION_COMMAND_JSON","CURRENCY_CODE","OWNER_COMPANY_BA_ID","OWNER_TENANT_ID"},
            "JOURNAL_ENTRY"=>new[]{"BOOK_ID","COMPANY_LEDGER_PERIOD_ID","CURRENCY_CODE","FIELD_ID","OWNER_COMPANY_BA_ID","OWNER_TENANT_ID","POSTING_WORKFLOW_JSON","PREPARATION_WORKFLOW_JSON","REVIEW_WORKFLOW_JSON","SALES_JOURNAL_POSTING_INTENT_ID","SOURCE_AR_INVOICE_ID"},
            "GL_ENTRY"=>new[]{"BOOK_ID","COMPANY_LEDGER_PERIOD_ID","CURRENCY_CODE","FIELD_ID","JOURNAL_ENTRY_LINE_ID","MAPPING_KEY","OWNER_COMPANY_BA_ID","OWNER_TENANT_ID","SALES_JOURNAL_POSTING_INTENT_ID","SOURCE_AR_INVOICE_ID"},
            "JOURNAL_ENTRY_LINE"=>new[]{"BOOK_ID","COMPANY_LEDGER_PERIOD_ID","FIELD_ID","MAPPING_KEY","OWNER_COMPANY_BA_ID","OWNER_TENANT_ID","SOURCE_AR_INVOICE_ID"},_=>Array.Empty<string>()};
        Assert.Equal(expectedAdded.Order(StringComparer.Ordinal),added.Select(value=>value.Name).Order(StringComparer.Ordinal));
        foreach(var property in added)
        {
            var expectedType=entityName=="FX_RATE" && property.Name is "EFFECTIVE_FROM" or "EFFECTIVE_THROUGH" or "OBSERVED_AT_UTC"?typeof(DateTime?):entityName=="FX_RATE" && property.Name=="RATE_DECIMALS"?typeof(int?):typeof(string);
            Assert.Equal(expectedType,property.PropertyType);
            Assert.Equal(System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,property.GetCustomAttribute<System.Text.Json.Serialization.JsonIgnoreAttribute>()!.Condition);
            if(expectedType==typeof(string))Assert.NotNull(property.GetCustomAttribute<System.ComponentModel.DataAnnotations.MaxLengthAttribute>());
        }
        Assert.Equal(legacyProperties.Select(value=>value.Name).Order(StringComparer.Ordinal),currentProperties.Where(value=>added.All(extra=>extra.Name!=value.Name)).Select(value=>value.Name).Order(StringComparer.Ordinal));
        var currentRow=Activator.CreateInstance(current)!;var legacyRow=Activator.CreateInstance(legacy)!;
        foreach(var property in legacyProperties.Where(value=>value.CanWrite && value.GetIndexParameters().Length==0))
        {
            var primitive=Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            object? value=primitive==typeof(string)?"legacy-"+property.Name:primitive==typeof(decimal)?123.45m:primitive==typeof(DateTime)?new DateTime(2026,10,12,13,14,15,DateTimeKind.Utc):primitive==typeof(int)?7:primitive==typeof(bool)?true:null;
            if(value is null)continue;
            property.SetValue(legacyRow,value);current.GetProperty(property.Name)!.SetValue(currentRow,value);
        }
        Assert.Equal(JsonSerializer.Serialize(legacyRow,legacy),JsonSerializer.Serialize(currentRow,current));
    }
#endif
}
