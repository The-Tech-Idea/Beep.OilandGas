using System.Runtime.CompilerServices;

// Financial feature implementations share internal evidence validation with their contract owner.
// These helpers never cache authority or datasource reads.
[assembly: InternalsVisibleTo("Beep.OilandGas.Accounting")]
[assembly: InternalsVisibleTo("Beep.OilandGas.ProductionAccounting")]

[assembly: InternalsVisibleTo("Beep.OilandGas.Repository.Tests")]
