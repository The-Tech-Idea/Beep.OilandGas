using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
using System.Reflection;
using System.Text.Json;
using Xunit;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class CompanyFinancialCanonicalOwnershipTests
{
    [Theory]
    [InlineData("Beep.OilandGas.Accounting.Models.CreateGLAccountRequest","Beep.OilandGas.Accounting")]
    [InlineData("Beep.OilandGas.Accounting.Models.UpdateGLAccountRequest","Beep.OilandGas.Accounting")]
    [InlineData("Beep.OilandGas.Accounting.Models.GLAccount","Beep.OilandGas.Accounting")]
    [InlineData("Beep.OilandGas.Accounting.Models.GLAccountResponse","Beep.OilandGas.Accounting")]
    [InlineData("Beep.OilandGas.Accounting.Models.CreateJournalEntryRequest","Beep.OilandGas.Accounting")]
    [InlineData("Beep.OilandGas.Accounting.Models.JournalEntryLineData","Beep.OilandGas.Accounting")]
    [InlineData("Beep.OilandGas.Accounting.Models.JournalEntry","Beep.OilandGas.Accounting")]
    [InlineData("Beep.OilandGas.Accounting.Models.JournalEntryLine","Beep.OilandGas.Accounting")]
    [InlineData("Beep.OilandGas.Accounting.Models.JournalEntryApprovalResult","Beep.OilandGas.Accounting")]
    [InlineData("Beep.OilandGas.Models.Data.ProductionAccounting.JournalPostingEvidence","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.CompanyGLAccountReview","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.CompanyGLMappingReview","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Accounting.Models.CompanyGLAccountConfigurationCommand","Beep.OilandGas.Accounting")]
    [InlineData("Beep.OilandGas.Accounting.Models.CompanyGLAccountConfigurationSubmission","Beep.OilandGas.Accounting")]
    [InlineData("Beep.OilandGas.Accounting.Models.CompanyGLAccountConfigurationReceipt","Beep.OilandGas.Accounting")]
    [InlineData("Beep.OilandGas.Accounting.Models.CompanyGLAccountConfigurationReview","Beep.OilandGas.Accounting")]
    [InlineData("Beep.OilandGas.Accounting.Models.CompanyGLMappingConfigurationCommand","Beep.OilandGas.Accounting")]
    [InlineData("Beep.OilandGas.Accounting.Models.CompanyGLMappingConfigurationReview","Beep.OilandGas.Accounting")]
    [InlineData("Beep.OilandGas.Accounting.Models.CompanyGLMappingConfigurationSubmission","Beep.OilandGas.Accounting")]
    [InlineData("Beep.OilandGas.Accounting.Models.CompanyGLMappingConfigurationReceipt","Beep.OilandGas.Accounting")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.CompanyLedgerPeriodReview","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.CompanyLedgerBookFacts","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.CompanyLedgerBookReview","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.CompanyLedgerBookConfigurationCommand","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.CompanyLedgerBookConfigurationReview","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.CompanyLedgerBookConfigurationSubmission","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.CompanyLedgerBookConfigurationReceipt","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.CompanyFxRateFacts","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.CompanyFxRateReview","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.CompanyFxRateWindow","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Accounting.Models.CompanySalesPaymentFacts","Beep.OilandGas.Accounting")]
    [InlineData("Beep.OilandGas.Accounting.Models.CompanySalesPaymentSource","Beep.OilandGas.Accounting")]
    [InlineData("Beep.OilandGas.Accounting.Models.CompanySalesPaymentCommand","Beep.OilandGas.Accounting")]
    [InlineData("Beep.OilandGas.Accounting.Models.CompanySalesPaymentReceipt","Beep.OilandGas.Accounting")]
    [InlineData("Beep.OilandGas.Accounting.Models.CompanySalesPaymentReviewSummary","Beep.OilandGas.Accounting")]
    [InlineData("Beep.OilandGas.Accounting.Models.CompanySalesPaymentReviewSource","Beep.OilandGas.Accounting")]
    [InlineData("Beep.OilandGas.Accounting.Models.CompanySalesPaymentReviewCommand","Beep.OilandGas.Accounting")]
    [InlineData("Beep.OilandGas.Accounting.Models.CompanySalesPaymentReviewProof","Beep.OilandGas.Accounting")]
    [InlineData("Beep.OilandGas.Accounting.Models.CompanySalesPaymentReviewRecovery","Beep.OilandGas.Accounting")]
    [InlineData("Beep.OilandGas.Accounting.Models.CompanySalesPaymentWorkflowSource","Beep.OilandGas.Accounting")]
    [InlineData("Beep.OilandGas.Accounting.Models.CompanySalesPaymentProcessSourceReview","Beep.OilandGas.Accounting")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.CompanyFxRateConfigurationCommand","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.CompanyFxRateConfigurationReview","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.CompanyFxRateConfigurationSubmission","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.CompanyFxRateReceipt","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.CompanyFxConvertedComponent","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.CompanyFxConversion","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.CompanyLedgerPeriodCreation","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.CompanyLedgerPeriodCreationSubmission","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.CompanyLedgerPeriodCreationReceipt","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.CompanyLedgerPeriodFacts","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.CompanyLedgerPeriodSource","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.CompanyLedgerPeriodConfigurationReview","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.CompanySalesJournalMappings","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.CompanySalesJournalSourceLine","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.CompanySalesJournalSourceReview","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.CompanySalesJournalPreparation","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.CompanySalesJournalLineRevision","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.CompanySalesJournalPreparationProof","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.CompanySalesJournalPreparationOutcome","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Accounting.Models.CompanySalesJournalWorkflowSource","Beep.OilandGas.Accounting")]
    [InlineData("Beep.OilandGas.Accounting.Models.CompanySalesPostedLedgerLine","Beep.OilandGas.Accounting")]
    [InlineData("Beep.OilandGas.Accounting.Models.CompanySalesPostedLedgerJournal","Beep.OilandGas.Accounting")]
    [InlineData("Beep.OilandGas.Accounting.Models.CompanySalesPostedLedgerReview","Beep.OilandGas.Accounting")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.CompanySalesJournalPostingHeaderProof","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.CompanySalesJournalPostingOutcome","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.CompanySalesJournalPostingRecovery","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.CompanySalesJournalPostingSource","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.CompanySalesJournalPostingCommand","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.CompanySalesJournalReviewSource","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.CompanySalesJournalReviewCommand","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.CompanySalesJournalReviewProof","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.CompanySalesJournalReviewRecovery","Beep.OilandGas.Models")]
    public void SharedFinancialContractHasCanonicalOwnerAndCannotBecomeABusinessTable(string name,string expectedOwner)
    {
        var assembly=Assembly.Load(expectedOwner);
        var contract=assembly.GetType(name,throwOnError:true)!;
        Assert.Equal(assembly,contract.Assembly);
        Assert.Equal(name,contract.FullName);
        Assert.Throws<ArgumentException>(()=>ModuleSchemaBoundary.Validate([contract]));
    }

    [Theory]
    [InlineData("Beep.OilandGas.Models.Data.ProductionAccounting.GL_ACCOUNT","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.ProductionAccounting.JOURNAL_ENTRY","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.ProductionAccounting.JOURNAL_ENTRY_LINE","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.ProductionAccounting.GL_ENTRY","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.ProductionAccounting.AR_PAYMENT","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.ProductionAccounting.AR_CREDIT_MEMO","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.ProductionAccounting.FX_RATE","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.GLAccountMapping","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.COMPANY_LEDGER_PERIOD","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.COMPANY_LEDGER_BOOK","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Accounting.Models.SALES_JOURNAL_POSTING_OUTCOME","Beep.OilandGas.Accounting")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.SALES_JOURNAL_POSTING_INTENT","Beep.OilandGas.Models")]
    [InlineData("Beep.OilandGas.Accounting.Models.SALES_PAYMENT_REVIEW_INTENT","Beep.OilandGas.Accounting")]
    [InlineData("Beep.OilandGas.Models.Data.Accounting.SALES_JOURNAL_REVIEW_INTENT","Beep.OilandGas.Models")]
    public void FinancialEntityHasOneModuleOwnerAndPreservesNativeName(string name,string expectedOwner)
    {
        var assembly=Assembly.Load(expectedOwner);
        var entity=assembly.GetType(name,throwOnError:true)!;
        Assert.Equal(name,entity.FullName);
        Assert.Equal(assembly,entity.Assembly);
        Assert.Equal(typeof(Beep.OilandGas.Models.Data.ModelEntityBase).Assembly,entity.BaseType!.Assembly);
        Assert.Equal("Beep.OilandGas.Models.Data.ModelEntityBase",entity.BaseType.FullName);
        Assert.NotNull(Activator.CreateInstance(entity));
        ModuleSchemaBoundary.Validate([entity]);
    }

    [Fact]
    public void CompanyMappingOwnershipFieldsAreBoundedNullableAndOmittedFromLegacyJson()
    {
        var entity=typeof(CompanyScope).Assembly.GetType("Beep.OilandGas.Models.Data.Accounting.GLAccountMapping",throwOnError:true)!;
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

}
