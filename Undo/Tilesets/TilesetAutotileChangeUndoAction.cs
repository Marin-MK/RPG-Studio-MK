using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK;

public class TilesetAutotileChangeUndoAction : BaseUndoAction
{
    int TilesetID;
    int AutotileID;
    string OldAutotile;
    string NewAutotile;

    public TilesetAutotileChangeUndoAction(int TilesetID, int AutotileID, string OldAutotile, string NewAutotile)
    {
        this.TilesetID = TilesetID;
        this.AutotileID = AutotileID;
        this.OldAutotile = OldAutotile;
        this.NewAutotile = NewAutotile;
    }

    public static void Create(int TilesetID, int AutotileID, string OldAutotile, string NewAutotile)
    {
        new TilesetAutotileChangeUndoAction(TilesetID, AutotileID, OldAutotile, NewAutotile);
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
        Game.Tileset Tileset = Game.Data.Tilesets[TilesetID];
        if (Tileset.Autotiles[AutotileID] == null)
        {
            Game.Autotile a = new Game.Autotile();
            a.ID = AutotileID;
            a.Passability = Tileset.Passabilities[(AutotileID + 1) * 48];
            a.Priority = Tileset.Priorities[(AutotileID + 1) * 48];
            a.Tag = Tileset.Tags[(AutotileID + 1) * 48];
            Game.Data.Autotiles[AutotileID] = a;
            Tileset.Autotiles[Tileset.ID * 7 + AutotileID] = Game.Data.Autotiles[AutotileID];
        }
        if (IsRedo)
        {
            Tileset.Autotiles[AutotileID].SetGraphic(NewAutotile);
        }
        else
        {
            Tileset.Autotiles[AutotileID].SetGraphic(OldAutotile);
        }
        dtt.AutotileBoxes[AutotileID].SetText(Tileset.Autotiles[AutotileID].GraphicName);
        dtt.TilesetContainer.SetTileset(Tileset, true);
        return true;
    }
}
