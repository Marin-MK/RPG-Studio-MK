using RPGStudioMK.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RPGStudioMK.Game;

public static class PluginManager
{
    public static void LoadAll(bool WithProgress)
    {
        List<string> PluginDirs = Directory.EnumerateDirectories(Data.ProjectPath + "/Plugins").ToList();
        for (int i = 0; i < PluginDirs.Count; i++)
        {
            string PluginDirectory = PluginDirs[i];
            GamePlugin p = new GamePlugin(PluginDirectory);
            Data.Plugins.Add(p);
            if (WithProgress) Data.SetLoadProgress((float) i / (PluginDirs.Count - 1));
        }
    }
}
