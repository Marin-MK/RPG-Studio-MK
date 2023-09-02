using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace RPGStudioMK;

[DebuggerDisplay("({ToString()})")]
public struct SemanticVersion : IComparable<SemanticVersion>
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

    public int CompareTo(SemanticVersion other)
    {
        if (this.Major > other.Major) return 1;
        if (this.Major < other.Major) return -1;
        if (this.Minor > other.Minor) return 1;
        if (this.Minor < other.Minor) return -1;
        return this.Patch.CompareTo(other.Patch);
    }

    public override bool Equals([NotNullWhen(true)] object obj)
    {
        if (obj is not SemanticVersion) return false;
        SemanticVersion v = (SemanticVersion) obj;
        return this.Major == v.Major && this.Minor == v.Minor && this.Patch == v.Patch;
    }

    public override int GetHashCode()
    {
        return ToString().GetHashCode();
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

    public static bool operator >(SemanticVersion a, SemanticVersion b) => a.CompareTo(b) == 1;
    public static bool operator <(SemanticVersion a, SemanticVersion b) => a.CompareTo(b) == -1;
    public static bool operator >=(SemanticVersion a, SemanticVersion b) => a.CompareTo(b) >= 0;
    public static bool operator <=(SemanticVersion a, SemanticVersion b) => a.CompareTo(b) <= 0;
    public static bool operator ==(SemanticVersion a, SemanticVersion b) => a.CompareTo(b) == 0;
    public static bool operator !=(SemanticVersion a, SemanticVersion b) => a.CompareTo(b) != 0;

    public static implicit operator SemanticVersion(string s) => new SemanticVersion(s);
}
