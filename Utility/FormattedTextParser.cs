using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Utility;

public static class FormattedTextParser
{
    public static void ParseSectionBasedFile(string Filename, Action<string, Dictionary<string, string>> OnParseSection)
    {
        if (!File.Exists(Filename)) throw new Exception($"The specified file '{Filename}' does not exist.");
        StreamReader sr = new StreamReader(File.OpenRead(Filename));
        string CurrentID = null;
        Dictionary<string, string> CurrentSection = new Dictionary<string, string>();
        while (!sr.EndOfStream)
        {
            string line = sr.ReadLine().Trim();
            // Skip empty lines and comments
            if (string.IsNullOrEmpty(line) || line[0] == '#') continue;
            if (line[0] == '[' && line.Contains(']')) // Start a section
            {
                if (line.Contains('#'))
                {
                    line = line.Substring(0, line.IndexOf('#')).Trim();
                }
                if (line[^1] != ']') throw new Exception($"Invalid PBS data in '{Filename}' line:\n{line}");
                if (CurrentID != null)
                {
                    // Parse the previous section
                    OnParseSection(CurrentID, CurrentSection);
                    CurrentSection.Clear();
                }
                CurrentID = line.Substring(1, line.Length - 2);
            }
            else if (line.Contains("="))
            {
                string[] split = line.Split('=');
                CurrentSection.Add(split[0].Trim(), split[1].Trim());
            }
        }
        sr.Close();
        OnParseSection(CurrentID, CurrentSection);
    }

    public static void ParseSectionBasedFileWithOrder(string Filename, Action<string, List<(string, string)>> OnParseSection)
    {
        if (!File.Exists(Filename)) throw new Exception($"The specified file '{Filename}' does not exist.");
        StreamReader sr = new StreamReader(File.OpenRead(Filename));
        string CurrentID = null;
        List<(string, string)> CurrentSection = new List<(string, string)>();
        while (!sr.EndOfStream)
        {
            string line = sr.ReadLine().Trim();
            // Skip empty lines and comments
            if (string.IsNullOrEmpty(line) || line[0] == '#') continue;
            if (line[0] == '[' && line.Contains(']')) // Start a section
            {
                if (line.Contains('#'))
                {
                    line = line.Substring(0, line.IndexOf('#')).Trim();
                }
                if (line[^1] != ']') throw new Exception($"Invalid PBS data in '{Filename}' line:\n{line}");
                if (CurrentID != null)
                {
                    // Parse the previous section
                    OnParseSection(CurrentID, CurrentSection);
                    CurrentSection.Clear();
                }
                CurrentID = line.Substring(1, line.Length - 2);
            }
            else if (line.Contains("="))
            {
                string[] split = line.Split('=');
                CurrentSection.Add((split[0].Trim(), split[1].Trim()));
            }
        }
        sr.Close();
        OnParseSection(CurrentID, CurrentSection);
    }

    public static void ParseSectionBasedFileWithOrderWithoutHeader(string Filename, Action<List<(string, string)>> OnParseSection)
    {
        if (!File.Exists(Filename)) throw new Exception($"The specified file '{Filename}' does not exist.");
        StreamReader sr = new StreamReader(File.OpenRead(Filename));
        List<(string, string)> CurrentSection = new List<(string, string)>();
        while (!sr.EndOfStream)
        {
            string line = sr.ReadLine().Trim();
            // Skip empty lines and comments
            if (string.IsNullOrEmpty(line) || line[0] == '#') continue;
            if (line.Contains("="))
            {
                string[] split = line.Split('=');
                CurrentSection.Add((split[0].Trim(), split[1].Trim()));
            }
        }
        sr.Close();
        OnParseSection(CurrentSection);
    }

    public static void ParseLineByLineWithHeader(string Filename, Action<string, List<string>> OnParseSection)
    {
        if (!File.Exists(Filename)) throw new Exception($"The specified file '{Filename}' does not exist.");
        StreamReader sr = new StreamReader(File.OpenRead(Filename));
        string CurrentID = null;
        List<string> CurrentSection = new List<string>();
        while (!sr.EndOfStream)
        {
            string line = sr.ReadLine().Trim();
            // Skip empty lines and comments
            if (string.IsNullOrEmpty(line) || line[0] == '#') continue;
            if (line[0] == '[' && line.Contains(']')) // Start a section
            {
                if (line.Contains('#'))
                {
                    line = line.Substring(0, line.IndexOf('#')).Trim();
                }
                if (line[^1] != ']') throw new Exception($"Invalid PBS data in '{Filename}' line:\n{line}");
                if (CurrentID != null)
                {
                    // Parse the previous section
                    OnParseSection(CurrentID, CurrentSection);
                    CurrentSection.Clear();
                }
                CurrentID = line.Substring(1, line.Length - 2);
            }
            else
            {
                CurrentSection.Add(line);
            }
        }
        sr.Close();
        OnParseSection(CurrentID, CurrentSection);
    }
}
