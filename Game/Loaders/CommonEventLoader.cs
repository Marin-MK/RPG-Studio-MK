using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Game;

public static partial class Data
{
    private static void LoadCommonEvents()
    {
        SafeLoad("CommonEvents.rxdata", File =>
        {
            IntPtr list = Ruby.Marshal.Load(File);
            Ruby.Pin(list);
            for (int i = 1; i < Ruby.Array.Length(list); i++)
            {
                CommonEvent ce = new CommonEvent(Ruby.Array.Get(list, i));
                CommonEvents.Add(ce);
            }
            Ruby.Unpin(list);
        });
    }

    private static void SaveCommonEvents()
    {
        SafeSave("CommonEvents.rxdata", File =>
        {
            IntPtr list = Ruby.Array.Create();
            Ruby.Pin(list);
            for (int i = 0; i < CommonEvents.Count; i++)
            {
                Ruby.Array.Set(list, i + 1, CommonEvents[i].Save());
            }
            Ruby.Marshal.Dump(list, File);
            Ruby.Unpin(list);
        });
    }
}
