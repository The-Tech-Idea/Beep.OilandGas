using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Beep.OilandGas.Client.Exceptions;
using Beep.OilandGas.Client.Models;

namespace Beep.OilandGas.Client.Authentication
{
    /// <summary>
    /// Authentication provider that handles username/password authentication with IdentityServer
    /// </summary>
    public class CredentialsAuthenticationProvider : IAuthenticationProvider
    {
        private readonly HttpClient _httpClient;
        private readonly string _identityServerUrl;
        private readonly string _clientId;
        private readonly string? _clientSecret;
        private readonly string _username;
        private readonly string _password;
        private string? _accessToken;
        private string? _refreshToken;
        private DateTime? _expiresAt;

        public CredentialsAuthenticationProvider(
            HttpClient httpClient,
            string identityServerUrl,
            string username,
            string password,
            string? clientId = null,
            string? clientSecret = null)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _identityServerUrl = identityServerUrl ?? throw new ArgumentNullException(nameof(identityServerUrl));
            _username = username ?? throw new ArgumentNullException(nameof(username));
            _password = password ?? throw new ArgumentNullException(nameof(password));
            _clientId = clientId ?? "beep-api";
            _clientSecret = clientSecret;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            if (_expiresAt.HasValue && DateTime.UtcNow >= _expiresAt.Value.AddMinutes(-5))
            {
                if (!string.IsNullOrEmpty(_refreshToken))
                {
                    if (await RefreshTokenAsync())
                    {
                        return _accessToken!;
                    }
                }
                await AcquireTokenAsync();
                return _accessToken!;
            }

            if (string.IsNullOrEmpty(_accessToken))
            {
                await AcquireTokenAsync();
                return _accessToken!;
            }

            return _accessToken;
        }

        public async Task<bool> RefreshTokenAsync()
        {
            if (string.IsNullOrEmpty(_refreshToken))
                return false;

            try
            {
                var tokenEndpoint = _identityServerUrl.TrimEnd('/') + "/connect/token";
                var request = new RefreshTokenRequest
                {
                    RefreshToken = _refreshToken,
                    ClientId = _clientId,
                    ClientSecret = _clientSecret
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(tokenEndpoint, content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.AccessToken))
                {
                    _accessToken = tokenResponse.AccessToken;
                    _refreshToken = tokenResponse.RefreshToken ?? _refreshToken;
                    _expiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private async Task AcquireTokenAsync()
        {
            try
            {
                var tokenEndpoint = _identityServerUrl.TrimEnd('/') + "/connect/token";
                var request = new TokenRequest
                {
                    Username = _username,
                    Password = _password,
                    ClientId = _clientId,
                    ClientSecret = _clientSecret
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(tokenEndpoint, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new ApiAuthenticationException(
                        $"Failed to acquire access token. Status: {response.StatusCode}, Response: {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
                {
                    throw new ApiAuthenticationException("Token response did not contain an access token");
                }

                _accessToken = tokenResponse.AccessToken;
                _refreshToken = tokenResponse.RefreshToken;
                _expiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
            }
            catch (ApiAuthenticationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApiAuthenticationException("Failed to acquire access token", ex);
            }
        }
    }
}
