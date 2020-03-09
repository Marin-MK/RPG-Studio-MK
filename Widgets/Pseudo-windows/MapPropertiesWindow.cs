using System;
using System.Collections.Generic;
using ODL;
using MKEditor.Game;

namespace MKEditor.Widgets
{
    public class MapPropertiesWindow : PopupWindow
    {
        public Map Map;
        public Map OldMap;

        public bool UnsavedChanges = false;
        public bool UpdateMapViewer = false;

        TextBox MapName;
        TextBox DisplayName;
        NumericBox Width;
        NumericBox Height;
        ListBox Tilesets;
        ListBox Autotiles;

        public MapPropertiesWindow(Map Map, object Parent, string Name = "mapPropertiesWindow")
            : base(Parent, Name)
        {
            this.OldMap = Map;
            this.Map = Map.Clone() as Map;
            this.SetTitle($"Map Properties - {Utilities.Digits(Map.ID, 3)}: {Map.DevName}");
            this.SetSize(540, 460);
            this.Center();
            Label settings = new Label(this);
            settings.SetText("Info");
            settings.SetFont(Font.Get("Fonts/Ubuntu-B", 14));
            settings.SetPosition(12, 26);

            GroupBox box1 = new GroupBox(this);
            box1.SetPosition(19, 47);
            box1.SetSize(450, 203);

            Font f = Font.Get("Fonts/ProductSans-M", 12);

            Label namelabel = new Label(box1);
            namelabel.SetText("Working Name:");
            namelabel.SetFont(f);
            namelabel.SetPosition(7, 6);
            MapName = new TextBox(box1);
            MapName.SetPosition(6, 22);
            MapName.SetSize(136, 27);
            MapName.SetInitialText(Map.DevName);
            MapName.OnTextChanged += delegate (object sender, EventArgs e)
            {
                this.Map.DevName = MapName.Text;
            };

            Label displaynamelabel = new Label(box1);
            displaynamelabel.SetText("In-game Name:");
            displaynamelabel.SetFont(f);
            displaynamelabel.SetPosition(7, 52);
            DisplayName = new TextBox(box1);
            DisplayName.SetPosition(6, 68);
            DisplayName.SetSize(136, 27);
            DisplayName.SetInitialText(Map.DisplayName);
            DisplayName.OnTextChanged += delegate (object sender, EventArgs e)
            {
                this.Map.DisplayName = DisplayName.Text;
            };

            Label widthlabel = new Label(box1);
            widthlabel.SetText("Width:");
            widthlabel.SetFont(f);
            widthlabel.SetPosition(7, 99);
            Width = new NumericBox(box1);
            Width.SetPosition(6, 115);
            Width.MinValue = 1;
            Width.MaxValue = 255;
            Width.SetSize(66, 27);
            Width.SetValue(this.Map.Width);
            Width.OnValueChanged += delegate (object sender, EventArgs e)
            {
                this.Map.Width = Width.Value;
            };

            Label heightlabel = new Label(box1);
            heightlabel.SetText("Height:");
            heightlabel.SetFont(f);
            heightlabel.SetPosition(78, 99);
            Height = new NumericBox(box1);
            Height.SetPosition(77, 115);
            Height.MinValue = 1;
            Height.MaxValue = 255;
            Height.SetSize(66, 27);
            Height.SetValue(this.Map.Height);
            Height.OnValueChanged += delegate (object sender, EventArgs e)
            {
                this.Map.Height = Height.Value;
            };

            Tilesets = new ListBox(box1);
            Tilesets.SetPosition(162, 22);
            List<ListItem> tilesetitems = new List<ListItem>();
            for (int i = 0; i < this.Map.TilesetIDs.Count; i++)
            {
                int id = this.Map.TilesetIDs[i];
                Tileset tileset = Data.Tilesets[id];
                tilesetitems.Add(new ListItem(tileset));
            }
            Tilesets.SetItems(tilesetitems);
            Tilesets.SetButtonText("Add Tileset");
            Tilesets.ListDrawer.SetContextMenuList(new List<IMenuItem>()
            {
                new MenuItem("Move Tileset Up")
                {
                    IsClickable = delegate (object sender, ConditionEventArgs e)
                    {
                        e.ConditionValue = Tilesets.SelectedIndex > 0;
                    },
                    OnLeftClick = MoveTilesetUp
                },
                new MenuItem("Move Tileset Down")
                {
                    IsClickable = delegate (object sender, ConditionEventArgs e)
                    {
                        e.ConditionValue = Tilesets.SelectedIndex < Tilesets.Items.Count - 1;
                    },
                    OnLeftClick = MoveTilesetDown
                },
                new MenuSeparator(),
                new MenuItem("Remove Tileset")
                {
                    IsClickable = delegate (object sender, ConditionEventArgs e)
                    {
                        e.ConditionValue = Tilesets.Items.Count > 1;
                    },
                    OnLeftClick = RemoveTileset
                }
            });
            // Makes it so you can right click on the whole widget, except for the last 20 pixels (which is the button)
            Tilesets.ListDrawer.OnContextMenuOpening += delegate (object sender, CancelEventArgs e)
            {
                int ry = Graphics.LastMouseEvent.Y - Tilesets.ListDrawer.Viewport.Y + Tilesets.ListDrawer.Position.Y - Tilesets.ListDrawer.ScrolledPosition.Y;
                if (ry >= Tilesets.ListDrawer.Size.Height - 20) e.Cancel = true;
            };
            Tilesets.ListDrawer.OnButtonClicked += AddTileset;
            Label tilesetslabel = new Label(box1);
            tilesetslabel.SetText("Tilesets:");
            tilesetslabel.SetFont(f);
            tilesetslabel.SetPosition(163, 6);

            Autotiles = new ListBox(box1);
            Autotiles.SetPosition(312, 22);
            List<ListItem> autotileitems = new List<ListItem>();
            for (int i = 0; i < this.Map.AutotileIDs.Count; i++)
            {
                int id = this.Map.AutotileIDs[i];
                Autotile autotile = Data.Autotiles[id];
                autotileitems.Add(new ListItem(autotile));
            }
            Autotiles.SetItems(autotileitems);
            Autotiles.SetButtonText("Add Autotile");
            // Makes it so you can right click on the whole widget, except for the last 20 pixels (which is the button)
            Autotiles.ListDrawer.OnContextMenuOpening += delegate (object sender, CancelEventArgs e)
            {
                int ry = Graphics.LastMouseEvent.Y - Autotiles.ListDrawer.Viewport.Y + Autotiles.ListDrawer.Position.Y - Autotiles.ListDrawer.ScrolledPosition.Y;
                if (ry >= Autotiles.ListDrawer.Size.Height - 20) e.Cancel = true;
            };
            Autotiles.ListDrawer.OnButtonClicked += AddAutotile;
            Label autotileslabel = new Label(box1);
            autotileslabel.SetText("Autotiles:");
            autotileslabel.SetFont(f);
            autotileslabel.SetPosition(313, 6);

            CreateButton("Cancel", Cancel);
            CreateButton("OK", OK);
        }

        public void AddTileset(object sender, EventArgs e)
        {
            TilesetPickerMap picker = new TilesetPickerMap(this.Map, Window);
            picker.OnClosed += delegate (object sender2, EventArgs e2)
            {
                bool update = false;
                if (Map.TilesetIDs.Count != picker.ResultIDs.Count) update = true;
                if (!update)
                {
                    for (int i = 0; i < picker.ResultIDs.Count; i++)
                    {
                        if (picker.ResultIDs[i] != Map.TilesetIDs[i])
                        {
                            update = true;
                            break;
                        }
                    }
                }
                if (update)
                {
                    Map.TilesetIDs = picker.ResultIDs;
                    List<ListItem> tilesetitems = new List<ListItem>();
                    for (int i = 0; i < this.Map.TilesetIDs.Count; i++)
                    {
                        int id = this.Map.TilesetIDs[i];
                        Tileset tileset = Data.Tilesets[id];
                        tilesetitems.Add(new ListItem(tileset));
                    }
                    Tilesets.SetItems(tilesetitems);
                }
            };
        }

        public void MoveTilesetUp(object sender, EventArgs e)
        {
            if (Tilesets.SelectedIndex > 0)
            {
                Tilesets.Items.Swap(Tilesets.SelectedIndex - 1, Tilesets.SelectedIndex);
                Map.TilesetIDs.Swap(Tilesets.SelectedIndex - 1, Tilesets.SelectedIndex);
                Tilesets.Redraw();
            }
        }

        public void MoveTilesetDown(object sender, EventArgs e)
        {
            if (Tilesets.SelectedIndex < Tilesets.Items.Count - 1)
            {
                Tilesets.Items.Swap(Tilesets.SelectedIndex + 1, Tilesets.SelectedIndex);
                Map.TilesetIDs.Swap(Tilesets.SelectedIndex + 1, Tilesets.SelectedIndex);
                Tilesets.Redraw();
            }
        }

        public void RemoveTileset(object sender, EventArgs e)
        {
            if (Tilesets.Items.Count > 1)
            {
                Tilesets.Items.RemoveAt(Tilesets.SelectedIndex);
                Map.TilesetIDs.RemoveAt(Tilesets.SelectedIndex);
                Tilesets.SetSelectedIndex(-1);
                Tilesets.Redraw();
            }
        }

        public void AddAutotile(object sender, EventArgs e)
        {
            AutotilePicker picker = new AutotilePicker(this.Map, Window);
            picker.OnClosed += delegate (object sender2, EventArgs e2)
            {
                bool update = false;
                if (Map.AutotileIDs.Count != picker.ResultIDs.Count) update = true;
                if (!update)
                {
                    for (int i = 0; i < picker.ResultIDs.Count; i++)
                    {
                        if (picker.ResultIDs[i] != Map.AutotileIDs[i])
                        {
                            update = true;
                            break;
                        }
                    }
                }
                if (update)
                {
                    Map.AutotileIDs = picker.ResultIDs;
                    List<ListItem> autotileitems = new List<ListItem>();
                    for (int i = 0; i < this.Map.AutotileIDs.Count; i++)
                    {
                        int id = this.Map.AutotileIDs[i];
                        Autotile autotile = Data.Autotiles[id];
                        autotileitems.Add(new ListItem(autotile));
                    }
                    Autotiles.SetItems(autotileitems);
                }
            };
        }

        public void OK(object sender, EventArgs e)
        {
            this.UpdateMapViewer = true;
            Action Finalize = delegate
            {
                Close();
            };
            Action Continue = delegate
            {
                // Updates autotiles
                bool autotileschanged = false;
                if (Map.AutotileIDs.Count != OldMap.AutotileIDs.Count) autotileschanged = true;
                if (!autotileschanged)
                    for (int i = 0; i < Map.AutotileIDs.Count; i++)
                    {
                        if (Map.AutotileIDs[i] != OldMap.AutotileIDs[i])
                        {
                            autotileschanged = true;
                            break;
                        }
                    }
                if (autotileschanged)
                {
                    UnsavedChanges = true;
                    bool warn = false;
                    for (int layer = 0; layer < Map.Layers.Count; layer++)
                    {
                        for (int i = 0; i < Map.Width * Map.Height; i++)
                        {
                            if (Map.Layers[layer].Tiles[i] == null || Map.Layers[layer].Tiles[i].TileType == TileType.Tileset) continue;
                            int autotileID = OldMap.AutotileIDs[Map.Layers[layer].Tiles[i].Index];
                            if (!Map.AutotileIDs.Contains(autotileID))
                            {
                                warn = true;
                                break;
                            }
                        }
                    }
                    if (warn)
                    {
                        MessageBox msg = new MessageBox("Warning", "One of the deleted autotiles was still in use. By choosing to continue, tiles of that autotile will be deleted.", new List<string>() { "Continue", "Cancel" });
                        msg.OnButtonPressed += delegate (object sender2, EventArgs e2)
                        {
                            if (msg.Result == 0) // Continue
                            {
                                for (int layer = 0; layer < Map.Layers.Count; layer++)
                                {
                                    for (int i = 0; i < Map.Width * Map.Height; i++)
                                    {
                                        if (Map.Layers[layer].Tiles[i] == null || Map.Layers[layer].Tiles[i].TileType == TileType.Tileset) continue;
                                        int autotileID = OldMap.AutotileIDs[Map.Layers[layer].Tiles[i].Index];
                                        if (!Map.AutotileIDs.Contains(autotileID))
                                        {
                                            Map.Layers[layer].Tiles[i] = null;
                                        }
                                        else Map.Layers[layer].Tiles[i].Index = Map.AutotileIDs.IndexOf(autotileID);
                                    }
                                }
                                Finalize();
                            }
                            else if (msg.Result == 1) // Cancel
                            {
                                UnsavedChanges = false;
                                UpdateMapViewer = false;
                            }
                        };
                    }
                    else
                    {
                        for (int layer = 0; layer < Map.Layers.Count; layer++)
                        {
                            for (int i = 0; i < Map.Width * Map.Height; i++)
                            {
                                if (Map.Layers[layer].Tiles[i] == null || Map.Layers[layer].Tiles[i].TileType == TileType.Tileset) continue;
                                int autotileID = OldMap.AutotileIDs[Map.Layers[layer].Tiles[i].Index];
                                if (!Map.AutotileIDs.Contains(autotileID))
                                {
                                    throw new Exception("Impossible-to-reach code has been reached.");
                                }
                                else Map.Layers[layer].Tiles[i].Index = Map.AutotileIDs.IndexOf(autotileID);
                            }
                        }
                        Finalize();
                    }
                }
                if (!autotileschanged) Finalize();
            };
            // Resizes Map
            if (Map.Width != OldMap.Width || Map.Height != OldMap.Height)
            {
                UnsavedChanges = true;
                int diffw = Map.Width - OldMap.Width;
                bool diffwneg = diffw < 0;
                diffw = Math.Abs(diffw);
                int diffh = Map.Height - OldMap.Height;
                bool diffhneg = diffh < 0;
                diffh = Math.Abs(diffh);
                for (int layer = 0; layer < Map.Layers.Count; layer++)
                {
                    for (int y = 0; y < OldMap.Height; y++)
                    {
                        for (int i = 0; i < diffw; i++)
                        {
                            if (diffwneg) Map.Layers[layer].Tiles.RemoveAt(y * Map.Width + Map.Width);
                            else Map.Layers[layer].Tiles.Insert(y * Map.Width + OldMap.Width, null);
                        }
                    }
                }
                for (int layer = 0; layer < Map.Layers.Count; layer++)
                {
                    if (diffhneg) Map.Layers[layer].Tiles.RemoveRange(Map.Width * Map.Height, diffh * Map.Width);
                    else
                    {
                        for (int i = 0; i < diffh * Map.Width; i++)
                        {
                            Map.Layers[layer].Tiles.Add(null);
                        }
                    }
                }
            }
            // Marks name change
            if (Map.DevName != OldMap.DevName || Map.DisplayName != OldMap.DisplayName) UnsavedChanges = true;
            // Updates tilesets
            bool tilesetschanged = false;
            if (Map.TilesetIDs.Count != OldMap.TilesetIDs.Count) tilesetschanged = true;
            if (!tilesetschanged)
                for (int i = 0; i < Map.TilesetIDs.Count; i++)
                {
                    if (Map.TilesetIDs[i] != OldMap.TilesetIDs[i])
                    {
                        tilesetschanged = true;
                        break;
                    }
                }
            if (tilesetschanged)
            {
                UnsavedChanges = true;
                bool warn = false;
                for (int layer = 0; layer < Map.Layers.Count; layer++)
                {
                    for (int i = 0; i < Map.Width * Map.Height; i++)
                    {
                        if (Map.Layers[layer].Tiles[i] == null || Map.Layers[layer].Tiles[i].TileType == TileType.Autotile) continue;
                        int tilesetID = OldMap.TilesetIDs[Map.Layers[layer].Tiles[i].Index];
                        if (!Map.TilesetIDs.Contains(tilesetID))
                        {
                            warn = true;
                            break;
                        }
                    }
                }
                if (warn)
                {
                    MessageBox msg = new MessageBox("Warning", "One of the deleted tilesets was still in use. By choosing to continue, tiles of that tileset will be deleted.", new List<string>() { "Continue", "Cancel" });
                    msg.OnButtonPressed += delegate (object sender2, EventArgs e2)
                    {
                        if (msg.Result == 0) // Continue
                        {
                            for (int layer = 0; layer < Map.Layers.Count; layer++)
                            {
                                for (int i = 0; i < Map.Width * Map.Height; i++)
                                {
                                    if (Map.Layers[layer].Tiles[i] == null || Map.Layers[layer].Tiles[i].TileType == TileType.Autotile) continue;
                                    int tilesetID = OldMap.TilesetIDs[Map.Layers[layer].Tiles[i].Index];
                                    if (!Map.TilesetIDs.Contains(tilesetID))
                                    {
                                        Map.Layers[layer].Tiles[i] = null;
                                    }
                                    else Map.Layers[layer].Tiles[i].Index = Map.TilesetIDs.IndexOf(tilesetID);
                                }
                            }
                            Continue();
                        }
                        else if (msg.Result == 1) // Cancel
                        {
                            UnsavedChanges = false;
                            UpdateMapViewer = false;
                        }
                    };
                }
                else
                {
                    for (int layer = 0; layer < Map.Layers.Count; layer++)
                    {
                        for (int i = 0; i < Map.Width * Map.Height; i++)
                        {
                            if (Map.Layers[layer].Tiles[i] == null || Map.Layers[layer].Tiles[i].TileType == TileType.Autotile) continue;
                            int tilesetID = OldMap.TilesetIDs[Map.Layers[layer].Tiles[i].Index];
                            if (!Map.TilesetIDs.Contains(tilesetID))
                            {
                                throw new Exception("Impossible-to-reach code has been reached.");
                            }
                            else Map.Layers[layer].Tiles[i].Index = Map.TilesetIDs.IndexOf(tilesetID);
                        }
                    }
                    Continue();
                }
            }
            if (!tilesetschanged) Continue();
        }

        public void Cancel(object sender, EventArgs e)
        {
            Close();
        }
    }

    public class GroupBox : Widget
    {
        public GroupBox(object Parent, string Name = "groupBox")
            : base(Parent, Name)
        {
            Sprites["bg"] = new Sprite(this.Viewport);
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            if (Sprites["bg"].Bitmap != null) Sprites["bg"].Bitmap.Dispose();
            Sprites["bg"].Bitmap = new Bitmap(Size);
            Sprites["bg"].Bitmap.Unlock();
            Sprites["bg"].Bitmap.DrawRect(Size, 59, 91, 124);
            Sprites["bg"].Bitmap.DrawRect(1, 1, Size.Width - 2, Size.Height - 2, 17, 27, 38);
            Sprites["bg"].Bitmap.FillRect(2, 2, Size.Width - 4, Size.Height - 4, 24, 38, 53);
            Sprites["bg"].Bitmap.Lock();
        }
    }
}
