namespace Interview.Backend
{
    public static class RedisEnvironmentConfigure
    {
        public static void Configure(IConfiguration configuration, RedisConnectionInformation present, Action elseFunc)
        {
            var storageSection = configuration.GetSection("EventStorage");
            var useRedis = storageSection?.GetValue<bool?>("Enabled") ?? false;

            if (useRedis)
            {
                var redisUsername = storageSection?.GetValue<string>("Username");
                var redisHost = storageSection?.GetValue<string>("Host");
                var redisPort = storageSection?.GetValue<int>("Port");
                var redisPassword = storageSection?.GetValue<string>("Password");

                present.Invoke(redisHost, redisPort, redisUsername, redisPassword);
            }
            else
            {
                elseFunc.Invoke();
            }
        }

        public static void Configure(IConfiguration configuration, RedisConnectionInformation present)
        {
            Configure(configuration, present, () => { });
        }
    }

    public delegate void RedisConnectionInformation(string? host, int? port, string? username, string? password);
}
