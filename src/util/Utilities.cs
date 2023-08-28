using System;
using System.Collections.Generic;
using System.Diagnostics;
using RPGStudioMK.Game;
using System.IO;
using System.Linq;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

namespace RPGStudioMK;

public static class Utilities
{
    private static Random RandomObject = new Random();

    /// <summary>
    /// The bitmap sheet of icons from icons.png.
    /// </summary>
    public static Bitmap IconSheet;

    /// <summary>
    /// The bitmap of the folder icon of the file explorer.
    /// </summary>
    public static Bitmap FolderIcon;

    /// <summary>
    /// The bitmap sheet of icons from event_command_icons.png.
    /// </summary>
    public static Bitmap EventCommandIconSheet;

    public static Bitmap ButtonCornerFade;
    public static Bitmap ButtonHorizontalFade;
    public static Bitmap ButtonVerticalFade;

    /// <summary>
    /// Draws the collapsed or uncollapsed icon on a bitmap. Used for tileset boxes.
    /// </summary>
    /// <param name="b">The bitmap to draw the icon onto.</param>
    /// <param name="x">The x position to draw the icon at.</param>
    /// <param name="y">The y position to draw the icon at.</param>
    /// <param name="collapsed">Whether the icon is collapsed or not.</param>
    public static void DrawCollapseBox(Bitmap b, int x, int y, bool collapsed)
    {
        b.SetPixel(x, y + 2, 17, 33, 50);
        b.SetPixel(x + 2, y, 17, 33, 50);
        b.SetPixel(x + 8, y, 17, 33, 50);
        b.SetPixel(x + 10, y + 2, 17, 33, 50);
        b.SetPixel(x, y + 8, 17, 33, 50);
        b.SetPixel(x + 2, y + 10, 17, 33, 50);
        b.SetPixel(x + 8, y + 10, 17, 33, 50);
        b.SetPixel(x + 10, y + 8, 17, 33, 50);
        b.SetPixel(x + 1, y + 1, 26, 45, 66);
        b.SetPixel(x + 1, y + 9, 26, 45, 66);
        b.SetPixel(x + 9, y + 1, 26, 45, 66);
        b.SetPixel(x + 9, y + 9, 26, 45, 66);
        b.SetPixel(x, y + 3, 39, 64, 90);
        b.SetPixel(x, y + 7, 39, 64, 90);
        b.SetPixel(x + 10, y + 3, 39, 64, 90);
        b.SetPixel(x + 10, y + 7, 39, 64, 90);
        b.SetPixel(x + 3, y + 10, 39, 64, 90);
        b.SetPixel(x + 7, y + 10, 39, 64, 90);
        b.SetPixel(x, y + 4, 53, 83, 114);
        b.SetPixel(x, y + 6, 53, 83, 114);
        b.SetPixel(x + 10, y + 4, 53, 83, 114);
        b.SetPixel(x + 10, y + 6, 53, 83, 114);
        b.SetPixel(x + 4, y + 10, 53, 83, 114);
        b.SetPixel(x + 6, y + 10, 53, 83, 114);
        b.SetPixel(x, y + 5, 58, 90, 122);
        b.SetPixel(x + 10, y + 5, 58, 90, 122);
        b.SetPixel(x + 3, y, 49, 78, 107);
        b.SetPixel(x + 7, y, 49, 78, 107);
        b.DrawLine(x + 4, y, x + 6, y, 64, 104, 146);
        b.DrawLine(x + 2, y + 1, x + 8, y + 1, 64, 104, 146);
        b.DrawLine(x + 1, y + 2, x + 9, y + 2, 64, 104, 146);
        b.DrawLine(x + 1, y + 3, x + 9, y + 3, 64, 104, 146);
        b.DrawLine(x + 1, y + 7, x + 9, y + 7, 64, 104, 146);
        b.DrawLine(x + 1, y + 8, x + 9, y + 8, 64, 104, 146);
        b.DrawLine(x + 2, y + 9, x + 8, y + 9, 64, 104, 146);
        b.DrawLine(x + 2, y + 9, x + 8, y + 9, 64, 104, 146);
        b.SetPixel(x + 5, y + 10, 64, 104, 146);
        b.SetPixel(x + 1, y + 4, 35, 55, 76);
        b.SetPixel(x + 1, y + 6, 35, 55, 76);
        b.SetPixel(x + 9, y + 4, 35, 55, 76);
        b.SetPixel(x + 9, y + 6, 35, 55, 76);
        b.DrawLine(x + 2, y + 4, x + 8, y + 4, 17, 27, 38);
        b.DrawLine(x + 2, y + 6, x + 8, y + 6, 17, 27, 38);
        b.SetPixel(x + 1, y + 5, 17, 27, 38);
        b.SetPixel(x + 9, y + 5, 17, 27, 38);
        b.SetPixel(x + 2, y + 5, 181, 193, 206);
        b.SetPixel(x + 8, y + 5, 181, 193, 206);
        b.DrawLine(x + 3, y + 5, x + 7, y + 5, Color.WHITE);
        if (collapsed)
        {
            b.SetPixel(x + 3, y, 39, 64, 90);
            b.SetPixel(x + 7, y, 39, 64, 90);
            b.SetPixel(x + 4, y, 53, 83, 114);
            b.SetPixel(x + 6, y, 53, 83, 114);
            b.SetPixel(x + 5, y, 58, 90, 122);
            b.SetPixel(x + 5, y + 10, 58, 90, 122);
            b.SetPixel(x + 4, y + 1, 35, 55, 76);
            b.SetPixel(x + 4, y + 9, 35, 55, 76);
            b.SetPixel(x + 6, y + 1, 35, 55, 76);
            b.SetPixel(x + 6, y + 9, 35, 55, 76);
            b.SetPixel(x + 4, y + 3, 17, 27, 38);
            b.SetPixel(x + 4, y + 2, 17, 27, 38);
            b.SetPixel(x + 5, y + 1, 17, 27, 38);
            b.SetPixel(x + 6, y + 2, 17, 27, 38);
            b.SetPixel(x + 6, y + 3, 17, 27, 38);
            b.SetPixel(x + 4, y + 7, 17, 27, 38);
            b.SetPixel(x + 4, y + 8, 17, 27, 38);
            b.SetPixel(x + 5, y + 9, 17, 27, 38);
            b.SetPixel(x + 6, y + 8, 17, 27, 38);
            b.SetPixel(x + 6, y + 7, 17, 27, 38);
            b.SetPixel(x + 5, y + 2, 181, 193, 206);
            b.SetPixel(x + 5, y + 8, 181, 193, 206);
            b.DrawLine(x + 5, y + 3, x + 5, y + 7, Color.WHITE);
        }
    }

    public static object CloneUnknown(object o)
    {
        if (o is int || o is long || o is string || o is true || o is false || o is null) return o;
        else if (o is ICloneable) return ((ICloneable) o).Clone();
        else if (o is List<object>)
        {
            List<object> list = new List<object>();
            for (int i = 0; i < ((List<object>) o).Count; i++)
            {
                list.Add(CloneUnknown(((List<object>) o)[i]));
            }
            return list;
        }
        else throw new Exception("Uncloneable object: " + o.GetType().ToString());
    }

    /// <summary>
    /// Swaps two <paramref name="List"/>&lt;<typeparamref name="T"/>&gt;  elements.
    /// </summary>
    /// <typeparam name="T">The type of list elements.</typeparam>
    /// <param name="List">The list to modify.</param>
    /// <param name="Index1">The first index to swap with the second index.</param>
    /// <param name="Index2">The second index to swap with the first index.</param>
    public static void Swap<T>(this List<T> List, int Index1, int Index2)
    {
        if (Index1 == Index2) return;
        int min = Index1 > Index2 ? Index2 : Index1;
        int max = Index1 > Index2 ? Index1 : Index2;
        List.Insert(min, List[max]);
        List.Insert(max + 1, List[min + 1]);
        List.RemoveAt(min + 1);
        List.RemoveAt(max + 1);
    }

    /// <summary>
    /// Ensures the given number has at least as many digits as specified by adding trailing zeroes.
    /// </summary>
    /// <param name="Number">The number to format.</param>
    /// <param name="Digits">The number of digits.</param>
    public static string Digits(int Number, int Digits)
    {
        string num = Number.ToString();
        bool neg = num.StartsWith('-');
        if (neg) num = num.Substring(1);
        if (num.Length >= Digits) return num;
        int missing = Digits - num.Length;
        for (int i = 0; i < missing; i++) num = "0" + num;
        return neg ? "-" + num : num;
    }

    /// <summary>
    /// Initializes the IconSheet bitmap.
    /// </summary>
    public static void Initialize(bool ButtonsOnly = false)
    {
        if (!ButtonsOnly)
        {
            IconSheet = new Bitmap("assets/img/icons.png");
            FolderIcon = new Bitmap("assets/img/file_explorer_large_folder.png");
            EventCommandIconSheet = new Bitmap("assets/img/event_command_icons.png");
            int seconds = Editor.GeneralSettings.SecondsUsed % 60;
            int minutes = Editor.GeneralSettings.SecondsUsed / 60 % 60;
            int hours = Editor.GeneralSettings.SecondsUsed / 60 / 60 % 24;
            Logger.WriteLine($"Time spent in the program: {hours}h:{minutes}min:{seconds}s");
        }
        Color black = new Color(0, 0, 0, 64);
        Bitmap c = new Bitmap(5, 5);
        c.Unlock();
        c.FillGradientRect(0, 0, 5, 5, Color.ALPHA, Color.ALPHA, Color.ALPHA, black);
        c.Lock();
        ButtonCornerFade = c;
        Bitmap h = new Bitmap(5, 1);
        h.Unlock();
        h.DrawGradientLine(0, 0, 4, 0, Color.ALPHA, black);
        h.Lock();
        ButtonHorizontalFade = h;
        Bitmap v = new Bitmap(1, 5);
        v.Unlock();
        v.DrawGradientLine(0, 0, 0, 4, Color.ALPHA, black);
        v.Lock();
        ButtonVerticalFade = v;
    }

    /// <summary>
    /// Formats the file path based on the platform.
    /// </summary>
    public static string FormatPath(string Path, odl.Platform Platform)
    {
        if (Platform == odl.Platform.Windows)
        {
            while (Path.Contains("/")) Path = Path.Replace("/", "\\");
        }
        else
        {
            while (Path.Contains("\\")) Path = Path.Replace("\\", "/");
        }
        return Path;
    }

    /// <summary>
    /// Opens the link in the browser.
    /// </summary>
    public static void OpenLink(string url)
    {
        Graphics.OpenURL(url);
    }

    /// <summary>
    /// Opens the folder in the file explorer.
    /// </summary>
    public static void OpenFolder(string Folder)
    {
        string path = FormatPath(Folder, odl.Graphics.Platform);
        if (odl.Graphics.Platform == odl.Platform.Windows)
        {
            Process.Start("explorer.exe", path);
        }
        else if (odl.Graphics.Platform == odl.Platform.Linux)
        {
            Process.Start("xdg-open", path);
        }
        else if (odl.Graphics.Platform == odl.Platform.MacOS)
        {
            Process.Start("open", $"-R \"{path}\"");
        }
        else
        {
            try
            {
                Process.Start($"\"{Folder}\"");
            }
            catch
            {
                throw new Exception("Failed to open file explorer '" + path + "'.");
            }
        }
    }

    public static Bitmap CreateMapPreview(Map Map)
    {
        Bitmap bmp = new Bitmap(Map.Width * 32, Map.Height * 32);
        bmp.Unlock();
        for (int layer = 0; layer < Map.Layers.Count; layer++)
        {
            for (int y = 0; y < Map.Height; y++)
            {
                for (int x = 0; x < Map.Width; x++)
                {
                    TileData tile = Map.Layers[layer].Tiles[x + y * Map.Width];
                    if (tile == null) continue;
                    if (tile.TileType == TileType.Tileset)
                    {
                        Bitmap tilesetimage = Data.Tilesets[Map.TilesetIDs[tile.Index]].TilesetBitmap;
                        int tilesetx = tile.ID % 8;
                        int tilesety = (int) Math.Floor(tile.ID / 8d);
                        if (tilesety * 32 + 32 <= tilesetimage.Height)
                            bmp.Build(new Rect(x * 32, y * 32, 32, 32), tilesetimage, new Rect(tilesetx * 32, tilesety * 32, 32, 32), layer == 0 ? BlendMode.None : BlendMode.Blend);
                    }
                    else if (tile.TileType == TileType.Autotile)
                    {
                        Autotile autotile = Data.Autotiles[Map.AutotileIDs[tile.Index]];
                        if (autotile.Format == AutotileFormat.Single)
                        {
                            bmp.Build(new Rect(x * 32, y * 32, 32, 32), autotile.AutotileBitmap, new Rect(0, 0, 32, 32), layer == 0 ? BlendMode.None : BlendMode.Blend);
                        }
                        else
                        {
                            List<int> Tiles = Autotile.AutotileCombinations[autotile.Format][tile.ID];
                            for (int i = 0; i < 4; i++)
                            {
                                bmp.Build(new Rect(x * 32 + 16 * (i % 2), y * 32 + 16 * (int)Math.Floor(i / 2d), 16, 16), autotile.AutotileBitmap,
                                    new Rect(16 * (Tiles[i] % 6), 16 * (int)Math.Floor(Tiles[i] / 6d), 16, 16),
                                    layer == 0 ? BlendMode.None : BlendMode.Blend);
                            }
                        }
                    }
                }
            }
        }
        bmp.Lock();
        return bmp;
    }

    public static Bitmap CreateSmallMapPreview(Map Map)
    {
        Bitmap bmp = new Bitmap(Map.Width * 16, Map.Height * 16);
        bmp.Unlock();
        for (int layer = 0; layer < Map.Layers.Count; layer++)
        {
            for (int y = 0; y < Map.Height; y++)
            {
                for (int x = 0; x < Map.Width; x++)
                {
                    TileData tile = Map.Layers[layer].Tiles[x + y * Map.Width];
                    if (tile == null) continue;
                    if (tile.TileType == TileType.Tileset)
                    {
                        Bitmap tilesetimage = Data.Tilesets[Map.TilesetIDs[tile.Index]].TilesetBitmap;
                        int tilesetx = tile.ID % 8;
                        int tilesety = (int)Math.Floor(tile.ID / 8d);
                        if (tilesety * 32 + 32 <= tilesetimage.Height)
                            bmp.Build(new Rect(x * 16, y * 16, 16, 16), tilesetimage, new Rect(tilesetx * 32, tilesety * 32, 32, 32), layer == 0 ? BlendMode.None : BlendMode.Blend);
                    }
                    else if (tile.TileType == TileType.Autotile)
                    {
                        Autotile autotile = Data.Autotiles[Map.AutotileIDs[tile.Index]];
                        if (autotile.Format == AutotileFormat.Single)
                        {
                            bmp.Build(new Rect(x * 16, y * 16, 16, 16), autotile.AutotileBitmap, new Rect(0, 0, 32, 32), layer == 0 ? BlendMode.None : BlendMode.Blend);
                        }
                        else
                        {
                            List<int> Tiles = Autotile.AutotileCombinations[autotile.Format][tile.ID];
                            for (int i = 0; i < 4; i++)
                            {
                                bmp.Build(new Rect(x * 16 + 8 * (i % 2), y * 16 + 8 * (int)Math.Floor(i / 2d), 8, 8), autotile.AutotileBitmap,
                                    new Rect(16 * (Tiles[i] % 6), 16 * (int)Math.Floor(Tiles[i] / 6d), 16, 16),
                                    layer == 0 ? BlendMode.None : BlendMode.Blend);
                            }
                        }
                    }
                }
            }
        }
        bmp.Lock();
        return bmp;
    }

    public static List<string> FormatString(Font f, string Text, int Width)
    {
        List<string> Lines = new List<string>();
        int startidx = 0;
        int lastsplittableindex = -1;
        for (int i = 0; i < Text.Length; i++)
        {
            char c = Text[i];
            string txt = Text.Substring(startidx, i - startidx + 1);
            Size s = f.TextSize(txt);
            if (c == '\n')
            {
                Lines.Add(Text.Substring(startidx, i - startidx));
                startidx = i + 1;
                if (i == Text.Length - 1) Lines.Add("");
                lastsplittableindex = -1;
            }
            else if (Width != -1 && s.Width > Width)
            {
                int endidx = lastsplittableindex == -1 ? i : lastsplittableindex + 1;
                Lines.Add(Text.Substring(startidx, endidx - startidx - 1));
                startidx = endidx - 1;
                lastsplittableindex = -1;
            }
            else if (c == ' ' || c == '-')
            {
                lastsplittableindex = i + 1;
            }
        }
        if (startidx != Text.Length)
        {
            Lines.Add(Text.Substring(startidx));
        }
        else if (Lines.Count == 0)
        {
            Lines.Add("");
        }
        return Lines;
    }

    public static int Random(int Min, int Max)
    {
        return RandomObject.Next(Min, Max);
    }

    public static bool IsNumeric(char c)
    {
        return c == '0' || c == '1' || c == '2' || c == '3' || c == '4' ||
               c == '5' || c == '6' || c == '7' || c == '8' || c == '9';
    }

    public static bool IsNumeric(string s)
    {
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            if (c == '-' && i == 0)
            {
                if (s.Length == 1) return false;
                continue;
            }
            if (!IsNumeric(c)) return false;
        }
        return true;
    }

    public static List<System.Type> GetParentTypes(System.Type type)
    {
        List<System.Type> Types = new List<System.Type>();
        foreach (System.Type intf in type.GetInterfaces())
        {
            Types.Add(intf);
        }
        System.Type BaseType = type.BaseType;
        while (BaseType != null)
        {
            Types.Add(BaseType);
            BaseType = BaseType.BaseType;
        }
        return Types;
    }

    public static object RubyToNative(IntPtr obj)
    {
        if (obj == Ruby.Nil) return null;
        else if (obj == Ruby.True) return true;
        else if (obj == Ruby.False) return false;
        else if (Ruby.Funcall(obj, "is_a?", Ruby.GetConst(Ruby.Object.Class, "Integer")) == Ruby.True) return Ruby.Integer.FromPtr(obj);
        else if (Ruby.Is(obj, "Symbol")) return Ruby.Symbol.FromPtr(obj);
        else if (Ruby.Is(obj, "String")) return Ruby.String.FromPtr(obj);
        else if (Ruby.Is(obj, "Array"))
        {
            List<object> list = new List<object>();
            for (int i = 0; i < Ruby.Array.Length(obj); i++)
            {
                list.Add(RubyToNative(Ruby.Array.Get(obj, i)));
            }
            return list;
        }
        else if (Ruby.Is(obj, "Hash"))
        {
            Dictionary<object, object> dict = new Dictionary<object, object>();
            IntPtr keys = Ruby.Hash.Keys(obj);
            Ruby.Pin(keys);
            for (int i = 0; i < Ruby.Array.Length(keys); i++)
            {
                IntPtr key = Ruby.Array.Get(keys, i);
                object value = RubyToNative(Ruby.Hash.Get(obj, key));
                dict.Add(RubyToNative(key), value);
            }
            Ruby.Unpin(keys);
            return dict;
        }
        else if (Ruby.Is(obj, "RPG::AudioFile")) return new AudioFile(obj);
        else if (Ruby.Is(obj, "RPG::MoveRoute")) return new MoveRoute(obj);
        else if (Ruby.Is(obj, "RPG::MoveCommand")) return new MoveCommand(obj);
        else if (Ruby.Is(obj, "Tone"))
        {
            short red = (short)Ruby.Float.FromPtr(Ruby.GetIVar(obj, "@red"));
            short green = (short)Ruby.Float.FromPtr(Ruby.GetIVar(obj, "@green"));
            short blue = (short)Ruby.Float.FromPtr(Ruby.GetIVar(obj, "@blue"));
            byte gray = (byte)Ruby.Float.FromPtr(Ruby.GetIVar(obj, "@gray"));
            return new Tone(red, green, blue, gray);
        }
        else if (Ruby.Is(obj, "Color"))
        {
            byte red = (byte)Ruby.Float.FromPtr(Ruby.GetIVar(obj, "@red"));
            byte green = (byte)Ruby.Float.FromPtr(Ruby.GetIVar(obj, "@green"));
            byte blue = (byte)Ruby.Float.FromPtr(Ruby.GetIVar(obj, "@blue"));
            byte alpha = (byte)Ruby.Float.FromPtr(Ruby.GetIVar(obj, "@alpha"));
            return new Color(red, green, blue, alpha);
        }
        else
        {
            throw new Exception($"Could not convert Ruby's '{Ruby.GetClassName(obj)}' class to a native class.");
        }
    }

    public static object JsonToNative(JsonElement e)
    {
        if (e.ValueKind == JsonValueKind.Number) return e.GetInt64();
        else if (e.ValueKind == JsonValueKind.String) return e.GetString();
        else if (e.ValueKind == JsonValueKind.Array) return e.EnumerateArray().Select(e => JsonToNative(e)).ToList();
        else if (e.ValueKind == JsonValueKind.Object)
        {
            var enumerator = e.EnumerateObject();
            Dictionary<string, JsonProperty> dict = new Dictionary<string, JsonProperty>();
            while (enumerator.MoveNext())
            {
                JsonProperty property = enumerator.Current;
                dict.Add(property.Name, property);
            }
            if (dict.ContainsKey("Name") && dict.ContainsKey("Volume") && dict.ContainsKey("Pitch"))
            {
                return new AudioFile(
                    (string) JsonToNative(dict["Name"].Value),
                    (int) (long) JsonToNative(dict["Volume"].Value),
                    (int) (long) JsonToNative(dict["Pitch"].Value)
                );
            }
            else
            {
                throw new NotImplementedException($"Object type not known.\nKeys: {dict.Keys.Aggregate((a, b) => a + ", " + b)}");
            }
        }
        return e;
    }

    public static Color RandomColor(byte Alpha = 255)
    {
        return new Color((byte) Random(0, 255), (byte) Random(0, 255), (byte) Random(0, 255), Alpha);
    }

    public static string RandomText(int? Length = null)
    {
        if (Length == null) Length = Random(1, 27);
        StringBuilder sb = new StringBuilder();
        char[] choice = new char[]
        {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
        };
        for (int i = 0; i < Length; i++)
        {
            sb.Append(choice[Random(0, choice.Length)]);
        }
        return sb.ToString();
    }

    public static IntPtr NativeToRuby(object obj)
    {
        if (obj == null) return Ruby.Nil;
        else if (obj is true) return Ruby.True;
        else if (obj is false) return Ruby.False;
        else if (obj is int) return Ruby.Integer.ToPtr((int) obj);
        else if (obj is long) return Ruby.Integer.ToPtr((long) obj);
        else if (obj is string) return Ruby.String.ToPtr((string) obj);
        else if (obj is List<object>)
        {
            IntPtr array = Ruby.Array.Create();
            Ruby.Pin(array);
            for (int i = 0; i < ((List<object>)obj).Count; i++)
            {
                IntPtr element = NativeToRuby(((List<object>)obj)[i]);
                Ruby.Array.Set(array, i, element);
            }
            Ruby.Unpin(array);
            return array;
        }
        else if (obj is Dictionary<object, object>)
        {
            IntPtr hash = Ruby.Hash.Create();
            Ruby.Pin(hash);
            foreach (KeyValuePair<object, object> kvp in (Dictionary<object, object>)obj)
            {
                IntPtr key = NativeToRuby(kvp.Key);
                IntPtr value = NativeToRuby(kvp.Value);
                Ruby.Hash.Set(hash, key, value);
            }
            Ruby.Unpin(hash);
            return hash;
        }
        else if (obj is AudioFile)
        {
            IntPtr audiofile = Ruby.Funcall(Compatibility.RMXP.AudioFile.Class, "new");
            Ruby.Pin(audiofile);
            Ruby.SetIVar(audiofile, "@name", Ruby.String.ToPtr(((AudioFile)obj).Name));
            Ruby.SetIVar(audiofile, "@volume", Ruby.Integer.ToPtr(((AudioFile)obj).Volume));
            Ruby.SetIVar(audiofile, "@pitch", Ruby.Integer.ToPtr(((AudioFile)obj).Pitch));
            Ruby.Unpin(audiofile);
            return audiofile;
        }
        else if (obj is MoveRoute) return ((MoveRoute)obj).Save();
        else if (obj is MoveCommand) return ((MoveCommand)obj).Save();
        else if (obj is Tone)
        {
            IntPtr tone = Ruby.Funcall(Compatibility.RMXP.Tone.Class, "new");
            Ruby.Pin(tone);
            Ruby.SetIVar(tone, "@red", Ruby.Float.ToPtr(((Tone)obj).Red));
            Ruby.SetIVar(tone, "@green", Ruby.Float.ToPtr(((Tone)obj).Green));
            Ruby.SetIVar(tone, "@blue", Ruby.Float.ToPtr(((Tone)obj).Blue));
            Ruby.SetIVar(tone, "@gray", Ruby.Float.ToPtr(((Tone)obj).Gray));
            Ruby.Unpin(tone);
            return tone;
        }
        else if (obj is Color)
        {
            IntPtr color = Ruby.Funcall(Compatibility.RMXP.Color.Class, "new");
            Ruby.Pin(color);
            Ruby.SetIVar(color, "@red", Ruby.Float.ToPtr(((Color)obj).Red));
            Ruby.SetIVar(color, "@green", Ruby.Float.ToPtr(((Color)obj).Green));
            Ruby.SetIVar(color, "@blue", Ruby.Float.ToPtr(((Color)obj).Blue));
            Ruby.SetIVar(color, "@gray", Ruby.Float.ToPtr(((Color)obj).Alpha));
            Ruby.Unpin(color);
            return color;
        }
        else
        {
            throw new Exception($"Could not convert internal class '{obj.GetType().ToString()}' to a Ruby class.");
        }
    }

    public static List<Point> GetIdenticalConnected(Map map, int layer, int x, int y)
    {
        if (x < 0 || y < 0 || x >= map.Width || y >= map.Height) return new List<Point>();
        bool[] visited = new bool[map.Width * map.Height];
        bool[] connected = new bool[map.Width * map.Height];
        Queue<(int, int)> queue = new Queue<(int, int)>();
        int pos = y * map.Width + x;
        TileData sourceTile = map.Layers[layer].Tiles[pos];
        queue.Enqueue((x, y));
        while (queue.Count > 0)
        {
            (x, y) = queue.Dequeue();
            pos = y * map.Width + x;
            if (visited[pos]) continue;
            visited[pos] = true;
            connected[pos] = true;
            if (x > 0 && IsTileSimilar(map, layer, x - 1, y, sourceTile))
            {
                queue.Enqueue((x - 1, y));
            }
            if (x < map.Width - 1 && IsTileSimilar(map, layer, x + 1, y, sourceTile))
            {
                queue.Enqueue((x + 1, y));
            }
            if (y > 0 && IsTileSimilar(map, layer, x, y - 1, sourceTile))
            {
                queue.Enqueue((x, y - 1));
            }
            if (y < map.Height - 1 && IsTileSimilar(map, layer, x, y + 1, sourceTile))
            {
                queue.Enqueue((x, y + 1));
            }
        }
        List<Point> points = new List<Point>();
        for (int i = 0; i < connected.Length; i++)
        {
            if (connected[i])
            {
                // Turn the index into an x,y coordinate
                int px = i % map.Width;
                int py = i / map.Width;
                points.Add(new Point(px, py));
            }
        }
        return points;
    }

    private static bool IsTileSimilar(Map map, int layer, int x, int y, TileData sourceTile)
    {
        TileData targetTile = map.Layers[layer].Tiles[y * map.Width + x];
        if (sourceTile is null && targetTile is null) return true;
        if (sourceTile is null && targetTile is not null || sourceTile is not null && targetTile is null) return false;
        return sourceTile.Equals(targetTile);
    }

    public static void SetClipboard(string s)
    {
        Input.SetClipboard(s);
    }

    public static void SetClipboard(object o, BinaryData Type)
    {
        string data = Convert.ToBase64String(SerializeAndCompress(o));
        Input.SetClipboard($"RSMKDATA.{Type}:{data}");
    }

    public static string GetClipboardString()
    {
        return Input.GetClipboard();
    }

    public static T GetClipboard<T>()
    {
        string data = GetClipboardString();
        if (data.StartsWith("RSMKDATA.")) data = data.Substring(data.IndexOf(':') + 1);
        else throw new Exception("Attempted to parse non-RSMK data.");
        T obj = DeserializeAndDecompress<T>(Convert.FromBase64String(data));
        return obj;
    }

    public static bool IsClipboardValidBinary(BinaryData Type)
    {
        return GetClipboardString().StartsWith($"RSMKDATA.{Type}:");
    }

    public static string Serialize<T>(T Object)
    {
        return JsonSerializer.Serialize<T>(Object, new JsonSerializerOptions() { IncludeFields = true });
    }

    public static T Deserialize<T>(string String)
    {
        return JsonSerializer.Deserialize<T>(String, new JsonSerializerOptions() { IncludeFields = true });
    }

    public static string SerializeAndCompressStr<T>(T Object)
    {
        return CompressGZipStr(Serialize<T>(Object));
    }

    public static byte[] SerializeAndCompress<T>(T Object)
    {
        return CompressGZip(Serialize<T>(Object));
    }

    public static T DeserializeAndDecompress<T>(string String)
    {
        return Deserialize<T>(DecompressGZipStr(String));
    }

    public static T DeserializeAndDecompress<T>(byte[] Bytes)
    {
        string json = DecompressGZip(Bytes);
        return Deserialize<T>(json);
    }

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

    public static byte[] ReadStream(Stream Stream)
    {
        long LengthLeft = Stream.Length - Stream.Position;
        byte[] Bytes = new byte[LengthLeft];
        int count = 0;
        while (count != LengthLeft)
        {
            count += Stream.Read(Bytes, count, (int) Math.Min(8192, LengthLeft - count));
        }
        return Bytes;
    }

    public static T DeserializeStream<T>(Stream Stream)
    {
        return JsonSerializer.Deserialize<T>(DecompressGZip(ReadStream(Stream)));
    }

    public static void SerializeStream<T>(Stream Stream, T Object)
    {
        Stream.Write(CompressGZip(JsonSerializer.Serialize<T>(Object)));
    }

    public static void ReadSerializationID(Stream Stream, byte ID)
    {
        byte ReadID = (byte) Stream.ReadByte();
        if (ReadID != ID) throw new Exception($"Serialization IDs do not match. Got {ReadID}, expected {ID}.");
    }

    public static void WriteSerializationID(Stream Stream, byte ID)
    {
        Stream.WriteByte(ID);
    }

    public static bool KitExists(string KitName)
    {
        string Filename = Path.Combine(Editor.KitsFolder, KitName + ".zip");
        if (!File.Exists(Filename)) return false;
        FileInfo fi = new FileInfo(Filename);
        return fi.Length > 0;
    }

    public static async Task CopyKit(string KitName, string DestinationFolder, CancellationTokenSource Source, Action<float> OnProgress)
    {
        string Filename = Path.Combine(Editor.KitsFolder, KitName + ".zip");
        MKUtils.Archive archive = new MKUtils.Archive(Filename);
        string MainFolder = null;
        if (!Directory.Exists(DestinationFolder)) Directory.CreateDirectory(DestinationFolder);
        foreach (MKUtils.ArchiveEntry entry in archive.Files)
        {
            if (entry.Filename.Contains('\\') || entry.Filename.Contains('/')) continue;
            if (MainFolder != null)
            {
                MainFolder = null;
                break;
            }
            MainFolder = entry.Filename;
        }
        // if MainFolder is null, then there either are no entries,
        // or there is more than one file/folder in the main archive,
        // meaning we extract it as-is.
        if (MainFolder == null)
        {
            float total = archive.Files.Count;
            float count = 0;
            foreach (MKUtils.ArchiveEntry entry in archive.Files)
            {
                await entry.ExtractAsync(DestinationFolder);
                Source.Token.ThrowIfCancellationRequested();
                count++;
                OnProgress(count / total);
            }
            if (count != total) OnProgress(1);
        }
        else
        {
            // If MainFolder is not null, that means the archive has one single folder at its root.
            // We want to ignore this folder as we've already created our own folder in which we want
            // all the files to reside, thus we purge this part of the path for all the other entries.
            float total = archive.Files.Count - 1;
            float count = 0;
            foreach (MKUtils.ArchiveEntry entry in archive.Files)
            {
                if (entry.Filename == MainFolder) continue;
                if (entry.Filename.Contains(MainFolder)) entry.Rename(entry.Filename.Substring(MainFolder.Length + 1));
                await entry.ExtractAsync(DestinationFolder);
                Source.Token.ThrowIfCancellationRequested();
                count++;
                OnProgress(count / total);
            }
            if (count != total) OnProgress(1);
        }
        archive.Dispose();
    }

    public static string LegalizeFilename(string Filename)
    {
        return string.Concat(Filename.Split(Path.GetInvalidFileNameChars()));
    }

    public static string GetInjectedCodeStart()
    {
        return @"require 'socket'

def pbConnectToEditor
  $Editor = nil
  $EditorReady = false
  t = Thread.new do
    begin
      $Editor = server = TCPSocket.open(""localhost"", 59995)
      puts ""Server connected.""
      begin
        while true
          data = server.gets.chomp
          puts ""Server :: #{data}""
          if data == ""ping""
            server.puts ""keep-alive""
            server.flush
          elsif data == ""close""
            break
          elsif data == ""ready""
            $EditorReady = true
          else
            # Handle arbitrary message
          end
        end
      ensure
        server.close
        puts ""Server disconnected.""
      end
    rescue
      # Likely no server open at port 59995,
      # or the server quit when the game is still running.
    end
  end
  t.abort_on_exception = true
end

def pbMessageEditor(txt)
  if $Editor && $EditorReady
    $Editor.puts(txt)
    $Editor.flush
  end
end

def pbDisconnectFromEditor
  $Editor.close if $Editor
  $Editor = nil
  $EditorReady = false
end

pbConnectToEditor";
    }

    public static string GetInjectedCodeAboveMain()
    {
        return @"";
    }

    /// <summary>
    /// Gradient Descent algorithm.
    /// </summary>
    /// <param name="RandomStartRange">The lower bound of a random initial value. Supports 2 decimal places.</param>
    /// <param name="RandomEndRange">The upper bound of a random initial value. Supports 2 decimal places.</param>
    /// <param name="InitialStepSize">The initial step size of the algorithm.</param>
    /// <param name="Trials">The maximum number of trials after which to stop the algorithm.</param>
    /// <param name="CostFunction">The function that, given an input, produces an output representing cost.</param>
    /// <returns>The local minimum near the initial value. Note that this may not be the global minimum.</returns>
    public static (double Value, double Cost) GradientDescent1D(double RandomStartRange, double RandomEndRange, double InitialStepSize, double Trials, Func<double, double> CostFunction)
    {
    	return GradientDescent1D(Random((int) RandomStartRange * 100, (int) RandomEndRange * 100) / 100d, InitialStepSize, Trials, CostFunction);
    }

    /// <summary>
    /// Gradient Descent algorithm.
    /// </summary>
    /// <param name="StartValue">The initial value to start the gradient descent algorithm at.</param>
    /// <param name="InitialStepSize">The initial step size of the algorithm.</param>
    /// <param name="Trials">The maximum number of trials after which to stop the algorithm.</param>
    /// <param name="CostFunction">The function that, given an input, produces an output representing cost.</param>
    /// <returns>The local minimum near the initial value. Note that this may not be the global minimum.</returns>
    public static (double Value, double Cost) GradientDescent1D(double StartValue, double InitialStepSize, double Trials, Func<double, double> CostFunction)
    {
        double Value = StartValue;
        double Step = InitialStepSize;
        double Cost = CostFunction(Value);
        for (int i = 0; i < Trials; i++)
        {
            double NewValue = Value + Step;
            double NewCost = CostFunction(NewValue);
            if (NewCost < Cost)
            {
                Value = NewValue;
                Cost = NewCost;
            }
            else
            {
                // If the other direction has better cost, flip the step size. If not, halve step size.
                NewValue = Value - Step;
                NewCost = CostFunction(NewValue);
                if (NewCost < Cost)
                {
                    Value = NewValue;
                    Cost = NewCost;
                }
                else
                {
                    // Reversing direction was not more beneficial; halve step size instead.
                    Step /= 2d;
                }
            }
        }
        return (Value, Cost);
    }

    public static (double Value1, double Value2, double Value3, double Cost) GradientDescent3D(double StartValue1, double StartValue2, double StartValue3, double StepSize1, double StepSize2, double StepSize3, double Trials, Func<double, double, double, double> CostFunction)
    {
        double Value1 = StartValue1;
        double Value2 = StartValue2;
        double Value3 = StartValue3;
        double Step1 = StepSize1;
        double Step2 = StepSize2;
        double Step3 = StepSize3;
        double Cost = CostFunction(Value1, Value2, Value3);
        for (int i = 0; i < Trials; i++)
        {
            (double, double, double, double)[] Cases = new[]
            {
                // 27 cases when each value can be 0, +, or -.
                // 26 because we exclude no change to either 3.
                (Value1 + Step1, Value2, Value3, 0d),
                (Value1 + Step1, Value2, Value3 + Step3, 0d),
                (Value1 + Step1, Value2, Value3 - Step3, 0d),
                (Value1 + Step1, Value2 + Step2, Value3, 0d),
                (Value1 + Step1, Value2 + Step2, Value3 + Step3, 0d),
                (Value1 + Step1, Value2 + Step2, Value3 - Step3, 0d),
                (Value1 + Step1, Value2 - Step2, Value3, 0d),
                (Value1 + Step1, Value2 - Step2, Value3 + Step3, 0d),
                (Value1 + Step1, Value2 - Step2, Value3 - Step3, 0d),

                (Value1 - Step1, Value2, Value3, 0d),
                (Value1 - Step1, Value2, Value3 + Step3, 0d),
                (Value1 - Step1, Value2, Value3 - Step3, 0d),
                (Value1 - Step1, Value2 + Step2, Value3, 0d),
                (Value1 - Step1, Value2 + Step2, Value3 + Step3, 0d),
                (Value1 - Step1, Value2 + Step2, Value3 - Step3, 0d),
                (Value1 - Step1, Value2 - Step2, Value3, 0d),
                (Value1 - Step1, Value2 - Step2, Value3 + Step3, 0d),
                (Value1 - Step1, Value2 - Step2, Value3 - Step3, 0d),

                (Value1, Value2, Value3 + Step3, 0d),
                (Value1, Value2, Value3 - Step3, 0d),
                (Value1, Value2 + Step2, Value3, 0d),
                (Value1, Value2 + Step2, Value3 + Step3, 0d),
                (Value1, Value2 + Step2, Value3 - Step3, 0d),
                (Value1, Value2 - Step2, Value3, 0d),
                (Value1, Value2 - Step2, Value3 + Step3, 0d),
                (Value1, Value2 - Step2, Value3 - Step3, 0d),
            };
            for (int j = 0; j < Cases.Length; j++)
            {
                double NewValue1 = Cases[j].Item1;
                double NewValue2 = Cases[j].Item2;
                double NewValue3 = Cases[j].Item3;
                double NewCost = CostFunction(NewValue1, NewValue2, NewValue3);
                Cases[j].Item4 = NewCost;
            }
            List<double> CostList = Cases.Select(c => c.Item4).ToList();
            if (Cases.All(c => c.Item4 >= Cost))
            {
                // All cases cost more than our current cost, i.e. each step is worse than we can do right now
                // Halve all stepsizes.
                Step1 /= 2;
                Step2 /= 2;
                Step3 /= 2;
            }
            else
            {
                // Select lowest cost.
                double Min = double.MaxValue;
                int idx = -1;
                for (int j = 0; j < Cases.Length; j++)
                {
                    if (Cases[j].Item4 < Min)
                    {
                        Min = Cases[j].Item4;
                        idx = j;
                    }
                }
                Value1 = Cases[idx].Item1;
                Value2 = Cases[idx].Item2;
                Value3 = Cases[idx].Item3;
                Cost = Cases[idx].Item4;
                if (Cost == 0) break;
            }
        }
        return (Value1, Value2, Value3, Cost);
    }

    public static void ForEachColor(Action<Color> Callback)
    {
        ForRange(0, 255, x => {
            byte a = (byte) x;
            Callback(new Color(a, 0, 0));
            Callback(new Color(0, a, 0));
            Callback(new Color(0, 0, a));
            ForRange(0, 255, y =>
            {
                byte b = (byte) y;
                Callback(new Color(a, b, 0));
                Callback(new Color(0, a, b));
                Callback(new Color(a, 0, b));
                ForRange(0, 255, z =>
                {
                    byte c = (byte) z;
                    Callback(new Color(a, b, c));
                });
            });
        });
    }

    private static void ForRange(int Start, int End, Action<int> Callback)
    {
        for (int i = Start; i <= End; i++) Callback(i);
    }

    public static bool DoesDirectoryHaveAnyFiles(string Path)
    {
        if (Directory.EnumerateFiles(Path).Any()) return true;
        foreach (string folder in Directory.EnumerateDirectories(Path))
        {
            if (DoesDirectoryHaveAnyFiles(folder)) return true;
        }
        return false;
    }

    public static bool FloatIsInt(float x)
    {
        return Math.Abs(x - (int) x) < float.Epsilon;
    }

    public static bool DoubleIsInt(double x)
    {
        return Math.Abs(x - (int) x) < double.Epsilon;
    }

    public static string Internalize(string name)
	{
		string str = name.ToUpper();
		int idx = 0;
		while (idx < str.Length)
		{
			char c = str[idx];
			if ((c == '_' && idx == 0) || (c < 'A' || c > 'Z') && c != '_')
			{
				str = str.Remove(idx, 1);
				continue;
			}
			else idx++;
		}
		return str;
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
    MOVE,
    ABILITY,
    ITEM,
    TM
}
