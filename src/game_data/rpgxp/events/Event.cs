using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace RPGStudioMK.Game;

public class Event : ICloneable
{
    public int ID;
    public string Name;
    public int X;
    public int Y;
    public int Width;
    public int Height;
    public List<EventPage> Pages = new List<EventPage>();

    public Event()
    {

    }

    public Event(int ID)
    {
        this.ID = ID;
        this.Name = $"Event {Utilities.Digits(ID, 3)}";
        this.Width = 1;
        this.Height = 1;
        this.Pages = new List<EventPage>();
        this.Pages.Add(new EventPage());
    }

    public Event(IntPtr data)
    {
        this.ID = (int)Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@id"));
        this.Name = Ruby.String.FromPtr(Ruby.GetIVar(data, "@name"));
        Match match = Regex.Match(this.Name, @"[sS]ize\((\d+),(\d+)\)");
        if (Data.IsVersionAtLeast(EssentialsVersion.v19) && match.Success)
        {
            this.Width = Convert.ToInt32(match.Groups[1].Value);
            this.Height = Convert.ToInt32(match.Groups[2].Value);
            this.Name = this.Name.Remove(match.Index, match.Length).TrimEnd();
        }
        else
        {
            this.Width = 1;
            this.Height = 1;
        }
        this.X = (int)Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@x"));
        this.Y = (int)Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@y"));
        IntPtr pages = Ruby.GetIVar(data, "@pages");
        for (int i = 0; i < Ruby.Array.Length(pages); i++)
        {
            IntPtr pagedata = Ruby.Array.Get(pages, i);
            EventPage page = new EventPage(pagedata);
            page.Name = (i + 1).ToString();
            this.Pages.Add(page);
        }
    }

    public IntPtr Save()
    {
        IntPtr e = Ruby.Funcall(Compatibility.RMXP.Event.Class, "new");
        Ruby.Pin(e);
        string savename = this.Name;
        if (this.Width > 1 || this.Height > 1) savename += $"{(savename.Length > 0 ? " " : "")}size({this.Width},{this.Height})";
        Ruby.SetIVar(e, "@name", Ruby.String.ToPtr(savename));
        Ruby.SetIVar(e, "@x", Ruby.Integer.ToPtr(this.X));
        Ruby.SetIVar(e, "@y", Ruby.Integer.ToPtr(this.Y));
        Ruby.SetIVar(e, "@id", Ruby.Integer.ToPtr(this.ID));
        IntPtr pages = Ruby.Array.Create();
        Ruby.SetIVar(e, "@pages", pages);
        for (int i = 0; i < this.Pages.Count; i++)
        {
            IntPtr page = this.Pages[i].Save();
            Ruby.Array.Set(pages, i, page);
        }
        Ruby.Unpin(e);
        return e;
    }

    public object Clone()
    {
        Event e = new Event(this.ID);
        e.Name = this.Name;
        e.X = this.X;
        e.Y = this.Y;
        e.Width = this.Width;
        e.Height = this.Height;
        e.Pages = new List<EventPage>();
        this.Pages.ForEach(p => e.Pages.Add((EventPage)p.Clone()));
        return e;
    }
}

public enum TriggerMode
{
    Action = 0,
    PlayerTouch = 1,
    EventTouch = 2,
    Autorun = 3,
    ParallelProcess = 4
}
