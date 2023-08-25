using RPGStudioMK.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Undo;

public class TilesetPanoramaChangeUndoAction : BaseUndoAction
{
    public override string Title => "Panorama changed";
    public override string Description
    {
        get
        {
            string s = $"Tileset: {Utilities.Digits(TilesetID, 3)}\n\n";
            s += $"Old Panorama\n";
            s += $"Name: {OldPanoramaName}\n";
            s += $"Hue: {OldPanoramaHue}\n\n";

            s += $"New Panorama\n";
            s += $"Name: {NewPanoramaName}\n";
            s += $"Hue: {NewPanoramaHue}\n";
            return s;
        }
    }

    int TilesetID;
    string OldPanoramaName;
    string NewPanoramaName;
    int OldPanoramaHue;
    int NewPanoramaHue;

    public TilesetPanoramaChangeUndoAction(int TilesetID, string OldPanoramaName, string NewPanoramaName, int OldPanoramaHue, int NewPanoramaHue)
    {
        this.TilesetID = TilesetID;
        this.OldPanoramaName = OldPanoramaName;
        this.NewPanoramaName = NewPanoramaName;
        this.OldPanoramaHue = OldPanoramaHue;
        this.NewPanoramaHue = NewPanoramaHue;
    }

    public static void Create(int TilesetID, string OldPanoramaName, string NewPanoramaName, int OldPanoramaHue, int NewPanoramaHue)
    {
        var c = new TilesetPanoramaChangeUndoAction(TilesetID, OldPanoramaName, NewPanoramaName, OldPanoramaHue, NewPanoramaHue);
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
        if (dtt.SelectedItem == null || dtt.SelectedItem.ID != TilesetID)
        {
            dtt.SetSelectedIndex(TilesetID - 1);
            Continue = false;
        }
        if (!Continue) return false;
        TriggerLogical(IsRedo);
        dtt.PanoramaBox.SetText(Game.Data.Tilesets[TilesetID].PanoramaName);
        return true;
    }

    public override void TriggerLogical(bool IsRedo)
    {
        if (IsRedo)
        {
            Data.Tilesets[TilesetID].PanoramaName = NewPanoramaName;
            Data.Tilesets[TilesetID].PanoramaHue = NewPanoramaHue;
        }
        else
        {
            Data.Tilesets[TilesetID].PanoramaName = OldPanoramaName;
            Data.Tilesets[TilesetID].PanoramaHue = OldPanoramaHue;
        }
    }
}
