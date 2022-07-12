// See https://aka.ms/new-console-template for more information


using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Net;
using CoreWCF.Configuration;


//using BBS.WCF;
//using CoreWCF.Configuration;
//using Microsoft.AspNetCore;
//using Microsoft.AspNetCore.Hosting;
//using System.Net;

namespace BBS.WCF
{
    
    partial class Program
    {
        //public static IConfigurationRoot? Configuration { get; set; }
        public static IConfiguration? MyConfiguration { get; set; }

        /// <summary>
        /// Project Property > Debug Tab > input argument address
        /// https 및 basic confide
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
                int tcpPort= int.Parse(hostingTcpPost);

                var host = WebHost.CreateDefaultBuilder();

                host.UseKestrel(option =>
                {
                    option.AllowSynchronousIO = true;
                    option.Listen(address, httpPort);
                });
                
                host.UseNetTcp(address, tcpPort);
                host.UseStartup<StartupDBService>();
                return host;

            }
            catch (Exception )
            {
          
                throw ;
            }


        }

    

    
    }

}

