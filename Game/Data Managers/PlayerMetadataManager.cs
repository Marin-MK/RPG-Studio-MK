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

public class PlayerMetadataManager : BaseDataManager
{
    // This file cannot be loaded from PBS; if you want it to come from PBS instead,
    // make the MetadataManager load from PBS.
    public PlayerMetadataManager()
        : base("PlayerMetadata", "player_metadata.dat", null, "player metadata", false) { }

    public override void Load()
    {
        if (Data.LoadedMetadataFromPBS) return;
        base.Load();
    }

    protected override void LoadData()
    {
        base.LoadData();
        LoadAsHash((key, value) =>
        {
            int ckey = (int) Ruby.Integer.FromPtr(key);
            Data.PlayerMetadata.Add(ckey, new PlayerMetadata(value));
        });
    }

    // The PBS parser for this data file is included in the global metadata PBS parser,
    // as that PBS file contains both the global metadata and the player metadata.

    protected override void SaveData()
    {
        base.SaveData();
        SaveAsHash(Data.PlayerMetadata.Values, m => Ruby.Integer.ToPtr(m.ID));
    }

    public override void Clear()
    {
        base.Clear();
        Data.PlayerMetadata.Clear();
    }
}
