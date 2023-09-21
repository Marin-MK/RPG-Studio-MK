using RPGStudioMK.Utility;

namespace RPGStudioMK.Game;

public class TypeManager : BaseDataManager
{
    public TypeManager() : base("Type", "types.dat", "types.txt", "types") { }

    protected override void LoadData()
    {
        base.LoadData();
        Logger.WriteLine("Loading types");
        LoadAsHash((key, value) =>
        {
            string ckey = Ruby.Symbol.FromPtr(key);
            Data.Types.Add(ckey, new Type(value));
        });
    }

    protected override void LoadPBS()
    {
        base.LoadPBS();
        Logger.WriteLine("Loading types from PBS");
        FormattedTextParser.ParseSectionBasedFile(PBSFilename, (id, hash) =>
        {
            Data.Types.Add(id, new Type(id, hash));
        }, Data.SetLoadProgress);
    }

    protected override void SaveData()
    {
        base.SaveData();
        Logger.WriteLine("Saving types");
        SaveDataAsHash(Data.Types.Values, t => Ruby.Symbol.ToPtr(t.ID));
    }

    public override void Clear()
    {
        base.Clear();
        Logger.WriteLine("Clearing types");
        Data.Types.Clear();
    }
}
