using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;

namespace RPGStudioMK.Game;

public static partial class Data
{
    private static Encoding win1252;

    private static void LoadGameINI()
    {
        if (win1252 == null)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            win1252 = Encoding.GetEncoding("windows-1252");
        }
        string data = File.ReadAllText(ProjectPath + "/Game.ini", win1252);
        Match m = Regex.Match(data, @"Title=(.*)\n"); // \r will be included in the match, so we trim that out.
        if (m.Success && !string.IsNullOrEmpty(m.Groups[1].Value.Trim()))
        {
            Editor.ProjectSettings.ProjectName = m.Groups[1].Value.Trim();
        }
    }

    private static void SaveGameINI()
    {
        string data = File.ReadAllText(ProjectPath + "/Game.ini", win1252);
        data = Regex.Replace(data, @"Title=.*\n", $"Title={Editor.ProjectSettings.ProjectName}{Environment.NewLine}");
        File.WriteAllText(ProjectPath + "/Game.ini", data, win1252);
    }
}
