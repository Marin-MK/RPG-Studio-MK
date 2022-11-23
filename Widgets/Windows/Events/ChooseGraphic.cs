using System;
using System.Collections.Generic;
using System.IO;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class ChooseGraphic : PopupWindow
{
    public bool Apply = false;
    public EventGraphic Graphic;

    Map Map;
    Event Event;
    EventPage Page;
    EventGraphic OldGraphic;

    Container GraphicContainer;
    FileExplorer FileExplorer;
    TileGraphicPicker TileGraphicPicker;
    ImageBox GraphicBox;
    CursorWidget Cursor;
    Label TypeLabel;
    DropdownBox DirectionBox;
    NumericBox FrameBox;
    DropdownBox NumDirectionsBox;
    NumericBox NumFramesBox;
    NumericBox OpacityBox;
    NumericSlider HueBox;

    bool CursorOnly = false;

    public ChooseGraphic(Map Map, Event Event, EventPage Page, EventGraphic gr, bool FromMoveRouteEditor, string RelativeFolder)
    {
        this.Map = Map;
        this.Event = Event;
        this.Page = Page;
        this.OldGraphic = gr;
        this.Graphic = (EventGraphic) gr.Clone();
        
        SetTitle("Choose Graphic");
        MinimumSize = MaximumSize = new Size(735, 504);
        SetSize(MaximumSize);
        Center();

        GroupBox GraphicGroupBox = new GroupBox(this);
        GraphicGroupBox.SetPosition(529, 35);
        GraphicGroupBox.SetSize(198, 460);
        GraphicGroupBox.SetInnerColor(new Color(28, 50, 73));

        Label GraphicLabel = new Label(this);
        GraphicLabel.SetFont(Fonts.Header);
        GraphicLabel.SetText("Current Graphic");
        GraphicLabel.SetPosition(559, 38);

        Button ClearGraphicButton = new Button(this);
        ClearGraphicButton.SetPosition(588, 58);
        ClearGraphicButton.SetSize(80, 32);
        ClearGraphicButton.SetText("Clear");
        
        GroupBoxWithScrollBars GraphicContainerGroupBox = new GroupBoxWithScrollBars(this);
        GraphicContainerGroupBox.SetPosition(542, 91);
        GraphicContainerGroupBox.SetSize(177, 177);

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

        GraphicBox = new ImageBox(GraphicContainer);

        Cursor = new CursorWidget(GraphicContainer);
        Cursor.ConsiderInAutoScrollCalculation = false;

        TypeLabel = new Label(this);
        TypeLabel.SetPosition(547, 276);
        TypeLabel.SetFont(Fonts.Paragraph);
        TypeLabel.SetText($"Type: {(Graphic.TileID >= 384 ? "Tile" : !string.IsNullOrEmpty(Graphic.CharacterName) ? "Character" : "None")}");

        Label DirectionLabel = new Label(this);
        DirectionLabel.SetFont(Fonts.Paragraph);
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

        Label FrameLabel = new Label(this);
        FrameLabel.SetFont(Fonts.Paragraph);
        FrameLabel.SetText("Frame:");
        FrameLabel.SetPosition(547, 330);
        FrameLabel.SetEnabled(Graphic.TileID < 384 && !string.IsNullOrEmpty(Graphic.CharacterName));
        FrameBox = new NumericBox(this);
        FrameBox.SetPosition(629, 325);
        FrameBox.SetSize(90, 27);
        FrameBox.SetMinValue(1);
        FrameBox.SetMaxValue(Graphic.NumFrames);
        FrameBox.SetValue(Graphic.Pattern + 1);
        FrameBox.SetEnabled(Graphic.TileID < 384 && !string.IsNullOrEmpty(Graphic.CharacterName));
        FrameBox.OnValueChanged += delegate (BaseEventArgs e)
        {
            Graphic.Pattern = FrameBox.Value - 1;
            RedrawGraphic();
        };

        Label NumDirectionsLabel = new Label(this);
        NumDirectionsLabel.SetFont(Fonts.Paragraph);
        NumDirectionsLabel.SetText("No. Directions:");
        NumDirectionsLabel.SetPosition(547, 360);
        NumDirectionsLabel.SetEnabled(false);
        NumDirectionsBox = new DropdownBox(this);
        NumDirectionsBox.SetPosition(679, 356);
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
        NumFramesLabel.SetFont(Fonts.Paragraph);
        NumFramesLabel.SetText("No. Frames:");
        NumFramesLabel.SetPosition(547, 390);
        NumFramesLabel.SetEnabled(false);
        NumFramesBox = new NumericBox(this);
        NumFramesBox.SetPosition(629, 385);
        NumFramesBox.SetSize(90, 27);
        NumFramesBox.SetMinValue(1);
        NumFramesBox.SetMaxValue(999);
        NumFramesBox.SetValue(Graphic.NumFrames);
        NumFramesBox.SetEnabled(false);
        NumFramesBox.OnValueChanged += delegate (BaseEventArgs e)
        {
            Graphic.NumFrames = NumFramesBox.Value;
            FrameBox.SetMaxValue(Graphic.NumFrames);
            if (FrameBox.Value > FrameBox.MaxValue) FrameBox.SetValue(FrameBox.MaxValue);
            RedrawGraphic();
        };

        Label OpacityLabel = new Label(this);
        OpacityLabel.SetFont(Fonts.Paragraph);
        OpacityLabel.SetText("Opacity:");
        OpacityLabel.SetPosition(547, 420);
        OpacityLabel.SetEnabled(!FromMoveRouteEditor);
        OpacityBox = new NumericBox(this);
        OpacityBox.SetPosition(629, 416);
        OpacityBox.SetSize(90, 27);
        OpacityBox.SetMinValue(0);
        OpacityBox.SetMaxValue(255);
        OpacityBox.SetValue(Graphic.Opacity);
        OpacityBox.SetEnabled(!FromMoveRouteEditor);
        OpacityBox.OnValueChanged += delegate (BaseEventArgs e)
        {
            Graphic.Opacity = OpacityBox.Value;
            GraphicBox.SetOpacity((byte) Graphic.Opacity);
        };

        SubmodeView GraphicTypePicker = new SubmodeView(this);
        GraphicTypePicker.SetFont(Fonts.TabFont);
        GraphicTypePicker.SetHDocked(true);
        GraphicTypePicker.SetPosition(144, 8);
        GraphicTypePicker.SetHeaderSelBackgroundColor(new Color(59, 91, 124));
        GraphicTypePicker.SetHeaderHeight(28);
        GraphicTypePicker.SetHeight(25);
        GraphicTypePicker.CreateTab("File");
        if (!FromMoveRouteEditor) GraphicTypePicker.CreateTab("Tiles");
        GraphicTypePicker.OnSelectionChanged += _ =>
        {
            FileExplorer.SetVisible(GraphicTypePicker.SelectedIndex == 0);
            TileGraphicPicker.SetVisible(GraphicTypePicker.SelectedIndex == 1);
        };

        Widget Divider = new Widget(this);
        Divider.SetHDocked(true);
        Divider.SetPadding(8, 32, 8, 0);
        Divider.SetHeight(3);
        Divider.SetBackgroundColor(new Color(59, 91, 124));

        FileExplorer = new FileExplorer(this);
        FileExplorer.SetPosition(8, 35);
        FileExplorer.SetSize(522, 430);
        string path = Data.ProjectPath;
        if (!string.IsNullOrEmpty(RelativeFolder)) path += "/" + RelativeFolder;
        FileExplorer.SetBaseDirectory(path);
        FileExplorer.SetFileExtensions("png");
        while (Graphic.CharacterName.Contains('\\')) Graphic.CharacterName = Graphic.CharacterName.Replace('\\', '/');
        if (Graphic.CharacterName.Contains("/"))
        {
            string folder = Graphic.CharacterName.Substring(0, Graphic.CharacterName.LastIndexOf('/'));
            FileExplorer.SetDirectory(folder);
        }
        else FileExplorer.RedrawDirectory();

        TileGraphicPicker = new TileGraphicPicker(new Size(Event.Width, Event.Height), Map, Graphic, this);
        TileGraphicPicker.SetPosition(8, 35);
        TileGraphicPicker.SetSize(522, 430);
        TileGraphicPicker.SetVisible(false);
        TileGraphicPicker.OnTileDoubleClicked += _ => OK();

        Label HueLabel = new Label(this);
        HueLabel.SetBottomDocked(true);
        HueLabel.SetPadding(20, 0, 0, -17);
        HueLabel.SetHeight(24);
        HueLabel.SetFont(Fonts.Paragraph);
        HueLabel.SetText("Hue: ");
        HueBox = new NumericSlider(this);
        HueBox.SetMinimumValue(0);
        HueBox.SetMaximumValue(359);
        HueBox.SetSnapValues(0, 59, 119, 179, 239, 299, 359);
        HueBox.SetSnapStrength(16);
        HueBox.SetHDocked(true);
        HueBox.SetBottomDocked(true);
        HueBox.SetPadding(50, 0, 214, 16);
        HueBox.OnValueChanged += _ =>
        {
            Graphic.CharacterHue = HueBox.Value;
            RedrawGraphic();
        };
        HueBox.SetValue(Graphic.CharacterHue);

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
                FrameLabel.SetEnabled(true);
                FrameBox.SetEnabled(true);
                OpacityLabel.SetEnabled(!FromMoveRouteEditor);
                OpacityBox.SetEnabled(!FromMoveRouteEditor);
                HueLabel.SetEnabled(true);
                HueBox.SetEnabled(true);
                RedrawGraphic();
                TileGraphicPicker.HideCursor();
            }
        };

        TileGraphicPicker.OnTileSelected += _ =>
        {
            this.Graphic.CharacterName = "";
            this.Graphic.TileID = TileGraphicPicker.TileID;
            this.Graphic.NumFrames = 1;
            this.Graphic.NumDirections = 1;
            DirectionBox.SetSelectedIndex(0);
            TypeLabel.SetText("Type: Tile");
            DirectionLabel.SetEnabled(false);
            DirectionBox.SetEnabled(false);
            NumDirectionsBox.SetSelectedIndex(0);
            NumFramesBox.SetValue(1);
            FrameLabel.SetEnabled(false);
            FrameBox.SetEnabled(false);
            OpacityLabel.SetEnabled(!FromMoveRouteEditor);
            OpacityBox.SetEnabled(!FromMoveRouteEditor);
            HueLabel.SetEnabled(true);
            HueBox.SetEnabled(true);
            RedrawGraphic();
            FileExplorer.SetSelectedFile(null);
        };

        ClearGraphicButton.OnClicked += _ =>
        {
            this.Graphic.CharacterName = "";
            this.Graphic.TileID = 0;
            DirectionBox.SetSelectedIndex(0);
            FrameBox.SetValue(0);
            HueBox.SetValue(0);
            TypeLabel.SetText("Type: None");
            DirectionLabel.SetEnabled(false);
            DirectionBox.SetEnabled(false);
            FrameLabel.SetEnabled(false);
            FrameBox.SetEnabled(false);
            OpacityLabel.SetEnabled(false);
            OpacityBox.SetEnabled(false);
            HueLabel.SetEnabled(false);
            HueBox.SetEnabled(false);
            RedrawGraphic();
            TileGraphicPicker.HideCursor();
            FileExplorer.SetSelectedFile(null);
        };

        CreateButton("Cancel", _ => Cancel());
        Buttons[0].SetPosition(Size.Width - 99, Buttons[0].Position.Y);
        CreateButton("OK", _ => OK());
        Buttons[1].SetPosition(Size.Width - 188, Buttons[1].Position.Y);

        if (!string.IsNullOrEmpty(Graphic.CharacterName)) FileExplorer.SetSelectedFile(Graphic.CharacterName + ".png");

        RegisterShortcuts(new List<Shortcut>()
        {
            new Shortcut(this, new Key(Keycode.ENTER, Keycode.CTRL), _ => OK(), true)
        });

        RedrawGraphic();
    }

    public void Cancel()
    {
        this.Graphic = OldGraphic;
        Close();
    }

    public void OK()
    {
        Apply = true;
        Close();
    }

    public void RedrawGraphic()
    {
        if (FileExplorer.SelectedIsFolder) return;
        if (!CursorOnly)
        {
            GraphicBox.DisposeBitmap();
            GraphicBox.SetY(0);
            if (Graphic.TileID >= 384)
            {
                Tileset Tileset = Data.Tilesets[Map.TilesetIDs[0]];
                if (Tileset.TilesetBitmap != null)
                {
                    Bitmap SourceBitmap = Tileset.TilesetBitmap;
                    int tx = (Graphic.TileID - 384) % 8;
                    int ty = (Graphic.TileID - 384) / 8;
                    int srcx = tx * 32;
                    int srcy = ty * 32 - (Event.Height - 1) * 32;
                    int srcw = Event.Width * 32;
                    int srch = Event.Height * 32;
                    if (srcy < 0)
                    {
                        srch += srcy;
                        GraphicBox.SetY(-srcy);
                        srcy = 0;
                    }
                    if (srcx + srcw >= SourceBitmap.Width)
                    {
                        srcw = SourceBitmap.Width - srcx;
                    }
                    Bitmap SmallBmp = new Bitmap(srcw, srch);
                    SmallBmp.Unlock();
                    SmallBmp.Build(0, 0, SourceBitmap, new Rect(srcx, srcy, srcw, srch));
                    SmallBmp.Lock();
                    GraphicBox.SetBitmap(SmallBmp);
                    if (Graphic.CharacterHue != 0)
                    {
                        GraphicBox.SetBitmap(SmallBmp.ApplyHue(Graphic.CharacterHue));
                        SmallBmp.Dispose();
                    }
                }
            }
            else if (!string.IsNullOrEmpty(Graphic.CharacterName))
            {
                Bitmap SourceBitmap = new Bitmap(Path.Combine(Data.ProjectPath, "Graphics/Characters", Graphic.CharacterName));
                GraphicBox.SetBitmap(SourceBitmap);
                if (Graphic.CharacterHue != 0)
                {
                    GraphicBox.SetBitmap(GraphicBox.Bitmap.ApplyHue(Graphic.CharacterHue));
                    SourceBitmap.Dispose();
                }
            }
            else
            {
                Cursor.SetVisible(false);
                return;
            }
            GraphicBox.SetOpacity((byte) Graphic.Opacity);
        }
        int x = 0;
        int y = 0;
        int sw = GraphicBox.SrcRect.Width;
        int sh = GraphicBox.SrcRect.Height;
        if (Graphic.TileID >= 384)
        {
            sw = Event.Width * 32;
            sh = Event.Height * 32;
        }
        int w = sw / Graphic.NumFrames;
        int h = sh / Graphic.NumDirections;
        if (Graphic.NumDirections == 4) y = h * (Graphic.Direction / 2 - 1);
        if (Graphic.NumDirections == 8) y = h * (Graphic.Direction - 1);
        x = w * Graphic.Pattern;
        Cursor.SetPosition(x - 7, y - 7);
        Cursor.SetSize(w + 14, h + 14);
        Cursor.SetVisible(true);
        CursorOnly = false;
    }

    int OldFrameBoxValue;
    int OldDirectionBoxValue;

    private void LeftDownInsideGraphicBox(MouseEventArgs e)
    {
        if (e.Handled) return;
        if (Graphic.TileID < 384 && string.IsNullOrEmpty(Graphic.CharacterName)) return;

        int rx = e.X - GraphicContainer.Viewport.X + GraphicBox.LeftCutOff;
        int ry = e.Y - GraphicContainer.Viewport.Y + GraphicBox.TopCutOff;
        int Frame = Math.Clamp((int) Math.Floor((double) rx / GraphicBox.Bitmap.Width * Graphic.NumFrames), 0, Graphic.NumFrames - 1);
        CursorOnly = true;
        FrameBox.SetValue(Frame + 1);
        int mindir = Graphic.NumDirections == 8 ? 1 : 2;
        int maxdir = Graphic.NumDirections * 2;
        int Direction = Math.Clamp(((int) Math.Floor((double) ry / GraphicBox.Bitmap.Height * Graphic.NumDirections) + 1) * 2, mindir, maxdir);
        CursorOnly = true;
        DirectionBox.SetSelectedIndex(Direction / 2 - 1);
        OldFrameBoxValue = FrameBox.Value;
        OldDirectionBoxValue = DirectionBox.SelectedIndex;
    }

    private void DoubleLeftDownInsideGraphicBox(MouseEventArgs e)
    {
        int rx = e.X - GraphicContainer.Viewport.X + GraphicBox.LeftCutOff;
        int ry = e.Y - GraphicContainer.Viewport.Y + GraphicBox.TopCutOff;
        int FrameBoxValue = Math.Clamp((int) Math.Floor((double) rx / GraphicBox.Bitmap.Width * Graphic.NumFrames), 0, Graphic.NumFrames - 1) + 1;

        int mindir = Graphic.NumDirections == 8 ? 1 : 2;
        int maxdir = Graphic.NumDirections * 2;
        int DirectionBoxValue = Math.Clamp(((int) Math.Floor((double) ry / GraphicBox.Bitmap.Height * Graphic.NumDirections) + 1) * 2, mindir, maxdir) / 2 - 1;

        if (FrameBoxValue != OldFrameBoxValue || DirectionBoxValue != OldDirectionBoxValue) return;
        
        OK();
        e.Handled = true;
    }
}
