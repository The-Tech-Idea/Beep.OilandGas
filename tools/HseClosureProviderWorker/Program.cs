using Beep.OilandGas.Repository.Tests;
if (args.Length != 2) throw new ArgumentException("Expected selected HSE closure stage and isolated directory.");
await CurrentFieldLifecycleGateTests.RunHseClosureRaceStageAsync(args[0], args[1]);
return 0;
