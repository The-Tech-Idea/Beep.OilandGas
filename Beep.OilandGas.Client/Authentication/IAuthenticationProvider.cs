using System.Threading.Tasks;

namespace Beep.OilandGas.Client.Authentication
{
    /// <summary>
    /// Interface for authentication providers that supply access tokens
    /// </summary>
    public interface IAuthenticationProvider
    {
        /// <summary>
        /// Gets an access token asynchronously
        /// </summary>
        /// <returns>Access token string</returns>
        Task<string> GetAccessTokenAsync();

        /// <summary>
        /// Refreshes the access token if applicable
        /// </summary>
        /// <returns>True if refresh was successful</returns>
        Task<bool> RefreshTokenAsync();
    }
}

