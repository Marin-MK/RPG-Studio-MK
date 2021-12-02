using System;
using System.Collections.Generic;
using RPGStudioMK.Game;

namespace RPGStudioMK;

public class LayerRenameUndoAction : BaseUndoAction
{
    public int MapID;
    public int LayerIndex;
    public string OldName;
    public string NewName;

    public LayerRenameUndoAction(int MapID, int LayerIndex, string OldName, string NewName)
    {
        this.MapID = MapID;
        this.LayerIndex = LayerIndex;
        this.OldName = OldName;
        this.NewName = NewName;
    }

    public static void Create(int MapID, int LayerIndex, string OldName, string NewName)
    {
        new LayerRenameUndoAction(MapID, LayerIndex, OldName, NewName);
    }

    public override bool Trigger(bool IsRedo)
    {
        Map Map = Data.Maps[MapID];
        Map.Layers[LayerIndex].Name = IsRedo ? NewName : OldName;
        Editor.MainWindow.MapWidget.MapViewerTiles.LayerPanel.layerwidget.Redraw();
        return true;
    }
}
