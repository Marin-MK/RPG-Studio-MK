using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Game;

public class Ability
{
    public static nint Class = nint.Zero;

    public string ID;
    public string Name;
    public string Description;
    public List<string> Flags;

    public Ability(string ID, Dictionary<string, string> hash)
    {
        this.ID = ID;
        this.Name = hash["Name"];
        this.Description = hash["Description"];
        if (hash.ContainsKey("Flags")) this.Flags = hash["Flags"].Split(',').Select(x => x.Trim()).ToList();
        else this.Flags = new List<string>();
    }

    public Ability(nint Data)
    {
        this.ID = Ruby.Symbol.FromPtr(Ruby.GetIVar(Data, "@id"));
        this.Name = Ruby.String.FromPtr(Ruby.GetIVar(Data, "@real_name"));
        this.Description = Ruby.String.FromPtr(Ruby.GetIVar(Data, "@real_description"));
        nint FlagsArray = Ruby.GetIVar(Data, "@flags");
        int FlagsArrayLength = (int) Ruby.Array.Length(FlagsArray);
        this.Flags = new List<string>();
        for (int i = 0; i < FlagsArrayLength; i++)
        {
            this.Flags.Add(Ruby.String.FromPtr(Ruby.Array.Get(FlagsArray, i)));
        }
        Ruby.SetGlobal("$d", Data);
        Ruby.Eval("p $d");
    }

    public nint Save()
    {
        nint e = Ruby.Funcall(Class, "new");
        Ruby.Pin(e);
        Ruby.SetIVar(e, "@id", Ruby.Symbol.ToPtr(this.ID));
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
}
