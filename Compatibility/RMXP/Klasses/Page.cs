using System;
using rubydotnet;

namespace RPGStudioMK.Compatibility
{
    public static partial class RMXP
    {
        public class Page : Ruby.Object
        {
            public new static string KlassName = "RPG::Event::Page";
            public new static Ruby.Class Class { get => (Ruby.Class) GetKlass(KlassName); }

            public Page(IntPtr Pointer) : base(Pointer, true) { }

            public static void Create()
            {
                Ruby.Class c = Ruby.Class.DefineClass<Page>("Page", null, "RPG::Event");
            }

            public Condition Condition
            {
                get => GetIVar("@condition").Convert<Condition>();
            }
            public Graphic Graphic
            {
                get => GetIVar("@graphic").Convert<Graphic>();
            }
            public Ruby.Integer MoveType
            {
                get => GetIVar("@move_type").Convert<Ruby.Integer>();
            }
            public Ruby.Integer MoveSpeed
            {
                get => GetIVar("@move_speed").Convert<Ruby.Integer>();
            }
            public Ruby.Integer MoveFrequency
            {
                get => GetIVar("@move_frequency").Convert<Ruby.Integer>();
            }
            public MoveRoute MoveRoute
            {
                get => GetIVar("@move_route").Convert<MoveRoute>();
            }
            public bool WalkAnime
            {
                get => GetIVar("@walk_anime") == Ruby.True;
            }
            public bool StepAnime
            {
                get => GetIVar("@step_anime") == Ruby.True;
            }
            public bool DirectionFix
            {
                get => GetIVar("@direction_fix") == Ruby.True;
            }
            public bool Through
            {
                get => GetIVar("@through") == Ruby.True;
            }
            public bool AlwaysOnTop
            {
                get => GetIVar("@always_on_top") == Ruby.True;
            }
            public Ruby.Integer Trigger
            {
                get => GetIVar("@trigger").Convert<Ruby.Integer>();
            }
            public Ruby.Array List
            {
                get => GetIVar("@list").Convert<Ruby.Array>();
            }
        }
    }
}
