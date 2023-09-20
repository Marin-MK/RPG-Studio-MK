using System.Collections.Generic;
using System.IO;
using RPGStudioMK.Game;
namespace RPGStudioMK.Widgets;

public class CharacterGraphicPickerWindow : PopupWindow
{
    public bool Apply = true;

    Map map;
    Event mapEvent;
    EventGraphic eventGraphic;
    int tileID;
    int direction;
    int pattern;

    ListBox ListBox;
    ImageBox ImageBox;
    CursorWidget CursorWidget;
    Label OpacityLabel;
    NumericBox OpacityBox;
    Label BlendingLabel;
    DropdownBox BlendingBox;
    Label HueLabel;
    NumericSlider HueSlider;

    public CharacterGraphicPickerWindow(Map map, Event mapEvent, EventGraphic eventGraphic)
    {
        this.map = map;
        this.mapEvent = mapEvent;
        this.eventGraphic = eventGraphic;
        this.tileID = eventGraphic.TileID > 0 ? eventGraphic.TileID - 384 : 0;
        this.direction = eventGraphic.Direction;
        this.pattern = eventGraphic.Pattern;

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
        ImageBox.OnLeftMouseDownInside += ClickedPreview;
        ImageBox.OnDoubleLeftMouseDownInside += _ => OK();

        CursorWidget = new CursorWidget(outline);
        CursorWidget.SetVisible(false);
        CursorWidget.ConsiderInAutoScrollCalculation = false;

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
            new TreeNode("Add"),
            new TreeNode("Sub")
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
        }

        UpdatePreview();
    }

    public void UpdatePreview(bool updateScroll = true)
    {
        if (ImageBox.DestroyBitmap) ImageBox.Bitmap?.Dispose();
        ImageBox.ClearBitmap();
        if (updateScroll) ImageBox.UpdateSize();
		CursorWidget.SetVisible(false);
		if ((string) ListBox.SelectedItem.Object == ":blank") return;
        CursorWidget.SetVisible(true);
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
            UpdateCursor();
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
        UpdateCursor();
    }

    private void UpdateCursor()
    {
        if ((string) ListBox.SelectedItem.Object == ":tileset")
        {
            int mx = this.tileID % 8;
            int my = this.tileID / 8;
            CursorWidget.SetPosition(-7 + 32 * mx, -7 + 32 * (my - mapEvent.Height + 1));
            CursorWidget.SetSize(14 + 32 * mapEvent.Width, 14 + 32 * mapEvent.Height);
            int diffX = (mx + mapEvent.Width - 1) - 7;
            if (diffX > 0) CursorWidget.SetWidth(14 + 32 * (mapEvent.Width - diffX));
            int diffY = my - mapEvent.Height + 1;
            if (diffY < 0)
            {
                CursorWidget.SetPosition(CursorWidget.Position.X, -7);
                CursorWidget.SetHeight(14 + 32 * (mapEvent.Height + diffY));
            }
            return;
        }
		int fw = ImageBox.Bitmap.Width / 4;
		int fh = ImageBox.Bitmap.Height / 4;
		CursorWidget.SetPosition(-7 + this.pattern * fw, -7 + (this.direction / 2 - 1) * fh);
		CursorWidget.SetSize(14 + fw, 14 + fh);
	}

    private void ClickedPreview(MouseEventArgs e)
    {
        int rx = e.X - ImageBox.Viewport.X + ImageBox.LeftCutOff;
        int ry = e.Y - ImageBox.Viewport.Y + ImageBox.TopCutOff;
        if (rx < 0 || rx >= ImageBox.Bitmap.Width || ry < 0 || ry >= ImageBox.Bitmap.Height) return;
        if ((string) ListBox.SelectedItem.Object == ":blank") return;
        if ((string) ListBox.SelectedItem.Object == ":tileset")
        {
            int mx = rx / 32;
            int my = ry / 32;
            this.tileID = my * 8 + mx;
            UpdateCursor();
            return;
        }
		int fw = ImageBox.Bitmap.Width / 4;
		int fh = ImageBox.Bitmap.Height / 4;
        this.pattern = rx / fw;
        this.direction = 2 * (ry / fh + 1);
        UpdateCursor();
	}
    
    public void OK()
    {
        this.eventGraphic.CharacterHue = HueSlider.Value;
        this.eventGraphic.Opacity = OpacityBox.Value;
        this.eventGraphic.BlendType = BlendingBox.SelectedIndex;
        if ((string) ListBox.SelectedItem.Object == ":blank")
        {
            this.eventGraphic.TileID = 0;
            this.eventGraphic.CharacterName = "";
			this.eventGraphic.NumFrames = 1;
			this.eventGraphic.NumDirections = 1;
            this.eventGraphic.Pattern = 0;
            this.eventGraphic.Direction = 2;
		}
        else if ((string) ListBox.SelectedItem.Object == ":tileset")
        {
            this.eventGraphic.TileID = 384 + this.tileID;
            this.eventGraphic.CharacterName = "";
			this.eventGraphic.NumFrames = 1;
			this.eventGraphic.NumDirections = 1;
            this.eventGraphic.Pattern = 0;
            this.eventGraphic.Direction = 2;
		}
        else
        {
            this.eventGraphic.TileID = 0;
            this.eventGraphic.CharacterName = ListBox.SelectedItem.Text;
			this.eventGraphic.NumFrames = 4;
			this.eventGraphic.NumDirections = 4;
            this.eventGraphic.Pattern = this.pattern;
            this.eventGraphic.Direction = this.direction;
		}
        Close();
    }
    
    public void Cancel()
    {
        this.Apply = false;
        Close();
    }
}
