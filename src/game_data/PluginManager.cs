using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RPGStudioMK.Game;

public static class PluginManager
{
    public static void LoadAll(bool WithProgress)
    {
        if (!Directory.Exists(Data.ProjectPath + "/Plugins")) return;
        List<string> PluginDirs = Directory.EnumerateDirectories(Data.ProjectPath + "/Plugins").ToList();
        for (int i = 0; i < PluginDirs.Count; i++)
        {
            string PluginDirectory = PluginDirs[i];
            GamePlugin p = new GamePlugin(PluginDirectory);
            Data.Plugins.Add(p);
            if (WithProgress) Data.SetLoadProgress((float) i / (PluginDirs.Count - 1));
        }
    }

    public static void SaveAll()
    {
        Data.Plugins.ForEach(p => p.Save());
    }
}
