using System;
using System.Collections.Generic;
using System.IO;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class ChooseGraphic : PopupWindow
{
    public bool Apply = false;

    Map Map;
    Event Event;
    EventPage Page;
    EventGraphic Graphic;

    Container GraphicContainer;
    FileExplorer FileExplorer;
    TileGraphicPicker TileGraphicPicker;
    PictureBox GraphicBox;
    CursorWidget Cursor;
    Label TypeLabel;
    DropdownBox DirectionBox;
    DropdownBox NumDirectionsBox;
    NumericBox NumFramesBox;

    public ChooseGraphic(Map Map, Event Event, EventPage Page, EventGraphic gr)
    {
        this.Map = Map;
        this.Event = Event;
        this.Page = Page;
        this.Graphic = (EventGraphic) gr.Clone();
        
        SetTitle("Choose Graphic");
        MinimumSize = MaximumSize = new Size(735, 454);
        SetSize(MaximumSize);
        Center();

        GroupBox GraphicGroupBox = new GroupBox(this);
        GraphicGroupBox.SetPosition(529, 35);
        GraphicGroupBox.SetSize(198, 410);
        GraphicGroupBox.SetInnerColor(new Color(28, 50, 73));

        Label GraphicLabel = new Label(this);
        GraphicLabel.SetFont(Fonts.UbuntuBold.Use(16));
        GraphicLabel.SetText("Current Graphic");
        GraphicLabel.SetPosition(559, 38);

        Button ClearGraphicButton = new Button(this);
        ClearGraphicButton.SetFont(Fonts.UbuntuBold.Use(16));
        ClearGraphicButton.SetPosition(588, 58);
        ClearGraphicButton.SetSize(80, 32);
        ClearGraphicButton.SetText("Clear");
        
        Widget GraphicContainerGroupBox = new Widget(this);
        GraphicContainerGroupBox.SetPosition(542, 91);
        GraphicContainerGroupBox.SetSize(177, 177);
        GraphicContainerGroupBox.SetBackgroundColor(Color.GREEN);
        Color outline = new Color(59, 91, 124);
        Color inline = new Color(17, 27, 38);
        Color filler = new Color(24, 38, 53);
        GraphicContainerGroupBox.Sprites["gfxbox"] = new Sprite(GraphicContainerGroupBox.Viewport);
        GraphicContainerGroupBox.Sprites["gfxbox"].Bitmap = new Bitmap(177, 177);
        GraphicContainerGroupBox.Sprites["gfxbox"].Bitmap.Unlock();
        GraphicContainerGroupBox.Sprites["gfxbox"].Bitmap.DrawRect(0, 0, 177, 177, outline);
        GraphicContainerGroupBox.Sprites["gfxbox"].Bitmap.DrawRect(1, 1, 175, 175, inline);
        GraphicContainerGroupBox.Sprites["gfxbox"].Bitmap.FillRect(2, 2, 173, 173, filler);
        GraphicContainerGroupBox.Sprites["gfxbox"].Bitmap.FillRect(163, 163, 13, 13, outline);
        GraphicContainerGroupBox.Sprites["gfxbox"].Bitmap.DrawLine(162, 1, 162, 162, inline);
        GraphicContainerGroupBox.Sprites["gfxbox"].Bitmap.DrawLine(163, 1, 163, 162, outline);
        GraphicContainerGroupBox.Sprites["gfxbox"].Bitmap.DrawLine(164, 1, 164, 162, inline);
        GraphicContainerGroupBox.Sprites["gfxbox"].Bitmap.DrawLine(165, 162, 174, 162, inline);
        GraphicContainerGroupBox.Sprites["gfxbox"].Bitmap.DrawLine(1, 162, 162, 162, inline);
        GraphicContainerGroupBox.Sprites["gfxbox"].Bitmap.DrawLine(1, 163, 162, 163, outline);
        GraphicContainerGroupBox.Sprites["gfxbox"].Bitmap.DrawLine(1, 164, 162, 164, inline);
        GraphicContainerGroupBox.Sprites["gfxbox"].Bitmap.DrawLine(162, 165, 162, 174, inline);
        GraphicContainerGroupBox.Sprites["gfxbox"].Bitmap.Lock();

        GraphicContainer = new Container(this);
        GraphicContainer.SetPosition(544, 93);
        GraphicContainer.SetSize(160, 160);
        GraphicContainer.HAutoScroll = true;
        GraphicContainer.VAutoScroll = true;
        GraphicContainer.OnLeftMouseDownInside += LeftDownInsideGraphicBox;
        GraphicContainer.OnDoubleLeftMouseDownInside += DoubleLeftDownInsideGraphicBox;
        VScrollBar vs = new VScrollBar(this);
        vs.SetPosition(708, 94);
        vs.SetSize(10, 158);
        GraphicContainer.SetVScrollBar(vs);
        HScrollBar hs = new HScrollBar(this);
        hs.SetPosition(545, 257);
        hs.SetSize(158, 10);
        GraphicContainer.SetHScrollBar(hs);

        GraphicBox = new PictureBox(GraphicContainer);

        Cursor = new CursorWidget(GraphicContainer);
        Cursor.ConsiderInAutoScrollCalculation = false;

        Font f = Fonts.ProductSansMedium.Use(12);

        TypeLabel = new Label(this);
        TypeLabel.SetPosition(547, 276);
        TypeLabel.SetFont(f);
        TypeLabel.SetText($"Type: {(Graphic.TileID >= 384 ? "Tile" : !string.IsNullOrEmpty(Graphic.CharacterName) ? "Character" : "None")}");

        Label DirectionLabel = new Label(this);
        DirectionLabel.SetFont(f);
        DirectionLabel.SetText("Direction:");
        DirectionLabel.SetPosition(547, 301);
        DirectionLabel.SetEnabled(Graphic.TileID < 384 && !string.IsNullOrEmpty(Graphic.CharacterName));
        DirectionBox = new DropdownBox(this);
        DirectionBox.SetPosition(609, 295);
        DirectionBox.SetSize(110, 25);
        DirectionBox.SetItems(new List<ListItem>()
        {
            new ListItem("Down"),
            new ListItem("Left"),
            new ListItem("Right"),
            new ListItem("Up")
        });
        DirectionBox.SetSelectedIndex(Graphic.Direction / 2 - 1);
        DirectionBox.SetEnabled(Graphic.TileID < 384 && !string.IsNullOrEmpty(Graphic.CharacterName));
        DirectionBox.OnSelectionChanged += delegate (BaseEventArgs e)
        {
            Graphic.Direction = (DirectionBox.SelectedIndex + 1) * 2;
            RedrawGraphic();
        };

        Label NumDirectionsLabel = new Label(this);
        NumDirectionsLabel.SetFont(f);
        NumDirectionsLabel.SetText("Number of Directions:");
        NumDirectionsLabel.SetPosition(547, 340);
        NumDirectionsLabel.SetEnabled(false);
        NumDirectionsBox = new DropdownBox(this);
        NumDirectionsBox.SetPosition(679, 336);
        NumDirectionsBox.SetSize(40, 25);
        NumDirectionsBox.SetItems(new List<ListItem>()
        {
            new ListItem("1"),
            new ListItem("4")
        });
        
        NumDirectionsBox.SetSelectedIndex(Graphic.NumDirections == 1 ? 0 : 1);
        NumDirectionsBox.SetEnabled(false);
        NumDirectionsBox.OnSelectionChanged += delegate (BaseEventArgs e)
        {
            Graphic.NumDirections = NumDirectionsBox.SelectedIndex == 0 ? 1 : 4;
            RedrawGraphic();
        };

        Label NumFramesLabel = new Label(this);
        NumFramesLabel.SetFont(f);
        NumFramesLabel.SetText("Number of Frames:");
        NumFramesLabel.SetPosition(547, 380);
        NumFramesLabel.SetEnabled(false);
        NumFramesBox = new NumericBox(this);
        NumFramesBox.SetPosition(669, 375);
        NumFramesBox.SetSize(50, 27);
        NumFramesBox.MinValue = 1;
        NumFramesBox.MaxValue = 999;
        NumFramesBox.SetValue(Graphic.NumFrames);
        NumFramesBox.SetEnabled(false);
        NumFramesBox.OnValueChanged += delegate (BaseEventArgs e)
        {
            Graphic.NumFrames = NumFramesBox.Value;
            RedrawGraphic();
        };

        SubmodeView GraphicTypePicker = new SubmodeView(this);
        GraphicTypePicker.SetHDocked(true);
        GraphicTypePicker.SetPosition(128, 8);
        GraphicTypePicker.SetHeaderSelBackgroundColor(new Color(59, 91, 124));
        GraphicTypePicker.SetHeaderHeight(28);
        GraphicTypePicker.SetHeight(25);
        GraphicTypePicker.CreateTab("File");
        GraphicTypePicker.CreateTab("Tiles");
        GraphicTypePicker.OnSelectionChanged += _ =>
        {
            FileExplorer.SetVisible(GraphicTypePicker.SelectedIndex == 0);
            TileGraphicPicker.SetVisible(GraphicTypePicker.SelectedIndex == 1);
        };

        Widget Divider = new Widget(this);
        Divider.SetHDocked(true);
        Divider.SetMargins(8, 32, 8, 0);
        Divider.SetHeight(3);
        Divider.SetBackgroundColor(new Color(59, 91, 124));

        FileExplorer = new FileExplorer(this);
        FileExplorer.SetPosition(8, 35);
        FileExplorer.SetSize(522, 410);
        FileExplorer.SetBaseDirectory(Data.ProjectPath);
        FileExplorer.SetFileExtensions("png");
        FileExplorer.SetDirectory("Graphics/Characters");
        FileExplorer.OnFileSelected += _ =>
        {
            string charname = FileExplorer.SelectedFilename.Replace(Data.ProjectPath + "/Graphics/Characters/", "").Replace(".png", "");
            if (this.Graphic.CharacterName != charname)
            {
                this.Graphic.CharacterName = charname;
                this.Graphic.TileID = 0;
                this.Graphic.NumFrames = 4;
                this.Graphic.NumDirections = 4;
                TypeLabel.SetText("Type: Character");
                DirectionLabel.SetEnabled(true);
                DirectionBox.SetEnabled(true);
                DirectionBox.SetSelectedIndex(0);
                NumDirectionsBox.SetSelectedIndex(1);
                NumFramesBox.SetValue(4);
                RedrawGraphic();
                TileGraphicPicker.HideCursor();
            }
        };

        TileGraphicPicker = new TileGraphicPicker(new Size(Event.Width, Event.Height), Map, Graphic, this);
        TileGraphicPicker.SetPosition(8, 35);
        TileGraphicPicker.SetSize(522, 410);
        TileGraphicPicker.SetVisible(false);
        TileGraphicPicker.OnTileSelected += _ =>
        {
            this.Graphic.CharacterName = "";
            this.Graphic.TileID = TileGraphicPicker.TileID;
            this.Graphic.NumFrames = 1;
            this.Graphic.NumDirections = 1;
            this.Graphic.Direction = 2;
            TypeLabel.SetText("Type: Tile");
            DirectionLabel.SetEnabled(false);
            DirectionBox.SetEnabled(false);
            NumDirectionsBox.SetSelectedIndex(0);
            NumFramesBox.SetValue(1);
            RedrawGraphic();
            FileExplorer.SetSelectedFile(null);
        };
        TileGraphicPicker.OnTileDoubleClicked += _ => OK(new BaseEventArgs());

        ClearGraphicButton.OnClicked += _ =>
        {
            this.Graphic.CharacterName = "";
            this.Graphic.TileID = 0;
            TypeLabel.SetText("Type: None");
            DirectionLabel.SetEnabled(false);
            DirectionBox.SetEnabled(false);
            RedrawGraphic();
            TileGraphicPicker.HideCursor();
            FileExplorer.SetSelectedFile(null);
        };

        CreateButton("Cancel", Cancel);
        Buttons[0].SetPosition(Size.Width - 99, Buttons[0].Position.Y);
        CreateButton("OK", OK);
        Buttons[1].SetPosition(Size.Width - 188, Buttons[1].Position.Y);

        if (!string.IsNullOrEmpty(Graphic.CharacterName)) FileExplorer.SetSelectedFile(Graphic.CharacterName + ".png");

        RedrawGraphic();
    }

    public void Cancel(BaseEventArgs e)
    {
        Close();
    }

    public void OK(BaseEventArgs e)
    {
        Page.Graphic = this.Graphic;
        Apply = true;
        Close();
    }

    public void RedrawGraphic()
    {
        if (FileExplorer.SelectedIsFolder) return;
        if (GraphicBox.Sprite.DestroyBitmap) GraphicBox.Sprite.Bitmap?.Dispose();
        else GraphicBox.Sprite.Bitmap = null;
        GraphicBox.Sprite.Y = 0;
        if (Graphic.TileID >= 384)
        {
            Tileset Tileset = Data.Tilesets[Map.TilesetIDs[0]];
            if (Tileset.TilesetBitmap != null)
            {
                GraphicBox.Sprite.Bitmap = Tileset.TilesetBitmap;
                GraphicBox.Sprite.DestroyBitmap = false;
                int tx = (Graphic.TileID - 384) % 8;
                int ty = (Graphic.TileID - 384) / 8;
                GraphicBox.Sprite.SrcRect.X = tx * 32;
                GraphicBox.Sprite.SrcRect.Y = ty * 32 - (Event.Height - 1) * 32;
                GraphicBox.Sprite.SrcRect.Width = Event.Width * 32;
                GraphicBox.Sprite.SrcRect.Height = Event.Height * 32;
                if (GraphicBox.Sprite.SrcRect.Y < 0)
                {
                    GraphicBox.Sprite.SrcRect.Height += GraphicBox.Sprite.SrcRect.Y;
                    GraphicBox.Sprite.Y = -GraphicBox.Sprite.SrcRect.Y;
                    GraphicBox.Sprite.SrcRect.Y = 0;
                }
                if (GraphicBox.Sprite.SrcRect.X + GraphicBox.Sprite.SrcRect.Width >= GraphicBox.Sprite.Bitmap.Width)
                {
                    GraphicBox.Sprite.SrcRect.Width = GraphicBox.Sprite.Bitmap.Width - GraphicBox.Sprite.SrcRect.X;
                }
            }
        }
        else if (!string.IsNullOrEmpty(Graphic.CharacterName))
        {
            GraphicBox.Sprite.Bitmap = new Bitmap(Path.Combine(Data.ProjectPath, "Graphics/Characters", Graphic.CharacterName));
            GraphicBox.Sprite.DestroyBitmap = true;
        }
        else
        {
            Cursor.SetVisible(false);
            return;
        }
        int x = 0;
        int y = 0;
        int sw = GraphicBox.Sprite.SrcRect.Width;
        int sh = GraphicBox.Sprite.SrcRect.Height;
        if (Graphic.TileID >= 384)
        {
            sw = Event.Width * 32;
            sh = Event.Height * 32;
        }
        int w = sw / Graphic.NumFrames;
        int h = sh / Graphic.NumDirections;
        if (Graphic.NumDirections == 4) y = h * (Graphic.Direction / 2 - 1);
        if (Graphic.NumDirections == 8) y = h * (Graphic.Direction - 1);
        Cursor.SetPosition(x - 7, y - 7);
        Cursor.SetSize(w + 14, h + 14);
        Cursor.SetVisible(true);
    }

    private void LeftDownInsideGraphicBox(MouseEventArgs e)
    {
        if (e.Handled) return;
        if (Graphic.TileID < 384 && string.IsNullOrEmpty(Graphic.CharacterName)) return;
        int rx = e.X - GraphicContainer.Viewport.X + GraphicBox.LeftCutOff;
        int ry = e.Y - GraphicContainer.Viewport.Y + GraphicBox.TopCutOff;
        int Frame = Math.Clamp((int) Math.Floor((double) rx / GraphicBox.Sprite.Bitmap.Width * Graphic.NumFrames), 0, Graphic.NumFrames - 1);
        int mindir = Graphic.NumDirections == 8 ? 1 : 2;
        int maxdir = Graphic.NumDirections * 2;
        int Direction = Math.Clamp(((int) Math.Floor((double) ry / GraphicBox.Sprite.Bitmap.Height * Graphic.NumDirections) + 1) * 2, mindir, maxdir);
        DirectionBox.SetSelectedIndex(Direction / 2 - 1);
    }

    private void DoubleLeftDownInsideGraphicBox(MouseEventArgs e)
    {
        OK(new BaseEventArgs());
        e.Handled = true;
    }
}
