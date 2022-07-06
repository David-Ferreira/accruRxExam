using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http.Headers;

namespace ReliableDownloader.Tests
{
    public class MockWebSystemCalls : IWebSystemCalls
    {
        private Random _random = new Random();
        public const int DataLength = 2000;
        private byte[] _data = new byte[DataLength];

        public MockWebSystemCalls()
        {
            _random.NextBytes(_data);
        }


        public Task<HttpResponseMessage> DownloadContent(string url, CancellationToken token)
        {
            HttpResponseMessage message = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StreamContent(new MemoryStream(_data))
            };
            return Task.FromResult(message);
        }

        public Task<HttpResponseMessage> DownloadPartialContent(string url, long from, long to, CancellationToken token)
        {
            //https://keestalkstech.com/2010/11/seek-position-of-a-string-in-a-file-or-filestream/#seek-string-in-stream
            var streamData = DownloadContent(url, token).Result;
            var streamDataContent = streamData.Content.ReadAsByteArrayAsync().Result;

            long bufferSize = to - from;


            var buffer = new byte[bufferSize];


            //copy only specific bytes from StreamData.Content to content.

            for(int i = 0;  i < bufferSize; i++)
            {
                buffer[i] = streamDataContent[from + i];
            }
      
            StreamContent content = new StreamContent(new MemoryStream(buffer));

            
            



            

            HttpResponseMessage message = new HttpResponseMessage(System.Net.HttpStatusCode.PartialContent)
            {
                Content = content,


            };
            message.Content.Headers.Add(@"Content-Length", bufferSize.ToString());
            ContentRangeHeaderValue rangeHeaderValue = new ContentRangeHeaderValue(from, to, streamDataContent.Length );
            
            message.Content.Headers.ContentRange = rangeHeaderValue;
            return Task.FromResult(message);
        }

        public Task<HttpResponseMessage> GetHeadersAsync(string url, CancellationToken token)
        {
            HttpResponseMessage message = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            if (url == "DownloadPartialContent")
            {
                message.Headers.Add(FileDownloader.PartialDownloadHeader, "bytes");
            }
            return Task.FromResult(message);
        }
    }
}
