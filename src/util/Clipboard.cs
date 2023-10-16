using System;

namespace RPGStudioMK;

public static class Clipboard
{
    public static void SetContent(string s)
    {
        Input.SetClipboard(s);
    }

    public static void SetObject(object o, BinaryData Type)
    {
        string data = Convert.ToBase64String(SerializeAndCompress(o));
        Input.SetClipboard($"RSMKDATA.{Type}:{data}");
    }

    public static string GetContent()
    {
        return Input.GetClipboard();
    }

    public static T GetObject<T>()
    {
        string data = GetContent();
        if (data.StartsWith("RSMKDATA.")) data = data.Substring(data.IndexOf(':') + 1);
        else throw new Exception("Attempted to parse non-RSMK data.");
        T obj = DeserializeAndDecompress<T>(Convert.FromBase64String(data));
        return obj;
    }

    public static bool IsValid(BinaryData Type)
    {
        return GetContent().StartsWith($"RSMKDATA.{Type}:");
    }

    public static string SerializeAndCompressStr<T>(T Object)
    {
        return StringCompressor.CompressGZipStr(Serializer.Serialize<T>(Object));
    }

    public static byte[] SerializeAndCompress<T>(T Object)
    {
        return StringCompressor.CompressGZip(Serializer.Serialize<T>(Object));
    }

    public static T DeserializeAndDecompress<T>(string String)
    {
        return Serializer.Deserialize<T>(StringCompressor.DecompressGZipStr(String));
    }

    public static T DeserializeAndDecompress<T>(byte[] Bytes)
    {
        string json = StringCompressor.DecompressGZip(Bytes);
        return Serializer.Deserialize<T>(json);
    }
}

public enum BinaryData
{
    MAP_SELECTION,
    MAPS,
    TILESET,
    EVENT,
    EVENT_PAGE,
    EVENT_COMMANDS,
    MOVE_COMMAND,
    SPECIES,
    MOVES,
    ABILITIES,
    ITEMS,
    TMS,
    TYPES
}
