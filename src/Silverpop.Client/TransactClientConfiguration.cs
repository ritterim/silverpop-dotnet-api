using System;

namespace Silverpop.Client
{
    public class TransactClientConfiguration
    {
        public const int MaxRecipientsPerBatchRequest = 5000;
        public const int MaxRecipientsPerNonBatchRequest = 10;

        private int? _podNumber;

        public int? PodNumber
        {
            get { return _podNumber; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(
                        "PodNumber",
                        "PodNumber must not be a negative number.");
                }

                _podNumber = value;
            }
        }

        public string EndpointIdentifier
        {
            get
            {
                switch (_podNumber)
                {
                    case 1:
                        return "us-1";
                    case 2:
                        return "us-2";
                    case 3:
                        return "us-3";
                    case 4:
                        return "us-4";
                    case 5:
                        return "us-5";
                    case 6:
                        return "eu-1";
                    case 7:
                        return "ap-2";
                    case 8:
                        return "ca-1";
                    case 9:
                        return "us-6";
                    default:
                        return "";
                }
            }
        }

        public string Username { get; set; }

        public string Password { get; set; }

        public string OAuthClientId { get; set; }

        public string OAuthClientSecret { get; set; }

        public string OAuthRefreshToken { get; set; }

        public void HydrateUsing(TransactClientConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            if (config.PodNumber.HasValue)
                PodNumber = config.PodNumber;

            if (!string.IsNullOrWhiteSpace(config.Username))
                Username = config.Username;

            if (!string.IsNullOrWhiteSpace(config.Password))
                Password = config.Password;

            if (!string.IsNullOrWhiteSpace(config.OAuthClientId))
                OAuthClientId = config.OAuthClientId;

            if (!string.IsNullOrWhiteSpace(config.OAuthClientSecret))
                OAuthClientSecret = config.OAuthClientSecret;

            if (!string.IsNullOrWhiteSpace(config.OAuthRefreshToken))
                OAuthRefreshToken = config.OAuthRefreshToken;
        }
    }
}