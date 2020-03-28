using ODL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MKEditor
{
    public class OpenFileDialog
    {
        [DllImport("tinyfiledialogs64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr tinyfd_openFileDialog(
            string Title,
            string DefaultPath,
            int NumFilePatterns,
            string[] FilterPatterns,
            string SingleFilterDescription,
            int AllowMultiple
        );

        public string Title;
        public string DefaultFolder;
        public FileFilter FileFilter;
        public bool AllowMultiple;

        public OpenFileDialog()
        {
            this.Title = "Open File";
            this.DefaultFolder = Directory.GetCurrentDirectory();
            this.AllowMultiple = false;
        }

        public void SetTitle(string Title)
        {
            this.Title = Title;
        }

        public void SetInitialDirectory(string Dir)
        {
            while (Dir.Contains('\\')) Dir = Dir.Replace('\\', '/');
            this.DefaultFolder = Dir;
            if (!Dir.EndsWith('/')) this.DefaultFolder += '/';
        }

        public void SetFilter(FileFilter FileFilter)
        {
            this.FileFilter = FileFilter;
        }

        public void SetAllowMultiple(bool AllowMultiple)
        {
            this.AllowMultiple = AllowMultiple;
        }

        public object Show()
        {
            IntPtr ptr = tinyfd_openFileDialog(
                this.Title,
                this.DefaultFolder,
                this.FileFilter == null ? 0 : this.FileFilter.Extensions.Count,
                this.FileFilter.Extensions.Select(e => "*." + e).ToArray(),
                this.FileFilter.ToString(),
                this.AllowMultiple ? 1 : 0
            );
            if (ptr != IntPtr.Zero)
            {
                string Files = Marshal.PtrToStringAnsi(ptr);
                if (!string.IsNullOrEmpty(Files))
                {
                    while (Files.Contains("\\")) Files = Files.Replace('\\', '/');
                    if (Files.Contains('|'))
                        return Files.Split('|').ToList();
                    else return Files;
                }
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
