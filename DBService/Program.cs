// See https://aka.ms/new-console-template for more information

using CoreWCF.Configuration;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System.Net;

namespace BBS
{
    partial class Program
    {
        /// <summary>
        /// Project Property > Debug Tab > input argument address
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            if (args.Length <= 0)
            {
                Console.WriteLine("Input Server IP Address");
                return;
            }
            Console.WriteLine(args[0]);

            IWebHost host = CreateWebHost(args).Build();
            host.Run();

        }
        public static IWebHostBuilder CreateWebHost(string[] args)
        {
            string _ipAddress = args[0];

            var host = WebHost.CreateDefaultBuilder(args);
            host.UseKestrel(option =>
            {
                option.AllowSynchronousIO = true;
                 //option.ListenLocalhost(9110);
                 option.Listen(IPAddress.Parse(_ipAddress), 9110);
                //option.Listen(IPAddress.Parse("172.20.105.36"), 9110);
                //option.Listen(IPAddress.Parse("172.20.105.36"), 9130);
            });
            host.UseNetTcp(IPAddress.Loopback, 9120);
            //host.UseNetTcp(IPAddress.Parse("172.20.105.36"), 9120);
            host.UseStartup<StartupDBService>();

            return host;

        }

    

    
    }

}

