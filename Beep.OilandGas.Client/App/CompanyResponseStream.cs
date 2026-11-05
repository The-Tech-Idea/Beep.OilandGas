using System.Net.Http;

namespace Beep.OilandGas.Client.App;

/// <summary>A download belongs to the company that started it and owns its HTTP response.</summary>
public sealed class CompanyResponseStream(Stream inner, HttpResponseMessage response, CompanyClientContext company) : Stream
{
    public override bool CanRead => inner.CanRead;
    public override bool CanSeek => inner.CanSeek;
    public override bool CanWrite => false;
    public override long Length { get { company.EnsureCurrent(response); return inner.Length; } }
    public override long Position { get => inner.Position; set { company.EnsureCurrent(response); inner.Position = value; } }
    public override void Flush() => inner.Flush();
    public override int Read(byte[] buffer, int offset, int count)
    {
        company.EnsureCurrent(response);
        var read = inner.Read(buffer, offset, count);
        company.EnsureCurrent(response);
        return read;
    }
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        company.EnsureCurrent(response);
        var read = await inner.ReadAsync(buffer, cancellationToken);
        company.EnsureCurrent(response);
        return read;
    }
    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        await ReadAsync(buffer.AsMemory(offset, count), cancellationToken);
    public override long Seek(long offset, SeekOrigin origin) { company.EnsureCurrent(response); return inner.Seek(offset, origin); }
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    protected override void Dispose(bool disposing) { if (disposing) { inner.Dispose(); response.Dispose(); } base.Dispose(disposing); }
}
