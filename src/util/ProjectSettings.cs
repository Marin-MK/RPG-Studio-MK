﻿using System.Collections.Generic;

namespace RPGStudioMK.Utility;

public class ProjectSettings : BaseSettings
{
    public override Dictionary<string, object> Schema => new Dictionary<string, object>()
    {
        { "SAVED_VERSION", "1.0.0" },
        { "PROJECT_NAME", "Untitled Game" },
        { "PROJECT_VERSION", "1.0.0" },
        { "LAST_EXPORT_LOCATION", "" },
        { "LAST_MODE", EditorMode.Mapping },
        { "LAST_MAPPING_SUBMODE", MapMode.Tiles },
        { "LAST_DATABASE_SUBMODE", DatabaseMode.Species },
        { "LAST_MAP_ID", 1 },
        { "LAST_LAYER", 1 },
        { "LAST_ZOOM_FACTOR", 1d },
        { "TILESET_CAPACITY", 25 },
        { "AUTOTILE_CAPACITY", 25 },
        { "EVENT_GRAPHIC_VIEW_MODE", EventGraphicViewMode.BoxAndGraphic },
        { "SHOW_EVENT_BOXES_IN_TILES", false },
        { "HIDDEN_SPECIES_FORMS", new List<string>() },
        { "DATABASE_COLLAPSED_CONTAINERS", new List<string>() },
        { "LAST_SPECIES_ID", "" },
        { "LAST_MOVE_ID", "" },
        { "LAST_ABILITY_ID", "" },
        { "LAST_ITEM_ID", "" },
        { "LAST_TM_ID", "" },
        { "LAST_TYPE_ID", "" },
        { "LAST_SPECIES_SCROLL", 0 },
        { "LAST_SPECIES_SUBMODE", 0 },
        { "LAST_MOVE_SCROLL", 0 },
        { "LAST_ABILITY_SCROLL", 0 },
        { "LAST_ITEM_SCROLL", 0 },
        { "LAST_TM_SCROLL", 0 },
        { "LAST_TYPE_SCROLL", 0 }
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
    /// The version of the project.
    /// </summary>
    public string ProjectVersion
    {
        get => Get<string>("PROJECT_VERSION");
        set => Set("PROJECT_VERSION", value);
    }
    /// <summary>
    /// The last-used location to export the project to.
    /// </summary>
    public string LastExportLocation
    {
        get => Get<string>("LAST_EXPORT_LOCATION");
        set => Set("LAST_EXPORT_LOCATION", value);
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
    /// Whether to show event boxes in the Tiles submode.
    /// </summary>
    public bool ShowEventBoxesInTilesSubmode
    {
        get => Get<bool>("SHOW_EVENT_BOXES_IN_TILES");
        set => Set("SHOW_EVENT_BOXES_IN_TILES", value);
    }

    /// <summary>
    /// A list of species IDs that will be hidden on initializing the species list.
    /// </summary>
    public List<string> HiddenSpeciesForms
    {
        get => Get<List<string>>("HIDDEN_SPECIES_FORMS");
        set => Set("HIDDEN_SPECIES_FORMS", value);
    }

    /// <summary>
    /// A list of container IDs that have been collapsed.
    /// </summary>
    public List<string> DatabaseCollapsedContainers
    {
        get => Get<List<string>>("DATABASE_COLLAPSED_CONTAINERS");
        set => Set("DATABASE_COLLAPSED_CONTAINERS", value);
    }

    /// <summary>
    /// The last species that was opened.
    /// </summary>
    public string LastSpeciesID
    {
        get => Get<string>("LAST_SPECIES_ID");
        set => Set("LAST_SPECIES_ID", value);
    }

	/// <summary>
	/// The last move that was opened.
	/// </summary>
	public string LastMoveID
	{
		get => Get<string>("LAST_MOVE_ID");
		set => Set("LAST_MOVE_ID", value);
	}

    /// <summary>
    /// The last ability that was opened.
    /// </summary>
    public string LastAbilityID
    {
        get => Get<string>("LAST_ABILITY_ID");
        set => Set("LAST_ABILITY_ID", value);
    }

    /// <summary>
    /// The last item that was opened.
    /// </summary>
    public string LastItemID
    {
        get => Get<string>("LAST_ITEM_ID");
        set => Set("LAST_ITEM_ID", value);
    }

    /// <summary>
    /// The last TM that was opened.
    /// </summary>
    public string LastTMID
    {
        get => Get<string>("LAST_TM_ID");
        set => Set("LAST_TM_ID", value);
    }

    /// <summary>
    /// The last Type that was opened.
    /// </summary>
    public string LastTypeID
    {
        get => Get<string>("LAST_TYPE_ID");
        set => Set("LAST_TYPE_ID", value);
    }

	/// <summary>
	/// The last scroll amount in the species section of the database.
	/// </summary>
	public int LastSpeciesScroll
	{
		get => Get<int>("LAST_SPECIES_SCROLL");
		set => Set("LAST_SPECIES_SCROLL", value);
	}

    /// <summary>
    /// The last selected tab in the Species submode of the database.
    /// </summary>
    public int LastSpeciesSubmode
    {
        get => Get<int>("LAST_SPECIES_SUBMODE");
        set => Set("LAST_SPECIES_SUBMODE", value);
    }

	/// <summary>
	/// The last scroll amount in the move section of the database.
	/// </summary>
	public int LastMoveScroll
    {
        get => Get<int>("LAST_MOVE_SCROLL");
        set => Set("LAST_MOVE_SCROLL", value);
    }

    /// <summary>
    /// The last scroll amount in the ability section of the database.
    /// </summary>
    public int LastAbilityScroll
    {
        get => Get<int>("LAST_ABILITY_SCROLL");
        set => Set("LAST_ABILITY_SCROLL", value);
    }

    /// <summary>
    /// The last scroll amount in the item section of the database.
    /// </summary>
    public int LastItemScroll
    {
        get => Get<int>("LAST_ITEM_SCROLL");
        set => Set("LAST_ITEM_SCROLL", value);
    }

    /// <summary>
    /// The last scroll amount in the TM section of the database.
    /// </summary>
    public int LastTMScroll
    {
        get => Get<int>("LAST_TM_SCROLL");
        set => Set("LAST_TM_SCROLL", value);
    }

    /// <summary>
    /// The last scroll amount in the Types section of the database.
    /// </summary>
    public int LastTypeScroll
    {
        get => Get<int>("LAST_TYPE_SCROLL");
        set => Set("LAST_TYPE_SCROLL", value);
    }
}