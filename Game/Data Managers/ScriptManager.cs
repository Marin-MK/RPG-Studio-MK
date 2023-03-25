using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace RPGStudioMK.Game;

public class ScriptManager : BaseDataManager
{
    public ScriptManager() : base(null, "Scripts.rxdata", null, "scripts") { }

    public override void Load(bool fromPBS = false)
    {
        base.Load(fromPBS);
        if (Directory.Exists(Data.DataPath + "/Scripts"))
            LoadScriptsExternal();
        else LoadScriptsRXDATA();
        #region Code Injection
        bool Inject = false;
        // Injects code at the top of the script list
        if (Inject)
        {
            string startcode = Utilities.GetInjectedCodeStart();
            if (Data.Scripts[0].Name != "RPG Studio MK1")
            {
                if (!string.IsNullOrEmpty(startcode))
                {
                    Script script = new Script();
                    script.Name = "RPG Studio MK1";
                    script.Content = startcode;
                    Data.Scripts.Insert(0, script);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(startcode)) Data.Scripts.RemoveAt(0);
                else Data.Scripts[0].Content = startcode;
            }
            // Injects code at the bottom of the script list, above Main
            string maincode = Utilities.GetInjectedCodeAboveMain();
            if (Data.Scripts.Count < 3 || Data.Scripts[Data.Scripts.Count - 2].Name != "RPG Studio MK2")
            {
                if (!string.IsNullOrEmpty(maincode))
                {
                    Script script = new Script();
                    script.Name = "RPG Studio MK2";
                    script.Content = maincode;
                    Data.Scripts.Insert(Data.Scripts.Count - 1, script);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(maincode)) Data.Scripts.RemoveAt(Data.Scripts.Count - 2);
                else Data.Scripts[Data.Scripts.Count - 2].Content = maincode;
            }
        }
        #endregion
        #region Version Detection
        // Find Essentials version
        for (int i = 0; i < Data.Scripts.Count; i++)
        {
            Script s = Data.Scripts[i];
            Match m = Regex.Match(s.Content, "module Essentials[\t\r\n ]*VERSION[\t\r\n ]*=[\t\r\n ]*\"(.*)\"");
            if (m.Success && !string.IsNullOrEmpty(m.Groups[1].Value)) // v19, v19.1, v20, etc.
            {
                Data.EssentialsVersion = m.Groups[1].Value switch
                {
                    "19" => EssentialsVersion.v19,
                    "19.1" => EssentialsVersion.v19_1,
                    "20" => EssentialsVersion.v20,
                    "20.1" => EssentialsVersion.v20_1,
                    _ => EssentialsVersion.Unknown
                };
                break;
            }
            m = Regex.Match(s.Content, "(ESSENTIALS_VERSION|ESSENTIALSVERSION)[\t\r\n ]*=[\t\r\n ]*\"(.*)\"");
            if (m.Success && !string.IsNullOrEmpty(m.Groups[2].Value)) // v17, v17.1, v17.2, v18, v18.1
            {
                Data.EssentialsVersion = m.Groups[2].Value switch
                {
                    "17" => EssentialsVersion.v17,
                    "17.1" => EssentialsVersion.v17_1,
                    "17.2" => EssentialsVersion.v17_2,
                    "18" => EssentialsVersion.v18,
                    "18.1" => EssentialsVersion.v18_1,
                    _ => EssentialsVersion.Unknown
                };
                break;
            }
        }
        #endregion
    }

    private void LoadScriptsExternal()
    {
        Logger.Write("Loading scripts from external files");
        List<(string, string)>? GetScripts(string Path, int Depth)
        {
            List<(string, string)>? Files = new List<(string, string)>();
            foreach (string File in Directory.GetFiles(Path))
            {
                try
                {
                    StreamReader sr = new StreamReader(global::System.IO.File.OpenRead(File));
                    string filename = global::System.IO.Path.GetFileNameWithoutExtension(File);
                    Match m = Regex.Match(filename, @"^\d+_(.*)$");
                    if (!m.Success) continue;
                    filename = m.Groups[1].Value;
                    Files.Add((filename, sr.ReadToEnd()));
                    sr.Close();
                }
                catch (Exception ex)
                {
                    LoadError("Scripts", ex.Message + "\n\n" + ex.StackTrace);
                    return null;
                }
            }
            foreach (string Directory in Directory.GetDirectories(Path))
            {
                List<(string, string)>? DirFiles = GetScripts(Directory, Depth + 1);
                if (DirFiles == null) return null;
                if (Depth == 0) Files.Add(("==================", ""));
                else Files.Add(("", ""));
                string DirName = global::System.IO.Path.GetFileName(Directory);
                Match m = Regex.Match(DirName, @"^\d+_(.*)$");
                if (!m.Success) continue;
                string SectionName = m.Groups[1].Value;
                Files.Add(($"[[ {SectionName} ]]", ""));
                Files.AddRange(DirFiles);
            }
            return Files;
        }
        List<(string, string)>? Files = GetScripts(Data.DataPath + "/Scripts", 0);
        if (Files == null) return;
        if (Files.Count == 0)
        {
            LoadScriptsRXDATA();
            return;
        }
        foreach ((string Name, string Code) in Files)
        {
            Script Script = new Script();
            Script.Name = Name;
            Script.Content = Code;
            Data.Scripts.Add(Script);
        }
        Data.UsesExternalScripts = true;
    }

    private void LoadScriptsRXDATA()
    {
        Logger.Write("Loading scripts from RXDATA file");
        Data.UsesExternalScripts = false;
        SafeLoad("Scripts.rxdata", File =>
        {
            IntPtr data = Ruby.Marshal.Load(File);
            Ruby.Pin(data);
            for (int i = 0; i < Ruby.Array.Length(data); i++)
            {
                IntPtr script = Ruby.Array.Get(data, i);
                Data.Scripts.Add(new Script(script));
            }
            Ruby.Unpin(data);
        });
    }

    public override void Save()
    {
        base.Save();
        if (Data.UsesExternalScripts) SaveScriptsExternal();
        else SaveScriptsRXDATA();
    }

    private void SaveScriptsExternal()
    {
        Logger.Write("Saving scripts to external files");
        if (!Directory.Exists(Data.DataPath + "/Scripts")) Directory.CreateDirectory(Data.DataPath + "/Scripts");
        else
        {
            // Delete all .rb files and all (\d+)_* folders
            ClearScriptFolder(Data.DataPath + "/Scripts", false);
        }
        string? FirstParent = null;
        string? SecondParent = null;
        int MainCount = 0;
        int SubCount = 0;
        List<(string, string, int)> Tracker = new List<(string, string, int)>(); // First Parent, Second Parent, No. Files
        foreach (Script script in Data.Scripts)
        {
            if (script.Name == "==================")
            {
                FirstParent = null;
                SecondParent = null;
                SubCount = 0;
            }
            else
            {
                Match m = Regex.Match(script.Name, @"\[\[ (.*) \]\]");
                if (m.Success)
                {
                    if (FirstParent == null)
                    {
                        MainCount++;
                        if (m.Groups[1].Value == "Main") FirstParent = "999_" + m.Groups[1].Value;
                        else FirstParent = Utilities.Digits(MainCount, 3) + "_" + m.Groups[1].Value;
                    }
                    else
                    {
                        SubCount++;
                        SecondParent = Utilities.Digits(SubCount, 3) + "_" + m.Groups[1].Value;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(script.Name) && string.IsNullOrEmpty(script.Content)) continue; // We don't write scripts without a name and without content
                    string ScriptPath = null;
                    if (FirstParent == null) ScriptPath = Data.DataPath + "/Scripts";
                    else if (SecondParent == null) ScriptPath = Data.DataPath + "/Scripts/" + FirstParent;
                    else ScriptPath = Data.DataPath + "/Scripts/" + FirstParent + "/" + SecondParent;
                    Directory.CreateDirectory(ScriptPath);
                    (string First, string Second, int Count)? result = Tracker.Find(t => t.Item1 == FirstParent && t.Item2 == SecondParent);
                    int FileCount = result == null ? 0 : result.Value.Count;
                    if (result != null) Tracker.Remove(((string, string, int))result);
                    FileCount += 1;
                    Tracker.Add((FirstParent, SecondParent, FileCount));
                    if (script.Name == "Main") FileCount = 999;
                    ScriptPath += $"/{Utilities.Digits(FileCount, 3)}_{script.Name}.rb";
                    File.WriteAllText(ScriptPath, script.Content);
                }
            }
        }
    }

    private void ClearScriptFolder(string Path, bool Delete)
    {
        foreach (string file in Directory.GetFiles(Path))
        {
            if (file.EndsWith(".rb")) File.Delete(file);
        }
        foreach (string folder in Directory.GetDirectories(Path))
        {
            if (Regex.IsMatch(folder, @"\d+_.*$"))
            {
                ClearScriptFolder(folder, true);
                if (!Utilities.DoesDirectoryHaveAnyFiles(folder)) Directory.Delete(folder);
            }
        }
    }

    private void SaveScriptsRXDATA()
    {
        Logger.Write("Saving scripts to RXDATA file");
        SafeSave(Filename, File =>
        {
            IntPtr scripts = Ruby.Array.Create();
            Ruby.Pin(scripts);
            foreach (Script script in Data.Scripts)
            {
                IntPtr scriptdata = script.Save();
                Ruby.Array.Push(scripts, scriptdata);
            }
            Ruby.Marshal.Dump(scripts, File);
            Ruby.Unpin(scripts);
        });
    }

    public override void Clear()
    {
        base.Clear();
        Logger.Write("Clearing scripts");
        Data.Scripts.Clear();
    }
}
