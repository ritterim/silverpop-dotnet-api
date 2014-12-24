using Newtonsoft.Json;
using System;

namespace Silverpop.Client
{
    public class AccessTokenRefreshResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        public DateTime? ExpiresAt
        {
            get
            {
                if (ExpiresIn == default(int))
                    return null;

                if (ExpiresIn < 0)
                    throw new ArgumentOutOfRangeException("ExpiresIn");

                return UtcNow.AddSeconds(ExpiresIn);
            }
        }

        public virtual DateTime UtcNow
        {
            get { return DateTime.UtcNow; }
        }
    }
}