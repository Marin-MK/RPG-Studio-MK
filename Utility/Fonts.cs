namespace RPGStudioMK;

public class Fonts
{
    public static Fonts UbuntuBold = new Fonts("Ubuntu-B");
    public static Fonts CabinMedium = new Fonts("Cabin-Medium");
    public static Fonts FiraCode = new Fonts("FiraCode-Medium");
    public static Fonts Monospace = new Fonts("UbuntuMono");

    public string Filename;

    public Fonts(string Filename) { this.Filename = Filename; }


    public Font Use(int Size)
    {
        return Font.Get(this.Filename, Size);
    }
}