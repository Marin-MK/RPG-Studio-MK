using System;
using System.Collections.Generic;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class VariablePickerBox : BrowserBox
{
    public int VariableID { get; protected set; }

    public BaseEvent OnVariableChanged;

    public VariablePickerBox(IContainer Parent) : base(Parent)
    {
        OnDropDownClicked += _ =>
        {
            VariablePicker picker = new VariablePicker(this.VariableID);
            picker.OnClosed += _ =>
            {
                if (!picker.Apply)
                {
                    // Ensure the name is updated
                    this.SetVariableID(this.VariableID);
                    return;
                }
                this.SetVariableID(picker.VariableID);
                this.OnVariableChanged?.Invoke(new BaseEventArgs());
            };
        };
    }

    public void SetVariableID(int VariableID)
    {
        this.VariableID = VariableID;
        this.SetText(GetVariableText(VariableID));
    }

    private string GetVariableText(int VariableID)
    {
        return $"[{Utilities.Digits(VariableID, 3)}]: {Data.System.Variables[VariableID - 1]}";
    }
}
