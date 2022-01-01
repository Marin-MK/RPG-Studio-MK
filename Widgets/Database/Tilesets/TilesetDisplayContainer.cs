using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Widgets;

public class TilesetDisplayContainer : Widget
{
    public Game.Tileset Tileset { get; protected set; }
    public TilesetDisplayMode Mode { get; protected set; }

    public TilesetDisplayContainer(IContainer Parent) : base(Parent)
    {
        Sprites["horizontal"] = new Sprite(this.Viewport, new SolidBitmap(263, 1, new Color(121, 121, 122)));
        Sprites["vertical"] = new Sprite(this.Viewport);
        Sprites["tileset"] = new Sprite(this.Viewport);
        Sprites["tileset"].Y = 33;
        Sprites["autotiles"] = new Sprite(this.Viewport);
        Sprites["impassable"] = new Sprite(this.Viewport);
        Sprites["impassable"].Z = 1;
        for (int i = 0; i <= 17; i++)
        {
            Sprites[$"tag{i}"] = new Sprite(this.Viewport);
            Sprites[$"tag{i}"].Z = 1;
        }
        Sprites["big_up"] = new Sprite(this.Viewport);
        Sprites["big_up"].Z = 1;
        Sprites["big_left"] = new Sprite(this.Viewport);
        Sprites["big_left"].Z = 1;
        Sprites["big_right"] = new Sprite(this.Viewport);
        Sprites["big_right"].Z = 1;
        Sprites["big_down"] = new Sprite(this.Viewport);
        Sprites["big_down"].Z = 1;
        Sprites["small_up"] = new Sprite(this.Viewport);
        Sprites["small_up"].Z = 1;
        Sprites["small_left"] = new Sprite(this.Viewport);
        Sprites["small_left"].Z = 1;
        Sprites["small_right"] = new Sprite(this.Viewport);
        Sprites["small_right"].Z = 1;
        Sprites["small_down"] = new Sprite(this.Viewport);
        Sprites["small_down"].Z = 1;
        for (int i = 0; i <= 5; i++)
        {
            Sprites[$"prio{i}"] = new Sprite(this.Viewport);
            Sprites[$"prio{i}"].Z = 1;
        }
        Sprites["bush"] = new Sprite(this.Viewport);
        Sprites["bush"].Z = 1;
        Sprites["counter"] = new Sprite(this.Viewport);
        Sprites["counter"].Z = 1;
    }

    public void SetTileset(Game.Tileset Tileset, bool ForceRedraw = false)
    {
        if (this.Tileset != Tileset || ForceRedraw)
        {
            this.Tileset = Tileset;
            Sprites["tileset"].Bitmap = null;
            Sprites["autotiles"].Bitmap?.Dispose();
            Sprites["vertical"].Bitmap?.Dispose();
            Sprites["horizontal"].Visible = false;
            if (this.Tileset != null && Tileset.TilesetListBitmap != null)
            {
                Sprites["tileset"].Bitmap = Tileset.TilesetListBitmap;
                Sprites["tileset"].DestroyBitmap = false;
                int tilecount = (int) Math.Ceiling(Tileset.TilesetBitmap.Height / 32d);
                for (int i = 1; i < tilecount; i++)
                {
                    Sprites["horizontal"].MultiplePositions.Add(new Point(0, 33 * i - 1));
                    Sprites["horizontal"].Visible = true;
                }
                Sprites["vertical"].Bitmap = new SolidBitmap(1, Tileset.TilesetListBitmap.Height, new Color(121, 121, 122));
                for (int i = 1; i < 8; i++)
                {
                    Sprites["vertical"].MultiplePositions.Add(new Point(i * 33 - 1, 0));
                }
                Sprites["autotiles"].Bitmap = new Bitmap(263, 32);
                Sprites["autotiles"].Bitmap.Unlock();
                for (int i = 0; i < Tileset.Autotiles.Count; i++)
                {
                    if (Tileset.Autotiles[i] == null || Tileset.Autotiles[i].AutotileBitmap == null) continue;
                    Sprites["autotiles"].Bitmap.Build(
                        (i + 1) * 33, 0,
                        Tileset.Autotiles[i].AutotileBitmap,
                        new Rect(0, 0, 32, 32)
                    );
                }
                Sprites["autotiles"].Bitmap.Lock();
                SetHeight(Tileset.TilesetListBitmap.Height);
            }
            else SetHeight(32);
        }
        SetMode(Mode);
    }

    public void SetMode(TilesetDisplayMode Mode)
    {
        this.Mode = Mode;
        Sprites["impassable"].Visible = false;
        for (int i = 0; i <= 17; i++)
        {
            Sprites[$"tag{i}"].Visible = false;
        }
        Sprites["big_up"].Visible = false;
        Sprites["big_left"].Visible = false;
        Sprites["big_right"].Visible = false;
        Sprites["big_down"].Visible = false;
        Sprites["small_up"].Visible = false;
        Sprites["small_left"].Visible = false;
        Sprites["small_right"].Visible = false;
        Sprites["small_down"].Visible = false;
        for (int i = 0; i <= 5; i++)
        {
            Sprites[$"prio{i}"].Visible = false;
        }
        Sprites["bush"].Visible = false;
        Sprites["counter"].Visible = false;
        switch (Mode)
        {
            case TilesetDisplayMode.Passage:
                LoadPassabilities();
                break;
            case TilesetDisplayMode.Directions:
                LoadDirections();
                break;
            case TilesetDisplayMode.Priority:
                LoadPriorities();
                break;
            case TilesetDisplayMode.BushFlag:
                LoadBushFlag();
                break;
            case TilesetDisplayMode.CounterFlag:
                LoadCounterFlag();
                break;
            case TilesetDisplayMode.TerrainTag:
                LoadTerrainTags();
                break;
        }
        this.Viewport.Update();
    }

    void LoadPassabilities()
    {
        if (Sprites["impassable"].Bitmap == null)
            Sprites["impassable"].Bitmap = new Bitmap("assets/img/database_tileset_impassable");
        Sprites["impassable"].MultiplePositions.Clear();
        if (Tileset != null)
        {
            for (int i = 384; i < Tileset.Passabilities.Count; i++)
            {
                Game.Passability tile = Tileset.Passabilities[i];
                if (Tileset.Passabilities[i] != Game.Passability.All)
                {
                    int x = (i - 384) % 8;
                    int y = (int) Math.Floor((i - 384) / 8d);
                    Sprites["impassable"].MultiplePositions.Add(new Point(x * 33, (y + 1) * 33));
                }
            }
            for (int i = 0; i < 8; i++)
            {
                if (Tileset.Passabilities[i * 48] != Game.Passability.All)
                {
                    Sprites["impassable"].MultiplePositions.Add(new Point(i * 33, 0));
                }
            }
        }
        Sprites["impassable"].Visible = Sprites["impassable"].MultiplePositions.Count > 0;
    }

    void LoadDirections()
    {
        if (Sprites["big_up"].Bitmap == null) Sprites["big_up"].Bitmap = new Bitmap("assets/img/database_tileset_big_up");
        if (Sprites["big_left"].Bitmap == null) Sprites["big_left"].Bitmap = new Bitmap("assets/img/database_tileset_big_left");
        if (Sprites["big_right"].Bitmap == null) Sprites["big_right"].Bitmap = new Bitmap("assets/img/database_tileset_big_right");
        if (Sprites["big_down"].Bitmap == null) Sprites["big_down"].Bitmap = new Bitmap("assets/img/database_tileset_big_down");
        if (Sprites["small_up"].Bitmap == null) Sprites["small_up"].Bitmap = new Bitmap("assets/img/database_tileset_small_up");
        if (Sprites["small_left"].Bitmap == null) Sprites["small_left"].Bitmap = new Bitmap("assets/img/database_tileset_small_left");
        if (Sprites["small_right"].Bitmap == null) Sprites["small_right"].Bitmap = new Bitmap("assets/img/database_tileset_small_right");
        if (Sprites["small_down"].Bitmap == null) Sprites["small_down"].Bitmap = new Bitmap("assets/img/database_tileset_small_down");
        Sprites["big_up"].MultiplePositions.Clear();
        Sprites["big_left"].MultiplePositions.Clear();
        Sprites["big_right"].MultiplePositions.Clear();
        Sprites["big_down"].MultiplePositions.Clear();
        Sprites["small_up"].MultiplePositions.Clear();
        Sprites["small_left"].MultiplePositions.Clear();
        Sprites["small_right"].MultiplePositions.Clear();
        Sprites["small_down"].MultiplePositions.Clear();
        if (Tileset != null)
        {
            for (int i = 0; i < 8; i++)
            {
                Game.Passability tile = Tileset.Passabilities[i * 48];
                Point p = new Point(i * 33, 0);
                if ((tile & Game.Passability.Up) != 0) Sprites["big_up"].MultiplePositions.Add(p);
                else Sprites["small_up"].MultiplePositions.Add(p);
                if ((tile & Game.Passability.Left) != 0) Sprites["big_left"].MultiplePositions.Add(p);
                else Sprites["small_left"].MultiplePositions.Add(p);
                if ((tile & Game.Passability.Right) != 0) Sprites["big_right"].MultiplePositions.Add(p);
                else Sprites["small_right"].MultiplePositions.Add(p);
                if ((tile & Game.Passability.Down) != 0) Sprites["big_down"].MultiplePositions.Add(p);
                else Sprites["small_down"].MultiplePositions.Add(p);
            }
            for (int i = 384; i < Tileset.Passabilities.Count; i++)
            {
                Game.Passability tile = Tileset.Passabilities[i];
                int x = 33 * ((i - 384) % 8);
                int y = 33 * ((int) Math.Floor((i - 384) / 8d) + 1);
                Point p = new Point(x, y);
                if ((tile & Game.Passability.Up) != 0) Sprites["big_up"].MultiplePositions.Add(p);
                else Sprites["small_up"].MultiplePositions.Add(p);
                if ((tile & Game.Passability.Left) != 0) Sprites["big_left"].MultiplePositions.Add(p);
                else Sprites["small_left"].MultiplePositions.Add(p);
                if ((tile & Game.Passability.Right) != 0) Sprites["big_right"].MultiplePositions.Add(p);
                else Sprites["small_right"].MultiplePositions.Add(p);
                if ((tile & Game.Passability.Down) != 0) Sprites["big_down"].MultiplePositions.Add(p);
                else Sprites["small_down"].MultiplePositions.Add(p);
            }
        }
        Sprites["big_up"].Visible = Sprites["big_up"].MultiplePositions.Count > 0;
        Sprites["big_left"].Visible = Sprites["big_left"].MultiplePositions.Count > 0;
        Sprites["big_right"].Visible = Sprites["big_right"].MultiplePositions.Count > 0;
        Sprites["big_down"].Visible = Sprites["big_down"].MultiplePositions.Count > 0;
        Sprites["small_up"].Visible = Sprites["small_up"].MultiplePositions.Count > 0;
        Sprites["small_left"].Visible = Sprites["small_left"].MultiplePositions.Count > 0;
        Sprites["small_right"].Visible = Sprites["small_right"].MultiplePositions.Count > 0;
        Sprites["small_down"].Visible = Sprites["small_down"].MultiplePositions.Count > 0;
    }

    void LoadPriorities()
    {
        for (int i = 0; i <= 5; i++)
        {
            if (Sprites[$"prio{i}"].Bitmap == null)
                Sprites[$"prio{i}"].Bitmap = new Bitmap($"assets/img/database_tileset_tag_{i}");
            Sprites[$"prio{i}"].MultiplePositions.Clear();
        }
        if (Tileset != null)
        {
            for (int i = 384; i < Tileset.Priorities.Count; i++)
            {
                int Priority = Tileset.Priorities[i];
                if (Priority < 0 || Priority > 5) continue;
                int x = (i - 384) % 8;
                int y = (int)Math.Floor((i - 384) / 8d);
                Sprites[$"prio{Priority}"].MultiplePositions.Add(new Point(x * 33, (y + 1) * 33));
            }
            for (int i = 0; i < 8; i++)
            {
                int Priority = Tileset.Priorities[i * 48];
                if (Priority < 0 || Priority > 5) continue;
                Sprites[$"prio{Priority}"].MultiplePositions.Add(new Point(i * 33, 0));
            }
        }
        for (int i = 0; i <= 5; i++)
        {
            Sprites[$"prio{i}"].Visible = Sprites[$"prio{i}"].MultiplePositions.Count > 0;
        }
    }

    void LoadBushFlag()
    {
        if (Sprites["bush"].Bitmap == null)
            Sprites["bush"].Bitmap = new Bitmap("assets/img/database_tileset_bush");
        Sprites["bush"].MultiplePositions.Clear();
        if (Tileset != null)
        {
            for (int i = 384; i < Tileset.BushFlags.Count; i++)
            {
                if (Tileset.BushFlags[i])
                {
                    int x = (i - 384) % 8;
                    int y = (int)Math.Floor((i - 384) / 8d);
                    Sprites["bush"].MultiplePositions.Add(new Point(x * 33, (y + 1) * 33));
                }
            }
            for (int i = 0; i < 8; i++)
            {
                if (Tileset.BushFlags[i * 48])
                {
                    Sprites["bush"].MultiplePositions.Add(new Point(i * 33, 0));
                }
            }
        }
        Sprites["bush"].Visible = Sprites["bush"].MultiplePositions.Count > 0;
    }

    void LoadCounterFlag()
    {
        if (Sprites["counter"].Bitmap == null)
            Sprites["counter"].Bitmap = new Bitmap("assets/img/database_tileset_counter");
        Sprites["counter"].MultiplePositions.Clear();
        if (Tileset != null)
        {
            for (int i = 384; i < Tileset.CounterFlags.Count; i++)
            {
                if (Tileset.CounterFlags[i])
                {
                    int x = (i - 384) % 8;
                    int y = (int)Math.Floor((i - 384) / 8d);
                    Sprites["counter"].MultiplePositions.Add(new Point(x * 33, (y + 1) * 33));
                }
            }
            for (int i = 0; i < 8; i++)
            {
                if (Tileset.CounterFlags[i * 48])
                {
                    Sprites["counter"].MultiplePositions.Add(new Point(i * 33, 0));
                }
            }
        }
        Sprites["counter"].Visible = Sprites["counter"].MultiplePositions.Count > 0;
    }

    void LoadTerrainTags()
    {
        for (int i = 0; i <= 17; i++)
        {
            if (Sprites[$"tag{i}"].Bitmap == null)
                Sprites[$"tag{i}"].Bitmap = new Bitmap($"assets/img/database_tileset_tag_{i}");
            Sprites[$"tag{i}"].MultiplePositions.Clear();
        }
        if (Tileset != null)
        {
            for (int i = 384; i < Tileset.Tags.Count; i++)
            {
                int Tag = Tileset.Tags[i];
                if (Tag < 0 || Tag > 17) continue;
                int x = (i - 384) % 8;
                int y = (int) Math.Floor((i - 384) / 8d);
                Sprites[$"tag{Tag}"].MultiplePositions.Add(new Point(x * 33, (y + 1) * 33));
            }
            for (int i = 0; i < 8; i++)
            {
                int Tag = Tileset.Tags[i * 48];
                if (Tag < 0 || Tag > 17) continue;
                Sprites[$"tag{Tag}"].MultiplePositions.Add(new Point(i * 33, 0));
            }
        }
        for (int i = 0; i <= 17; i++)
        {
            Sprites[$"tag{i}"].Visible = Sprites[$"tag{i}"].MultiplePositions.Count > 0;
        }
    }

    public override void MouseDown(MouseEventArgs e)
    {
        base.MouseDown(e);
        if (WidgetIM.Hovering)
        {
            int rx = e.X - Viewport.X;
            int ry = e.Y - Viewport.Y + Position.Y - ScrolledPosition.Y;
            int tilex = (int) Math.Floor(rx / 33d);
            int tiley = (int) Math.Floor(ry / 33d);
            int tx = rx - tilex * 33;
            int ty = ry - tiley * 33;
            bool Top = false;
            bool Left = false;
            bool Right = false;
            bool Bottom = false;
            if (tx > ty) // Top right diagonal
            {
                if (tx < 32 - ty) Top = true;
                else Right = true;
            }
            else // Bottom left diagonal
            {
                if (tx < 32 - ty) Left = true;
                else Bottom = true;
            }
            int idx = -1;
            if (tiley == 0) // Autotile
            {
                idx = tilex * 48;
            }
            else
            {
                idx = 384 + tilex + (tiley - 1) * 8;
            }
            switch (Mode)
            {
                case TilesetDisplayMode.Passage:
                    InputPassability(e, idx, tilex, tiley);
                    break;
                case TilesetDisplayMode.Directions:
                    InputDirections(e, idx, tilex, tiley, Top, Left, Right, Bottom);
                    break;
                case TilesetDisplayMode.Priority:
                    InputPriority(e, idx, tilex, tiley);
                    break;
                case TilesetDisplayMode.BushFlag:
                    InputBush(e, idx, tilex, tiley);
                    break;
                case TilesetDisplayMode.CounterFlag:
                    InputCounter(e, idx, tilex, tiley);
                    break;
                case TilesetDisplayMode.TerrainTag:
                    InputTerrainTag(e, idx, tilex, tiley);
                    break;
            }
        }
    }

    void InputPassability(MouseEventArgs e, int TileID, int TileX, int TileY)
    {
        if (e.LeftButton != e.OldLeftButton && e.LeftButton ||
            e.RightButton != e.OldRightButton && e.RightButton)
        {
            Game.Passability OldPassability = Tileset.Passabilities[TileID];
            Game.Passability passability = OldPassability == Game.Passability.All ? Game.Passability.None : Game.Passability.All;
            SetTilePassability(TileID, TileX, TileY, passability);
            TilePassabilityChangeUndoAction.Create(Tileset.ID, TileID, TileX, TileY, OldPassability, Tileset.Passabilities[TileID], false);
        }
    }

    void InputDirections(MouseEventArgs e, int TileID, int TileX, int TileY, bool Top, bool Left, bool Right, bool Bottom)
    {
        if (e.LeftButton != e.OldLeftButton && e.LeftButton ||
            e.RightButton != e.OldRightButton && e.RightButton)
        {
            Game.Passability OldPassability = Tileset.Passabilities[TileID];
            Game.Passability passability = OldPassability;
            if (Top)
            {
                if ((passability & Game.Passability.Up) != 0) passability ^= Game.Passability.Up;
                else passability |= Game.Passability.Up;
            }
            else if (Left)
            {
                if ((passability & Game.Passability.Left) != 0) passability ^= Game.Passability.Left;
                else passability |= Game.Passability.Left;
            }
            else if (Right)
            {
                if ((passability & Game.Passability.Right) != 0) passability ^= Game.Passability.Right;
                else passability |= Game.Passability.Right;
            }
            else
            {
                if ((passability & Game.Passability.Down) != 0) passability ^= Game.Passability.Down;
                else passability |= Game.Passability.Down;
            }
            SetTilePassability(TileID, TileX, TileY, passability);
            TilePassabilityChangeUndoAction.Create(Tileset.ID, TileID, TileX, TileY, OldPassability, Tileset.Passabilities[TileID], true);
        }
    }

    void InputPriority(MouseEventArgs e, int TileID, int TileX, int TileY)
    {
        int OldPriority = Tileset.Priorities[TileID];
        int priority = OldPriority;
        if (e.LeftButton != e.OldLeftButton && e.LeftButton) priority += 1;
        else if (e.RightButton != e.OldRightButton && e.RightButton) priority -= 1;
        else return;
        if (priority > 5) priority = 0;
        if (priority < 0) priority = 5;
        SetTilePriority(TileID, TileX, TileY, priority);
        TilePriorityChangeUndoAction.Create(Tileset.ID, TileID, TileX, TileY, OldPriority, Tileset.Priorities[TileID]);
    }

    void InputBush(MouseEventArgs e, int TileID, int TileX, int TileY)
    {
        if (e.LeftButton != e.OldLeftButton && e.LeftButton ||
            e.RightButton != e.OldRightButton && e.RightButton)
        {
            bool OldBush = Tileset.BushFlags[TileID];
            bool Bush = !OldBush;
            SetTileBush(TileID, TileX, TileY, Bush);
            TileBushChangeUndoAction.Create(Tileset.ID, TileID, TileX, TileY, OldBush, Tileset.BushFlags[TileID]);
        }
    }

    void InputCounter(MouseEventArgs e, int TileID, int TileX, int TileY)
    {
        if (e.LeftButton != e.OldLeftButton && e.LeftButton ||
            e.RightButton != e.OldRightButton && e.RightButton)
        {
            bool OldCounter = Tileset.CounterFlags[TileID];
            bool Counter = !OldCounter;
            SetTileCounter(TileID, TileX, TileY, Counter);
            TileCounterChangeUndoAction.Create(Tileset.ID, TileID, TileX, TileY, OldCounter, Tileset.CounterFlags[TileID]);
        }
    }

    void InputTerrainTag(MouseEventArgs e, int TileID, int TileX, int TileY)
    {
        int OldTag = Tileset.Tags[TileID];
        int tag = OldTag;
        if (e.LeftButton != e.OldLeftButton && e.LeftButton) tag += 1;
        else if (e.RightButton != e.OldRightButton && e.RightButton) tag -= 1;
        else return;
        if (tag > 17) tag = 0;
        if (tag < 0) tag = 17;
        SetTileTag(TileID, TileX, TileY, tag);
        TileTagChangeUndoAction.Create(Tileset.ID, TileID, TileX, TileY, OldTag, Tileset.Tags[TileID]);
    }

    public void SetTilePassability(int TileID, int TileX, int TileY, Game.Passability Passability)
    {
        Tileset.Passabilities[TileID] = Passability;
        if (Sprites["impassable"].Bitmap != null)
        {
            Sprites["impassable"].MultiplePositions.RemoveAll(p =>
            {
                return p.X == TileX * 33 && p.Y == TileY * 33;
            });
            if (Passability != Game.Passability.All)
            {
                Sprites["impassable"].MultiplePositions.Add(new Point(TileX * 33, TileY * 33));
            }
            Sprites["impassable"].Visible = Mode == TilesetDisplayMode.Passage && Sprites["impassable"].MultiplePositions.Count > 0;
            // Ensure the renderer marks the change and forces a redraw
            Sprites["impassable"].Update();
        }
        if (Sprites["big_up"].Bitmap != null)
        {
            Sprites["big_up"].MultiplePositions.RemoveAll(p => p.X == TileX * 33 && p.Y == TileY * 33);
            Sprites["big_left"].MultiplePositions.RemoveAll(p => p.X == TileX * 33 && p.Y == TileY * 33);
            Sprites["big_right"].MultiplePositions.RemoveAll(p => p.X == TileX * 33 && p.Y == TileY * 33);
            Sprites["big_down"].MultiplePositions.RemoveAll(p => p.X == TileX * 33 && p.Y == TileY * 33);
            Sprites["small_up"].MultiplePositions.RemoveAll(p => p.X == TileX * 33 && p.Y == TileY * 33);
            Sprites["small_left"].MultiplePositions.RemoveAll(p => p.X == TileX * 33 && p.Y == TileY * 33);
            Sprites["small_right"].MultiplePositions.RemoveAll(p => p.X == TileX * 33 && p.Y == TileY * 33);
            Sprites["small_down"].MultiplePositions.RemoveAll(p => p.X == TileX * 33 && p.Y == TileY * 33);
            Point p = new Point(TileX * 33, TileY * 33);
            if ((Passability & Game.Passability.Up) != 0) Sprites["big_up"].MultiplePositions.Add(p);
            else Sprites["small_up"].MultiplePositions.Add(p);
            if ((Passability & Game.Passability.Left) != 0) Sprites["big_left"].MultiplePositions.Add(p);
            else Sprites["small_left"].MultiplePositions.Add(p);
            if ((Passability & Game.Passability.Right) != 0) Sprites["big_right"].MultiplePositions.Add(p);
            else Sprites["small_right"].MultiplePositions.Add(p);
            if ((Passability & Game.Passability.Down) != 0) Sprites["big_down"].MultiplePositions.Add(p);
            else Sprites["small_down"].MultiplePositions.Add(p);
            Sprites["big_up"].Visible = Mode == TilesetDisplayMode.Directions && Sprites["big_up"].MultiplePositions.Count > 0;
            Sprites["big_left"].Visible = Mode == TilesetDisplayMode.Directions && Sprites["big_left"].MultiplePositions.Count > 0;
            Sprites["big_right"].Visible = Mode == TilesetDisplayMode.Directions && Sprites["big_right"].MultiplePositions.Count > 0;
            Sprites["big_down"].Visible = Mode == TilesetDisplayMode.Directions && Sprites["big_down"].MultiplePositions.Count > 0;
            Sprites["small_up"].Visible = Mode == TilesetDisplayMode.Directions && Sprites["small_up"].MultiplePositions.Count > 0;
            Sprites["small_left"].Visible = Mode == TilesetDisplayMode.Directions && Sprites["small_left"].MultiplePositions.Count > 0;
            Sprites["small_right"].Visible = Mode == TilesetDisplayMode.Directions && Sprites["small_right"].MultiplePositions.Count > 0;
            Sprites["small_down"].Visible = Mode == TilesetDisplayMode.Directions && Sprites["small_down"].MultiplePositions.Count > 0;
            // Ensure the renderer marks the change and forces a redraw
            this.Viewport.Update();
        }
    }

    public void SetTilePriority(int TileID, int TileX, int TileY, int Priority)
    {
        Tileset.Priorities[TileID] = Priority;
        if (Sprites[$"prio{Priority}"].Bitmap != null)
        {
            for (int i = 0; i <= 5; i++)
            {
                Sprites[$"prio{i}"].MultiplePositions.RemoveAll(p =>
                {
                    return p.X == TileX * 33 && p.Y == TileY * 33;
                });
                Sprites[$"prio{i}"].Visible = Sprites[$"prio{i}"].MultiplePositions.Count > 0;
            }
            Sprites[$"prio{Priority}"].MultiplePositions.Add(new Point(TileX * 33, TileY * 33));
            Sprites[$"prio{Priority}"].Visible = true;
            // Ensure the renderer marks the change and forces a redraw
            Sprites[$"prio{Priority}"].Update();
        }
    }

    public void SetTileBush(int TileID, int TileX, int TileY, bool Bush)
    {
        Tileset.BushFlags[TileID] = Bush;
        if (Sprites["bush"].Bitmap != null)
        {
            Sprites["bush"].MultiplePositions.RemoveAll(p =>
            {
                return p.X == TileX * 33 && p.Y == TileY * 33;
            });
            if (Bush)
            {
                Sprites["bush"].MultiplePositions.Add(new Point(TileX * 33, TileY * 33));
            }
            Sprites["bush"].Visible = Mode == TilesetDisplayMode.BushFlag && Sprites["bush"].MultiplePositions.Count > 0;
            // Ensure the renderer marks the change and forces a redraw
            Sprites["bush"].Update();
        }
    }

    public void SetTileCounter(int TileID, int TileX, int TileY, bool Counter)
    {
        Tileset.CounterFlags[TileID] = Counter;
        if (Sprites["counter"].Bitmap != null)
        {
            Sprites["counter"].MultiplePositions.RemoveAll(p =>
            {
                return p.X == TileX * 33 && p.Y == TileY * 33;
            });
            if (Counter)
            {
                Sprites["counter"].MultiplePositions.Add(new Point(TileX * 33, TileY * 33));
            }
            Sprites["counter"].Visible = Mode == TilesetDisplayMode.CounterFlag && Sprites["counter"].MultiplePositions.Count > 0;
            // Ensure the renderer marks the change and forces a redraw
            Sprites["counter"].Update();
        }
    }

    public void SetTileTag(int TileID, int TileX, int TileY, int Tag)
    {
        Tileset.Tags[TileID] = Tag;
        if (Sprites[$"tag{Tag}"].Bitmap != null)
        {
            for (int i = 0; i <= 17; i++)
            {
                Sprites[$"tag{i}"].MultiplePositions.RemoveAll(p =>
                {
                    return p.X == TileX * 33 && p.Y == TileY * 33;
                });
                Sprites[$"tag{i}"].Visible = Sprites[$"tag{i}"].MultiplePositions.Count > 0;
            }
            Sprites[$"tag{Tag}"].MultiplePositions.Add(new Point(TileX * 33, TileY * 33));
            Sprites[$"tag{Tag}"].Visible = true;
            // Ensure the renderer marks the change and forces a redraw
            Sprites[$"tag{Tag}"].Update();
        }
    }
}

public enum TilesetDisplayMode
{
    Passage,
    Directions,
    Priority,
    BushFlag,
    CounterFlag,
    TerrainTag
}