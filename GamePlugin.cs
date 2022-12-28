using RPGStudioMK.Game;
using RPGStudioMK.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RPGStudioMK;

public class GamePlugin
{
    public string Name;
    public string FolderName;
    public SemanticVersion Version;
    public string? Link;
    public List<string> Credits;
    public List<SemanticVersion> EssentialsVersions;
    public List<(string, SemanticVersion, DependencyStrength)> Dependencies;
    public List<string> Incompatibilities;
    public List<Script> Scripts;

    public GamePlugin(string Root)
    {
        this.FolderName = Path.GetFileName(Root);
        this.Scripts = new List<Script>();
        this.Dependencies = new List<(string, SemanticVersion, DependencyStrength)>();
        this.EssentialsVersions = new List<SemanticVersion>();
        this.Incompatibilities = new List<string>();
        foreach (string File in Directory.EnumerateFiles(Root))
        {
            string LocalFilename = Path.GetFileName(File);
            if (LocalFilename.EndsWith(".rb"))
            {
                Script s = new Script();
                s.Name = Path.GetFileNameWithoutExtension(File);
                s.Content = System.IO.File.ReadAllText(File);
                this.Scripts.Add(s);
            }
            else if (LocalFilename == "meta.txt")
            {
                FormattedTextParser.ParseSectionBasedFileWithOrderWithoutHeader(File, list =>
                {
                    foreach ((string key, string value) in list)
                    {
                        switch (key)
                        {
                            case "Name":
                                this.Name = value;
                                break;
                            case "Version":
                                this.Version = new SemanticVersion(value);
                                break;
                            case "Credits":
                                this.Credits = value.Split(',').Select(x => x.Trim()).ToList();
                                break;
                            case "Essentials":
                                this.EssentialsVersions = value.Split(',').Select(x => new SemanticVersion(x.Trim())).ToList();
                                break;
                            case "Link":
                                this.Link = value;
                                break;
                            case "Requires": case "Exact": case "Optional":
                                string[] dep = value.Split(',');
                                SemanticVersion depversion = new SemanticVersion(0, 0, 0);
                                DependencyStrength depstrength = key switch
                                {
                                    "Requires" => DependencyStrength.Default,
                                    "Exact" => DependencyStrength.Exact,
                                    "Optional" => DependencyStrength.Optional,
                                    _ => DependencyStrength.Default
                                };
                                if (dep.Length == 2) depversion = new SemanticVersion(dep[1]);
                                else if (dep.Length > 3) throw new Exception($"Invalid meta.txt for '{Root}'.");
                                string depname = dep[0];
                                this.Dependencies.Add((depname, depversion, depstrength));
                                break;
                            case "Conflicts":
                                this.Incompatibilities = value.Split(',').Select(x => x.Trim()).ToList();
                                break;
                        }
                    }
                });
            }
        }
    }
}

public enum DependencyStrength
{
    Default,
    Optional,
    Exact,
    OptionalExact
}