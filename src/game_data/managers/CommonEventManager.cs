namespace RPGStudioMK.Game;

public class CommonEventManager : BaseDataManager
{
    public CommonEventManager() 
        : base(null, "CommonEvents.rxdata", null, "common events") { }

    public override void Load(bool fromPBS = false)
    {
        base.Load(fromPBS);
        Logger.WriteLine("Loading common events");
        LoadAsArray(e => Data.CommonEvents.Add(new CommonEvent(e)), true);
    }

    public override void Save()
    {
        base.Save();
        Logger.WriteLine("Saving common events");
        SaveDataAsArray(Data.CommonEvents, true);
    }

    public override void Clear()
    {
        base.Clear();
        Logger.WriteLine("Clearing common events");
        Data.CommonEvents.Clear();
    }
}
