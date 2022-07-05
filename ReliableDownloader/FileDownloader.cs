using System;
using System.Threading.Tasks;
using System.IO;


namespace ReliableDownloader
{
    public class FileDownloader : IFileDownloader
    {
        IWebSystemCalls webCaller;
        //Mabye DI
        public FileDownloader()
        {
            webCaller = new WebSystemCalls();
        }


        //https://stackoverflow.com/a/44444633
        //https://stackoverflow.com/a/1278990

        public Task<bool> DownloadFile(string contentFileUrl, string localFilePath, Action<FileProgress> onProgressChanged)
        {
            //Use websystem client to Get Header.
            var response = webCaller.GetHeadersAsync(contentFileUrl, new System.Threading.CancellationToken()).Result;



            var fileResponse = webCaller.DownloadContent(contentFileUrl, new System.Threading.CancellationToken()).Result;

            using (FileStream fs = new FileStream(path: localFilePath, FileMode.Append, FileAccess.Write))
            {
                //https://stackoverflow.com/a/5515894
                fileResponse.Content.ReadAsStreamAsync().Result.CopyTo(fs);
            }
            //if allow partial content -> 

            //Get partial

            //else 
            //get full

            //User websys client to get file content. 


            //

            throw new NotImplementedException();
        }

        public void CancelDownloads()
        {
            throw new NotImplementedException();
        }
    }
}