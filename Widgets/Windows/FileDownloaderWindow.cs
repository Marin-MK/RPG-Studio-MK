using System.Collections.Generic;

namespace RPGStudioMK.Widgets;

public class FileDownloaderWindow : ProgressWindow
{
    FileDownloader downloader;

    public BaseEvent OnFinished;
    public BaseEvent OnError;

    public FileDownloaderWindow(string URL, string Filename, string DownloadText = "Downloading file...", bool CloseWhenDone = true) : base("Downloader", DownloadText, CloseWhenDone)
    {
        downloader = new FileDownloader(URL, Filename);
        downloader.OnFinished += delegate (BaseEventArgs e)
        {
            downloader = null;
            if (CloseWhenDone && !Disposed) this.Close();
            else if (Buttons.Count > 0) Buttons[0].SetEnabled(true);
            this.OnFinished?.Invoke(new BaseEventArgs());
        };
        downloader.OnProgress += delegate (BaseEventArgs e)
        {
            this.SetProgress(downloader.Progress);
        };
        downloader.OnError += delegate (ErrorEventArgs e)
        {
            MessageBox mbox = new MessageBox("Error", "Failed to download file.", ButtonType.OK, IconType.Error);
            mbox.OnClosed += delegate (BaseEventArgs _)
            {
                this.Close();
                this.OnError?.Invoke(new BaseEventArgs());
            };
        };

        if (!CloseWhenDone)
        {
            CreateButton("OK", _ => OK());
            Buttons[0].SetEnabled(false);

            RegisterShortcuts(new List<Shortcut>()
            {
                new Shortcut(this, new Key(Keycode.ENTER, Keycode.CTRL), _ => OK(), true, e => e.Value = Buttons[0].Enabled)
            });
        }
    }

    private void OK()
    {
        Close();
    }

    public override void Update()
    {
        base.Update();
        downloader?.Update();
    }
}
