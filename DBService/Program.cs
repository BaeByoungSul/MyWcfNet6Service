// See https://aka.ms/new-console-template for more information

using CoreWCF.Configuration;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System.Net;

namespace BBS
{
    partial class Program
    {
        static void Main(string[] args)
        {

            IWebHost host = CreateWebHost(args).Build();
            host.Run();

        }
        public static IWebHostBuilder CreateWebHost(string[] args)
        {
            var host = WebHost.CreateDefaultBuilder(args);
            host.UseKestrel(option =>
            {
                option.AllowSynchronousIO = true;
                option.Listen(IPAddress.Parse("172.20.105.36"), 9110);
                //option.Listen(IPAddress.Parse("172.20.105.36"), 9130);
            });
            host.UseNetTcp(IPAddress.Parse("172.20.105.36"), 9120);
            host.UseStartup<StartupDBService>();

            return host;

        }

    

    
    }

}

