using RPGStudioMK.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RPGStudioMK.Game;

public class GamePlugin
{
    public string Name;
    public string FolderName;
    public SemanticVersion Version;
    public string Link;
    public List<string> Credits;
    public List<SemanticVersion> EssentialsVersions;
    public List<(string Plugin, SemanticVersion? Version, DependencyStrength Strength)> Dependencies;
    public List<string> Conflicts;
    public List<Script> Scripts;

    public GamePlugin(string Root)
    {
        this.FolderName = Path.GetFileName(Root);
        this.Scripts = new List<Script>();
        this.Credits = new List<string>();
        this.EssentialsVersions = new List<SemanticVersion>();
        this.Dependencies = new List<(string, SemanticVersion?, DependencyStrength)>();
        this.Conflicts = new List<string>();
        foreach (string file in Directory.EnumerateFiles(Root))
        {
            string LocalFilename = Path.GetFileName(file);
            if (LocalFilename.EndsWith(".rb"))
            {
                Script s = new Script();
                s.Name = Path.GetFileNameWithoutExtension(file);
                s.Content = File.ReadAllText(file);
                Scripts.Add(s);
            }
            else if (LocalFilename == "meta.txt")
            {
                FormattedTextParser.ParseSectionBasedFileWithOrderWithoutHeader(file, list =>
                {
                    foreach ((string key, string value) in list)
                    {
                        switch (key)
                        {
                            case "Name":
                                Name = value;
                                break;
                            case "Version":
                                Version = new SemanticVersion(value);
                                break;
                            case "Credits":
                                Credits.AddRange(value.Split(',').Select(x => x.Trim()));
                                break;
                            case "Essentials":
                                EssentialsVersions.AddRange(value.Split(',').Select(x => new SemanticVersion(x.Trim())));
                                break;
                            case "Website":
                            case "Link":
                                Link = value;
                                break;
                            case "Requires":
                            case "Exact":
                            case "Optional":
                                string[] dep = value.Split(',');
                                SemanticVersion? depversion = null;
                                DependencyStrength depstrength = key switch
                                {
                                    "Requires" => DependencyStrength.Default,
                                    "Exact" => DependencyStrength.Exact,
                                    "Optional" => DependencyStrength.Optional,
                                    _ => DependencyStrength.Default
                                };
                                if (dep.Length == 2) depversion = new SemanticVersion(dep[1]);
                                else if (dep.Length > 2) throw new Exception($"Invalid meta.txt for '{Root}'.");
                                string depname = dep[0];
                                Dependencies.Add((depname, depversion, depstrength));
                                break;
                            case "Conflicts":
                                Conflicts.AddRange(value.Split(',').Select(x => x.Trim()));
                                break;
                        }
                    }
                });
            }
        }
    }

    public void Save()
    {
        string pluginFolder = Path.Combine(Data.ProjectPath, "Plugins", FolderName);
        if (Directory.Exists(pluginFolder))
        {
            foreach (string file in Directory.EnumerateFiles(pluginFolder))
            {
                string relativeName = Path.GetFileName(file);
                if (relativeName != "meta.txt" && !relativeName.EndsWith(".rb")) continue;
        		File.Delete(file);
            }
        }
        File.WriteAllText(Path.Combine(pluginFolder, "meta.txt"), GenerateMetaFile());
        foreach (Script script in this.Scripts)
        {
            File.WriteAllText(Path.Combine(pluginFolder, script.Name + ".rb"), script.Content);
        }
    }

    protected string GenerateMetaFile()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Name = " + this.Name);
        sb.AppendLine("Version = " + this.Version.ToString());
		if (this.EssentialsVersions.Count > 0) sb.AppendLine("Essentials = " + this.EssentialsVersions.Select(v => v.ToString()).Aggregate((a, b) => a + "," + b));
        if (this.Credits.Count > 0) sb.AppendLine("Credits = " + this.Credits.Aggregate((a, b) => a + "," + b));
        if (!string.IsNullOrEmpty(this.Link)) sb.AppendLine("Link = " + this.Link);
        List<string> requiresList = this.Dependencies.FindAll(d => d.Strength == DependencyStrength.Default).Select(d => d.Plugin + (d.Version is null ? "" : "," + d.Version.ToString())).ToList();
		List<string> exactList = this.Dependencies.FindAll(d => d.Strength == DependencyStrength.Exact).Select(d => d.Plugin + (d.Version is null ? "" : "," + d.Version.ToString())).ToList();
		List<string> optionalList = this.Dependencies.FindAll(d => d.Strength == DependencyStrength.Optional).Select(d => d.Plugin + (d.Version is null ? "" : "," + d.Version.ToString())).ToList();
        if (optionalList.Count > 0) optionalList.ForEach(d => sb.AppendLine("Optional = " + d));
        if (exactList.Count > 0) exactList.ForEach(d => sb.AppendLine("Exact = " + d));
        if (requiresList.Count > 0) requiresList.ForEach(d => sb.AppendLine("Requires = " + d));
        this.Conflicts.ForEach(c => sb.AppendLine("Conflicts = " + c));
        return sb.ToString();
	}
}

public enum DependencyStrength
{
    Default,
    Optional,
    Exact
}