using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Game;

public static partial class Data
{
    private static void LoadSystem()
    {
        SafeLoad("System.rxdata", File =>
        {
            IntPtr data = Ruby.Marshal.Load(File);
            Ruby.Pin(data);
            System = new System(data);
            Ruby.Unpin(data);
        });
    }

    private static void SaveSystem()
    {
        System.EditMapID = Editor.ProjectSettings.LastMapID;
        SafeSave("System.rxdata", File =>
        {
            IntPtr data = System.Save();
            Ruby.Pin(data);
            Ruby.Marshal.Dump(data, File);
            Ruby.Unpin(data);
        });
    }
}
