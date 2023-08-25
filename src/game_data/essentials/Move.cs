using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;

namespace RPGStudioMK.Game;

[DebuggerDisplay("{ID}")]
public class Move : IGameData, ICloneable
{
    public static nint Class => BaseDataManager.Classes["Move"];

    public string ID;
    [Obsolete]
    public int? IDNumber;
    public string Name;
    public TypeResolver Type;
    public string Category;
    public int BaseDamage;
    public int Accuracy;
    public int TotalPP;
    public string Target;
    public int Priority;
    public string FunctionCode;
    public List<string> Flags;
    public int EffectChance;
    public string Description;

    /// <summary>
    /// DO NOT USE!
    /// </summary>
    public Move() { }

    public static Move Create()
    {
        Move m = new Move();
        m.Name = "";
        m.Type = (TypeResolver) (Type) Data.Sources.Types[0].Object;
        m.Category = Data.HardcodedData.MoveCategories[0];
        m.BaseDamage = 60;
        m.Accuracy = 100;
        m.TotalPP = 25;
        m.Target = Data.HardcodedData.MoveTargets[0];
        m.Priority = 0;
        m.FunctionCode = "";
        m.Flags = new List<string>();
        m.EffectChance = 0;
        m.Description = "";
        return m;
    }

    public Move(string ID, Dictionary<string, string> hash)
    {
        this.ID = ID;
        this.Name = hash["Name"];
        this.Type = (TypeResolver) hash["Type"];
        this.Category = Data.HardcodedData.Assert(hash["Category"], Data.HardcodedData.MoveCategories);
        if (hash.ContainsKey("BaseDamage")) this.BaseDamage = Convert.ToInt32(hash["BaseDamage"]);
        else if (hash.ContainsKey("Power")) this.BaseDamage = Convert.ToInt32(hash["Power"]);
        this.Accuracy = Convert.ToInt32(hash["Accuracy"]);
        this.TotalPP = Convert.ToInt32(hash["TotalPP"]);
        this.Target = Data.HardcodedData.Assert(hash["Target"], Data.HardcodedData.MoveTargets);
        if (hash.ContainsKey("Priority")) this.Priority = Convert.ToInt32(hash["Priority"]);
        this.FunctionCode = hash["FunctionCode"];
        if (hash.ContainsKey("Flags")) this.Flags = hash["Flags"].Split(',').Select(x => x.Trim()).ToList();
        else this.Flags = new List<string>();
        if (hash.ContainsKey("EffectChance")) this.EffectChance = Convert.ToInt32(hash["EffectChance"]);
        this.Description = hash["Description"];
    }

    public Move(List<string> line)
    {
        this.IDNumber = Convert.ToInt32(line[0]);
        this.ID = line[1];
        this.Name = line[2];
        this.FunctionCode = line[3];
        this.BaseDamage = Convert.ToInt32(line[4]);
        this.Type = (TypeResolver) line[5];
        this.Category = Data.HardcodedData.Assert(line[6], Data.HardcodedData.MoveCategories);
        this.Accuracy = Convert.ToInt32(line[7]);
        this.TotalPP = Convert.ToInt32(line[8]);
        this.EffectChance = Convert.ToInt32(line[9]);
        this.Target = Data.HardcodedData.Assert(line[10], Data.HardcodedData.MoveTargets);
        this.Priority = Convert.ToInt32(line[11]);
        this.Flags = new List<string>();
        foreach (char c in line[12])
        {
            this.Flags.Add(c.ToString());
        }
        this.Description = line[13];
    }

    public Move(nint Data)
    {
        this.ID = Ruby.Symbol.FromPtr(Ruby.GetIVar(Data, "@id"));
        if (Ruby.GetIVar(Data, "@id_number") != Ruby.Nil) this.IDNumber = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@id_number"));
        this.Name = Ruby.String.FromPtr(Ruby.GetIVar(Data, "@real_name"));
        this.Type = (TypeResolver) Ruby.Symbol.FromPtr(Ruby.GetIVar(Data, "@type"));
        this.Category = Game.Data.HardcodedData.Get((int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@category")), Game.Data.HardcodedData.MoveCategories);
        if (Game.Data.IsVersionAtLeast(EssentialsVersion.v21)) this.BaseDamage = (int)Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@power"));
        else this.BaseDamage = (int)Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@base_damage"));
        this.Accuracy = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@accuracy"));
        this.TotalPP = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@total_pp"));
        this.Target = Game.Data.HardcodedData.Assert(Ruby.Symbol.FromPtr(Ruby.GetIVar(Data, "@target")), Game.Data.HardcodedData.MoveTargets);
        this.Priority = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@priority"));
        this.FunctionCode = Ruby.String.FromPtr(Ruby.GetIVar(Data, "@function_code"));
        nint FlagsData = Ruby.GetIVar(Data, "@flags");
        this.Flags = new List<string>();
        if (Game.Data.IsVersionAtLeast(EssentialsVersion.v20))
        {
            int FlagCount = (int) Ruby.Array.Length(FlagsData);
            for (int i = 0; i < FlagCount; i++)
            {
                string Flag = Ruby.String.FromPtr(Ruby.Array.Get(FlagsData, i));
                this.Flags.Add(Flag);
            }
        }
        else
        {
            string FlagString = Ruby.String.FromPtr(FlagsData);
            foreach (char c in FlagString)
            {
                this.Flags.Add(c.ToString());
            }
        }
        this.EffectChance = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@effect_chance"));
        this.Description = Ruby.String.FromPtr(Ruby.GetIVar(Data, "@real_description"));
    }

    public nint Save()
    {
        nint e = Ruby.Funcall(Class, "new");
        Ruby.Pin(e);
        Ruby.SetIVar(e, "@id", Ruby.Symbol.ToPtr(this.ID));
        if (this.IDNumber.HasValue) Ruby.SetIVar(e, "@id_number", Ruby.Integer.ToPtr(this.IDNumber.Value));
        Ruby.SetIVar(e, "@real_name", Ruby.String.ToPtr(this.Name));
        Ruby.SetIVar(e, "@type", Ruby.Symbol.ToPtr(this.Type));
        Ruby.SetIVar(e, "@category", Ruby.Integer.ToPtr(Data.HardcodedData.AssertIndex(this.Category, Data.HardcodedData.MoveCategories)));
        if (Data.IsVersionAtLeast(EssentialsVersion.v21)) Ruby.SetIVar(e, "@power", Ruby.Integer.ToPtr(this.BaseDamage));
        else Ruby.SetIVar(e, "@base_damage", Ruby.Integer.ToPtr(this.BaseDamage));
		Ruby.SetIVar(e, "@accuracy", Ruby.Integer.ToPtr(this.Accuracy));
        Ruby.SetIVar(e, "@total_pp", Ruby.Integer.ToPtr(this.TotalPP));
        Ruby.SetIVar(e, "@target", Ruby.Symbol.ToPtr(this.Target));
        Ruby.SetIVar(e, "@priority", Ruby.Integer.ToPtr(this.Priority));
        Ruby.SetIVar(e, "@function_code", Ruby.String.ToPtr(this.FunctionCode));
        if (Data.IsVersionAtLeast(EssentialsVersion.v20))
        {
            nint FlagsArray = Ruby.Array.Create();
            Ruby.SetIVar(e, "@flags", FlagsArray);
            foreach (string Flag in Flags)
            {
                Ruby.Array.Push(FlagsArray, Ruby.String.ToPtr(Flag));
            }
        }
        else
        {
            string FlagString = this.Flags.Aggregate((a, b) => a + b);
            Ruby.SetIVar(e, "@flags", Ruby.String.ToPtr(FlagString));
        }
        Ruby.SetIVar(e, "@effect_chance", Ruby.Integer.ToPtr(this.EffectChance));
        Ruby.SetIVar(e, "@real_description", Ruby.String.ToPtr(this.Description));
        Ruby.Unpin(e);
        return e;
    }

    public object Clone()
    {
        Move m = new Move();
        m.ID = this.ID;
        m.Name = this.Name;
        m.Type = (TypeResolver) this.Type.ID;
        m.Category = this.Category;
        m.BaseDamage = this.BaseDamage;
        m.Accuracy = this.Accuracy;
        m.TotalPP = this.TotalPP;
        m.Target = this.Target;
        m.Priority = this.Priority;
        m.FunctionCode = this.FunctionCode;
        m.Flags = new List<string>(this.Flags);
        m.EffectChance = this.EffectChance;
        m.Description = this.Description;
        return m;
    }
}

[DebuggerDisplay("{ID}")]
public class MoveResolver
{
    public string ID;
    [JsonIgnore]
	public bool Valid => !string.IsNullOrEmpty(ID) && Data.Moves.ContainsKey(ID);
    [JsonIgnore]
    public Move Move => Data.Moves[ID];

    /// <summary>
    /// DO NOT USE!
    /// </summary>
    public MoveResolver() { }

    public MoveResolver(string ID)
    {
        this.ID = ID;
    }

    public MoveResolver(Move Move)
    {
        this.ID = Move.ID;
    }

    public static implicit operator string(MoveResolver s) => s.ID;
    public static implicit operator Move(MoveResolver s) => s.Move;
    public static explicit operator MoveResolver(Move s) => new MoveResolver(s);
    public static explicit operator MoveResolver(string ID) => new MoveResolver(ID);
}