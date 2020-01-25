using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.Widgets
{
    public class TilesetEditor : Widget
    {
        public DatabaseDataList DBDataList;

        TabView TabView;
        TabContainer PassageContainer;
        TabContainer FourDirContainer;
        TabContainer PriorityContainer;

        Container SharedContainer;

        TilesetDisplay PassageList;
        TilesetDisplay FourDirList;

        Label NameLabel;
        TextBox NameBox;
        Label GraphicLabel;
        FileBrowserBox GraphicBox;
        Button ClearTilesetButton;

        Bitmap small_up = new Bitmap("database_tileset_small_up.png");
        Bitmap small_left = new Bitmap("database_tileset_small_left.png");
        Bitmap small_right = new Bitmap("database_tileset_small_right.png");
        Bitmap small_down = new Bitmap("database_tileset_small_down.png");
        Bitmap big_up = new Bitmap("database_tileset_big_up.png");
        Bitmap big_left = new Bitmap("database_tileset_big_left.png");
        Bitmap big_right = new Bitmap("database_tileset_big_right.png");
        Bitmap big_down = new Bitmap("database_tileset_big_down.png");
        Bitmap passable = new Bitmap("database_tileset_passable.png");
        Bitmap impassable = new Bitmap("database_tileset_impassable.png");

        Game.Tileset Tileset;
        int TilesetID;

        public TilesetEditor(object Parent, string Name = "tilesetEditor")
            : base(Parent, Name)
        {
            TabView = new TabView(this);
            TabView.SetHeader(86, 33, 12);
            PassageContainer = TabView.CreateTab("Passage");
            FourDirContainer = TabView.CreateTab("4-Dir");
            PriorityContainer = TabView.CreateTab("Priority");
            //TabView.CreateTab("Terrain Tag");
            //TabView.CreateTab("Bush Flag");
            //TabView.CreateTab("Counter Flag");

            Container PassageSubContainer = new Container(PassageContainer);
            PassageList = new TilesetDisplay(PassageSubContainer);
            PassageList.OnTilesetLoaded += delegate (object sender, EventArgs e)
            {
                PassageDrawAll();
            };
            PassageList.OnTileClicked += delegate (object sender, PointEventArgs e)
            {
                PassageInput(e);
            };

            Container FourDirSubContainer = new Container(FourDirContainer);
            FourDirList = new TilesetDisplay(FourDirSubContainer);
            FourDirList.OnTilesetLoaded += delegate (object sender, EventArgs e)
            {
                FourDirDrawAll();
            };
            FourDirList.OnTileClicked += delegate (object sender, PointEventArgs e)
            {
                FourDirInput(e);
            };

            PassageContainer.SetBackgroundColor(28, 50, 73);
            FourDirContainer.SetBackgroundColor(28, 50, 73);
            PriorityContainer.SetBackgroundColor(28, 50, 73);

            SharedContainer = new Container(this);
            SharedContainer.SetPosition(22, 41);
            SharedContainer.Sprites["bg"] = new Sprite(SharedContainer.Viewport);
            SimpleFade fade = new SimpleFade(SharedContainer);
            fade.SetPosition(4, 4);
            NameLabel = new Label(SharedContainer);
            NameLabel.SetText("Name");
            NameLabel.SetFont(Font.Get("Fonts/Ubuntu-B", 14));
            NameLabel.SetPosition(19, 16);
            NameBox = new TextBox(SharedContainer);
            NameBox.SetPosition(19, 40);
            NameBox.SetSize(156, 21);
            NameBox.SetSkin(1);
            // Updates tileset list
            NameBox.OnTextChanged += delegate (object sender, EventArgs e)
            {
                if (this.Tileset == null) return;
                this.Tileset.Name = NameBox.Text;
                ListItem item = DBDataList.DataList.Items[TilesetID - 1];
                item.Name = item.Name.Split(':')[0] + ": " + this.Tileset.Name;
                DBDataList.DataList.Redraw();
            };

            GraphicLabel = new Label(SharedContainer);
            GraphicLabel.SetFont(Font.Get("Fonts/Ubuntu-B", 14));
            GraphicLabel.SetPosition(19, 79);
            GraphicLabel.SetText("Tileset Graphic");

            GraphicBox = new FileBrowserBox(SharedContainer);
            GraphicBox.SetPosition(19, 103);
            GraphicBox.SetSize(156, 21);
            // Updates graphic if browsed
            GraphicBox.OnFileChosen += delegate (object sender, ObjectEventArgs e)
            {
                // Converts path (C:\...\...\tileset_image.png) to filename (tileset_image)
                string path = e.Value as string;
                while (path.Contains('/')) path = path.Replace('/', '\\');
                string[] folders = path.Split('\\');
                string file_ext = folders[folders.Length - 1];
                string[] dots = file_ext.Split('.');
                string file = "";
                for (int i = 0; i < dots.Length - 1; i++)
                {
                    file += dots[i];
                    if (i != dots.Length - 2) file += '.';
                }
                string tilesetsfolder = Game.Data.ProjectPath + "\\gfx\\tilesets";
                while (tilesetsfolder.Contains('/')) tilesetsfolder = tilesetsfolder.Replace('/', '\\');
                // Selected file not in the tilesets folder
                // Copies source to tilesets folder
                if (System.IO.Directory.GetParent(path).FullName != tilesetsfolder)
                {
                    MessageBox box = new MessageBox("Error",
                        "The selected file doesn't exist in the gfx/tilesets folder. Would you like to import it?", ButtonTypes.YesNo);
                    box.OnButtonPressed += delegate (object sender, EventArgs e)
                    {
                        if (box.Result == 0) // Yes
                        {
                            string newfilename = null;
                            if (System.IO.File.Exists(tilesetsfolder + "\\" + file_ext))
                            {
                                int iterator = 1;
                                while (string.IsNullOrEmpty(newfilename))
                                {
                                    if (!System.IO.File.Exists(tilesetsfolder + "\\" + file + " (" + iterator.ToString() + ")." + dots[dots.Length - 1]))
                                    {
                                        newfilename = tilesetsfolder + "\\" + file + " (" + iterator.ToString() + ")." + dots[dots.Length - 1];
                                        file = file + " (" + iterator.ToString() + ")";
                                    }
                                    iterator++;
                                }
                            }
                            else
                            {
                                newfilename = tilesetsfolder + "\\" + file_ext;
                            }
                            System.IO.File.Copy(path, newfilename);
                            SetTilesetGraphic(file);
                        }
                    };
                }
                // File is in tilesets folder
                else
                {
                    SetTilesetGraphic(file);
                }
            };
            // Updates graphic if typed
            GraphicBox.TextArea.OnWidgetDeselected += delegate (object sender, EventArgs e)
            {
                string file = GraphicBox.Text;
                if (!System.IO.File.Exists(Game.Data.ProjectPath + "\\gfx\\tilesets\\" + file + ".png"))
                {
                    new MessageBox("Error", "No tileset with the name '" + file + "' exists in gfx/tilesets.");
                }
                else
                {
                    SetTilesetGraphic(file);
                }
            };

            ClearTilesetButton = new Button(SharedContainer);
            ClearTilesetButton.SetPosition(25, 150);
            ClearTilesetButton.SetSize(140, 44);
            ClearTilesetButton.SetFont(Font.Get("Fonts/Ubuntu-B", 16));
            ClearTilesetButton.SetText("Clear Tileset");
            ClearTilesetButton.OnClicked += delegate (object sender, EventArgs e)
            {
                ConfirmClearTileset();
            };

            TabView.SelectTab(1);
        }

        public void SetTileset(Game.Tileset Tileset, int ID)
        {
            this.Tileset = Tileset;
            this.TilesetID = ID;
            PassageList.SetSize(277, Size.Height - PassageList.Position.Y - 93);
            FourDirList.SetSize(277, Size.Height - PassageList.Position.Y - 93);

            PassageList.SetTileset(Tileset);
            FourDirList.SetTileset(Tileset);

            NameBox.SetInitialText(Tileset == null ? "" : Tileset.Name);
            GraphicBox.SetInitialText(Tileset == null ? "" : Tileset.GraphicName);
        }

        public void SetTilesetGraphic(string GraphicName)
        {
            if (this.Tileset == null)
            {
                Game.Tileset t = new Game.Tileset();
                t.ID = TilesetID;
                t.Name = GraphicName;
                t.SetGraphic(GraphicName);
                Game.Data.Tilesets[TilesetID] = t;
                this.Tileset = t;
                ListItem item = DBDataList.DataList.Items[TilesetID - 1];
                item.Object = this.Tileset;
                item.Name = item.Name.Split(':')[0] + ": " + this.Tileset.Name;
                DBDataList.DataList.Redraw();
                this.SetTileset(this.Tileset, TilesetID);
            }
            else if (this.Tileset.GraphicName != GraphicName)
            {
                this.Tileset.SetGraphic(GraphicName);
                this.SetTileset(this.Tileset, TilesetID);
            }
        }

        public void ConfirmClearTileset()
        {
            List<int> Maps = new List<int>();
            foreach (KeyValuePair<int, Game.Map> kvp in Game.Data.Maps)
            {
                if (kvp.Value.TilesetIDs.Contains(TilesetID)) Maps.Add(kvp.Key);
            }
            if (Maps.Count > 0)
            {
                bool plural = Maps.Count > 1;
                MessageBox box = new MessageBox("Warning",
                    $"This tileset is currently being used by {Maps.Count} map{(plural ? "s" : "")}.\n" +
                    $"The tiles from this tileset that are being used on {(plural ? "those maps" : "that map")} will be deleted.\n" +
                    "Are you sure you'd like to delete those tiles?",
                    new List<string>() { "Delete", "Cancel" });
                box.OnButtonPressed += delegate (object sender, EventArgs e)
                {
                    if (box.Result == 0) // Delete
                    {
                        foreach (int MapID in Maps)
                        {
                            Game.Data.Maps[MapID].RemoveTileset(TilesetID);
                        }
                        ClearTileset();
                    }
                };
            }
            else
            {
                ClearTileset();
            }
        }

        public void ClearTileset()
        {
            this.Tileset.TilesetBitmap.Dispose();
            this.Tileset.TilesetListBitmap.Dispose();
            ListItem item = DBDataList.DataList.Items[TilesetID - 1];
            item.Name = item.Name.Split(':')[0] + ": ";
            item.Object = null;
            DBDataList.DataList.Redraw();
            Game.Data.Tilesets[TilesetID] = null;
            this.SetTileset(null, TilesetID);
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            TabView.SetSize(this.Size);
            if (SharedContainer.Sprites["bg"].Bitmap != null) SharedContainer.Sprites["bg"].Bitmap.Dispose();

            int sw = 585;
            int sh = Size.Height - 64;

            if (Window.Width < 980)
            {
                sw = Window.Width - 400;
            }

            int tx = sw - 270;
            int ty = 17;

            if (Window.Width < 880)
            {
                tx = 41;
                ty = 100;
            }

            int th = Size.Height - ty - 83;

            SharedContainer.SetSize(sw, sh);
            SharedContainer.Sprites["bg"].Bitmap = new Bitmap(sw, sh);
            SharedContainer.Sprites["bg"].Bitmap.Unlock();
            SharedContainer.Sprites["bg"].Bitmap.DrawRect(0, 0, sw, sh, 36, 57, 79);
            SharedContainer.Sprites["bg"].Bitmap.DrawRect(1, 1, sw - 2, sh - 2, 44, 64, 85);
            SharedContainer.Sprites["bg"].Bitmap.DrawRect(2, 2, sw - 4, sh - 4, 49, 69, 90);
            SharedContainer.Sprites["bg"].Bitmap.DrawRect(3, 3, sw - 6, sh - 6, 52, 72, 92);
            SharedContainer.Sprites["bg"].Bitmap.Lock();
            SharedContainer.Widgets[0].SetSize(sw - 8, sh - 8); // Vignette fade

            PassageList.SetPosition(tx, ty);
            FourDirList.SetPosition(tx, ty);

            PassageList.SetHeight(th);
            FourDirList.SetHeight(th);
        }

        public void FourDirDrawAll()
        {
            Bitmap bmp = FourDirList.TilesetBox.Sprites["controls"].Bitmap;
            bmp.Unlock();
            int tileycount = (int) Math.Floor(this.Tileset.TilesetBitmap.Height / 32d);
            for (int y = 0; y < tileycount; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    FourDirDrawTile(x, y);
                }
            }
            bmp.Lock();
        }

        public void FourDirInput(PointEventArgs p)
        {
            bool LeftButton = p.LeftButton;
            bool RightButton = p.RightButton;
            bool MiddleButton = p.MiddleButton;

            int TileX = (int) Math.Floor(p.X / 33d);
            int TileY = (int) Math.Floor(p.Y / 33d);
            int rx = p.X - TileX * 33;
            int ry = p.Y - TileY * 33;
            bool up = false;
            bool left = false;
            bool right = false;
            bool down = false;
            if (ry <= 15 &&
                rx >= 1 + ry && rx <= 31 - ry) up = true;
            else if (ry >= 16 &&
                rx >= 31 - ry && rx <= ry - 1) down = true;
            else if (rx <= 15 &&
                ry >= rx && ry < 31 - rx) left = true;
            else right = true;
            Game.Passability pass = this.Tileset.Passabilities[TileY * 8 + TileX];
            if (up)
            {
                if ((pass & Game.Passability.Up) == Game.Passability.Up) pass -= Game.Passability.Up;
                else pass |= Game.Passability.Up;
            }
            else if (left)
            {
                if ((pass & Game.Passability.Left) == Game.Passability.Left) pass -= Game.Passability.Left;
                else pass |= Game.Passability.Left;
            }
            else if (right)
            {
                if ((pass & Game.Passability.Right) == Game.Passability.Right) pass -= Game.Passability.Right;
                else pass |= Game.Passability.Right;
            }
            else if (down)
            {
                if ((pass & Game.Passability.Down) == Game.Passability.Down) pass -= Game.Passability.Down;
                else pass |= Game.Passability.Down;
            }
            else
            {
                throw new Exception($"Input management incorrect (rx,ry) = ({rx},{ry})");
            }
            this.Tileset.Passabilities[TileY * 8 + TileX] = pass;
            Bitmap bmp4dir = FourDirList.TilesetBox.Sprites["controls"].Bitmap;
            bmp4dir.Unlock();
            bmp4dir.FillRect(TileX * 33, TileY * 33, 32, 32, Color.ALPHA);
            FourDirDrawTile(TileX, TileY);
            bmp4dir.Lock();
            Bitmap bmppass = PassageList.TilesetBox.Sprites["controls"].Bitmap;
            bmppass.Unlock();
            bmppass.FillRect(TileX * 33, TileY * 33, 32, 32, Color.ALPHA);
            PassageDrawTile(TileX, TileY);
            bmppass.Lock();
        }

        public void FourDirDrawTile(int TileX, int TileY)
        {
            Bitmap bmp = FourDirList.TilesetBox.Sprites["controls"].Bitmap;
            Game.Passability p = this.Tileset.Passabilities[TileY * 8 + TileX];
            int ix = TileX * 33;
            int iy = TileY * 33;
            bool up = (p & Game.Passability.Up) == Game.Passability.Up;
            bool left = (p & Game.Passability.Left) == Game.Passability.Left;
            bool right = (p & Game.Passability.Right) == Game.Passability.Right;
            bool down = (p & Game.Passability.Down) == Game.Passability.Down;
            
            if (up)
            {
                bmp.Build(ix + 8, iy + 1, big_up);
            }
            else
            {
                bmp.Build(ix + 12, iy + 2, small_up);
            }
            if (left)
            {
                bmp.Build(ix + 1, iy + 8, big_left);
            }
            else
            {
                bmp.Build(ix + 2, iy + 12, small_left);
            }
            if (right)
            {
                bmp.Build(ix + 25, iy + 8, big_right);
            }
            else
            {
                bmp.Build(ix + 24, iy + 12, small_right);
            }
            if (down)
            {
                bmp.Build(ix + 8, iy + 25, big_down);
            }
            else
            {
                bmp.Build(ix + 12, iy + 24, small_down);
            }
        }

        public void PassageDrawAll()
        {
            Bitmap bmp = PassageList.TilesetBox.Sprites["controls"].Bitmap;
            bmp.Unlock();
            int tileycount = (int) Math.Floor(this.Tileset.TilesetBitmap.Height / 32d);
            for (int y = 0; y < tileycount; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    PassageDrawTile(x, y);
                }
            }
            bmp.Lock();
        }

        public void PassageInput(PointEventArgs p)
        {
            bool LeftButton = p.LeftButton;
            bool RightButton = p.RightButton;
            bool MiddleButton = p.MiddleButton;

            int TileX = (int) Math.Floor(p.X / 33d);
            int TileY = (int) Math.Floor(p.Y / 33d);

            Game.Passability pass = this.Tileset.Passabilities[TileY * 8 + TileX];
            if (pass == Game.Passability.None) pass = Game.Passability.All;
            else pass = Game.Passability.None;
            this.Tileset.Passabilities[TileY * 8 + TileX] = pass;

            Bitmap bmp4dir = FourDirList.TilesetBox.Sprites["controls"].Bitmap;
            bmp4dir.Unlock();
            bmp4dir.FillRect(TileX * 33, TileY * 33, 32, 32, Color.ALPHA);
            FourDirDrawTile(TileX, TileY);
            bmp4dir.Lock();
            Bitmap bmppass = PassageList.TilesetBox.Sprites["controls"].Bitmap;
            bmppass.Unlock();
            bmppass.FillRect(TileX * 33, TileY * 33, 32, 32, Color.ALPHA);
            PassageDrawTile(TileX, TileY);
            bmppass.Lock();
        }

        public void PassageDrawTile(int TileX, int TileY)
        {
            Bitmap bmp = PassageList.TilesetBox.Sprites["controls"].Bitmap;
            Game.Passability p = this.Tileset.Passabilities[TileY * 8 + TileX];
            int ix = TileX * 33;
            int iy = TileY * 33;
            if (p == Game.Passability.None)
                bmp.Build(ix, iy, impassable);
            else bmp.Build(ix, iy, passable);
        }
    }
}
