using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;

namespace RPGStudioMK.Game;

public class GameConfigManager : BaseDataManager
{
    public GameConfigManager()
        : base(null, "Game.ini", null, "game config", false) { }

    private static Encoding win1252;

    protected override void LoadData()
    {
        base.LoadData();
        if (win1252 == null)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            win1252 = Encoding.GetEncoding("windows-1252");
        }
        string data = File.ReadAllText($"{Data.ProjectPath}/{Filename}", win1252);
        Match m = Regex.Match(data, @"Title=(.*)\n"); // \r will be included in the match, so we trim that out.
        if (m.Success && !string.IsNullOrEmpty(m.Groups[1].Value.Trim()))
        {
            Editor.ProjectSettings.ProjectName = m.Groups[1].Value.Trim();
        }
    }

    protected override void SaveData()
    {
        base.SaveData();
        string inifilename = $"{Data.ProjectPath}/{Filename}";
        string data = File.ReadAllText(inifilename, win1252);
        data = Regex.Replace(data, @"Title=.*\n", $"Title={Editor.ProjectSettings.ProjectName}{Environment.NewLine}");
        File.WriteAllText(inifilename, data, win1252);
    }

    public override void Clear()
    {
        base.Clear();
        Editor.ProjectSettings.ProjectName = null;
    }
}
