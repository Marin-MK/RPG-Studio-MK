using RPGStudioMK.Utility;
using System;
using System.Linq;
using System.Text;
using static rubydotnet.Ruby;

namespace RPGStudioMK.Game;

public class MetadataManager : BaseDataManager
{
    // Loading the metadata from PBS will also load player metadata from PBS (as it's the same file)
    public MetadataManager() : base("Metadata", "metadata.dat", "metadata.txt", "metadata") { }

    protected override void LoadData()
    {
        base.LoadData();
        Logger.WriteLine("Loading metadata");
        SafeLoad(Filename, File =>
        {
            nint Data = Marshal.Load(File);
            Ruby.Pin(Data);
            nint mainmetadata = Ruby.Hash.Get(Data, Ruby.Integer.ToPtr(0));
            if (!Game.Data.IsVersionAtLeast(EssentialsVersion.v20))
            {
                Logger.WriteLine("Loading player metadata too since EV < v20");
                // Also load player metadata in v19; disables PlayerMetadataManager
                string[] players = { "player_A", "player_B", "player_C", "player_D", "player_E", "player_F", "player_G", "player_H" };
                for (int i = 0; i < players.Length; i++)
                {
                    nint playerdata = Ruby.GetIVar(mainmetadata, "@" + players[i]);
                    if (playerdata != Ruby.Nil)
                    {
                        Game.Data.PlayerMetadata.Add(i, new PlayerMetadata(playerdata, true) { ID = i + 1 });
                    }
                }
            }
            Game.Data.Metadata = new Metadata(mainmetadata);
            Ruby.Unpin(Data);
        });
    }

    protected override void LoadPBS()
    {
        base.LoadPBS();
        if (Game.Data.IsVersionAtLeast(EssentialsVersion.v20))
        {
            Logger.WriteLine("Loading metadata and player metadata from PBS with EV >= v20");
            FormattedTextParser.ParseSectionBasedFile(PBSFilename, (id, hash) =>
            {
                int nid = Convert.ToInt32(id);
                if (nid == 0) // Global metadata
                {
                    Logger.WriteLine("Loading metadata from PBS with EV >= v20");
                    Data.Metadata = new Metadata(nid, hash);
                }
                else
                {
                    Logger.WriteLine($"Loading player {nid} metadata from PBS with EV >= v20");
                    Data.PlayerMetadata.Add(nid, new PlayerMetadata(nid, hash));
                }
            }, Data.SetLoadProgress);
        }
        else
        {
            Logger.WriteLine("Loading metadata, player metadata and map metadata from PBS with EV < v20");
            FormattedTextParser.ParseSectionBasedFile(PBSFilename, (id, hash) =>
            {
                int nid = Convert.ToInt32(id);
                if (nid == 0) // Metadata and player metadata
                {
                    Logger.WriteLine("Loading metadata from PBS with EV < v20");
                    Data.Metadata = new Metadata(nid, hash);
                    string[] players = { "PlayerA", "PlayerB", "PlayerC", "PlayerD", "PlayerE", "PlayerF", "PlayerG", "PlayerH" };
                    for (int i = 0; i < players.Length; i++)
                    {
                        if (hash.ContainsKey(players[i]))
                        {
                            Logger.WriteLine("Loading player {0} metadata from PBS with EV < v20", i + 1);
                            Game.Data.PlayerMetadata.Add(i + 1, new PlayerMetadata(hash[players[i]].Split(',').ToList()) { ID = i + 1 });
                        }
                    }
                    Logger.WriteLine("Loading map metadata from PBS with EV < v20");
                }
                else
                {
                    // Map metadata
                    Map map = Data.Maps[nid];
                    MapMetadataManager.SetMapMetadata(map, hash);
                }
            });
        }
    }

    protected override void SaveData()
    {
        base.SaveData();
        Logger.WriteLine("Saving metadata");
        SafeSave(Filename, File =>
        {
            nint Data = Ruby.Hash.Create();
            Ruby.Pin(Data);
            Ruby.Hash.Set(Data, Ruby.Integer.ToPtr(0), Game.Data.Metadata.Save());
            Ruby.Marshal.Dump(Data, File);
            Ruby.Unpin(Data);
        });
    }

	protected override void SavePBS()
	{
		base.SavePBS();
        StringBuilder sb = new StringBuilder();
        sb.Append(Data.Metadata.SaveToString());
        foreach (PlayerMetadata playerMetadata in Data.PlayerMetadata.Values)
        {
            sb.Append(playerMetadata.SaveToString());
        }
        SaveAsPBS(sb.ToString());
    }
}
