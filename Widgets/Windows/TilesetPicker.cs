using System;
using System.Collections.Generic;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class TilesetPicker : PopupWindow
{
    List<int> OldIDs;

    public List<int> ResultIDs;

    Tileset SelectedTileset { get { return Available.SelectedItem is null ? InUse.SelectedItem.Object as Tileset : Available.SelectedItem.Object as Tileset; } }

    Button ActionButton;
    ListBox Available;
    ListBox InUse;
    Container TilesetContainer;

    public TilesetPicker(Map Map)
    {
        SetTitle("Change Tileset");
        MinimumSize = MaximumSize = new Size(506, 505);
        SetSize(MaximumSize);
        Center();

        OldIDs = new List<int>(Map.TilesetIDs);

        ColoredBox box1 = new ColoredBox(this);
        box1.SetOuterColor(59, 91, 124);
        box1.SetInnerColor(17, 27, 38);
        box1.SetPosition(200, 51);
        box1.SetSize(280, 409);

        ColoredBox box2 = new ColoredBox(this);
        box2.SetOuterColor(24, 38, 53);
        box2.SetPosition(201, 52);
        box2.SetSize(278, 407);

        TilesetContainer = new Container(this);
        TilesetContainer.SetPosition(203, 54);
        TilesetContainer.SetSize(274, 403);
        TilesetContainer.VAutoScroll = true;
        VScrollBar vs = new VScrollBar(this);
        vs.SetPosition(469, 54);
        vs.SetSize(10, 403);
        TilesetContainer.SetVScrollBar(vs);

        Font f = Fonts.UbuntuBold.Use(14);

        Label labelavail = new Label(this);
        labelavail.SetText("Available");
        labelavail.SetPosition(16, 31);
        labelavail.SetFont(f);

        Label labelinuse = new Label(this);
        labelinuse.SetText("In-use");
        labelinuse.SetPosition(16, 257);
        labelinuse.SetFont(f);

        Label labelprev = new Label(this);
        labelprev.SetText("Preview");
        labelprev.SetPosition(192, 31);
        labelprev.SetFont(f);

        ActionButton = new Button(this);
        ActionButton.SetPosition(52, 232);
        ActionButton.SetSize(85, 30);
        ActionButton.SetText("Set");
        ActionButton.OnClicked += ActionButtonClicked;

        Available = new ListBox(this);
        Available.SetPosition(25, 51);
        Available.SetSize(151, 179);
        Available.OnSelectionChanged += delegate (BaseEventArgs e)
        {
            if (Available.SelectedIndex != -1)
            {
                InUse.SetSelectedIndex(-1);
                SelectionChanged(e);
            }
        };
        Available.ListDrawer.SetContextMenuList(new List<IMenuItem>()
            {
                new MenuItem("Set Tileset")
                {
                    IsClickable = delegate (BoolEventArgs e)
                    {
                        e.Value = SelectedTileset is not null;
                    },
                    OnLeftClick = ActionButtonClicked
                }
            });

        InUse = new ListBox(this);
        InUse.SetPosition(25, 281);
        InUse.SetSize(151, 179);
        InUse.OnSelectionChanged += delegate (BaseEventArgs e)
        {
            if (InUse.SelectedIndex != -1)
            {
                Available.SetSelectedIndex(-1);
                SelectionChanged(e);
            }
        };
        /*InUse.ListDrawer.SetContextMenuList(new List<IMenuItem>()
        {
            new MenuItem("Move Tileset Up")
            {
                IsClickable = delegate (BoolEventArgs e)
                {
                    e.Value = InUse.SelectedIndex > 0;
                },
                OnLeftClick = MoveTilesetUp
            },
            new MenuItem("Move Tileset Down")
            {
                IsClickable = delegate (BoolEventArgs e)
                {
                    e.Value = InUse.SelectedIndex < InUse.Items.Count - 1;
                },
                OnLeftClick = MoveTilesetDown
            },
            new MenuSeparator(),
            new MenuItem("Remove Tileset")
            {
                OnLeftClick = ActionButtonClicked
            }
        });*/

        List<ListItem> AvailableList = new List<ListItem>();
        List<ListItem> InUseList = new List<ListItem>();

        // Populate lists
        for (int i = 0; i < Map.TilesetIDs.Count; i++)
        {
            InUseList.Add(new ListItem($"{Utilities.Digits(Map.TilesetIDs[i], 3)}: {Data.Tilesets[Map.TilesetIDs[i]].Name}", Data.Tilesets[Map.TilesetIDs[i]]));
        }
        for (int i = 1; i < Data.Tilesets.Count; i++)
        {
            if (!Map.TilesetIDs.Contains(i))
            {
                AvailableList.Add(new ListItem($"{Utilities.Digits(i, 3)}: {Data.Tilesets[i]?.Name}", Data.Tilesets[i]));
            }
        }

        Available.SetItems(AvailableList);
        InUse.SetItems(InUseList);

        CreateButton("Cancel", delegate (BaseEventArgs e)
        {
            ResultIDs = OldIDs;
            Close();
        });

        CreateButton("OK", delegate (BaseEventArgs e)
        {
            ResultIDs = new List<int>();
            for (int i = 0; i < InUse.Items.Count; i++)
            {
                ResultIDs.Add(Data.Tilesets.IndexOf((InUse.Items[i].Object as Tileset)));
            }
            Close();
        });

        if (Available.Items.Count > 0)
        {
            Available.SetSelectedIndex(0);
        }
        else
        {
            InUse.SetSelectedIndex(0);
        }

        SetTimer("frame", (long)Math.Round(1000 / 60d));
    }

    public void SelectionChanged(BaseEventArgs e)
    {
        if (InUse.SelectedIndex == -1)
        {
            //ActionButton.SetText("Add");
            ActionButton.SetEnabled(true);
        }
        else
        {
            //ActionButton.SetText("Remove");
            ActionButton.SetEnabled(false);
        }
        Tileset tileset = SelectedTileset;
        TilesetContainer.Widgets.FindAll(w => w is PictureBox).ForEach(w => w.Dispose());
        if (tileset is null || tileset.TilesetListBitmap is null)
        {
            ActionButton.SetEnabled(false);
        }
        else if (tileset.TilesetListBitmap.IsChunky)
        {
            int y = 0;
            foreach (Bitmap b in tileset.TilesetListBitmap.InternalBitmaps)
            {
                PictureBox img = new PictureBox(TilesetContainer);
                img.SetPosition(0, y);
                img.Sprite.Bitmap = b;
                if (!b.Locked) b.Lock();
                img.Sprite.DestroyBitmap = false;
                img.SetSize(b.Width, b.Height);
                y += b.Height;
            }
        }
        else
        {
            PictureBox img = new PictureBox(TilesetContainer);
            img.Sprite.Bitmap = tileset.TilesetListBitmap;
            img.Sprite.DestroyBitmap = false;
            img.SetSize(img.Sprite.Bitmap.Width, img.Sprite.Bitmap.Height);
        }
    }

    private void ActionButtonClicked(BaseEventArgs e)
    {
        ListItem item = Available.SelectedItem;
        Available.Items.Remove(item);
        Available.Items.Add(new ListItem($"{Utilities.Digits(((Tileset)InUse.Items[0].Object).ID, 3)}: {((Tileset)InUse.Items[0].Object).Name}", InUse.Items[0].Object));
        Available.Items.Sort((ListItem i1, ListItem i2) => { return ((Tileset)i1.Object).ID.CompareTo(((Tileset)i2.Object).ID); });
        Available.SetItems(Available.Items);
        InUse.Items.Clear();
        InUse.Items.Add(item);
        InUse.SetItems(InUse.Items);
        /*if (InUse.SelectedIndex == -1) // Add
        {
            if (SelectedTileset is null) return;
            ListItem item = Available.SelectedItem;
            Available.Items.Remove(item);
            InUse.Items.Add(item);
            Available.SetItems(Available.Items);
            InUse.SetItems(InUse.Items);
            if (Available.SelectedIndex == -1 && InUse.SelectedIndex == -1)
                InUse.SetSelectedIndex(0);
        }
        else // Remove
        {
            if (SelectedTileset is null) return;
            ListItem item = InUse.SelectedItem;
            InUse.Items.Remove(item);
            List<ListItem> availitems = new List<ListItem>();
            for (int i = 1; i < Data.Tilesets.Count; i++)
            {
                if (InUse.Items.Find(item => item.Object == Data.Tilesets[i]) is null)
                {
                    availitems.Add(new ListItem($"{Utilities.Digits(i, 3)}: {Data.Tilesets[i]?.Name}", Data.Tilesets[i]));
                }
            }
            Available.SetItems(availitems);
            InUse.SetItems(InUse.Items);
            if (Available.SelectedIndex == -1 && InUse.SelectedIndex == -1)
                Available.SetSelectedIndex(0);
        }*/
        SelectionChanged(e);
    }

    public void MoveTilesetUp(BaseEventArgs e)
    {
        if (InUse.SelectedIndex > 0)
        {
            InUse.Items.Swap(InUse.SelectedIndex - 1, InUse.SelectedIndex);
            InUse.Redraw();
            InUse.SetSelectedIndex(InUse.SelectedIndex - 1);
        }
    }

    public void MoveTilesetDown(BaseEventArgs e)
    {
        if (InUse.SelectedIndex < InUse.Items.Count - 1)
        {
            InUse.Items.Swap(InUse.SelectedIndex + 1, InUse.SelectedIndex);
            InUse.Redraw();
            InUse.SetSelectedIndex(InUse.SelectedIndex + 1);
        }
    }
}
