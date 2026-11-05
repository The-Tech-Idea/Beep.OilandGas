using System.Runtime.CompilerServices;
using Beep.OilandGas.Models.Enums.Shared;

// The canonical enum is Data-owned; existing Models consumers keep their type identity.
[assembly: TypeForwardedTo(typeof(PermitToWorkType))]
