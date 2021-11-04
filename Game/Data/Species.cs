using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace RPGStudioMK.Game
{
    public class Species
    {
        public string IntName;
        public int ID;
        public List<Species> Forms;
        public string Name;
        public string Type1;
        public string Type2;
        public Stats Stats;
        public List<string> Abilities;
        public string HiddenAbility;
        public Stats EVYield;
        public List<string> EggGroups;
        public float? Height;
        public float? Weight;
        public int? BaseEXP;
        public string LevelingRate;
        public string GenderRatio;
        public byte? CatchRate;
        public int? HatchCycles;
        public string PokedexColor;
        public string PokedexEntry;
        public byte? Happiness;
        public Moveset Moveset;
        public List<Evolution> Evolutions;
        public Species BaseForm;

        public Species(Dictionary<string, object> Data, string Prefix = "@")
        {
            if (Prefix == "@") // If there are instance variables, there is also a class
            {
                if (Data.ContainsKey("^c"))
                {
                    if ((string) Data["^c"] != "Species") throw new Exception("Invalid class - Expected class of type Species but got " + (string) Data["^c"] + ".");
                }
                else
                {
                    throw new Exception("Could not find a ^c key to identify this class.");
                }
            }
            if (Data.ContainsKey($"{Prefix}id")) this.ID = Convert.ToInt32(Data[$"{Prefix}id"]);
            if (Data.ContainsKey($"{Prefix}intname")) this.IntName = ((string) Data[$"{Prefix}intname"]).Replace(":", "");
            if (Data.ContainsKey($"{Prefix}name")) this.Name = (string) Data[$"{Prefix}name"];
            if (Data.ContainsKey($"{Prefix}type1")) this.Type1 = ((string) Data[$"{Prefix}type1"]).Replace(":", "");
            if (Data.ContainsKey($"{Prefix}type2")) this.Type2 = ((string) Data[$"{Prefix}type2"]).Replace(":", "");
            if (Data.ContainsKey($"{Prefix}stats")) this.Stats = new Stats(((JObject) Data[$"{Prefix}stats"]).ToObject<Dictionary<string, object>>());
            if (Data.ContainsKey($"{Prefix}abilities"))
            {
                this.Abilities = ((JArray) Data[$"{Prefix}abilities"]).ToObject<List<string>>();
                for (int i = 0; i < this.Abilities.Count; i++) this.Abilities[i] = this.Abilities[i].Replace(":", "");
            }
            if (Data.ContainsKey($"{Prefix}hidden_ability")) this.HiddenAbility = ((string) Data[$"{Prefix}hidden_ability"]).Replace(":", "");
            if (Data.ContainsKey($"{Prefix}ev_yield")) this.EVYield = new Stats(((JObject) Data[$"{Prefix}ev_yield"]).ToObject<Dictionary<string, object>>());
            if (Data.ContainsKey($"{Prefix}egg_groups"))
            {
                this.EggGroups = ((JArray) Data[$"{Prefix}egg_groups"]).ToObject<List<string>>();
                for (int i = 0; i < this.EggGroups.Count; i++) this.EggGroups[i] = this.EggGroups[i].Replace(":", "");
            }
            if (Data.ContainsKey($"{Prefix}height")) this.Height = (float) Convert.ToDouble(Data[$"{Prefix}height"]);
            if (Data.ContainsKey($"{Prefix}weight")) this.Weight = (float) Convert.ToDouble(Data[$"{Prefix}weight"]);
            if (Data.ContainsKey($"{Prefix}base_exp")) this.BaseEXP = Convert.ToInt32(Data[$"{Prefix}base_exp"]);
            if (Data.ContainsKey($"{Prefix}leveling_rate")) this.LevelingRate = ((string) Data[$"{Prefix}leveling_rate"]).Replace(":", "");
            if (Data.ContainsKey($"{Prefix}gender_ratio")) this.GenderRatio = ((string) Data[$"{Prefix}gender_ratio"]).Replace(":", "");
            if (Data.ContainsKey($"{Prefix}catch_rate")) this.CatchRate = Convert.ToByte(Data[$"{Prefix}catch_rate"]);
            if (Data.ContainsKey($"{Prefix}hatch_cycles")) this.HatchCycles = Convert.ToInt32(Data[$"{Prefix}hatch_cycles"]);
            if (Data.ContainsKey($"{Prefix}pokedex_color")) this.PokedexColor = ((string) Data[$"{Prefix}pokedex_color"]).Replace(":", "");
            if (Data.ContainsKey($"{Prefix}pokedex_entry")) this.PokedexEntry = (string) Data[$"{Prefix}pokedex_entry"];
            if (Data.ContainsKey($"{Prefix}happiness")) this.Happiness = Convert.ToByte(Data[$"{Prefix}happiness"]);

            if (Data.ContainsKey($"{Prefix}moveset"))
            {
                this.Moveset = new Moveset(((JObject) Data[$"{Prefix}moveset"]).ToObject<Dictionary<string, object>>());
            }

            if (Data.ContainsKey($"{Prefix}evolutions"))
            {
                Evolutions = new List<Evolution>();
                foreach (object o in ((JArray) Data[$"{Prefix}evolutions"]).ToObject<List<object>>())
                {
                    this.Evolutions.Add(new Evolution(((JObject) o).ToObject<Dictionary<string, object>>()));
                }
            }

            if (Prefix == "@" && Data.ContainsKey("@forms"))
            {
                Forms = new List<Species>();
                Dictionary<int, object> forms = ((JObject) Data["@forms"]).ToObject<Dictionary<int, object>>();
                foreach (KeyValuePair<int, object> kvp in forms)
                {
                    int id = kvp.Key;
                    Species form = new Species(((JObject) kvp.Value).ToObject<Dictionary<string, object>>(), ":");
                    form.BaseForm = this;
                    if (id >= this.Forms.Count) this.Forms.AddRange(new Species[id - this.Forms.Count + 1]);
                    this.Forms[id] = form;
                }
            }
        }

        public Dictionary<string, object> ToJSON(string Prefix = "@")
        {
            Dictionary<string, object> Data = new Dictionary<string, object>();
            if (Prefix == "@") Data["^c"] = "Species";
            Data[$"{Prefix}id"] = ID;
            if (Stats != null) Data[$"{Prefix}stats"] = Stats.ToJSON();
            if (EVYield != null) Data[$"{Prefix}ev_yield"] = EVYield.ToJSON();
            if (Moveset != null) Data[$"{Prefix}moveset"] = Moveset.ToJSON();
            if (Forms != null && Forms.Count > 1 && Prefix == "@")
            {
                Dictionary<int, Dictionary<string, object>> forms = new Dictionary<int, Dictionary<string, object>>();
                for (int i = 1; i < Forms.Count; i++) forms[i] = Forms[i].ToJSON(":");
                Data["@forms"] = forms;
            }
            if (!string.IsNullOrEmpty(IntName)) Data[$"{Prefix}intname"] = ":" + IntName;
            if (!string.IsNullOrEmpty(Name)) Data[$"{Prefix}name"] = Name;
            if (!string.IsNullOrEmpty(Type1)) Data[$"{Prefix}type1"] = ":" + Type1;
            if (!string.IsNullOrEmpty(Type2)) Data[$"{Prefix}type2"] = ":" + Type2;
            if (Abilities != null)
            {
                List<string> abils = new List<string>(Abilities);
                for (int i = 0; i < abils.Count; i++) abils[i] = ":" + abils[i];
                Data[$"{Prefix}abilities"] = abils;
            }
            if (!string.IsNullOrEmpty(HiddenAbility)) Data[$"{Prefix}hidden_ability"] = ":" + HiddenAbility;
            if (EggGroups != null)
            {
                List<string> groups = new List<string>(EggGroups);
                for (int i = 0; i < groups.Count; i++) groups[i] = ":" + groups[i];
                Data[$"{Prefix}egg_groups"] = groups;
            }
            if (Height != null) Data[$"{Prefix}height"] = Height;
            if (Weight != null) Data[$"{Prefix}weight"] = Weight;
            if (BaseEXP != null) Data[$"{Prefix}base_exp"] = BaseEXP;
            if (!string.IsNullOrEmpty(LevelingRate)) Data[$"{Prefix}leveling_rate"] = ":" + LevelingRate;
            if (!string.IsNullOrEmpty(GenderRatio)) Data[$"{Prefix}gender_ratio"] = ":" + GenderRatio;
            if (CatchRate != null) Data[$"{Prefix}catch_rate"] = CatchRate;
            if (HatchCycles != null) Data[$"{Prefix}hatch_cycles"] = HatchCycles;
            if (!string.IsNullOrEmpty(PokedexColor)) Data[$"{Prefix}pokedex_color"] = ":" + PokedexColor;
            if (!string.IsNullOrEmpty(PokedexEntry)) Data[$"{Prefix}pokedex_entry"] = PokedexEntry;
            if (Happiness != null) Data[$"{Prefix}happiness"] = Happiness;
            if (Evolutions != null) Data[$"{Prefix}evolutions"] = new List<Dictionary<string, object>>(Array.ConvertAll(Evolutions.ToArray(), e => e.ToJSON()));
            return Data;
        }
    }

    public class Stats
    {
        public int HP = 0;
        public int Attack = 0;
        public int Defense = 0;
        public int SpAtk = 0;
        public int SpDef = 0;
        public int Speed = 0;

        public Stats(Dictionary<string, object> Data)
        {
            if (Data.ContainsKey("^c"))
            {
                if ((string) Data["^c"] != "Stats") throw new Exception("Invalid class - Expected class of type Stats but got " + (string) Data["^c"] + ".");
            }
            else
            {
                throw new Exception("Could not find a ^c key to identify this class.");
            }
            if (Data.ContainsKey("@hp")) this.HP = Convert.ToInt32(Data["@hp"]);
            if (Data.ContainsKey("@attack")) this.Attack = Convert.ToInt32(Data["@attack"]);
            if (Data.ContainsKey("@defense")) this.Defense = Convert.ToInt32(Data["@defense"]);
            if (Data.ContainsKey("@spatk")) this.SpAtk = Convert.ToInt32(Data["@spatk"]);
            if (Data.ContainsKey("@spdef")) this.SpDef = Convert.ToInt32(Data["@spdef"]);
            if (Data.ContainsKey("@speed")) this.Speed = Convert.ToInt32(Data["@speed"]);
        }

        public Dictionary<string, object> ToJSON()
        {
            Dictionary<string, object> Data = new Dictionary<string, object>();
            Data["^c"] = "Stats";
            Data["@hp"] = HP;
            Data["@attack"] = Attack;
            Data["@defense"] = Defense;
            Data["@spatk"] = SpAtk;
            Data["@spdef"] = SpDef;
            Data["@speed"] = Speed;
            return Data;
        }
    }

    public class Moveset
    {
        public Dictionary<int, object> Level = new Dictionary<int, object>();
        public List<string> TMs;
        public List<string> Tutor;
        public List<string> Evolution;
        public List<string> Egg;

        public Moveset(Dictionary<string, object> Data)
        {
            if (Data.ContainsKey("^c"))
            {
                if ((string) Data["^c"] != "Species::Moveset") throw new Exception("Invalid class - Expected class of type Species::Moveset but got " + (string) Data["^c"] + ".");
            }
            else
            {
                throw new Exception("Could not find a ^c key to identify this class.");
            }
            Dictionary<int, object> level = ((JObject) Data["@level"]).ToObject<Dictionary<int, object>>();
            foreach (KeyValuePair<int, object> kvp in level)
            {
                if (kvp.Value is string)
                {
                    Level[kvp.Key] = ((string) kvp.Value).Replace(":", "");
                }
                else
                {
                    List<string> list = ((JArray) kvp.Value).ToObject<List<string>>();
                    for (int i = 0; i < list.Count; i++) list[i] = list[i].Replace(":", "");
                    Level[kvp.Key] = list;
                }
            }
            this.TMs = ((JArray) Data["@tms"]).ToObject<List<string>>();
            for (int i = 0; i < TMs.Count; i++) TMs[i] = TMs[i].Replace(":", "");

            this.Tutor = ((JArray) Data["@tutor"]).ToObject<List<string>>();
            for (int i = 0; i < Tutor.Count; i++) Tutor[i] = Tutor[i].Replace(":", "");

            this.Evolution = ((JArray) Data["@evolution"]).ToObject<List<string>>();
            for (int i = 0; i < Evolution.Count; i++) Evolution[i] = Evolution[i].Replace(":", "");

            this.Egg = ((JArray) Data["@egg"]).ToObject<List<string>>();
            for (int i = 0; i < Egg.Count; i++) Egg[i] = Egg[i].Replace(":", "");
        }

        public Dictionary<string, object> ToJSON()
        {
            Dictionary<string, object> Data = new Dictionary<string, object>();
            Data["^c"] = "Species::Moveset";
            Dictionary<int, object> level = new Dictionary<int, object>();
            foreach (KeyValuePair<int, object> kvp in Level)
            {
                if (kvp.Value is string) level[kvp.Key] = ":" + kvp.Value;
                else level[kvp.Key] = new List<string>(Array.ConvertAll(((List<string>) kvp.Value).ToArray(), e => ":" + e));
            }
            Data["@level"] = level;
            Data["@tms"] = new List<string>(Array.ConvertAll(TMs.ToArray(), e => ":" + e));
            Data["@tutor"] = new List<string>(Array.ConvertAll(Tutor.ToArray(), e => ":" + e));
            Data["@evolution"] = new List<string>(Array.ConvertAll(Evolution.ToArray(), e => ":" + e));
            Data["@egg"] = new List<string>(Array.ConvertAll(Egg.ToArray(), e => ":" + e));
            return Data;
        }
    }

    public class Evolution
    {
        public string Mode;
        public string Species;
        public object Argument;

        public Evolution(Dictionary<string, object> Data)
        {
            this.Mode = ((string) Data[":mode"]).Replace(":", "");
            this.Species = ((string) Data[":species"]).Replace(":", "");
            object o = Data[":argument"];
            if (o is int) this.Argument = Convert.ToInt32(o);
            else if (o is string) this.Argument = (string) o;
            else this.Argument = o;
        }

        public Dictionary<string, object> ToJSON()
        {
            Dictionary<string, object> Data = new Dictionary<string, object>();
            Data[":mode"] = ":" + Mode;
            Data[":species"] = ":" + Species;
            Data[":argument"] = Argument;
            return Data;
        }
    }
}
