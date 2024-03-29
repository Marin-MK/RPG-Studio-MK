﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RPGStudioMK.Game;

public class Metadata : IGameData, ICloneable
{
    public static nint Class => BaseDataManager.Classes["Metadata"];

    public int ID;
    public int? StartMoney;
    public List<ItemResolver>? StartItemStorage;
    public (int MapID, int X, int Y, Direction Dir) Home;
    public string? RealStorageCreator;
    public string? WildBattleBGM;
    public string? TrainerBattleBGM;
    public string? WildVictoryBGM;
    public string? TrainerVictoryBGM;
    public string? WildCaptureME;
    public string? SurfBGM;
    public string? BicycleBGM;

    private Metadata() { }

    public Metadata(int ID, Dictionary<string, string> hash)
    {
        this.ID = ID;
        if (hash.ContainsKey("StartMoney")) this.StartMoney = Convert.ToInt32(hash["StartMoney"]);
        if (hash.ContainsKey("StartItemStorage")) this.StartItemStorage = hash["StartItemStorage"].Split(',').Select(x => (ItemResolver) x.Trim()).ToList();
        int[] _home = hash["Home"].Split(',').Select(x => Convert.ToInt32(x)).ToArray();
        this.Home = (_home[0], _home[1], _home[2], (Direction) _home[3]);
        if (hash.ContainsKey("StorageCreator")) this.RealStorageCreator = hash["StorageCreator"];
        bool v20plus = Game.Data.IsVersionAtLeast(EssentialsVersion.v20);
        this.WildBattleBGM = hash["WildBattleBGM"];
        this.TrainerBattleBGM = hash["TrainerBattleBGM"];
        this.WildVictoryBGM = hash[v20plus ? "WildVictoryBGM" : "WildVictoryME"];
        this.TrainerVictoryBGM = hash[v20plus ? "TrainerVictoryBGM" : "TrainerVictoryME"];
        if (hash.ContainsKey("WildCaptureME")) this.WildCaptureME = hash["WildCaptureME"];
        this.SurfBGM = hash["SurfBGM"];
        this.BicycleBGM = hash["BicycleBGM"];
    }

    public Metadata(nint Data)
    {
        string GetStrOrNull(string Variable)
        {
            nint value = Ruby.GetIVar(Data, Variable);
            if (value == Ruby.Nil) return null;
            return Ruby.String.FromPtr(value);
        }
        this.ID = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@id"));
        if (Ruby.GetIVar(Data, "@start_money") != Ruby.Nil) this.StartMoney = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@start_money"));
        nint ItemsArray = Ruby.GetIVar(Data, "@start_item_storage");
        if (ItemsArray != Ruby.Nil)
        {
            int ItemCount = (int) Ruby.Array.Length(ItemsArray);
            this.StartItemStorage = new List<ItemResolver>();
            for (int i = 0; i < ItemCount; i++)
            {
                string item = Ruby.Symbol.FromPtr(Ruby.Array.Get(ItemsArray, i));
                this.StartItemStorage.Add((ItemResolver) item);
            }
        }
        nint HomeArray = Ruby.GetIVar(Data, "@home");
        int HomeMapID = (int) Ruby.Integer.FromPtr(Ruby.Array.Get(HomeArray, 0));
        int HomeMapX = (int) Ruby.Integer.FromPtr(Ruby.Array.Get(HomeArray, 1));
        int HomeMapY = (int) Ruby.Integer.FromPtr(Ruby.Array.Get(HomeArray, 2));
        Direction HomeDir = (Direction) Ruby.Integer.FromPtr(Ruby.Array.Get(HomeArray, 3));
        this.Home = (HomeMapID, HomeMapX, HomeMapY, HomeDir);
        this.RealStorageCreator = GetStrOrNull("@real_storage_creator");
        this.WildBattleBGM = GetStrOrNull("@wild_battle_BGM");
        this.TrainerBattleBGM = GetStrOrNull("@trainer_battle_BGM");
        this.WildVictoryBGM = GetStrOrNull("@wild_victory_BGM");
        this.TrainerVictoryBGM = GetStrOrNull("@trainer_victory_BGM");
        this.WildCaptureME = GetStrOrNull("@wild_capture_ME");
        this.SurfBGM = GetStrOrNull("@surf_BGM");
        this.BicycleBGM = GetStrOrNull("@bicycle_BGM");
    }

    public nint Save()
    {
        nint e = Ruby.Funcall(Class, "new");
        Ruby.Pin(e);
        Ruby.SetIVar(e, "@id", Ruby.Integer.ToPtr(this.ID));
        if (this.StartMoney.HasValue) Ruby.SetIVar(e, "@start_money", Ruby.Integer.ToPtr((int) this.StartMoney));
        nint ItemsArray = Ruby.Array.Create();
        Ruby.SetIVar(e, "@start_item_storage", ItemsArray);
        foreach (string Item in StartItemStorage)
        {
            Ruby.Array.Push(ItemsArray, Ruby.Symbol.ToPtr(Item));
        }
        nint HomeArray = Ruby.Array.Create();
        Ruby.SetIVar(e, "@home", HomeArray);
        Ruby.Array.Push(HomeArray, Ruby.Integer.ToPtr(Home.MapID));
        Ruby.Array.Push(HomeArray, Ruby.Integer.ToPtr(Home.X));
        Ruby.Array.Push(HomeArray, Ruby.Integer.ToPtr(Home.Y));
        Ruby.Array.Push(HomeArray, Ruby.Integer.ToPtr((int) Home.Dir));
        Ruby.SetIVar(e, "@real_storage_creator", this.RealStorageCreator == null ? Ruby.Nil : Ruby.String.ToPtr(this.RealStorageCreator));
        Ruby.SetIVar(e, "@wild_battle_BGM", this.WildBattleBGM == null ? Ruby.Nil : Ruby.String.ToPtr(this.WildBattleBGM));
        Ruby.SetIVar(e, "@trainer_battle_BGM", this.TrainerBattleBGM == null ? Ruby.Nil : Ruby.String.ToPtr(this.TrainerBattleBGM));
        Ruby.SetIVar(e, "@wild_victory_BGM", this.WildVictoryBGM == null ? Ruby.Nil : Ruby.String.ToPtr(this.WildVictoryBGM));
        Ruby.SetIVar(e, "@trainer_victory_BGM", this.TrainerVictoryBGM == null ? Ruby.Nil : Ruby.String.ToPtr(this.TrainerVictoryBGM));
        Ruby.SetIVar(e, "@wild_capture_ME", this.WildCaptureME == null ? Ruby.Nil : Ruby.String.ToPtr(this.WildCaptureME));
        Ruby.SetIVar(e, "@surf_BGM", this.SurfBGM == null ? Ruby.Nil : Ruby.String.ToPtr(this.SurfBGM));
        Ruby.SetIVar(e, "@bicycle_BGM", this.BicycleBGM == null ? Ruby.Nil : Ruby.String.ToPtr(this.BicycleBGM));
        Ruby.Unpin(e);
        return e;
	}

	public string SaveToString()
	{
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("#-------------------------------");
        sb.AppendLine("[0]");
        if (this.StartMoney is not null) sb.AppendLine($"StartMoney = {this.StartMoney}");
        if (this.StartItemStorage is not null) sb.AppendLine($"StartItemStorage = {this.StartItemStorage.Select(x => x.ID).Aggregate((a, b) => a + "," + b)}");
        sb.AppendLine($"Home = {this.Home.MapID},{this.Home.X},{this.Home.Y},{(int) this.Home.Dir}");
        if (this.RealStorageCreator is not null) sb.AppendLine($"StorageCreator = {this.RealStorageCreator}");
        if (this.WildBattleBGM is not null) sb.AppendLine($"WildBattleBGM = {this.WildBattleBGM}");
        if (this.TrainerBattleBGM is not null) sb.AppendLine($"TrainerBattleBGM = {this.TrainerBattleBGM}");
        if (this.WildVictoryBGM is not null) sb.AppendLine($"WildVictoryBGM = {this.WildVictoryBGM}");
        if (this.TrainerVictoryBGM is not null) sb.AppendLine($"TrainerVictoryBGM = {this.TrainerVictoryBGM}");
        if (this.WildCaptureME is not null) sb.AppendLine($"WildCaptureME = {this.WildCaptureME}");
        if (this.SurfBGM is not null) sb.AppendLine($"SurfBGM = {this.SurfBGM}");
        if (this.BicycleBGM is not null) sb.AppendLine($"BicycleBGM = {this.BicycleBGM}");
        return sb.ToString();
	}

	public object Clone()
    {
        Metadata m = new Metadata();
        m.ID = this.ID;
        m.StartMoney = this.StartMoney;
        m.StartItemStorage = this.StartItemStorage.Select(x => (ItemResolver) x.ID).ToList();
        m.Home = (this.Home.MapID, this.Home.X, this.Home.Y, this.Home.Dir);
        m.RealStorageCreator = this.RealStorageCreator;
        m.WildBattleBGM = this.WildBattleBGM;
        m.TrainerBattleBGM = this.TrainerBattleBGM;
        m.WildVictoryBGM = this.WildVictoryBGM;
        m.TrainerVictoryBGM = this.TrainerVictoryBGM;
        m.WildCaptureME = this.WildCaptureME;
        m.SurfBGM = this.SurfBGM;
        m.BicycleBGM = this.BicycleBGM;
        return m;
    }
}