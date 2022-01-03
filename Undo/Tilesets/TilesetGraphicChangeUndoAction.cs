using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Undo;

public class TilesetGraphicChangeUndoAction : BaseUndoAction
{
    int TilesetID;
    string OldGraphicName;
    string NewGraphicName;

    public TilesetGraphicChangeUndoAction(int TilesetID, string OldGraphicName, string NewGraphicName)
    {
        this.TilesetID = TilesetID;
        this.OldGraphicName = OldGraphicName;
        this.NewGraphicName = NewGraphicName;
    }

    public static void Create(int TilesetID, string OldGraphicName, string NewGraphicName)
    {
        var c = new TilesetGraphicChangeUndoAction(TilesetID, OldGraphicName, NewGraphicName);
        c.Register();
    }

    public override bool Trigger(bool IsRedo)
    {
        bool Continue = true;
        if (!InMode(EditorMode.Database))
        {
            SetDatabaseMode(Widgets.DatabaseMode.Tilesets);
            Continue = false;
        }
        if (!InDatabaseSubmode(Widgets.DatabaseMode.Tilesets))
        {
            SetDatabaseSubmode(Widgets.DatabaseMode.Tilesets);
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
            Game.Data.Tilesets[TilesetID].GraphicName = NewGraphicName;
        }
        else
        {
            Game.Data.Tilesets[TilesetID].GraphicName = OldGraphicName;
        }
        Game.Data.Tilesets[TilesetID].CreateBitmap(true);
        dtt.GraphicBox.SetText(Game.Data.Tilesets[TilesetID].GraphicName);
        dtt.TilesetContainer.SetTileset(Game.Data.Tilesets[TilesetID], true);
        return true;
    }
}
