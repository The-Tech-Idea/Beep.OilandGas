using System.Net.Http;
using System.Text.Json;
using TheTechIdeaWeb.Diagnostics;

namespace Beep.OilandGas.Web.Services
{
    /// <summary>
    /// HTTP client service for calling the API service endpoints.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Every call either succeeds or throws (OILGAS-CATCH-01): a non-success answer is an <see cref="OilGasApiException"/>
    /// carrying the API's status, its sentence and — for a failure — the reference it filed it under, which a page words
    /// through <see cref="OilGasApiFailures"/>; a request that did not complete is the framework's
    /// <see cref="HttpRequestException"/>; an answer that cannot be read is <see cref="JsonException"/>.
    /// </para>
    /// <para>
    /// The calls that send and expect nothing back return <see cref="Task"/>. They returned <c>bool</c>, answering a refusal,
    /// a failure and a lost connection alike as <c>false</c>, logged and reported nowhere — and two pages discarded even
    /// that, so a delete the API refused looked done.
    /// </para>
    /// </remarks>
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IFailureReporter _failures;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        public ApiClient(HttpClient httpClient, IFailureReporter failures)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _failures = failures ?? throw new ArgumentNullException(nameof(failures));
        }

        public async Task<T?> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            return await ReadAsync<T>(response, cancellationToken);
        }

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(
            string endpoint,
            TRequest data,
            CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.PostAsync(endpoint, Json(data), cancellationToken);
            return await ReadAsync<TResponse>(response, cancellationToken);
        }

        public async Task PostAsync<TRequest>(
            string endpoint,
            TRequest data,
            CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.PostAsync(endpoint, Json(data), cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
        }

        public async Task<TResponse?> PutAsync<TRequest, TResponse>(
            string endpoint,
            TRequest data,
            CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.PutAsync(endpoint, Json(data), cancellationToken);
            return await ReadAsync<TResponse>(response, cancellationToken);
        }

        public async Task PutAsync<TRequest>(
            string endpoint,
            TRequest data,
            CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.PutAsync(endpoint, Json(data), cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
        }

        public async Task<TResponse?> PatchAsync<TRequest, TResponse>(
            string endpoint,
            TRequest data,
            CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Patch, endpoint) { Content = Json(data) };
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            return await ReadAsync<TResponse>(response, cancellationToken);
        }

        public async Task PatchAsync<TRequest>(
            string endpoint,
            TRequest data,
            CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Patch, endpoint) { Content = Json(data) };
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
        }

        public async Task DeleteAsync(string endpoint, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.DeleteAsync(endpoint, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
        }

        public async Task<TResult?> DeleteAsync<TResult>(string endpoint, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.DeleteAsync(endpoint, cancellationToken);
            return await ReadAsync<TResult>(response, cancellationToken);
        }

        /// <summary>
        /// Post with multipart form data (for file uploads)
        /// </summary>
        public async Task<TResponse?> PostAsync<TResponse>(
            string endpoint,
            HttpContent? content,
            CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
            return await ReadAsync<TResponse>(response, cancellationToken);
        }

        /// <summary>
        /// Post with object and get stream response. The response stays open for the stream; the caller disposes the stream.
        /// </summary>
        public async Task<Stream?> PostStreamAsync<TRequest>(
            string endpoint,
            TRequest data,
            CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsync(endpoint, Json(data), cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                using (response)
                {
                    throw await OilGasApiException.ReadAsync(response, _failures, cancellationToken);
                }
            }

            return await response.Content.ReadAsStreamAsync(cancellationToken);
        }

        private static StringContent Json<TRequest>(TRequest data) =>
            new(JsonSerializer.Serialize(data, JsonOptions), System.Text.Encoding.UTF8, "application/json");

        private async Task<T?> ReadAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            await EnsureSuccessAsync(response, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<T>(content, JsonOptions);
        }

        private async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw await OilGasApiException.ReadAsync(response, _failures, cancellationToken);
            }
        }
    }
}
