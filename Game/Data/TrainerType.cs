using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RPGStudioMK.Game;

[DebuggerDisplay("{ID}")]
public class TrainerType : IGameData, ICloneable
{
    public static nint Class => BaseDataManager.Classes["TrainerType"];

    public string ID;
    [Obsolete]
    public int? IDNumber;
    public string Name;
    public int Gender;
    public int BaseMoney;
    public int SkillLevel;
    public List<string> Flags;
    public string? IntroBGM;
    public string? BattleBGM;
    public string? VictoryBGM;
    [Obsolete]
    public string? SkillCode;

    private TrainerType() { }

    public TrainerType(string ID, Dictionary<string, string> hash)
    {
        this.ID = ID;
        this.Name = hash["Name"];
        this.Gender = hash["Gender"].ToLower() switch
        {
            "male" or "m" or "0" => 0,
            "female" or "f" or "1" => 1,
            _ => 2,
        };
        this.BaseMoney = Convert.ToInt32(hash["BaseMoney"]);
        if (hash.ContainsKey("SkillLevel")) this.SkillLevel = Convert.ToInt32(hash["SkillLevel"]);
        else this.SkillLevel = this.BaseMoney;
        if (hash.ContainsKey("Flags")) this.Flags = hash["Flags"].Split(',').Select(x => x.Trim()).ToList();
        else this.Flags = new List<string>();
        if (hash.ContainsKey("IntroBGM")) this.IntroBGM = hash["IntroBGM"];
        if (hash.ContainsKey("BattleBGM")) this.BattleBGM = hash["BattleBGM"];
        if (hash.ContainsKey("VictoryBGM")) this.VictoryBGM = hash["VictoryBGM"];
    }

    public TrainerType(List<string> line)
    {
        this.IDNumber = Convert.ToInt32(line[0]);
        this.ID = line[1];
        this.Name = line[2];
        this.BaseMoney = Convert.ToInt32(line[3]);
        this.BattleBGM = line[4];
        this.VictoryBGM = line[5];
        this.IntroBGM = line[6];
        this.Gender = line[7].ToLower() switch
        {
            "male" or "m" or "0" => 0,
            "female" or "f" or "1" => 1,
            _ => 2
        };
        this.SkillLevel = string.IsNullOrEmpty(line[8]) ? BaseMoney : Convert.ToInt32(line[8]);
        this.SkillCode = line[9];
    }

    public TrainerType(nint Data)
    {
        this.ID = Ruby.Symbol.FromPtr(Ruby.GetIVar(Data, "@id"));
        if (Ruby.GetIVar(Data, "@id_number") != Ruby.Nil) this.IDNumber = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@id_number"));
        this.Name = Ruby.String.FromPtr(Ruby.GetIVar(Data, "@real_name"));
        this.Gender = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@gender"));
        this.BaseMoney = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@base_money"));
        this.SkillLevel = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@skill_level"));
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
        string introVariable = "@intro_";
        string victoryVariable = "@victory_";
        if (Game.Data.IsVersionAtLeast(EssentialsVersion.v20))
        {
            introVariable += "BGM";
            victoryVariable += "BGM";
        }
        else
        {
            introVariable += "ME";
            victoryVariable += "ME";
        }
        if (Ruby.GetIVar(Data, introVariable) != Ruby.Nil) this.IntroBGM = Ruby.String.FromPtr(Ruby.GetIVar(Data, introVariable));
        if (Ruby.GetIVar(Data, "@battle_BGM") != Ruby.Nil) this.BattleBGM = Ruby.String.FromPtr(Ruby.GetIVar(Data, "@battle_BGM"));
        if (Ruby.GetIVar(Data, victoryVariable) != Ruby.Nil) this.VictoryBGM = Ruby.String.FromPtr(Ruby.GetIVar(Data, victoryVariable));
    }

    public nint Save()
    {
        nint e = Ruby.Funcall(Class, "new");
        Ruby.Pin(e);
        Ruby.SetIVar(e, "@id", Ruby.Symbol.ToPtr(this.ID));
        Ruby.SetIVar(e, "@real_name", Ruby.String.ToPtr(this.Name));
        Ruby.SetIVar(e, "@gender", Ruby.Integer.ToPtr(this.Gender));
        Ruby.SetIVar(e, "@base_money", Ruby.Integer.ToPtr(this.BaseMoney));
        Ruby.SetIVar(e, "@skill_level", Ruby.Integer.ToPtr(this.SkillLevel));
        nint FlagsArray = Ruby.Array.Create();
        Ruby.SetIVar(e, "@flags", FlagsArray);
        foreach (string Flag in Flags)
        {
            Ruby.Array.Push(FlagsArray, Ruby.String.ToPtr(Flag));
        }
        string introVariable = "@intro_";
        string victoryVariable = "@victory_";
        if (Game.Data.IsVersionAtLeast(EssentialsVersion.v20))
        {
            introVariable += "BGM";
            victoryVariable += "BGM";
        }
        else
        {
            introVariable += "ME";
            victoryVariable += "ME";
        }
        Ruby.SetIVar(e, introVariable, this.IntroBGM == null ? Ruby.Nil : Ruby.String.ToPtr(this.IntroBGM));
        Ruby.SetIVar(e, "@battle_BGM", this.BattleBGM == null ? Ruby.Nil : Ruby.String.ToPtr(this.BattleBGM));
        Ruby.SetIVar(e, victoryVariable, this.VictoryBGM == null ? Ruby.Nil : Ruby.String.ToPtr(this.VictoryBGM));
        Ruby.Unpin(e);
        return e;
    }

    public object Clone()
    {
        TrainerType t = new TrainerType();
        t.ID = this.ID;
        t.Name = this.Name;
        t.Gender = this.Gender;
        t.BaseMoney = this.BaseMoney;
        t.SkillLevel = this.SkillLevel;
        t.Flags = new List<string>(this.Flags);
        t.IntroBGM = this.IntroBGM;
        t.BattleBGM = this.BattleBGM;
        t.VictoryBGM = this.VictoryBGM;
        return t;
    }
}

[DebuggerDisplay("{ID}")]
public class TrainerTypeResolver
{
    public string ID;
    [JsonIgnore]
    public bool Valid => !string.IsNullOrEmpty(this.ID) && Data.TrainerTypes.ContainsKey(this.ID);
    [JsonIgnore]
    public TrainerType TrainerType => Data.TrainerTypes[this.ID];

    /// <summary>
    /// DO NOT USE!
    /// </summary>
    public TrainerTypeResolver() { }

    public TrainerTypeResolver(string ID)
    {
        this.ID = ID;
    }

    public TrainerTypeResolver(TrainerType TrainerType)
    {
        this.ID = TrainerType.ID;
    }

    public static implicit operator string(TrainerTypeResolver s) => s.ID;
    public static implicit operator TrainerType(TrainerTypeResolver s) => s.TrainerType;
    public static explicit operator TrainerTypeResolver(TrainerType s) => new TrainerTypeResolver(s);
    public static explicit operator TrainerTypeResolver(string ID) => new TrainerTypeResolver(ID);
}
