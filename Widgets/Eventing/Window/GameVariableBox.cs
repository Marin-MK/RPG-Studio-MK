using System;
using System.Collections.Generic;
using System.Text;
using odl;
using amethyst;

namespace RPGStudioMK.Widgets
{
    public class GameVariableBox : BrowserBox
    {
        public int GroupID;
        public int VariableID;

        public BaseEvent OnVariableChanged;

        public GameVariableBox(IContainer Parent) : base(Parent)
        {
            this.GroupID = 1;
            this.VariableID = 1;
            this.OnDropDownClicked += delegate (BaseEventArgs e)
            {
                GameVariablePicker picker = new GameVariablePicker(this.GroupID, this.VariableID);
                picker.OnClosed += delegate (BaseEventArgs e)
                {
                    this.GroupID = picker.GroupID;
                    this.VariableID = picker.VariableID;
                    this.OnVariableChanged?.Invoke(new BaseEventArgs());
                    this.ResetText();
                };
            };
            this.SetReadOnly(true);
            this.ResetText();
        }

        public override object GetValue(string Identifier)
        {
            if (Identifier == "group_id") return this.GroupID;
            else if (Identifier == "variable_id") return this.VariableID;
            else if (Identifier == null)
            {
                return new Dictionary<string, object>() { { ":group_id", this.GroupID }, { ":variable_id", this.VariableID } };
            }
            return base.GetValue(Identifier);
        }

        public override void SetValue(string Identifier, object Value)
        {
            if (Identifier == "group_id")
            {
                if (Value is string && ((string) Value == "nil" || string.IsNullOrEmpty((string) Value))) this.GroupID = -1;
                else this.GroupID = Convert.ToInt32(Value);
                ResetText();
            }
            else if (Identifier == "variable_id")
            {
                if (Value is string && ((string) Value == "nil" || string.IsNullOrEmpty((string) Value))) this.VariableID = -1;
                else this.VariableID = Convert.ToInt32(Value);
                ResetText();
            }
            else base.SetValue(Identifier, Value);
        }

        public void ResetText()
        {
            if (this.GroupID == -1 || this.VariableID == -1)
            {
                SetInitialText("");
            }
            else
            {
                if (this.GroupID > Editor.ProjectSettings.Variables.Count) SetInitialText("");
                else if (this.VariableID > Editor.ProjectSettings.Variables[this.GroupID - 1].Variables.Count) SetInitialText("");
                else
                {
                    GameVariable var = Editor.ProjectSettings.Variables[this.GroupID - 1].Variables[this.VariableID - 1];
                    SetInitialText($"{Utilities.Digits(this.VariableID, 3)}: {var.Name}");
                }
            }
        }
    }
}
