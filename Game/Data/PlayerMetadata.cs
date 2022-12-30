using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Game;

[DebuggerDisplay("{ID}: {TrainerType}")]
public class PlayerMetadata : IGameData
{
    public static nint Class => BaseDataManager.Classes["PlayerMetadata"];

    public int ID;
    public TrainerTypeResolver TrainerType;
    public string WalkCharset;
    public string? RunCharset;
    public string? CycleCharset;
    public string? SurfCharset;
    public string? DiveCharset;
    public string? FishCharset;
    public string? SurfFishCharset;
    public (int MapID, int X, int Y, Direction Dir)? Home;

    public PlayerMetadata(int ID, Dictionary<string, string> hash)
    {
        this.ID = ID;
        this.TrainerType = (TrainerTypeResolver) hash["TrainerType"];
        this.WalkCharset = hash["WalkCharset"];
        this.RunCharset = hash["RunCharset"];
        this.CycleCharset = hash["CycleCharset"];
        this.SurfCharset = hash["SurfCharset"];
        if (hash.ContainsKey("DiveCharset")) this.DiveCharset = hash["DiveCharset"];
        if (hash.ContainsKey("SurfFishCharset")) this.SurfFishCharset = hash["SurfFishCharset"];
        if (hash.ContainsKey("Home"))
        {
            int[] _home = hash["Home"].Split(',').Select(x => Convert.ToInt32(x.Trim())).ToArray();
            this.Home = (_home[0], _home[1], _home[2], (Direction) _home[3]);
        }
    }

    public PlayerMetadata(nint Data)
    {
        string GetStrOrNull(string Variable)
        {
            nint value = Ruby.GetIVar(Data, Variable);
            if (value == Ruby.Nil) return null;
            return Ruby.String.FromPtr(value);
        }
        this.ID = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@id"));
        this.TrainerType = (TrainerTypeResolver) Ruby.Symbol.FromPtr(Ruby.GetIVar(Data, "@trainer_type"));
        this.WalkCharset = Ruby.String.FromPtr(Ruby.GetIVar(Data, "@walk_charset"));
        this.RunCharset = GetStrOrNull("@run_charset");
        this.CycleCharset = GetStrOrNull("@cycle_charset");
        this.SurfCharset = GetStrOrNull("@surf_charset");
        this.DiveCharset = GetStrOrNull("@dive_charset");
        this.FishCharset = GetStrOrNull("@fish_charset");
        this.SurfFishCharset = GetStrOrNull("@surf_fish_charset");
        nint HomeArray = Ruby.GetIVar(Data, "@home");
        if (HomeArray != Ruby.Nil)
        {
            int HomeMapID = (int) Ruby.Integer.FromPtr(Ruby.Array.Get(HomeArray, 0));
            int HomeMapX = (int) Ruby.Integer.FromPtr(Ruby.Array.Get(HomeArray, 1));
            int HomeMapY = (int) Ruby.Integer.FromPtr(Ruby.Array.Get(HomeArray, 2));
            Direction HomeDir = (Direction) Ruby.Integer.FromPtr(Ruby.Array.Get(HomeArray, 3));
            this.Home = (HomeMapID, HomeMapX, HomeMapY, HomeDir);
        }
    }

    public nint Save()
    {
        nint e = Ruby.Funcall(Class, "new");
        Ruby.Pin(e);
        Ruby.SetIVar(e, "@id", Ruby.Integer.ToPtr(this.ID));
        Ruby.SetIVar(e, "@trainer_type", Ruby.Symbol.ToPtr(this.TrainerType));
        Ruby.SetIVar(e, "@walk_charset", Ruby.String.ToPtr(this.WalkCharset));
        Ruby.SetIVar(e, "@run_charset", this.RunCharset == null ? Ruby.Nil : Ruby.String.ToPtr(this.RunCharset));
        Ruby.SetIVar(e, "@cycle_charset", this.CycleCharset == null ? Ruby.Nil : Ruby.String.ToPtr(this.CycleCharset));
        Ruby.SetIVar(e, "@surf_charset", this.SurfCharset == null ? Ruby.Nil : Ruby.String.ToPtr(this.SurfCharset));
        Ruby.SetIVar(e, "@dive_charset", this.DiveCharset == null ? Ruby.Nil : Ruby.String.ToPtr(this.DiveCharset));
        Ruby.SetIVar(e, "@fish_charset", this.FishCharset == null ? Ruby.Nil : Ruby.String.ToPtr(this.FishCharset));
        Ruby.SetIVar(e, "@surf_fish_charset", this.SurfFishCharset == null ? Ruby.Nil : Ruby.String.ToPtr(this.SurfFishCharset));
        nint HomeArray = Ruby.Nil;
        if (Home.HasValue)
        {
            HomeArray = Ruby.Array.Create();
            Ruby.Pin(HomeArray);
            Ruby.Array.Push(HomeArray, Ruby.Integer.ToPtr(Home.Value.MapID));
            Ruby.Array.Push(HomeArray, Ruby.Integer.ToPtr(Home.Value.Y));
            Ruby.Array.Push(HomeArray, Ruby.Integer.ToPtr(Home.Value.X));
            Ruby.Array.Push(HomeArray, Ruby.Integer.ToPtr((int) Home.Value.Dir));
            Ruby.Unpin(HomeArray);
        }
        Ruby.SetIVar(e, "@home", HomeArray);
        Ruby.Unpin(e);
        return e;
    }
}