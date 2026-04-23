using Beep.OilandGas.Drawing.Core;

namespace Beep.OilandGas.Drawing.Tests.GoldenImages;

public sealed record GoldenImageSnapshot(
    string Name,
    int ExpectedWidth,
    int ExpectedHeight,
    string ExpectedSha256,
    Func<DrawingEngine> CreateEngine);