using System;
using System.Collections.Generic;
using RPGStudioMK.Game;

namespace RPGStudioMK.Undo;

public class EventMovedUndoAction : BaseUndoAction
{
    public int MapID;
    public int EventID;
    public Point OldPosition;
    public Point NewPosition;

    public EventMovedUndoAction(int MapID, int EventID, Point OldPosition, Point NewPosition)
    {
        this.MapID = MapID;
        this.EventID = EventID;
        this.OldPosition = OldPosition;
        this.NewPosition = NewPosition;
    }

    public static void Create(int MapID, int EventID, Point OldPosition, Point NewPosition)
    {
        var c = new EventMovedUndoAction(MapID, EventID, OldPosition, NewPosition);
        c.Register();
    }

    public override bool Trigger(bool IsRedo)
    {
        // Ensure we're in the Mapping mode
        bool Continue = true;
        if (!InMode(EditorMode.Mapping))
        {
            SetMappingMode(MapMode.Events);
            Continue = false;
        }
        // Ensure we're in the Tiles submode
        if (!InMappingSubmode(MapMode.Events))
        {
            SetMappingMode(MapMode.Events);
            Continue = false;
        }
        // Ensure we're on the map this action was taken on
        if (Editor.MainWindow.MapWidget.Map.ID != MapID)
        {
            Editor.MainWindow.MapWidget.MapSelectPanel.SetMap(Data.Maps[this.MapID]);
            Continue = false;
        }
        if (!Continue) return false;

        if (IsRedo)
        {
            // Redo
            Data.Maps[MapID].Events[EventID].X = NewPosition.X;
            Data.Maps[MapID].Events[EventID].Y = NewPosition.Y;
            Editor.MainWindow.MapWidget.MapViewer.MoveEventFromUndo(Data.Maps[MapID].Events[EventID]);
        }
        else
        {
            // Undo
            Data.Maps[MapID].Events[EventID].X = OldPosition.X;
            Data.Maps[MapID].Events[EventID].Y = OldPosition.Y;
            Editor.MainWindow.MapWidget.MapViewer.MoveEventFromUndo(Data.Maps[MapID].Events[EventID]);
        }

        return true;
    }
}
