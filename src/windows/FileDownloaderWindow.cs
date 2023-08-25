using System;

namespace RPGStudioMK.Widgets;

public class FileDownloaderWindow : ProgressWindow
{
    MKUtils.Downloader downloader;

    public Action<Exception> OnError;

    string DownloadText;

    public FileDownloaderWindow(string URL, string Filename, string DownloadText = "Downloading File...", bool CloseWhenDone = true, bool Cancellable = true) :
        base("Downloader", "Connecting to server...", CloseWhenDone, Cancellable, false, true)
    {
        this.DownloadText = DownloadText;
        Logger.WriteLine("Initializing downloader...");
        downloader = new MKUtils.Downloader(URL, Filename);
        this.OnCancelled += () => downloader.Cancel();
        downloader.OnError += e => 
        {
            Logger.Error("Downloader failed: " + e.Message + "\n" + e.StackTrace);
            MessageBox mbox = new MessageBox("Error", "Failed to download file.", ButtonType.OK, IconType.Error);
            mbox.OnClosed += delegate (BaseEventArgs _)
            {
                this.Close();
                this.OnError?.Invoke(e);
            };
        };
    }

    public void Download()
    {
        downloader.Download(null, new MKUtils.DynamicCallbackManager<MKUtils.DownloadProgress>(TimeSpan.FromMilliseconds(16), x =>
        {
            SetMessage(DownloadText);
            SetProgress((float) x.Factor);
            Window.UI.Update();
            Graphics.Update();
        }, () => { Window.UI.Update(); Graphics.Update(); })
        { ForceFirstUpdate = true });
    }
}
