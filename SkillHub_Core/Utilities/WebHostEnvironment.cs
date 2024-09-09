using Microsoft.AspNetCore.Hosting;

namespace LMSCore.Utilities
{
    public class WebHostEnvironment
    {
        private static IWebHostEnvironment _env;
        public static IWebHostEnvironment Environment { get; set; }
        public static void Config(IWebHostEnvironment env)
        {
            _env = env;
            Environment = env;
        }
    }
}
