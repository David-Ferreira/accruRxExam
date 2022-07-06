using System;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Net;
using System.Collections.Generic;
using System.Net.Sockets;

namespace ReliableDownloader
{
    public class FileDownloader : IFileDownloader, IDisposable
    {
        private IWebSystemCalls _webCaller;
        private CancellationTokenSource _cancellationSource;
        public const string PartialDownloadHeader = "Accept-Ranges";

        public FileDownloader()
        {
            _webCaller = new WebSystemCalls();
            _cancellationSource = new CancellationTokenSource();
        }

        public FileDownloader(IWebSystemCalls webSystemCalls)
        {
            _webCaller = webSystemCalls;
            _cancellationSource = new CancellationTokenSource();
        }

        public FileDownloader(IWebSystemCalls webSystemCalls, CancellationTokenSource cancellationSource)
        {
            _webCaller = webSystemCalls;
            _cancellationSource = cancellationSource;

        }

        //https://stackoverflow.com/a/44444633
        //https://stackoverflow.com/a/1278990
        public Task<bool> DownloadFile(string contentFileUrl, string localFilePath, Action<FileProgress> onProgressChanged)
        {
            var response = _webCaller.GetHeadersAsync(contentFileUrl, _cancellationSource.Token).Result;
            if(!response.IsSuccessStatusCode)
            {
                throw new ApplicationException("Unable to reach Server");
            }
            return response.Headers.Contains(PartialDownloadHeader) ?
                DownloadFilePartial(contentFileUrl, localFilePath, onProgressChanged)
                : DownloadFullFile(contentFileUrl, localFilePath);
        }

        public void CancelDownloads()
        {
            _cancellationSource.Cancel();
        }

        private Task<bool> DownloadFullFile(string contentFileUrl, string localFilePath)
        {

            var fileResponse = _webCaller.DownloadContent(contentFileUrl, _cancellationSource.Token).Result;

            using (FileStream fs = new FileStream(path: localFilePath, FileMode.Create, FileAccess.Write))
            {
                _cancellationSource.Token.ThrowIfCancellationRequested();
                fileResponse.Content.ReadAsStreamAsync().Result.CopyTo(fs);
            }
            var attributes = new FileInfo(localFilePath);

            return Task.FromResult(attributes.Length == fileResponse.Content.Headers.ContentLength);
        }

        private Task<bool> DownloadFilePartial(string contentFileUrl, string localFilePath, Action<FileProgress> onProgressChanged)
        {
            long maxReadSize = 1000;
            int backoffSeconds = 2;
            int minBackoffSeconds = 2;
            long totalLoaded = 0;
            long totalFileLength = 0;
            int negligibleBytesRange = 10;

            using (FileStream fs = new FileStream(path: localFilePath, FileMode.Append, FileAccess.Write))
            {
                do
                {
                    _cancellationSource.Token.ThrowIfCancellationRequested();
                    try
                    {
                        var partialResponse = _webCaller.DownloadPartialContent(contentFileUrl, totalLoaded, totalLoaded + maxReadSize, _cancellationSource.Token).Result;
                        if (partialResponse.StatusCode == System.Net.HttpStatusCode.PartialContent)
                        {

                            totalFileLength = partialResponse.Content.Headers.ContentRange.Length.Value;
                            totalLoaded = partialResponse.Content.Headers.ContentRange.To.Value;
                            partialResponse.Content.CopyToAsync(fs);

                            FileProgress progress = new FileProgress(totalFileLength, totalLoaded, totalLoaded / (double)totalFileLength, TimeSpan.FromMinutes(60));
                            onProgressChanged(progress);
                            if (backoffSeconds > minBackoffSeconds)
                            {
                                backoffSeconds--;
                            }
                        }
                        else if (!partialResponse.IsSuccessStatusCode)
                        {
                            BackOff(backoffSeconds);
                            backoffSeconds++;
                        }
                    }
                    catch (AggregateException ex)
                    {
                        ex.Handle(e => { if (e is TaskCanceledException) throw e; return true; });
                        Console.WriteLine($"Error: {ex.Message} - waiting {GetBackOffSeconds(backoffSeconds).TotalSeconds} seconds before retry.");
                        BackOff(backoffSeconds);
                        backoffSeconds++;
                    }

                }
                while (totalLoaded < totalFileLength || Math.Abs(totalLoaded - totalFileLength) > negligibleBytesRange);
            }
            return Task.FromResult(Math.Abs(totalLoaded - totalFileLength) <= negligibleBytesRange);
        }

        public void Dispose()
        {
            _cancellationSource.Dispose();
        }


        private void BackOff(int backOffSeconds)
        {
            Thread.Sleep(GetBackOffSeconds(backOffSeconds));
        }

        private TimeSpan GetBackOffSeconds(int backOffSeconds)
        {
            return TimeSpan.FromSeconds(Math.Pow(2, backOffSeconds));
        }


        //TODO parallel downloads
        //https://stackoverflow.com/a/15657433
        //https://docs.microsoft.com/en-us/dotnet/framework/network-programming/managing-connections



    }
}