using ExcelAssess.Aws.SecretsManager.Cache;
using ExcelAssess.Aws.SecretsManager;
using ExcelAssess.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ExcelAssess.TestPlayer.ResponseVerification.Console.Models;

namespace ExcelAssess.TestPlayer.ResponseVerification.Console
{
    public static class DbConfiguration
    {
        private static async Task<string> InitializeConnectionAsync(IConfiguration _config)
        {
            var RedisConnectionString = new RedisSettings
            {
                RedisConnectionString = _config["RedisSettings:RedisConnectionString"]
                                        ?? throw new Exception("no redis connection string found in app settings")
            };

            // Create a Secrets Manager client with caching
            var secretsManagerCache = new AwsSecretsManagerCache(new RedisClient(RedisConnectionString), new AwsSecretsManager());
            string POSTGRESCONNECTION__SECRETNAME = _config["PostgresConnection:SecretName"]
                                                    ?? throw new Exception("secret name not found from app settings");
            string POSTGRESCONNECTION__KEY = _config["PostgresConnection:SecretKey"]
                                             ?? throw new Exception("secret key not found from app settings");

            return await secretsManagerCache.GetSecretCacheAsync(POSTGRESCONNECTION__SECRETNAME, POSTGRESCONNECTION__KEY)
                   ?? throw new Exception("no connection string found from app settings");
        }

        public static void ConfigureDb(this IServiceCollection services, IConfiguration _config)
        {
            services.AddSingleton<DBConnectionString>(new DBConnectionString
            {
                ConnectionString = InitializeConnectionAsync(_config).Result
            });
        }
    }

    
}
