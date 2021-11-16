using System;

namespace RPGStudioMK.Game
{
    public class EventSettings : ICloneable
    {
        //public float MoveSpeed;
        //public float IdleSpeed;
        //public bool MoveAnimation;
        //public bool IdleAnimation;
        //public bool DirectionLock;
        //public bool Passable;
        //public bool SavePosition;
        public bool DirectionFix;
        public bool StepAnime;
        public bool AlwaysOnTop;
        public bool WalkAnime;
        public bool Through;

        public EventSettings()
        {

        }

        public object Clone()
        {
            EventSettings s = new EventSettings();
            s.DirectionFix = this.DirectionFix;
            s.StepAnime = this.StepAnime;
            s.AlwaysOnTop = this.AlwaysOnTop;
            s.WalkAnime = this.WalkAnime;
            s.Through = this.Through;
            return s;
        }
    }
}
