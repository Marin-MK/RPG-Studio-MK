using System.Collections.Generic;
using RPGStudioMK.Game;
namespace RPGStudioMK.Widgets;

public class TilesetPickerWindow : PopupWindow
{
    public bool Apply = true;
    public Tileset Tileset;

    ListBox TilesetList;
    Container PreviewContainer;

    List<ImageBox> TilesetPreviewBoxes = new List<ImageBox>();

    public TilesetPickerWindow(Tileset? defaultTileset, bool ShowIDs = true)
    {
        SetTitle("Tileset Picker");
        MinimumSize = MaximumSize = new Size(600, 490);
        SetSize(MaximumSize);
        Center();

        Label pickerlabel = new Label(this);
        pickerlabel.SetText("Tilesets");
        pickerlabel.SetPosition(18, 34);
        pickerlabel.SetFont(Fonts.Paragraph);
        TilesetList = new ListBox(this);
        TilesetList.SetPosition(25, 56);
        TilesetList.SetSize(151, 380);
        List<TreeNode> items = new List<TreeNode>();
        for (int i = 1; i < Data.Tilesets.Count; i++)
        {
            Tileset tileset = Data.Tilesets[i];
            string Name = ShowIDs ? $"{Utilities.Digits(i, 2)}: {tileset.Name}" : tileset.Name;
            items.Add(new TreeNode(Name, tileset));
        }
        TilesetList.SetItems(items);
        TilesetList.OnSelectionChanged += _ => UpdatePreview();

        Label previewlabel = new Label(this);
        previewlabel.SetText("Preview");
        previewlabel.SetPosition(192, 34);
        previewlabel.SetFont(Fonts.Paragraph);

        ColoredBox outline = new ColoredBox(this);
        outline.SetPosition(194, 56);
        outline.SetSize(380, 380);
        outline.SetOuterColor(59, 91, 124);
        outline.SetInnerColor(24, 38, 53);

        PreviewContainer = new Container(outline);
        PreviewContainer.SetDocked(true);
        PreviewContainer.SetPadding(3, 3, 10, 3);
        PreviewContainer.SetBackgroundColor(17, 27, 38);

        VScrollBar vs = new VScrollBar(outline);
        vs.SetVDocked(true);
        vs.SetPadding(0, 3, 1, 3);
        vs.SetRightDocked(true);
        PreviewContainer.SetVScrollBar(vs);
        PreviewContainer.VAutoScroll = true;

        CreateButton("Cancel", _ => Cancel());
        CreateButton("OK", _ => OK());

        RegisterShortcuts(new List<Shortcut>()
        {
            new Shortcut(this, new Key(Keycode.ENTER, Keycode.CTRL), _ => OK(), true)
        });

        if (defaultTileset is not null)
        {
            int idx = TilesetList.Items.FindIndex(item => (Tileset) item.Object == defaultTileset);
            if (idx >= 0) TilesetList.SetSelectedIndex(idx);
            else if (TilesetList.Items.Count > 0) TilesetList.SetSelectedIndex(0);
        }
        else if (TilesetList.Items.Count > 0) TilesetList.SetSelectedIndex(0);
    }

    public void UpdatePreview()
    {
        Tileset? ts = TilesetList.SelectedItem.Object as Tileset;
        PreviewContainer.ScrolledY = 0;
        TilesetPreviewBoxes.ForEach(b => b.Dispose());
        TilesetPreviewBoxes.Clear();
        if (ts == null) return;
        if (ts.TilesetListBitmap.IsChunky)
        {
            int y = 0;
            foreach (Bitmap b in ts.TilesetListBitmap.InternalBitmaps)
            {
                ImageBox img = new ImageBox(PreviewContainer);
                img.SetPosition(0, y);
                img.SetBitmap(b);
                if (!b.Locked) b.Lock();
                img.SetDestroyBitmap(false);
                img.SetFillMode(FillMode.CenterX);
                y += b.Height;
                TilesetPreviewBoxes.Add(img);
            }
        }
        else
        {
            ImageBox img = new ImageBox(PreviewContainer);
            img.SetBitmap(ts.TilesetListBitmap);
            img.SetDestroyBitmap(false);
            img.SetFillMode(FillMode.CenterX);
            TilesetPreviewBoxes.Add(img);
        }
    }

    public void OK()
    {
        if (TilesetList.SelectedIndex >= 0)
        {
            Tileset tileset = (Tileset) TilesetList.SelectedItem.Object;
            this.Tileset = tileset;
        }
        else
        {
            this.Tileset = null;
        }
        Close();
    }

    public void Cancel()
    {
        this.Tileset = null;
        this.Apply = false;
        Close();
    }
}
