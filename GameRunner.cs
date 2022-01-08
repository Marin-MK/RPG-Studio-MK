using System;
using System.Diagnostics;
using System.IO;
using RPGStudioMK.Game;

namespace RPGStudioMK;

public class GameRunner
{
    private Process Process;
    private StreamWriter StreamWriter;

    public bool Running { get { return !this.Process.HasExited; } }
    public TextEvent OnDataOutput;

    public GameRunner()
    {
        this.Process = new Process();
        this.Process.StartInfo.FileName = Data.ProjectPath + "/Game.exe";
        this.Process.StartInfo.Arguments = "debug";
        //this.Process.StartInfo.RedirectStandardInput = true;
        //this.Process.StartInfo.RedirectStandardOutput = true;
        //this.Process.StartInfo.UseShellExecute = false;
        
        this.Process.Start();
        //this.Process.BeginOutputReadLine();

        //this.Process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
        //{
        //    this.OnDataOutput?.Invoke(new TextEventArgs(e.Data, null));
        //};

        //this.StreamWriter = this.Process.StandardInput;
    }

    public void Write(string Text, bool WriteInstantly = true)
    {
        this.StreamWriter.Write(Text);
        if (WriteInstantly) this.Flush();
    }

    public void WriteLine(string Text, bool WriteInstantly = true)
    {
        this.StreamWriter.WriteLine(Text);
        if (WriteInstantly) this.Flush();
    }

    public void Flush()
    {
        this.StreamWriter.Flush();
    }
}
