using System;
using System.Threading.Tasks;
using ReliableDownloader;


namespace ReliableDownloader
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            // If this url 404's, you can get a live one from https://installerstaging.accurx.com/chain/latest.json.
            var exampleUrl = "https://installerstaging.accurx.com/chain/3.182.57641.0/accuRx.Installer.Local.msi";

            int displayDecimalPlaces = 2;
            var exampleFilePath = "C:/Users/[USER]/myfirstdownload.msi";
            exampleFilePath = System.IO.Path.Combine(Environment.CurrentDirectory, "myfirstdownload.msi");
            using (var fileDownloader = new FileDownloader())
            {
                try
                {
                    Task checkforCancel = Task.Run(() => CheckUserCancel(fileDownloader));
                    await fileDownloader.DownloadFile(exampleUrl, exampleFilePath, progress => { Console.WriteLine($"Percent progress is {Math.Round(progress.ProgressPercent.Value * 100, displayDecimalPlaces)}"); });
                    await checkforCancel;
                }
                catch (TaskCanceledException)           
                {
                    if (System.IO.File.Exists(exampleFilePath))
                    {
                        System.IO.File.Delete(exampleFilePath);
                    }
                    Console.WriteLine("Operation was canceled.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error! {ex.Message}");
                }
            }
        }

        //Allow Cancel https://darchuk.net/2019/02/08/waiting-for-a-keypress-asynchronously-in-a-c-console-app/
        public static bool CheckUserCancel(FileDownloader downloader)
        {
            ConsoleKeyInfo cki = new ConsoleKeyInfo();
            do
            {
                // true hides the pressed character from the console
                cki = Console.ReadKey(true);

                // Wait for an ESC
            } while (cki.Key != ConsoleKey.Escape);
            downloader.CancelDownloads();
            return true;
        }
    }
}
