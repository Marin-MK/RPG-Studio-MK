using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace RPGStudioMK;

[DebuggerDisplay("({Major}.{Minor}.{Patch})")]
public struct SemanticVersion
{
    public int Major;
    public int Minor;
    public int Patch;

    public SemanticVersion(int Major, int Minor, int Patch)
    {
        this.Major = Major;
        this.Minor = Minor;
        this.Patch = Patch;
    }

    public SemanticVersion(string Version)
    {
        Match m = Regex.Match(Version, @"(\d+)\.(\d+)\.(\d+)");
        if (m.Success)
        {
            this.Major = Convert.ToInt32(m.Groups[1].Value);
            this.Minor = Convert.ToInt32(m.Groups[2].Value);
            this.Patch = Convert.ToInt32(m.Groups[3].Value);
            return;
        }
        m = Regex.Match(Version, @"(\d+)\.(\d+)");
        if (m.Success)
        {
            this.Major = Convert.ToInt32(m.Groups[1].Value);
            this.Minor = Convert.ToInt32(m.Groups[2].Value);
            this.Patch = 0;
            return;
        }
        m = Regex.Match(Version, @"(\d+)");
        if (m.Success)
        {
            this.Major = Convert.ToInt32(m.Groups[1].Value);
            this.Minor = 0;
            this.Patch = 0;
            return;
        }
        throw new ArgumentException($"Invalid version format (expected A.B.C, A.B, or A), got '{Version}'.");
    }

	public override string ToString()
	{
        if (this.Patch == 0)
        {
            if (this.Minor == 0) return this.Major.ToString();
            return $"{this.Major}.{this.Minor}";
        }
        return $"{this.Major}.{this.Minor}.{this.Patch}";
	}
}
