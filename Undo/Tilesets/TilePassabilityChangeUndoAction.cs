using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK;

public class TilePassabilityChangeUndoAction : BaseUndoAction
{
    Game.Tileset Tileset;
    int TileID;
    int TileX;
    int TileY;
    Game.Passability OldPassability;
    Game.Passability NewPassability;

    public TilePassabilityChangeUndoAction(Game.Tileset Tileset, int TileID, int TileX, int TileY, Game.Passability OldPassability, Game.Passability NewPassability)
    {
        this.Tileset = Tileset;
        this.TileID = TileID;
        this.TileX = TileX;
        this.TileY = TileY;
        this.OldPassability = OldPassability;
        this.NewPassability = NewPassability;
    }

    public static void Create(Game.Tileset Tileset, int TileID, int TileX, int TileY, Game.Passability OldPassability, Game.Passability NewPassability)
    {
        new TilePassabilityChangeUndoAction(Tileset, TileID, TileX, TileY, OldPassability, NewPassability);
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
        Widgets.DataTypeTilesets dtt = (Widgets.DataTypeTilesets) Editor.MainWindow.DatabaseWidget.ActiveDatabaseWidget;
        if (dtt.SelectedItem == null || dtt.SelectedItem.ID != Tileset.ID)
        {
            dtt.SetSelectedIndex(Tileset.ID - 1);
            Continue = false;
        }
        if (dtt.Tabs.SelectedIndex != 0)
        {
            dtt.Tabs.SelectTab(0);
            Continue = false;
        }
        if (!Continue) return false;
        if (IsRedo)
        {
            this.Tileset.Passabilities[TileID] = NewPassability;
        }
        else
        {
            this.Tileset.Passabilities[TileID] = OldPassability;
        }
        dtt.TilesetContainer.SetTilePassability(this.TileID, this.TileX, this.TileY, this.Tileset.Passabilities[TileID]);
        return true;
    }
}
