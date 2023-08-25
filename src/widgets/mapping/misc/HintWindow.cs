namespace RPGStudioMK.Widgets;

public class HintWindow : HelpText
{
    public HintWindow(IContainer Parent) : base(Parent) { }

    private int Opacity = 255;

    public override void SetText(string Text)
    {
        base.SetText(Text);
        if (TimerExists("frame_fade")) DestroyTimer("frame_fade");
        SetTimer("frame_fade", 10);
        foreach (Sprite s in Sprites.Values) s.Opacity = 255;
        this.Opacity = 255;
        SetVisible(true);
    }

    public override void Update()
    {
        base.Update();
        if (TimerPassed("frame_fade"))
        {
            foreach (Sprite s in Sprites.Values) s.Opacity--;
            this.Opacity--;
            if (this.Opacity == 0)
            {
                DestroyTimer("frame_fade");
                SetVisible(false);
            }
            else ResetTimer("frame_fade");
        }
    }
}
