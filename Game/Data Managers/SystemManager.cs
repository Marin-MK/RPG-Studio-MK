using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Game;

public class SystemManager : BaseDataManager
{
    public SystemManager()
        : base(null, "System.rxdata", null, "system", false) { }

    protected override void LoadData()
    {
        base.LoadData();
        SafeLoad("System.rxdata", File =>
        {
            IntPtr data = Ruby.Marshal.Load(File);
            Ruby.Pin(data);
            Data.System = new System(data);
            Ruby.Unpin(data);
        });
    }

    protected override void SaveData()
    {
        base.SaveData();
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
        Data.System = null;
    }
}
