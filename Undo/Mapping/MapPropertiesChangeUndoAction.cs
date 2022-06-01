using RPGStudioMK.Game;
using System.Collections.Generic;

namespace RPGStudioMK.Undo;

public class MapPropertiesChangeUndoAction : BaseUndoAction
{
    public int MapID;
    public List<BaseUndoAction> Changes;

    public MapPropertiesChangeUndoAction(int MapID, List<BaseUndoAction> Changes)
    {
        this.MapID = MapID;
        this.Changes = Changes;
    }

    public static void Create(int MapID, List<BaseUndoAction> Changes)
    {
        var c = new MapPropertiesChangeUndoAction(MapID, Changes);
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
        foreach (BaseUndoAction action in Changes)
        {
            action.Trigger(IsRedo);
        }
        return true;
    }
}
