﻿using System;

namespace RPGStudioMK.Game;

public class EventGraphic : ICloneable
{
    public int Direction;
    public int TileID;
    public int CharacterHue;
    public int BlendType;
    public int Pattern;
    public int Opacity;
    public string CharacterName;

    public int NumDirections;
    public int NumFrames;

    /// <summary>
    /// DO NOT USE!
    /// </summary>
    public EventGraphic()
    {

    }

    public EventGraphic(string CharacterName = "")
    {
        this.Direction = 2;
        this.NumDirections = 4;
        this.NumFrames = 4;
        this.CharacterName = CharacterName;
        this.TileID = 0;
        this.CharacterHue = 0;
        this.BlendType = 0;
        this.Pattern = 0;
        this.Opacity = 255;
    }

    public EventGraphic(IntPtr data)
    {
        this.NumDirections = 4;
        this.NumFrames = 4;
        this.Direction = (int)Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@direction"));
        this.TileID = (int)Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@tile_id"));
        this.CharacterHue = (int)Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@character_hue"));
        this.BlendType = (int)Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@blend_type"));
        this.Pattern = (int)Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@pattern"));
        this.Opacity = (int)Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@opacity"));
        this.CharacterName = Ruby.String.FromPtr(Ruby.GetIVar(data, "@character_name"));
        if (TileID >= 384) NumDirections = NumFrames = 1;
    }

    public IntPtr Save()
    {
        IntPtr graphic = Ruby.Funcall(Compatibility.RMXP.Graphic.Class, "new");
        Ruby.Pin(graphic);
        Ruby.SetIVar(graphic, "@direction", Ruby.Integer.ToPtr(this.Direction));
        Ruby.SetIVar(graphic, "@character_name", Ruby.String.ToPtr(this.CharacterName));
        Ruby.SetIVar(graphic, "@tile_id", Ruby.Integer.ToPtr(this.TileID));
        Ruby.SetIVar(graphic, "@character_hue", Ruby.Integer.ToPtr(this.CharacterHue));
        Ruby.SetIVar(graphic, "@blend_type", Ruby.Integer.ToPtr(this.BlendType));
        Ruby.SetIVar(graphic, "@pattern", Ruby.Integer.ToPtr(this.Pattern));
        Ruby.SetIVar(graphic, "@opacity", Ruby.Integer.ToPtr(this.Opacity));
        Ruby.Unpin(graphic);
        return graphic;
    }

    public object Clone()
    {
        EventGraphic g = new EventGraphic();
        g.Direction = this.Direction;
        g.TileID = this.TileID;
        g.CharacterHue = this.CharacterHue;
        g.BlendType = this.BlendType;
        g.Pattern = this.Pattern;
        g.Opacity = this.Opacity;
        g.CharacterName = this.CharacterName;
        g.NumFrames = this.NumFrames;
        g.NumDirections = this.NumDirections;
        return g;
    }
}
