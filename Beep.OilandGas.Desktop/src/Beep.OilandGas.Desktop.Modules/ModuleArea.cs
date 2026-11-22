namespace TheTechIdea.Beep.OilandGas.Desktop.Modules;

/// <summary>
/// Marker for the feature-page assembly.
/// </summary>
/// <remarks>
/// Stage 0 hosts the eight catalog pages in the host assembly to prove the shell, the route guard and the
/// view factory before there are 130 of them. From STG-01 each feature's <c>Views/</c> and
/// <c>ViewModels/</c> move here, one folder per area, and the host keeps only the shell, the setup
/// wizard and the sign-in journey. The reference direction is already fixed (Modules → Core, Controls),
/// so the move changes file locations only.
/// </remarks>
public static class ModuleArea
{
    public const string AssemblyName = "TheTechIdea.Beep.OilandGas.Desktop.Modules";
}
