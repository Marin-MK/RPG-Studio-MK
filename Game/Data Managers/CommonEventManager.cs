using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Game;

public class CommonEventManager : BaseDataManager
{
    public CommonEventManager() 
        : base(null, "CommonEvents.rxdata", null, "common events", false) { }

    protected override void LoadData()
    {
        base.LoadData();
        LoadAsArray(e => Data.CommonEvents.Add(new CommonEvent(e)), true);
    }

    protected override void LoadPBS()
    {
        throw new MethodNotSupportedException(this);
    }

    protected override void SaveData()
    {
        base.SaveData();
        SaveAsArray(Data.CommonEvents, true);
    }

    public override void Clear()
    {
        base.Clear();
        Data.CommonEvents.Clear();
    }
}
