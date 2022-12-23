using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Game;

public class Evolution
{
    public SpeciesResolver Species;
    public EvolutionType Type;
    public List<object> Parameters;
    public bool Prevolution;

    public Evolution(nint Array)
    {
        this.Species = (SpeciesResolver) Ruby.Symbol.FromPtr(Ruby.Array.Get(Array, 0));
        this.Type = Ruby.Symbol.FromPtr(Ruby.Array.Get(Array, 1)) switch
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
        this.Parameters = new List<object>();
        for (int i = 2; i < Ruby.Array.Length(Array) - 1; i++)
        {
            nint robj = Ruby.Array.Get(Array, i);
            object nobj = Utilities.RubyToNative(robj);
            this.Parameters.Add(nobj);
        }
        this.Prevolution = Ruby.Array.Get(Array, (int) Ruby.Array.Length(Array) - 1) == Ruby.True;
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