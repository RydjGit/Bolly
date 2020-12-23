namespace Bolly.Models
{
    public class Proxy
    {
        public string Host { get; }
        public string Port { get; }
        public string Username { get; }
        public string Password { get; }
        public bool WithCredentials { get; }
        public bool IsValid { get; }

        private const string Separator = ":";
        private const int Count = 2;
        private const int CountWithCredentials = 4;

        public Proxy(string proxy)
        {
            string[] proxySplit = proxy.Split(Separator);

            if (proxySplit.Length != Count && proxySplit.Length != CountWithCredentials) return;

            Host = proxySplit[0];
            Port = proxySplit[1];

            if (proxySplit.Length == CountWithCredentials)
            {
                Username = proxySplit[2];
                Password = proxySplit[3];

                WithCredentials = true;
            }

            IsValid = true;
        }

        public override string ToString()
        {
            if (WithCredentials) return string.Join(Separator, Host, Port, Username, Password);
            return string.Join(Separator, Host, Port);
        }
    }
}
