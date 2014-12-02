namespace Silverpop.Client
{
    public class TransactClientConfiguration
    {
        /// <summary>
        /// transact#.silverpop.com/XTMail
        /// where # is the pod number you wish to connect to.
        /// </summary>
        public string TransactHttpHost { get; set; }

        /// <summary>
        /// transfer#.silverpop.com
        /// where # is the pod number you wish to connect to.
        /// </summary>
        public string TransactFtpHost { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }
    }
}