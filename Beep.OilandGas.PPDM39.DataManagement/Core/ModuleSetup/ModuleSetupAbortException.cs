using System;
using System.Runtime.Serialization;

namespace Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup
{
    /// <summary>
    /// Thrown inside <see cref="IModuleSetup.SeedAsync"/> only when a module
    /// encounters an unrecoverable state (e.g. database connection is lost)
    /// and the entire seeding run must stop immediately.
    ///
    /// For ordinary row or table failures, append to
    /// <see cref="ModuleSetupResult.Errors"/> instead of throwing this exception.
    /// </summary>
    [Serializable]
    public sealed class ModuleSetupAbortException : Exception
    {
        public string ModuleId { get; }

        public ModuleSetupAbortException(string moduleId, string message)
            : base($"[{moduleId}] {message}")
        {
            ModuleId = moduleId;
        }

        public ModuleSetupAbortException(string moduleId, string message, Exception inner)
            : base($"[{moduleId}] {message}", inner)
        {
            ModuleId = moduleId;
        }
    }
}
