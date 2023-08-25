using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Utility;

public static class FormattedTextParser
{
    public static string? RootFolder;

    public static void ParseSectionBasedFile(string Filename, Action<string, Dictionary<string, string>> OnParseSection, Action<float>? OnProgress = null)
    {
        if (RootFolder != null && File.Exists(RootFolder + "/" + Filename)) Filename = RootFolder + "/" + Filename;
        if (!File.Exists(Filename)) throw new Exception($"The specified file '{Filename}' does not exist.");
        string CurrentID = null;
        Queue<string> LineQueue = new Queue<string>(File.ReadLines(Filename));
        int Current = 0;
        int Total = LineQueue.Count - 1;
        Dictionary<string, string> CurrentSection = new Dictionary<string, string>();
        while (LineQueue.Count > 0)
        {
            Current++;
            string line = LineQueue.Dequeue().Trim();
            // Skip empty lines and comments
            if (Total != 0) OnProgress?.Invoke((float) Current / Total);
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
        OnParseSection(CurrentID, CurrentSection);
    }

    public static void ParseSectionBasedFileWithOrder(string Filename, Action<string, List<(string, string)>> OnParseSection, Action<float>? OnProgress = null)
    {
        if (RootFolder != null && File.Exists(RootFolder + "/" + Filename)) Filename = RootFolder + "/" + Filename;
        if (!File.Exists(Filename)) throw new Exception($"The specified file '{Filename}' does not exist.");
        Queue<string> LineQueue = new Queue<string>(File.ReadLines(Filename));
        int Current = 0;
        int Total = LineQueue.Count - 1;
        string CurrentID = null;
        List<(string, string)> CurrentSection = new List<(string, string)>();
        while (LineQueue.Count > 0)
        {
            Current++;
            string line = LineQueue.Dequeue().Trim();
            if (Total != 0) OnProgress?.Invoke((float) Current / Total);
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
        OnParseSection(CurrentID, CurrentSection);
    }

    public static void ParseSectionBasedFileWithOrderWithoutHeader(string Filename, Action<List<(string, string)>> OnParseSection, Action<float>? OnProgress = null)
    {
        if (RootFolder != null && File.Exists(RootFolder + "/" + Filename)) Filename = RootFolder + "/" + Filename;
        if (!File.Exists(Filename)) throw new Exception($"The specified file '{Filename}' does not exist.");
        Queue<string> LineQueue = new Queue<string>(File.ReadLines(Filename));
        int Current = 0;
        int Total = LineQueue.Count - 1;
        List<(string, string)> CurrentSection = new List<(string, string)>();
        while (LineQueue.Count > 0)
        {
            Current++;
            string line = LineQueue.Dequeue().Trim();
            if (Total != 0) OnProgress?.Invoke((float) Current / Total);
            // Skip empty lines and comments
            if (string.IsNullOrEmpty(line) || line[0] == '#') continue;
            if (line.Contains("="))
            {
                string[] split = line.Split('=');
                CurrentSection.Add((split[0].Trim(), split[1].Trim()));
            }
        }
        OnParseSection(CurrentSection);
    }

    public static void ParseLineByLineWithHeader(string Filename, Action<string, List<string>> OnParseSection, Action<float>? OnProgress = null)
    {
        if (RootFolder != null && File.Exists(RootFolder + "/" + Filename)) Filename = RootFolder + "/" + Filename;
        if (!File.Exists(Filename)) throw new Exception($"The specified file '{Filename}' does not exist.");
        Queue<string> LineQueue = new Queue<string>(File.ReadLines(Filename));
        int Current = 0;
        int Total = LineQueue.Count - 1;
        string CurrentID = null;
        List<string> CurrentSection = new List<string>();
        while (LineQueue.Count > 0)
        {
            Current++;
            string line = LineQueue.Dequeue().Trim();
            if (Total != 0) OnProgress?.Invoke((float) Current / Total);
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
        OnParseSection(CurrentID, CurrentSection);
    }

    public static void ParseLineByLineCommaBased(string Filename, Action<List<string>> OnParseSection, Action<float>? OnProgress = null)
    {
        if (RootFolder != null && File.Exists(RootFolder + "/" + Filename)) Filename = RootFolder + "/" + Filename;
        if (!File.Exists(Filename)) throw new Exception($"The specified file '{Filename}' does not exist.");
        Queue<string> LineQueue = new Queue<string>(File.ReadLines(Filename));
        int Current = 0;
        int Total = LineQueue.Count - 1;
        while (LineQueue.Count > 0)
        {
            Current++;
            string line = LineQueue.Dequeue().Trim();
            if (Total != 0) OnProgress?.Invoke((float) Current / Total);
            // Skip empty lines and comments
            if (string.IsNullOrEmpty(line) || line[0] == '#') continue;
            List<string> ary = line.Split(',').ToList();
            int startIndex = -1;
            for (int i = 0; i < ary.Count; i++)
            {
                string trimmed = ary[i].Trim();
                if (trimmed.Length == 0) continue;
                if (startIndex == -1 && trimmed[0] == '"')
                {
                    startIndex = i;
                }
                if (trimmed[^1] == '"' && i != startIndex)
                {
                    ary[startIndex] = ary.GetRange(startIndex, i - startIndex + 1).Aggregate((a, b) => a + "," + b).Trim('"');
                    ary.RemoveRange(startIndex + 1, i - startIndex);
                    startIndex = -1;
                }
            }
            OnParseSection?.Invoke(ary.Select(x => x.Trim().Trim('"')).ToList());
        }
    }
}
