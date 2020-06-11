using System;
using System.Collections.Generic;
using System.Text;
using odl;
using MKEditor.Game;
using System.Linq;
using amethyst;

namespace MKEditor.Widgets
{
    public class ChooseGraphic : PopupWindow
    {
        public EventGraphic OldGraphic;
        public EventGraphic GraphicData;

        FileExplorer FileExplorer;
        PictureBox Graphic;
        CursorWidget Cursor;
        Label TypeLabel;
        DropdownBox DirectionBox;
        DropdownBox NumDirectionsBox;
        NumericBox NumFramesBox;

        public ChooseGraphic(EventGraphic graphic)
        {
            this.OldGraphic = graphic;
            this.GraphicData = graphic.Clone();
            SetTitle("Choose Graphic");
            MinimumSize = MaximumSize = new Size(735, 421);
            SetSize(MaximumSize);
            Center();

            Label GraphicLabel = new Label(this);
            GraphicLabel.SetFont(Font.Get("Fonts/Ubuntu-B", 16));
            GraphicLabel.SetText("Current Graphic");
            GraphicLabel.SetPosition(559, 35);

            Color outline = new Color(59, 91, 124);
            Color inline = new Color(17, 27, 38);
            Color filler = new Color(24, 38, 53);
            Sprites["gfxbox"] = new Sprite(this.Viewport);
            Sprites["gfxbox"].X = 542;
            Sprites["gfxbox"].Y = 59;
            Sprites["gfxbox"].Bitmap = new Bitmap(177, 177);
            Sprites["gfxbox"].Bitmap.Unlock();
            Sprites["gfxbox"].Bitmap.DrawRect(0, 0, 177, 177, outline);
            Sprites["gfxbox"].Bitmap.DrawRect(1, 1, 175, 175, inline);
            Sprites["gfxbox"].Bitmap.FillRect(2, 2, 173, 173, filler);
            Sprites["gfxbox"].Bitmap.FillRect(163, 163, 13, 13, outline);
            Sprites["gfxbox"].Bitmap.DrawLine(162, 1, 162, 162, inline);
            Sprites["gfxbox"].Bitmap.DrawLine(163, 1, 163, 162, outline);
            Sprites["gfxbox"].Bitmap.DrawLine(164, 1, 164, 162, inline);
            Sprites["gfxbox"].Bitmap.DrawLine(165, 162, 174, 162, inline);
            Sprites["gfxbox"].Bitmap.DrawLine(1, 162, 162, 162, inline);
            Sprites["gfxbox"].Bitmap.DrawLine(1, 163, 162, 163, outline);
            Sprites["gfxbox"].Bitmap.DrawLine(1, 164, 162, 164, inline);
            Sprites["gfxbox"].Bitmap.DrawLine(162, 165, 162, 174, inline);
            Sprites["gfxbox"].Bitmap.Lock();

            Container GraphicContainer = new Container(this);
            GraphicContainer.SetPosition(544, 61);
            GraphicContainer.SetSize(160, 160);
            GraphicContainer.HAutoScroll = true;
            GraphicContainer.VAutoScroll = true;
            VScrollBar vs = new VScrollBar(this);
            vs.SetPosition(708, 62);
            vs.SetSize(10, 158);
            GraphicContainer.SetVScrollBar(vs);
            HScrollBar hs = new HScrollBar(this);
            hs.SetPosition(545, 225);
            hs.SetSize(158, 10);
            GraphicContainer.SetHScrollBar(hs);

            Graphic = new PictureBox(GraphicContainer);

            Cursor = new CursorWidget(GraphicContainer);
            Cursor.ConsiderInAutoScrollCalculation = false;

            Font f = Font.Get("Fonts/ProductSans-M", 12);

            TypeLabel = new Label(this);
            TypeLabel.SetPosition(547, 244);
            TypeLabel.SetFont(f);
            TypeLabel.SetText("Type: File");

            Label DirectionLabel = new Label(this);
            DirectionLabel.SetFont(f);
            DirectionLabel.SetText("Direction:");
            DirectionLabel.SetPosition(547, 269);
            DirectionBox = new DropdownBox(this);
            DirectionBox.SetPosition(609, 263);
            DirectionBox.SetSize(110, 25);
            DirectionBox.SetItems(new List<ListItem>()
            {
                new ListItem("Down"),
                new ListItem("Left"),
                new ListItem("Right"),
                new ListItem("Up")
            });
            DirectionBox.SetSelectedIndex(GraphicData.Direction / 2 - 1);
            DirectionBox.OnSelectionChanged += delegate (BaseEventArgs e)
            {
                GraphicData.Direction = (DirectionBox.SelectedIndex + 1) * 2;
                if (GraphicData.NumDirections == 1)
                {
                    GraphicData.NumDirections = 4;
                    NumDirectionsBox.SetSelectedIndex(1);
                }
                RedrawGraphic();
            };

            Label NumDirectionsLabel = new Label(this);
            NumDirectionsLabel.SetFont(f);
            NumDirectionsLabel.SetText("Number of Directions:");
            NumDirectionsLabel.SetPosition(547, 308);
            NumDirectionsBox = new DropdownBox(this);
            NumDirectionsBox.SetPosition(679, 304);
            NumDirectionsBox.SetSize(40, 25);
            NumDirectionsBox.SetItems(new List<ListItem>()
            {
                new ListItem("1"),
                new ListItem("4")
            });
            NumDirectionsBox.SetSelectedIndex(GraphicData.NumDirections == 1 ? 0 : 1);
            NumDirectionsBox.OnSelectionChanged += delegate (BaseEventArgs e)
            {
                GraphicData.NumDirections = NumDirectionsBox.SelectedIndex == 0 ? 1 : 4;
                RedrawGraphic();
            };

            Label NumFramesLabel = new Label(this);
            NumFramesLabel.SetFont(f);
            NumFramesLabel.SetText("Number of Frames:");
            NumFramesLabel.SetPosition(547, 348);
            NumFramesBox = new NumericBox(this);
            NumFramesBox.SetPosition(669, 343);
            NumFramesBox.SetSize(50, 27);
            NumFramesBox.MinValue = 1;
            NumFramesBox.MaxValue = 999;
            NumFramesBox.SetValue(GraphicData.NumFrames);
            NumFramesBox.OnValueChanged += delegate (BaseEventArgs e)
            {
                GraphicData.NumFrames = NumFramesBox.Value;
                RedrawGraphic();
            };

            FileExplorer = new FileExplorer(this);
            FileExplorer.SetPosition(1, 24);
            FileExplorer.SetSize(529, 396);
            FileExplorer.SetBaseDirectory(Data.ProjectPath);
            FileExplorer.SetFileExtensions("png");
            string dir = "";
            if (GraphicData.Param != null)
            {
                List<string> dirs = ((string) GraphicData.Param).Split('/').ToList();
                for (int i = 0; i < dirs.Count - 1; i++)
                {
                    dir += dirs[i];
                    if (i != dirs.Count - 2) dir += "/";
                }
            }
            else dir = "gfx/characters";
            FileExplorer.SetDirectory(dir);
            FileExplorer.OnFileSelected += delegate (BaseEventArgs e)
            {
                string param = FileExplorer.SelectedFilename.Replace(Data.ProjectPath + "/", "").Replace(".png", "");
                if (param != (string) this.GraphicData.Param)
                {
                    this.GraphicData = new EventGraphic();
                    this.GraphicData.Type = ":file";
                    this.GraphicData.Param = param;
                    DirectionBox.SetSelectedIndex(0);
                    NumDirectionsBox.SetSelectedIndex(1);
                    NumFramesBox.SetValue(4);
                    RedrawGraphic();
                }
            };
            FileExplorer.SetSelectedFile((string) GraphicData.Param + ".png");

            CreateButton("Cancel", Cancel);
            Buttons[0].SetPosition(Size.Width - 99, Buttons[0].Position.Y);
            CreateButton("OK", OK);
            Buttons[1].SetPosition(Size.Width - 188, Buttons[1].Position.Y);

            RedrawGraphic();
        }

        public void Cancel(BaseEventArgs e)
        {
            this.GraphicData = OldGraphic;
            Close();
        }

        public void OK(BaseEventArgs e)
        {
            Close();
        }

        public void RedrawGraphic()
        {
            if (FileExplorer.SelectedIsFolder) return;
            Graphic.Sprite.Bitmap?.Dispose();
            Graphic.Sprite.Bitmap = new Bitmap(FileExplorer.SelectedFilename);
            int x = 0;
            int y = 0;
            int w = Graphic.Sprite.Bitmap.Width / GraphicData.NumFrames;
            int h = Graphic.Sprite.Bitmap.Height / GraphicData.NumDirections;
            if (GraphicData.NumDirections == 4) y = h * (GraphicData.Direction / 2 - 1);
            if (GraphicData.NumDirections == 8) y = h * (GraphicData.Direction - 1);
            Cursor.SetPosition(x - 7, y - 7);
            Cursor.SetSize(w + 14, h + 14);
        }
    }
}
