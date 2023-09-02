using System;
using System.IO;
using System.Text.Json;

namespace RPGStudioMK;

public static class Serializer
{
    public static string Serialize<T>(T Object)
    {
        return JsonSerializer.Serialize<T>(Object, new JsonSerializerOptions() { IncludeFields = true });
    }

    public static T Deserialize<T>(string String)
    {
        return JsonSerializer.Deserialize<T>(String, new JsonSerializerOptions() { IncludeFields = true });
    }

    public static T ReadObjectFromStream<T>(Stream Stream)
    {
        return JsonSerializer.Deserialize<T>(StringCompressor.DecompressGZip(Stream.ReadToEnd()));
    }

    public static void WriteObjectToStream<T>(Stream Stream, T Object)
    {
        Stream.Write(StringCompressor.CompressGZip(JsonSerializer.Serialize<T>(Object)));
    }

    public static void ReadSerializationID(Stream Stream, byte ID)
    {
        byte ReadID = (byte)Stream.ReadByte();
        if (ReadID != ID) throw new Exception($"Serialization IDs do not match. Got {ReadID}, expected {ID}.");
    }

    public static void WriteSerializationID(Stream Stream, byte ID)
    {
        Stream.WriteByte(ID);
    }
}

