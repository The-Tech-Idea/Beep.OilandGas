using System.Text;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// A minimal RP66 v1 DLIS file written to the specification, ported from the tools' own
/// <c>DlisTests.SampleFile</c> so the desktop proves it reads a real file rather than a mock.
/// </summary>
/// <remarks>
/// It contains one frame with a scalar index (DEPT), one scalar curve (GR) and one <b>array</b> channel
/// (WAVE, dimension 3) — the array is what INT-01 AC-3 requires to be reported, not silently dropped. The
/// channels are also split across two visible records to exercise the record-following logic.
/// </remarks>
internal static class DlisFixture
{
    public static byte[] SampleFile()
    {
        var fileHeader = Eflr(
            "FILE-HEADER",
            [("SEQUENCE-NUMBER", 20), ("ID", 20)],
            (ObName(1, 0, "1"), [Val(1, Uvari(1), Encoding.ASCII.GetBytes("1")), Val(1, Uvari(4), Encoding.ASCII.GetBytes("TEST"))]));

        var origin = Eflr(
            "ORIGIN",
            [("WELL-NAME", 20), ("FIELD-NAME", 20), ("COMPANY", 20), ("WELL-ID", 20)],
            (ObName(1, 0, "DEFINING_ORIGIN"), [
                Val(1, Uvari(7), Encoding.ASCII.GetBytes("DLIS-17")),
                Val(1, Uvari(5), Encoding.ASCII.GetBytes("Demo ")),
                Val(1, Uvari(4), Encoding.ASCII.GetBytes("Beep")),
                null,
            ]));

        var channels = Eflr(
            "CHANNEL",
            [("LONG-NAME", 20), ("REPRESENTATION-CODE", 15), ("UNITS", 27), ("DIMENSION", 18)],
            (ObName(1, 0, "DEPT"), [Val(1, Uvari(5), Encoding.ASCII.GetBytes("Depth")), Val(1, new byte[] { 7 }), Val(1, Ident("ft")), Val(1, Uvari(1))]),
            (ObName(1, 0, "GR"), [Val(1, Uvari(9), Encoding.ASCII.GetBytes("Gamma ray")), Val(1, new byte[] { 2 }), Val(1, Ident("gAPI")), Val(1, Uvari(1))]),
            (ObName(1, 0, "WAVE"), [Val(1, Uvari(8), Encoding.ASCII.GetBytes("Waveform")), Val(1, new byte[] { 2 }), Val(1, Ident("mV")), Val(1, Uvari(3))]));

        var frame = Eflr(
            "FRAME",
            [("CHANNELS", 23), ("INDEX-TYPE", 19)],
            (ObName(1, 0, "MAIN"), [
                Val(3, ObName(1, 0, "DEPT"), ObName(1, 0, "GR"), ObName(1, 0, "WAVE")),
                Val(1, Ident("BOREHOLE-DEPTH")),
            ]));

        byte[] Row(long no, double depth, float gr) =>
            Cat(Uvari(no), Fdoubl(depth), Fsingl(gr), Fsingl(1), Fsingl(2), Fsingl(3));

        var frameData1 = Cat(ObName(1, 0, "MAIN"), Row(1, 1000, 50.5f), Row(2, 1000.5f, -999.25f));
        var frameData2 = Cat(ObName(1, 0, "MAIN"), Row(3, 1001, 75f));

        var half = channels.Length / 2;

        return Cat(
            Sul(),
            Vr(Segment(0, true, fileHeader), Segment(1, true, origin, pad: true)),
            Vr(Segment(3, true, channels.Take(half).ToArray(), succ: true)),
            Vr(Segment(3, true, channels.Skip(half).ToArray(), pred: true), Segment(4, true, frame)),
            Vr(Segment(0, false, frameData1, pad: true), Segment(0, false, frameData2)));
    }

    /// <summary>Writes the fixture to a disposable folder and returns its path.</summary>
    public static string WriteFile()
    {
        var directory = Path.Combine(
            TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping.OgPaths.DataRoot,
            "dlis-" + Guid.NewGuid().ToString("n")[..8]);

        Directory.CreateDirectory(directory);

        var path = Path.Combine(directory, "sample.dlis");
        File.WriteAllBytes(path, SampleFile());
        return path;
    }

    private static byte[] Ident(string s)
    {
        var b = Encoding.ASCII.GetBytes(s);
        return [(byte)b.Length, .. b];
    }

    private static byte[] Uvari(long v) => v switch
    {
        < 128 => [(byte)v],
        < 16384 => [(byte)(0x80 | (v >> 8)), (byte)v],
        _ => [(byte)(0xC0 | (v >> 24)), (byte)(v >> 16), (byte)(v >> 8), (byte)v],
    };

    private static byte[] ObName(long origin, int copy, string id) =>
        Cat(Uvari(origin), [(byte)copy], Ident(id));

    private static byte[] Fsingl(float f)
    {
        var b = BitConverter.GetBytes(f);

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(b);
        }

        return b;
    }

    private static byte[] Fdoubl(double d)
    {
        var b = BitConverter.GetBytes(d);

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(b);
        }

        return b;
    }

    private static byte[] Cat(params byte[][] parts) => parts.SelectMany(x => x).ToArray();

    private static byte[] Eflr(
        string type,
        (string label, int code)[] template,
        params (byte[] name, byte[]?[] values)[] objects)
    {
        var b = new List<byte> { 0xF0 };
        b.AddRange(Ident(type));

        foreach (var (label, code) in template)
        {
            b.Add(0x34);
            b.AddRange(Ident(label));
            b.Add((byte)code);
        }

        foreach (var (name, values) in objects)
        {
            b.Add(0x70);
            b.AddRange(name);

            foreach (var value in values)
            {
                if (value is null)
                {
                    b.Add(0x00);
                }
                else
                {
                    b.Add(0x29);
                    b.AddRange(value);
                }
            }
        }

        return b.ToArray();
    }

    private static byte[] Val(long count, params byte[][] values) =>
        Cat(Uvari(count), values.SelectMany(x => x).ToArray());

    private static byte[] Segment(
        int type,
        bool explicitRecord,
        byte[] body,
        bool pred = false,
        bool succ = false,
        bool pad = false)
    {
        var padCount = pad ? 3 : 0;
        var length = 4 + body.Length + padCount;
        var attributes = (byte)((explicitRecord ? 0x80 : 0) | (pred ? 0x40 : 0) | (succ ? 0x20 : 0) | (pad ? 0x01 : 0));

        var segment = new List<byte> { (byte)(length >> 8), (byte)length, attributes, (byte)type };
        segment.AddRange(body);

        for (var i = 0; i < padCount; i++)
        {
            segment.Add((byte)padCount);
        }

        return segment.ToArray();
    }

    private static byte[] Vr(params byte[][] segments)
    {
        var body = Cat(segments);
        var length = 4 + body.Length;
        return Cat([(byte)(length >> 8), (byte)length, 0xFF, 0x01], body);
    }

    private static byte[] Sul() =>
        Encoding.ASCII.GetBytes("   1V1.00RECORD 8192" + "TEST STORAGE SET".PadRight(60));
}
