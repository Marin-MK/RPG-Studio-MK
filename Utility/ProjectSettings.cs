using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Utility;

public class ProjectSettings : BaseSettings
{
    public override Dictionary<string, object> Schema => new Dictionary<string, object>()
    {
        { "SAVED_VERSION", "1.0.0" },
        { "PROJECT_NAME", "Untitled Game" },
        { "LAST_MODE", EditorMode.Mapping },
        { "LAST_MAPPING_SUBMODE", MapMode.Tiles },
        { "LAST_DATABASE_SUBMODE", DatabaseMode.Tilesets },
        { "LAST_MAP_ID", 1 },
        { "LAST_LAYER", 1 },
        { "LAST_ZOOM_FACTOR", 1d },
        { "TILESET_CAPACITY", 25 },
        { "AUTOTILE_CAPACITY", 25 },
        { "EVENT_GRAPHIC_VIEW_MODE", EventGraphicViewMode.BoxAndGraphic },
        { "SHOW_EVENT_BOXES_IN_TILES", false }
    };

    public ProjectSettings() : base() { }

    public ProjectSettings(Dictionary<string, object> dict) : base(dict) { }

    /// <summary>
    /// The last-used version of the editor to save the project. Can be used to programmatically port old data formats to new formats upon an update.
    /// </summary>
    public string SavedVersion
    {
        get => Get<string>("SAVED_VERSION");
        set => Set("SAVED_VERSION", value);
    }
    /// <summary>
    /// The name of the project.
    /// </summary>
    public string ProjectName
    {
        get => Get<string>("PROJECT_NAME");
        set => Set("PROJECT_NAME", value);
    }
    /// <summary>
    /// The last-active mode of the project.
    /// </summary>
    public EditorMode LastMode
    {
        get => Get<EditorMode>("LAST_MODE");
        set => Set("LAST_MODE", value);
    }
    /// <summary>
    /// The last-active submode within the Mapping mode.
    /// </summary>
    public MapMode LastMappingSubmode
    {
        get => Get<MapMode>("LAST_MAPPING_SUBMODE");
        set => Set("LAST_MAPPING_SUBMODE", value);
    }
    /// <summary>
    /// The last-active submode within the Database mode.
    /// </summary>
    public DatabaseMode LastDatabaseSubmode
    {
        get => Get<DatabaseMode>("LAST_DATABASE_SUBMODE");
        set => Set("LAST_DATABASE_SUBMODE", value);
    }
    /// <summary>
    /// The last selected Map.
    /// </summary>
    public int LastMapID
    {
        get => Get<int>("LAST_MAP_ID");
        set => Set("LAST_MAP_ID", value);
    }
    /// <summary>
    /// The last selected layer.
    /// </summary>
    public int LastLayer
    {
        get => Get<int>("LAST_LAYER");
        set => Set("LAST_LAYER", value);
    }
    /// <summary>
    /// The last zoom factor.
    /// </summary>
    public double LastZoomFactor
    {
        get => Get<double>("LAST_ZOOM_FACTOR");
        set => Set("LAST_ZOOM_FACTOR", value);
    }
    /// <summary>
    /// The maximum tileset capacity.
    /// </summary>
    public int TilesetCapacity
    {
        get => Get<int>("TILESET_CAPACITY");
        set => Set("TILESET_CAPACITY", value);
    }
    /// <summary>
    /// The maximum autotile capacity.
    /// </summary>
    public int AutotileCapacity
    {
        get => Get<int>("AUTOTILE_CAPACITY");
        set => Set("AUTOTILE_CAPACITY", value);
    }

    /// <summary>
    /// The view mode of event graphics.
    /// </summary>
    public EventGraphicViewMode EventGraphicViewMode
    {
        get => Get<EventGraphicViewMode>("EVENT_GRAPHIC_VIEW_MODE");
        set => Set("EVENT_GRAPHIC_VIEW_MODE", value);
    }

    /// <summary>
    /// Whether to show event boxes in the TIles submode
    /// </summary>
    public bool ShowEventBoxesInTilesSubmode
    {
        get => Get<bool>("SHOW_EVENT_BOXES_IN_TILES");
        set => Set("SHOW_EVENT_BOXES_IN_TILES", value);
    }
}