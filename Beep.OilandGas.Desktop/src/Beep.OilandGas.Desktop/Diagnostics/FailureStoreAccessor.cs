namespace TheTechIdea.Beep.OilandGas.Desktop.Diagnostics;

/// <summary>Holds the one failure store instance for the process.</summary>
public sealed class FailureStoreAccessor : IFailureStoreAccessor
{
    public FailureStoreAccessor(IOgFailureStore store)
    {
        Store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public IOgFailureStore Store { get; }
}
