using System.Collections.Generic;

namespace RPGStudioMK.Game;

public class MapSelection
{
    public Dictionary<int, TileGroup> LayerSelections = new Dictionary<int, TileGroup>();
    public int Width;
    public int Height;

    public MapSelection()
    {

    }

    public void AddTile(int Layer, TileData Tile)
    {
        if (!LayerSelections.ContainsKey(Layer)) LayerSelections[Layer] = new TileGroup();
        LayerSelections[Layer].AddTile(Tile);
    }

    public TileData GetTile(int Layer, int ListIndex)
    {
        if (!LayerSelections.ContainsKey(Layer)) return null;
        return LayerSelections[Layer].GetTile(ListIndex);
    }

    public void Clear()
    {
        this.LayerSelections.Clear();
        this.Width = 0;
        this.Height = 0;
    }
}
