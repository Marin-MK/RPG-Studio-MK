using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace RPGStudioMK.Game
{
    [Serializable]
    public class TileGroup : ISerializable
    {
        public List<TileData> Tiles = new List<TileData>();
        public int Width;
        public int Height;

        //private List<string> SerializedTiles;

        public TileGroup()
        {

        }

        public TileGroup(List<TileData> Tiles, int Width, int Height)
        {
            this.Tiles = Tiles;
            this.Width = Width;
            this.Height = Height;
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

        public static TileGroup Deserialize(string data)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream(Convert.FromBase64String(data));
            TileGroup result = (TileGroup) formatter.Deserialize(stream);
            stream.Close();
            return result;
        }
    }
}
