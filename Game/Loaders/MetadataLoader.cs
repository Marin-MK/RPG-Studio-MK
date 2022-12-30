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

public static partial class Data
{
    private static void LoadMetadata()
    {
        LoadMetadataFromPBS(ProjectPath + "/PBS/metadata.txt");
        return;
        SafeLoad("metadata.dat", File =>
        {
            nint Data = Marshal.Load(File);
            Ruby.Pin(Data);
            Metadata = new Metadata(Ruby.Hash.Get(Data, Ruby.Integer.ToPtr(0)));
            Ruby.Unpin(Data);
        });
    }

    private static void LoadMetadataFromPBS(string FilePath)
    {
        FormattedTextParser.ParseSectionBasedFile(FilePath, (id, hash) =>
        {
            int nid = Convert.ToInt32(id);
            if (nid == 0) // Global metadata
            {
                Metadata = new Metadata(nid, hash);
            }
            else
            {
                PlayerMetadata.Add(nid, new PlayerMetadata(nid, hash));
            }
        });
    }

    private static void SaveMetadata()
    {
        SafeSave("metadata.dat", File =>
        {
            nint Data = Ruby.Hash.Create();
            Ruby.Pin(Data);
            Ruby.Hash.Set(Data, Ruby.Integer.ToPtr(0), Metadata.Save());
            Ruby.Marshal.Dump(Data, File);
            Ruby.Unpin(Data);
        });
    }
}
