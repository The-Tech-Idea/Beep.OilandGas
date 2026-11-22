using TheTechIdea.Beep.OilandGas.Desktop.Headless;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// FND-01 AC-2: the mapping from command-line words to verbs, including the cases an operator actually
/// gets wrong — an unknown verb, a missing argument, and a stray flag after the first one.
/// </summary>
public sealed class OgCommandLineTests
{
    [Theory]
    [InlineData("--smoke", OgHeadlessVerb.Smoke)]
    [InlineData("--drivers-report", OgHeadlessVerb.DriversReport)]
    [InlineData("--entitlement-report", OgHeadlessVerb.EntitlementReport)]
    [InlineData("--licence-check", OgHeadlessVerb.LicenceCheck)]
    [InlineData("--restore", OgHeadlessVerb.Restore)]
    [InlineData("--schema-apply", OgHeadlessVerb.SchemaApply)]
    [InlineData("--schema-resume", OgHeadlessVerb.SchemaResume)]
    [InlineData("--SMOKE", OgHeadlessVerb.Smoke)]
    public void Each_documented_verb_is_recognised(string word, OgHeadlessVerb expected)
    {
        var command = OgCommandLine.Parse([word]);

        Assert.Equal(expected, command.Verb);
        Assert.True(command.IsHeadless);
    }

    [Fact]
    public void No_arguments_means_open_the_application_normally()
    {
        var command = OgCommandLine.Parse([]);

        Assert.Equal(OgHeadlessVerb.None, command.Verb);
        Assert.False(command.IsHeadless);
    }

    [Fact]
    public void A_non_flag_first_argument_does_not_become_a_verb()
    {
        var command = OgCommandLine.Parse(["report.txt"]);

        Assert.Equal(OgHeadlessVerb.None, command.Verb);
    }

    [Fact]
    public void An_unknown_flag_is_an_unknown_verb_not_a_normal_start()
    {
        var command = OgCommandLine.Parse(["--not-a-verb"]);

        Assert.Equal(OgHeadlessVerb.Unknown, command.Verb);
        Assert.True(command.IsHeadless);
    }

    [Fact]
    public void The_second_argument_is_the_verbs_argument()
    {
        var command = OgCommandLine.Parse(["--licence-check", @"C:\licences\acme.json"]);

        Assert.Equal(OgHeadlessVerb.LicenceCheck, command.Verb);
        Assert.Equal(@"C:\licences\acme.json", command.Argument);
        Assert.False(command.IsMissingArgument);
    }

    [Theory]
    [InlineData("--licence-check")]
    [InlineData("--restore")]
    public void A_file_verb_without_a_path_is_a_missing_argument(string verb)
    {
        var command = OgCommandLine.Parse([verb]);

        Assert.True(command.IsMissingArgument);
    }

    [Fact]
    public void A_flag_after_the_first_argument_does_not_change_the_verb()
    {
        // Otherwise a file literally named "--restore" (or a path containing it) could hijack the run.
        var command = OgCommandLine.Parse(["--licence-check", "--restore"]);

        Assert.Equal(OgHeadlessVerb.LicenceCheck, command.Verb);
        Assert.Equal("--restore", command.Argument);
    }

    [Fact]
    public void Describing_a_command_names_the_file_not_the_whole_path()
    {
        var command = OgCommandLine.Parse(["--licence-check", @"C:\private\licences\acme.json"]);

        Assert.Equal("LicenceCheck (acme.json)", command.Describe());
    }

    [Fact]
    public void The_usage_text_names_every_documented_verb()
    {
        foreach (var verb in new[]
                 {
                     "--smoke", "--drivers-report", "--entitlement-report", "--licence-check",
                     "--restore", "--schema-apply", "--schema-resume",
                 })
        {
            Assert.Contains(verb, OgCommandLine.Usage, StringComparison.Ordinal);
        }
    }
}
