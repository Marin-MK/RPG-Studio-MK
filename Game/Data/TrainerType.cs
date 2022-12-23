using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Game;

public class TrainerType
{
    public static nint Class = nint.Zero;

    public string ID;
    public string Name;
    public int Gender;
    public int BaseMoney;
    public int SkillLevel;
    public List<string> Flags;
    public string? IntroBGM;
    public string? BattleBGM;
    public string? VictoryBGM;

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

    public TrainerType(nint Data)
    {
        this.ID = Ruby.Symbol.FromPtr(Ruby.GetIVar(Data, "@id"));
        this.Name = Ruby.String.FromPtr(Ruby.GetIVar(Data, "@real_name"));
        this.Gender = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@gender"));
        this.BaseMoney = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@base_money"));
        this.SkillLevel = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@skill_level"));
        nint FlagsArray = Ruby.GetIVar(Data, "@flags");
        int FlagsArrayLength = (int) Ruby.Array.Length(FlagsArray);
        this.Flags = new List<string>();
        for (int i = 0; i < FlagsArrayLength; i++)
        {
            this.Flags.Add(Ruby.String.FromPtr(Ruby.Array.Get(FlagsArray, i)));
        }
        Ruby.SetGlobal("$d", Data);
        Ruby.Eval("p $d");
        if (Ruby.GetIVar(Data, "@intro_BGM") != Ruby.Nil) this.IntroBGM = Ruby.String.FromPtr(Ruby.GetIVar(Data, "@intro_BGM"));
        if (Ruby.GetIVar(Data, "@battle_BGM") != Ruby.Nil) this.BattleBGM = Ruby.String.FromPtr(Ruby.GetIVar(Data, "@battle_BGM"));
        if (Ruby.GetIVar(Data, "@victory_BGM") != Ruby.Nil) this.VictoryBGM = Ruby.String.FromPtr(Ruby.GetIVar(Data, "@victory_BGM"));
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
        Ruby.SetIVar(e, "@intro_BGM", this.IntroBGM == null ? Ruby.Nil : Ruby.String.ToPtr(this.IntroBGM));
        Ruby.SetIVar(e, "@battle_BGM", this.BattleBGM == null ? Ruby.Nil : Ruby.String.ToPtr(this.BattleBGM));
        Ruby.SetIVar(e, "@victory_BGM", this.VictoryBGM == null ? Ruby.Nil : Ruby.String.ToPtr(this.VictoryBGM));
        Ruby.Unpin(e);
        return e;
    }
}
