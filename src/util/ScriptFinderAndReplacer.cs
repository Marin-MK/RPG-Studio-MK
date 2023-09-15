using RPGStudioMK.Game;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static amethyst.MultilineTextArea;

namespace RPGStudioMK.Utility;

public class ScriptFinderAndReplacer
{
    public List<string> Lines { get; protected set; }
    public bool UseRegex { get; protected set; } = false;
    public bool CaseSensitive { get; protected set; } = false;

    public ScriptFinderAndReplacer()
    {

    }

    public void SetScript(List<Line> lines)
    {
        this.Lines = lines.Select(l => l.Text).ToList();
    }

    public void SetScript(List<string> lines)
    {
        this.Lines = lines;
    }

    public void SetScript(string content)
    {
        this.Lines = content.Split('\n').ToList();
    }

    public void SetScript(Script script)
    {
        this.Lines = script.Content.Split('\n').ToList();
    }

    public void SetUseRegex(bool useRegex)
    {
        this.UseRegex = useRegex;
    }

    public void SetCaseSensitive(bool caseSensitive)
    {
        this.CaseSensitive = caseSensitive;
    }

    public List<Occurrence> Find(string query)
    {
        List<Occurrence> occurrences = new List<Occurrence>();
        if (!CaseSensitive) query = query.ToLower();
        for (int i = 0; i < Lines.Count; i++)
        {
            string lineContent = Lines[i];
            int sIdx = 0;
            while (sIdx < lineContent.Length)
            {
                Occurrence? occ = FindInLine(lineContent, i, query, sIdx);
                if (occ is not null)
                {
                    Occurrence rOcc = (Occurrence) occ;
                    occurrences.Add(rOcc);
                    sIdx = rOcc.IndexInLine + rOcc.Length;
                }
                else break;
            }
        }
        return occurrences;
    }

    private Occurrence? FindInLine(string lineContent, int lineNumber, string query, int lineStartIndex = 0)
    {
        string lineToSearch = lineContent.Substring(lineStartIndex);
        if (!CaseSensitive) lineToSearch = lineToSearch.ToLower();
        if (UseRegex)
        {
            Match match = Regex.Match(lineToSearch, query);
            if (match.Success)
            {
                return new Occurrence()
                {
                    LineNumber = lineNumber,
                    IndexInLine = lineStartIndex + match.Index,
                    Length = match.Length,
                    Content = match.Value,
                    Captures = match.Groups.Values.Skip(1).Select(g => g.Value).ToArray()
                };
            }
            return null;
        }
        int index = lineToSearch.IndexOf(query);
        if (index == -1) return null;
        return new Occurrence()
        {
            LineNumber = lineNumber,
            IndexInLine = index + lineStartIndex,
            Length = query.Length,
            Content = query,
            Captures = new string[0]
        };
    }
}

public class Occurrence
{
    public int ID;
    public int LineNumber;
    public int IndexInLine;
    public int Length;
    public string Content;
    public string[] Captures;
}