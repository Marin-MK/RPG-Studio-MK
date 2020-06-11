using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using MKEditor.Game;
using Newtonsoft.Json.Linq;
using odl;
using amethyst;

namespace MKEditor
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
        public static string FormatPath(string Path, Platform Platform)
        {
            if (Platform == Platform.Windows)
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
            if (Editor.Platform == Platform.Windows)
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
            else if (Editor.Platform == Platform.Linux)
            {
                Process.Start("xdg-open", url);
            }
            else if (Editor.Platform == Platform.MacOS)
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
            string path = FormatPath(Folder, Editor.Platform);
            if (Editor.Platform == Platform.Windows)
            {
                Process.Start("explorer.exe", path);
            }
            else if (Editor.Platform == Platform.Linux)
            {
                Process.Start("xdg-open", path);
            }
            else if (Editor.Platform == Platform.MacOS)
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
                if (!Utilities.IsNumeric(c)) return false;
            }
            return true;
        }

        public static bool EvaluateBooleanExpression(string Expression, Dictionary<string, object> Parameters, IUIParser Parser)
        {
            if (Expression.Contains("&"))
            {
                List<string> ands = Expression.Split('&').ToList();
                for (int i = 0; i < ands.Count; i++)
                {
                    bool result = EvaluateBooleanExpression(ands[i], Parameters, Parser);
                    if (!result) return false;
                }
                return true;
            }
            bool normal = !Expression.StartsWith('!');
            if (!normal) Expression = Expression.Substring(1);
            if (Parameters.ContainsKey(":" + Expression))
            {
                object value = Parameters[":" + Expression];
                if (value is false || value == null) return normal ? false : true;
                return normal ? true : false;
            }
            else if (Regex.IsMatch(Expression, @"([a-zA-Z0-9_]*=[a-zA-Z0-9_:])|([a-zA-Z0-9_]*![a-zA-Z0-9_:])"))
            {
                char c = Expression.Contains('=') ? '=' : '!';
                string varname = Expression.Substring(0, Expression.IndexOf(c));
                string value = Expression.Substring(Expression.IndexOf(c) + 1, Expression.Length - Expression.IndexOf(c) - 1);
                string variable = null;
                if (!Parameters.ContainsKey(":" + varname))
                {
                    Widget w = Parser.GetWidgetFromName(varname);
                    string Identifier = Parser.GetIdentifierFromName(varname);
                    if (w == null) return false;
                    variable = w.GetValue(Identifier).ToString();
                }
                else variable = Parameters[":" + varname].ToString();
                bool result = variable == value;
                if (c == '!') result = !result;
                return normal ? result : !result;
            }
            else if (Regex.IsMatch(Expression, @"[a-zA-Z0-9_]*\?[a-zA-Z0-9_:]"))
            {
                string varname = Expression.Substring(0, Expression.IndexOf('?'));
                string value = Expression.Substring(Expression.IndexOf('?') + 1, Expression.Length - Expression.IndexOf('?') - 1);
                object variable = null;
                if (!Parameters.ContainsKey(":" + varname))
                {
                    Widget w = Parser.GetWidgetFromName(varname);
                    string Identifier = Parser.GetIdentifierFromName(varname);
                    if (w == null) return false;
                    variable = w.GetValue(Identifier);
                }
                else variable = Parameters[":" + varname];
                bool result = false;
                if (value == "string") result = variable is string;
                else if (value == "int") result = variable is int || variable is long;
                else if (value == "hash") result = variable is Dictionary<string, object>;
                else if (value == "array") result = variable is List<object>;
                else if (value == "bool") result = variable is bool;
                else throw new Exception($"Invalid type: '{value}'");
                return normal ? result : !result;
            }
            else
            {
                Widget w = Parser.GetWidgetFromName(Expression);
                if (w == null) return true;
                object v = w.GetValue(Parser.GetIdentifierFromName(Expression));
                if (v == null || v is bool && (bool)v == false) return normal ? false : true;
                return normal ? true : false;
            }
        }

        public static object EvaluateExpression(string Expression, Dictionary<string, object> Parameters, IUIParser Parser)
        {
            if (Regex.IsMatch(Expression, @"[a-zA-Z0-9_\?]*->[\-a-zA-Z0-9_@:\.]*\|[\-a-zA-Z0-9_@:\.]*"))
            {
                int leftidx = Expression.IndexOf('-');
                int mididx = Expression.IndexOf('|');
                string left = Expression.Substring(0, leftidx);
                string mid = Expression.Substring(leftidx + 2, mididx - leftidx - 2);
                string right = Expression.Substring(mididx + 1, Expression.Length - mididx - 1);
                if (EvaluateBooleanExpression(left, Parameters, Parser)) return EvaluateExpression(mid, Parameters, Parser);
                else return EvaluateExpression(right, Parameters, Parser);
            }
            else if (Regex.IsMatch(Expression, @"switch\([a-zA-Z0-9_]*, [a-zA-Z0-9_]*\)"))
            {
                int pid = Expression.IndexOf('(');
                int cid = Expression.IndexOf(',');
                string arg1 = Expression.Substring(pid + 1, cid - pid - 1);
                string arg2 = Expression.Substring(cid + 2, Expression.Length - cid - 3);
                int group_id = -1;
                int switch_id = -1;
                if (Parameters.ContainsKey(":" + arg1)) group_id = (int) Parameters[":" + arg1];
                if (Parameters.ContainsKey(":" + arg2)) switch_id = (int) Parameters[":" + arg2];
                if (group_id != -1 && switch_id != -1)
                {
                    return Editor.ProjectSettings.Switches[group_id - 1].Switches[switch_id - 1].Name;
                }
            }
            else if (Regex.IsMatch(Expression, @"variable\([a-zA-Z0-9_]*, [a-zA-Z0-9_]*\)"))
            {
                int pid = Expression.IndexOf('(');
                int cid = Expression.IndexOf(',');
                string arg1 = Expression.Substring(pid + 1, cid - pid - 1);
                string arg2 = Expression.Substring(cid + 2, Expression.Length - cid - 3);
                int group_id = -1;
                int variable_id = -1;
                if (Parameters.ContainsKey(":" + arg1)) group_id = (int) Parameters[":" + arg1];
                if (Parameters.ContainsKey(":" + arg2)) variable_id = (int) Parameters[":" + arg2];
                if (group_id != -1 && variable_id != -1)
                {
                    return Editor.ProjectSettings.Variables[group_id - 1].Variables[variable_id - 1].Name;
                }
            }
            else
            {
                string type = null;
                string settings = null;
                string identifier = null;
                if (Expression.Contains('@'))
                {
                    settings = Expression.Substring(Expression.IndexOf('@') + 1);
                    Expression = Expression.Substring(0, Expression.IndexOf('@'));
                }
                if (Expression.Contains('?'))
                {
                    type = Expression.Substring(Expression.IndexOf('?') + 1);
                    Expression = Expression.Substring(0, Expression.IndexOf('?'));
                }
                if (Expression.Contains('.'))
                {
                    identifier = Expression.Substring(Expression.IndexOf('.') + 1);
                    Expression = Expression.Substring(0, Expression.IndexOf('.'));
                }
                if (Parameters.ContainsKey(":" + Expression))
                {
                    object param = Parameters[":" + Expression];
                    if (!string.IsNullOrEmpty(identifier))
                    {
                        if (!(param is Dictionary<string, object>)) throw new Exception($"Failed to apply identifier '{identifier}' to parameter of type '{param.GetType().Name}'.");
                        param = ((Dictionary<string, object>) param)[":" + identifier];
                    }
                    if (!string.IsNullOrEmpty(settings))
                    {
                        if (param is int)
                        {
                            if (IsNumeric(settings)) return Digits((int) param, Convert.ToInt32(settings));
                        }
                        else if (param is bool)
                        {
                            if (settings == "ON") return (bool)param ? "ON" : "OFF";
                        }
                    }
                    if (!string.IsNullOrEmpty(type))
                    {
                        bool result = false;
                        if (type == "int") result = param is int || param is long;
                        else if (type == "string") result = param is string;
                        else if (type == "hash") result = param is Dictionary<string, object> || param is Dictionary<object, object>;
                        else if (type == "array") result = param is List<object>;
                        else throw new Exception($"Invalid type: '{type}'");
                        return result ? "true" : "false";
                    }
                    if (param == null) return "";
                    return param;
                }
                else if (Parser != null)
                {
                    Widget w = Parser.GetWidgetFromName(Expression);
                    if (w == null) return Expression;
                    return w.GetValue(Parser.GetIdentifierFromName(Expression));
                }
            }
            return Expression;
        }

        public static string ProcessText(object Object, Dictionary<string, object> Parameters, IUIParser Parser, bool Parsing)
        {
            string Str = "";
            string format = null;
            if (Object is Dictionary<string, string>)
            {
                foreach (string condition in ((Dictionary<string, string>) Object).Keys)
                {
                    if (EvaluateBooleanExpression(condition, Parameters, Parser))
                    {
                        format = ((Dictionary<string, string>) Object)[condition];
                        break;
                    }
                }
            }
            else if (Object is string)
            {
                format = (string) Object;
            }
            else
            {
                throw new Exception($"Invalid text format: '{Object.GetType().Name}'");
            }
            if (format == null) return "";
            if (!Parsing) return format;
            for (int i = 0; i < format.Length; i++)
            {
                if (format[i] == '[' && (i == 0 || format[i - 1] != '\\') && format[i + 1] == 'c' && format[i + 2] == '=')
                {
                    i += 3;
                    int endidx = -1;
                    for (int j = i; j < format.Length; j++)
                    {
                        if ((j == 0 || format[j - 1] != '\\') && format[j] == ']')
                        {
                            endidx = j;
                            break;
                        }
                    }
                    int length = endidx - i;
                    string color = format.Substring(i, length);
                    if (!IsNumeric(color))
                    {
                        color = (string) EvaluateExpression(color, Parameters, Parser);
                    }
                    Str += $"[c={color}]";
                    i += length;
                }
                else if (format[i] == '{' && (i == 0 || format[i - 1] != '\\'))
                {
                    i += 1;
                    int endidx = -1;
                    for (int j = i; j < format.Length; j++)
                    {
                        if (format[j] == '}' && (j == 0 || format[j - 1] != '\\'))
                        {
                            endidx = j;
                            break;
                        }
                    }
                    int length = endidx - i;
                    string var = format.Substring(i, length);
                    Str += EvaluateExpression(var, Parameters, Parser);
                    i += length;
                }
                else if (format[i] == '\\' && (i == 0 || format[i - 1] != '\\'))
                {

                }
                else
                {
                    Str += format[i];
                }
            }
            return Str;
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
    }
}
