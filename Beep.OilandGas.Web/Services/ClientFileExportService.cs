using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Beep.OilandGas.Web.Services;

public interface IClientFileExportService
{
    Task DownloadJsonAsync<T>(string fileName, T payload);
    Task DownloadTextAsync(string fileName, string content, string contentType);
}

public sealed class ClientFileExportService : IClientFileExportService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly IJSRuntime _jsRuntime;

    public ClientFileExportService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
    }

    public Task DownloadJsonAsync<T>(string fileName, T payload)
    {
        var normalizedFileName = fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
            ? fileName
            : $"{fileName}.json";

        var json = JsonSerializer.Serialize(payload, JsonOptions);
        return DownloadTextAsync(normalizedFileName, json, "application/json");
    }

    public async Task DownloadTextAsync(string fileName, string content, string contentType)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var base64 = Convert.ToBase64String(bytes);
        var dataUrl = $"data:{contentType};charset=utf-8;base64,{base64}";

        await _jsRuntime.InvokeVoidAsync("downloadFile", fileName, dataUrl);
    }
}