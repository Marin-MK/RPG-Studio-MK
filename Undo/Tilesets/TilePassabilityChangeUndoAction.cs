using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Undo;

public class TilePassabilityChangeUndoAction : BaseUndoAction
{
    int TilesetID;
    int TileID;
    int TileX;
    int TileY;
    Game.Passability OldPassability;
    Game.Passability NewPassability;
    bool Directional;

    public TilePassabilityChangeUndoAction(int TilesetID, int TileID, int TileX, int TileY, Game.Passability OldPassability, Game.Passability NewPassability, bool Directional)
    {
        this.TilesetID = TilesetID;
        this.TileID = TileID;
        this.TileX = TileX;
        this.TileY = TileY;
        this.OldPassability = OldPassability;
        this.NewPassability = NewPassability;
        this.Directional = Directional;
    }

    public static void Create(int TilesetID, int TileID, int TileX, int TileY, Game.Passability OldPassability, Game.Passability NewPassability, bool Directional)
    {
        var c = new TilePassabilityChangeUndoAction(TilesetID, TileID, TileX, TileY, OldPassability, NewPassability, Directional);
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
        int tab = Directional ? 1 : 0;
        if (dtt.Tabs.SelectedIndex != tab)
        {
            dtt.Tabs.SelectTab(tab);
            Continue = false;
        }
        if (!Continue) return false;
        if (IsRedo)
        {
            Game.Data.Tilesets[TilesetID].Passabilities[TileID] = NewPassability;
        }
        else
        {
            Game.Data.Tilesets[TilesetID].Passabilities[TileID] = OldPassability;
        }
        dtt.TilesetContainer.SetTilePassability(this.TileID, this.TileX, this.TileY, Game.Data.Tilesets[TilesetID].Passabilities[TileID]);
        return true;
    }
}
