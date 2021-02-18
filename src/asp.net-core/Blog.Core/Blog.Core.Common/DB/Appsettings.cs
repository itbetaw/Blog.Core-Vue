using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Blog.Core.Common
{
    public class Appsettings
    {
        static IConfiguration Configuration { get; set; }
        static Appsettings()
        {
            Configuration = new ConfigurationBuilder()
                .Add(new JsonConfigurationSource
                {
                    Path = "appsettings.json",
                    ReloadOnChange = true
                }).Build();
        }
        public static string app(params string[] sections)
        {
            try
            {

                if (sections.Any())
                {
                    return Configuration[string.Join(":", sections)];
                }
            }
            catch (Exception) { }

            return "";
        }
    }

    public static class AppConfigurations
    {
        private static readonly ConcurrentDictionary<string, IConfigurationRoot>
            ConfigurationCache;
        static AppConfigurations()
        {
            ConfigurationCache = new ConcurrentDictionary<string, IConfigurationRoot>();
        }
        public static IConfigurationRoot Get(string path, string environmentName = null
            )
        {
            var cacheKey = path + "#" + environmentName;
            return ConfigurationCache.GetOrAdd(cacheKey,
                _ => BuildConfiguration(path, environmentName));
        }
        private static IConfigurationRoot BuildConfiguration(string path, string environmentName = null)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(path)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            if (!string.IsNullOrWhiteSpace(environmentName))
            {
                builder = builder.AddJsonFile($"appsettings.{environmentName}.json", optional: true);
            }

            builder = builder.AddEnvironmentVariables();
            return builder.Build();
        }
    }
}
