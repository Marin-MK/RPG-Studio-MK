using System;
using System.Collections.Generic;

namespace RPGStudioMK.Widgets;

public class DataTypeSubList : Widget
{
    public int SelectedIndex { get { return ListDrawer.SelectedIndex; } }
    public ListItem SelectedItem { get { return ListDrawer.SelectedItem; } }
    public int HoveringIndex { get { return ListDrawer.HoveringIndex; } }
    public ListItem HoveringItem { get { return ListDrawer.HoveringItem; } }
    public List<ListItem> Items { get { return ListDrawer.Items; } }

    ListDrawer ListDrawer;
    Container ScrollContainer;
    Button ChangeMaxBtn;

    public int ListMaximum;

    public BaseEvent OnSelectionChanged { get { return ListDrawer.OnSelectionChanged; } set { ListDrawer.OnSelectionChanged = value; } }
    public ObjectEvent OnMaximumChanged;

    public DataTypeSubList(string HeaderText, int InitialListMaximum, IContainer Parent) : base(Parent)
    {
        ListMaximum = InitialListMaximum;
        Sprites["line1"] = new Sprite(this.Viewport, new SolidBitmap(1, 1, new Color(10, 23, 37)));
        Sprites["line1"].X = 168;
        Sprites["line1"].Y = 29;
        Sprites["line2"] = new Sprite(this.Viewport, new SolidBitmap(1, 1, Color.BLACK));
        Sprites["line2"].X = 180;
        Sprites["line2"].Y = 29;
        Sprites["line3"] = new Sprite(this.Viewport, new SolidBitmap(180, 1, new Color(10, 23, 37)));
        Sprites["headerbg"] = new Sprite(this.Viewport, new SolidBitmap(181, 29, new Color(17, 33, 51)));
        Sprites["header"] = new Sprite(this.Viewport);
        Font f = Fonts.UbuntuBold.Use(18);
        Size s = f.TextSize(HeaderText);
        Sprites["header"].Bitmap = new Bitmap(s);
        Sprites["header"].Bitmap.Font = f;
        Sprites["header"].Bitmap.Unlock();
        Sprites["header"].Bitmap.DrawText(HeaderText, Color.WHITE);
        Sprites["header"].Bitmap.Lock();
        Sprites["header"].X = 90 - s.Width / 2;
        Sprites["header"].Y = 6;
        ScrollContainer = new Container(this);
        ScrollContainer.SetPosition(0, 29);
        VScrollBar vs = new VScrollBar(this);
        vs.SetPosition(170, 30);
        ScrollContainer.SetVScrollBar(vs);
        ScrollContainer.VAutoScroll = true;
        ListDrawer = new ListDrawer(ScrollContainer);
        ListDrawer.SetLineHeight(24);
        ListDrawer.SetFont(Fonts.ProductSansMedium.Use(14));
        ChangeMaxBtn = new Button(this);
        ChangeMaxBtn.SetText("Change Maximum");
        ChangeMaxBtn.OnClicked += _ =>
        {
            SetMaximumWindow win = new SetMaximumWindow(ListMaximum);
            win.OnClosed += _ =>
            {
                if (win.PressedOK)
                {
                    OnMaximumChanged?.Invoke(new ObjectEventArgs(win.Maximum));
                    ListMaximum = win.Maximum;
                }
            };
        };
        SetWidth(181);
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        ScrollContainer.SetSize(Size.Width - 13, Size.Height - 70);
        ScrollContainer.VScrollBar.SetHeight(Size.Height - 72);
        ListDrawer.SetSize(ScrollContainer.Size);
        ((SolidBitmap)Sprites["line1"].Bitmap).SetSize(1, Size.Height - 70);
        ((SolidBitmap)Sprites["line2"].Bitmap).SetSize(1, Size.Height - 29);
        Sprites["line3"].Y = Size.Height - 41;
        ChangeMaxBtn.SetPosition(4, Size.Height - 37);
        ChangeMaxBtn.SetSize(Size.Width - 8, 33);
    }

    public void SetItems(List<ListItem> Items)
    {
        ListDrawer.SetItems(Items);
    }

    public void SetSelectedIndex(int SelectedIndex)
    {
        ListDrawer.SetSelectedIndex(SelectedIndex);
    }
}
