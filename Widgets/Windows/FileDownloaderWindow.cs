using System;
using System.Collections.Generic;
using System.Text;
using odl;
using amethyst;

namespace RPGStudioMK.Widgets
{
    public class FileDownloaderWindow : PopupWindow
    {
        FileDownloader dl;

        public FileDownloaderWindow(string URL, string Filename, bool CloseWhenDone = true)
        {
            SetTitle("Downloader");
            MinimumSize = MaximumSize = new Size(300, CloseWhenDone ? 100 : 120);
            SetSize(MaximumSize);
            Center();

            Label downloadlabel = new Label(this);
            downloadlabel.SetFont(Fonts.UbuntuRegular.Use(14));
            downloadlabel.SetText("Downloading file...");
            downloadlabel.SetPosition(Size.Width / 2 - downloadlabel.Size.Width / 2, 24);

            ProgressBar w = new ProgressBar(this);
            w.SetPosition(8, 48);
            w.SetSize(Size.Width - 16, 32);

            dl = new FileDownloader(URL, Filename);
            dl.OnFinished += delegate (BaseEventArgs e)
            {
                if (CloseWhenDone) this.Close();
                else Buttons[0].SetEnabled(true);
                dl = null;
            };
            dl.OnProgress += delegate (BaseEventArgs e)
            {
                w.SetProgress(dl.Progress);
            };
            dl.OnError += delegate (ErrorEventArgs e)
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
            dl?.Update();
        }
    }
}
