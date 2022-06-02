using System;
using System.Collections.Generic;
using System.Linq;
using RPGStudioMK.Game;

namespace RPGStudioMK.Undo;

public class TilesetChangeUndoAction : BaseUndoAction
{
    public override string Title => "Tileset changed";
    public override string Description
    {
        get
        {
            string s = "";
            s += $"Old Tileset\n";
            s += $"ID: {Utilities.Digits(OldTileset.ID, 3)}\n";
            s += $"Name: {OldTileset.Name}\n";
            s += $"Graphic: {OldTileset.GraphicName}\n";
            s += $"Fog Name: {OldTileset.FogName}\n";
            s += $"Fog Hue: {OldTileset.FogHue}\n";
            s += $"Fog Blend Type: {OldTileset.FogBlendType}\n";
            s += $"Fog SX: {OldTileset.FogSX}\n";
            s += $"Fog SY: {OldTileset.FogSY}\n";
            s += $"Fog Zoom: {OldTileset.FogZoom}\n";
            s += $"Fog Opacity: {OldTileset.FogOpacity}\n";
            s += $"Panorama Name: {OldTileset.PanoramaName}\n";
            s += $"Panorama Hue: {OldTileset.PanoramaHue}\n";
            s += $"Battleback Name: {OldTileset.BattlebackName}\n";
            s += $"Autotiles: {OldTileset.Autotiles.Select(a => a?.Name ?? "").Aggregate((a, b) => a + ", " + b)}\n\n";

            s += $"New Tileset\n";
            s += $"ID: {Utilities.Digits(NewTileset.ID, 3)}\n";
            s += $"Name: {NewTileset.Name}\n";
            s += $"Graphic: {NewTileset.GraphicName}\n";
            s += $"Fog Name: {NewTileset.FogName}\n";
            s += $"Fog Hue: {NewTileset.FogHue}\n";
            s += $"Fog Blend Type: {NewTileset.FogBlendType}\n";
            s += $"Fog SX: {NewTileset.FogSX}\n";
            s += $"Fog SY: {NewTileset.FogSY}\n";
            s += $"Fog Zoom: {NewTileset.FogZoom}\n";
            s += $"Fog Opacity: {NewTileset.FogOpacity}\n";
            s += $"Panorama Name: {NewTileset.PanoramaName}\n";
            s += $"Panorama Hue: {NewTileset.PanoramaHue}\n";
            s += $"Battleback Name: {NewTileset.BattlebackName}\n";
            s += $"Autotiles: {NewTileset.Autotiles.Select(a => a?.Name ?? "").Aggregate((a, b) => a + ", " + b)}\n\n";
            return s;
        }
    }

    Tileset OldTileset;
    Tileset NewTileset;

    public TilesetChangeUndoAction(Tileset OldTileset, Tileset NewTileset)
    {
        this.OldTileset = OldTileset;
        this.NewTileset = NewTileset;
    }

    public static void Create(Tileset OldTileset, Tileset NewTileset)
    {
        var c = new TilesetChangeUndoAction(OldTileset, NewTileset);
        c.Register();
    }

    public override bool Trigger(bool IsRedo)
    {
        bool Continue = true;
        if (!InMode(EditorMode.Database))
        {
            SetDatabaseMode(DatabaseMode.Tilesets);
            Continue = false;
        }
        if (!InDatabaseSubmode(DatabaseMode.Tilesets))
        {
            SetDatabaseSubmode(DatabaseMode.Tilesets);
            Continue = false;
        }
        Widgets.DataTypeTilesets dtt = (Widgets.DataTypeTilesets)Editor.MainWindow.DatabaseWidget.ActiveDatabaseWidget;
        if (dtt.SelectedItem == null || dtt.SelectedItem.ID != OldTileset.ID)
        {
            dtt.SetSelectedIndex(OldTileset.ID - 1);
            Continue = false;
        }
        if (!Continue) return false;
        TriggerLogical(IsRedo);
        dtt.RedrawList();
        dtt.SetTileset(Data.Tilesets[OldTileset.ID], true);
        return true;
    }

    public override void TriggerLogical(bool IsRedo)
    {
        Data.Tilesets[OldTileset.ID] = IsRedo ? NewTileset : OldTileset;
    }
}
