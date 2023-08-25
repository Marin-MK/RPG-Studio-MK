using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class SwitchPickerBox : BrowserBox
{
    public int SwitchID { get; protected set; } = 1;

    public BaseEvent OnSwitchChanged;

    public SwitchPickerBox(IContainer Parent) : base(Parent)
    {
        SetFont(Fonts.Paragraph);
        OnDropDownClicked += _ =>
        {
            SwitchPicker picker = new SwitchPicker(this.SwitchID);
            picker.OnClosed += _ =>
            {
                if (!picker.Apply)
                {
                    // Ensure the name is updated
                    this.SetSwitchID(this.SwitchID);
                    return;
                }
                this.SetSwitchID(picker.SwitchID);
                this.OnSwitchChanged?.Invoke(new BaseEventArgs());
            };
        };
        SetSwitchID(1);
    }

    public void SetSwitchID(int SwitchID)
    {
        this.SwitchID = SwitchID;
        this.SetText(GetSwitchText(SwitchID));
    }

    private string GetSwitchText(int SwitchID)
    {
        return $"[{Utilities.Digits(SwitchID, 3)}]: {Data.System.Switches[SwitchID - 1]}";
    }
}
