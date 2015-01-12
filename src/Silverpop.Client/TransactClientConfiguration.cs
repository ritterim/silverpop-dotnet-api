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

        public string Username { get; set; }

        public string Password { get; set; }

        public string OAuthClientId { get; set; }

        public string OAuthClientSecret { get; set; }

        public string OAuthRefreshToken { get; set; }
    }
}