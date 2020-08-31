namespace vtb.Utils.MongoDb
{
    public class MongoDbConfiguration
    {
        public string Host { get; set; }
        public string Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public string Database { get; set; }

        private static string DefaultPort = "27037";

        public string ConnectionString
        {
            get
            {
                if (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password) && !string.IsNullOrEmpty(Host))
                {
                    return $"mongodb://{Username}:{Password}@{Host}:{Port ?? DefaultPort}";
                }
                else if (!string.IsNullOrEmpty(Host))
                {
                    return $"mongodb://{Host}:{Port ?? DefaultPort}";
                }

                return string.Empty;
            }
        }
    }
}