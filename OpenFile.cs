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

        public String filter = null;
        public String customFilter = null;
        public int maxCustFilter = 0;
        public int filterIndex = 0;

        public String file = null;
        public int maxFile = 0;

        public String fileTitle = null;
        public int maxFileTitle = 0;

        public String initialDir = null;

        public String title = null;

        public int flags = 0;
        public short fileOffset = 0;
        public short fileExtension = 0;

        public String defExt = null;

        public IntPtr custData = IntPtr.Zero;
        public IntPtr hook = IntPtr.Zero;

        public String templateName = null;

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

            ofn.file = new string(new char[256]);
            ofn.maxFile = ofn.file.Length;

            ofn.fileTitle = new string(new char[64]);
            ofn.maxFileTitle = ofn.fileTitle.Length;

            ofn.initialDir = "C:\\";
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

        public string Show()
        {
            string olddir = System.IO.Directory.GetCurrentDirectory();
            string ret = null;
            if (GetOpenFileName(ofn))
            {
                ret = ofn.file;
            }
            System.IO.Directory.SetCurrentDirectory(olddir);
            return ret;
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
