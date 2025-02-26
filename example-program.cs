using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Steeltoe.Configuration.ConfigServer;

namespace Simple
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }
        #region webhostsnippet
        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                    .AddConfigServer()
                    .UseStartup<Startup>()
                    .Build();
            #some other thing
        #endregion
    }
}
