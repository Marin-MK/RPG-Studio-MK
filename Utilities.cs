using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ODL;

namespace MKEditor
{
    public static class Utilities
    {
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

        public static string Digits(int Number, int Digits)
        {
            string num = Number.ToString();
            if (num.Length >= Digits) return num;
            int missing = Digits - num.Length;

            for (int i = 0; i < missing; i++) num = "0" + num;
            return num;
        }

        public static Bitmap IconSheet;

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
    }
}
