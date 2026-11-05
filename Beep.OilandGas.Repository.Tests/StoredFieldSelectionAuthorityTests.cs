using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Repository.Models;
using System.Text.Json.Nodes;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.PPDM39.Core;
using Microsoft.EntityFrameworkCore;
using Xunit;
namespace Beep.OilandGas.Repository.Tests;
public sealed partial class CurrentFieldLifecycleGateTests
{
    [Theory][InlineData("allowed")][InlineData("revision")][InlineData("field")][InlineData("disabled")]
    [InlineData("missing-profile")][InlineData("malformed-profile")][InlineData("actor")][InlineData("duplicate-selection")]
    public async Task StoredFieldAuthorityRechecksExistingPreferenceInsideLifecycleWithoutNativeReads(string fault)
    {
        using var f=await Ready();var profile=Profile(f);await Save(profile);var review=(await profile.GetCurrentFieldSelectionAsync()).SelectionContext!;
        if(fault=="revision"){f.Db.Users.Single().ConcurrencyStamp="changed";await f.Db.SaveChangesAsync();}
        if(fault=="disabled"){f.Db.Users.Single().IsActive=false;await f.Db.SaveChangesAsync();}
        if(fault=="field")
        {
            var extension=f.Db.Set<AppUserExtension>().Single();var root=JsonNode.Parse(extension.PreferencesJson!)!.AsObject();root["OilGasFieldSelections"]![0]!["fieldId"]="B";extension.PreferencesJson=root.ToJsonString();await f.Db.SaveChangesAsync();
        }
        if(fault=="missing-profile"){f.Db.Remove(f.Db.Set<AppUserExtension>().Single());await f.Db.SaveChangesAsync();}
        if(fault=="malformed-profile"){f.Db.Set<AppUserExtension>().Single().PreferencesJson="{";await f.Db.SaveChangesAsync();}
        if(fault is "actor" or "duplicate-selection")
        {
            var extension=f.Db.Set<AppUserExtension>().Single();var root=JsonNode.Parse(extension.PreferencesJson!)!.AsObject();var selections=root["OilGasFieldSelections"]!.AsArray();
            if(fault=="actor")selections[0]!["actorUserId"]="another-account";else selections.Add(selections[0]!.DeepClone());
            extension.PreferencesJson=root.ToJsonString();await f.Db.SaveChangesAsync();
        }
        f.EnableCorrectiveActionTransactions();var reads=f.Source.Invocations.Count(call=>call.Method.Name=="GetEntityAsync");
        if(fault=="allowed")await PPDMDataSourceTransaction.ExecuteAsync(f.Source.Object,()=>profile.RequireStoredSelectionAsync(review));
        else
        {
            var error=await Assert.ThrowsAnyAsync<Exception>(()=>PPDMDataSourceTransaction.ExecuteAsync(f.Source.Object,()=>profile.RequireStoredSelectionAsync(review)));
            if(fault is "missing-profile" or "actor" or "malformed-profile" or "duplicate-selection")
                Assert.Equal(RefusalKind.Conflict,Assert.IsType<RefusalException>(error).Kind);
        }
        Assert.Equal(reads,f.Source.Invocations.Count(call=>call.Method.Name=="GetEntityAsync"));Assert.Empty(f.Writes);Assert.Null(f.LeaseActiveTransaction);
    }
}
