using RPGStudioMK.Utility;

namespace RPGStudioMK.Game;

public class TrainerManager : BaseDataManager
{
    public TrainerManager() : base("Trainer", "trainers.dat", "trainers.txt", "trainers") { }

    protected override void LoadData()
    {
        base.LoadData();
        Logger.WriteLine("Loading trainers");
        LoadAsHash((key, value) => Data.Trainers.Add(new Trainer(value)));
    }

    protected override void LoadPBS()
    {
        base.LoadPBS();
        Logger.WriteLine("Loading trainers from PBS");
        FormattedTextParser.ParseSectionBasedFileWithOrder(PBSFilename, (id, pairs) =>
        {
            Data.Trainers.Add(new Trainer(id, pairs));
        }, Data.SetLoadProgress);
    }

    protected override void SaveData()
    {
        base.SaveData();
        Logger.WriteLine("Saving trainers");
        SaveDataAsHash(Data.Trainers, t =>
        {
            nint Array = Ruby.Array.Create();
            Ruby.Pin(Array);
            Ruby.Array.Push(Array, Ruby.Symbol.ToPtr(t.TrainerType));
            Ruby.Array.Push(Array, Ruby.String.ToPtr(t.Name));
            Ruby.Array.Push(Array, Ruby.Integer.ToPtr(t.Version));
            Ruby.Unpin(Array);
            return Array;
        });
    }

    public override void Clear()
    {
        base.Clear();
        Logger.WriteLine("Clearing trainers");
        Data.Trainers.Clear();
    }
}
