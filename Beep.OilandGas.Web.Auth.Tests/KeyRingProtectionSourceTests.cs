using System.Text.RegularExpressions;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

/// <summary>
/// R7-W24, the 2026-10-02 key-ring rule: the Web's key ring protects its sign-in cookie and antiforgery tokens. It had none
/// of its own — the framework's default store, which a shared host's app pool without a profile keeps in memory, so each
/// recycle signed everybody out — so outside Development it now persists under App_Data and is encrypted at rest with the
/// PEM key in DataProtection:Key.
/// </summary>
/// <remarks>
/// <b>Blind spot:</b> read from <c>Program.cs</c> as <see cref="SignInSurfaceTests"/>' configuration arms are; that the shared
/// library then refuses a start without the key, and writes the keys encrypted, is its own <c>KeyRingProtectionTests</c>'
/// (Beep.Foundation.IdentityServer.Shared.Tests).
/// </remarks>
public sealed class KeyRingProtectionSourceTests
{
    [Fact]
    public void The_web_persists_its_key_ring_and_encrypts_it_with_its_pem_key()
    {
        var program = Regex.Replace(File.ReadAllText(Path.Combine(WebRoot(), "Program.cs")), @"//[^\n]*", " ");

        Assert.Contains(".SetApplicationName(\"Beep.OilandGas.Web\")", program, StringComparison.Ordinal);
        Assert.Contains("PersistKeysToFileSystem(", program, StringComparison.Ordinal);
        Assert.Matches(@"ProtectKeysWithPemKey\(builder\.Configuration, builder\.Environment, new KeyRingIdentity\(""CN=Beep Oil and Gas key ring""", program);
    }

    private static string WebRoot([System.Runtime.CompilerServices.CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Beep.OilandGas.Web"));
}
