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
    public TypeManager(bool FromPBS = false)
        : base("Type", "types.dat", "types.txt", "types", FromPBS) { }

    protected override void LoadData()
    {
        base.LoadData();
        LoadAsHash((key, value) =>
        {
            string ckey = Ruby.Symbol.FromPtr(key);
            Data.Types.Add(ckey, new Type(value));
        });
    }

    protected override void LoadPBS()
    {
        base.LoadPBS();
        FormattedTextParser.ParseSectionBasedFile(PBSFilename, (id, hash) =>
        {
            Data.Types.Add(id, new Type(id, hash));
        }, Data.SetLoadProgress);
    }

    protected override void SaveData()
    {
        base.SaveData();
        SaveAsHash(Data.Types.Values, t => Ruby.Symbol.ToPtr(t.ID));
    }

    public override void Clear()
    {
        base.Clear();
        Data.Types.Clear();
    }
}
