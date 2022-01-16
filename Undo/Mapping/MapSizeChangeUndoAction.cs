﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPGStudioMK.Game;

namespace RPGStudioMK.Undo;

public class MapSizeChangeUndoAction : BaseUndoAction
{
    public int MapID;
    public List<Layer> OldLayers;
    public Size OldSize;
    public List<Layer> NewLayers;
    public Size NewSize;

    public MapSizeChangeUndoAction(int MapID, List<Layer> OldLayers, Size OldSize, List<Layer> NewLayers, Size NewSize)
    {
        this.MapID = MapID;
        this.OldLayers = OldLayers;
        this.OldSize = OldSize;
        this.NewLayers = NewLayers;
        this.NewSize = NewSize;
    }

    public static void Create(int MapID, List<Layer> OldLayers, Size OldSize, List<Layer> NewLayers, Size NewSize)
    {
        var c = new MapSizeChangeUndoAction(MapID, OldLayers, OldSize, NewLayers, NewSize);
        c.Register();
    }

    public override bool Trigger(bool IsRedo)
    {
        if (!InMode(EditorMode.Mapping))
        {
            SetMode(EditorMode.Mapping);
            Editor.MainWindow.MapWidget.MapSelectPanel.SetMap(Data.Maps[this.MapID]);
            return false;
        }
        bool ActiveMap = Editor.MainWindow.MapWidget.Map.ID == MapID;
        if (!ActiveMap)
        {
            Editor.MainWindow.MapWidget.MapSelectPanel.SetMap(Data.Maps[this.MapID]);
            return false;
        }
        if (IsRedo)
        {
            Data.Maps[this.MapID].Layers = NewLayers;
            Data.Maps[this.MapID].Width = NewSize.Width;
            Data.Maps[this.MapID].Height = NewSize.Height;
        }
        else
        {
            Data.Maps[this.MapID].Layers = OldLayers;
            Data.Maps[this.MapID].Width = OldSize.Width;
            Data.Maps[this.MapID].Height = OldSize.Height;
        }
        Editor.MainWindow.MapWidget.SetMap(Data.Maps[this.MapID]);
        return true;
    }
}