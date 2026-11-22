using TheTechIdea.Beep.OilandGas.Desktop.Core;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// SETUP-05 AC-6: the policy screens against a published list of the most common breached passwords, and the rule is
/// still length rather than composition — the screen must not quietly reintroduce character-class rules.
/// </summary>
public sealed class PasswordPolicyTests
{
    [Fact]
    public void The_published_list_is_embedded_and_loaded()
    {
        // A missing or empty resource would make every screen pass; the count is the proof it is there.
        Assert.True(
            OgPasswordPolicy.BreachedPasswordCount >= 9_000,
            $"the embedded list held {OgPasswordPolicy.BreachedPasswordCount} entries");
    }

    [Theory]
    [InlineData("password")]
    [InlineData("123456")]
    [InlineData("qwerty")]
    [InlineData("letmein")]
    [InlineData("dragon")]
    [InlineData("trustno1")]
    public void A_listed_password_is_refused_with_the_reason(string password)
    {
        // The list refuses it on its own; a short listed password also fails the length rule, and both reasons are
        // reported so the person knows what to change.
        Assert.True(OgPasswordPolicy.IsBreached(password), $"'{password}' should be in the embedded list");

        var problems = OgPasswordPolicy.Problems(password);

        Assert.Contains(problems, problem => problem.Contains("breached", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void The_screen_ignores_case()
    {
        Assert.True(OgPasswordPolicy.IsBreached("PASSWORD"));
        Assert.True(OgPasswordPolicy.IsBreached("QwErTy"));
        Assert.False(OgPasswordPolicy.IsBreached("correct horse battery staple"));
    }

    [Fact]
    public void An_unlisted_passphrase_is_accepted()
    {
        const string passphrase = "correct horse battery staple";

        Assert.False(OgPasswordPolicy.IsBreached(passphrase));
        Assert.Empty(OgPasswordPolicy.Problems(passphrase));
    }

    [Fact]
    public void No_composition_rule_is_reintroduced()
    {
        // Lower case only, no digits, no symbols, one long sentence: allowed, because length is the rule.
        const string passphrase = "the quick brown fox jumps over the lazy dog";

        Assert.True(passphrase.Length >= OgPasswordPolicy.MinimumLength);
        Assert.Empty(OgPasswordPolicy.Problems(passphrase));

        // A long password that is not on the list passes even though it has no symbol.
        Assert.Empty(OgPasswordPolicy.Problems("abcdefghijklmnop"));
    }

    [Fact]
    public void Length_and_digits_only_rules_still_apply()
    {
        Assert.NotEmpty(OgPasswordPolicy.Problems("short"));

        var overLong = new string('a', OgPasswordPolicy.MaximumLength + 1);
        Assert.NotEmpty(OgPasswordPolicy.Problems(overLong));

        Assert.NotEmpty(OgPasswordPolicy.Problems(new string('7', 18)));
    }

    [Fact]
    public void A_passphrase_that_merely_contains_a_listed_password_is_allowed()
    {
        // "password" is listed; "password-not-1" is not. Rejecting every passphrase that contains a common word
        // would punish exactly the people who followed the advice to lengthen it.
        Assert.False(OgPasswordPolicy.IsBreached("password-not-1"));
    }
}
