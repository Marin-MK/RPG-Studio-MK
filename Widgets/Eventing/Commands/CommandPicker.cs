using System;
using System.Collections.Generic;
using odl;
using amethyst;

namespace MKEditor.Widgets
{
    public class CommandPicker : PopupWindow
    {
        TabView MainTabView;

        public DynamicCommandType ChosenCommand;

        public CommandPicker()
        {
            SetTitle("Pick Command");
            MainTabView = new TabView(this);
            MainTabView.SetPosition(1, 25);
            MainTabView.SetHeaderColor(new Color(59, 91, 124));
            MainTabView.SetXOffset(16);
            for (int i = 0; i < CommandPlugins.CommandTypes.Count; i++)
            {
                DynamicCommandType t = CommandPlugins.CommandTypes[i];
                if (string.IsNullOrEmpty(t.Name)) continue;
                string name = t.PickerTabName;
                int tabidx = MainTabView.Names.IndexOf(name);
                TabContainer tc = null;
                if (tabidx == -1)
                {
                    tc = MainTabView.CreateTab(name);
                    Container ButtonContainer = new Container(tc);
                }
                else tc = MainTabView.Tabs[tabidx];
                int idx = tc.Widgets[0].Widgets.Count;
                Button b = new Button(tc.Widgets[0]);
                b.SetText(t.Name);
                b.SetPosition(9 + 153 * (idx % 2), 7 + 38 * (int) Math.Floor(idx / 2d));
                b.SetSize(153, 38);
                b.OnClicked += delegate (BaseEventArgs e)
                {
                    CreateCommand(t);
                };
            }
            int count = 0;
            foreach (TabContainer tc in MainTabView.Tabs)
            {
                if (tc.Widgets[0].Widgets.Count > count) count = tc.Widgets[0].Widgets.Count;
            }
            MinimumSize = MaximumSize = new Size(count > 1 ? 326 : 173, 69 + 45 + 38 * count / 2);
            SetSize(MaximumSize);
            Center();

            CreateButton("Cancel", Cancel);
        }

        public void CreateCommand(DynamicCommandType Type)
        {
            this.ChosenCommand = Type;
            this.Close();
        }

        public void Cancel(BaseEventArgs e)
        {
            this.Close();
        }

        public override void SizeChanged(BaseEventArgs e)
        {
            base.SizeChanged(e);
            MainTabView.SetSize(Size.Width - MainTabView.Position.X - 1, Size.Height - MainTabView.Position.Y - 1 - 45);
        }
    }
}
