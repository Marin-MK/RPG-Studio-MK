using System;
using System.Collections.Generic;
using RPGStudioMK.Game;

namespace RPGStudioMK
{
    public class TileGroupUndoAction : BaseUndoAction
    {
        public int MapID;
        public List<TileChange> Tiles = new List<TileChange>();
        public bool Ready = false;
        public bool PartOfSelection = false;

        public TileGroupUndoAction(int MapID, bool PartOfSelection = false)
        {
            this.MapID = MapID;
            this.PartOfSelection = PartOfSelection;
        }

        public static void Log(int MapID, bool PartOfSelection = false)
        {
            new TileGroupUndoAction(MapID, PartOfSelection);
        }

        public static TileGroupUndoAction GetLatest()
        {
            return (TileGroupUndoAction) Editor.MapUndoList.FindLast(a => a is TileGroupUndoAction && !((TileGroupUndoAction) a).Ready);
        }

        public static TileGroupUndoAction GetLatestAll()
        {
            return (TileGroupUndoAction) Editor.MapUndoList.FindLast(a => a is TileGroupUndoAction);
        }

        public static void AddToLatest(int MapPosition, int Layer, TileData NewTile, TileData OldTile)
        {
            TileGroupUndoAction action = GetLatest();
            TileChange change = action.Tiles.Find(t => t.MapPosition == MapPosition && t.Layer == Layer);
            if (change != null)
            {
                OldTile = change.OldTile;
                action.Tiles.Remove(change);
            }
            action.Tiles.Add(new TileChange(MapPosition, Layer, NewTile, OldTile));
        }

        public override void Trigger(bool IsRedo)
        {
            if (!Ready) throw new Exception("Attempted to undo an unfinished TileGroupUndoAction.");
            bool ActiveMap = Editor.MainWindow.MapWidget.Map.ID == MapID;
            Map Map = Data.Maps[MapID];
            List<int> UnlockedLayers = new List<int>();
            foreach (TileChange tile in Tiles)
            {
                if (ActiveMap && Editor.MainWindow.MapWidget.MapImageWidget.IsLayerLocked(tile.Layer))
                {
                    Editor.MainWindow.MapWidget.MapImageWidget.SetLayerLocked(tile.Layer, false);
                    UnlockedLayers.Add(tile.Layer);
                }
                // OldTile is the tile that's currently displayed on the map - not necessarily related to this undo action,
                // but necessary for the map renderer to compare with the new tile.
                TileData OldTile = Map.Layers[tile.Layer].Tiles[tile.MapPosition];
                TileData NewTile = IsRedo ? tile.NewTile : tile.OldTile;
                Map.Layers[tile.Layer].Tiles[tile.MapPosition] = NewTile;
                if (ActiveMap)
                {
                    Editor.MainWindow.MapWidget.MapImageWidget.DrawTile(
                        tile.MapPosition % Map.Width,
                        (int) Math.Floor((double) tile.MapPosition / Map.Width),
                        tile.Layer,
                        NewTile,
                        OldTile,
                        true
                    );
                }
            }
            if (ActiveMap)
            {
                UnlockedLayers.ForEach(Layer =>
                {
                    Editor.MainWindow.MapWidget.MapImageWidget.SetLayerLocked(Layer, true);
                });
            }
            else
            {
                Editor.MainWindow.MapWidget.MapSelectPanel.SetMap(Map);
            }
        }

        public class TileChange
        {
            public int MapPosition;
            public int Layer;
            public TileData NewTile;
            public TileData OldTile;

            public TileChange(int MapPosition, int Layer, TileData NewTile, TileData OldTile)
            {
                this.MapPosition = MapPosition;
                this.Layer = Layer;
                this.NewTile = (TileData) NewTile?.Clone();
                this.OldTile = (TileData) OldTile?.Clone();
            }
        }
    }
}
