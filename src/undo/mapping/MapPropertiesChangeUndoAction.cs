using RPGStudioMK.Game;
using System.Collections.Generic;
using System.Text;

namespace RPGStudioMK.Undo;

public class MapPropertiesChangeUndoAction : BaseUndoAction
{
    public override string Title => $"Map properties changed";
    public override string Description
    {
        get
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Map: {(Data.Maps.ContainsKey(MapID) ? Data.Maps[MapID].Name : Utilities.Digits(MapID, 3))}\n");
            Changes.ForEach(change =>
            {
                sb.AppendLine(change.Title + ": " + change.Description + "\n");
            });
            return sb.ToString();
        }
    }

    public int MapID;
    public List<BaseUndoAction> Changes;

    public MapPropertiesChangeUndoAction(int MapID, List<BaseUndoAction> Changes)
    {
        this.MapID = MapID;
        this.Changes = Changes;
    }

    public static void Create(int MapID, List<BaseUndoAction> Changes)
    {
        if (Changes.Count == 0) return;
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
            Editor.MainWindow.MapWidget.SetMap(Data.Maps[this.MapID]);
            Continue = false;
        }
        if (!Continue) return false;
        foreach (BaseUndoAction action in Changes)
        {
            action.Trigger(IsRedo);
        }
        Editor.MainWindow.MapWidget.SetHint($"{(IsRedo ? "Redid" : "Undid")} property changes of map {Utilities.Digits(MapID, 3)}");
        return true;
    }

    public override void TriggerLogical(bool IsRedo)
    {
        foreach (BaseUndoAction action in Changes)
        {
            action.TriggerLogical(IsRedo);
        }
    }
}
