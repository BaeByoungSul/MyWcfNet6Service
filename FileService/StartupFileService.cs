using CoreWCF.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using BBS;
using CoreWCF;

namespace BBS
{
    public class StartupFileService
    {
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddServiceModelServices()
                    .AddServiceModelMetadata()  ;

        }
        public void Configure(IApplicationBuilder app)
        {
            //app.UseDeveloperExceptionPage();
            
            app.UseServiceModel(builder => 
            {
                builder.AddService<FileService>();

                var basicBinding = new BasicHttpBinding();
                basicBinding.TransferMode = TransferMode.Streamed;
                basicBinding.MessageEncoding = WSMessageEncoding.Mtom;
                basicBinding.MaxReceivedMessageSize = 2147483647;
                basicBinding.MaxBufferSize = 65536;

                //basicBinding.ReaderQuotas.MaxStringContentLength = 2147483647;
                //basicBinding.ReaderQuotas.MaxBytesPerRead = 2147483647;
                //basicBinding.ReaderQuotas.MaxArrayLength = 2147483647;
                //basicBinding.ReaderQuotas.MaxNameTableCharCount = 2147483647;
                //basicBinding.ReaderQuotas.MaxDepth = 2147483647;

                basicBinding.OpenTimeout = TimeSpan.FromMinutes(5);
                basicBinding.CloseTimeout = TimeSpan.FromMinutes(5);
                basicBinding.ReceiveTimeout = TimeSpan.FromMinutes(15);
                basicBinding.SendTimeout = TimeSpan.FromMinutes(15);
                
                builder.AddServiceEndpoint<FileService, IFileService>(basicBinding, "/FileService");

                var nettcpBinding = new NetTcpBinding();
                nettcpBinding.TransferMode = TransferMode.Streamed;
                nettcpBinding.Security.Mode = SecurityMode.None;
                nettcpBinding.MaxReceivedMessageSize = 2147483647;
                nettcpBinding.MaxBufferSize = 65536;


                nettcpBinding.OpenTimeout = TimeSpan.FromMinutes(5);
                nettcpBinding.CloseTimeout = TimeSpan.FromMinutes(5);
                nettcpBinding.ReceiveTimeout = TimeSpan.FromMinutes(15);
                nettcpBinding.SendTimeout = TimeSpan.FromMinutes(15);
                

                builder.AddServiceEndpoint<FileService, IFileService>(nettcpBinding, "/FileService");



                var serviceMetadataBehavior = app.ApplicationServices.GetRequiredService<CoreWCF.Description.ServiceMetadataBehavior>();
                serviceMetadataBehavior.HttpGetEnabled = true;
                //serviceMetadataBehavior.HttpsGetEnabled = true;

                

            });
        }
    
    
    }



    
}