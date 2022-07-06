using NUnit.Framework;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;

namespace ReliableDownloader.Tests
{
    [TestFixture]
    public class FileDownLoaderTests
    {
        public const string TestPartialUrl = "DownloadPartialContent";
        public const string TestFullUrl = "DownloadContent";
        private string _testDirectory = Environment.CurrentDirectory;

        private FileDownloader _fileDownloader { get; set; }
        private List<string> _testFiles = new List<string>();

        [OneTimeSetUp]        
        public void Setup()
        {
            _fileDownloader = new FileDownloader(new MockWebSystemCalls());
        }
        
        [Category("FullDownload")]
        [Category("Mocked")]
        [Category("Fragile")]
        [Test]
        public void FileDownloader_Full_VeriySize()
        {
            FileProgress fileProgress = null;
            var testFile = Path.GetTempFileName();
            _testFiles.Add(testFile);
            Task<bool> test = Task.Run(() => _fileDownloader.DownloadFile(TestFullUrl, testFile, progress => { fileProgress = progress; }));

            Assert.True(test.Result);
            AssertFileLentgh(testFile);
        } 

        [Category("PartialDownload")]
        [Category("Mocked")]
        [Category("Fragile")]
        [Test]
        public void FileDownloader_Partial_VerifySize()
        {
            FileProgress fileProgress = null;
            var partialTestFile = Path.GetTempFileName();

            _testFiles.Add(partialTestFile);
            Task<bool> test = Task.Run(() => _fileDownloader.DownloadFile(TestPartialUrl, partialTestFile, progress => { fileProgress = progress; }));

            Assert.True(test.Result);
            AssertFileLentgh(partialTestFile);
        }

        private void AssertFileLentgh(string filePath)
        {
            //may not pass if disk is busy...
            var fileInfo = new FileInfo(filePath);
            Assert.AreEqual(MockWebSystemCalls.DataLength, fileInfo.Length);
        }


        [OneTimeTearDown]
        public void TearDown()
        {
            foreach(var fileName in _testFiles)
            {
                if(File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
        }

    }
}