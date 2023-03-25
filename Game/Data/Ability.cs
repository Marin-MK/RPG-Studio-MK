using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Game;

[DebuggerDisplay("{ID}")]
public class Ability : IGameData, ICloneable
{
    public static nint Class => BaseDataManager.Classes["Ability"];

    public string ID;
    [Obsolete]
    public int? IDNumber;
    public string Name;
    public string Description;
    public List<string> Flags;

    private Ability()
    {

    }

    public Ability(string ID, Dictionary<string, string> hash)
    {
        this.ID = ID;
        this.Name = hash["Name"];
        this.Description = hash["Description"];
        if (hash.ContainsKey("Flags")) this.Flags = hash["Flags"].Split(',').Select(x => x.Trim()).ToList();
        else this.Flags = new List<string>();
    }

    public Ability(List<string> line)
    {
        this.IDNumber = Convert.ToInt32(line[0]);
        this.ID = line[1];
        this.Name = line[2];
        this.Description = line[3];
        this.Flags = new List<string>();
    }

    public Ability(nint Data)
    {
        this.ID = Ruby.Symbol.FromPtr(Ruby.GetIVar(Data, "@id"));
        if (Ruby.GetIVar(Data, "@id_number") != Ruby.Nil) this.IDNumber = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@id_number"));
        this.Name = Ruby.String.FromPtr(Ruby.GetIVar(Data, "@real_name"));
        this.Description = Ruby.String.FromPtr(Ruby.GetIVar(Data, "@real_description"));
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
    }

    public nint Save()
    {
        nint e = Ruby.Funcall(Class, "new");
        Ruby.Pin(e);
        Ruby.SetIVar(e, "@id", Ruby.Symbol.ToPtr(this.ID));
        if (this.IDNumber.HasValue) Ruby.SetIVar(e, "@id_number", Ruby.Integer.ToPtr(this.IDNumber.Value));
        Ruby.SetIVar(e, "@real_name", Ruby.String.ToPtr(this.Name));
        Ruby.SetIVar(e, "@real_description", Ruby.String.ToPtr(this.Description));
        nint FlagsArray = Ruby.Array.Create();
        Ruby.SetIVar(e, "@flags", FlagsArray);
        foreach (string Flag in Flags)
        {
            Ruby.Array.Push(FlagsArray, Ruby.String.ToPtr(Flag));
        }
        Ruby.Unpin(e);
        return e;
    }

    public object Clone()
    {
        Ability a = new Ability();
        a.ID = this.ID;
        a.Name = this.Name;
        a.Description = this.Description;
        a.Flags = new List<string>(this.Flags);
        return a;
    }
}

[DebuggerDisplay("{ID}")]
public class AbilityResolver
{
    private string _id;
    public string ID { get => _id; set { _id = value; _ability = null; } }
    private Ability _ability;
    public Ability Ability
    {
        get
        {
            if (_ability != null) return _ability;
            _ability = Data.Abilities[ID];
            return _ability;
        }
    }

    public AbilityResolver(string ID)
    {
        this.ID = ID;
    }

    public AbilityResolver(Ability Ability)
    {
        this.ID = Ability.ID;
        _ability = Ability;
    }

    public static implicit operator string(AbilityResolver s) => s.ID;
    public static implicit operator Ability(AbilityResolver s) => s.Ability;
    public static explicit operator AbilityResolver(Ability s) => new AbilityResolver(s);
    public static explicit operator AbilityResolver(string ID) => new AbilityResolver(ID);
}