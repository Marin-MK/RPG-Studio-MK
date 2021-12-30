using System.Collections.Generic;
using System.IO;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class TilesetFilePicker : PopupWindow
{
    public bool PressedOK;
    public string TilesetFilename;

    ListBox Tilesets;
    GroupBox previewbox;
    PictureBox tileset;
    Container scroll;

    public TilesetFilePicker(string InitialFilename = null)
    {
        SetTitle("Pick File");
        MinimumSize = MaximumSize = new Size(506, 512);
        SetSize(MaximumSize);
        Center();

        Label pickerlabel = new Label(this);
        pickerlabel.SetText("Tilesets");
        pickerlabel.SetPosition(25, 31);
        pickerlabel.SetFont(Fonts.UbuntuBold.Use(14));
        Tilesets = new ListBox(this);
        Tilesets.SetPosition(32, 51);
        Tilesets.SetSize(151, 416);
        List<ListItem> items = new List<ListItem>();
        foreach (string filename in Directory.GetFiles(Data.ProjectPath + "/Graphics/Tilesets"))
        {
            items.Add(new ListItem(Path.GetFileNameWithoutExtension(filename), filename));
        }
        items.Sort(delegate (ListItem l1, ListItem l2) { return l1.Name.CompareTo(l2.Name); });
        items.Insert(0, new ListItem("(None)", null));
        Tilesets.SetItems(items);
        Tilesets.OnSelectionChanged += delegate (BaseEventArgs e)
        {
            UpdatePreview();
        };

        Label previewlabel = new Label(this);
        previewlabel.SetText("Preview");
        previewlabel.SetPosition(199, 31);
        previewlabel.SetFont(Fonts.UbuntuBold.Use(14));
        previewbox = new GroupBox(this);
        previewbox.SetPosition(207, 51);
        previewbox.SetSize(273, 416);
        previewbox.Sprites["line"] = new Sprite(previewbox.Viewport, new SolidBitmap(1, 405, new Color(40, 62, 84)));
        previewbox.Sprites["line"].X = 260;
        previewbox.Sprites["line"].Y = 2;
        scroll = new Container(previewbox);
        scroll.SetPosition(3, 3);
        scroll.SetSize(263, 410);
        scroll.SetVScrollBar(new VScrollBar(previewbox));
        scroll.VScrollBar.SetPosition(262, 3);
        scroll.VScrollBar.SetSize(8, 403);
        scroll.VAutoScroll = true;

        tileset = new PictureBox(scroll);

        CreateButton("Cancel", Cancel);
        CreateButton("OK", OK);

        if (!string.IsNullOrEmpty(InitialFilename))
        {
            ListItem item = Tilesets.Items.Find(i => i.Name == InitialFilename);
            if (item == null) Tilesets.SetSelectedIndex(0);
            else Tilesets.SetSelectedIndex(Tilesets.Items.IndexOf(item));
        }
        else Tilesets.SetSelectedIndex(0);
    }

    public void UpdatePreview()
    {
        string filename = (string) Tilesets.SelectedItem.Object;
        tileset.Sprite.Bitmap?.Dispose();
        tileset.Sprite.Bitmap = null;
        tileset.SetSize(1, 1);
        if (string.IsNullOrEmpty(filename) || !Bitmap.FileExistsCaseSensitive(filename)) return;
        tileset.Sprite.Bitmap = new Bitmap(filename);
        tileset.SetSize(tileset.Sprite.Bitmap.Width, tileset.Sprite.Bitmap.Height);
        scroll.VScrollBar.SetValue(0);
    }

    public void OK(BaseEventArgs e)
    {
        this.PressedOK = true;
        this.TilesetFilename = Tilesets.SelectedItem.Object == null ? "" : Tilesets.SelectedItem.Name;
        Close();
    }

    public void Cancel(BaseEventArgs e)
    {
        this.PressedOK = false;
        this.TilesetFilename = null;
        Close();
    }
}
