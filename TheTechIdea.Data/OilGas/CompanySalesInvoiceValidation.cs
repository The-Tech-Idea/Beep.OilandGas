using System.Text.Json;
namespace TheTechIdea.Data.OilGas;

/// <summary>Deduplicates identical canonical invoice evidence validation inside one operation. Never caches authorization or datasource reads.</summary>
public static class CompanySalesInvoiceValidation
{
    // Bounded hashes cover a complete multi-owner financial capture; neither authority nor native reads are cached.
    private const int MaximumEvidenceKeys=512;
    private static readonly AsyncLocal<HashSet<string>?> Current=new();
    public static IDisposable Enter()
    {
        if(Current.Value is not null)return new Scope(false);
        Current.Value=new(StringComparer.Ordinal);return new Scope(true);
    }
    internal static string Key(string contract,object evidence,object? context=null)=>contract+":"+Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new{evidence,context})));
    internal static bool Contains(string key){if(Current.Value is not {} entries)return false;lock(entries)return entries.Contains(key);}
    internal static void Remember(string key){if(Current.Value is not {} entries)return;lock(entries){if(entries.Count<MaximumEvidenceKeys)entries.Add(key);}}
    private sealed class Scope(bool owns):IDisposable
    {
        private bool disposed;
        public void Dispose(){if(disposed)return;disposed=true;if(owns)Current.Value=null;}
    }
}

