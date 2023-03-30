using Downloader;
using System.ComponentModel;

public static class GitHubDownloader
{
    public static async Task<IDownload> Download(
        string Url,
        string FileName,
        string SavePath,
        EventHandler<DownloadProgressChangedEventArgs> DownloadProgressChanged,
        EventHandler<AsyncCompletedEventArgs> DownloadFileCompleted,
        EventHandler<DownloadStartedEventArgs> DownloadStarted)
    {
        IDownload download = DownloadBuilder.New()
    .WithUrl(Url)
    .WithDirectory(SavePath)
    .WithFileName(FileName)
    .WithConfiguration(new DownloadConfiguration())
    .Build();

        download.DownloadProgressChanged += DownloadProgressChanged;
        download.DownloadFileCompleted += DownloadFileCompleted;
        download.DownloadStarted += DownloadStarted;

      await download.StartAsync();
        return download;
    }


}

