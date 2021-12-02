using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace RPGStudioMK.Game;

[Serializable]
public class MapSelection : ISerializable
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

    public string Serialize()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream stream = new MemoryStream();
        formatter.Serialize(stream, this);
        string data = Convert.ToBase64String(stream.ToArray());
        stream.Close();
        return data;
    }

    public static MapSelection Deserialize(string data)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream stream = new MemoryStream(Convert.FromBase64String(data));
        MapSelection result = (MapSelection)formatter.Deserialize(stream);
        stream.Close();
        return result;
    }
}
