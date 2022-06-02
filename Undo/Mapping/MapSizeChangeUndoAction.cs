using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPGStudioMK.Game;

namespace RPGStudioMK.Undo;

public class MapSizeChangeUndoAction : BaseUndoAction
{
    public override string Title => $"Map resized";
    public override string Description => $"Map {(Data.Maps.ContainsKey(MapID) ? Data.Maps[MapID].Name : Utilities.Digits(MapID, 3))}\nOld Size: ({OldSize.Width}x{OldSize.Height})\nNew Size: ({NewSize.Width}x{NewSize.Height})";

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
        // Ensure we're in the Mapping mode
        bool Continue = true;
        if (!InMode(EditorMode.Mapping))
        {
            SetMappingMode(MapMode.Tiles);
            Continue = false;
        }
        // Ensure we're on the map this action was taken on
        if (Editor.MainWindow.MapWidget.Map.ID != MapID)
        {
            Editor.MainWindow.MapWidget.MapSelectPanel.SetMap(Data.Maps[this.MapID]);
            Continue = false;
        }
        if (!Continue) return false;
        TriggerLogical(IsRedo);
        Editor.MainWindow.MapWidget.SetMap(Data.Maps[this.MapID]);
        return true;
    }

    public override void TriggerLogical(bool IsRedo)
    {
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
    }
}
