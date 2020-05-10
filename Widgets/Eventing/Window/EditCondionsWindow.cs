using System;
using System.Collections.Generic;
using System.Text;
using ODL;
using MKEditor.Game;

namespace MKEditor.Widgets
{
    public class EditCondionsWindow : PopupWindow
    {
        public Event EventData;
        public EventPage PageData;

        ConditionBox ConditionList;
        Button EditConditionButton;
        Button RemoveConditionButton;

        TabView TypeTabs;

        public Button ApplyButton { get { return Buttons[0]; } }

        public EditCondionsWindow(Event EventData, EventPage PageData)
        {
            this.EventData = EventData;
            this.PageData = PageData;
            SetTitle("Edit Conditions");
            MinimumSize = MaximumSize = new Size(610, 369);
            SetSize(MaximumSize);
            Center();

            ConditionList = new ConditionBox(this);
            ConditionList.SetPosition(6, 23);
            ConditionList.SetSize(279, 302);
            ConditionList.SetEventPage(EventData, PageData);
            ConditionList.SetSelectable(true);

            TypeTabs = new TabView(this);
            TypeTabs.SetPosition(285, 23);
            TypeTabs.SetSize(324, 302);
            TypeTabs.SetXOffset(8);
            TypeTabs.SetHeaderColor(59, 91, 124);
            ConditionParser.Headers.ForEach(header =>
            {
                TabContainer tc = TypeTabs.CreateTab(header.Name);
                for (int i = 0; i < header.Types.Count; i++)
                {
                    string typeid = header.Types[i];
                    ConditionType type = ConditionParser.Types.Find(t => t.Identifier == typeid);
                    Button b = new Button(tc);
                    b.SetPosition(i % 2 == 0 ? 12 : 165, 12 + 38 * (int) Math.Floor(i / 2d));
                    b.SetSize(153, 38);
                    b.SetText(type.Name);
                    b.OnClicked += delegate (BaseEventArgs e)
                    {
                        BasicCondition condition = new BasicCondition(type, type.Identifier, type.CreateBlankParameters());
                        ConditionUIParser parser = new ConditionUIParser(type.UI, condition);
                        parser.Window.OnClosed += delegate (BaseEventArgs e)
                        {
                            if (parser.NeedUpdate)
                            {
                                PageData.Conditions.Add(condition);
                                ConditionList.SetEventPage(EventData, PageData);
                            }
                        };
                    };
                }
            });

            EditConditionButton = new Button(this);
            EditConditionButton.SetText("Edit");
            EditConditionButton.SetPosition(14, 331);
            EditConditionButton.SetSize(122, 31);

            RemoveConditionButton = new Button(this);
            RemoveConditionButton.SetText("Remove");
            RemoveConditionButton.SetPosition(155, 331);
            RemoveConditionButton.SetSize(122, 31);
            RemoveConditionButton.OnClicked += delegate (BaseEventArgs e) { RemoveCondition(); };
            if (PageData.Conditions.Count == 0) RemoveConditionButton.SetClickable(false);

            CreateButton("Apply", Apply);
            CreateButton("Cancel", Cancel);
            CreateButton("OK", OK);
            
            ApplyButton.SetClickable(false);
        }

        public void RemoveCondition()
        {
            int idx = ConditionList.SelectedIndex;
            PageData.Conditions.RemoveAt(idx);
            ConditionList.SetEventPage(EventData, PageData);
            if (idx >= PageData.Conditions.Count) idx -= 1;
            if (PageData.Conditions.Count == 0) RemoveConditionButton.SetClickable(false);
            else ConditionList.SetSelectedIndex(idx);
        }

        public void Apply(BaseEventArgs e)
        {

        }

        public void Cancel(BaseEventArgs e)
        {

        }

        public void OK(BaseEventArgs e)
        {

        }
    }
}
