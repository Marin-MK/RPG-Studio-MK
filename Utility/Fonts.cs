namespace RPGStudioMK;

public class Fonts
{
    public static Font HomeTitle = new Fonts("Ubuntu-B", 22).Use();
    public static Font HomeFont = new Fonts("Ubuntu-B", 15).Use();
    public static Font Header = new Fonts("Ubuntu-B", 11).Use();
    public static Font TabFont = new Fonts("Ubuntu-B", 12).Use();
    public static Font Paragraph = new Fonts("Cabin-Medium", 9).Use();
    public static Font ParagraphBold = new Fonts("Ubuntu-B", 9).Use();
    public static Font Monospace = new Fonts("UbuntuMono", 11).Use();

    public string Filename;
    public int Size;

    public Fonts(string Filename, int Size)
    {
        this.Filename = Filename;
        this.Size = Size;
    }


    public Font Use()
    {
        return Font.Get(this.Filename, this.Size);
    }
}