using odl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Game;

[DebuggerDisplay("{ID}")]
public class Item
{
    public static nint Class = nint.Zero;

    public string ID;
    public string Name;
    public string Plural;
    public ItemPocket Pocket;
    public int Price;
    public int SellPrice;
    public string Description;
    public FieldUse FieldUse;
    public BattleUse BattleUse;
    public bool Consumable;
    public List<string> Flags;
    public MoveResolver? Move;

    public Item(string ID, Dictionary<string, string> hash)
    {
        this.ID = ID;
        this.Name = hash["Name"];
        this.Plural = hash["NamePlural"];
        this.Pocket = (ItemPocket) Convert.ToInt32(hash["Pocket"]);
        this.Price = Convert.ToInt32(hash["Price"]);
        if (hash.ContainsKey("SellPrice")) this.SellPrice = Convert.ToInt32(hash["SellPrice"]);
        else this.SellPrice = this.Price / 2;
        this.Description = hash["Description"];
        if (hash.ContainsKey("FieldUse")) this.FieldUse = hash["FieldUse"] switch
        {
            "None" => FieldUse.None,
            "OnPokemon" => FieldUse.OnPokemon,
            "Direct" => FieldUse.Direct,
            "TM" => FieldUse.TM,
            "HM" => FieldUse.HM,
            "TR" => FieldUse.TR,
            _ => throw new Exception($"Invalid item field use '{hash["FieldUse"]}'.")
        };
        if (hash.ContainsKey("BattleUse")) this.BattleUse = hash["BattleUse"] switch
        {
            "None" => BattleUse.None,
            "OnPokemon" => BattleUse.OnPokemon,
            "OnMove" => BattleUse.OnMove,
            "OnBattler" => BattleUse.OnBattler,
            "OnFoe" => BattleUse.OnFoe,
            "Direct" => BattleUse.Direct,
            _ => throw new Exception($"Invalid item battle use '{hash["BattleUse"]}'.")
        };
        if (hash.ContainsKey("Flags")) this.Flags = hash["Flags"].Split(',').Select(x => x.Trim()).ToList();
        else this.Flags = new List<string>();
        if (hash.ContainsKey("Consumable"))
        {
            switch (hash["Consumable"].ToLower())
            {
                case "true": case "t": case "yes": case "y":
                    this.Consumable = true;
                    break;
                default:
                    this.Consumable = false;
                    break;
            }
        }
        // Determine whether the item is consumable based on if it's a key item, TM or HM.
        else if (!Flags.Contains("KeyItem") && FieldUse != FieldUse.TM && FieldUse != FieldUse.HM || FieldUse == FieldUse.TR)
        {
            // Not an important item, so it will be consumable.
            this.Consumable = true;
        }
        if (hash.ContainsKey("Move")) this.Move = (MoveResolver) hash["Move"];
    }

    public Item(nint Data)
    {
        this.ID = Ruby.Symbol.FromPtr(Ruby.GetIVar(Data, "@id"));
        this.Name = Ruby.String.FromPtr(Ruby.GetIVar(Data, "@real_name"));
        this.Plural = Ruby.String.FromPtr(Ruby.GetIVar(Data, "@real_name_plural"));
        this.Pocket = (ItemPocket) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@pocket"));
        this.Price = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@price"));
        this.SellPrice = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@sell_price"));
        this.Description = Ruby.String.FromPtr(Ruby.GetIVar(Data, "@real_description"));
        this.FieldUse = (FieldUse) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@field_use"));
        this.BattleUse = (BattleUse) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@battle_use"));
        this.Consumable = Ruby.GetIVar(Data, "@consumable") == Ruby.True;
        nint FlagsArray = Ruby.GetIVar(Data, "@flags");
        int FlagsArrayLength = (int) Ruby.Array.Length(FlagsArray);
        this.Flags = new List<string>();
        for (int i = 0; i < FlagsArrayLength; i++)
        {
            this.Flags.Add(Ruby.String.FromPtr(Ruby.Array.Get(FlagsArray, i)));
        }
        if (Ruby.GetIVar(Data, "@move") != Ruby.Nil) this.Move = (MoveResolver) Ruby.Symbol.FromPtr(Ruby.GetIVar(Data, "@move"));
    }

    public nint Save()
    {
        nint e = Ruby.Funcall(Class, "new");
        Ruby.Pin(e);
        Ruby.SetIVar(e, "@id", Ruby.Symbol.ToPtr(this.ID));
        Ruby.SetIVar(e, "@real_name", Ruby.String.ToPtr(this.Name));
        Ruby.SetIVar(e, "@real_name_plural", Ruby.String.ToPtr(this.Plural));
        Ruby.SetIVar(e, "@pocket", Ruby.Integer.ToPtr((int) this.Pocket));
        Ruby.SetIVar(e, "@price", Ruby.Integer.ToPtr(this.Price));
        Ruby.SetIVar(e, "@sell_price", Ruby.Integer.ToPtr(this.SellPrice));
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
}

[DebuggerDisplay("{ID}")]
public class ItemResolver
{
    private string _id;
    public string ID { get => _id; set { _id = value; _item = null; } }
    private Item _item;
    public Item Item
    {
        get
        {
            if (_item != null) return _item;
            _item = Data.Items[ID];
            return _item;
        }
    }

    public ItemResolver(string ID)
    {
        this.ID = ID;
    }

    public ItemResolver(Item Item)
    {
        this.ID = Item.ID;
        _item = Item;
    }

    public static implicit operator string(ItemResolver s) => s.ID;
    public static implicit operator Item(ItemResolver s) => s.Item;
    public static explicit operator ItemResolver(Item s) => new ItemResolver(s);
    public static explicit operator ItemResolver(string ID) => new ItemResolver(ID);
}