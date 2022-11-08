using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace RPGStudioMK;

public class FileDownloader
{
    public string URL { get; protected set; }
    public string Filename { get; protected set; }
    public bool Done { get; protected set; }
    public bool Paused { get; protected set; }
    public bool Stopped { get; protected set; }
    public float Progress { get; protected set; }

    public Action OnFinished;
    public Action OnCancelled;
    public Action<float> OnProgress;
    public Action<Exception> OnError;

    private long BytesRead = 0;
    private long BytesTotal = 0;


    public FileDownloader(string url, string Filename)
    {
        this.URL = url;
        this.Filename = Filename;
    }

    public void Download()
    {
        try
        {
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(this.URL);
            request.Timeout = 5000;
            request.AllowWriteStreamBuffering = false;
            HttpWebResponse response = (HttpWebResponse) request.GetResponse();
            Stream contentstream = response.GetResponseStream();

            // Write to disk
            this.BytesTotal = response.ContentLength;
            FileStream filestream = new FileStream(Filename, FileMode.Create);
            byte[] read = new byte[8192];
            int count = contentstream.Read(read, 0, read.Length);
            while (count > 0)
            {
                filestream.Write(read, 0, count);
                while (Paused)
                {
                    Thread.Sleep(10);
                }
                count = contentstream.Read(read, 0, read.Length);
                this.BytesRead += count;
                this.Progress = this.BytesRead / (float) this.BytesTotal;
                if (!Stopped) this.OnProgress?.Invoke(this.Progress);
            }
            // Close everything
            filestream.Close();
            filestream = null;
            contentstream.Close();
            contentstream = null;
            response.Close();
            response = null;
            if (Stopped)
            {
                // Stopped downloading a not-fully-downloaded file, so to avoid leaving a
                // possibly corrupt file around, we delete it.
                if (File.Exists(Filename)) File.Delete(Filename);
                this.OnCancelled?.Invoke();
            }
            else
            {
                if (this.Progress != 1)
                {
                    this.BytesRead = this.BytesTotal;
                    this.Progress = 1;
                    this.OnProgress?.Invoke(1f);
                }
                this.Done = true;
                this.OnFinished?.Invoke();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error downloading: " + ex.Message + "\n" + ex.StackTrace);
            OnError?.Invoke(ex);
        }
    }

    public async Task DownloadAsync()
    {
        await Task.Run(() => Download());
    }

    /// <summary>
    /// Permanently stops the downloader and deletes the downloaded file if it was not fully downloaded.
    /// </summary>
    public void Stop()
    {
        if (Stopped) throw new Exception("Downloader already stopped.");
        Stopped = true;
    }

    /// <summary>
    /// Temporarily pauses the downloader.
    /// </summary>
    public void Pause()
    {
        if (Paused) throw new Exception("Downloader already paused.");
        Paused = true;
    }

    /// <summary>
    /// Resumes the downloader if it had been paused.
    /// </summary>
    public void Resume()
    {
        if (!Paused) throw new Exception("Downloader was never paused.");
        Paused = false;
    }
}
