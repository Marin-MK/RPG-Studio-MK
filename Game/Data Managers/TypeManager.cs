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

public class TypeManager : BaseDataManager
{
    public TypeManager() : base("Type", "types.dat", "types.txt", "types") { }

    protected override void LoadData()
    {
        base.LoadData();
        Logger.Write("Loading types");
        LoadAsHash((key, value) =>
        {
            string ckey = Ruby.Symbol.FromPtr(key);
            Data.Types.Add(ckey, new Type(value));
        });
    }

    protected override void LoadPBS()
    {
        base.LoadPBS();
        Logger.Write("Loading types from PBS");
        FormattedTextParser.ParseSectionBasedFile(PBSFilename, (id, hash) =>
        {
            Data.Types.Add(id, new Type(id, hash));
        }, Data.SetLoadProgress);
    }

    protected override void SaveData()
    {
        base.SaveData();
        Logger.Write("Saving types");
        SaveAsHash(Data.Types.Values, t => Ruby.Symbol.ToPtr(t.ID));
    }

    public override void Clear()
    {
        base.Clear();
        Logger.Write("Clearing types");
        Data.Types.Clear();
    }
}
