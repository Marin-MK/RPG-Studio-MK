using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Game;

public class SystemManager : BaseDataManager
{
    public SystemManager() : base(null, "System.rxdata", null, "system") { }

    public override void Load(bool fromPBS = false)
    {
        base.Load(fromPBS);
        Logger.Write("Loading system data");
        SafeLoad("System.rxdata", File =>
        {
            IntPtr data = Ruby.Marshal.Load(File);
            Ruby.Pin(data);
            Data.System = new System(data);
            Ruby.Unpin(data);
        });
    }

    public override void Save()
    {
        base.Save();
        Logger.Write("Saving system data");
        Data.System.EditMapID = Editor.ProjectSettings.LastMapID;
        SafeSave("System.rxdata", File =>
        {
            IntPtr data = Data.System.Save();
            Ruby.Pin(data);
            Ruby.Marshal.Dump(data, File);
            Ruby.Unpin(data);
        });
    }

    public override void Clear()
    {
        base.Clear();
        Logger.Write("Clearing system data");
        Data.System = null;
    }
}
