using Blog.Core.Common;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Blog.Core
{
    public class DefaultAppConfigurationAccessor : IAppConfigurationAccessor
    {
        public IConfigurationRoot Configuration { get; }

        public DefaultAppConfigurationAccessor()
        {
            Configuration = AppConfigurations.Get(Directory.GetCurrentDirectory());
        }
    }
}
