using System;
using System.IO;
using System.Net;
using odl;
using System.Threading;

namespace RPGStudioMK
{
    public class FileDownloader
    {
        public string URL { get; protected set; }
        public bool Done { get; protected set; }
        public bool Paused { get { return PauseThread; } }
        public float Progress { get; protected set; }

        public BaseEvent OnFinished;
        public BaseEvent OnProgress;
        public ErrorEvent OnError;

        private long BytesRead = 0;
        private long BytesTotal = 0;
        private bool StopThread = false;
        private bool PauseThread = false;
        private bool ShownError = false;

        private Thread DownloadThread;
        private Exception ThreadException = null;

        public FileDownloader(string url, string Filename)
        {
            this.URL = url;
            DownloadThread = new Thread(delegate ()
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
                    request.Timeout = 5000;
                    request.AllowWriteStreamBuffering = false;
                    HttpWebResponse response = (HttpWebResponse) request.GetResponse();
                    Stream contentstream = response.GetResponseStream();

                    // Write to disk
                    this.BytesTotal = response.ContentLength;
                    FileStream filestream = new FileStream(Filename, FileMode.Create);
                    byte[] read = new byte[8192];
                    int count = contentstream.Read(read, 0, read.Length);
                    bool stopped = false;
                    while (count > 0)
                    {
                        filestream.Write(read, 0, count);
                        if (StopThread)
                        {
                            stopped = true;
                            break;
                        }
                        else if (PauseThread)
                        {
                            while (PauseThread)
                            {
                                Thread.Sleep(10);
                            }
                        }
                        count = contentstream.Read(read, 0, read.Length);
                        this.BytesRead += count;
                    }
                    // Close everything
                    filestream.Close();
                    filestream = null;
                    contentstream.Close();
                    contentstream = null;
                    response.Close();
                    response = null;
                    if (stopped)
                    {
                        // Stopped downloading a not-fully-downloaded file, so to avoid leaving a
                        // possibly corrupt file around, we delete it.
                        File.Delete(Filename);
                    }
                    else this.Done = true;
                }
                catch (Exception ex)
                {
                    ThreadException = ex;
                }
            });
            DownloadThread.Start();
        }

        /// <summary>
        /// Must be called to invoke the progress/finish events, and to detect errors.
        /// </summary>
        public void Update()
        {
            if (ShownError) return;
            if (ThreadException != null)
            {
                OnError?.Invoke(new odl.ErrorEventArgs(ThreadException));
                ShownError = true;
                return;
            }
            float OldProgress = this.Progress;
            if (Done) Progress = 1;
            else if (BytesTotal != 0) Progress = BytesRead / (float) BytesTotal;
            else Progress = 0;
            if (this.Progress != OldProgress && !StopThread) OnProgress?.Invoke(new BaseEventArgs());
            if (this.Done)
            {
                this.Progress = 1;
                OnProgress?.Invoke(new BaseEventArgs());
                OnFinished?.Invoke(new BaseEventArgs());
            }
        }

        public void Stop()
        {
            if (StopThread) throw new Exception("Downloading already stopped.");
            StopThread = true;
        }

        public void Pause()
        {
            if (PauseThread) throw new Exception("Downloading already paused.");
            PauseThread = true;
        }

        public void Resume()
        {
            if (!PauseThread) throw new Exception("Downloading was never paused.");
            PauseThread = false;
        }
    }
}
