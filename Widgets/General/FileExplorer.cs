using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace RPGStudioMK.Widgets;

public class FileExplorer : Widget
{
    public List<string> Extensions { get; protected set; } = new List<string>();
    public string BaseDirectory { get; protected set; }
    public string Directory { get; protected set; }

    public DirectoryViewMode ViewMode { get; protected set; } = DirectoryViewMode.LargeThumbnails;

    FileEntryWidget? SelectedFEW => (FileEntryWidget) DirGrid.Widgets.Find(w => ((FileEntryWidget) w).Selected);
    public string SelectedFilename => SelectedFEW?.Filename;
    public bool SelectedIsFolder => SelectedFEW?.IsFolder ?? false;
    public bool SelectedIsFile => SelectedFEW?.IsFile ?? false;

    public BaseEvent OnFileSelected;
    public BaseEvent OnFileDoubleClicked;

    List<Label> PathLabels = new List<Label>();
    Container PathContainer;
    Container GridContainer;
    Grid DirGrid;
    Label EmptyLabel;
    int MaxPathX = 0;
    int ColumnCount;
    double ColumnMargin = 0;

    Thread LoaderThread;
    List<FileEntryWidget> LoadPool = new List<FileEntryWidget>();

    public FileExplorer(IContainer Parent) : base(Parent)
    {
        Sprites["box"] = new Sprite(this.Viewport);
        PathContainer = new Container(this);
        PathContainer.SetPosition(60, 11);

        GridContainer = new Container(this);
        VScrollBar vs = new VScrollBar(this);
        GridContainer.SetVScrollBar(vs);
        GridContainer.VAutoScroll = true;
        GridContainer.SetPosition(18, 53);
        DirGrid = new Grid(GridContainer);
        DirGrid.SetDocked(false);
        DirGrid.SetHDocked(true);

        SetSize(478, 325);
    }

    public void SetBaseDirectory(string Directory)
    {
        while (Directory.Contains('\\')) Directory = Directory.Replace('\\', '/');
        this.BaseDirectory = Directory;
    }

    public void SetFileExtensions(params string[] Extensions)
    {
        this.Extensions = Extensions.ToList();
    }

    public void SetDirectory(string Directory)
    {
        if (this.Directory != Directory)
        {
            while (Directory.Contains('\\')) Directory = Directory.Replace('\\', '/');
            this.Directory = Directory;
            RedrawDirectory();
        }
    }

    public void SetSelectedFile(string Filename)
    {
        if (string.IsNullOrEmpty(Filename))
        {
            DirGrid.Widgets.ForEach(w => ((FileEntryWidget) w).SetSelected(false));
        }
        else
        {
            bool found = false;
            foreach (FileEntryWidget few in DirGrid.Widgets)
            {
                if (few.Filename.EndsWith(Filename))
                {
                    few.SetSelected(true);
                    OnFileSelected?.Invoke(new BaseEventArgs());
                    found = true;
                    break;
                }
            }
            if (!found) throw new Exception("File not found to select.");
        }
    }

    public void RedrawDirectory()
    {
        List<string> path = new List<string>() { "  ~  " };
        PathLabels.Clear();
        while (PathContainer.Widgets.Count > 0) PathContainer.Widgets[0].Dispose();
        path.AddRange(this.Directory.Split('/'));
        GridContainer.VScrollBar.SetValue(0);
        int x = 6;
        Font f = Fonts.CabinMedium.Use(11);
        Color arrowcolor = new Color(86, 108, 134);
        Color arrowshadow = new Color(36, 34, 36);
        for (int i = 0; i < path.Count; i++)
        {
            Label dir = new Label(PathContainer);
            dir.SetFont(f);
            dir.SetText(path[i]);
            dir.SetPosition(x, -1);
            PathLabels.Add(dir);
            x += f.TextSize(path[i]).Width;
            if (i != path.Count - 1)
            {
                Widget arrow = new Widget(PathContainer);
                arrow.SetPosition(x + 6, 4);
                arrow.Sprites["arrow"] = new Sprite(arrow.Viewport);
                arrow.Sprites["arrow"].Bitmap = new Bitmap(7, 11);
                arrow.Sprites["arrow"].Bitmap.Unlock();
                arrow.Sprites["arrow"].Bitmap.DrawLine(0, 0, 5, 5, arrowcolor);
                arrow.Sprites["arrow"].Bitmap.DrawLine(1, 0, 6, 5, arrowcolor);
                arrow.Sprites["arrow"].Bitmap.DrawLine(4, 6, 0, 10, arrowcolor);
                arrow.Sprites["arrow"].Bitmap.DrawLine(5, 6, 1, 10, arrowcolor);
                arrow.Sprites["arrow"].Bitmap.DrawLine(0, 1, 4, 5, arrowshadow);
                arrow.Sprites["arrow"].Bitmap.DrawLine(3, 6, 0, 9, arrowshadow);
                arrow.Sprites["arrow"].Bitmap.Lock();
                x += 18;
            }
            MaxPathX = x + 10;
            dir.OnHoverChanged += _ =>
            {
                dir.SetBackgroundColor(dir.Mouse.Inside ? new Color(40, 62, 84) : Color.ALPHA);
            };
            dir.OnLeftMouseDownInside += _ =>
            {
                int idx = PathLabels.IndexOf(dir);
                string path = "";
                for (int i = 1; i <= idx; i++)
                {
                    path += PathLabels[i].Text;
                    if (i != idx) path += "/";
                }
                this.SetDirectory(path);
            };
        }
        if (MaxPathX > PathContainer.Size.Width) PathContainer.ScrolledX = MaxPathX - PathContainer.Size.Width;
        else PathContainer.ScrolledX = 0;
        PathContainer.UpdateBounds();
        RedrawDirectoryItems();
    }

    public void RedrawDirectoryItems()
    {
        while (DirGrid.Widgets.Count > 0) DirGrid.Widgets[0].Dispose();
        EmptyLabel?.Dispose();
        EmptyLabel = null;
        DirGrid.Rows.Clear();
        bool Empty = true;
        foreach (string directory in System.IO.Directory.GetDirectories(this.BaseDirectory + "/" + this.Directory))
        {
            Empty = false;
            string dir = directory;
            while (dir.Contains('\\')) dir = dir.Replace('\\', '/');
            DrawItem(dir, true);
        }

        foreach (string file in System.IO.Directory.GetFiles(this.BaseDirectory + "/" + this.Directory))
        {
            if (!Extensions.Contains(file.Split('.').ToList().Last())) continue;
            string f = file;
            while (f.Contains('\\')) f = f.Replace('\\', '/');
            DrawItem(f, false);
            Empty = false;
        }
        DirGrid.UpdateContainers();
        DirGrid.UpdateLayout();

        if (Empty)
        {
            EmptyLabel = new Label(GridContainer);
            EmptyLabel.SetFont(Fonts.CabinMedium.Use(11));
            EmptyLabel.SetText("This directory does not contain any (relevant) files.");
            EmptyLabel.SetPosition(GridContainer.Size.Width / 2 - EmptyLabel.Size.Width / 2 - 20, GridContainer.Size.Height / 2 - EmptyLabel.Size.Height / 2 - 50);
        }
    }

    public void DrawItem(string Filename, bool IsFolder)
    {
        int ItemColumn = -1;
        int ItemRow = -1;
        for (int column = 0; column < DirGrid.Columns.Count; column++)
        {
            for (int row = 0; row < DirGrid.Rows.Count; row++)
            {
                if (DirGrid.Widgets.Find(w => w.GridColumnStart == column && w.GridRowStart == row) == null)
                {
                    // Free spot
                    ItemColumn = column;
                    ItemRow = row;
                    break;
                }
            }
            if (ItemColumn != -1 && ItemRow != -1) break;
        }
        if (ItemColumn == -1 || ItemRow == -1)
        {
            // No free spot, so add row
            DirGrid.Rows.Add(new GridSize(102, Unit.Pixels));
            ItemColumn = 0;
            ItemRow = DirGrid.Rows.Count - 1;
        }
        if (LoaderThread == null) SpawnLoaderThread();
        FileEntryWidget few = new FileEntryWidget(this, DirGrid);
        few.SetGrid(ItemRow, ItemColumn);
        if (!IsFolder) few.SetFile(Filename);
        else few.SetFolder(Filename);
        few.OnSelectionChanged += delegate (BaseEventArgs e)
        {
            if (few.Selected) OnFileSelected?.Invoke(new BaseEventArgs());
        };
        few.OnDoubleLeftMouseDownInside += _ =>
        {
            if (few.IsFolder)
            {
                this.SetDirectory(few.Filename.Replace(this.BaseDirectory + '/', ""));
            }
            else
            {
                OnFileDoubleClicked?.Invoke(new BaseEventArgs());
            }
        };
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        Sprites["box"].Bitmap?.Dispose();
        Sprites["box"].Bitmap = new Bitmap(Size);
        Sprites["box"].Bitmap.Unlock();
        Color outline = new Color(59, 91, 124);
        Color inline = new Color(17, 27, 38);
        Color filler = new Color(24, 38, 53);
        Color other = new Color(86, 108, 134);
        Sprites["box"].Bitmap.FillRect(0, 0, Size.Width, Size.Height, filler);
        Sprites["box"].Bitmap.DrawRect(0, 0, Size.Width, Size.Height, outline);
        Sprites["box"].Bitmap.DrawRect(1, 1, Size.Width - 2, Size.Height - 2, inline);
        Sprites["box"].Bitmap.DrawLine(1, 34, Size.Width - 2, 34, inline);
        Sprites["box"].Bitmap.DrawLine(1, 35, Size.Width - 2, 35, outline);
        Sprites["box"].Bitmap.DrawLine(1, 36, Size.Width - 2, 36, inline);
        Sprites["box"].Bitmap.DrawLine(Size.Width - 13, 36, Size.Width - 13, Size.Height - 2, inline);
        Sprites["box"].Bitmap.DrawLine(Size.Width - 14, 36, Size.Width - 14, Size.Height - 2, outline);
        Sprites["box"].Bitmap.DrawLine(Size.Width - 15, 36, Size.Width - 15, Size.Height - 2, inline);
        Sprites["box"].Bitmap.SetPixel(60, 11, other);
        Sprites["box"].Bitmap.DrawLine(59, 12, 59, 27, other);
        Sprites["box"].Bitmap.SetPixel(60, 28, other);
        Sprites["box"].Bitmap.DrawLine(61, 10, Size.Width - 10, 10, other);
        Sprites["box"].Bitmap.SetPixel(Size.Width - 9, 11, other);
        Sprites["box"].Bitmap.DrawLine(Size.Width - 8, 12, Size.Width - 8, 27, other);
        Sprites["box"].Bitmap.SetPixel(Size.Width - 9, 28, other);
        Sprites["box"].Bitmap.DrawLine(61, 29, Size.Width - 10, 29, other);
        Sprites["box"].Bitmap.Lock();

        PathContainer.SetSize(Size.Width - 68, 18);
        PathContainer.ScrolledX = MaxPathX - PathContainer.Size.Width;
        PathContainer.UpdateBounds();
        GridContainer.SetSize(Size.Width - DirGrid.Position.X - 7, Size.Height - DirGrid.Position.Y - 18);
        int OldCount = ColumnCount;
        ColumnCount = (int)Math.Floor(DirGrid.Size.Width / 106d);
        ColumnMargin = (DirGrid.Size.Width % 106) / (double)ColumnCount;
        if (OldCount != ColumnCount)
        {
            DirGrid.Columns.Clear();
            bool lastfloor = true;
            for (int i = 0; i < ColumnCount; i++)
            {
                int margin = lastfloor ? (int)Math.Ceiling(ColumnMargin) : (int)Math.Floor(ColumnMargin);
                lastfloor = !lastfloor;
                DirGrid.Columns.Add(new GridSize(106 + margin, Unit.Pixels));
            }
            DirGrid.UpdateContainers();
        }
        else
        {
            DirGrid.Widgets.ForEach(w =>
            {
                w.SetPadding((int)Math.Floor(ColumnMargin), 4, 0, 0);
            });
        }
        DirGrid.UpdateLayout();

        GridContainer.VScrollBar.SetPosition(Size.Width - 11, 38);
        GridContainer.VScrollBar.SetSize(10, Size.Height - GridContainer.VScrollBar.Position.Y - 3);
        EmptyLabel?.SetPosition(GridContainer.Size.Width / 2 - EmptyLabel.Size.Width / 2 - 20, GridContainer.Size.Height / 2 - EmptyLabel.Size.Height / 2 - 50);
    }

    public void AddToLoadPool(FileEntryWidget few)
    {
        LoadPool.Add(few);
    }

    private void SpawnLoaderThread()
    {
        if (LoaderThread != null) return;
        LoaderThread = new Thread(LoadImages);
        LoaderThread.Start();
    }

    private void LoadImages()
    {
        Thread.Sleep(100);
        while (LoadPool.Count > 0)
        {
            FileEntryWidget few = LoadPool[0];
            if (this.Disposed || Window.IsClosed) break;
            if (few.Disposed)
            {
                LoadPool.RemoveAt(0);
                continue;
            }
            (byte[] Bytes, int Width, int Height) result = decodl.PNGDecoder.Decode(few.Filename);
            few.BitmapPendingCreation = result.Bytes;
            few.BitmapWidth = result.Width;
            few.BitmapHeight = result.Height;
            LoadPool.RemoveAt(0);
        }
        LoaderThread = null;
    }
}

public class FileEntryWidget : Widget
{
    public string DirectoryName { get { return Filename; } }
    public string Filename;
    public string Name { get { return Filename.Split('/').ToList().Last(); } }
    public bool IsFolder = false;
    public bool IsFile { get { return !IsFolder; } }
    public bool Selected = false;
    public bool Hovering = false;
    public bool TwoLines = false;

    public BaseEvent OnSelectionChanged;

    FileExplorer FileExplorer;
    
    public byte[] BitmapPendingCreation;
    public int BitmapWidth;
    public int BitmapHeight;

    public FileEntryWidget(FileExplorer FileExplorer, IContainer Parent) : base(Parent)
    {
        this.FileExplorer = FileExplorer;
        Sprites["box"] = new Sprite(this.Viewport, new SolidBitmap(1, 1, new Color(17, 34, 52)));
        Sprites["box"].X = 1;
        Sprites["box"].Y = 1;
        Sprites["box"].Visible = false;
        Sprites["gfx"] = new Sprite(this.Viewport);
        Sprites["text"] = new Sprite(this.Viewport);
        Sprites["text"].X = 2;
        Sprites["outline"] = new Sprite(this.Viewport);
        Sprites["outline"].Visible = false;
        Sprites["hover"] = new Sprite(this.Viewport, new SolidBitmap(1, 2, new Color(61, 201, 240)));
        Sprites["hover"].Visible = false;
    }

    public void SetSelected(bool Selected)
    {
        if (this.Selected != Selected)
        {
            this.Selected = Selected;
            if (this.Selected)
            {
                foreach (FileEntryWidget few in Parent.Widgets)
                {
                    if (few != this && few.Selected) few.SetSelected(false);
                }
            }
            Sprites["outline"].Visible = this.Selected;
            Sprites["box"].Visible = this.Selected;
            Sprites["hover"].Visible = Mouse.Inside && !this.Selected;
            this.OnSelectionChanged?.Invoke(new BaseEventArgs());
        }
    }

    public override void HoverChanged(MouseEventArgs e)
    {
        base.HoverChanged(e);
        Sprites["hover"].Visible = Mouse.Inside && !this.Selected;
    }

    public override void LeftMouseDownInside(MouseEventArgs e)
    {
        base.LeftMouseDownInside(e);
        this.SetSelected(true);
    }

    public void SetFile(string Filename)
    {
        this.Filename = Filename;
        if (this.Filename.EndsWith(".png") && File.Exists(this.Filename))
        {
            FileExplorer.AddToLoadPool(this);
        }
    }

    public void SetFolder(string Filename)
    {
        this.Filename = Filename;
        this.IsFolder = true;
        RedrawGraphic();
    }

    public void RedrawGraphic(bool ReopenFile = false)
    {
        if (Sprites["gfx"].Bitmap == null) ReopenFile = true;
        if (!this.IsFolder && ReopenFile)
        {
            if (BitmapPendingCreation != null)
            {
                Sprites["gfx"].Bitmap?.Dispose();
                Sprites["gfx"].Bitmap = new Bitmap(BitmapPendingCreation, BitmapWidth, BitmapHeight);
                Sprites["gfx"].DestroyBitmap = true;
            }
        }
        else if (this.IsFolder && ReopenFile)
        {
            Sprites["gfx"].Bitmap = Utilities.FolderIcon;
            Sprites["gfx"].DestroyBitmap = false;
        }
        if (Sprites["gfx"].Bitmap != null)
        {
            double percx = (double)Size.Width / Sprites["gfx"].Bitmap.Width;
            double percy = 64d / Sprites["gfx"].Bitmap.Height;
            double perc = percx > percy ? percy : percx;
            Sprites["gfx"].ZoomX = perc;
            Sprites["gfx"].ZoomY = perc;
            Sprites["gfx"].X = Size.Width / 2 - (int) Math.Round(Sprites["gfx"].Bitmap.Width * Sprites["gfx"].ZoomX / 2d);
            Sprites["gfx"].Y = 32 - (int) Math.Round(Sprites["gfx"].Bitmap.Height * Sprites["gfx"].ZoomY / 2d);
        }
    }

    public override void Update()
    {
        base.Update();
        if (BitmapPendingCreation != null)
        {
            RedrawGraphic();
            BitmapPendingCreation = null;
        }
    }

    public void RedrawName()
    {
        Sprites["text"].Bitmap?.Dispose();
        Font f = Fonts.CabinMedium.Use(11);
        List<string> Lines = Utilities.FormatString(f, this.Name, Size.Width - 4);
        if (Lines.Count > 1)
        {
            f = Fonts.CabinMedium.Use(9);
            Lines = Utilities.FormatString(f, this.Name, Size.Width - 4);
        }
        TwoLines = false;
        if (Lines.Count > 2)
        {
            TwoLines = true;
            Lines.RemoveRange(2, Lines.Count - 2);
            Lines[1] = Lines[1].Remove(Lines[1].Length - 4) + "...";
        }
        Sprites["text"].Bitmap = new Bitmap(Size.Width - 4, 32);
        Sprites["text"].Bitmap.Unlock();
        Sprites["text"].Bitmap.Font = f;
        int y = Lines.Count > 1 ? -2 : 4;
        for (int i = 0; i < Lines.Count; i++)
        {
            Sprites["text"].Bitmap.DrawText(Lines[i], Size.Width / 2 - 2, y, Color.WHITE, DrawOptions.CenterAlign);
            y += 12;
        }
        Sprites["text"].Bitmap.Lock();
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        if (Size.Width == 50) return;

        RedrawGraphic();
        RedrawName();
        Sprites["text"].Y = Size.Height - 32;
        (Sprites["hover"].Bitmap as SolidBitmap).SetSize(Size.Width, 2);
        Sprites["hover"].Y = Size.Height - (TwoLines ? 2 : 8);

        Sprites["outline"].Bitmap?.Dispose();
        Sprites["outline"].Bitmap = new Bitmap(this.Size);
        Sprites["outline"].Bitmap.Unlock();
        Sprites["outline"].Bitmap.DrawRect(0, 0, Size.Width, Size.Height, new Color(61, 201, 240));
        Sprites["outline"].Bitmap.SetPixel(0, 0, Color.ALPHA);
        Sprites["outline"].Bitmap.SetPixel(Size.Width - 1, 0, Color.ALPHA);
        Sprites["outline"].Bitmap.SetPixel(0, Size.Height - 1, Color.ALPHA);
        Sprites["outline"].Bitmap.SetPixel(Size.Width - 1, Size.Height - 1, Color.ALPHA);
        Sprites["outline"].Bitmap.SetPixel(1, 1, 44, 125, 156);
        Sprites["outline"].Bitmap.SetPixel(Size.Width - 2, 1, 44, 125, 156);
        Sprites["outline"].Bitmap.SetPixel(1, Size.Height - 2, 44, 125, 156);
        Sprites["outline"].Bitmap.SetPixel(Size.Width - 2, Size.Height - 2, 44, 125, 156);
        Sprites["outline"].Bitmap.Lock();

        (Sprites["box"].Bitmap as SolidBitmap).SetSize(Size.Width - 2, Size.Height - 2);
    }
}

public enum DirectoryViewMode
{
    Listed,
    SmallThumbnails,
    MediumThumbnails,
    LargeThumbnails
}
