namespace RPGStudioMK.Game;

public class PlayerMetadataManager : BaseDataManager
{
    // This file cannot be loaded from PBS; if you want it to come from PBS instead,
    // make the MetadataManager load from PBS.
    public PlayerMetadataManager()
        : base("PlayerMetadata", "player_metadata.dat", null, "player metadata") { }

    public override void Load(bool fromPBS = false)
    {
        if (!Game.Data.IsVersionAtLeast(EssentialsVersion.v20)) return;
        base.Load(fromPBS);
    }

    protected override void LoadData()
    {
        base.LoadData();
        Logger.WriteLine("Loading player metadata");
        LoadAsHash((key, value) =>
        {
            int ckey = (int) Ruby.Integer.FromPtr(key);
            Data.PlayerMetadata.Add(ckey, new PlayerMetadata(value, false));
        });
    }

    // The PBS parser for this data file is included in the global metadata PBS parser,
    // as that PBS file contains both the global metadata and the player metadata.

    public override void Save()
    {
        if (!Game.Data.IsVersionAtLeast(EssentialsVersion.v20)) return;
        base.Save();
    }

    protected override void SaveData()
    {
        base.SaveData();
        Logger.WriteLine("Saving player metadata");
        SaveAsHash(Data.PlayerMetadata.Values, m => Ruby.Integer.ToPtr(m.ID));
    }

    public override void Clear()
    {
        base.Clear();
        Logger.WriteLine("Clearing player metadata");
        Data.PlayerMetadata.Clear();
    }
}
