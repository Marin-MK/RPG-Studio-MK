using ODL;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MKEditor
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class OpenFileName
    {
        public int structSize = 0;
        public IntPtr dlgOwner = IntPtr.Zero;
        public IntPtr instance = IntPtr.Zero;

        public string filter = null;
        public string customFilter = null;
        public int maxCustFilter = 0;
        public int filterIndex = 0;

        public IntPtr file = IntPtr.Zero;
        public int maxFile = 0;

        public string fileTitle = null;
        public int maxFileTitle = 0;

        public string initialDir = null;

        public string title = null;

        public int flags = 0;
        public short fileOffset = 0;
        public short fileExtension = 0;

        public string defExt = null;

        public IntPtr custData = IntPtr.Zero;
        public IntPtr hook = IntPtr.Zero;

        public string templateName = null;

        public IntPtr reservedPtr = IntPtr.Zero;
        public int reservedInt = 0;
        public int flagsEx = 0;
    }

    public class OpenFile
    {
        [DllImport("Comdlg32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetOpenFileName([In, Out] OpenFileName ofn);

        private OpenFileName ofn = new OpenFileName();

        public OpenFile()
        {
            ofn.structSize = Marshal.SizeOf(ofn);

            SDL2.SDL.SDL_SysWMinfo wmInfo = new SDL2.SDL.SDL_SysWMinfo();
            SDL2.SDL.SDL_VERSION(out wmInfo.version);
            SDL2.SDL.SDL_GetWindowWMInfo(Graphics.Windows[0].SDL_Window, ref wmInfo);
            IntPtr hWnd = wmInfo.info.win.window;
            ofn.dlgOwner = hWnd;

            ofn.filter = "All Files (*.*)\0*.*";

            ofn.file = Marshal.StringToBSTR(new string(new char[16384]));
            ofn.maxFile = 16384;

            ofn.fileTitle = new string(new char[64]);
            ofn.maxFileTitle = ofn.fileTitle.Length;

            ofn.defExt = "mkproj";
        }

        public void SetTitle(string Title)
        {
            ofn.title = Title;
        }

        public void SetInitialDirectory(string Dir)
        {
            while (Dir.Contains("/")) Dir = Dir.Replace("/", "\\");
            ofn.initialDir = Dir;
        }

        public void SetFilters(List<FileFilter> Filters)
        {
            string filter = "";
            foreach (FileFilter f in Filters)
            {
                filter += f.ToString();
            }
            ofn.filter = filter;
        }

        public void SetAllowMultiple(bool Allow)
        {
            if ((ofn.flags & 0x00000200) != 0x00000200) // Allow Multiple
                if (Allow) ofn.flags |= 0x00000200;
            else
                if (!Allow) ofn.flags -= 0x00000200;

            if ((ofn.flags & 0x00080000) != 0x00080000) // Normal Explorer style
                if (Allow) ofn.flags |= 0x00080000;
            else
                if (!Allow) ofn.flags -= 0x00080000;
        }

        public object Show()
        {
            string olddir = System.IO.Directory.GetCurrentDirectory();
            bool Valid = GetOpenFileName(ofn);
            System.IO.Directory.SetCurrentDirectory(olddir);
            if (Valid)
            {
                string FirstFile = null;
                List<string> Files = new List<string>();
                string File = null;
                do
                {
                    File = Marshal.PtrToStringAuto(ofn.file);
                    if (FirstFile == null) FirstFile = File;
                    else if (!string.IsNullOrEmpty(File)) Files.Add(FirstFile + "\\" + File);
                    ofn.file = (IntPtr) ((long) (ofn.file) + File.Length * 2 + 2);
                } while (!string.IsNullOrEmpty(File));
                if (Files.Count == 0) return FirstFile;
                return Files;
            }
            return null;
        }
    }

    public class FileFilter
    {
        public string Name;
        public List<string> Extensions;

        public FileFilter(string Name, params string[] ext)
        {
            this.Name = Name;
            this.Extensions = new List<string>(ext);
        }

        public override string ToString()
        {
            string ext = "";
            for (int i = 0; i < Extensions.Count; i++)
            {
                ext += "*." + Extensions[i];
                if (i != Extensions.Count - 1) ext += ";";
            }
            return $"{Name} ({ext})\0{ext}\0";
        }
    }
}
