using System.Collections.Generic;
using System.IO;
using RPGStudioMK.Game;
namespace RPGStudioMK.Widgets;

public class CharacterGraphicPickerWindow : PopupWindow
{
    public bool Apply = true;

    Map map;
    EventGraphic eventGraphic;

    ListBox ListBox;
    ImageBox ImageBox;
    CursorWidget CursorWidget;
    Label OpacityLabel;
    NumericBox OpacityBox;
    Label BlendingLabel;
    DropdownBox BlendingBox;
    Label HueLabel;
    NumericSlider HueSlider;

    public CharacterGraphicPickerWindow(Map map, EventGraphic eventGraphic)
    {
        this.map = map;
        this.eventGraphic = eventGraphic;

        SetTitle("Choose Graphic");
        MinimumSize = MaximumSize = new Size(640, 600);
        SetSize(MaximumSize);
        Center();

        ListBox = new ListBox(this);
        ListBox.SetPosition(25, 44);
        ListBox.SetSize(169, 505);
        List<TreeNode> items = new List<TreeNode>()
        {
            new TreeNode("(Blank)", ":blank"),
            new TreeNode("(Tileset)", ":tileset")
        };
        foreach (string filename in Directory.GetFiles(Data.ProjectPath + "/Graphics/Characters"))
        {
            if (!filename.ToLower().EndsWith(".png")) continue;
            string fileNoExt = Path.GetFileNameWithoutExtension(filename);
            items.Add(new TreeNode(fileNoExt, filename));
        }
        ListBox.SetItems(items);
        ListBox.OnSelectionChanged += _ => UpdatePreview();

        ScrollBox outline = new ScrollBox(this);
        outline.SetPosition(206, 44);
        outline.SetSize(411, 373);

        ImageBox = new ImageBox(outline);

        OpacityLabel = new Label(this);
        OpacityLabel.SetText("Opacity:");
        OpacityLabel.SetPosition(206, 429);
        OpacityLabel.SetFont(Fonts.Paragraph);

        OpacityBox = new NumericBox(this);
        OpacityBox.SetPosition(206, 453);
        OpacityBox.SetSize(124, 30);
        OpacityBox.SetValue(eventGraphic.Opacity);
        OpacityBox.SetMinValue(0);
        OpacityBox.SetMaxValue(255);
        OpacityBox.OnValueChanged += _ => ImageBox.SetOpacity((byte) OpacityBox.Value);

        BlendingLabel = new Label(this);
        BlendingLabel.SetText("Blending:");
        BlendingLabel.SetPosition(357, 429);
        BlendingLabel.SetFont(Fonts.Paragraph);

        BlendingBox = new DropdownBox(this);
        BlendingBox.SetPosition(357, 453);
        BlendingBox.SetSize(134, 24);
        BlendingBox.SetItems(new List<TreeNode>()
        {
            new TreeNode("Normal"),
            new TreeNode("Subtract"),
            new TreeNode("Add")
        });

        HueLabel = new Label(this);
        HueLabel.SetText("Hue:");
        HueLabel.SetPosition(206, 497);
        HueLabel.SetFont(Fonts.Paragraph);

        HueSlider = new NumericSlider(this);
        HueSlider.SetPosition(206, 521);
        HueSlider.SetSize(411, 17);
		HueSlider.SetMinimumValue(0);
		HueSlider.SetMaximumValue(359);
		HueSlider.SetSnapValues(0, 59, 119, 179, 239, 299, 359);
		HueSlider.SetSnapStrength(16);
        HueSlider.SetValue(eventGraphic.CharacterHue);
        HueSlider.OnValueChanged += _ => UpdatePreview(false);

		CreateButton("Cancel", _ => Cancel());
        CreateButton("OK", _ => OK());

        RegisterShortcuts(new List<Shortcut>()
        {
            new Shortcut(this, new Key(Keycode.ENTER, Keycode.CTRL), _ => OK(), true)
        });

        // Select current graphic
        if (!string.IsNullOrEmpty(eventGraphic.CharacterName))
        {
            TreeNode? node = ListBox.Items.Find(n => n.Text == eventGraphic.CharacterName);
            if (node is not null) ListBox.SetSelectedItem(node);
            else ListBox.SetSelectedIndex(0);
        }
        else if (eventGraphic.TileID != 0)
        {
            ListBox.SetSelectedIndex(1);
            // Select tile
        }
    }

    public void UpdatePreview(bool updateScroll = true)
    {
        if (ImageBox.DestroyBitmap) ImageBox.Bitmap?.Dispose();
        ImageBox.ClearBitmap();
        if (updateScroll) ImageBox.UpdateSize();
        if ((string) ListBox.SelectedItem.Object == ":blank") return;
        if ((string) ListBox.SelectedItem.Object == ":tileset")
        {
            Tileset tileset = Data.Tilesets[map.TilesetIDs[0]];
            ImageBox.SetBitmap(tileset.TilesetBitmap);
			ImageBox.SetDestroyBitmap(false);
			if (HueSlider.Value != 0)
            {
                ImageBox.SetBitmap(tileset.TilesetBitmap.ApplyHue(HueSlider.Value));
				ImageBox.SetDestroyBitmap(true);
			}
            return;
        }
        ImageBox.SetDestroyBitmap(true);
        Bitmap bmp = new Bitmap((string) ListBox.SelectedItem.Object);
        if (HueSlider.Value != 0)
        {
            Bitmap hueBmp = bmp.ApplyHue(HueSlider.Value);
            bmp.Dispose();
            bmp = hueBmp;
        }
        ImageBox.SetBitmap(bmp);
    }
    
    public void OK()
    {
        // Apply data to this.eventGraphic.
        //if (ListBox.SelectedIndex >= 0)
        //{
        //    Tileset tileset = (Tileset) ListBox.SelectedItem.Object;
        //    this.Filename = ListBox.
        //}
        //else
        //{
        //    this.Tileset = null;
        //}
        Close();
    }
    
    public void Cancel()
    {
        this.Apply = false;
        Close();
    }
}
