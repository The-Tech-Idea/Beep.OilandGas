using System.Numerics;

namespace Beep.OilandGas.Models.Data.Accounting;

/// <summary>Diagnostic quantity arithmetic that never invents a rounded amount or uses overflow as an ordinary answer.</summary>
public static class ExactDecimalArithmetic
{
    public static decimal? SumOrUnavailable(IEnumerable<decimal> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        BigInteger total = 0;
        var scale = 0;
        foreach (var value in values)
        {
            var (coefficient, valueScale) = Parts(value);
            var nextScale = Math.Max(scale, valueScale);
            total = total * BigInteger.Pow(10, nextScale - scale) + coefficient * BigInteger.Pow(10, nextScale - valueScale);
            scale = nextScale;
        }
        return TryCreate(total, scale, out var result) ? result : null;
    }

    public static decimal? MultiplyOrUnavailable(decimal left, decimal right)
    {
        var a = Parts(left);
        var b = Parts(right);
        return TryCreate(a.Coefficient * b.Coefficient, a.Scale + b.Scale, out var result) ? result : null;
    }

    private static (BigInteger Coefficient, int Scale) Parts(decimal value)
    {
        var bits = decimal.GetBits(value);
        var coefficient = (BigInteger)(uint)bits[0] + ((BigInteger)(uint)bits[1] << 32) + ((BigInteger)(uint)bits[2] << 64);
        return ((bits[3] & int.MinValue) == 0 ? coefficient : -coefficient, (bits[3] >> 16) & 255);
    }

    private static bool TryCreate(BigInteger coefficient, int scale, out decimal result)
    {
        var magnitude = BigInteger.Abs(coefficient);
        var maximum = (BigInteger.One << 96) - 1;
        while ((scale > 28 || magnitude > maximum) && scale > 0 && magnitude % 10 == 0)
        {
            magnitude /= 10;
            scale--;
        }
        if (scale > 28 || magnitude > maximum)
        {
            result = default;
            return false;
        }
        result = new decimal(unchecked((int)(uint)(magnitude & uint.MaxValue)), unchecked((int)(uint)((magnitude >> 32) & uint.MaxValue)),
            unchecked((int)(uint)((magnitude >> 64) & uint.MaxValue)), coefficient.Sign < 0, (byte)scale);
        return true;
    }
}
