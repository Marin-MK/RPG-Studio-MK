using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK;

[Serializable]
public class ProjectSettings
{
    public Dictionary<string, object> RawData;

    public ProjectSettings()
    {
        RawData = new Dictionary<string, object>();
        RawData.Add("SAVED_VERSION", "1.0.0");
        RawData.Add("PROJECT_NAME", "Untitled Game");
        RawData.Add("LAST_MODE", EditorMode.Mapping);
        RawData.Add("LAST_MAPPING_SUBMODE", MapMode.Tiles);
        RawData.Add("LAST_DATABASE_SUBMODE", DatabaseMode.Tilesets);
        RawData.Add("LAST_MAP_ID", 1);
        RawData.Add("LAST_LAYER", 1);
        RawData.Add("LAST_ZOOM_FACTOR", 1d);
        RawData.Add("TILESET_CAPACITY", 25);
        RawData.Add("AUTOTILE_CAPACITY", 25);
        RawData.Add("EVENT_GRAPHIC_VIEW_MODE", EventGraphicViewMode.BoxAndGraphic);
        RawData.Add("SHOW_EVENT_BOXES_IN_TILES", false);
    }

    /// <summary>
    /// Perform any version upgrades or updates here.
    /// </summary>
    public void Update()
    {
        
    }

    /// <summary>
    /// The last-used version of the editor to save the project. Can be used to programmatically port old data formats to new formats upon an update.
    /// </summary>
    public string SavedVersion
    {
        get => (string) RawData["SAVED_VERSION"];
        set => RawData["SAVED_VERSION"] = value;
    }
    /// <summary>
    /// The name of the project.
    /// </summary>
    public string ProjectName
    {
        get => (string) RawData["PROJECT_NAME"];
        set => RawData["PROJECT_NAME"] = value;
    }
    /// <summary>
    /// The last-active mode of the project.
    /// </summary>
    public EditorMode LastMode
    {
        get => (EditorMode) RawData["LAST_MODE"];
        set => RawData["LAST_MODE"] = value;
    }
    /// <summary>
    /// The last-active submode within the Mapping mode.
    /// </summary>
    public MapMode LastMappingSubmode
    {
        get => (MapMode) RawData["LAST_MAPPING_SUBMODE"];
        set => RawData["LAST_MAPPING_SUBMODE"] = value;
    }
    /// <summary>
    /// The last-active submode within the Database mode.
    /// </summary>
    public DatabaseMode LastDatabaseSubmode
    {
        get => (DatabaseMode) RawData["LAST_DATABASE_SUBMODE"];
        set => RawData["LAST_DATABASE_SUBMODE"] = value;
    }
    /// <summary>
    /// The last selected Map.
    /// </summary>
    public int LastMapID
    {
        get => (int) RawData["LAST_MAP_ID"];
        set => RawData["LAST_MAP_ID"] = value;
    }
    /// <summary>
    /// The last selected layer.
    /// </summary>
    public int LastLayer
    {
        get => (int) RawData["LAST_LAYER"];
        set => RawData["LAST_LAYER"] = value;
    }
    /// <summary>
    /// The last zoom factor.
    /// </summary>
    public double LastZoomFactor
    {
        get => (double) RawData["LAST_ZOOM_FACTOR"];
        set => RawData["LAST_ZOOM_FACTOR"] = value;
    }
    /// <summary>
    /// The maximum tileset capacity.
    /// </summary>
    public int TilesetCapacity
    {
        get => (int) RawData["TILESET_CAPACITY"];
        set => RawData["TILESET_CAPACITY"] = value;
    }
    /// <summary>
    /// The maximum autotile capacity.
    /// </summary>
    public int AutotileCapacity
    {
        get => (int) RawData["AUTOTILE_CAPACITY"];
        set => RawData["AUTOTILE_CAPACITY"] = value;
    }

    /// <summary>
    /// The view mode of event graphics.
    /// </summary>
    public EventGraphicViewMode EventGraphicViewMode
    {
        get => (EventGraphicViewMode) RawData["EVENT_GRAPHIC_VIEW_MODE"];
        set => RawData["EVENT_GRAPHIC_VIEW_MODE"] = value;
    }

    /// <summary>
    /// Whether to show event boxes in the TIles submode
    /// </summary>
    public bool ShowEventBoxesInTilesSubmode
    {
        get => (bool) RawData["SHOW_EVENT_BOXES_IN_TILES"];
        set => RawData["SHOW_EVENT_BOXES_IN_TILES"] = value;
    }
}