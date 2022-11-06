using System.Collections.Generic;

namespace RPGStudioMK;

public class Fonts
{
    public static List<(string Alias, Font Font)> AllFonts = new List<(string, Font)>();

    public static Font HomeTitle = new Fonts("Ubuntu-B", 22).Use("Home Title");
    public static Font HomeFont = new Fonts("Ubuntu-B", 15).Use("Home Font");
    public static Font Header = new Fonts("Ubuntu-B", 11).Use("Header");
    public static Font TabFont = new Fonts("Ubuntu-B", 12).Use("Tab Font");
    public static Font Paragraph = new Fonts("Cabin-Medium", 9).Use("Paragraph");
    public static Font ParagraphBold = new Fonts("Ubuntu-B", 9).Use("Paragraph Bold");
    public static Font Monospace = new Fonts("UbuntuMono", 11).Use("Monospace");

    public string Filename;
    public int Size;

    public Fonts(string Filename, int Size)
    {
        this.Filename = Filename;
        this.Size = Size;
    }

    public Font Use(string Alias)
    {
        Font f = Font.Get(this.Filename, this.Size);
        AllFonts.Add((Alias, f));
        return f;
    }
}