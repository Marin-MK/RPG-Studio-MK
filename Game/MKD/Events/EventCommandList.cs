using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Game;

[Serializable]
public class EventCommandList : ISerializable
{
    public List<EventCommand> Commands;

    public EventCommandList(List<EventCommand> Commands)
    {
        this.Commands = Commands;
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

    public static EventCommandList Deserialize(string data)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream stream = new MemoryStream(Convert.FromBase64String(data));
        EventCommandList result = (EventCommandList) formatter.Deserialize(stream);
        stream.Close();
        return result;
    }
}
