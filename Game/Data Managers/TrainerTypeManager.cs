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

public class TrainerTypeManager : BaseDataManager
{
    public TrainerTypeManager()
        : base("TrainerType", "trainer_types.dat", "trainer_types.txt", "trainer types") { }

    protected override void LoadData()
    {
        base.LoadData();
        Logger.WriteLine("Loading trainer types");
        LoadAsHash((key, value) =>
        {
            string ckey = Ruby.Symbol.FromPtr(key);
            Data.TrainerTypes.Add(ckey, new TrainerType(value));
        });
    }

    protected override void LoadPBS()
    {
        base.LoadPBS();
        if (Game.Data.IsVersionAtLeast(EssentialsVersion.v20))
        {
            Logger.WriteLine("Loading trainer types from PBS with EV >= v20");
            FormattedTextParser.ParseSectionBasedFile(PBSFilename, (id, hash) =>
            {
                Data.TrainerTypes.Add(id, new TrainerType(id, hash));
            }, Data.SetLoadProgress);
        }
        else
        {
            Logger.WriteLine("Loading trainer types from PBS with EV < v20");
            FormattedTextParser.ParseLineByLineCommaBased("trainertypes.txt", line =>
            {
                TrainerType trainertypedata = new TrainerType(line);
                Data.TrainerTypes.Add(trainertypedata.ID, trainertypedata);
            }, Data.SetLoadProgress);
        }
    }

    protected override void SaveData()
    {
        base.SaveData();
        Logger.WriteLine("Saving trainer types");
        SaveAsHash(Data.TrainerTypes.Values, t => Ruby.Symbol.ToPtr(t.ID));
    }

    public override void Clear()
    {
        base.Clear();
        Logger.WriteLine("Clearing trainer types");
        Data.TrainerTypes.Clear();
    }
}
