using Blog.Core.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blog.Core
{
    public static class HostingEnvironmentHelper
    {
        public static IConfigurationRoot GetAppConfiguration(this IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            if (!string.IsNullOrWhiteSpace(env.EnvironmentName))
            {
                builder = builder.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);
            }

            builder = builder.AddEnvironmentVariables();
            return builder.Build();
        }
    }
}
