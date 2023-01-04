﻿using System;
using System.Collections.Generic;
using RPGStudioMK.Game;

namespace RPGStudioMK.Undo;

public class EventMovedUndoAction : BaseUndoAction
{
    public override string Title => $"Moved event";
    public override string Description => $"Map: {(Data.Maps.ContainsKey(MapID) ? Data.Maps[MapID].Name : Utilities.Digits(MapID, 3))}\nEvent: {Utilities.Digits(EventID, 3)}\n" +
        $"Old Position: ({OldPosition.X}, {OldPosition.Y})\nNew Position: ({NewPosition.X},{NewPosition.Y})";

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
            Editor.MainWindow.MapWidget.SetMap(Data.Maps[this.MapID]);
            Continue = false;
        }
        if (!Continue) return false;

        TriggerLogical(IsRedo);
        Editor.MainWindow.MapWidget.MapViewer.MoveEventFromUndo(Data.Maps[MapID].Events[EventID]);
        Editor.MainWindow.MapWidget.SetHint($"Moved event {Utilities.Digits(EventID, 3)} to ({Data.Maps[MapID].Events[EventID].X},{Data.Maps[MapID].Events[EventID].Y})");

        return true;
    }

    public override void TriggerLogical(bool IsRedo)
    {
        if (IsRedo)
        {
            // Redo
            Data.Maps[MapID].Events[EventID].X = NewPosition.X;
            Data.Maps[MapID].Events[EventID].Y = NewPosition.Y;
        }
        else
        {
            // Undo
            Data.Maps[MapID].Events[EventID].X = OldPosition.X;
            Data.Maps[MapID].Events[EventID].Y = OldPosition.Y;
        }
    }
}
