
using CoreWCF;
using System.Configuration;
using System.Diagnostics;

namespace BBS
{

    public class FileService : IFileService    
    {

        //private string serverPath = ConfigurationManager.AppSettings.Get("ServerFolder");

        private string ServerFolder { get; set; } = string.Empty;

        public FileService()
        {

            ServerFolder = ConfigurationManager.AppSettings.Get("ServerFolder") ?? string.Empty;
            Console.WriteLine("File Service Created...");
        }
        /// <summary>
        /// Stream as a return value in WCF - who disposes it?
        ///    ;; https://stackoverflow.com/questions/6483320/stream-as-a-return-value-in-wcf-who-disposes-it
        /// 서버에서 Steam을 Close해야 한다.    
        /// </summary>
        /// <param name="reqFile"></param>
        /// <returns></returns>
        /// <exception cref="FaultException"></exception>
        public FileData DownloadFile(DownloadRequest reqFile)
        {
            try
            {
                string filePath = Path.Combine(ServerFolder, reqFile.FileName);

                FileStream? stream = null;

                // 서버에서 Stream Close해야 함
                OperationContext clientContext = OperationContext.Current;
                clientContext.OperationCompleted += (sender, args) =>
                {
                    //Console.WriteLine("Download File Operation Completed");
                    if (stream != null)
                        stream.Dispose();
                };


                //FileStream stream = new FileStream(filePath, FileMode.Open);
                //var stream = File.OpenRead(filePath);

                stream = new FileStream(filePath,
                        System.IO.FileMode.Open,
                        System.IO.FileAccess.Read,
                        System.IO.FileShare.ReadWrite );
                        
                Console.WriteLine("DownloadFile Request {0} {1}", reqFile.FileName, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                
                return new FileData
                {
                    FileName = reqFile.FileName,
                    FileLength = new System.IO.FileInfo(filePath).Length,
                    MyStream = stream
                };


            }
            catch (Exception ex)
            {
                throw new FaultException(ex.ToString());

            }

        }

        public void UploadFile(FileData uploadFile)
        {
            try
            {
                //if (uploadFile == null) throw new Exception("FileData is null error ");
                if (uploadFile.MyStream == null) throw new Exception("FileData is null error ");

                // 서버 파일경로 + 파일명
                string filePath = Path.Combine(ServerFolder, uploadFile.FileName);

                // file이 있어면 삭제
                if (File.Exists(filePath)) File.Delete(filePath);

                using (FileStream targetStream = 
                    new FileStream(filePath, FileMode.Create, FileAccess.Write,FileShare.None))
                {
                    Stream sourceStream = uploadFile.MyStream;
                    // 기본 buffer size 4K
                    sourceStream.CopyTo(targetStream,4096);
                    targetStream.Close();
                    sourceStream.Close();
                }
                

                Console.WriteLine("UploadFile Request {0} {1}", uploadFile.FileName, DateTime.Now);

            }
            catch (Exception ex)
            {
                throw new FaultException(ex.ToString());
            }
        }
        public CheckFileResponse CheckFile(string fileName)
        {
            try
            {

                // 서버 파일경로 + 파일명
                string filePath = Path.Combine(ServerFolder, fileName);

                if (!File.Exists(filePath))
                {
                    return new CheckFileResponse
                    {
                        FileExists = false,
                        FileVersion = string.Empty
                    };
                }
                else
                {
                    FileInfo fileInfo = new System.IO.FileInfo(filePath);
                    FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(filePath);

                    // 버전이 null인 것은 공백처리
                    return new CheckFileResponse
                    {
                        FileExists = true,
                        FileVersion = string.IsNullOrEmpty(versionInfo.FileVersion) ? string.Empty : versionInfo.FileVersion
                    };
                }
            }

            catch (Exception ex)
            {
                throw new FaultException(ex.ToString());
            }

        }

    
    }
}