using System;
using System.Threading.Tasks;
using System.IO;
using System.Threading;


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


            long maxReadSize = 1000;
            int backoff = 2;
            long totalLoaded = 0;
            long totalFileLength = 0;
            int negligibleBytesRange = 10;
            using (FileStream fs = new FileStream(path: localFilePath, FileMode.Append, FileAccess.Write))
            {

                do
                {
                    var partialResponse = webCaller.DownloadPartialContent(contentFileUrl, totalLoaded, totalLoaded + maxReadSize, new System.Threading.CancellationToken()).Result;
                    if (partialResponse.StatusCode == System.Net.HttpStatusCode.PartialContent)
                    {

                        totalFileLength = partialResponse.Content.Headers.ContentRange.Length.Value;
                        totalLoaded = partialResponse.Content.Headers.ContentRange.To.Value;
                        partialResponse.Content.CopyToAsync(fs);
                        
                        if (backoff > 2)
                        {
                            backoff--;
                        }
                    }
                    else if (!partialResponse.IsSuccessStatusCode)
                    {                        
                        backoff++;
                        Thread.Sleep((int)TimeSpan.FromSeconds(2 ^ backoff).TotalMilliseconds);
                    }

                }
                while (totalLoaded < totalFileLength || Math.Abs(totalLoaded - totalFileLength) < negligibleBytesRange);
            }


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


        private bool DownloadFullFile(string contentFileUrl, string localFilePath)
        {
            var fileResponse = webCaller.DownloadContent(contentFileUrl, new System.Threading.CancellationToken()).Result;

            using (FileStream fs = new FileStream(path: localFilePath, FileMode.Create, FileAccess.Write))
            {
                //https://stackoverflow.com/a/5515894
                fileResponse.Content.ReadAsStreamAsync().Result.CopyTo(fs);
            }

            return true;
        }
    }
}