using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK;

public class TilesetFogChangeUndoAction : BaseUndoAction
{
    int TilesetID;
    string OldFogName;
    string NewFogName;
    int OldFogHue;
    int NewFogHue;
    byte OldFogOpacity;
    byte NewFogOpacity;
    int OldFogBlendType;
    int NewFogBlendType;
    int OldFogZoom;
    int NewFogZoom;
    int OldFogSX;
    int NewFogSX;
    int OldFogSY;
    int NewFogSY;

    public TilesetFogChangeUndoAction(int TilesetID, string OldFogName, string NewFogName, int OldFogHue, int NewFogHue,
                                      byte OldFogOpacity, byte NewFogOpacity, int OldFogBlendType, int NewFogBlendType,
                                      int OldFogZoom, int NewFogZoom, int OldFogSX, int NewFogSX, int OldFogSY, int NewFogSY)
    {
        this.TilesetID = TilesetID;
        this.OldFogName = OldFogName;
        this.NewFogName = NewFogName;
        this.OldFogHue = OldFogHue;
        this.NewFogHue = NewFogHue;
        this.OldFogOpacity = OldFogOpacity;
        this.NewFogOpacity = NewFogOpacity;
        this.OldFogBlendType = OldFogBlendType;
        this.NewFogBlendType = NewFogBlendType;
        this.OldFogZoom = OldFogZoom;
        this.NewFogZoom = NewFogZoom;
        this.OldFogSX = OldFogSX;
        this.NewFogSX = NewFogSX;
        this.OldFogSY = OldFogSY;
        this.NewFogSY = NewFogSY;
    }

    public static void Create(int TilesetID, string OldFogName, string NewFogName, int OldFogHue, int NewFogHue,
                              byte OldFogOpacity, byte NewFogOpacity, int OldFogBlendType, int NewFogBlendType,
                              int OldFogZoom, int NewFogZoom, int OldFogSX, int NewFogSX, int OldFogSY, int NewFogSY)
    {
        new TilesetFogChangeUndoAction(TilesetID, OldFogName, NewFogName, OldFogHue, NewFogHue,
                                       OldFogOpacity, NewFogOpacity, OldFogBlendType, NewFogBlendType,
                                       OldFogZoom, NewFogZoom, OldFogSX, NewFogSX, OldFogSY, NewFogSY);
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
            Game.Data.Tilesets[TilesetID].FogName = NewFogName;
            Game.Data.Tilesets[TilesetID].FogHue = NewFogHue;
            Game.Data.Tilesets[TilesetID].FogOpacity = NewFogOpacity;
            Game.Data.Tilesets[TilesetID].FogBlendType = NewFogBlendType;
            Game.Data.Tilesets[TilesetID].FogZoom = NewFogZoom;
            Game.Data.Tilesets[TilesetID].FogSX = NewFogSX;
            Game.Data.Tilesets[TilesetID].FogSY = NewFogSY;
        }
        else
        {
            Game.Data.Tilesets[TilesetID].FogName = OldFogName;
            Game.Data.Tilesets[TilesetID].FogHue = OldFogHue;
            Game.Data.Tilesets[TilesetID].FogOpacity = OldFogOpacity;
            Game.Data.Tilesets[TilesetID].FogBlendType = OldFogBlendType;
            Game.Data.Tilesets[TilesetID].FogZoom = OldFogZoom;
            Game.Data.Tilesets[TilesetID].FogSX = OldFogSX;
            Game.Data.Tilesets[TilesetID].FogSY = OldFogSY;
        }
        dtt.FogBox.SetText(Game.Data.Tilesets[TilesetID].FogName);
        return true;
    }
}
