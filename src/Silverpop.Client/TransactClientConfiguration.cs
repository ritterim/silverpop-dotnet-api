namespace Silverpop.Client
{
    public class TransactClientConfiguration
    {
        /// <summary>
        /// transact#.silverpop.com/XTMail
        /// where # is the pod number you wish to connect to.
        /// </summary>
        public string TransactHttpsHost { get; set; }

        /// <summary>
        /// transfer#.silverpop.com
        /// where # is the pod number you wish to connect to.
        /// </summary>
        public string TransactSftpHost { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }
    }
}