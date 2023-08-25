using RPGStudioMK.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Undo;

public class TilesetAutotileChangeUndoAction : BaseUndoAction
{
    public override string Title => $"Changed autotile";
    public override string Description => $"Tileset: {TilesetID}\nAutotile: {AutotileID}\nOld: {OldAutotile}\nNew: {NewAutotile}";

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
        var c = new TilesetAutotileChangeUndoAction(TilesetID, AutotileID, OldAutotile, NewAutotile);
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
        Tileset Tileset = Data.Tilesets[TilesetID];
        dtt.AutotileBoxes[AutotileID].SetText(Tileset.Autotiles[AutotileID].GraphicName);
        dtt.TilesetContainer.SetTileset(Tileset, true);
        return true;
    }

    public override void TriggerLogical(bool IsRedo)
    {
        Tileset Tileset = Data.Tilesets[TilesetID];
        if (Tileset.Autotiles[AutotileID] == null)
        {
            Autotile a = new Autotile();
            a.ID = AutotileID;
            a.Passability = Tileset.Passabilities[(AutotileID + 1) * 48];
            a.Priority = Tileset.Priorities[(AutotileID + 1) * 48];
            a.Tag = Tileset.Tags[(AutotileID + 1) * 48];
            Data.Autotiles[AutotileID] = a;
            Tileset.Autotiles[Tileset.ID * 7 + AutotileID] = Data.Autotiles[AutotileID];
        }
        Tileset.Autotiles[AutotileID].SetGraphic(IsRedo ? NewAutotile : OldAutotile);
    }
}
