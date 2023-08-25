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

public class EncounterManager : BaseDataManager
{
    public EncounterManager() : base("Encounter", "encounters.dat", "encounters.txt", "encounters") { }

    protected override void LoadData()
    {
        base.LoadData();
        Logger.WriteLine("Loading encounters");
        LoadAsHash((key, value) =>
        {
            string ckey = Ruby.Symbol.FromPtr(key);
            int mapid = 0;
            int version = 0;
            if (ckey.Contains('_'))
            {
                mapid = Convert.ToInt32(ckey.Split('_')[0]);
                version = Convert.ToInt32(ckey.Split('_')[1]);
            }
            else mapid = Convert.ToInt32(ckey);
            EncounterTable enct = new EncounterTable(value);
            Data.Encounters.Add((mapid, version), enct);
        });
    }

    protected override void LoadPBS()
    {
        base.LoadPBS();
        Logger.WriteLine("Loading encounters from PBS");
        FormattedTextParser.ParseLineByLineWithHeader(PBSFilename, (id, lines) =>
        {
            int mapid = 0;
            int version = 0;
            if (id.Contains(','))
            {
                mapid = Convert.ToInt32(id.Split(',')[0]);
                version = Convert.ToInt32(id.Split(',')[1]);
            }
            else mapid = Convert.ToInt32(id);
            Data.Encounters.Add((mapid, version), new EncounterTable(mapid, version, lines));
        }, Data.SetLoadProgress);
    }

    protected override void SaveData()
    {
        base.SaveData();
        Logger.WriteLine("Saving encounters");
        SaveAsHash(Data.Encounters.Values, enc => Ruby.Symbol.ToPtr(enc.ID));
    }

    public override void Clear()
    {
        base.Clear();
        Logger.WriteLine("Clearing encounters");
        Data.Encounters.Clear();
    }
}
