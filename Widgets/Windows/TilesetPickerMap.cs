using System;
using System.Collections.Generic;
using MKEditor.Game;
using odl;
using amethyst;

namespace MKEditor.Widgets
{
    public class TilesetPickerMap : PopupWindow
    {
        List<int> OldIDs;

        public List<int> ResultIDs;

        Tileset SelectedTileset { get { return Available.SelectedItem is null ? InUse.SelectedItem.Object as Tileset : Available.SelectedItem.Object as Tileset; } }

        Button ActionButton;
        ListBox Available;
        ListBox InUse;
        PictureBox TilesetBox;
        Container TilesetContainer;

        public TilesetPickerMap(Map Map)
        {
            SetTitle("Change Tilesets");
            MinimumSize = MaximumSize = new Size(506, 498);
            SetSize(MaximumSize);
            Center();

            OldIDs = new List<int>(Map.AutotileIDs);

            ColoredBox box1 = new ColoredBox(this);
            box1.SetOuterColor(59, 91, 124);
            box1.SetInnerColor(17, 27, 38);
            box1.SetPosition(200, 44);
            box1.SetSize(280, 409);

            ColoredBox box2 = new ColoredBox(this);
            box2.SetOuterColor(24, 38, 53);
            box2.SetPosition(201, 45);
            box2.SetSize(278, 407);

            TilesetContainer = new Container(this);
            TilesetContainer.SetPosition(203, 47);
            TilesetContainer.SetSize(274, 403);
            TilesetContainer.VAutoScroll = true;
            VScrollBar vs = new VScrollBar(this);
            vs.SetPosition(469, 47);
            vs.SetSize(10, 403);
            TilesetContainer.SetVScrollBar(vs);

            TilesetBox = new PictureBox(TilesetContainer);

            Font f = Font.Get("Fonts/Ubuntu-B", 14);

            Label labelavail = new Label(this);
            labelavail.SetText("Available");
            labelavail.SetPosition(16, 24);
            labelavail.SetFont(f);

            Label labelinuse = new Label(this);
            labelinuse.SetText("In-use");
            labelinuse.SetPosition(16, 250);
            labelinuse.SetFont(f);

            Label labelprev = new Label(this);
            labelprev.SetText("Preview");
            labelprev.SetPosition(192, 24);
            labelprev.SetFont(f);

            ActionButton = new Button(this);
            ActionButton.SetPosition(52, 225);
            ActionButton.SetSize(85, 30);
            ActionButton.OnClicked += ActionButtonClicked;

            Available = new ListBox(this);
            Available.SetPosition(25, 44);
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
                new MenuItem("Add Tileset")
                {
                    IsClickable = delegate (BoolEventArgs e)
                    {
                        e.Value = !(SelectedTileset is null);
                    },
                    OnLeftClick = ActionButtonClicked
                }
            });

            InUse = new ListBox(this);
            InUse.SetPosition(25, 274);
            InUse.SetSize(151, 179);
            InUse.OnSelectionChanged += delegate (BaseEventArgs e)
            {
                if (InUse.SelectedIndex != -1)
                {
                    Available.SetSelectedIndex(-1);
                    SelectionChanged(e);
                }
            };
            InUse.ListDrawer.SetContextMenuList(new List<IMenuItem>()
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
            });

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
                ActionButton.SetText("Add");
            }
            else
            {
                InUse.SetSelectedIndex(0);
                ActionButton.SetText("Remove");
            }

            SetTimer("frame", (long) Math.Round(1000 / 60d));
        }

        public void SelectionChanged(BaseEventArgs e)
        {
            ActionButton.SetEnabled(true);
            if (InUse.SelectedIndex == -1)
            {
                ActionButton.SetText("Add");
            }
            else
            {
                ActionButton.SetText("Remove");
            }
            Tileset tileset = SelectedTileset;
            if (tileset is null)
            {
                TilesetBox.Sprite.Bitmap = null;
                ActionButton.SetEnabled(false);
                return;
            }
            TilesetBox.Sprite.Bitmap = tileset.TilesetListBitmap;
            TilesetBox.Sprite.DestroyBitmap = false;
        }

        private void ActionButtonClicked(BaseEventArgs e)
        {
            if (InUse.SelectedIndex == -1) // Add
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
            }
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
}
