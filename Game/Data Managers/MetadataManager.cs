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

public class MetadataManager : BaseDataManager
{
    // Loading the metadata from PBS will also load player metadata from PBS (as it's the same file)
    public MetadataManager(bool FromPBS = false)
        : base("Metadata", "metadata.dat", "metadata.txt", "metadata", FromPBS) { }

    protected override void LoadData()
    {
        Data.LoadedMetadataFromPBS = false;
        base.LoadData();
        SafeLoad(Filename, File =>
        {
            nint Data = Marshal.Load(File);
            Ruby.Pin(Data);
            Game.Data.Metadata = new Metadata(Ruby.Hash.Get(Data, Ruby.Integer.ToPtr(0)));
            Ruby.Unpin(Data);
        });
    }

    protected override void LoadPBS()
    {
        if (Data.PlayerMetadata.Count > 0)
        {
            // Player metadata was already loaded from data because it loaded
            // before this manager could load from PBS. Since we've clearly specified
            // in this constructor that we want to load from PBS, we discard that data.
            // This can be avoided by not putting the player metadata manager above
            // the metadata manager.
            Data.PlayerMetadata.Clear();
        }
        Data.LoadedMetadataFromPBS = true;
        base.LoadPBS();
        FormattedTextParser.ParseSectionBasedFile(PBSFilename, (id, hash) =>
        {
            int nid = Convert.ToInt32(id);
            if (nid == 0) // Global metadata
            {
                Data.Metadata = new Metadata(nid, hash);
            }
            else
            {
                Data.PlayerMetadata.Add(nid, new PlayerMetadata(nid, hash));
            }
        }, Data.SetLoadProgress);
    }

    protected override void SaveData()
    {
        base.SaveData();
        SafeSave(Filename, File =>
        {
            nint Data = Ruby.Hash.Create();
            Ruby.Pin(Data);
            Ruby.Hash.Set(Data, Ruby.Integer.ToPtr(0), Game.Data.Metadata.Save());
            Ruby.Marshal.Dump(Data, File);
            Ruby.Unpin(Data);
        });
    }
}
