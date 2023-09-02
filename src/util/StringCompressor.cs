using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace RPGStudioMK;

public static class StringCompressor
{
    public static string DecompressGZip(byte[] Bytes)
    {
        MemoryStream input = new MemoryStream(Bytes);
        MemoryStream output = new MemoryStream();
        GZipStream zip = new GZipStream(input, CompressionMode.Decompress);
        zip.CopyTo(output);
        zip.Dispose();
        string String = Encoding.UTF8.GetString(output.ToArray());
        input.Dispose();
        return String;
    }

    public static string DecompressGZipStr(string String)
    {
        return DecompressGZip(Encoding.UTF8.GetBytes(String));
    }

    public static byte[] CompressGZip(string String)
    {
        MemoryStream input = new MemoryStream(Encoding.UTF8.GetBytes(String));
        MemoryStream output = new MemoryStream();
        GZipStream zip = new GZipStream(output, CompressionLevel.Optimal);
        input.CopyTo(zip);
        zip.Dispose();
        byte[] Bytes = output.ToArray();
        input.Dispose();
        return Bytes;
    }

    public static string CompressGZipStr(string String)
    {
        return Encoding.UTF8.GetString(CompressGZip(String));
    }
}

