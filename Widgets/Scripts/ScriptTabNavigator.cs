using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class ScriptTabNavigator : Widget
{
    public List<Script> OpenScripts { get; protected set; } = new List<Script>();
    public List<(Script Script, int TabXStart, int TabWidth)> VisibleScripts { get; } = new List<(Script, int, int)>();
    public Script? OpenScript { get; protected set; }
    public Script? PreviewScript { get; protected set; }
    public Script? HoveringScript { get; protected set; }
    public Font Font { get; protected set; }
    public int Pivot { get; protected set; }

    public BaseEvent OnHoveredScriptChanged;
    public BaseEvent OnOpenScriptChanged;
    public BaseEvent OnOpenScriptChanging;
    public GenericObjectEvent<Script> OnScriptClosing;
    public BaseEvent OnScriptClosed;

    int BarWidth => Size.Width - Sprites["bar"].X * 2;

    IconButton LeftNavButton;
    IconButton RightNavButton;
    IconButton LeftDownNavButton;
    IconButton RightDownNavButton;

    IconButton CloseButton;

    public ScriptTabNavigator(IContainer parent) : base(parent)
    {
        this.Font = Fonts.Paragraph;
        Sprites["bg"] = new Sprite(this.Viewport);
        Sprites["bar"] = new Sprite(this.Viewport);
        Sprites["bar"].X = 43;
        Sprites["text"] = new Sprite(this.Viewport);
        Sprites["text"].X = Sprites["bar"].X;
        CloseButton = new IconButton(this);
        CloseButton.SetIcon(Icon.Cancel);
        CloseButton.SetVisible(true);
        CloseButton.UseBlueHoverBar = false;
        CloseButton.Selectable = false;
        CloseButton.OnClicked += _ =>
        {
            // Close currently open script
            CloseScript(HoveringScript);
        };
        //LeftNavButton = new IconButton(this);
        //LeftNavButton.SetIcon(Icon.LeftNav);
        //LeftNavButton.SetPosition(24, 11);
    }

    public void SetOpenScripts(List<Script> scripts)
    {
        if (this.OpenScripts != scripts)
        { 
            this.OpenScripts = scripts;
            this.OpenScript = scripts[0];
            this.PreviewScript = null;
            this.UpdateVisibleScripts();
        }
    }

    public void SetPreviewScript(Script? script)
    {
        if (this.PreviewScript != script)
        {
            this.PreviewScript = script;
            this.UpdateVisibleScripts();
        }
    }

    public void SetOpenScript(Script script, bool force = false)
    {
        if (this.OpenScript != script || force)
        {
            this.OnOpenScriptChanging?.Invoke(new BaseEventArgs());
            if (!this.OpenScripts.Contains(script) && this.PreviewScript != script) this.OpenScripts.Add(script);
            this.OpenScript = script;
            this.UpdateVisibleScripts();
            this.OnOpenScriptChanged?.Invoke(new BaseEventArgs());
        }
    }

    public void CloseScript(Script script)
    {
        this.OnScriptClosing?.Invoke(new GenericObjectEventArgs<Script>(script));
        int index = this.OpenScripts.IndexOf(script);
        this.OpenScripts.Remove(script);
        if (script == this.OpenScript)
        {
            OnOpenScriptChanging?.Invoke(new BaseEventArgs());
            if (index > 0) this.OpenScript = this.OpenScripts[index - 1];
            else if (this.OpenScripts.Count > 0) this.OpenScript = this.OpenScripts[0];
            else this.OpenScript = null;
            CloseButton.SetVisible(false);
            OnOpenScriptChanged?.Invoke(new BaseEventArgs());
        }
        this.UpdateVisibleScripts();
        OnScriptClosed?.Invoke(new BaseEventArgs());
    }

    public void SetFont(Font font)
    {
        if (this.Font != font)
        {
            this.Font = font;
            this.RedrawTabs();
        }
    }

    public void SetPivot(int pivot)
    {
        if (this.Pivot != pivot)
        {
            this.Pivot = pivot;
            this.UpdateVisibleScripts();
        }
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        if (Size.Width < 100) return;
        Sprites["bg"].Bitmap?.Dispose();
        Sprites["bg"].Bitmap = new Bitmap(Size);
        Color lineColor = new Color(86, 108, 134);
        Color fillerColor = new Color(28, 50, 73);
        Sprites["bg"].Bitmap.Unlock();
        Sprites["bg"].Bitmap.DrawRect(Size, lineColor);
        Sprites["bg"].Bitmap.FillRect(1, 1, Size.Width - 2, Size.Height - 2, fillerColor);
        Sprites["bg"].Bitmap.DrawLine(Sprites["bar"].X - 1, 1, Sprites["bar"].X - 1, Size.Height - 2, lineColor);
        Sprites["bg"].Bitmap.DrawLine(Size.Width - Sprites["bar"].X, 1, Size.Width - Sprites["bar"].X, Size.Height - 2, lineColor);
        Sprites["bg"].Bitmap.Lock();
        UpdateVisibleScripts();
    }

    public int GetScriptWidth(Script script)
    {
        return Font.TextSize(script.Name).Width + 60;
    }

    public void UpdateVisibleScripts(bool forceOpenFirst = false)
    {
        if (Size.Width < 100) return;
        int maxWidth = BarWidth;
        this.VisibleScripts.Clear();
        int x = 0;
        if (forceOpenFirst && this.OpenScript is not null)
        {
            int openWidth = GetScriptWidth(this.OpenScript);
            this.VisibleScripts.Add((this.OpenScript, x, openWidth));
            x += openWidth - 1; // One pixel overlap
            maxWidth -= openWidth;
        }
        int? previewWidth = null;
        if (this.PreviewScript is not null)
        {
            previewWidth = GetScriptWidth(this.PreviewScript);
            maxWidth -= (int) previewWidth;
        }
        for (int i = Pivot; i < this.OpenScripts.Count; i++)
        {
            Script script = this.OpenScripts[i];
            if (this.VisibleScripts.Exists(d => d.Script == script)) continue;
            int sWidth = GetScriptWidth(script);
            maxWidth -= sWidth;
            if (maxWidth >= 0)
            {
                this.VisibleScripts.Add((script, x, sWidth));
                x += sWidth - 1; // One pixel overlap
            }
        }
        if (previewWidth is not null) this.VisibleScripts.Add((this.PreviewScript, BarWidth - (int) previewWidth, (int) previewWidth));
        if (!forceOpenFirst && this.OpenScript is not null && !this.VisibleScripts.Exists(d => d.Script == this.OpenScript))
        {
            // If we have an open script but it's not being drawn right now, we need draw it as the first tab to guarantee it's visible.
            UpdateVisibleScripts(true);
            return;
        }
        RedrawTabs();
    }

    public void RedrawTabs()
    {
        Sprites["bar"].Bitmap?.Dispose();
        Sprites["text"].Bitmap?.Dispose();
        Sprites["bar"].Bitmap = new Bitmap(BarWidth, Size.Height);
        Sprites["text"].Bitmap = new Bitmap(BarWidth, Size.Height);
        Sprites["bar"].Bitmap.Unlock();
        Sprites["text"].Bitmap.Unlock();
        Sprites["text"].Bitmap.Font = this.Font;
        for (int i = 0; i < this.VisibleScripts.Count; i++)
        {
            RedrawScript(this.VisibleScripts[i], false);
        }
        Sprites["bar"].Bitmap.Lock();
        Sprites["text"].Bitmap.Lock();
    }

    public void RedrawScript(Script script, bool clear)
    {
        int index = this.VisibleScripts.FindIndex(d => d.Script == script);
        if (index == -1) return;
        this.RedrawScript(this.VisibleScripts[index], clear);
    }

    public void RedrawScript((Script script, int tabXStart, int tabWidth) data, bool clear)
    {
        Color lineColor = new Color(86, 108, 134);
        Color fillerColor = new Color(10, 23, 37);
        Script script = data.script;
        int x = data.tabXStart;
        int width = data.tabWidth;
        if (clear) Sprites["bar"].Bitmap.FillRect(x, 0, width, Sprites["bar"].Bitmap.Height, Color.ALPHA);
        Sprites["bar"].Bitmap.DrawRect(x, 0, width, Sprites["bar"].Bitmap.Height, lineColor);
        if (script == this.OpenScript)
        {
            Sprites["bar"].Bitmap.FillRect(x + 1, 1, width - 2, Sprites["bar"].Bitmap.Height - 1, fillerColor);
        }
        Color textColor = data.script == this.HoveringScript ? new Color(0, 194, 255) : Color.WHITE;
        DrawOptions drawOptions = DrawOptions.LeftAlign;
        if (script == this.PreviewScript) drawOptions |= DrawOptions.Italic;
        Sprites["text"].Bitmap.DrawText(script.Name, x + 10, 4, textColor, drawOptions);
    }

    public void SetHoveringScript(Script? script)
    {
        if (HoveringScript != script)
        {
            Script oldHoveringScript = HoveringScript;
            HoveringScript = script;
            Sprites["bar"].Bitmap.Unlock();
            Sprites["text"].Bitmap.Unlock();
            if (oldHoveringScript is not null) RedrawScript(oldHoveringScript, true);
            if (HoveringScript is not null) RedrawScript(HoveringScript, true);
            Sprites["bar"].Bitmap.Lock();
            Sprites["text"].Bitmap.Lock();
            OnHoveredScriptChanged?.Invoke(new BaseEventArgs());
        }
    }

    public override void MouseMoving(MouseEventArgs e)
    {
        base.MouseMoving(e);
        int rx = e.X - Viewport.X - Sprites["bar"].X;
        int ry = e.Y - Viewport.Y;
        bool found = false;
        if (!(!Mouse.Inside || rx < 0 || rx >= BarWidth || ry < 0 || ry >= Size.Height))
        {
            for (int i = 0; i < this.VisibleScripts.Count; i++)
            {
                if (rx >= this.VisibleScripts[i].TabXStart && rx < this.VisibleScripts[i].TabXStart + this.VisibleScripts[i].TabWidth)
                {
                    CloseButton.SetPosition(Sprites["bar"].X + this.VisibleScripts[i].TabXStart + this.VisibleScripts[i].TabWidth - 26, 1);
                    CloseButton.SetVisible(true);
                    SetHoveringScript(this.VisibleScripts[i].Script);
                    found = true;
                    break;
                }
            }
        }
        if (!found)
        {
            CloseButton.SetVisible(false);
            SetHoveringScript(null);
        }
    }

    public override void MouseDown(MouseEventArgs e)
    {
        base.MouseDown(e);
        if (HoveringScript is not null && !CloseButton.Mouse.Inside)
        {
            if (TimerExists("double") && !TimerPassed("double") && HoveringScript == PreviewScript)
            {
                SetPreviewScript(null);
                SetOpenScript(HoveringScript, true);
                DestroyTimer("double");
            }
            else
            {
                SetOpenScript(HoveringScript);
                if (TimerExists("double") && TimerPassed("double")) DestroyTimer("double");
                SetTimer("double", 300);
            }
        }
    }

    public override void Update()
    {
        base.Update();
        if (TimerExists("double") && TimerPassed("double")) DestroyTimer("double");
    }
}
