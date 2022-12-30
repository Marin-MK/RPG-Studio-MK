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

public class MoveManager : BaseDataManager
{
    public MoveManager(bool FromPBS = false)
        : base("Move", "moves.dat", "moves.txt", "moves", FromPBS) { }

    protected override void LoadData()
    {
        base.LoadData();
        LoadAsHash((key, value) =>
        {
            string ckey = Ruby.Symbol.FromPtr(key);
            Data.Moves.Add(ckey, new Move(value));
        });
    }

    protected override void LoadPBS()
    {
        base.LoadPBS();
        FormattedTextParser.ParseSectionBasedFile(PBSFilename, (id, hash) =>
        {
            Data.Moves.Add(id, new Move(id, hash));
        }, Data.SetLoadProgress);
    }

    protected override void SaveData()
    {
        base.SaveData();
        SaveAsHash(Data.Moves.Values, m => Ruby.Symbol.ToPtr(m.ID));
    }

    public override void Clear()
    {
        base.Clear();
        Data.Moves.Clear();
    }
}