using RPGStudioMK.Utility;
using System.Linq;

namespace RPGStudioMK.Game;

public class ItemManager : BaseDataManager
{
    public ItemManager() : base("Item", "items.dat", "items.txt", "items") { }

    protected override void LoadData()
    {
        base.LoadData();
        Logger.WriteLine("Loading items");
        LoadAsHash((key, value) =>
        {
            string item = Ruby.Symbol.FromPtr(key);
            Data.Items.Add(item, new Item(value));
        });
    }

    protected override void LoadPBS()
    {
        base.LoadPBS();
        if (Game.Data.IsVersionAtLeast(EssentialsVersion.v20))
        {
            Logger.WriteLine("Loading items from PBS with EV >= v20");
            FormattedTextParser.ParseSectionBasedFile(PBSFilename, (id, hash) =>
            {
                Data.Items.Add(id, new Item(id, hash));
            }, Data.SetLoadProgress);
        }
        else
        {
            Logger.WriteLine("Loading items from PBS with EV < v20");
            FormattedTextParser.ParseLineByLineCommaBased(PBSFilename, line =>
            {
                Item itemdata = new Item(line);
                Data.Items.Add(itemdata.ID, itemdata);
            }, Data.SetLoadProgress);
        }
    }

	public override void Load(bool fromPBS)
	{
		base.Load(fromPBS);
        SplitTMsFromItems();
	}

    private void SplitTMsFromItems()
    {
        Data.TMsHMs.Clear();
		Data.Items.Values.ToList()
			.FindAll(item => item.Move is not null &&
					(Data.HardcodedData.Get(item.FieldUse, Data.HardcodedData.ItemFieldUses) == "TR" ||
					Data.HardcodedData.Get(item.FieldUse, Data.HardcodedData.ItemFieldUses) == "TM" ||
					Data.HardcodedData.Get(item.FieldUse, Data.HardcodedData.ItemFieldUses) == "HM"))
			.ForEach(item => {
				Data.Items.Remove(item.ID);
				Data.TMsHMs.Add(item.ID, item);
			});
	}

    private void InsertTMsIntoItems()
    {
        Data.TMsHMs.Values.ToList()
            .ForEach(item =>
            {
                Data.TMsHMs.Remove(item.ID);
                Data.Items.Add(item.ID, item);
            });
        Data.TMsHMs.Clear();
    }

	public override void Save()
	{
        InsertTMsIntoItems();
		base.Save();
        SplitTMsFromItems();
	}

	protected override void SaveData()
    {
        base.SaveData();
        Logger.WriteLine("Saving items");
        SaveAsHash(Data.Items.Values, e => Ruby.Symbol.ToPtr(e.ID));
    }

    public override void Clear()
    {
        base.Clear();
        Logger.WriteLine("Clearing items");
        Data.Items.Clear();
    }
}
