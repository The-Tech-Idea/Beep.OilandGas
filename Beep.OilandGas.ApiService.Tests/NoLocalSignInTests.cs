using System.Text.RegularExpressions;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

/// <summary>
/// S3-06 §6. OilGas signs nobody in and issues no token: people sign in at Beep.IdentityServer, and the API only validates
/// what it issued. A local login — a password hash checked here, a token minted here, the password grant asked for — is
/// what was removed (<c>AuthService</c>, <c>MfaService</c>, the client library's <c>CredentialsAuthenticationProvider</c>), and
/// this keeps it removed.
/// </summary>
/// <remarks>
/// <b>Blind spot:</b> a source scan by name. A token built by hand from JSON and a signature, or a password checked through
/// a type these names do not cover, passes. Test projects are not scanned: they mint tokens for the API to validate.
/// </remarks>
public sealed class NoLocalSignInTests
{
    private static readonly Regex SignInHere = new(
        @"\b(JwtSecurityTokenHandler|JsonWebTokenHandler|SecurityTokenDescriptor|PasswordHasher<|IPasswordHasher<|CheckPasswordAsync|PasswordSignInAsync|Rfc2898DeriveBytes)\b"
        + @"|grant_type\s*=\s*password|GrantTypes\.Password",
        RegexOptions.Compiled);

    [Fact]
    public void No_OilGas_code_signs_anybody_in_or_mints_a_token()
    {
        var root = RepositoryRoot();
        var offenders = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(path => !IsExcluded(Path.GetRelativePath(root, path)))
            .SelectMany(path => File.ReadLines(path).Select((line, index) => (path, line, number: index + 1)))
            .Where(entry => !entry.line.TrimStart().StartsWith("//", StringComparison.Ordinal)
                && !entry.line.TrimStart().StartsWith("///", StringComparison.Ordinal)
                && !entry.line.TrimStart().StartsWith("*", StringComparison.Ordinal)
                && SignInHere.IsMatch(entry.line))
            .Select(entry => $"{Path.GetRelativePath(root, entry.path)}:{entry.number}")
            .ToList();

        Assert.Empty(offenders);
    }

    /// <summary>The scan can fail: each name it looks for is recognised in a line of code.</summary>
    [Theory]
    [InlineData("var handler = new JwtSecurityTokenHandler();")]
    [InlineData("return new JsonWebTokenHandler().CreateToken(descriptor);")]
    [InlineData("var descriptor = new SecurityTokenDescriptor { };")]
    [InlineData("private readonly IPasswordHasher<USER> _hasher;")]
    [InlineData("if (!await users.CheckPasswordAsync(user, password)) return null;")]
    [InlineData("var body = \"grant_type=password&username=\" + name;")]
    [InlineData("options.GrantType = OpenIddictConstants.GrantTypes.Password;")]
    public void The_scan_recognises_what_it_looks_for(string line) => Assert.Matches(SignInHere, line);

    private static bool IsExcluded(string relative)
    {
        var parts = relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return parts.Any(part => part is "bin" or "obj" or "_tmpbuild" or "MudBlazor_Docs")
            || parts[0].EndsWith(".Tests", StringComparison.Ordinal);
    }

    private static string RepositoryRoot()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "Beep.OilandGas.sln")))
            root = root.Parent;
        Assert.NotNull(root);
        return root.FullName;
    }
}
