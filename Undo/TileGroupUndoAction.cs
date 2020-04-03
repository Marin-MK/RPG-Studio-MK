using System;
using System.Collections.Generic;
using MKEditor.Game;

namespace MKEditor
{
    public class TileGroupUndoAction : BaseUndoAction
    {
        public int MapID;
        public int LayerIndex;
        public List<TileChange> Tiles = new List<TileChange>();
        public bool Ready = false;

        public TileGroupUndoAction(int MapID, int LayerIndex)
        {
            this.MapID = MapID;
            this.LayerIndex = LayerIndex;
        }

        public static void Log(int MapID, int LayerIndex)
        {
            new TileGroupUndoAction(MapID, LayerIndex);
        }

        public static TileGroupUndoAction GetLatest()
        {
            return (TileGroupUndoAction) Editor.MapUndoList.FindLast(a => a is TileGroupUndoAction && !((TileGroupUndoAction) a).Ready);
        }

        public static void AddToLatest(int MapPosition, TileData NewTile, TileData OldTile)
        {
            GetLatest().Tiles.Add(new TileChange(MapPosition, NewTile, OldTile));
        }

        public override void Trigger(bool IsRedo)
        {
            if (!Ready) throw new Exception("Attempted to undo an unfinished TileGroupUndoAction.");
            bool ActiveMap = Editor.MainWindow.MapWidget.Map.ID == MapID;
            if (ActiveMap) Editor.MainWindow.MapWidget.MapImageWidget.SetLayerLocked(LayerIndex, false);
            Map Map = Data.Maps[MapID];
            foreach (TileChange tile in Tiles)
            {
                // OldTile is the tile that's currently displayed on the map - not necessarily related to this undo action,
                // but necessary for the map renderer to compare with the new tile.
                TileData OldTile = Map.Layers[LayerIndex].Tiles[tile.MapPosition];
                TileData NewTile = IsRedo ? tile.NewTile : tile.OldTile;
                Map.Layers[LayerIndex].Tiles[tile.MapPosition] = NewTile;
                if (ActiveMap)
                {
                    Editor.MainWindow.MapWidget.MapImageWidget.DrawTile(
                        tile.MapPosition % Map.Width,
                        (int) Math.Floor((double) tile.MapPosition / Map.Width),
                        LayerIndex,
                        NewTile,
                        OldTile,
                        true
                    );
                }
            }
            if (ActiveMap) Editor.MainWindow.MapWidget.MapImageWidget.SetLayerLocked(LayerIndex, true);
            else
            {
                Editor.MainWindow.MapWidget.MapSelectPanel.SetMap(Map);
            }
        }

        public class TileChange
        {
            public int MapPosition;
            public TileData NewTile;
            public TileData OldTile;

            public TileChange(int MapPosition, TileData NewTile, TileData OldTile)
            {
                this.MapPosition = MapPosition;
                this.NewTile = NewTile?.Clone();
                this.OldTile = OldTile?.Clone();
            }
        }
    }
}
