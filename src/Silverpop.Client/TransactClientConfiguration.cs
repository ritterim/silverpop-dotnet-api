using System;

namespace Silverpop.Client
{
    public class TransactClientConfiguration
    {
        private string _transactHttpsUrl;
        private string _transactSftpUrl;

        /// <summary>
        /// https://transact#.silverpop.com/XTMail
        /// where # is the pod number you wish to connect to.
        /// </summary>
        public string TransactHttpsUrl
        {
            get { return _transactHttpsUrl; }
            set
            {
                if (value != null && !value.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException(
                        "TransactHttpsUrl property must begin with \"https://\".");

                _transactHttpsUrl = value;
            }
        }

        /// <summary>
        /// sftp://transfer#.silverpop.com
        /// where # is the pod number you wish to connect to.
        /// </summary>
        public string TransactSftpUrl
        {
            get { return _transactSftpUrl; }
            set
            {
                if (value != null && !value.StartsWith("sftp://", StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException(
                        "TransactSftpUrl property must begin with \"sftp://\".");

                _transactSftpUrl = value;
            }
        }

        public string Username { get; set; }

        public string Password { get; set; }
    }
}