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
            string[] strHostIp = new string[1];
#if DEBUG
            strHostIp[0] = "127.0.0.1";
#else // Release
            if (args.Length <= 0)
            {
                Console.WriteLine("Input Server IP Address");
                return;
            }
            Console.WriteLine(args[0]);
            strHostIp[0] = args[0];
#endif

            try
            {
                IWebHost host = CreateWebHost(strHostIp).Build();
                host.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

            }

        }
        public static IWebHostBuilder CreateWebHost(string[] args)
        {
            // 830 MB
            //const int maxRequestLimit = 837280000;
            const int maxRequestLimit = 1048576000;

            try
            {
                IPAddress address = IPAddress.Parse(args[0]);
                var host = WebHost.CreateDefaultBuilder();
                host.UseKestrel(option =>
                {
                    option.AllowSynchronousIO = true;
                    option.Limits.MaxRequestBodySize = maxRequestLimit;

                    option.Listen(address, 9210);
                    //option.Listen(IPAddress.Parse("172.20.105.36"), 9210);
                });
                host.UseNetTcp(address, 9220);
                host.UseStartup<StartupFileService>();

                return host;
            }
            catch (Exception)
            {
                throw;
            }
           

        }


    }

}