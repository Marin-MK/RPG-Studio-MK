using System;
using System.Collections.Generic;
using MKEditor.Game;

namespace MKEditor
{
    public class TileGroupUndoAction : UndoAction
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
            return (TileGroupUndoAction)Editor.MapUndoList.FindLast(a => a is TileGroupUndoAction && !((TileGroupUndoAction)a).Ready);
        }

        public static void AddToLatest(int MapPosition, TileData OldTile, TileData NewTile)
        {
            GetLatest().Tiles.Add(new TileChange(MapPosition, OldTile, NewTile));
        }

        public override void Trigger(bool IsRedo)
        {
            if (!Ready) throw new Exception("Attempted to undo an unfinished TileGroupUndoAction.");
            Editor.MainWindow.MapWidget.MapImageWidget.SetLayerLocked(LayerIndex, false);
            foreach (TileChange tile in Tiles)
            {
                Map Map = Data.Maps[MapID];
                TileData OldTile = Map.Layers[LayerIndex].Tiles[tile.MapPosition];
                TileData NewTile = IsRedo ? tile.NewTile : tile.OldTile;
                Editor.MainWindow.MapWidget.MapImageWidget.DrawTile(
                    tile.MapPosition % Map.Width,
                    (int)Math.Floor((double)tile.MapPosition / Map.Width),
                    LayerIndex,
                    NewTile,
                    OldTile
                );
                Map.Layers[LayerIndex].Tiles[tile.MapPosition] = NewTile;
            }
            Editor.MainWindow.MapWidget.MapImageWidget.SetLayerLocked(LayerIndex, true);
        }

        public class TileChange
        {
            public int MapPosition;
            public TileData OldTile;
            public TileData NewTile;

            public TileChange(int MapPosition, TileData OldTile, TileData NewTile)
            {
                this.MapPosition = MapPosition;
                this.OldTile = OldTile?.Clone();
                this.NewTile = NewTile?.Clone();
            }
        }
    }
}
