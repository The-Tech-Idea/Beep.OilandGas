using System.Security.Cryptography;
using Beep.OilandGas.Drawing.Core;
using SkiaSharp;
using Xunit;

namespace Beep.OilandGas.Drawing.Tests.GoldenImages;

public static class GoldenImageAssert
{
    public static void AssertMatches(GoldenImageSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        using DrawingEngine engine = snapshot.CreateEngine();
        using SKImage image = engine.RenderToImage();
        using SKData encoded = image.Encode(SKEncodedImageFormat.Png, 100);

        byte[] pngBytes = encoded.ToArray();
        string actualHash = Convert.ToHexString(SHA256.HashData(pngBytes));

        Assert.Equal(snapshot.ExpectedWidth, image.Width);
        Assert.Equal(snapshot.ExpectedHeight, image.Height);

        if (string.Equals(snapshot.ExpectedSha256, "PENDING", StringComparison.OrdinalIgnoreCase))
        {
            string artifactPath = PersistFailureArtifact(snapshot.Name, pngBytes, actualHash);
            Assert.Fail(
                $"Golden hash for '{snapshot.Name}' is not set. Actual SHA256: {actualHash}. Actual PNG written to {artifactPath}.");
        }

        if (!string.Equals(snapshot.ExpectedSha256, actualHash, StringComparison.OrdinalIgnoreCase))
        {
            string artifactPath = PersistFailureArtifact(snapshot.Name, pngBytes, actualHash);
            Assert.Fail(
                $"Golden image mismatch for '{snapshot.Name}'. Expected SHA256: {snapshot.ExpectedSha256}. Actual SHA256: {actualHash}. Actual PNG written to {artifactPath}.");
        }
    }

    private static string PersistFailureArtifact(string snapshotName, byte[] pngBytes, string actualHash)
    {
        string outputDirectory = Path.Combine(AppContext.BaseDirectory, "GoldenImageFailures");
        Directory.CreateDirectory(outputDirectory);

        string sanitizedName = string.Concat(snapshotName.Select(character =>
            Path.GetInvalidFileNameChars().Contains(character) ? '_' : character));

        string imagePath = Path.Combine(outputDirectory, sanitizedName + ".png");
        string hashPath = Path.Combine(outputDirectory, sanitizedName + ".sha256.txt");

        File.WriteAllBytes(imagePath, pngBytes);
        File.WriteAllText(hashPath, actualHash + Environment.NewLine);

        return imagePath;
    }
}