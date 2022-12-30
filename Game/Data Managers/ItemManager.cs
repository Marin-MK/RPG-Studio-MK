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

public class ItemManager : BaseDataManager
{
    public ItemManager(bool FromPBS = false)
        : base("Item", "items.dat", "items.txt", "items", FromPBS) { }

    protected override void LoadData()
    {
        base.LoadData();
        LoadAsHash((key, value) =>
        {
            string item = Ruby.Symbol.FromPtr(key);
            Data.Items.Add(item, new Item(value));
        });
    }

    protected override void LoadPBS()
    {
        base.LoadPBS();
        FormattedTextParser.ParseSectionBasedFile(PBSFilename, (id, hash) =>
        {
            Data.Items.Add(id, new Item(id, hash));
        }, Data.SetLoadProgress);
    }

    protected override void SaveData()
    {
        base.SaveData();
        SaveAsHash(Data.Items.Values, e => Ruby.Symbol.ToPtr(e.ID));
    }

    public override void Clear()
    {
        base.Clear();
        Data.Items.Clear();
    }
}
