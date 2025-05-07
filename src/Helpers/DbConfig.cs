namespace api_cleany_app.src.Helpers
{
    public static class DbConfig
    {
        public static string ConnectionString { get; private set; } = string.Empty;

        public static void Init(IConfiguration config)
        {
            var profile = config["ConnectionProfile"];
            ConnectionString = config.GetConnectionString(profile ?? "LocalConnection")!;
        }
    }
}