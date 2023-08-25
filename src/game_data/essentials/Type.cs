using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RPGStudioMK.Game;

[DebuggerDisplay("{ID}")]
public class Type : IGameData, ICloneable
{
    public static nint Class => BaseDataManager.Classes["Type"];

    public string ID;
    [Obsolete]
    public int? IDNumber;
    public string Name;
    public bool SpecialType;
    public bool PseudoType;
    public List<TypeResolver> Weaknesses;
    public List<TypeResolver> Resistances;
    public List<TypeResolver> Immunities;
    public int? IconPosition;
    public List<string> Flags;

    /// <summary>
    /// DO NOT USE!
    /// </summary>
    public Type() { }

    public Type(string ID, Dictionary<string, string> hash)
    {
        this.ID = ID;
        this.Name = hash["Name"];
        if (hash.ContainsKey("IsSpecialType")) this.SpecialType = hash["IsSpecialType"].ToLower() == "true";
        if (hash.ContainsKey("IsPseudoType")) this.PseudoType = hash["IsPseudoType"].ToLower() == "true";
        if (hash.ContainsKey("Weaknesses")) this.Weaknesses = hash["Weaknesses"].Split(',').Select(x => (TypeResolver) x.Trim()).ToList();
        else this.Weaknesses = new List<TypeResolver>();
        if (hash.ContainsKey("Resistances")) this.Resistances = hash["Resistances"].Split(',').Select(x => (TypeResolver) x.Trim()).ToList();
        else this.Resistances = new List<TypeResolver>();
        if (hash.ContainsKey("Immunities")) this.Immunities = hash["Immunities"].Split(',').Select(x => (TypeResolver) x.Trim()).ToList();
        else this.Immunities = new List<TypeResolver>();
        if (hash.ContainsKey("IconPosition")) this.IconPosition = Convert.ToInt32(hash["IconPosition"]);
        if (hash.ContainsKey("Flags")) this.Flags = hash["Flags"].Split(',').Select(x => x.Trim()).ToList();
        else this.Flags = new List<string>();
    }

    public Type(nint Data)
    {
        this.ID = Ruby.Symbol.FromPtr(Ruby.GetIVar(Data, "@id"));
        if (Ruby.GetIVar(Data, "@id_number") != Ruby.Nil) this.IDNumber = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@id_number"));
        this.Name = Ruby.String.FromPtr(Ruby.GetIVar(Data, "@real_name"));
        this.SpecialType = Ruby.GetIVar(Data, "@special_type") == Ruby.True;
        this.PseudoType = Ruby.GetIVar(Data, "@pseudo_type") == Ruby.True;
        nint WeaknessArray = Ruby.GetIVar(Data, "@weaknesses");
        int WeaknessArrayLength = (int) Ruby.Array.Length(WeaknessArray);
        this.Weaknesses = new List<TypeResolver>();
        for (int i = 0; i < WeaknessArrayLength; i++)
        {
            this.Weaknesses.Add((TypeResolver) Ruby.Symbol.FromPtr(Ruby.Array.Get(WeaknessArray, i)));
        }
        nint ResistancesArray = Ruby.GetIVar(Data, "@resistances");
        int ResistancesArrayLength = (int) Ruby.Array.Length(ResistancesArray);
        this.Resistances = new List<TypeResolver>();
        for (int i = 0; i < ResistancesArrayLength; i++)
        {
            this.Resistances.Add((TypeResolver) Ruby.Symbol.FromPtr(Ruby.Array.Get(ResistancesArray, i)));
        }
        nint ImmunitiesArray = Ruby.GetIVar(Data, "@immunities");
        int ImmunitiesArrayLength = (int) Ruby.Array.Length(ImmunitiesArray);
        this.Immunities = new List<TypeResolver>();
        for (int i = 0; i < ImmunitiesArrayLength; i++)
        {
            this.Immunities.Add((TypeResolver) Ruby.Symbol.FromPtr(Ruby.Array.Get(ImmunitiesArray, i)));
        }
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
        if (Ruby.GetIVar(Data, "@icon_position") != Ruby.Nil) this.IconPosition = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@icon_position"));
    }

    public nint Save()
    {
        nint e = Ruby.Funcall(Class, "new");
        Ruby.Pin(e);
        Ruby.SetIVar(e, "@id", Ruby.Symbol.ToPtr(this.ID));
        if (this.IDNumber.HasValue) Ruby.SetIVar(e, "@id_number", Ruby.Integer.ToPtr(this.IDNumber.Value));
        Ruby.SetIVar(e, "@real_name", Ruby.String.ToPtr(this.Name));
        Ruby.SetIVar(e, "@special_type", this.SpecialType ? Ruby.True : Ruby.False);
        Ruby.SetIVar(e, "@pseudo_type", this.PseudoType ? Ruby.True : Ruby.False);
        nint WeaknessArray = Ruby.Array.Create();
        Ruby.SetIVar(e, "@weaknesses", WeaknessArray);
        foreach (string Type in Weaknesses)
        {
            Ruby.Array.Push(WeaknessArray, Ruby.Symbol.ToPtr(Type));
        }
        nint ResistanceArray = Ruby.Array.Create();
        Ruby.SetIVar(e, "@resistances", ResistanceArray);
        foreach (string Type in Resistances)
        {
            Ruby.Array.Push(ResistanceArray, Ruby.Symbol.ToPtr(Type));
        }
        nint ImmunitiesArray = Ruby.Array.Create();
        Ruby.SetIVar(e, "@immunities", ImmunitiesArray);
        foreach (string Type in Immunities)
        {
            Ruby.Array.Push(ImmunitiesArray, Ruby.Symbol.ToPtr(Type));
        }
        nint FlagsArray = Ruby.Array.Create();
        Ruby.SetIVar(e, "@flags", FlagsArray);
        foreach (string Flag in Flags)
        {
            Ruby.Array.Push(FlagsArray, Ruby.String.ToPtr(Flag));
        }
        if (this.IconPosition.HasValue) Ruby.SetIVar(e, "@icon_position", Ruby.Integer.ToPtr(this.IconPosition.Value));
        Ruby.Unpin(e);
        return e;
    }

    public object Clone()
    {
        Type t = new Type();
        t.ID = this.ID;
        t.Name = this.Name;
        t.SpecialType = this.SpecialType;
        t.PseudoType = this.PseudoType;
        t.Weaknesses = this.Weaknesses.Select(x => (TypeResolver) x.ID).ToList();
        t.Resistances = this.Resistances.Select(x => (TypeResolver) x.ID).ToList();
        t.Immunities = this.Immunities.Select(x => (TypeResolver) x.ID).ToList();
        t.IconPosition = this.IconPosition;
        t.Flags = new List<string>(this.Flags);
        return t;
    }
}

[DebuggerDisplay("{ID}")]
public class TypeResolver
{
    public string ID;
    [JsonIgnore]
    public bool Valid => !string.IsNullOrEmpty(ID) && Data.Types.ContainsKey(ID);
    [JsonIgnore]
    public Type Type => Data.Types[ID];

    /// <summary>
    /// DO NOT USE!
    /// </summary>
    public TypeResolver()
    {
        
    }

    public TypeResolver(string ID)
    {
        this.ID = ID;
    }

    public TypeResolver(Type Type)
    {
        this.ID = Type.ID;
    }

    public static implicit operator string(TypeResolver s) => s.ID;
    public static implicit operator Type(TypeResolver s) => s.Type;
    public static explicit operator TypeResolver(Type s) => new TypeResolver(s);
    public static explicit operator TypeResolver(string ID) => new TypeResolver(ID);
}