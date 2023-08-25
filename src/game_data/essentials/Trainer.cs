using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static rubydotnet.Ruby;

namespace RPGStudioMK.Game;

[DebuggerDisplay("{TrainerType} {Name} {Version}")]
public class Trainer : IGameData, ICloneable
{
    public static nint Class => BaseDataManager.Classes["Trainer"];

    public TrainerTypeResolver TrainerType;
    public string Name;
    public int Version;
    public string LoseText;
    public List<ItemResolver> Items;
    public List<TrainerPokemon> Party;

    private Trainer() { }

    public Trainer(string ID, List<(string Key, string Value)> pairs)
    {
        string[] split = ID.Split(',');
        this.TrainerType = (TrainerTypeResolver) split[0].Trim();
        this.Name = split[1].Trim();
        if (split.Length == 3) this.Version = Convert.ToInt32(split[2].Trim());
        else this.Version = 0;
        this.LoseText = pairs.Find(x => x.Key == "LoseText").Value;
        (string, string)? x = pairs.Find(x => x.Key == "Items");
        if (x.HasValue && x.Value.Item1 != null)
        {
            this.Items = x.Value.Item2.Split(',').Select(x => (ItemResolver) x.Trim()).ToList();
        }
        else this.Items = new List<ItemResolver>();
        int sidx = -1;
        this.Party = new List<TrainerPokemon>();
        for (int i = 0; i < pairs.Count; i++)
        {
            if (pairs[i].Key == "Pokemon")
            {
                if (sidx == -1) sidx = i;
                else
                {
                    this.Party.Add(new TrainerPokemon(pairs.GetRange(sidx, i - sidx)));
                    sidx = i;
                }
            }
            else if (pairs[i].Key == "LoseText" || pairs[i].Key == "Items")
            {
                if (sidx != -1)
                {
                    this.Party.Add(new TrainerPokemon(pairs.GetRange(sidx, i - sidx)));
                    sidx = i;
                }
            }
        }
        if (sidx != pairs.Count - 1)
        {
            this.Party.Add(new TrainerPokemon(pairs.GetRange(sidx, pairs.Count - sidx)));
        }
    }

    public Trainer(nint Data)
    {
        this.TrainerType = (TrainerTypeResolver) Ruby.Symbol.FromPtr(Ruby.GetIVar(Data, "@trainer_type"));
        this.Name = Ruby.String.FromPtr(Ruby.GetIVar(Data, "@real_name"));
        this.Version = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Data, "@version"));
        this.LoseText = Ruby.String.FromPtr(Ruby.GetIVar(Data, "@real_lose_text"));
        nint ItemsArray = Ruby.GetIVar(Data, "@items");
        int ItemsArrayLength = (int) Ruby.Array.Length(ItemsArray);
        this.Items = new List<ItemResolver>();
        for (int i = 0; i < ItemsArrayLength; i++)
        {
            this.Items.Add((ItemResolver) Ruby.Symbol.FromPtr(Ruby.Array.Get(ItemsArray, i)));
        }
        nint PartyArray = Ruby.GetIVar(Data, "@pokemon");
        int PartyLength = (int) Ruby.Array.Length(PartyArray);
        this.Party = new List<TrainerPokemon>();
        for (int i = 0; i < PartyLength; i++)
        {
            this.Party.Add(new TrainerPokemon(Ruby.Array.Get(PartyArray, i)));
        }
    }

    public nint Save()
    {
        nint e = Ruby.Funcall(Class, "new");
        Ruby.Pin(e);
        nint id = Ruby.Array.Create(3);
        Ruby.SetIVar(e, "@id", id);
        Ruby.Array.Set(id, 0, Ruby.Symbol.ToPtr(this.TrainerType.ID));
        Ruby.Array.Set(id, 1, Ruby.String.ToPtr(this.Name));
        Ruby.Array.Set(id, 2, Ruby.Integer.ToPtr(this.Version));
        Ruby.SetIVar(e, "@trainer_type", Ruby.Symbol.ToPtr(this.TrainerType));
        Ruby.SetIVar(e, "@real_name", Ruby.String.ToPtr(this.Name));
        Ruby.SetIVar(e, "@version", Ruby.Integer.ToPtr(this.Version));
        Ruby.SetIVar(e, "@real_lose_text", Ruby.String.ToPtr(this.LoseText));
        nint ItemsArray = Ruby.Array.Create();
        Ruby.SetIVar(e, "@items", ItemsArray);
        foreach (string Item in Items)
        {
            Ruby.Array.Push(ItemsArray, Ruby.Symbol.ToPtr(Item));
        }
        nint PartyArray = Ruby.Array.Create();
        Ruby.SetIVar(e, "@pokemon", PartyArray);
        foreach (TrainerPokemon pokemon in Party)
        {
            Ruby.Array.Push(PartyArray, pokemon.Save());
        }
        Ruby.Unpin(e);
        return e;
    }

    public object Clone()
    {
        Trainer t = new Trainer();
        t.TrainerType = (TrainerTypeResolver) this.TrainerType.ID;
        t.Name = this.Name;
        t.Version = this.Version;
        t.LoseText = this.LoseText;
        t.Items = this.Items.Select(x => (ItemResolver) x.ID).ToList();
        t.Party = this.Party.Select(x => (TrainerPokemon) x.Clone()).ToList();
        return t;
    }
}

[DebuggerDisplay("{Species} {Level}")]
public class TrainerPokemon : ICloneable
{
    public SpeciesResolver Species;
    public int Level;
    public int? Form;
    public string? Name;
    public List<MoveResolver>? Moves;
    public AbilityResolver? Ability;
    public int? AbilityIndex;
    public ItemResolver? Item;
    public int? Gender;
    public string? Nature;
    public Stats? IVs;
    public Stats? EVs;
    public int? Happiness;
    public bool? Shiny;
    public bool? SuperShiny;
    public bool? Shadow;
    public ItemResolver? Ball;

    private TrainerPokemon() { }

    public TrainerPokemon(List<(string Key, string Value)> Data)
    {
        if (Data[0].Key != "Pokemon") throw new Exception("Invalid trainer pokemon data.");
        string[] split = Data[0].Value.Split(',');
        this.Species = (SpeciesResolver) split[0].Trim();
        this.Level = Convert.ToInt32(split[1].Trim());
        for (int i = 1; i < Data.Count; i++)
        {
            switch (Data[i].Key)
            {
                case "Form":
                    this.Form = Convert.ToInt32(Data[i].Value);
                    break;
                case "Name":
                    this.Name = Data[i].Value;
                    break;
                case "Moves":
                    this.Moves = Data[i].Value.Split(',').Select(x => (MoveResolver) x.Trim()).ToList();
                    break;
                case "Ability":
                    this.Ability = (AbilityResolver) Data[i].Value;
                    break;
                case "AbilityIndex":
                    this.AbilityIndex = Convert.ToInt32(Data[i].Value);
                    break;
                case "Item":
                    this.Item = (ItemResolver) Data[i].Value;
                    break;
                case "Gender":
                    this.Gender = Data[i].Value.ToLower() switch
                    {
                        "male" or "m" or "0" => 0,
                        "female" or "f" or "1" => 1,
                        _ => 2
                    };
                    break;
                case "Nature":
                    this.Nature = Game.Data.HardcodedData.Assert(Data[i].Value, Game.Data.HardcodedData.Natures);
                    break;
                case "IV":
                    this.IVs = new Stats(Data[i].Value.Split(',').Select(x => Convert.ToInt32(x.Trim())).ToList());
                    break;
                case "EV":
                    this.EVs = new Stats(Data[i].Value.Split(',').Select(x => Convert.ToInt32(x.Trim())).ToList());
                    break;
                case "Happiness":
                    this.Happiness = Convert.ToInt32(Data[i].Value);
                    break;
                case "Shiny":
                    this.Shiny = Data[i].Value.ToLower() switch
                    {
                        "true" or "t" or "yes" or "y" => true,
                        _ => false
                    };
                    break;
                case "SuperShiny":
                    this.SuperShiny = Data[i].Value.ToLower() switch
                    {
                        "true" or "t" or "yes" or "y" => true,
                        _ => false
                    };
                    break;
                case "Shadow":
                    this.Shadow = Data[i].Value.ToLower() switch
                    {
                        "true" or "t" or "yes" or "y" => true,
                        _ => false
                    };
                    break;
                case "Ball":
                    this.Ball = (ItemResolver) Data[i].Value;
                    break;
                default:
                    throw new Exception($"Invalid key in trainer pokemon definition: '{Data[i].Key}'.");
            }
        }
    }

    public TrainerPokemon(nint Hash)
    {
        this.Species = (SpeciesResolver) Ruby.Symbol.FromPtr(Ruby.Hash.Get(Hash, Ruby.Symbol.ToPtr("species")));
        this.Level = (int) Ruby.Integer.FromPtr(Ruby.Hash.Get(Hash, Ruby.Symbol.ToPtr("level")));
        nint rform = Ruby.Hash.Get(Hash, Ruby.Symbol.ToPtr("form"));
        if (rform != Ruby.Nil) this.Form = (int) Ruby.Integer.FromPtr(rform);
        nint rname = Ruby.Hash.Get(Hash, Ruby.Symbol.ToPtr("name"));
        if (rname != Ruby.Nil) this.Name = Ruby.String.FromPtr(rname);
        nint rmoves = Ruby.Hash.Get(Hash, Ruby.Symbol.ToPtr("moves"));
        if (rmoves != Ruby.Nil)
        {
            int rmoveslength = (int) Ruby.Array.Length(rmoves);
            this.Moves = new List<MoveResolver>();
            for (int i = 0; i < rmoveslength; i++)
            {
                this.Moves.Add((MoveResolver) Ruby.Symbol.FromPtr(Ruby.Array.Get(rmoves, i)));
            }
        }
        nint rability = Ruby.Hash.Get(Hash, Ruby.Symbol.ToPtr("ability"));
        if (rability != Ruby.Nil) this.Ability = (AbilityResolver) Ruby.Symbol.FromPtr(rability);
        nint rabilityindex = Ruby.Hash.Get(Hash, Ruby.Symbol.ToPtr("ability_index"));
        if (rabilityindex != Ruby.Nil) this.AbilityIndex = (int) Ruby.Integer.FromPtr(rabilityindex);
        nint ritem = Ruby.Hash.Get(Hash, Ruby.Symbol.ToPtr("item"));
        if (ritem != Ruby.Nil) this.Item = (ItemResolver) Ruby.Symbol.FromPtr(ritem);
        nint rgender = Ruby.Hash.Get(Hash, Ruby.Symbol.ToPtr("gender"));
        if (rgender != Ruby.Nil) this.Gender = (int) Ruby.Integer.FromPtr(rgender);
        nint rnature = Ruby.Hash.Get(Hash, Ruby.Symbol.ToPtr("nature"));
        if (rnature != Ruby.Nil) this.Nature = Data.HardcodedData.Assert(Ruby.Symbol.FromPtr(rnature), Data.HardcodedData.Natures);
        nint rivs = Ruby.Hash.Get(Hash, Ruby.Symbol.ToPtr("iv"));
        if (rivs != Ruby.Nil) this.IVs = new Stats(rivs);
        nint revs = Ruby.Hash.Get(Hash, Ruby.Symbol.ToPtr("ev"));
        if (revs != Ruby.Nil) this.EVs = new Stats(revs);
        nint rhappiness = Ruby.Hash.Get(Hash, Ruby.Symbol.ToPtr("happiness"));
        if (rhappiness != Ruby.Nil) this.Happiness = (int) Ruby.Integer.FromPtr(rhappiness);
        nint rshiny = Ruby.Hash.Get(Hash, Ruby.Symbol.ToPtr("shininess"));
        if (rshiny != Ruby.Nil) this.Shiny = rshiny == Ruby.True;
        nint rsupershiny = Ruby.Hash.Get(Hash, Ruby.Symbol.ToPtr("super_shininess"));
        if (rsupershiny != Ruby.Nil) this.SuperShiny = rsupershiny == Ruby.True;
        nint rshadow = Ruby.Hash.Get(Hash, Ruby.Symbol.ToPtr("shadowness"));
        if (rshadow != Ruby.Nil) this.Shadow = rshadow == Ruby.True;
        nint rball = Ruby.Hash.Get(Hash, Ruby.Symbol.ToPtr("poke_ball"));
        if (rball != Ruby.Nil) this.Ball = (ItemResolver) Ruby.Symbol.FromPtr(rball);
    }

    public nint Save()
    {
        nint e = Ruby.Hash.Create();
        Ruby.Pin(e);
        Ruby.Hash.Set(e, Ruby.Symbol.ToPtr("species"), Ruby.Symbol.ToPtr(this.Species));
        Ruby.Hash.Set(e, Ruby.Symbol.ToPtr("level"), Ruby.Integer.ToPtr(this.Level));
        if (this.Form != null) Ruby.Hash.Set(e, Ruby.Symbol.ToPtr("form"), Ruby.Integer.ToPtr((int) this.Form));
        if (this.Name != null) Ruby.Hash.Set(e, Ruby.Symbol.ToPtr("name"), Ruby.String.ToPtr(this.Name));
        if (this.Moves != null)
        {
            nint MovesArray = Ruby.Array.Create();
            Ruby.Hash.Set(e, Ruby.Symbol.ToPtr("moves"), MovesArray);
            foreach (string Move in Moves)
            {
                Ruby.Array.Push(MovesArray, Ruby.Symbol.ToPtr(Move));
            }
        }
        if (this.Ability != null) Ruby.Hash.Set(e, Ruby.Symbol.ToPtr("ability"), Ruby.Symbol.ToPtr(this.Ability));
        if (this.AbilityIndex != null) Ruby.Hash.Set(e, Ruby.Symbol.ToPtr("ability_index"), Ruby.Integer.ToPtr((int) this.AbilityIndex));
        if (this.Item != null) Ruby.Hash.Set(e, Ruby.Symbol.ToPtr("item"), Ruby.Symbol.ToPtr(this.Item));
        if (this.Gender != null) Ruby.Hash.Set(e, Ruby.Symbol.ToPtr("gender"), Ruby.Integer.ToPtr((int) this.Gender));
        if (this.Nature != null) Ruby.Hash.Set(e, Ruby.Symbol.ToPtr("nature"), Ruby.Symbol.ToPtr(this.Nature));
        if (this.IVs != null) Ruby.Hash.Set(e, Ruby.Symbol.ToPtr("iv"), this.IVs.Value.Save());
        if (this.EVs != null) Ruby.Hash.Set(e, Ruby.Symbol.ToPtr("ev"), this.EVs.Value.Save());
        if (this.Happiness != null) Ruby.Hash.Set(e, Ruby.Symbol.ToPtr("happiness"), Ruby.Integer.ToPtr((int) this.Happiness));
        if (this.Shiny != null) Ruby.Hash.Set(e, Ruby.Symbol.ToPtr("shininess"), this.Shiny == true ? Ruby.True : Ruby.False);
        if (this.SuperShiny != null) Ruby.Hash.Set(e, Ruby.Symbol.ToPtr("super_shininess"), this.SuperShiny == true ? Ruby.True : Ruby.False);
        if (this.Shadow != null) Ruby.Hash.Set(e, Ruby.Symbol.ToPtr("shadowness"), this.Shadow == true ? Ruby.True : Ruby.False);
        if (this.Ball != null) Ruby.Hash.Set(e, Ruby.Symbol.ToPtr("poke_ball"), Ruby.Symbol.ToPtr(this.Ball));
        Ruby.Unpin(e);
        return e;
    }

    public object Clone()
    {
        TrainerPokemon t = new TrainerPokemon();
        t.Species = (SpeciesResolver) this.Species.ID;
        t.Level = this.Level;
        t.Form = this.Form;
        t.Name = this.Name;
        t.Moves = this.Moves.Select(x => (MoveResolver) x.ID).ToList();
        t.Ability = this.Ability;
        t.AbilityIndex = this.AbilityIndex;
        if (this.Item != null) t.Item = (ItemResolver) this.Item.ID;
        t.Gender = this.Gender;
        t.Nature = this.Nature;
        if (this.IVs.HasValue) t.IVs = (Stats) this.IVs.Value.Clone();
        if (this.EVs.HasValue) t.EVs = (Stats) this.EVs.Value.Clone();
        t.Happiness = this.Happiness;
        t.Shiny = this.Shiny;
        t.SuperShiny = this.SuperShiny;
        t.Shadow = this.Shadow;
        if (this.Ball != null) t.Ball = (ItemResolver) this.Ball.ID;
        return t;
    }
}