using System.Collections.Generic;

namespace RPGStudioMK.Utility;

public class GeneralSettings : BaseSettings
{
    public override Dictionary<string, object> Schema => new Dictionary<string, object>()
    {
        { "WAS_MAXIMIZED", true },
        { "LAST_WIDTH", 600 },
        { "LAST_HEIGHT", 600 },
        { "LAST_X", 50 },
        { "LAST_Y", 50 },
        { "RECENT_FILES", new List<List<string>>() },
        { "SHOW_MAP_ANIMATIONS", true },
        { "SHOW_GRID", true },
        { "EXPAND_EVENT_COMMANDS", true },
        { "PREFER_RECTANGLE_FILL", false },
        { "PREFER_ELLIPSE_FILL", false },
        { "PREFER_SELECTION_ALL", false },
        { "SECONDS_USED", 0 },
        { "ANIMATE_SPECIES_ICONS", true },
        { "ANIMATE_EGG_CRACKS", false },
        { "ANIMATE_EGG_ICONS", true },
        { "SHOW_SPECIES_SPRITES_AS_FEMALE", false },
        { "SHOW_SPECIES_SPRITES_AS_SHADOW", false }
    };

    public GeneralSettings() : base() { }

    public GeneralSettings(Dictionary<string, object> dict) : base(dict) { }

    /// <summary>
    /// Whether the editor window was maximized.
    /// </summary>
    public bool WasMaximized
    {
        get => Get<bool>("WAS_MAXIMIZED");
        set => Set("WAS_MAXIMIZED", value);
    }
    /// <summary>
    /// The last width the editor window had when it was not maximized.
    /// </summary>
    public int LastWidth
    {
        get => Get<int>("LAST_WIDTH");
        set => Set("LAST_WIDTH", value);
    }
    /// <summary>
    /// The last height the editor window had when it was not maximized.
    /// </summary>
    public int LastHeight
    {
        get => Get<int>("LAST_HEIGHT");
        set => Set("LAST_HEIGHT", value);
    }
    /// <summary>
    /// The last X position the editor window had when it was not maximized.
    /// </summary>
    public int LastX
    {
        get => Get<int>("LAST_X");
        set => Set("LAST_X", value);
    }
    /// <summary>
    /// The last Y position the editor window had when it was not maximized.
    /// </summary>
    public int LastY
    {
        get => Get<int>("LAST_Y");
        set => Set("LAST_Y", value);
    }
    /// <summary>
    /// The list of recently opened projects. May contain old/invalid paths.
    /// </summary>
    public List<List<string>> RecentFiles
    {
        get => Get<List<List<string>>>("RECENT_FILES");
        set => Set("RECENT_FILES", value);
    }
    /// <summary>
    /// Whether to play map animations.
    /// </summary>
    public bool ShowMapAnimations
    {
        get => Get<bool>("SHOW_MAP_ANIMATIONS");
        set => Set("SHOW_MAP_ANIMATIONS", value);
    }
    /// <summary>
    /// Whether to show the map grid overlay.
    /// </summary>
    public bool ShowGrid
    {
        get => Get<bool>("SHOW_GRID");
        set => Set("SHOW_GRID", value);
    }
    /// <summary>
    /// Whether to expand event commands.
    /// </summary>
    public bool ExpandEventCommands
    {
        get => Get<bool>("EXPAND_EVENT_COMMANDS");
        set => Set("EXPAND_EVENT_COMMANDS", value);
    }
    /// <summary>
    /// Whether the user prefers to use the rectangle tool including filling.
    /// </summary>
    public bool PreferRectangleFill
    {
        get => Get<bool>("PREFER_RECTANGLE_FILL");
        set => Set("PREFER_RECTANGLE_FILL", value);
    }
    /// <summary>
    /// Whether the user prefers to use the ellipse tool including filling.
    /// </summary>
    public bool PreferEllipseFill
    {
        get => Get<bool>("PREFER_ELLIPSE_FILL");
        set => Set("PREFER_ELLIPSE_FILL", value);
    }
    /// <summary>
    /// Whether the user prefers to use the selection tool for selecting multiple layers.
    /// </summary>
    public bool PreferSelectionAll
    {
        get => Get<bool>("PREFER_SELECTION_ALL");
        set => Set("PREFER_SELECTION_ALL", value);
    }
    /// <summary>
    /// The total number of seconds the program has ever been open.
    /// </summary>
    public int SecondsUsed
    {
        get => Get<int>("SECONDS_USED");
        set => Set("SECONDS_USED", value);
	}
	/// <summary>
	/// Whether species icons are animated in the database.
	/// </summary>
	public bool AnimateSpeciesIcons
	{
		get => Get<bool>("ANIMATE_SPECIES_ICONS");
		set => Set("ANIMATE_SPECIES_ICONS", value);
	}
    /// <summary>
    /// Whether egg icons are animated in the database.
    /// </summary>
    public bool AnimateEggIcons
    {
        get => Get<bool>("ANIMATE_EGG_ICONS");
        set => Set("ANIMATE_EGG_ICONS", value);
    }
	/// <summary>
	/// Whether egg cracks are animated in the database.
	/// </summary>
	public bool AnimateEggCracks
	{
		get => Get<bool>("ANIMATE_EGG_CRACKS");
		set => Set("ANIMATE_EGG_CRACKS", value);
	}
	/// <summary>
	/// Shows species sprites as female in the database.
	/// </summary>
	public bool ShowSpeciesSpritesAsFemale
	{
		get => Get<bool>("SHOW_SPECIES_SPRITES_AS_FEMALE");
		set => Set("SHOW_SPECIES_SPRITES_AS_FEMALE", value);
	}
	/// <summary>
	/// Shows species sprites as shadow in the database.
	/// </summary>
	public bool ShowSpeciesSpritesAsShadow
	{
		get => Get<bool>("SHOW_SPECIES_SPRITES_AS_SHADOW");
		set => Set("SHOW_SPECIES_SPRITES_AS_SHADOW", value);
	}
}