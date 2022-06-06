namespace RPGStudioMK;

public class Fonts
{
    public static Fonts UbuntuBold = new Fonts("assets/fonts/Ubuntu-B");
    public static Fonts CabinMedium = new Fonts("assets/fonts/Cabin-Medium");
    public static Fonts FiraCode = new Fonts("assets/fonts/FiraCode-Medium");

    public string Filename;

    public Fonts(string Filename) { this.Filename = Filename; }

    public Font Use(int Size)
    {
        return Font.Get(this.Filename, Size);
    }
}