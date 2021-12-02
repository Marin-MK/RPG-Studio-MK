using System;
using System.Collections.Generic;
using System.Text;
using odl;
using amethyst;

namespace RPGStudioMK.Widgets
{
    public class FileDownloaderWindow : ProgressWindow
    {
        FileDownloader downloader;

        public FileDownloaderWindow(string URL, string Filename, bool CloseWhenDone = true) : base("Downloader", "Downloading file...", CloseWhenDone)
        {
            downloader = new FileDownloader(URL, Filename);
            downloader.OnFinished += delegate (BaseEventArgs e)
            {
                if (CloseWhenDone) this.Close();
                else Buttons[0].SetEnabled(true);
                downloader = null;
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
                };
            };

            if (!CloseWhenDone)
            {
                CreateButton("OK", delegate (BaseEventArgs e) { Close(); });
                Buttons[0].SetEnabled(false);
            }
        }

        public override void Update()
        {
            base.Update();
            downloader?.Update();
        }
    }
}
