namespace Beep.OilandGas.Web.Services
{
    /// <summary>
    /// HTTP client service for calling the API service endpoints
    /// </summary>
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiClient> _logger;
        
        private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        public ApiClient(HttpClient httpClient, ILogger<ApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("GET {Endpoint}", endpoint);
                var response = await _httpClient.GetAsync(endpoint, cancellationToken);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return System.Text.Json.JsonSerializer.Deserialize<T>(content, JsonOptions);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error on GET {Endpoint}", endpoint);
                throw;
            }
        }

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(
            string endpoint, 
            TRequest data, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("POST {Endpoint}", endpoint);
                var json = System.Text.Json.JsonSerializer.Serialize(data, JsonOptions);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
                response.EnsureSuccessStatusCode();
                
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return System.Text.Json.JsonSerializer.Deserialize<TResponse>(responseContent, JsonOptions);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error on POST {Endpoint}", endpoint);
                throw;
            }
        }

        public async Task<bool> PostAsync<TRequest>(
            string endpoint, 
            TRequest data, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("POST {Endpoint}", endpoint);
                var json = System.Text.Json.JsonSerializer.Serialize(data, JsonOptions);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error on POST {Endpoint}", endpoint);
                return false;
            }
        }

        public async Task<TResponse?> PutAsync<TRequest, TResponse>(
            string endpoint, 
            TRequest data, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("PUT {Endpoint}", endpoint);
                var json = System.Text.Json.JsonSerializer.Serialize(data, JsonOptions);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PutAsync(endpoint, content, cancellationToken);
                response.EnsureSuccessStatusCode();
                
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return System.Text.Json.JsonSerializer.Deserialize<TResponse>(responseContent, JsonOptions);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error on PUT {Endpoint}", endpoint);
                throw;
            }
        }

        public async Task<bool> PutAsync<TRequest>(
            string endpoint, 
            TRequest data, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("PUT {Endpoint}", endpoint);
                var json = System.Text.Json.JsonSerializer.Serialize(data, JsonOptions);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PutAsync(endpoint, content, cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error on PUT {Endpoint}", endpoint);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(string endpoint, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("DELETE {Endpoint}", endpoint);
                var response = await _httpClient.DeleteAsync(endpoint, cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error on DELETE {Endpoint}", endpoint);
                return false;
            }
        }

        public async Task<TResult?> DeleteAsync<TResult>(string endpoint, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("DELETE {Endpoint}", endpoint);
                var response = await _httpClient.DeleteAsync(endpoint, cancellationToken);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return System.Text.Json.JsonSerializer.Deserialize<TResult>(content, JsonOptions);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error on DELETE {Endpoint}", endpoint);
                throw;
            }
        }

        /// <summary>
        /// Post with multipart form data (for file uploads)
        /// </summary>
        public async Task<TResponse?> PostAsync<TResponse>(
            string endpoint,
            HttpContent content,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("POST {Endpoint} (multipart)", endpoint);
                var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
                response.EnsureSuccessStatusCode();
                
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return System.Text.Json.JsonSerializer.Deserialize<TResponse>(responseContent, JsonOptions);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error on POST {Endpoint}", endpoint);
                throw;
            }
        }

        /// <summary>
        /// Post with object and get stream response
        /// </summary>
        public async Task<Stream?> PostStreamAsync<TRequest>(
            string endpoint,
            TRequest data,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("POST {Endpoint} (stream response)", endpoint);
                var json = System.Text.Json.JsonSerializer.Serialize(data, JsonOptions);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
                response.EnsureSuccessStatusCode();
                
                return await response.Content.ReadAsStreamAsync(cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error on POST {Endpoint}", endpoint);
                throw;
            }
        }
    }
}
