using Xunit;

// Several test classes change the ambient WellboreEnvironment (WellboreServices.Global / Scope) and several
// write under the same data root. xUnit runs classes in parallel by default, so one class's Scope could restore
// an environment another class had already replaced — the suite then failed intermittently in a different test
// each run. Parallelism is disabled deliberately: the suite takes seconds either way, and a deterministic
// result is worth more than the overlap.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
