using System;
using System.Collections.Generic;
using System.Text;
using MKEditor.Game;
using ODL;

namespace MKEditor.Widgets
{
    public class GraphicWidget : Widget
    {
        public Event EventData;
        public EventPage PageData;

        GraphicGrid GraphicGrid;
        PictureBox Graphic;

        public GraphicWidget(IContainer Parent) : base(Parent)
        {
            SetSize(121, 161);
            EventGroupBox egb = new EventGroupBox(this);
            egb.SetSize(Size);
            GraphicGrid = new GraphicGrid(egb);
            GraphicGrid.SetPosition(2, 2);
            GraphicGrid.SetSize(Size.Width - 4, Size.Height - 4);
            GraphicGrid.SetOffset(22, 30);

            Graphic = new PictureBox(egb);
            Graphic.SetPosition(2, 2);
            Graphic.ResizeBox = false;
            Graphic.SetSize(Size.Width - 4, Size.Height - 4);
        }

        public void SetEvent(Event EventData, EventPage PageData)
        {
            this.EventData = EventData;
            this.PageData = PageData;
            Graphic.Sprite.Bitmap?.Dispose();
            if (PageData.Graphic.Type == ":file")
            {
                Graphic.Sprite.Bitmap = new Bitmap(Data.ProjectPath + "/" + PageData.Graphic.Param.ToString());
                Graphic.Sprite.SrcRect.Width = Graphic.Sprite.Bitmap.Width / PageData.Graphic.NumFrames;
                Graphic.Sprite.SrcRect.Height = Graphic.Sprite.Bitmap.Height / PageData.Graphic.NumDirections;
                int dir = 0;
                if (PageData.Graphic.NumDirections == 4) dir = ((PageData.Graphic.Direction / 2) - 1);
                else if (PageData.Graphic.NumDirections == 8) dir = PageData.Graphic.Direction - 1;
                Graphic.Sprite.SrcRect.Y = Graphic.Sprite.SrcRect.Height * dir;
                Graphic.Sprite.X = Size.Width / 2 - Graphic.Sprite.SrcRect.Width / 2 - 2;
                Graphic.Sprite.Y = Size.Height / 2 - Graphic.Sprite.SrcRect.Height / 2 - 2;
                ConfigureGrid();
            }
        }

        public void ConfigureGrid()
        {
            GraphicGrid.SetOffset(EventData.Width % 2 == 0 ? 6 : 22, 30);
        }

        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);
            if (WidgetIM.Hovering && e.LeftButton != e.OldLeftButton)
            {
                ChooseGraphic cg = new ChooseGraphic(PageData.Graphic);
                cg.OnClosed += delegate (BaseEventArgs e)
                {
                    if (PageData.Graphic.Type != cg.GraphicData.Type ||
                        PageData.Graphic.Param != cg.GraphicData.Param ||
                        PageData.Graphic.NumDirections != cg.GraphicData.NumDirections ||
                        PageData.Graphic.NumFrames != cg.GraphicData.NumFrames ||
                        PageData.Graphic.Direction != cg.GraphicData.Direction)
                        MarkChanges();
                    if (PageData.Graphic != cg.GraphicData)
                    {
                        if (PageData.Graphic.Type != cg.GraphicData.Type ||
                            PageData.Graphic.Param != cg.GraphicData.Param)
                        {
                            PageData.Settings.Passable = false;
                            ((EventPageContainer) Parent).PassableBox.SetChecked(false);
                        }
                        PageData.Graphic = cg.GraphicData;
                        SetEvent(this.EventData, this.PageData);
                    }
                };
            }
        }

        public void MarkChanges()
        {
            ((EventPageContainer) Parent).MarkChanges();
        }
    }

    public class GraphicGrid : Widget
    {
        public int TileSize = 32;
        public int OffsetX = 0;
        public int OffsetY = 0;
        public string Border;

        public GraphicGrid(IContainer Parent) : base(Parent)
        {
            Sprites["one"] = new Sprite(this.Viewport, new SolidBitmap(32, 32, new Color(226, 226, 226)));
            Sprites["two"] = new Sprite(this.Viewport, new SolidBitmap(32, 32, new Color(246, 246, 246)));
            RedrawGrid();
        }

        public override void SizeChanged(BaseEventArgs e)
        {
            base.SizeChanged(e);
            RedrawGrid();
        }

        public void SetOffset(int x, int y)
        {
            if (this.OffsetX != x || this.OffsetY != y)
            {
                this.OffsetX = x % 32;
                this.OffsetY = y % 32;
                RedrawGrid();
            }
        }

        public void RedrawGrid()
        {
            Sprites["one"].MultiplePositions.Clear();
            Sprites["two"].MultiplePositions.Clear();

            for (int y = 0; y < (int) Math.Ceiling(Size.Height / 32d) + 1; y++)
            {
                for (int x = 0; x < (int) Math.Ceiling(Size.Width / 32d) + 1; x++)
                {
                    if (x % 2 == y % 2) Sprites["one"].MultiplePositions.Add(new Point(x * 32 - OffsetX, y * 32 - OffsetY));
                    else Sprites["two"].MultiplePositions.Add(new Point(x * 32 - OffsetX, y * 32 - OffsetY));
                }
            }
        }
    }
}
