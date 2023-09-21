using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace RPGStudioMK.Game;

[DebuggerDisplay("{ID}")]
public class Item : IGameData, ICloneable
{
    public static nint Class => BaseDataManager.Classes["Item"];

    public string ID;
    [Obsolete]
    public int? IDNumber;
    public string Name;
    public string Plural;
    public string PortionName;
    public string PortionNamePlural;
    public int Pocket;
    public int Price;
    public int BPPrice;
    public int SellPrice;
    public string Description;
    public int FieldUse;
    public int BattleUse;
    [Obsolete]
    public int? UseType;
    public bool Consumable;
    public List<string> Flags;
    public MoveResolver? Move;

    /// <summary>
    /// DO NOT USE!
    /// </summary>
    public Item() { }

    public static Item Create()
    {
        Item i = new Item();
        i.Name = "";
        i.Plural = "";
        i.Pocket = 0;
        i.PortionName = "";
        i.PortionNamePlural = "";
        i.Price = 0;
        i.BPPrice = 1;
        i.SellPrice = 0;
        i.Description = "";
        i.FieldUse = 0;
        i.BattleUse = 0;
        i.UseType = 0;
        i.Consumable = false;
        i.Flags = new List<string>();
        i.Move = null;
        return i;
    }

    public static Item CreateTM()
    {
		Item i = new Item();
		i.Name = "";
		i.Plural = "";
        i.PortionName = "";
        i.PortionNamePlural = "";
		i.Pocket = 0;
		i.Price = 0;
        i.BPPrice = 1;
		i.SellPrice = 0;
		i.Description = "";
        i.FieldUse = Data.HardcodedData.ItemFieldUses.IndexOf("TR");
		i.BattleUse = 0;
		i.Consumable = false;
		i.Flags = new List<string>();
		return i;
	}

    public Item(string ID, Dictionary<string, string> hash)
    {
        this.ID = ID;
        this.Name = hash["Name"];
        this.Plural = hash["NamePlural"];
        if (hash.ContainsKey("PortionName")) this.PortionName = hash["PortionName"];
        else this.PortionName = "";
        if (hash.ContainsKey("PortionNamePlural")) this.PortionNamePlural = hash["PortionNamePlural"];
        else this.PortionNamePlural = "";
        this.Pocket = Convert.ToInt32(hash["Pocket"]);
        this.Price = Convert.ToInt32(hash["Price"]);
        if (hash.ContainsKey("BPPrice")) this.BPPrice = Convert.ToInt32(hash["BPPrice"]);
        if (hash.ContainsKey("SellPrice")) this.SellPrice = Convert.ToInt32(hash["SellPrice"]);
        else this.SellPrice = this.Price / 2;
        this.Description = hash["Description"];
        if (hash.ContainsKey("FieldUse")) this.FieldUse = Data.HardcodedData.AssertIndex(hash["FieldUse"], Data.HardcodedData.ItemFieldUses);
        if (hash.ContainsKey("BattleUse")) this.BattleUse = Data.HardcodedData.AssertIndex(hash["BattleUse"], Data.HardcodedData.ItemBattleUses);
        if (hash.ContainsKey("Flags")) this.Flags = hash["Flags"].Split(',').Select(x => x.Trim()).ToList();
        else this.Flags = new List<string>();
        if (hash.ContainsKey("Consumable"))
        {
            this.Consumable = hash["Consumable"].ToLower() switch
            {
                "true" or "t" or "yes" or "y" => true,
                _ => false
            };
        }
        // Determine whether the item is consumable based on if it's a key item, TM or HM.
        // TODO: Remove hardcoding
        else if (!Flags.Contains("KeyItem") && Data.HardcodedData.ItemFieldUses[FieldUse] != "TM" && Data.HardcodedData.ItemFieldUses[FieldUse] != "HM" || Data.HardcodedData.ItemFieldUses[FieldUse] == "TR")
        {
            // Not an important item, so it will be consumable.
            this.Consumable = true;
        }
        if (hash.ContainsKey("Move")) this.Move = (MoveResolver) hash["Move"];
    }

    public Item(List<string> line)
    {
        this.IDNumber = Convert.ToInt32(line[0]);
        this.ID = line[1];
        this.Name = line[2];
        this.Plural = line[3];
        this.PortionName = "";
        this.PortionNamePlural = "";
        this.Pocket = Convert.ToInt32(line[4]);
        this.Price = Convert.ToInt32(line[5]);
        this.BPPrice = 1;
        this.SellPrice = this.Price / 2;
        this.Description = line[6];
        this.FieldUse = Convert.ToInt32(line[7]);
        this.BattleUse = Convert.ToInt32(line[8]);
        this.UseType = Convert.ToInt32(line[9]);
        if (!string.IsNullOrEmpty(line[10])) this.Move = (MoveResolver) line[10];
        this.Flags = new List<string>();
    }

    public Item(nint Data)
    {
        this.ID = Ruby.Symbol.FromPtr(Ruby.GetIVar(Data, "@id"));
        if (Ruby.GetIVar(Data, "@id_number") != Ruby.Nil) this.IDNumber = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@id_number"));
        this.Name = Ruby.String.FromPtr(Ruby.GetIVar(Data, "@real_name"));
        this.Plural = Ruby.String.FromPtr(Ruby.GetIVar(Data, "@real_name_plural"));
        if (Ruby.GetIVar(Data, "@real_portion_name") != Ruby.Nil) this.PortionName = Ruby.String.FromPtr(Ruby.GetIVar(Data, "@real_portion_name"));
        else this.PortionName = "";
        if (Ruby.GetIVar(Data, "@real_portion_name_plural") != Ruby.Nil) this.PortionNamePlural = Ruby.String.FromPtr(Ruby.GetIVar(Data, "@real_portion_name_plural"));
        else this.PortionNamePlural = "";
        this.Pocket = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@pocket"));
        this.Price = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@price"));
        if (Ruby.GetIVar(Data, "@bp_price") != Ruby.Nil) this.BPPrice = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@bp_price"));
        if (Ruby.GetIVar(Data, "@sell_price") != Ruby.Nil) this.SellPrice = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@sell_price"));
        else this.SellPrice = this.Price / 2;
        this.Description = Ruby.String.FromPtr(Ruby.GetIVar(Data, "@real_description"));
        this.FieldUse = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@field_use"));
        this.BattleUse = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@battle_use"));
		this.Consumable = Ruby.GetIVar(Data, "@consumable") == Ruby.True;
        nint FlagsArray = Ruby.GetIVar(Data, "@flags");
        this.Flags = new List<string>();
        if (FlagsArray != Ruby.Nil)
        {
            int FlagsArrayLength = (int) Ruby.Array.Length(FlagsArray);
            for (int i = 0; i < FlagsArrayLength; i++)
            {
                this.Flags.Add(Ruby.String.FromPtr(Ruby.Array.Get(FlagsArray, i)));
            }
        }
        if (Ruby.GetIVar(Data, "@move") != Ruby.Nil) this.Move = (MoveResolver) Ruby.Symbol.FromPtr(Ruby.GetIVar(Data, "@move"));
    }

    public nint Save()
    {
        nint e = Ruby.Funcall(Class, "new");
        Ruby.Pin(e);
        Ruby.SetIVar(e, "@id", Ruby.Symbol.ToPtr(this.ID));
        if (this.IDNumber.HasValue) Ruby.SetIVar(e, "@id_number", Ruby.Integer.ToPtr(this.IDNumber.Value));
        Ruby.SetIVar(e, "@real_name", Ruby.String.ToPtr(this.Name));
        Ruby.SetIVar(e, "@real_name_plural", Ruby.String.ToPtr(this.Plural));
        if (Game.Data.IsVersionAtLeast(EssentialsVersion.v21) && !string.IsNullOrEmpty(this.PortionName))
            Ruby.SetIVar(e, "@real_portion_name", Ruby.String.ToPtr(this.PortionName));
		if (Game.Data.IsVersionAtLeast(EssentialsVersion.v21) && !string.IsNullOrEmpty(this.PortionNamePlural))
			Ruby.SetIVar(e, "@real_portion_name_plural", Ruby.String.ToPtr(this.PortionNamePlural));
		Ruby.SetIVar(e, "@pocket", Ruby.Integer.ToPtr((int) this.Pocket));
        Ruby.SetIVar(e, "@price", Ruby.Integer.ToPtr(this.Price));
        if (Game.Data.IsVersionAtLeast(EssentialsVersion.v21)) Ruby.SetIVar(e, "@bp_price", Ruby.Integer.ToPtr(this.BPPrice));
        if (Game.Data.IsVersionAtLeast(EssentialsVersion.v20)) Ruby.SetIVar(e, "@sell_price", Ruby.Integer.ToPtr(this.SellPrice));
        Ruby.SetIVar(e, "@real_description", Ruby.String.ToPtr(this.Description));
        Ruby.SetIVar(e, "@field_use", Ruby.Integer.ToPtr((int) this.FieldUse));
        Ruby.SetIVar(e, "@battle_use", Ruby.Integer.ToPtr((int) this.BattleUse));
        Ruby.SetIVar(e, "@consumable", this.Consumable ? Ruby.True : Ruby.False);
        nint FlagsArray = Ruby.Array.Create();
        Ruby.SetIVar(e, "@flags", FlagsArray);
        foreach (string Flag in Flags)
        {
            Ruby.Array.Push(FlagsArray, Ruby.String.ToPtr(Flag));
        }
        Ruby.SetIVar(e, "@move", this.Move == null ? Ruby.Nil : Ruby.Symbol.ToPtr(this.Move));
        Ruby.Unpin(e);
        return e;
	}

	public string SaveToString()
	{
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"#-------------------------------");
        sb.AppendLine($"[{this.ID}]");
        sb.AppendLine($"Name = {this.Name}");
        sb.AppendLine($"NamePlural = {this.Plural}");
        if (Game.Data.IsVersionAtLeast(EssentialsVersion.v21) && !string.IsNullOrEmpty(this.PortionName)) sb.AppendLine($"PortionName = {this.PortionName}");
		if (Game.Data.IsVersionAtLeast(EssentialsVersion.v21) && !string.IsNullOrEmpty(this.PortionNamePlural)) sb.AppendLine($"PortionNamePlural = {this.PortionNamePlural}");
		sb.AppendLine($"Pocket = {this.Pocket}");
        sb.AppendLine($"Price = {this.Price}");
        if (Game.Data.IsVersionAtLeast(EssentialsVersion.v21) && this.BPPrice != 1) sb.AppendLine($"BPPrice = {this.BPPrice}");
        if (Game.Data.IsVersionAtLeast(EssentialsVersion.v20) && this.SellPrice != this.Price / 2) sb.AppendLine($"SellPrice = {this.SellPrice}");
        if (this.FieldUse != 0) sb.AppendLine($"FieldUse = {Data.HardcodedData.ItemFieldUses[this.FieldUse]}");
        if (this.BattleUse != 0) sb.AppendLine($"BattleUse = {Data.HardcodedData.ItemBattleUses[this.BattleUse]}");
        if (this.Flags.Count > 0) sb.AppendLine($"Flags = {this.Flags.Aggregate((a, b) => a + "," + b)}");
        if (!this.Consumable && (this.Pocket == 1 || this.Pocket == 7)) sb.AppendLine($"Consumable = false");
        if (this.Move is not null) sb.AppendLine($"Move = {this.Move.ID}");
        sb.AppendLine($"Description = {this.Description}");
		return sb.ToString();
	}

	public object Clone()
    {
        Item i = new Item();
        i.ID = this.ID;
        i.Name = this.Name;
        i.Plural = this.Plural;
        i.Pocket = this.Pocket;
        i.Price = this.Price;
        i.BPPrice = this.BPPrice;
        i.SellPrice = this.SellPrice;
        i.Description = this.Description;
        i.FieldUse = this.FieldUse;
        i.BattleUse = this.BattleUse;
        i.Consumable = this.Consumable;
        i.Flags = new List<string>(this.Flags);
        i.Move = this.Move;
        return i;
    }
}

[DebuggerDisplay("{ID}")]
public class ItemResolver : DataResolver
{
    [JsonIgnore]
	public override bool Valid => !string.IsNullOrEmpty(ID) && Data.Items.ContainsKey(ID);
    [JsonIgnore]
    public Item Item => Data.Items[ID];

    /// <summary>
    /// DO NOT USE!
    /// </summary>
    public ItemResolver()
    {
        
    }

    public ItemResolver(string ID)
    {
        this.ID = ID;
    }

    public ItemResolver(Item Item)
    {
        this.ID = Item.ID;
    }

    public static implicit operator string(ItemResolver s) => s.ID;
    public static implicit operator Item(ItemResolver s) => s.Item;
    public static explicit operator ItemResolver(Item s) => new ItemResolver(s);
    public static explicit operator ItemResolver(string ID) => new ItemResolver(ID);
}