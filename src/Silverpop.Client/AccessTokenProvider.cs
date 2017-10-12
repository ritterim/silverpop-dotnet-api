using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Silverpop.Client
{
    public class AccessTokenProvider
    {
        private static readonly string DataFormatString =
            "grant_type=refresh_token&client_id={0}&client_secret={1}&refresh_token={2}";

        private readonly TransactClientConfiguration _configuration;
        private readonly string _tokenUrl;
        private readonly HttpClient _httpClient;

        private string _currentAccessToken;
        private DateTime? _currentAccessTokenExpiresAt;

        /// <param name="httpClient">
        /// Optional: If null a default implementation will be used.
        /// </param>
        public AccessTokenProvider(TransactClientConfiguration configuration, HttpClient httpClient = null)
        {
            _configuration = configuration;
            _tokenUrl = string.Format(
                "https://api{0}.silverpop.com/oauth/token",
                configuration.PodNumber);
            _httpClient = httpClient ?? new HttpClient();
        }

        public string Get()
        {
            if (_currentAccessToken != null &&
                _currentAccessTokenExpiresAt > DateTime.UtcNow.AddMinutes(-5))
            {
                return _currentAccessToken;
            }

            var responseMessage = _httpClient.PostAsync(
                _tokenUrl,
                new StringContent(string.Format(
                    DataFormatString,
                    _configuration.OAuthClientId,
                    _configuration.OAuthClientSecret,
                    _configuration.OAuthRefreshToken
            ), Encoding.UTF8, "application/x-www-form-urlencoded")).ConfigureAwait(false).GetAwaiter().GetResult();

            responseMessage.EnsureSuccessStatusCode();

            var responseContentString = responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            var refreshResponse = JsonConvert.DeserializeObject<AccessTokenRefreshResponse>(
                responseContentString);

            _currentAccessToken = refreshResponse.AccessToken;
            _currentAccessTokenExpiresAt = refreshResponse.ExpiresAt;

            return _currentAccessToken;
        }

        public async Task<string> GetAsync()
        {
            if (_currentAccessToken != null &&
                _currentAccessTokenExpiresAt > DateTime.UtcNow.AddMinutes(-5))
            {
                return _currentAccessToken;
            }

            var responseMessage = await _httpClient.PostAsync(
                _tokenUrl,
                new StringContent(string.Format(
                    DataFormatString,
                    _configuration.OAuthClientId,
                    _configuration.OAuthClientSecret,
                    _configuration.OAuthRefreshToken
            ), Encoding.UTF8, "application/x-www-form-urlencoded")).ConfigureAwait(false);

            responseMessage.EnsureSuccessStatusCode();

            var responseContentString = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            var refreshResponse = JsonConvert.DeserializeObject<AccessTokenRefreshResponse>(
                responseContentString);

            _currentAccessToken = refreshResponse.AccessToken;
            _currentAccessTokenExpiresAt = refreshResponse.ExpiresAt;

            return _currentAccessToken;
        }

        public void Refresh()
        {
            _currentAccessToken = null;
            _currentAccessTokenExpiresAt = null;
        }
    }
}