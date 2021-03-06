using CoreWCF.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using BBS;
using CoreWCF;

namespace BBS.WCF
{
    public class StartupDBService
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddServiceModelServices()
                    .AddServiceModelMetadata(); ;
        }
        /// <summary>
        /// 2022.06.16 : NetTcpBinding: reliableSession 이 적용되지 않음
        /// 2022.06.?? : Mtom 일 때 아래 메세지가 나타나서 제외함 
        ///    the application completed without reading the entire request body
        /// 
        /// </summary>
        /// <param name="app"></param>
        public void Configure(IApplicationBuilder app)
        {
           
            app.UseServiceModel(builder => 
            {
                builder.AddService<DBService>();

                var basicBinding = new BasicHttpBinding();
                basicBinding.TransferMode = TransferMode.Streamed;
                //basicBinding.MessageEncoding = WSMessageEncoding.Mtom;
                basicBinding.MaxReceivedMessageSize = 2147483647;

                //basicBinding.ReaderQuotas.MaxDepth = 15;
                //basicBinding.ReaderQuotas.MaxStringContentLength = 2147483647;
                //basicBinding.ReaderQuotas.MaxArrayLength = 16384;
                //basicBinding.ReaderQuotas.MaxBytesPerRead = 4096;


                basicBinding.OpenTimeout = TimeSpan.FromMinutes(5);
                basicBinding.CloseTimeout = TimeSpan.FromMinutes(5);
                basicBinding.ReceiveTimeout = TimeSpan.FromMinutes(15);
                basicBinding.SendTimeout = TimeSpan.FromMinutes(15);
                
                builder.AddServiceEndpoint<DBService, IDBService>(basicBinding, "/DBService");


                var nettcpBinding = new NetTcpBinding();
                nettcpBinding.TransferMode = TransferMode.Streamed;
                nettcpBinding.Security.Mode = SecurityMode.None;
                
                nettcpBinding.MaxReceivedMessageSize = 2147483647;
                nettcpBinding.OpenTimeout = TimeSpan.FromMinutes(5);
                nettcpBinding.CloseTimeout = TimeSpan.FromMinutes(5);
                nettcpBinding.ReceiveTimeout = TimeSpan.FromMinutes(15);
                nettcpBinding.SendTimeout = TimeSpan.FromMinutes(15);

                builder.AddServiceEndpoint<DBService, IDBService>(nettcpBinding, "/DBService");

                //builder.AddServiceEndpoint<DBService, IDBService>(new WSHttpBinding( SecurityMode.None), "http://172.20.105.36:9130/DBService");
                //builder.AddServiceEndpoint<DBService, IDBService>(new WSHttpBinding( SecurityMode.Transport), "https://172.20.105.36:9130/DBService");

                var serviceMetadataBehavior = app.ApplicationServices.GetRequiredService<CoreWCF.Description.ServiceMetadataBehavior>();
                serviceMetadataBehavior.HttpGetEnabled = true;
                //serviceMetadataBehavior.HttpsGetEnabled = true;

                

            });
        }
    
    
    }



    
}