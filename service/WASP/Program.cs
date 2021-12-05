using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WASP.Storage;

namespace WASP
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args).ConfigureKestrel(serverOptions =>
            {
                serverOptions.Listen(IPAddress.Any, 443, listenOptions => { listenOptions.UseHttps("cert.pfx", "pass"); });
                serverOptions.ConfigureHttpsDefaults(configureOptions: co =>
                 {
                     co.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                 });
            }
            )
                .UseStartup<Startup>();
    }
}
