using System;

namespace RPGStudioMK.Game
{
    public class EventSettings
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
            //this.MoveSpeed = 0.25f;
            //this.IdleSpeed = 0.25f;
            //this.MoveAnimation = true;
            //this.IdleAnimation = false;
            //this.DirectionLock = false;
            //this.Passable = false;
            //this.SavePosition = false;
        }

        public EventSettings Clone()
        {
            throw new NotImplementedException();
            /*EventSettings es = new EventSettings();
            es.MoveSpeed = this.MoveSpeed;
            es.IdleSpeed = this.IdleSpeed;
            es.MoveAnimation = this.MoveAnimation;
            es.IdleAnimation = this.IdleAnimation;
            es.DirectionLock = this.DirectionLock;
            es.Passable = this.Passable;
            es.SavePosition = this.SavePosition;
            return es;*/
        }
    }
}
