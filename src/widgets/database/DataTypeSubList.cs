﻿using System.Collections.Generic;

namespace RPGStudioMK.Widgets;

public class DataTypeSubList : Widget
{
    public int SelectedIndex => ListDrawer.SelectedIndex;
    public TreeNode SelectedItem => ListDrawer.SelectedItem;
    public int HoveringIndex => ListDrawer.HoveringIndex;
    public TreeNode HoveringItem => ListDrawer.HoveringItem;
    public List<TreeNode> Items => ListDrawer.Items;

    ListDrawer ListDrawer;
    Container ScrollContainer;
    Button ChangeMaxBtn;

    public int ListMaximum;

    public BaseEvent OnSelectionChanged { get { return ListDrawer.OnSelectionChanged; } set { ListDrawer.OnSelectionChanged = value; } }
    public GenericObjectEvent<int> OnMaximumChanged;

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
        Size s = Fonts.Header.TextSize(HeaderText);
        Sprites["header"].Bitmap = new Bitmap(s);
        Sprites["header"].Bitmap.Font = Fonts.Header;
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
        ListDrawer.SetFont(Fonts.Paragraph);
        ListDrawer.SetCanMultiSelect(true);
        ChangeMaxBtn = new Button(this);
        ChangeMaxBtn.SetText("Change Maximum");
        ChangeMaxBtn.OnClicked += _ =>
        {
            GenericNumberPicker win = new GenericNumberPicker("Set Max", "Maximum:", ListMaximum, 1, null);
            win.OnClosed += _ =>
            {
                if (!win.Apply || ListMaximum == win.Value) return;
                OnMaximumChanged?.Invoke(new GenericObjectEventArgs<int>(win.Value));
                ListMaximum = win.Value;
            };
        };
        SetWidth(181);
    }

    public override void SetContextMenuList(List<IMenuItem> Items)
    {
        ListDrawer.SetContextMenuList(Items);
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

    public void SetItems(List<TreeNode> Items)
    {
        ListDrawer.SetItems(Items);
    }

    public void SetSelectedIndex(int SelectedIndex)
    {
        ListDrawer.SetSelectedIndex(SelectedIndex);
    }
}
