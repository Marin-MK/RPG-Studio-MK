using RPGStudioMK.Game;
using System;
using System.Collections.Generic;
using System.Text;
using odl;
using amethyst;

namespace RPGStudioMK.Widgets
{
    public class ConditionHandlerWidget : Widget
    {
        public BasicCondition ActiveCondition;
        public ConditionType ConditionType;
        public ConditionUIParser UIParser;
        public RadioBox RadioBox;
        public bool Enabled { get { return RadioBox.Enabled; } }

        public ConditionHandlerWidget(ConditionType ConditionType, IContainer Parent) : base(Parent)
        {
            this.ConditionType = ConditionType;
            RadioBox = new RadioBox(this);
            RadioBox.SetText(ConditionType.Name);
            RadioBox.OnCheckChanged += delegate (BaseEventArgs e)
            {
                if (RadioBox.Checked)
                {
                    BasicCondition condition = GetConditionsWindow().SelectedCondition;
                    this.SetCondition(condition);
                    GetConditionsWindow().SetActiveType(this);
                }
            };
            UIParser = new ConditionUIParser(ConditionType.UI, this);
            UIParser.Load(null);
        }

        public void SetEnabled(bool Enabled)
        {
            RadioBox.SetEnabled(Enabled);
        }

        public void SetCondition(BasicCondition Condition)
        {
            this.ActiveCondition = Condition;
            if (this.ActiveCondition != null && this.ActiveCondition.Type != this.ConditionType)
            {
                this.ActiveCondition.Identifier = ":" + this.ConditionType.Identifier;
                this.ActiveCondition.Type = this.ConditionType;
                this.ActiveCondition.Parameters = this.ConditionType.CreateBlankParameters();
            }
            else if (this.ActiveCondition == null)
            {
                RadioBox.SetChecked(false);
            }
            UIParser.Load(Condition);
        }

        public void SetSelected(bool Selected)
        {
            RadioBox.SetChecked(Selected);
        }

        public EditConditionsWindow GetConditionsWindow()
        {
            return (EditConditionsWindow) Parent.Parent.Parent;
        }

        public void UpdateParentList()
        {
            GetConditionsWindow().RedrawConditions();
        }
    }
}
