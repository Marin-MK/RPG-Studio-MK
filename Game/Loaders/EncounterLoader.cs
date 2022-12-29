using RPGStudioMK.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static rubydotnet.Ruby;

namespace RPGStudioMK.Game;

public static partial class Data
{
    private static void LoadEncounters()
    {
        SafeLoad("encounters.dat", File =>
        {
            nint Data = Marshal.Load(File);
            Ruby.Pin(Data);
            nint Keys = Ruby.Hash.Keys(Data);
            Ruby.Pin(Keys);
            int KeyCount = (int) Ruby.Array.Length(Keys);
            for (int i = 0; i < KeyCount; i++)
            {
                nint key = Ruby.Array.Get(Keys, i);
                nint robj = Ruby.Hash.Get(Data, key);
                string ckey = Ruby.Symbol.FromPtr(key);
                int mapid = Convert.ToInt32(ckey.Split('_')[0]);
                int version = Convert.ToInt32(ckey.Split('_')[1]);
                EncounterTable enct = new EncounterTable(robj);
                Encounters.Add((mapid, version), enct);
                SetLoadProgress((float) i / (KeyCount - 1));
            }
            Ruby.Unpin(Keys);
            Ruby.Unpin(Data);
        });
    }

    private static void LoadEncountersFromPBS(string FilePath)
    {
        FormattedTextParser.ParseLineByLineWithHeader(FilePath, (id, lines) =>
        {
            int mapid = 0;
            int version = 0;
            if (id.Contains(','))
            {
                mapid = Convert.ToInt32(id.Split(',')[0]);
                version = Convert.ToInt32(id.Split(',')[1]);
            }
            else mapid = Convert.ToInt32(id);
            Encounters.Add((mapid, version), new EncounterTable(mapid, version, lines));
        });
    }

    private static void SaveEncounters()
    {
        SafeSave("encounters.dat", File =>
        {
            nint Data = Ruby.Hash.Create();
            Ruby.Pin(Data);
            foreach ((int MapID, int Version) in Encounters.Keys)
            {
                nint rkey = Ruby.Symbol.ToPtr(MapID.ToString() + "_" + Version.ToString());
                nint tdata = Encounters[(MapID, Version)].Save();
                Ruby.Hash.Set(Data, rkey, tdata);
            }
            Ruby.Marshal.Dump(Data, File);
            Ruby.Unpin(Data);
        });
    }
}
