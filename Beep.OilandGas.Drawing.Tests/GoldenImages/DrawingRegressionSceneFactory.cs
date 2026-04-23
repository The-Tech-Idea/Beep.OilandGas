using Beep.OilandGas.Drawing.Samples;

namespace Beep.OilandGas.Drawing.Tests.GoldenImages;

public static class DrawingRegressionSceneFactory
{
    public static IReadOnlyList<GoldenImageSnapshot> CreateSnapshots()
    {
        return new[]
        {
            new GoldenImageSnapshot("FieldMap_AssetNetwork", 1600, 1000, "1A2A45E59A1F21E52282A4CD23E02138AE56959C7036D4FB13E868B9CAF00969", DrawingSampleGallery.CreateFieldMapAssetNetworkScene),
            new GoldenImageSnapshot("WellLog_Petrophysical", 1500, 1800, "E83C220D392747C622BEC5ABDCA2575F770AE3752F7F3C6E853C8A667665E6FB", DrawingSampleGallery.CreateWellLogPetrophysicalScene),
            new GoldenImageSnapshot("ReservoirContour_StructureMap", 1600, 1000, "D183358F98AF3AF9A0AC566DA4CCC29D64CC36685A47C183352F64BFAB56FDF8", DrawingSampleGallery.CreateReservoirContourStructureMapScene),
            new GoldenImageSnapshot("ReservoirCrossSection_Midline", 1600, 900, "53AE131F45D0C3F87ED70CBD1B5311C58CF1F4B7131ADC09007B8EF5D4177A69", DrawingSampleGallery.CreateReservoirCrossSectionMidlineScene),
            new GoldenImageSnapshot("WellSchematic_EnhancedVertical", 900, 1600, "7D986DF974F60801AA6F55E95E17F04EB54CAE0C35DB224B4483E1E64EB82470", DrawingSampleGallery.CreateWellSchematicEnhancedVerticalScene)
        };
    }
}