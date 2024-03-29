﻿using System;
using System.Collections.Generic;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class LayerWidget : Widget
{
    public LayerPanel LayerPanel { get { return (Parent.Parent) as LayerPanel; } }
    public TilesPanel TilesPanel { get { return MapViewer.TilesPanel; } }
    public MapViewer MapViewer { get { return LayerPanel.MapViewer; } }
    public List<Layer> Layers { get; private set; }
    public int SelectedLayer { get; private set; }
    public int HoveringIndex { get; private set; } = -1;
    public TextBox RenameBox;

    public LayerWidget(IContainer Parent) : base(Parent)
    {
        Sprites["bg"] = new Sprite(this.Viewport);
        Sprites["text"] = new Sprite(this.Viewport);
        Sprites["selector"] = new Sprite(this.Viewport, new SolidBitmap(2, 24, new Color(59, 227, 255)));
        Sprites["selector"].Visible = false;
    }

    public void SetLayers(List<Layer> Layers)
    {
        this.Layers = Layers;
        SelectedLayer = 0;
        Redraw();
        SetSize(278, (Layers?.Count ?? 1) * 24);
    }

    public void UpdateLayers()
    {
        SetHeight((Layers?.Count ?? 1) * 24);
        Redraw();
    }

    public void SetSelectedLayer(int layerindex)
    {
        SelectedLayer = layerindex;
        Editor.ProjectSettings.LastLayer = layerindex;
        Redraw();
    }

    public void SetLayerVisible(int layerindex, bool visible)
    {
        MapViewer.SetLayerVisible(layerindex, visible);
        Redraw();
    }

    public void RenameLayer(int Index)
    {
        RenameBox = new TextBox(this);
        RenameBox.SetPosition(50, (Layers.Count - Index - 1) * 24 + 1);
        RenameBox.SetSize(Size.Width - 58, 22);
        RenameBox.SetText(Layers[Index].Name);
        RenameBox.TextArea.SelectAll();
        RenameBox.TextArea.OnWidgetDeselected += delegate (BaseEventArgs e)
        {
            if (Layers[Index].Name != RenameBox.Text && !string.IsNullOrEmpty(RenameBox.Text))
            {
                Layers[Index].Name = RenameBox.Text;
                Redraw();
            }
            RenameBox.Dispose();
            RenameBox = null;
            Input.SetCursor(CursorType.Arrow);
            Window.UI.SetSelectedWidget((Parent.Parent as LayerPanel));
        };
        RenameBox.TextArea.OnWidgetSelected(new BaseEventArgs());
    }

    protected override void Draw()
    {
        base.Draw();
        Sprites["bg"].Bitmap?.Dispose();
        Sprites["text"].Bitmap?.Dispose();
        if (Layers == null) return;
        Sprites["bg"].Bitmap = new Bitmap(Size.Width, 24 * Layers.Count);
        Sprites["bg"].Bitmap.Unlock();
        Sprites["text"].Bitmap = new Bitmap(Size.Width, 24 * Layers.Count);
        Sprites["text"].Bitmap.Unlock();
        Sprites["text"].Bitmap.Font = Fonts.Paragraph;
        for (int i = 0; i < Layers.Count; i++)
        {
            int y = 24 * (Layers.Count - i - 1);
            Color c = Color.WHITE;
            bool visible = Layers[i].Visible;
            if (i == SelectedLayer)
            {
                c = new Color(55, 187, 255);
                Sprites["bg"].Bitmap.FillRect(0, y, Size.Width, 24, new Color(19, 36, 55));
            }
            Icon icon = visible ? Icon.EyeOpen : Icon.EyeClosed;
            Sprites["bg"].Bitmap.Build(8, y - 1, Utilities.IconSheet, new Rect((int) icon * 24, i == SelectedLayer ? 24 : 0, 24, 24));
            Sprites["text"].Bitmap.DrawText(Layers[i].Name, 53, y + 3, c);
        }
        Sprites["bg"].Bitmap.Lock();
        Sprites["text"].Bitmap.Lock();
    }

    public override void HoverChanged(MouseEventArgs e)
    {
        base.HoverChanged(e);
        if (!Mouse.Inside)
        {
            Sprites["selector"].Visible = false;
            HoveringIndex = -1;
        }
    }

    public override void MouseMoving(MouseEventArgs e)
    {
        base.MouseMoving(e);
        if (Layers == null) return;
        if (MapViewer.UsingLeft || MapViewer.UsingRight || TilesPanel.UsingLeft || TilesPanel.UsingRight) return;
        if (!Mouse.Inside)
        {
            Sprites["selector"].Visible = false;
            HoveringIndex = -1;
            return;
        }
        int ry = e.Y - Viewport.Y + Position.Y - ScrolledPosition.Y;
        Sprites["selector"].Y = 24 * (int)Math.Floor(ry / 24d);
        Sprites["selector"].Visible = true;
        HoveringIndex = (int)Math.Floor(ry / 24d);
    }

    public override void MouseDown(MouseEventArgs e)
    {
        base.MouseDown(e);
        if (Layers == null) return;
        if (MapViewer.UsingLeft || MapViewer.UsingRight || TilesPanel.UsingLeft || TilesPanel.UsingRight) return;
        if (Mouse.Inside)
        {
            if (Mouse.LeftMouseTriggered) LayerPanel.UsingLeft = true;
            if (Mouse.RightMouseTriggered) LayerPanel.UsingRight = true;
        }
        MouseMoving(e);
        int rx = e.X - Viewport.X;
        int ry = e.Y - Viewport.Y + Position.Y - ScrolledPosition.Y;
        if (!Mouse.Inside) return;
        int layerindex = Layers.Count - 1 - (int)Math.Floor(ry / 24d);
        if (layerindex < 0 || layerindex >= Layers.Count) return;
        if (rx < 39)
        {
            if (e.LeftButton != e.OldLeftButton && e.LeftButton)
            {
                SetLayerVisible(layerindex, !Layers[layerindex].Visible);
            }
            else
            {
                SetSelectedLayer(layerindex);
            }
        }
        else
        {
            SetSelectedLayer(layerindex);
        }
    }

    public override void MouseUp(MouseEventArgs e)
    {
        base.MouseUp(e);
        if (Mouse.LeftMouseReleased) LayerPanel.UsingLeft = false;
        if (Mouse.RightMouseReleased) LayerPanel.UsingRight = false;
    }
}
