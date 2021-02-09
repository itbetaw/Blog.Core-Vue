using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System;

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

                var val = string.Empty;
                for (int i = 0; i < sections.Length; i++)
                {
                    val += sections[i] + ":";
                }
                return Configuration[val.TrimEnd(':')];
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
