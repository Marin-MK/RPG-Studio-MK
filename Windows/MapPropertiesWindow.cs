using System;
using System.Collections.Generic;
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
            widthlabel.SetPosition(7, 99);
            NumericBox width = new NumericBox(box1);
            width.SetPosition(6, 115);
            width.MinValue = 1;
            width.MaxValue = 255;
            width.SetSize(66, 27);

            Label heightlabel = new Label(box1);
            heightlabel.SetText("Height:");
            heightlabel.SetFont(f);
            heightlabel.SetPosition(78, 99);
            NumericBox height = new NumericBox(box1);
            height.SetPosition(77, 115);
            height.MinValue = 1;
            height.MaxValue = 255;
            height.SetSize(66, 27);

            ListBox Tilesets = new ListBox(box1);
            Tilesets.SetPosition(162, 22);
            Tilesets.SetItems(new List<ListItem>()
            {
                new ListItem("Common"),
                new ListItem("Trees"),
                new ListItem("Outdoor"),
                new ListItem("Mountains"),
                new ListItem("Sea"),
                new ListItem("Houses"),
                new ListItem("Skyscrapers"),
                new ListItem("Rivers"),
                new ListItem("Animals"),
                new ListItem("Pokémon"),
                new ListItem("Objects"),
                new ListItem("Misc.")
            });
            Tilesets.SetButtonText("Add Tileset");
            Tilesets.ListDrawer.SetContextMenuList(new List<IMenuItem>()
            {
                new MenuItem("Move Tileset Up") { OnLeftClick = MoveTilesetUp },
                new MenuItem("Move Tileset Down") { OnLeftClick = MoveTilesetDown },
                new MenuSeparator(),
                new MenuItem("Remove Tileset") { OnLeftClick = RemoveTileset }
            });
            // Makes it so you can right click on the whole widget, except for the last 20 pixels (which is the button)
            Tilesets.ListDrawer.OnContextMenuOpening += delegate (object sender, CancelEventArgs e)
            {
                int ry = Graphics.LastMouseEvent.Y - Tilesets.ListDrawer.Viewport.Y + Tilesets.ListDrawer.Position.Y - Tilesets.ListDrawer.ScrolledPosition.Y;
                if (ry >= Tilesets.ListDrawer.Size.Height - 20) e.Cancel = true;
            };
            Tilesets.ListDrawer.OnButtonClicked += AddTileset;
            Label tilesetslabel = new Label(box1);
            tilesetslabel.SetText("Tilesets:");
            tilesetslabel.SetFont(f);
            tilesetslabel.SetPosition(163, 6);

            ListBox Autotiles = new ListBox(box1);
            Autotiles.SetPosition(312, 22);
            Autotiles.SetItems(new List<ListItem>()
            {
                new ListItem("Sea Tiles"),
                new ListItem("Flowers")
            });
            Autotiles.SetButtonText("Add Autotile");
            Autotiles.ListDrawer.SetContextMenuList(new List<IMenuItem>()
            {
                new MenuItem("Move Autotile Up") { OnLeftClick = MoveAutotileUp },
                new MenuItem("Move Autotile Down") { OnLeftClick = MoveAutotileDown },
                new MenuSeparator(),
                new MenuItem("Remove Autotile") { OnLeftClick = RemoveAutotile }
            });
            // Makes it so you can right click on the whole widget, except for the last 20 pixels (which is the button)
            Autotiles.ListDrawer.OnContextMenuOpening += delegate (object sender, CancelEventArgs e)
            {
                int ry = Graphics.LastMouseEvent.Y - Autotiles.ListDrawer.Viewport.Y + Autotiles.ListDrawer.Position.Y - Autotiles.ListDrawer.ScrolledPosition.Y;
                if (ry >= Autotiles.ListDrawer.Size.Height - 20) e.Cancel = true;
            };
            Autotiles.ListDrawer.OnButtonClicked += AddAutotile;
            Label autotileslabel = new Label(box1);
            autotileslabel.SetText("Autotiles:");
            autotileslabel.SetFont(f);
            autotileslabel.SetPosition(313, 6);
        }

        public void AddTileset(object sender, EventArgs e)
        {
            Console.WriteLine("Add Tileset");
        }

        public void MoveTilesetUp(object sender, EventArgs e)
        {
            Console.WriteLine("Move Tileset Up");
        }

        public void MoveTilesetDown(object sender, EventArgs e)
        {
            Console.WriteLine("Move Tileset Down");
        }

        public void RemoveTileset(object sender, EventArgs e)
        {
            Console.WriteLine("Remove Tileset");
        }

        public void AddAutotile(object sender, EventArgs e)
        {
            Console.WriteLine("Add Autotile");
        }

        public void MoveAutotileUp(object sender, EventArgs e)
        {
            Console.WriteLine("Move Autotile Up");
        }

        public void MoveAutotileDown(object sender, EventArgs e)
        {
            Console.WriteLine("Move Autotile Down");
        }

        public void RemoveAutotile(object sender, EventArgs e)
        {
            Console.WriteLine("Remove Autotile");
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
