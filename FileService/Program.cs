// See https://aka.ms/new-console-template for more information
using CoreWCF.Configuration;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace BBS
{
    partial class Program
    {
        public static IConfiguration? MyConfiguration { get; set; }

        /// <summary>
        /// Project Property > Debug Tab > input argument address
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {

            try
            {
                MyConfiguration = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .Build();

                if (MyConfiguration == null) throw new Exception("Configuration Builder Error");


                IWebHost host = CreateWebHost(args).Build();
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
            
            string hostingIPAddr = string.Empty;
            string hostingHttpPort = string.Empty;
            string hostingTcpPost = string.Empty;

            hostingIPAddr = MyConfiguration.GetValue<string>("HostAddress");
            hostingHttpPort = MyConfiguration.GetValue<string>("HostHttpPort");
            hostingTcpPost = MyConfiguration.GetValue<string>("HostTcpPort");

            try
            {
                IPAddress address = IPAddress.Parse(hostingIPAddr);
                int httpPort = int.Parse(hostingHttpPort);
                int tcpPort = int.Parse(hostingTcpPost);


                var host = WebHost.CreateDefaultBuilder();
                host.UseKestrel(option =>
                {
                    option.AllowSynchronousIO = true;
                    option.Limits.MaxRequestBodySize = maxRequestLimit;
                    option.Listen(address, httpPort);

                });
                host.UseNetTcp(address, tcpPort);
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