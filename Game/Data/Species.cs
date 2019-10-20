using System;
using System.Collections.Generic;

namespace MKEditor.Game
{
    public class Species : Serializable
    {
        public string IntName;
        public int ID;
        public List<Species> Forms = new List<Species>();
        public string Name;
        public string Type1;
        public string Type2;
        public Stats Stats;
        public List<string> Abilities;
        public string HiddenAbility;
        public Stats EVYield;
        public List<string> EggGroups;
        public float Height;
        public float Weight;
        public int BaseEXP;
        public string LevelingRate;
        public string GenderRatio;
        public byte CatchRate;
        public int HatchCycles;
        public string PokedexColor;
        public string PokedexEntry;
        public byte Happiness;
        public Moveset Moveset;
        public List<Evolution> Evolutions = new List<Evolution>();

        public Species(string path, VariableType VarType = VariableType.Normal)
            : base(path)
        {
            if (!Nil("id", VarType)) this.ID = GetVar<int>("id", VarType);
            if (!Nil("intname", VarType)) this.IntName = GetVar<string>("intname", VarType);
            if (!Nil("name", VarType)) this.Name = GetVar<string>("name", VarType);
            if (!Nil("type1", VarType)) this.Type1 = GetVar<string>("type1", VarType);
            if (!Nil("type2", VarType)) this.Type2 = GetVar<string>("type2", VarType);
            if (!Nil("stats", VarType)) this.Stats = new Stats(GetPath("stats", VarType));
            if (!Nil("abilities", VarType)) this.Abilities = GetList<string>("abilities", VarType);
            if (!Nil("hidden_ability", VarType)) this.HiddenAbility = GetVar<string>("hidden_ability", VarType);
            if (!Nil("ev_yield", VarType)) this.EVYield = new Stats(GetPath("ev_yield", VarType));
            if (!Nil("egg_groups", VarType)) this.EggGroups = GetList<string>("egg_groups", VarType);
            if (!Nil("height", VarType)) this.Height = GetVar<float>("height", VarType);
            if (!Nil("weight", VarType)) this.Weight = GetVar<float>("weight", VarType);
            if (!Nil("base_exp", VarType)) this.BaseEXP = GetVar<int>("base_exp", VarType);
            if (!Nil("leveling_rate", VarType)) this.LevelingRate = GetVar<string>("leveling_rate", VarType);
            if (!Nil("gender_rate", VarType)) this.GenderRatio = GetVar<string>("gender_ratio", VarType);
            if (!Nil("catch_rate", VarType)) this.CatchRate = GetVar<byte>("catch_rate", VarType);
            if (!Nil("hatch_cycles", VarType)) this.HatchCycles = GetVar<int>("hatch_cycles", VarType);
            if (!Nil("pokedex_color", VarType)) this.PokedexColor = GetVar<string>("pokedex_color", VarType);
            if (!Nil("pokedex_entry", VarType)) this.PokedexEntry = GetVar<string>("pokedex_entry", VarType);
            if (!Nil("happiness", VarType)) this.Happiness = GetVar<byte>("happiness", VarType);
            if (!Nil("moveset", VarType)) this.Moveset = new Moveset(GetPath("moveset", VarType));
            if (!Nil("evolutions", VarType))
            {
                int evocount = GetCount("evolutions", VarType);
                for (int i = 0; i < evocount; i++)
                {
                    this.Evolutions.Add(new Evolution($"{GetPath("evolutions", VarType)}[{i}]"));
                }
            }
            // VarType check so you can't have a forms property within a form
            if (VarType == VariableType.Normal && !Nil("forms"))
            {
                List<int> keys = GetKeys<int>("forms");
                foreach (int form in keys)
                {
                    if (form > Forms.Count) Forms.AddRange(new Species[form - Forms.Count + 1]);
                    Forms[form] = new Species($"{GetPath("forms")}[{form}]", VariableType.HashSymbol);
                }
            }
        }
    }

    public class Stats : Serializable
    {
        public int HP = 0;
        public int Attack = 0;
        public int Defense = 0;
        public int SpAtk = 0;
        public int SpDef = 0;
        public int Speed = 0;

        public Stats(string path)
            : base(path)
        {
            if (!Nil("hp", VariableType.Struct)) this.HP = GetVar<int>("hp", VariableType.Struct);
            if (!Nil("attack", VariableType.Struct)) this.Attack = GetVar<int>("attack", VariableType.Struct);
            if (!Nil("defense", VariableType.Struct)) this.Defense = GetVar<int>("defense", VariableType.Struct);
            if (!Nil("spatk", VariableType.Struct)) this.SpAtk = GetVar<int>("spatk", VariableType.Struct);
            if (!Nil("spdef", VariableType.Struct)) this.SpDef = GetVar<int>("spdef", VariableType.Struct);
            if (!Nil("speed", VariableType.Struct)) this.Speed = GetVar<int>("speed", VariableType.Struct);
        }
    }

    public class Moveset : Serializable
    {
        public Dictionary<int, object> Level = new Dictionary<int, object>();
        public List<string> TMs;
        public List<string> Tutor;
        public List<string> Evolution;
        public List<string> Egg;

        public Moveset(string path)
            : base(path)
        {
            List<int> levelkeys = GetKeys<int>("level", VariableType.Struct);
            foreach (int key in levelkeys)
            {
                bool array = Data.Exec($"{GetPath("level", VariableType.Struct)}[{key}].is_a?(Array)");
                object value;
                if (array)
                {
                    value = GetList<string>($"level[{key}]", VariableType.Struct);
                }
                else
                {
                    value = GetVar<string>($"level[{key}]", VariableType.Struct);
                }
                this.Level.Add(key, value);
            }
            if (!Nil("tms", VariableType.Struct)) this.TMs = GetList<string>("tms", VariableType.Struct);
            if (!Nil("tutor", VariableType.Struct)) this.Tutor = GetList<string>("tutor", VariableType.Struct);
            if (!Nil("evolution", VariableType.Struct)) this.Evolution = GetList<string>("evolution", VariableType.Struct);
            if (!Nil("egg", VariableType.Struct)) this.Egg = GetList<string>("egg", VariableType.Struct);
        }
    }

    public class Evolution : Serializable
    {
        public string Mode;
        public string Species;
        public object Argument;

        public Evolution(string path)
            : base(path)
        {
            this.Mode = GetVar<string>("mode", VariableType.HashSymbol);
            this.Species = GetVar<string>("species", VariableType.HashSymbol);
            bool num = Data.Exec($"{GetPath("argument", VariableType.HashSymbol)}.is_a?(Integer)");
            if (num)
            {
                this.Argument = GetVar<int>("argument", VariableType.HashSymbol);
            }
            else
            {
                this.Argument = GetVar<string>("argument", VariableType.HashSymbol);
            }
        }
    }
}
