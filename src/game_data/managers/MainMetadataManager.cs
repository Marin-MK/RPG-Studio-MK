namespace RPGStudioMK.Game;

public class MainMetadataManager : BaseDataManager
{
    PlayerMetadataManager playerMetadataManager;
    MetadataManager metadataManager;
    MapMetadataManager mapMetadataManager;

    public MainMetadataManager() : base(null, null, null, "metadata")
    {
        playerMetadataManager = new PlayerMetadataManager();
        metadataManager = new MetadataManager();
        mapMetadataManager = new MapMetadataManager();
    }

    public override void InitializeClass()
    {
        playerMetadataManager.InitializeClass();
        metadataManager.InitializeClass();
        mapMetadataManager.InitializeClass();
    }

    public override void Load(bool fromPBS)
    {
        base.Load(fromPBS);
        playerMetadataManager.Load(fromPBS);
        metadataManager.Load(fromPBS);
        mapMetadataManager.Load(fromPBS);
    }

    public override void Save()
    {
        base.Save();
        playerMetadataManager.Save();
        metadataManager.Save();
        mapMetadataManager.Save();
    }

    public override void Clear()
    {
        base.Clear();
        playerMetadataManager.Clear();
        metadataManager.Clear();
        mapMetadataManager.Clear();
    }
}
