using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RPGStudioMK.Widgets;

public class FileDownloaderWindow : ProgressWindow
{
    FileDownloader downloader;

    public Action<Exception> OnError;

    public FileDownloaderWindow(string URL, string Filename, string DownloadText = "Downloading File...", bool CloseWhenDone = true, bool Cancellable = true) :
        base("Downloader", "Connecting to server...", CloseWhenDone, Cancellable, false, true)
    {
        downloader = new FileDownloader(URL, Filename);
        this.OnCancelled += () => downloader.Stop();
        downloader.OnProgress += _ => Graphics.Schedule(() =>
        {
            if (!downloader.Stopped)
            {
                this.SetMessage(DownloadText);
                this.SetProgress(downloader.Progress);
            }
        });
        downloader.OnError += e => Graphics.Schedule(() =>
        {
            MessageBox mbox = new MessageBox("Error", "Failed to download file.", ButtonType.OK, IconType.Error);
            mbox.OnClosed += delegate (BaseEventArgs _)
            {
                this.Close();
                this.OnError?.Invoke(e);
            };
        });

        downloader.DownloadAsync();
    }
}
