
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
        }
        public FileData DownloadFile(DownloadRequest reqFile)
        {
            try
            {
                string filePath = Path.Combine(ServerFolder, reqFile.FileName);

                Stream stream = new FileStream(filePath,
                    System.IO.FileMode.Open,
                    System.IO.FileAccess.Read,
                    FileShare.ReadWrite);

                Console.WriteLine("DownloadFile Request {0} {1}", reqFile.FileName, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                return new FileData
                {
                    FileName = reqFile.FileName,
                    FileLength = new System.IO.FileInfo(filePath).Length,
                    Stream = stream
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
                if (uploadFile.Stream == null) throw new Exception("FileData is null error ");


                // 서버 파일경로 + 파일명
                string filePath = Path.Combine(ServerFolder, uploadFile.FileName);

                // file이 있어면 삭제
                if (File.Exists(filePath)) File.Delete(filePath);

                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    Stream sourceStream = uploadFile.Stream;
                    // 기본 buffer size 4K
                    sourceStream.CopyTo(fs);
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