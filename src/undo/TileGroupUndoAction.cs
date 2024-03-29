﻿using System;
using System.Collections.Generic;
using System.Text;
using RPGStudioMK.Game;
using RPGStudioMK.Undo;

namespace RPGStudioMK;

public class TileGroupUndoAction : BaseUndoAction
{
    public override string Title => $"Placed tile{(Tiles.Count > 1 ? "s" : "")}";
    public override string Description
    {
        get
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Map: {(Data.Maps.ContainsKey(MapID) ? Data.Maps[MapID].Name : Utilities.Digits(MapID, 3))}");
            sb.AppendLine();
            Tiles.ForEach(change =>
            {
                sb.AppendLine($"Pos: {change.MapPosition} Layer: {change.Layer} Old: {change.OldTile?.ID + (change.OldTile?.TileType == TileType.Autotile ? 384 : 0)} New: {change.NewTile?.ID + (change.NewTile?.TileType == TileType.Autotile ? 384 : 0)}");
            });
            return sb.ToString();
        }
    }

    public int MapID;
    public List<TileChange> Tiles = new List<TileChange>();
    public bool Ready = false;
    public bool PartOfSelection = false;

    public TileGroupUndoAction(int MapID, bool PartOfSelection = false)
    {
        this.MapID = MapID;
        this.PartOfSelection = PartOfSelection;
    }

    public static void Create(int MapID, bool PartOfSelection = false)
    {
        var c = new TileGroupUndoAction(MapID, PartOfSelection);
        c.Register();
    }

    public static TileGroupUndoAction GetLatest()
    {
        return (TileGroupUndoAction)Editor.UndoList.FindLast(a => a is TileGroupUndoAction && !((TileGroupUndoAction)a).Ready);
    }

    public static TileGroupUndoAction GetLatestAll()
    {
        return (TileGroupUndoAction)Editor.UndoList.FindLast(a => a is TileGroupUndoAction);
    }

    public static void AddToLatest(int MapPosition, int Layer, TileData NewTile, TileData OldTile, bool FromAutotile = false)
    {
        TileGroupUndoAction action = GetLatest();
        TileChange change = action.Tiles.Find(t => t.MapPosition == MapPosition && t.Layer == Layer);
        if (change != null)
        {
            OldTile = change.OldTile;
            action.Tiles.Remove(change);
        }
        action.Tiles.Add(new TileChange(MapPosition, Layer, NewTile, OldTile, FromAutotile));
    }

    public override bool Trigger(bool IsRedo)
    {
        if (!Ready) throw new Exception("Attempted to undo an unfinished TileGroupUndoAction.");

        // Ensure we're in the Mapping mode
        bool Continue = true;
        if (!InMode(EditorMode.Mapping))
        {
            SetMappingMode(MapMode.Tiles);
            Continue = false;
        }
        // Ensure we're in the Tiles submode
        if (!InMappingSubmode(MapMode.Tiles))
        {
            SetMappingMode(MapMode.Tiles);
            Continue = false;
        }
        // Ensure we're on the map this action was taken on
        if (Editor.MainWindow.MapWidget.Map.ID != MapID)
        {
            if (!Data.Maps.ContainsKey(MapID)) return false;
            Editor.MainWindow.MapWidget.SetMap(Data.Maps[MapID]);
            Continue = false;
        }
        if (!Continue) return false;

        Map Map = Data.Maps[MapID];
        List<int> UnlockedLayers = new List<int>();
        foreach (TileChange tile in Tiles)
        {
            if (Editor.MainWindow.MapWidget.MapViewer.IsLayerLocked(tile.Layer))
            {
                Editor.MainWindow.MapWidget.MapViewer.SetLayerLocked(tile.Layer, false);
                UnlockedLayers.Add(tile.Layer);
            }
            // OldTile is the tile that's currently displayed on the map - not necessarily related to this undo action,
            // but necessary for the map renderer to compare with the new tile.
            TileData OldTile = Map.Layers[tile.Layer].Tiles[tile.MapPosition];
            TileData NewTile = IsRedo ? tile.NewTile : tile.OldTile;
            Map.Layers[tile.Layer].Tiles[tile.MapPosition] = NewTile;
            Editor.MainWindow.MapWidget.MapViewer.DrawTile(
                tile.MapPosition % Map.Width,
                (int)Math.Floor((double)tile.MapPosition / Map.Width),
                tile.Layer,
                NewTile,
                OldTile,
                false,
                false
            );
        }
        UnlockedLayers.ForEach(Layer =>
        {
            Editor.MainWindow.MapWidget.MapViewer.SetLayerLocked(Layer, true);
        });
        return true;
    }

    public override void TriggerLogical(bool IsRedo)
    {
        Map Map = Data.Maps[MapID];
        foreach (TileChange tile in Tiles)
        {
            TileData NewTile = IsRedo ? tile.NewTile : tile.OldTile;
            Map.Layers[tile.Layer].Tiles[tile.MapPosition] = NewTile;
        }
    }

    public class TileChange
    {
        public int MapPosition;
        public int Layer;
        public TileData NewTile;
        public TileData OldTile;
        public bool FromAutotile;

        public TileChange(int MapPosition, int Layer, TileData NewTile, TileData OldTile, bool FromAutotile)
        {
            this.MapPosition = MapPosition;
            this.Layer = Layer;
            this.NewTile = (TileData)NewTile?.Clone();
            this.OldTile = (TileData)OldTile?.Clone();
            this.FromAutotile = FromAutotile;
        }
    }
}
