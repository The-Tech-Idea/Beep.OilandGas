using Xunit;

namespace Beep.OilandGas.Repository.Tests;

// Native HTTP acceptance has a real transport deadline. Run this collection without competing
// provider-heavy fixture classes; explicit concurrency exercised within a test remains intact.
[CollectionDefinition(CurrentCompanyHttpAcceptanceCollection.Name, DisableParallelization = true)]
public sealed class CurrentCompanyHttpAcceptanceCollection
{
    public const string Name = "Current company HTTP acceptance";
}
