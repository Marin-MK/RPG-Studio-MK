using System;
using System.Collections.Generic;
using System.Diagnostics;
using RPGStudioMK.Game;
using Newtonsoft.Json.Linq;
using odl;
using rubydotnet;

namespace RPGStudioMK
{
    public static class Utilities
    {
        private static Random RandomObject = new Random();

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
        /// The bitmap of sheet of icons from icons.png.
        /// </summary>
        public static Bitmap IconSheet;

        /// <summary>
        /// Initializes the IconSheet bitmap.
        /// </summary>
        public static void Initialize()
        {
            IconSheet = new Bitmap("icons.png");
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
            if (odl.Graphics.Platform == odl.Platform.Windows)
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
            else if (odl.Graphics.Platform == odl.Platform.Linux)
            {
                Process.Start("xdg-open", url);
            }
            else if (odl.Graphics.Platform == odl.Platform.MacOS)
            {
                Process.Start("open", url);
            }
            else
            {
                try
                {
                    Process.Start(url);
                }
                catch
                {
                    throw new Exception("Failed to open link '" + url + "'.");
                }
            }
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
                            bmp.Build(new Rect(x * 32, y * 32, 32, 32), tilesetimage, new Rect(tilesetx * 32, tilesety * 32, 32, 32));
                        }
                        else if (tile.TileType == TileType.Autotile)
                        {
                            Autotile autotile = Data.Autotiles[Map.AutotileIDs[tile.Index]];
                            if (autotile.Format == AutotileFormat.Single)
                            {
                                bmp.Build(new Rect(x * 32, y * 32, 32, 32), autotile.AutotileBitmap, new Rect(0, 0, 32, 32));
                            }
                            else
                            {
                                List<int> Tiles = Autotile.AutotileCombinations[autotile.Format][tile.ID];
                                for (int i = 0; i < 4; i++)
                                {
                                    bmp.Build(new Rect(x * 32 + 16 * (i % 2), y * 32 + 16 * (int) Math.Floor(i / 2d), 16, 16), autotile.AutotileBitmap,
                                        new Rect(16 * (Tiles[i] % 6), 16 * (int) Math.Floor(Tiles[i] / 6d), 16, 16));
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
                }
                else if (s.Width >= Width)
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

        public static object JsonToNative(object data)
        {
            if (data is JObject)
            {
                Dictionary<string, object> obj = ((JObject) data).ToObject<Dictionary<string, object>>();
                List<string> keys = new List<string>(obj.Keys);
                for (int i = 0; i < keys.Count; i++)
                {
                    obj[keys[i]] = JsonToNative(obj[keys[i]]);
                }
                return obj;
            }
            else if (data is JArray)
            {
                List<object> obj = ((JArray) data).ToObject<List<object>>();
                for (int i = 0; i < obj.Count; i++)
                {
                    obj[i] = JsonToNative(obj[i]);
                }
                return obj;
            }
            else if (data is long)
            {
                return Convert.ToInt32((long) data);
            }
            else
            {
                return data;
            }
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
                if (c == '-' && i == 0) continue;
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
            if (obj == Ruby.True) return true;
            if (obj == Ruby.False) return false;
            if (Ruby.Funcall(obj, "is_a?", Ruby.GetConst(Ruby.Object.Class, "Integer")) == Ruby.True) return Ruby.Integer.FromPtr(obj);
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
                short red = (short) Ruby.Integer.FromPtr(Ruby.GetIVar(obj, "@red"));
                short green = (short) Ruby.Integer.FromPtr(Ruby.GetIVar(obj, "@green"));
                short blue = (short) Ruby.Integer.FromPtr(Ruby.GetIVar(obj, "@blue"));
                byte gray = (byte) Ruby.Integer.FromPtr(Ruby.GetIVar(obj, "@grey"));
                return new Tone(red, green, blue, gray);
            }
            else
            {
                throw new Exception($"Could not convert Ruby's '{Ruby.GetClassName(obj)}' class to a native class.");
            }
        }
    }
}
