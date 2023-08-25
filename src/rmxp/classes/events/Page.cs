using System;

namespace RPGStudioMK.Compatibility;

public static partial class RMXP
{
    public static class Page
    {
        public static IntPtr Class;

        public static void Create()
        {
            Class = Ruby.Class.Define("Page", RMXP.Event.Class);
        }

        public static IntPtr Condition(IntPtr Self)
        {
            return Ruby.GetIVar(Self, "@condition");
        }
        public static IntPtr Graphic(IntPtr Self)
        {
            return Ruby.GetIVar(Self, "@graphic");
        }
        public static int MoveType(IntPtr Self)
        {
            return (int)Ruby.Integer.FromPtr(Ruby.GetIVar(Self, "@move_type"));
        }
        public static int MoveSpeed(IntPtr Self)
        {
            return (int)Ruby.Integer.FromPtr(Ruby.GetIVar(Self, "@move_speed"));
        }
        public static int MoveFrequency(IntPtr Self)
        {
            return (int)Ruby.Integer.FromPtr(Ruby.GetIVar(Self, "@move_frequency"));
        }
        public static IntPtr MoveRoute(IntPtr Self)
        {
            return Ruby.GetIVar(Self, "@move_route");
        }
        public static bool WalkAnime(IntPtr Self)
        {
            return Ruby.GetIVar(Self, "@walk_anime") == Ruby.True;
        }
        public static bool StepAnime(IntPtr Self)
        {
            return Ruby.GetIVar(Self, "@step_anime") == Ruby.True;
        }
        public static bool DirectionFix(IntPtr Self)
        {
            return Ruby.GetIVar(Self, "@direction_fix") == Ruby.True;
        }
        public static bool Through(IntPtr Self)
        {
            return Ruby.GetIVar(Self, "@through") == Ruby.True;
        }
        public static bool AlwaysOnTop(IntPtr Self)
        {
            return Ruby.GetIVar(Self, "@always_on_top") == Ruby.True;
        }
        public static int Trigger(IntPtr Self)
        {
            return (int)Ruby.Integer.FromPtr(Ruby.GetIVar(Self, "@trigger"));
        }
        public static IntPtr List(IntPtr Self)
        {
            return Ruby.GetIVar(Self, "@list");
        }
    }
}
