
using Microsoft.Extensions.Configuration;

namespace Models.Utilities
{
    public class ConfigurationUtility
    {
        public IConfigurationRoot config = new ConfigurationBuilder()
          .SetBasePath(Directory.GetCurrentDirectory())
          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
          .Build();
    }
}
