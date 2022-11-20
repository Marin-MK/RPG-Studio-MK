using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK;

[Serializable]
public class GeneralSettings
{
    public Dictionary<string, object> RawData;

    public GeneralSettings()
    {
        RawData = new Dictionary<string, object>();
        RawData.Add("WAS_MAXIMIZED", true);
        RawData.Add("LAST_WIDTH", 600);
        RawData.Add("LAST_HEIGHT", 600);
        RawData.Add("LAST_X", 50);
        RawData.Add("LAST_Y", 50);
        RawData.Add("RECENT_FILES", new List<List<string>>());
        RawData.Add("SHOW_MAP_ANIMATIONS", true);
        RawData.Add("SHOW_GRID", true);
        RawData.Add("EXPAND_EVENT_COMMANDS", true);
        RawData.Add("PREFER_RECTANGLE_FILL", false);
        RawData.Add("PREFER_ELLIPSE_FILL", false);
        RawData.Add("PREFER_SELECTION_ALL", false);
        RawData.Add("SECONDS_USED", 0);
    }

    /// <summary>
    /// Perform any version upgrades or updates here.
    /// </summary>
    public void Update()
    {
        if (!RawData.ContainsKey("EXPAND_EVENT_COMMANDS")) RawData.Add("EXPAND_EVENT_COMMANDS", true);
    }

    /// <summary>
    /// Whether the editor window was maximized.
    /// </summary>
    public bool WasMaximized
    {
        get => (bool) RawData["WAS_MAXIMIZED"];
        set => RawData["WAS_MAXIMIZED"] = value;
    }
    /// <summary>
    /// The last width the editor window had when it was not maximized.
    /// </summary>
    public int LastWidth
    {
        get => (int) RawData["LAST_WIDTH"];
        set => RawData["LAST_WIDTH"] = value;
    }
    /// <summary>
    /// The last height the editor window had when it was not maximized.
    /// </summary>
    public int LastHeight
    {
        get => (int) RawData["LAST_HEIGHT"];
        set => RawData["LAST_HEIGHT"] = value;
    }
    /// <summary>
    /// The last X position the editor window had when it was not maximized.
    /// </summary>
    public int LastX
    {
        get => (int) RawData["LAST_X"];
        set => RawData["LAST_X"] = value;
    }
    /// <summary>
    /// The last Y position the editor window had when it was not maximized.
    /// </summary>
    public int LastY
    {
        get => (int) RawData["LAST_Y"];
        set => RawData["LAST_Y"] = value;
    }
    /// <summary>
    /// The list of recently opened projects. May contain old/invalid paths.
    /// </summary>
    public List<List<string>> RecentFiles
    {
        get => ((List<List<string>>) RawData["RECENT_FILES"]);
        set => RawData["RECENT_FILES"] = value;
    }
    /// <summary>
    /// Whether to play map animations.
    /// </summary>
    public bool ShowMapAnimations
    {
        get => (bool) RawData["SHOW_MAP_ANIMATIONS"];
        set => RawData["SHOW_MAP_ANIMATIONS"] = value;
    }
    /// <summary>
    /// Whether to show the map grid overlay.
    /// </summary>
    public bool ShowGrid
    {
        get => (bool) RawData["SHOW_GRID"];
        set => RawData["SHOW_GRID"] = value;
    }
    /// <summary>
    /// Whether to expand event commands.
    /// </summary>
    public bool ExpandEventCommands
    {
        get => (bool) RawData["EXPAND_EVENT_COMMANDS"];
        set => RawData["EXPAND_EVENT_COMMANDS"] = value;
    }
    /// <summary>
    /// Whether the user prefers to use the rectangle tool including filling.
    /// </summary>
    public bool PreferRectangleFill
    {
        get => (bool) RawData["PREFER_RECTANGLE_FILL"];
        set => RawData["PREFER_RECTANGLE_FILL"] = value;
    }
    /// <summary>
    /// Whether the user prefers to use the ellipse tool including filling.
    /// </summary>
    public bool PreferEllipseFill
    {
        get => (bool) RawData["PREFER_ELLIPSE_FILL"];
        set => RawData["PREFER_ELLIPSE_FILL"] = value;
    }
    /// <summary>
    /// Whether the user prefers to use the selection tool for selecting multiple layers.
    /// </summary>
    public bool PreferSelectionAll
    {
        get => (bool) RawData["PREFER_SELECTION_ALL"];
        set => RawData["PREFER_SELECTION_ALL"] = value;
    }
    /// <summary>
    /// The total number of seconds the program has ever been open.
    /// </summary>
    public int SecondsUsed
    {
        get => (int) RawData["SECONDS_USED"];
        set => RawData["SECONDS_USED"] = value;
    }
}