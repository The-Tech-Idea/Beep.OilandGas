using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;

public interface IFieldObligationIntentStore
{
    Task<CreateFieldObligation?> ReadAsync(FieldSelectionReview context);
    Task RetainAsync(CreateFieldObligation request);
    Task ClearAsync(CreateFieldObligation request);
}

/// <summary>Retains the original request in the protected browser session; it is not proof of a committed write.</summary>
public sealed class FieldObligationIntentStore(ProtectedSessionStorage storage) : IFieldObligationIntentStore
{
    private readonly SemaphoreSlim gate = new(1, 1);
    private static string Key(FieldSelectionReview context) => "oilgas.field.obligation." + Convert.ToHexString(SHA256.HashData(
        JsonSerializer.SerializeToUtf8Bytes(new { context.Company, context.ActorUserId, context.Database, context.FieldId })));
    private static void Require(CreateFieldObligation request)
    {
        ArgumentNullException.ThrowIfNull(request); ArgumentNullException.ThrowIfNull(request.Context);
        var context = request.Context; FieldSelectionReviewValidator.RequireReview(context, context.Company, context.ActorUserId);
        if (!LeaseSummaryValidator.Identifier(context.FieldId) || request.IntentId == Guid.Empty ||
            !LeaseSummaryValidator.Identifier(request.ObligationType) || !LeaseSummaryValidator.Identifier(request.ComponentType) ||
            string.IsNullOrWhiteSpace(request.Description) || request.Description != request.Description.Trim() ||
            request.Description.Length > 255 || request.Description.Any(char.IsControl) || request.CriticalDate is { Kind: not DateTimeKind.Unspecified })
            throw new InvalidOperationException("The original obligation request is invalid.");
    }
    public async Task<CreateFieldObligation?> ReadAsync(FieldSelectionReview context)
    {
        FieldSelectionReviewValidator.RequireReview(context, context.Company, context.ActorUserId);
        var saved = await storage.GetAsync<CreateFieldObligation>(Key(context)); if (!saved.Success) return null;
        var request = saved.Value ?? throw new InvalidOperationException("The retained obligation request is unavailable."); Require(request);
        if (request.Context.Company != context.Company || request.Context.ActorUserId != context.ActorUserId ||
            request.Context.Database != context.Database || request.Context.FieldId != context.FieldId)
            throw new InvalidOperationException("The retained obligation belongs to another account or FIELD.");
        return request;
    }
    public async Task RetainAsync(CreateFieldObligation request)
    {
        Require(request); await gate.WaitAsync();
        try
        {
            var existing = await ReadAsync(request.Context);
            if (existing is not null && existing != request) throw new InvalidOperationException("Recover the original obligation request before starting another.");
            await storage.SetAsync(Key(request.Context), request);
            if (await ReadAsync(request.Context) != request) throw new InvalidOperationException("The obligation request was not retained.");
        }
        finally { gate.Release(); }
    }
    public async Task ClearAsync(CreateFieldObligation request)
    {
        Require(request); await gate.WaitAsync();
        try
        {
            var existing = await ReadAsync(request.Context);
            if (existing is not null && existing != request) throw new InvalidOperationException("The retained obligation request changed.");
            await storage.DeleteAsync(Key(request.Context));
        }
        finally { gate.Release(); }
    }
}
