using RPGStudioMK.Utility;

namespace RPGStudioMK.Game;

public class MoveManager : BaseDataManager
{
    public MoveManager()
        : base("Move", "moves.dat", "moves.txt", "moves") { }

    protected override void LoadData()
    {
        base.LoadData();
        Logger.WriteLine("Loading moves");
        LoadAsHash((key, value) =>
        {
			if (!Ruby.Is(key, "Symbol", "String")) return;
			string ckey = Ruby.Symbol.FromPtr(key);
            Data.Moves.Add(ckey, new Move(value));
        });
    }

    protected override void LoadPBS()
    {
        base.LoadPBS();
        if (Game.Data.IsVersionAtLeast(EssentialsVersion.v20))
        {
            Logger.WriteLine("Loading moves from PBS with EV >= v20");
            FormattedTextParser.ParseSectionBasedFile(PBSFilename, (id, hash) =>
            {
                Data.Moves.Add(id, new Move(id, hash));
            }, Data.SetLoadProgress);
        }
        else
        {
            Logger.WriteLine("Loading moves from PBS with EV < v20");
            FormattedTextParser.ParseLineByLineCommaBased(PBSFilename, line =>
            {
                Move movedata = new Move(line);
                Data.Moves.Add(movedata.ID, movedata);
            }, Data.SetLoadProgress);
        }
    }

    protected override void SaveData()
    {
        base.SaveData();
        Logger.WriteLine("Saving moves");
        SaveDataAsHash(Data.Moves.Values, m => Ruby.Symbol.ToPtr(m.ID));
    }

    public override void Clear()
    {
        base.Clear();
        Logger.WriteLine("Clearing moves");
        Data.Moves.Clear();
    }
}