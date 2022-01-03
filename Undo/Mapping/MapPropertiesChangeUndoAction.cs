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
        if (!InMode(EditorMode.Mapping))
        {
            SetMode(EditorMode.Mapping);
            Editor.MainWindow.MapWidget.MapSelectPanel.SetMap(Game.Data.Maps[this.MapID]);
            return false;
        }
        bool ActiveMap = Editor.MainWindow.MapWidget.Map.ID == MapID;
        if (!ActiveMap)
        {
            Editor.MainWindow.MapWidget.MapSelectPanel.SetMap(Game.Data.Maps[this.MapID]);
            return false;
        }
        foreach (BaseUndoAction action in Changes)
        {
            action.Trigger(IsRedo);
        }
        return true;
    }
}
