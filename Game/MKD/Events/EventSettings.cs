using System;

namespace RPGStudioMK.Game;

[Serializable]
public class EventSettings : ICloneable
{
    //public float MoveSpeed;
    //public float IdleSpeed;
    //public bool MoveAnimation;
    //public bool IdleAnimation;
    //public bool DirectionLock;
    //public bool Passable;
    //public bool SavePosition;
    public bool WalkAnime;
    public bool StepAnime;
    public bool DirectionFix;
    public bool Through;
    public bool AlwaysOnTop;

    public EventSettings()
    {
        this.WalkAnime = true;
        this.StepAnime = false;
        this.DirectionFix = false;
        this.Through = false;
        this.AlwaysOnTop = false;
    }

    public object Clone()
    {
        EventSettings s = new EventSettings();
        s.WalkAnime = this.WalkAnime;
        s.StepAnime = this.StepAnime;
        s.DirectionFix = this.DirectionFix;
        s.Through = this.Through;
        s.AlwaysOnTop = this.AlwaysOnTop;
        return s;
    }
}
