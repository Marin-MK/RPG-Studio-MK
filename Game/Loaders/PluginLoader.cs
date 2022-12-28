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

public static partial class Data
{
    private static void LoadPlugins()
    {
        List<string> PluginDirs = Directory.EnumerateDirectories(Data.ProjectPath + "/Plugins").ToList();
        for (int i = 0; i < PluginDirs.Count; i++)
        {
            string PluginDirectory = PluginDirs[i];
            GamePlugin p = new GamePlugin(PluginDirectory);
            Data.Plugins.Add(p);
            SetLoadProgress((float) i / (PluginDirs.Count - 1));
        }
    }
}
