using System;
using System.Collections.Generic;
using System.Text;
using odl;
using MKEditor.Game;

namespace MKEditor.Widgets
{
    public class EditConditionsWindow : PopupWindow
    {
        public List<BasicCondition> Conditions;
        public List<BasicCondition> OldConditions;

        ConditionBox ConditionList;
        Button NewConditionButton;
        Button RemoveConditionButton;

        TabView TypeTabs;

        public BasicCondition SelectedCondition { get { return Conditions[ConditionList.SelectedIndex]; } }

        public bool NeedUpdate = false;

        public Button ApplyButton { get { return Buttons[0]; } }

        public EditConditionsWindow(List<BasicCondition> Conditions)
        {
            this.Conditions = Conditions;
            this.OldConditions = new List<BasicCondition>();
            this.Conditions.ForEach(c => this.OldConditions.Add(c.Clone()));
            SetTitle("Edit Conditions");
            MinimumSize = MaximumSize = new Size(610, 469);
            SetSize(MaximumSize);
            Center();

            ConditionList = new ConditionBox(this);
            ConditionList.SetPosition(6, 23);
            ConditionList.SetSize(279, Size.Height - 67);
            ConditionList.SetConditions(this.Conditions);
            ConditionList.SetSelectable(true);
            ConditionList.OnSelectionChanged += delegate (BaseEventArgs e) { ConditionChanged(); };
            if (this.Conditions.Count > 0) ConditionList.SetSelectedIndex(0);

            TypeTabs = new TabView(this);
            TypeTabs.SetPosition(285, 23);
            TypeTabs.SetSize(324, Size.Height - 67);
            TypeTabs.SetXOffset(8);
            TypeTabs.SetHeaderColor(59, 91, 124);
            ConditionParser.Headers.ForEach(header =>
            {
                TabContainer tc = TypeTabs.CreateTab(header.Name);
                int h = 0;
                for (int i = 0; i < header.Types.Count; i++)
                {
                    string typeid = header.Types[i];
                    ConditionType type = ConditionParser.Types.Find(t => t.Identifier == typeid);
                    if (type.UI.Count == 0) continue;
                    ConditionHandlerWidget chw = new ConditionHandlerWidget(type, tc);
                    chw.SetPosition(11, 11 + h);
                    chw.SetSize(Convert.ToInt32(type.UI["width"]), Convert.ToInt32(type.UI["height"]));
                    h += Convert.ToInt32(type.UI["height"]);
                }
            });

            NewConditionButton = new Button(this);
            NewConditionButton.SetText("New");
            NewConditionButton.SetPosition(14, Size.Height - 38);
            NewConditionButton.SetSize(122, 31);
            NewConditionButton.OnClicked += delegate (BaseEventArgs e) { NewCondition(); };

            RemoveConditionButton = new Button(this);
            RemoveConditionButton.SetText("Remove");
            RemoveConditionButton.SetPosition(155, Size.Height - 38);
            RemoveConditionButton.SetSize(122, 31);
            RemoveConditionButton.OnClicked += delegate (BaseEventArgs e) { RemoveCondition(); };
            if (this.Conditions.Count == 0) RemoveConditionButton.SetEnabled(false);

            CreateButton("Apply", Apply);
            CreateButton("Cancel", Cancel);
            CreateButton("OK", OK);
            
            ApplyButton.SetEnabled(false);

            ConditionChanged();
            if (this.Conditions.Count == 0) SetAllEnabled(false);
        }

        public void RedrawConditions()
        {
            int oldidx = ConditionList.SelectedIndex;
            ConditionList.SetConditions(this.Conditions);
            if (oldidx >= this.Conditions.Count) oldidx = this.Conditions.Count - 1;
            if (oldidx != -1) ConditionList.SetSelectedIndex(oldidx);
        }

        public void SetActiveType(ConditionHandlerWidget chw = null)
        {
            for (int i = 0; i < TypeTabs.Tabs.Count; i++)
            {
                foreach (ConditionHandlerWidget w in TypeTabs.Tabs[i].Widgets)
                {
                    if (w != chw) w.SetCondition(null);
                }
            }
            SuspendIndexChange = true;
            RedrawConditions();
            SuspendIndexChange = false;
        }

        public void SetAllEnabled(bool Enabled)
        {
            for (int i = 0; i < TypeTabs.Tabs.Count; i++)
            {
                foreach (ConditionHandlerWidget w in TypeTabs.Tabs[i].Widgets)
                {
                    w.SetEnabled(Enabled);
                }
            }
        }

        bool SuspendIndexChange = false;

        public void ConditionChanged()
        {
            if (SuspendIndexChange || this.Conditions.Count == 0) return;
            BasicCondition condition = this.Conditions[ConditionList.SelectedIndex];
            for (int i = 0; i < TypeTabs.Tabs.Count; i++)
            {
                foreach (ConditionHandlerWidget chw in TypeTabs.Tabs[i].Widgets)
                {
                    if (chw.ConditionType == condition.Type)
                    {
                        chw.SetSelected(true);
                        chw.SetCondition(condition);
                    }
                }
            }
        }

        public void NewCondition()
        {
            BasicCondition c = new BasicCondition();
            c.Type = ((ConditionHandlerWidget) TypeTabs.Tabs[0].Widgets[0]).ConditionType;
            c.Identifier = ":" + c.Type.Identifier;
            c.Parameters = c.Type.CreateBlankParameters();
            this.Conditions.Add(c);
            RedrawConditions();
            SetAllEnabled(true);
            ConditionList.SetSelectedIndex(this.Conditions.Count - 1);
            RemoveConditionButton.SetEnabled(true);
            ApplyButton.SetEnabled(true);
        }

        public void RemoveCondition()
        {
            int idx = ConditionList.SelectedIndex;
            this.Conditions.RemoveAt(idx);
            ConditionList.SetConditions(this.Conditions);
            if (idx >= this.Conditions.Count) idx -= 1;
            if (this.Conditions.Count == 0) RemoveConditionButton.SetEnabled(false);
            else ConditionList.SetSelectedIndex(idx);
            ApplyButton.SetEnabled(true);
            if (this.Conditions.Count == 0)
            {
                SetActiveType(null);
                SetAllEnabled(false);
            }
        }

        public void Apply(BaseEventArgs e)
        {
            NeedUpdate = true;
            OldConditions.Clear();
            this.Conditions.ForEach(c => OldConditions.Add(c.Clone()));
            ApplyButton.SetEnabled(false);
        }

        public override void Close()
        {
            base.Close();
        }

        public void Cancel(BaseEventArgs e)
        {
            this.Conditions = OldConditions;
            Close();
        }

        public void OK(BaseEventArgs e)
        {
            Apply(e);
            Close();
        }
    }
}
