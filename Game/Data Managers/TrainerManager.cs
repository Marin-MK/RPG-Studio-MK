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

public class TrainerManager : BaseDataManager
{
    public TrainerManager(bool FromPBS = false)
        : base("Trainer", "trainers.dat", "trainers.txt", "trainers", FromPBS) { }

    protected override void LoadData()
    {
        base.LoadData();
        LoadAsHash((key, value) => Data.Trainers.Add(new Trainer(value)));
    }

    protected override void LoadPBS()
    {
        base.LoadPBS();
        FormattedTextParser.ParseSectionBasedFileWithOrder(PBSFilename, (id, pairs) =>
        {
            Data.Trainers.Add(new Trainer(id, pairs));
        }, Data.SetLoadProgress);
    }

    protected override void SaveData()
    {
        base.SaveData();
        SaveAsHash(Data.Trainers, t =>
        {
            nint Array = Ruby.Array.Create();
            Ruby.Pin(Array);
            Ruby.Array.Push(Array, Ruby.Symbol.ToPtr(t.TrainerType));
            Ruby.Array.Push(Array, Ruby.String.ToPtr(t.Name));
            Ruby.Array.Push(Array, Ruby.Integer.ToPtr(t.Version));
            Ruby.Unpin(Array);
            return Array;
        });
    }

    public override void Clear()
    {
        base.Clear();
        Data.Trainers.Clear();
    }
}
