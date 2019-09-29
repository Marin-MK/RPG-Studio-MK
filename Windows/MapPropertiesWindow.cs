using System;
using ODL;

namespace MKEditor.Widgets
{
    public class MapPropertiesWindow : PopupWindow
    {
        public MapPropertiesWindow(Data.Map Map, object Parent, string Name = "mapPropertiesWindow")
            : base(Parent, Name)
        {
            this.SetName($"Map Properties - {Utilities.Digits(Map.ID, 3)}: {Map.Name}");
            this.SetSize(540, 524);
            this.Center();
            Label settings = new Label(this);
            settings.SetText("Info");
            settings.SetFont(Font.Get("Fonts/Ubuntu-B", 14));
            settings.SetPosition(12, 26);

            GroupBox box1 = new GroupBox(this);
            box1.SetPosition(19, 47);
            box1.SetSize(450, 203);

            Font f = Font.Get("Fonts/ProductSans-M", 12);

            Label namelabel = new Label(box1);
            namelabel.SetText("Map Name:");
            namelabel.SetFont(f);
            namelabel.SetPosition(7, 6);
            TextBox mapname = new TextBox(box1);
            mapname.SetPosition(6, 22);
            mapname.SetSize(136, 27);

            Label displaynamelabel = new Label(box1);
            displaynamelabel.SetText("Display Name:");
            displaynamelabel.SetFont(f);
            displaynamelabel.SetPosition(7, 52);
            TextBox displayname = new TextBox(box1);
            displayname.SetPosition(6, 68);
            displayname.SetSize(136, 27);

            Label widthlabel = new Label(box1);
            widthlabel.SetText("Width:");
            widthlabel.SetFont(f);
            widthlabel.SetPosition(7, 101);
            NumericBox width = new NumericBox(box1);
            width.SetPosition(6, 115);
            width.MinValue = 1;
            width.MaxValue = 255;
            width.SetSize(66, 27);

            Label heightlabel = new Label(box1);
            heightlabel.SetText("Height:");
            heightlabel.SetFont(f);
            heightlabel.SetPosition(78, 101);
            NumericBox height = new NumericBox(box1);
            height.SetPosition(77, 115);
            height.MinValue = 1;
            height.MaxValue = 255;
            height.SetSize(66, 27);
        }
    }

    public class GroupBox : Widget
    {
        public GroupBox(object Parent, string Name = "groupBox")
            : base(Parent, Name)
        {
            Sprites["bg"] = new Sprite(this.Viewport);
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            if (Sprites["bg"].Bitmap != null) Sprites["bg"].Bitmap.Dispose();
            Sprites["bg"].Bitmap = new Bitmap(Size);
            Sprites["bg"].Bitmap.Unlock();
            Sprites["bg"].Bitmap.DrawRect(Size, 59, 91, 124);
            Sprites["bg"].Bitmap.DrawRect(1, 1, Size.Width - 2, Size.Height - 2, 17, 27, 38);
            Sprites["bg"].Bitmap.FillRect(2, 2, Size.Width - 4, Size.Height - 4, 24, 38, 53);
            Sprites["bg"].Bitmap.Lock();
        }
    }
}
