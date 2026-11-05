#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.ProductionAccounting.Services;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class LegacyFxRefusalTests
{
    [Theory][InlineData("missing")][InlineData("invalid")][InlineData("company")][InlineData("partial")]
    public async Task MissingInvalidOrCompanyMarkedRatesCannotBecomeGlobalRateOne(string fault)
    {
        var f=new CompanyGLAccountReviewTests.Fixture();f.OverrideRows=fault=="missing"?[]:[new FX_RATE{FX_RATE_ID="recorded",FROM_CURRENCY="USD",TO_CURRENCY="KWD",RATE_DATE=new DateTime(2026,10,12),RATE=fault=="invalid"?0:0.3m,OWNER_TENANT_ID=fault=="company"?"tenant":null,RATE_DIRECTION=fault=="partial"?"TARGET_PER_SOURCE":null}];var metadata=new Mock<IPPDMMetadataRepository>();metadata.Setup(value=>value.GetTableMetadataAsync(It.IsAny<string>())).ReturnsAsync((string name)=>new PPDMTableMetadata{TableName=name,EntityTypeName=name,PrimaryKeyColumn=name+"_ID"});var defaults=new Mock<IPPDM39DefaultsRepository>();defaults.Setup(value=>value.GetActiveIndicatorYes()).Returns("Y");var service=new FunctionalCurrencyService(f.Native.Editor.Object,new CommonColumnHandler(),defaults.Object,metadata.Object,NullLogger<FunctionalCurrencyService>.Instance);await Assert.ThrowsAnyAsync<Exception>(()=>service.TranslateAmountAsync(100,"USD","KWD",new DateTime(2026,10,12),"production"));
        var journal=new JournalEntryService(f.Native.Editor.Object,new CommonColumnHandler(),defaults.Object,metadata.Object,f.Service,NullLogger<JournalEntryService>.Instance);var legacy=new CurrencyTranslationService(f.Native.Editor.Object,new CommonColumnHandler(),defaults.Object,metadata.Object,new AccountingBasisPostingService(journal),NullLogger<CurrencyTranslationService>.Instance);await Assert.ThrowsAnyAsync<Exception>(()=>legacy.TranslateAmountAsync(100,"USD","KWD",new DateTime(2026,10,12),"production"));f.NoWrites();
    }
    [Fact] public async Task SameCurrencyPreservesActualMoneyAndUnavailableBalancesNeverPersistPlaceholderRows()
    {
        var f=new CompanyGLAccountReviewTests.Fixture();var service=new FunctionalCurrencyService(f.Native.Editor.Object,new CommonColumnHandler(),Mock.Of<IPPDM39DefaultsRepository>(),Mock.Of<IPPDMMetadataRepository>(),NullLogger<FunctionalCurrencyService>.Instance);Assert.Equal(12.345m,await service.TranslateAmountAsync(12.345m,"KWD","KWD",new DateTime(2026,10,12),"production"));await Assert.ThrowsAsync<InvalidOperationException>(()=>service.TranslateBalancesAsync("entity",new DateTime(2026,10,12),"USD","production"));Assert.Equal(0,f.Reads);f.NoWrites();
    }
}
#endif
