using System;
using System.Collections.Generic;
using RPGStudioMK.Game;

namespace RPGStudioMK.Undo;

public class EventChangeUndoAction : BaseUndoAction
{
    public int MapID;
    public Event Event;
    public bool Deletion;

    public EventChangeUndoAction(int MapID, Event Event, bool Deletion)
    {
        this.MapID = MapID;
        this.Event = Event;
        this.Deletion = Deletion;
    }

    public static void Create(int MapID, Event Event, bool Deletion)
    {
        var c = new EventChangeUndoAction(MapID, Event, Deletion);
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

        if (IsRedo != Deletion)
        {
            // Undo deletion, i.e. recreate the new event
            Data.Maps[MapID].Events[Event.ID] = Event;
            Editor.MainWindow.MapWidget.MapViewer.CreateEventFromUndo(Event);
        }
        else
        {
            // Delete the new event
            Data.Maps[MapID].Events.Remove(Event.ID);
            Editor.MainWindow.MapWidget.MapViewer.DeleteEventFromUndo(Event);
        }

        return true;
    }
}
