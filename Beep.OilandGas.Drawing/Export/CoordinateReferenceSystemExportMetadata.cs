using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Drawing.CoordinateSystems;

namespace Beep.OilandGas.Drawing.Export
{
    internal static class CoordinateReferenceSystemExportMetadata
    {
        public static object Create(CoordinateReferenceSystem coordinateReferenceSystem)
        {
            if (coordinateReferenceSystem == null)
                return null;

            return new Dictionary<string, object>
            {
                ["identifier"] = coordinateReferenceSystem.Identifier,
                ["name"] = coordinateReferenceSystem.Name,
                ["authority"] = coordinateReferenceSystem.Authority.ToString(),
                ["kind"] = coordinateReferenceSystem.Kind.ToString(),
                ["axes"] = coordinateReferenceSystem.Axes
                    .Select(axis => new Dictionary<string, object>
                    {
                        ["kind"] = axis.Kind.ToString(),
                        ["name"] = axis.Name,
                        ["unit"] = axis.Unit.Code,
                        ["unitDisplayName"] = axis.Unit.DisplayName,
                        ["isInverted"] = axis.IsInverted
                    })
                    .ToList()
            };
        }
    }
}