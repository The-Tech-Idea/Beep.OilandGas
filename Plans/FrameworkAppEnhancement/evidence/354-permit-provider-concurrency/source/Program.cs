using Beep.OilandGas.Repository.Tests;

if (args.Length != 2) throw new ArgumentException("Expected permit qualification stage and isolated directory.");
if (args[0].StartsWith("race-", StringComparison.Ordinal))
    await PermitProviderQualificationTests.RunRaceStageAsync(args[0], args[1]);
else
    await PermitProviderQualificationTests.RunRestartStageAsync(args[0], args[1]);
// A non-success transport result after commit leaves the caller with its retained original only.
return args[0] == "commit" ? 73 : 0;
