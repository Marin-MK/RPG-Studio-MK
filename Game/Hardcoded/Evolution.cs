using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Game;

public class Evolution : ICloneable
{
    public SpeciesResolver Species;
    public EvolutionType Type;
    public string? UnknownType;
    public object Parameter;
    public bool Prevolution;

    private Evolution() { }

    public Evolution(SpeciesResolver Species, EvolutionType Type, object Parameter, bool Prevolution = false)
    {
        this.Species = Species;
        this.Type = Type;
        this.Parameter = Parameter;
        this.Prevolution = Prevolution;
    }

    public Evolution(nint Array)
    {
        this.Species = (SpeciesResolver) Ruby.Symbol.FromPtr(Ruby.Array.Get(Array, 0));
        string rtype = Ruby.Symbol.FromPtr(Ruby.Array.Get(Array, 1));
        this.Type = MethodStrToEnum(rtype);
        if (this.Type == EvolutionType.None && !string.IsNullOrEmpty(rtype)) UnknownType = rtype;
        this.Parameter = Utilities.RubyToNative(Ruby.Array.Get(Array, 2));
        this.Prevolution = Ruby.Array.Get(Array, (int) Ruby.Array.Length(Array) - 1) == Ruby.True;
    }

    public nint Save()
    {
        nint e = Ruby.Array.Create();
        Ruby.Pin(e);
        Ruby.Array.Push(e, Ruby.Symbol.ToPtr(Species));
        string rtype = this.Type switch
        {
            EvolutionType.Level => "Level",
            EvolutionType.LevelMale => "LevelMale",
            EvolutionType.LevelFemale => "LevelFemale",
            EvolutionType.LevelDay => "LevelDay",
            EvolutionType.LevelNight => "LevelNight",
            EvolutionType.LevelMorning => "LevelMorning",
            EvolutionType.LevelAfternoon => "LevelAfternoon",
            EvolutionType.LevelEvening => "LevelEvening",
            EvolutionType.LevelNoWeather => "LevelNoWeather",
            EvolutionType.LevelSun => "LevelSun",
            EvolutionType.LevelRain => "LevelRain",
            EvolutionType.LevelSnow => "LevelSnow",
            EvolutionType.LevelSandstorm => "LevelSandstorm",
            EvolutionType.LevelCycling => "LevelCycling",
            EvolutionType.LevelSurfing => "LevelSurfing",
            EvolutionType.LevelDiving => "LevelDiving",
            EvolutionType.LevelDarkness => "LevelDarkness",
            EvolutionType.LevelDarkInParty => "LevelDarkInParty",
            EvolutionType.AttackGreaterThanDefense => "AttackGreater",
            EvolutionType.AttackDefenseEqual => "AtkDefEqual",
            EvolutionType.DefenseGreaterThanAttack => "DefenseGreater",
            EvolutionType.Silcoon => "Silcoon",
            EvolutionType.Cascoon => "Cascoon",
            EvolutionType.Ninjask => "Ninjask",
            EvolutionType.Shedinja => "Shedinja",
            EvolutionType.Happiness => "Happiness",
            EvolutionType.HappinessMale => "HappinessMale",
            EvolutionType.HappinessFemale => "HappinessFemale",
            EvolutionType.HappinessDay => "HappinessDay",
            EvolutionType.HappinessNight => "HappinessNight",
            EvolutionType.HappinessMove => "HappinessMove",
            EvolutionType.HappinessMoveType => "HappinessMoveType",
            EvolutionType.HappienssHoldItem => "HappienssHoldItem",
            EvolutionType.MaxHappiness => "MaxHappiness",
            EvolutionType.Beauty => "Beauty",
            EvolutionType.HoldItem => "HoldItem",
            EvolutionType.HoldItemMale => "HoldItemMale",
            EvolutionType.HoldItemFemale => "HoldItemFemale",
            EvolutionType.DayHoldItem => "DayHoldItem",
            EvolutionType.NightHoldItem => "NightHoldItem",
            EvolutionType.HoldItemHappiness => "HoldItemHappiness",
            EvolutionType.HasMove => "HasMove",
            EvolutionType.HasMoveType => "HasMoveType",
            EvolutionType.HasInParty => "HasInParty",
            EvolutionType.Location => "Location",
            EvolutionType.LocationFlag => "LocationFlag",
            EvolutionType.Region => "Region",
            EvolutionType.Item => "Item",
            EvolutionType.ItemMale => "ItemMale",
            EvolutionType.ItemFemale => "ItemFemale",
            EvolutionType.ItemDay => "ItemDay",
            EvolutionType.ItemNight => "ItemNight",
            EvolutionType.ItemHappiness => "ItemHappiness",
            EvolutionType.Trade => "Trade",
            EvolutionType.TradeMale => "TradeMale",
            EvolutionType.TradeFemale => "TradeFemale",
            EvolutionType.TradeDay => "TradeDay",
            EvolutionType.TradeNight => "TradeNight",
            EvolutionType.TradeItem => "TradeItem",
            EvolutionType.TradeSpecies => "TradeSpecies",
            EvolutionType.BattleDealCriticalHit => "BattleDealCriticalHit",
            EvolutionType.Event => "Event",
            EvolutionType.EventAfterDamageTaken => "EventAfterDamageTake",
            EvolutionType.None => UnknownType ?? "None",
            _ => throw new Exception($"Invalid evolution type '{this.Type}'.")
        };
        Ruby.Array.Push(e, Ruby.Symbol.ToPtr(rtype));
        Ruby.Array.Push(e, Utilities.NativeToRuby(this.Parameter));
        Ruby.Array.Push(e, Prevolution ? Ruby.True : Ruby.False);
        Ruby.Unpin(e);
        return e;
    }

    public static EvolutionType MethodStrToEnum(string type)
    {
        return type switch
        {
            "Level" => EvolutionType.Level,
            "LevelMale" => EvolutionType.LevelMale,
            "LevelFemale" => EvolutionType.LevelFemale,
            "LevelDay" => EvolutionType.LevelDay,
            "LevelNight" => EvolutionType.LevelNight,
            "LevelMorning" => EvolutionType.LevelMorning,
            "LevelAfternoon" => EvolutionType.LevelAfternoon,
            "LevelEvening" => EvolutionType.LevelEvening,
            "LevelNoWeather" => EvolutionType.LevelNoWeather,
            "LevelSun" => EvolutionType.LevelSun,
            "LevelRain" => EvolutionType.LevelRain,
            "LevelSnow" => EvolutionType.LevelSnow,
            "LevelSandstorm" => EvolutionType.LevelSandstorm,
            "LevelCycling" => EvolutionType.LevelCycling,
            "LevelSurfing" => EvolutionType.LevelSurfing,
            "LevelDiving" => EvolutionType.LevelDiving,
            "LevelDarkness" => EvolutionType.LevelDarkness,
            "LevelDarkInParty" => EvolutionType.LevelDarkInParty,
            "AttackGreater" => EvolutionType.AttackGreaterThanDefense,
            "AtkDefEqual" => EvolutionType.AttackDefenseEqual,
            "DefenseGreater" => EvolutionType.DefenseGreaterThanAttack,
            "Silcoon" => EvolutionType.Silcoon,
            "Cascoon" => EvolutionType.Cascoon,
            "Ninjask" => EvolutionType.Ninjask,
            "Shedinja" => EvolutionType.Shedinja,
            "Happiness" => EvolutionType.Happiness,
            "HappinessMale" => EvolutionType.HappinessMale,
            "HappinessFemale" => EvolutionType.HappinessFemale,
            "HappinessDay" => EvolutionType.HappinessDay,
            "HappinessNight" => EvolutionType.HappinessNight,
            "HappinessMove" => EvolutionType.HappinessMove,
            "HappinessMoveType" => EvolutionType.HappinessMoveType,
            "HappienssHoldItem" => EvolutionType.HappienssHoldItem,
            "MaxHappiness" => EvolutionType.MaxHappiness,
            "Beauty" => EvolutionType.Beauty,
            "HoldItem" => EvolutionType.HoldItem,
            "HoldItemMale" => EvolutionType.HoldItemMale,
            "HoldItemFemale" => EvolutionType.HoldItemFemale,
            "DayHoldItem" => EvolutionType.DayHoldItem,
            "NightHoldItem" => EvolutionType.NightHoldItem,
            "HoldItemHappiness" => EvolutionType.HoldItemHappiness,
            "HasMove" => EvolutionType.HasMove,
            "HasMoveType" => EvolutionType.HasMoveType,
            "HasInParty" => EvolutionType.HasInParty,
            "Location" => EvolutionType.Location,
            "LocationFlag" => EvolutionType.LocationFlag,
            "Region" => EvolutionType.Region,
            "Item" => EvolutionType.Item,
            "ItemMale" => EvolutionType.ItemMale,
            "ItemFemale" => EvolutionType.ItemFemale,
            "ItemDay" => EvolutionType.ItemDay,
            "ItemNight" => EvolutionType.ItemNight,
            "ItemHappiness" => EvolutionType.ItemHappiness,
            "Trade" => EvolutionType.Trade,
            "TradeMale" => EvolutionType.TradeMale,
            "TradeFemale" => EvolutionType.TradeFemale,
            "TradeDay" => EvolutionType.TradeDay,
            "TradeNight" => EvolutionType.TradeNight,
            "TradeItem" => EvolutionType.TradeItem,
            "TradeSpecies" => EvolutionType.TradeSpecies,
            "BattleDealCriticalHit" => EvolutionType.BattleDealCriticalHit,
            "Event" => EvolutionType.Event,
            "EventAfterDamageTake" => EvolutionType.EventAfterDamageTaken,
            _ => EvolutionType.None
        };
    }

    public object Clone()
    {
        Evolution e = new Evolution();
        e.Species = (SpeciesResolver) this.Species.ID;
        e.Type = this.Type;
        e.Parameter = this.Parameter; // Will realistically always be a primitive data type
        e.Prevolution = this.Prevolution;
        return e;
    }
}

public enum EvolutionType
{
    None,
    Level,
    LevelMale,
    LevelFemale,
    LevelDay,
    LevelNight,
    LevelMorning,
    LevelAfternoon,
    LevelEvening,
    LevelNoWeather,
    LevelSun,
    LevelRain,
    LevelSnow,
    LevelSandstorm,
    LevelCycling,
    LevelSurfing,
    LevelDiving,
    LevelDarkness,
    LevelDarkInParty,
    AttackGreaterThanDefense,
    AttackDefenseEqual,
    DefenseGreaterThanAttack,
    Silcoon,
    Cascoon,
    Ninjask,
    Shedinja,
    Happiness,
    HappinessMale,
    HappinessFemale,
    HappinessDay,
    HappinessNight,
    HappinessMove,
    HappinessMoveType,
    HappienssHoldItem,
    MaxHappiness,
    Beauty,
    HoldItem,
    HoldItemMale,
    HoldItemFemale,
    DayHoldItem,
    NightHoldItem,
    HoldItemHappiness,
    HasMove,
    HasMoveType,
    HasInParty,
    Location,
    LocationFlag,
    Region,
    Item,
    ItemMale,
    ItemFemale,
    ItemDay,
    ItemNight,
    ItemHappiness,
    Trade,
    TradeMale,
    TradeFemale,
    TradeDay,
    TradeNight,
    TradeItem,
    TradeSpecies,
    BattleDealCriticalHit,
    Event,
    EventAfterDamageTaken
}