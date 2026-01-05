using System;
using System.Threading.Tasks;
using Beep.OilandGas.Client.Exceptions;

namespace Beep.OilandGas.Client.Authentication
{
    /// <summary>
    /// Authentication provider that accepts external tokens
    /// </summary>
    public class TokenProvider : IAuthenticationProvider
    {
        private string? _accessToken;
        private DateTime? _expiresAt;
        private readonly Func<string>? _tokenFactory;
        private readonly Func<Task<string>>? _asyncTokenFactory;

        /// <summary>
        /// Creates a new TokenProvider with a static token
        /// </summary>
        public TokenProvider(string accessToken, DateTime? expiresAt = null)
        {
            _accessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
            _expiresAt = expiresAt;
        }

        /// <summary>
        /// Creates a new TokenProvider with a token factory function
        /// </summary>
        public TokenProvider(Func<string> tokenFactory)
        {
            _tokenFactory = tokenFactory ?? throw new ArgumentNullException(nameof(tokenFactory));
        }

        /// <summary>
        /// Creates a new TokenProvider with an async token factory function
        /// </summary>
        public TokenProvider(Func<Task<string>> asyncTokenFactory)
        {
            _asyncTokenFactory = asyncTokenFactory ?? throw new ArgumentNullException(nameof(asyncTokenFactory));
        }

        /// <summary>
        /// Gets or sets the current access token
        /// </summary>
        public string? AccessToken
        {
            get => _accessToken;
            set
            {
                _accessToken = value;
                _expiresAt = null; // Reset expiration when manually set
            }
        }

        /// <summary>
        /// Gets or sets the token expiration time
        /// </summary>
        public DateTime? ExpiresAt
        {
            get => _expiresAt;
            set => _expiresAt = value;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            // Check if token is expired
            if (_expiresAt.HasValue && DateTime.UtcNow >= _expiresAt.Value)
            {
                // Try to refresh
                if (!await RefreshTokenAsync())
                {
                    throw new ApiAuthenticationException("Access token has expired and could not be refreshed");
                }
            }

            // If token exists, return it
            if (!string.IsNullOrEmpty(_accessToken))
            {
                return _accessToken;
            }

            // Try to get token from factory
            if (_asyncTokenFactory != null)
            {
                _accessToken = await _asyncTokenFactory();
                return _accessToken;
            }

            if (_tokenFactory != null)
            {
                _accessToken = _tokenFactory();
                return _accessToken;
            }

            throw new ApiAuthenticationException("No access token available");
        }

        public virtual Task<bool> RefreshTokenAsync()
        {
            // Default implementation: return false (cannot refresh)
            // Subclasses or consumers should set a new token manually
            return Task.FromResult(false);
        }
    }
}

