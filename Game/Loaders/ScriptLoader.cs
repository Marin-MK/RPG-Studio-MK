using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace RPGStudioMK.Game;

public static partial class Data
{
    private static void LoadScripts()
    {
        if (Directory.Exists(DataPath + "/Scripts"))
            LoadScriptsExternal();
        else LoadScriptsRXDATA();
    }

    private static void LoadScriptsExternal()
    {
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
        List<(string, string)>? Files = GetScripts(DataPath + "/Scripts", 0);
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
            Scripts.Add(Script);
        }
        UsesExternalScripts = true;
    }

    private static void LoadScriptsRXDATA()
    {
        UsesExternalScripts = false;
        SafeLoad("Scripts.rxdata", File =>
        {
            IntPtr data = Ruby.Marshal.Load(File);
            Ruby.Pin(data);
            for (int i = 0; i < Ruby.Array.Length(data); i++)
            {
                IntPtr script = Ruby.Array.Get(data, i);
                Scripts.Add(new Script(script));
            }
            Ruby.Unpin(data);
            bool Inject = false;
            // Injects code at the top of the script list
            if (Inject)
            {
                string startcode = Utilities.GetInjectedCodeStart();
                if (Scripts[0].Name != "RPG Studio MK1")
                {
                    if (!string.IsNullOrEmpty(startcode))
                    {
                        Script script = new Script();
                        script.Name = "RPG Studio MK1";
                        script.Content = startcode;
                        Scripts.Insert(0, script);
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(startcode)) Scripts.RemoveAt(0);
                    else Scripts[0].Content = startcode;
                }
                // Injects code at the bottom of the script list, above Main
                string maincode = Utilities.GetInjectedCodeAboveMain();
                if (Scripts.Count < 3 || Scripts[Scripts.Count - 2].Name != "RPG Studio MK2")
                {
                    if (!string.IsNullOrEmpty(maincode))
                    {
                        Script script = new Script();
                        script.Name = "RPG Studio MK2";
                        script.Content = maincode;
                        Scripts.Insert(Scripts.Count - 1, script);
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(maincode)) Scripts.RemoveAt(Scripts.Count - 2);
                    else Scripts[Scripts.Count - 2].Content = maincode;
                }
            }
            // Find Essentials version
            for (int i = 0; i < Scripts.Count; i++)
            {
                Script s = Scripts[i];
                Match m = Regex.Match(s.Content, "module Essentials[\t\r\n ]*VERSION[\t\r\n ]*=[\t\r\n ]*\"(.*)\"");
                if (m.Success && !string.IsNullOrEmpty(m.Groups[1].Value)) // v19, v19.1, v20, etc.
                {
                    EssentialsVersion = m.Groups[1].Value switch
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
                    EssentialsVersion = m.Groups[2].Value switch
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
        });
    }

    private static void SaveScripts()
    {
        if (UsesExternalScripts) SaveScriptsExternal();
        else SaveScriptsRXDATA();
    }

    private static void SaveScriptsExternal()
    {
        if (!Directory.Exists(DataPath + "/Scripts")) Directory.CreateDirectory(DataPath + "/Scripts");
        else
        {
            // Delete all .rb files and all (\d+)_* folders
            ClearScriptFolder(DataPath + "/Scripts", false);
        }
        string? FirstParent = null;
        string? SecondParent = null;
        int MainCount = 0;
        int SubCount = 0;
        List<(string, string, int)> Tracker = new List<(string, string, int)>(); // First Parent, Second Parent, No. Files
        foreach (Script script in Scripts)
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
                    if (FirstParent == null) ScriptPath = DataPath + "/Scripts";
                    else if (SecondParent == null) ScriptPath = DataPath + "/Scripts/" + FirstParent;
                    else ScriptPath = DataPath + "/Scripts/" + FirstParent + "/" + SecondParent;
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

    private static void ClearScriptFolder(string Path, bool Delete)
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

    private static void SaveScriptsRXDATA()
    {
        SafeSave("Scripts.rxdata", File =>
        {
            IntPtr scripts = Ruby.Array.Create();
            Ruby.Pin(scripts);
            foreach (Script script in Scripts)
            {
                IntPtr scriptdata = script.Save();
                Ruby.Array.Push(scripts, scriptdata);
            }
            Ruby.Marshal.Dump(scripts, File);
            Ruby.Unpin(scripts);
        });
    }
}
