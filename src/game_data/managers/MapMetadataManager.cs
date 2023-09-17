using RPGStudioMK.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RPGStudioMK.Game;

public class MapMetadataManager : BaseDataManager
{
    public MapMetadataManager() : base("MapMetadata", "map_metadata.dat", "map_metadata.txt", "map metadata") { }

    protected override void LoadData()
    {
        base.LoadData();
        Logger.WriteLine("Loading map metadata");
        LoadAsHash((key, robj) =>
        {
            int ckey = (int) Ruby.Integer.FromPtr(key);
            // Read properties of the map and add the to the corresponding Map object.
            bool GetDefaultTrue(string Variable)
            {
                nint value = Ruby.GetIVar(robj, Variable);
                if (value == Ruby.Nil) return true;
                else return value == Ruby.True;
            }
            bool GetDefaultFalse(string Variable)
            {
                nint value = Ruby.GetIVar(robj, Variable);
                if (value == Ruby.Nil) return false;
                else return value == Ruby.True;
            }
            string GetStrOrNull(string Variable)
            {
                nint value = Ruby.GetIVar(robj, Variable);
                if (value == Ruby.Nil) return null;
                return Ruby.String.FromPtr(value);
            }
            if (!Data.Maps.ContainsKey(ckey)) return;
            Map Map = Data.Maps[ckey];
            Map.InGameName = GetStrOrNull("@real_name");
            Map.OutdoorMap = GetDefaultFalse("@outdoor_map");
            Map.AnnounceLocation = GetDefaultFalse("@announce_location");
            Map.CanBicycle = GetDefaultTrue("@can_bicycle");
            if (Ruby.GetIVar(robj, "@outdoor_map") == Ruby.Nil && Ruby.GetIVar(robj, "@can_bicycle") == Ruby.Nil)
            {
                // Maps default to being indoor maps, but
                // CanBicycle defaults to false on indoor maps, whereas it defaults to true on outdoor maps.
                Map.CanBicycle = false;
            }
            Map.AlwaysBicycle = GetDefaultFalse("@always_bicycle");
            nint tpdest = Ruby.GetIVar(robj, "@teleport_destination");
            if (tpdest != Ruby.Nil)
            {
                int mapid = (int)Ruby.Integer.FromPtr(Ruby.Array.Get(tpdest, 0));
                int mapx = (int)Ruby.Integer.FromPtr(Ruby.Array.Get(tpdest, 1));
                int mapy = (int)Ruby.Integer.FromPtr(Ruby.Array.Get(tpdest, 2));
                Map.HealingSpot = (mapid, mapx, mapy);
            }
            nint weather = Ruby.GetIVar(robj, "@weather");
            if (weather != Ruby.Nil)
            {
                string wtype = Data.HardcodedData.Assert(Ruby.Symbol.FromPtr(Ruby.Array.Get(weather, 0)), Data.HardcodedData.Weathers);
                int wstrength = (int)Ruby.Integer.FromPtr(Ruby.Array.Get(weather, 1));
                Map.Weather = (wtype, wstrength);
            }
            nint townmappos = Ruby.GetIVar(robj, "@town_map_position");
            if (townmappos != Ruby.Nil)
            {
                int regionid = (int)Ruby.Integer.FromPtr(Ruby.Array.Get(townmappos, 0));
                int regionx = (int)Ruby.Integer.FromPtr(Ruby.Array.Get(townmappos, 1));
                int regiony = (int)Ruby.Integer.FromPtr(Ruby.Array.Get(townmappos, 2));
                Map.TownMapPosition = (regionid, regionx, regiony);
            }
            nint rdiveid = Ruby.GetIVar(robj, "@dive_map_id");
            if (rdiveid != Ruby.Nil) Map.DiveMapID = (int)Ruby.Integer.FromPtr(rdiveid);
            Map.DarkMap = GetDefaultFalse("@dark_map");
            Map.SafariMap = GetDefaultFalse("@safari_map");
            Map.SnapEdges = GetDefaultFalse("@snap_edges");
            Map.RandomDungeon = GetDefaultFalse("@random_dungeon");
            Map.BattleBackground = GetStrOrNull("@battle_background");
            Map.WildBattleBGM = GetStrOrNull("@wild_battle_BGM");
            Map.TrainerBattleBGM = GetStrOrNull("@trainer_battle_BGM");
            Map.WildVictoryBGM = GetStrOrNull("@wild_victory_BGM");
            Map.TrainerVictoryBGM = GetStrOrNull("@trainer_victory_BGM");
            Map.WildCaptureME = GetStrOrNull("@wild_capture_ME");
            nint townmapsize = Ruby.GetIVar(robj, "@town_map_size");
            if (townmapsize != Ruby.Nil)
            {
                int sqwidth = (int)Ruby.Integer.FromPtr(Ruby.Array.Get(townmapsize, 0));
                string sqformat = Ruby.String.FromPtr(Ruby.Array.Get(townmapsize, 1));
                Map.TownMapSize = (sqwidth, sqformat);
            }
            Map.BattleEnvironment = Ruby.GetIVar(robj, "@battle_environment") == Ruby.Nil ? null : Ruby.Symbol.FromPtr(Ruby.GetIVar(robj, "@battle_environment"));
            nint FlagsArray = Ruby.GetIVar(robj, "@flags");
            Map.Flags = new List<string>();
            if (FlagsArray != Ruby.Nil)
            {
                int FlagCount = (int) Ruby.Array.Length(FlagsArray);
                for (int j = 0; j < FlagCount; j++)
                {
                    string flag = Ruby.String.FromPtr(Ruby.Array.Get(FlagsArray, j));
                    Map.Flags.Add(flag);
                }
            }
        });
    }

    protected override void LoadPBS()
    {
        if (!Game.Data.IsVersionAtLeast(EssentialsVersion.v20)) return;
        base.LoadPBS();
        Logger.WriteLine("Loading map metadata from PBS");
        FormattedTextParser.ParseSectionBasedFile(PBSFilename, (id, hash) =>
        {
            Map Map = Data.Maps[Convert.ToInt32(id)];
            SetMapMetadata(Map, hash);
        }, Data.SetLoadProgress);
    }

    public static void SetMapMetadata(Map Map, Dictionary<string, string> hash)
    {
        bool GetDefaultTrue(string Name)
        {
            if (hash.ContainsKey(Name))
            {
                switch (hash[Name].ToLower())
                {
                    case "true": case "t": case "yes": case "y":
                        return true;
                    default:
                        return false;
                }
            }
            return true;
        }
        bool GetDefaultFalse(string Name)
        {
            if (hash.ContainsKey(Name))
            {
                switch (hash[Name].ToLower())
                {
                    case "true": case "t": case "yes": case "y":
                        return true;
                    default:
                        return false;
                }
            }
            return false;
        }
        Map.InGameName = hash.ContainsKey("Name") ? hash["Name"] : null;
        Map.OutdoorMap = GetDefaultFalse("Outdoor");
        Map.AnnounceLocation = GetDefaultFalse("ShowArea");
        Map.CanBicycle = GetDefaultTrue("Bicycle");
        if (!hash.ContainsKey("Outdoor") && !hash.ContainsKey("Bicycle"))
        {
            // Default is outdoor false, bicycle true, but that would mean
            // indoor bicycling. If this is the case, turn bicycling off.
            Map.CanBicycle = false;
        }
        Map.AlwaysBicycle = GetDefaultFalse("BicycleAlways");
        if (hash.ContainsKey("HealingSpot"))
        {
            int[] _hspot = hash["HealingSpot"].Split(',').Select(x => Convert.ToInt32(x.Trim())).ToArray();
            Map.HealingSpot = (_hspot[0], _hspot[1], _hspot[2]);
        }
        if (hash.ContainsKey("Weather"))
        {
            string[] _weather = hash["Weather"].Split(',');
            string wtype = Data.HardcodedData.Assert(_weather[0], Data.HardcodedData.Weathers);
            int intensity = Convert.ToInt32(_weather[1]);
            Map.Weather = (wtype, intensity);
        }
        if (hash.ContainsKey("MapPosition"))
        {
            int[] _mappos = hash["MapPosition"].Split(',').Select(x => Convert.ToInt32(x.Trim())).ToArray();
            Map.TownMapPosition = (_mappos[0], _mappos[1], _mappos[2]);
        }
        if (hash.ContainsKey("DiveMap")) Map.DiveMapID = Convert.ToInt32(hash["DiveMap"]);
        Map.DarkMap = GetDefaultFalse("DarkMap");
        Map.SafariMap = GetDefaultFalse("SafariMap");
        Map.SnapEdges = GetDefaultFalse("SnapEdges");
        Map.RandomDungeon = GetDefaultFalse("Dungeon");
        Map.BattleBackground = hash.ContainsKey("BattleBack") ? hash["BattleBack"] : null;
        Map.WildBattleBGM = hash.ContainsKey("WildBattleBGM") ? hash["WildBattleBGM"] : null;
        Map.TrainerBattleBGM = hash.ContainsKey("TrainerBattleBGM") ? hash["TrainerBattleBGM"] : null;
        Map.WildVictoryBGM = hash.ContainsKey("WildVictoryBGM") ? hash["WildVictoryBGM"] : null;
        Map.TrainerVictoryBGM = hash.ContainsKey("TrainerVictoryBGM") ? hash["TrainerVictoryBGM"] : null;
        Map.WildCaptureME = hash.ContainsKey("WildCaptureME") ? hash["WildCaptureME"] : null;
        if (hash.ContainsKey("MapSize"))
        {
            string[] _mapsize = hash["MapSize"].Split(',').Select(x => x.Trim()).ToArray();
            int width = Convert.ToInt32(_mapsize[0]);
            string included = _mapsize[1];
            Map.TownMapSize = (width, included);
        }
        Map.BattleEnvironment = hash.ContainsKey("Environment") ? hash["Environment"] : null;
        if (hash.ContainsKey("Flags")) Map.Flags = hash["Flags"].Split(',').Select(x => x.Trim()).ToList();
        else Map.Flags = new List<string>();
    }

    protected override void SaveData()
    {
        base.SaveData();
        Logger.WriteLine("Saving map metadata");
        SafeSave("map_metadata.dat", File =>
        {
            nint Data = Ruby.Hash.Create();
            Ruby.Pin(Data);
            foreach (Map Map in Game.Data.Maps.Values)
            {
                nint e = Ruby.Funcall(GetClass(), "new");
                Ruby.Hash.Set(Data, Ruby.Integer.ToPtr(Map.ID), e);
                Ruby.SetIVar(e, "@real_name", Map.InGameName == null ? Ruby.Nil : Ruby.String.ToPtr(Map.InGameName));
                Ruby.SetIVar(e, "@outdoor_map", Map.OutdoorMap ? Ruby.True : Ruby.False);
                Ruby.SetIVar(e, "@announce_location", Map.AnnounceLocation ? Ruby.True : Ruby.False);
                Ruby.SetIVar(e, "@can_bicycle", Map.CanBicycle ? Ruby.True : Ruby.False);
                Ruby.SetIVar(e, "@always_bicycle", Map.AlwaysBicycle ? Ruby.True : Ruby.False);
                nint rhspot = Ruby.Nil;
                if (Map.HealingSpot.HasValue)
                {
                    rhspot = Ruby.Array.Create(3);
                    Ruby.Pin(rhspot);
                    Ruby.Array.Set(rhspot, 0, Ruby.Integer.ToPtr(Map.HealingSpot.Value.MapID));
                    Ruby.Array.Set(rhspot, 1, Ruby.Integer.ToPtr(Map.HealingSpot.Value.X));
                    Ruby.Array.Set(rhspot, 2, Ruby.Integer.ToPtr(Map.HealingSpot.Value.Y));
                    Ruby.Unpin(rhspot);
                }
                Ruby.SetIVar(e, "@teleport_destination", rhspot);
                nint rweather = Ruby.Nil;
                if (Map.Weather.HasValue)
                {
                    rweather = Ruby.Array.Create(2);
                    Ruby.Pin(rweather);
                    Ruby.Array.Set(rweather, 0, Ruby.Symbol.ToPtr(Map.Weather.Value.Weather));
                    Ruby.Array.Set(rweather, 1, Ruby.Integer.ToPtr(Map.Weather.Value.Intensity));
                    Ruby.Unpin(rweather);
                }
                Ruby.SetIVar(e, "@weather", rweather);
                nint rtmpos = Ruby.Nil;
                if (Map.TownMapPosition.HasValue)
                {
                    rtmpos = Ruby.Array.Create(3);
                    Ruby.Pin(rtmpos);
                    Ruby.Array.Set(rtmpos, 0, Ruby.Integer.ToPtr(Map.TownMapPosition.Value.Region));
                    Ruby.Array.Set(rtmpos, 1, Ruby.Integer.ToPtr(Map.TownMapPosition.Value.X));
                    Ruby.Array.Set(rtmpos, 2, Ruby.Integer.ToPtr(Map.TownMapPosition.Value.Y));
                    Ruby.Unpin(rtmpos);
                }
                Ruby.SetIVar(e, "@town_map_position", rtmpos);
                Ruby.SetIVar(e, "@dive_map_id", Map.DiveMapID == null ? Ruby.Nil : Ruby.Integer.ToPtr((int) Map.DiveMapID));
                Ruby.SetIVar(e, "@dark_map", Map.DarkMap ? Ruby.True : Ruby.False);
                Ruby.SetIVar(e, "@safari_map", Map.SafariMap ? Ruby.True : Ruby.False);
                Ruby.SetIVar(e, "@snap_edges", Map.SnapEdges ? Ruby.True : Ruby.False);
                Ruby.SetIVar(e, "@random_dungeon", Map.RandomDungeon ? Ruby.True : Ruby.False);
                Ruby.SetIVar(e, "@battle_background", Map.BattleBackground == null ? Ruby.Nil : Ruby.String.ToPtr(Map.BattleBackground));
                Ruby.SetIVar(e, "@wild_battle_BGM", Map.WildBattleBGM == null ? Ruby.Nil : Ruby.String.ToPtr(Map.WildBattleBGM));
                Ruby.SetIVar(e, "@trainer_battle_BGM", Map.TrainerBattleBGM == null ? Ruby.Nil : Ruby.String.ToPtr(Map.TrainerBattleBGM));
                Ruby.SetIVar(e, "@wild_victory_BGM", Map.WildVictoryBGM == null ? Ruby.Nil : Ruby.String.ToPtr(Map.WildVictoryBGM));
                Ruby.SetIVar(e, "@trainer_victory_BGM", Map.TrainerVictoryBGM == null ? Ruby.Nil : Ruby.String.ToPtr(Map.TrainerVictoryBGM));
                Ruby.SetIVar(e, "@wild_capture_ME", Map.WildCaptureME == null ? Ruby.Nil : Ruby.String.ToPtr(Map.WildCaptureME));
                nint rtmsize = Ruby.Nil;
                if (Map.TownMapSize.HasValue)
                {
                    rtmsize = Ruby.Array.Create(2);
                    Ruby.Pin(rtmsize);
                    Ruby.Array.Set(rtmsize, 0, Ruby.Integer.ToPtr(Map.TownMapSize.Value.Width));
                    Ruby.Array.Set(rtmsize, 1, Ruby.String.ToPtr(Map.TownMapSize.Value.Included));
                    Ruby.Unpin(rtmsize);
                }
                Ruby.SetIVar(e, "@town_map_size", rtmsize);
                Ruby.SetIVar(e, "@battle_environment", Map.BattleEnvironment == null ? Ruby.Nil : Ruby.String.ToPtr(Map.BattleEnvironment));
                nint FlagsArray = Ruby.Array.Create();
                Ruby.SetIVar(e, "@flags", FlagsArray);
                foreach (string Flag in Map.Flags) 
                {
                    Ruby.Array.Push(FlagsArray, Ruby.String.ToPtr(Flag));
                }
            }
            Ruby.Marshal.Dump(Data, File);
            Ruby.Unpin(Data);
        });
    }

    public override void Clear()
    {
        base.Clear();
        // Clearing this data happens when all maps are cleared,
        // as this data is part of map data internally.
    }
}
