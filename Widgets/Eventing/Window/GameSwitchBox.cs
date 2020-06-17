using System;
using System.Collections.Generic;
using System.Text;
using odl;
using amethyst;

namespace RPGStudioMK.Widgets
{
    public class GameSwitchBox : BrowserBox
    {
        public int GroupID;
        public int SwitchID;

        public BaseEvent OnSwitchChanged;

        public GameSwitchBox(IContainer Parent) : base(Parent)
        {
            this.GroupID = 1;
            this.SwitchID = 1;
            this.OnDropDownClicked += delegate (BaseEventArgs e)
            {
                GameSwitchPicker picker = new GameSwitchPicker(this.GroupID, this.SwitchID);
                picker.OnClosed += delegate (BaseEventArgs e)
                {
                    this.GroupID = picker.GroupID;
                    this.SwitchID = picker.SwitchID;
                    this.OnSwitchChanged?.Invoke(new BaseEventArgs());
                    this.ResetText();
                };
            };
            this.SetReadOnly(true);
            this.ResetText();
        }

        public override object GetValue(string Identifier)
        {
            if (Identifier == "group_id") return this.GroupID;
            else if (Identifier == "switch_id") return this.SwitchID;
            return base.GetValue(Identifier);
        }

        public override void SetValue(string Identifier, object Value)
        {
            if (Identifier == "group_id")
            {
                this.GroupID = Convert.ToInt32(Value);
                ResetText();
            }
            else if (Identifier == "switch_id")
            {
                this.SwitchID = Convert.ToInt32(Value);
                ResetText();
            }
            else base.SetValue(Identifier, Value);
        }

        public void ResetText()
        {
            if (this.GroupID == -1 || this.SwitchID == -1)
            {
                SetInitialText("");
            }
            else
            {
                if (this.GroupID > Editor.ProjectSettings.Switches.Count) SetInitialText("");
                else if (this.SwitchID > Editor.ProjectSettings.Switches[this.GroupID - 1].Switches.Count) SetInitialText("");
                else
                {
                    GameSwitch sw = Editor.ProjectSettings.Switches[this.GroupID - 1].Switches[this.SwitchID - 1];
                    SetInitialText($"{Utilities.Digits(this.SwitchID, 3)}: {sw.Name}");
                }
            }
        }
    }
}
