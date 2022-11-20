using System.Collections.Generic;

namespace RPGStudioMK;

public class Fonts
{
    public static List<(string Alias, Font Font, string CodeName)> AllFonts = new List<(string, Font, string)>();

    public static Font HomeTitle = new Fonts("Ubuntu-B", 22).Use("Home Title", "HomeTitle");
    public static Font HomeFont = new Fonts("Ubuntu-B", 15).Use("Home Font", "HomeFont");
    public static Font Header = new Fonts("Ubuntu-B", 11).Use("Header", "Header");
    public static Font TabFont = new Fonts("Ubuntu-B", 12).Use("Tab Font", "TabFont");
    public static Font Paragraph = new Fonts("Cabin-Medium", 9).Use("Paragraph", "Paragraph");
    public static Font ParagraphBold = new Fonts("Ubuntu-B", 9).Use("Paragraph Bold", "ParagraphBold");
    public static Font Monospace = new Fonts("UbuntuMono", 11).Use("Monospace", "Monospace");

    public string Filename;
    public int Size;

    public Fonts(string Filename, int Size)
    {
        this.Filename = Filename;
        this.Size = Size;
    }

    public Font Use(string Alias, string CodeName)
    {
        Font f = Font.Get(this.Filename, this.Size);
        AllFonts.Add((Alias, f, CodeName));
        return f;
    }
}