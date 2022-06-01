using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Undo;

public class TilesetPanoramaChangeUndoAction : BaseUndoAction
{
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
        if (IsRedo)
        {
            Game.Data.Tilesets[TilesetID].PanoramaName = NewPanoramaName;
            Game.Data.Tilesets[TilesetID].PanoramaHue = NewPanoramaHue;
        }
        else
        {
            Game.Data.Tilesets[TilesetID].PanoramaName = OldPanoramaName;
            Game.Data.Tilesets[TilesetID].PanoramaHue = OldPanoramaHue;
        }
        dtt.PanoramaBox.SetText(Game.Data.Tilesets[TilesetID].PanoramaName);
        return true;
    }
}
