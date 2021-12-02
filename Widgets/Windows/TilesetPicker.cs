﻿using System;
using System.Collections.Generic;
using odl;
using RPGStudioMK.Game;
using amethyst;

namespace RPGStudioMK.Widgets;

public class TilesetPicker : PopupWindow
{
    Map Map;

    public int ChosenTilesetID;

    ListBox Tilesets;
    GroupBox previewbox;
    PictureBox tileset;
    Container scroll;

    public TilesetPicker(Map Map)
    {
        this.Map = Map;
        SetTitle("Add Tileset");
        MinimumSize = MaximumSize = new Size(506, 498);
        SetSize(MaximumSize);
        Center();

        Label pickerlabel = new Label(this);
        pickerlabel.SetText("Tilesets");
        pickerlabel.SetPosition(18, 24);
        pickerlabel.SetFont(Fonts.UbuntuBold.Use(14));
        Tilesets = new ListBox(this);
        Tilesets.SetPosition(25, 44);
        Tilesets.SetSize(151, 409);
        List<ListItem> items = new List<ListItem>();
        for (int i = 1; i < Data.Tilesets.Count; i++)
        {
            Tileset tileset = Data.Tilesets[i];
            items.Add(new ListItem($"{Utilities.Digits(i, 3)}: {tileset?.Name}", tileset));
        }
        Tilesets.SetItems(items);
        Tilesets.OnSelectionChanged += delegate (BaseEventArgs e)
        {
            UpdatePreview();
        };

        Label previewlabel = new Label(this);
        previewlabel.SetText("Preview");
        previewlabel.SetPosition(192, 24);
        previewlabel.SetFont(Fonts.UbuntuBold.Use(14));
        previewbox = new GroupBox(this);
        previewbox.SetPosition(200, 44);
        previewbox.SetSize(280, 409);
        previewbox.Sprites["line"] = new Sprite(previewbox.Viewport, new SolidBitmap(1, 405, new Color(40, 62, 84)));
        previewbox.Sprites["line"].X = 267;
        previewbox.Sprites["line"].Y = 2;
        scroll = new Container(previewbox);
        scroll.SetPosition(3, 3);
        scroll.SetSize(263, 403);
        scroll.SetVScrollBar(new VScrollBar(previewbox));
        scroll.VScrollBar.SetPosition(269, 3);
        scroll.VScrollBar.SetSize(8, 403);
        scroll.VAutoScroll = true;

        tileset = new PictureBox(scroll);

        CreateButton("Cancel", Cancel);
        CreateButton("OK", OK);

        Tilesets.SetSelectedIndex(0);
    }

    public void UpdatePreview()
    {
        Tileset data = Tilesets.Items[Tilesets.SelectedIndex].Object as Tileset;
        tileset.Sprite.Bitmap = null;
        tileset.SetSize(1, 1);
        if (data == null) return;
        tileset.Sprite.Bitmap = data.TilesetListBitmap;
        tileset.Sprite.DestroyBitmap = false;
        tileset.SetSize(data.TilesetListBitmap.Width, data.TilesetListBitmap.Height);
        scroll.VScrollBar.SetValue(0);
    }

    public void OK(BaseEventArgs e)
    {
        Tileset t = (Tilesets.Items[Tilesets.SelectedIndex].Object as Tileset);
        if (t == null)
        {
            new MessageBox("Error", "This tileset doesn't have a graphic. Please pick a different tileset.", IconType.Error);
        }
        else
        {
            this.ChosenTilesetID = t.ID;
            if (this.Map != null && this.Map.TilesetIDs.Contains(this.ChosenTilesetID))
            {
                new MessageBox("Error", "This map already contains this tileset. Please pick a different tileset.", IconType.Error);
            }
            else
            {
                Close();
            }
        }
    }

    public void Cancel(BaseEventArgs e)
    {
        this.ChosenTilesetID = -1;
        Close();
    }
}
