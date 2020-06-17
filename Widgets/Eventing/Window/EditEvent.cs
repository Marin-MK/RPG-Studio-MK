using System;
using System.Collections.Generic;
using odl;
using RPGStudioMK.Game;
using amethyst;

namespace RPGStudioMK.Widgets
{
    public class EditEvent : PopupWindow
    {
        public Map MapData;
        public Event OldEvent;
        public Event EventData;

        public Button ApplyButton { get { return Buttons[0]; } }

        TabView TabController;
        Button DeletePageButton;

        List<EventPageContainer> EventPageContainers = new List<EventPageContainer>();

        public EditEvent(Map map, Event ev, bool NewEvent = false)
        {
            this.MapData = map;
            this.OldEvent = ev;
            this.EventData = ev.Clone();
            SetTitle($"{(NewEvent ? "New" : "Edit")} event (ID: {Utilities.Digits(EventData.ID, 3)})");
            MinimumSize = MaximumSize = new Size(752, 690);
            SetSize(MaximumSize);
            Center();

            EventGroupBox MainPropertyBox = new EventGroupBox(this);
            MainPropertyBox.SetPosition(8, 25);
            MainPropertyBox.SetSize(232, 72);

            Font f = new Font("Fonts/ProductSans-M", 12);
            Label NameLabel = new Label(MainPropertyBox);
            NameLabel.SetFont(f);
            NameLabel.SetText("Name:");
            NameLabel.SetPosition(8, 12);
            TextBox NameBox = new TextBox(MainPropertyBox);
            NameBox.SetPosition(46, 7);
            NameBox.TextArea.SetTextY(2);
            NameBox.TextArea.SetCaretY(4);
            NameBox.SetSize(180, 27);
            NameBox.SetInitialText(EventData.Name);
            NameBox.OnTextChanged += delegate (BaseEventArgs e)
            {
                EventData.Name = NameBox.Text;
                MarkChanges();
            };

            Label WidthLabel = new Label(MainPropertyBox);
            WidthLabel.SetFont(f);
            WidthLabel.SetText("Width:");
            WidthLabel.SetPosition(6, 44);
            NumericBox WidthBox = new NumericBox(MainPropertyBox);
            WidthBox.SetPosition(46, 38);
            WidthBox.SetSize(63, 27);
            WidthBox.MinValue = 1;
            WidthBox.MaxValue = 999;
            WidthBox.SetValue(EventData.Width);
            WidthBox.OnValueChanged += delegate (BaseEventArgs e)
            {
                EventData.Width = WidthBox.Value;
                TabController.Tabs.ForEach(tc => ((EventPageContainer) tc.Widgets[0]).GraphicWidget.ConfigureGrid());
                MarkChanges();
            };

            Label HeightLabel = new Label(MainPropertyBox);
            HeightLabel.SetFont(f);
            HeightLabel.SetText("Height:");
            HeightLabel.SetPosition(119, 44);
            NumericBox HeightBox = new NumericBox(MainPropertyBox);
            HeightBox.SetPosition(163, 38);
            HeightBox.SetSize(63, 27);
            HeightBox.MinValue = 1;
            HeightBox.MaxValue = 999;
            HeightBox.SetValue(EventData.Height);
            HeightBox.OnValueChanged += delegate (BaseEventArgs e)
            {
                EventData.Height = HeightBox.Value;
                MarkChanges();
            };

            Button NewPageButton = new Button(this);
            NewPageButton.SetPosition(414, 43);
            NewPageButton.SetSize(67, 59);
            NewPageButton.SetText("New\nPage");
            NewPageButton.OnClicked += delegate (BaseEventArgs e) { NewPage(); };

            Button CopyPageButton = new Button(this);
            CopyPageButton.SetPosition(481, 43);
            CopyPageButton.SetSize(67, 59);
            CopyPageButton.SetText("Copy\nPage");
            CopyPageButton.OnClicked += delegate (BaseEventArgs e) { CopyPage(); };

            Button PastePageButton = new Button(this);
            PastePageButton.SetPosition(548, 43);
            PastePageButton.SetSize(67, 59);
            PastePageButton.SetText("Paste\nPage");
            PastePageButton.OnClicked += delegate (BaseEventArgs e) { PastePage(); };

            Button ClearPageButton = new Button(this);
            ClearPageButton.SetPosition(615, 43);
            ClearPageButton.SetSize(67, 59);
            ClearPageButton.SetText("Clear\nPage");
            ClearPageButton.OnClicked += delegate (BaseEventArgs e) { ClearPage(); };

            DeletePageButton = new Button(this);
            DeletePageButton.SetPosition(682, 43);
            DeletePageButton.SetSize(67, 59);
            DeletePageButton.SetText("Delete\nPage");
            if (EventData.Pages.Count == 1) DeletePageButton.SetEnabled(false);
            DeletePageButton.OnClicked += delegate (BaseEventArgs e) { DeletePage(); };

            TabController = new TabView(this);
            TabController.SetXOffset(8);
            TabController.SetPosition(1, 106);
            TabController.SetSize(750, 544);
            TabController.SetHeaderColor(59, 91, 124);
            for (int i = 0; i < EventData.Pages.Count; i++)
            {
                TabContainer tc = TabController.CreateTab(string.IsNullOrWhiteSpace(EventData.Pages[i].Name) ? "Untitled" : EventData.Pages[i].Name);
                EventPageContainer epc = new EventPageContainer(this, EventData, EventData.Pages[i], tc);
                epc.SetSize(750, 515);
                EventPageContainers.Add(epc);
            }

            CreateButton("Apply", Apply);
            CreateButton("Cancel", Cancel);
            CreateButton("OK", OK);
            ApplyButton.SetEnabled(false);
        }

        public void MarkChanges()
        {
            if (Buttons.Count > 0) ApplyButton.SetEnabled(true);
        }

        public void UpdateNames()
        {
            TabController.SetHeader(78, TabController.HeaderHeight, TabController.TextY);
            for (int i = 0; i < EventData.Pages.Count; i++)
            {
                TabController.SetName(i, EventData.Pages[i].Name);
            }
        }

        public void NewPage()
        {
            TabContainer tc = TabController.CreateTab("Untitled");
            EventPage PageData = new EventPage();
            EventData.Pages.Add(PageData);
            EventPageContainer epc = new EventPageContainer(this, EventData, PageData, tc);
            epc.SetSize(750, 515);
            EventPageContainers.Add(epc);
            TabController.SelectTab(TabController.Tabs.Count - 1);
            TabController.Redraw();
            DeletePageButton.SetEnabled(true);
        }

        public void CopyPage()
        {
            Editor.WIP();
        }

        public void PastePage()
        {
            Editor.WIP();
        }

        public void ClearPage()
        {
            EventPageContainer ct = EventPageContainers.Find(epc => epc.PageData == EventData.Pages[TabController.SelectedIndex]);
            ct.Dispose();
            EventPageContainers.Remove(ct);
            EventData.Pages[TabController.SelectedIndex] = new EventPage();
            EventPageContainer newct = new EventPageContainer(this, EventData, EventData.Pages[TabController.SelectedIndex], TabController.Tabs[TabController.SelectedIndex]);
            newct.SetSize(750, 515);
            EventPageContainers.Insert(TabController.SelectedIndex, newct);
            UpdateNames();
        }

        public void DeletePage()
        {
            if (EventData.Pages.Count == 1) return;
            EventPageContainer ct = EventPageContainers.Find(epc => epc.PageData == EventData.Pages[TabController.SelectedIndex]);
            ct.Dispose();
            EventPageContainers.Remove(ct);
            EventData.Pages.RemoveAt(TabController.SelectedIndex);
            TabController.DestroyTab(TabController.SelectedIndex);
            if (TabController.SelectedIndex >= EventPageContainers.Count) TabController.SelectTab(TabController.SelectedIndex - 1);
            else TabController.SelectTab(TabController.SelectedIndex);
            UpdateNames();
            if (EventData.Pages.Count == 1) DeletePageButton.SetEnabled(false);
        }

        public void Apply(BaseEventArgs e)
        {
            MapData.Events[EventData.ID] = EventData;
            OldEvent = EventData;
            EventData = EventData.Clone();
            ApplyButton.SetEnabled(false);
        }

        public void Cancel(BaseEventArgs e)
        {
            EventData = OldEvent;
            Close();
        }

        public void OK(BaseEventArgs e)
        {
            Apply(e);
            Close();
        }
    }

    public class EventGroupBox : Container
    {
        public Color Outline = new Color(86, 108, 134);
        public Color Inline = new Color(17, 27, 38);
        public Color Filler = new Color(24, 38, 53);

        public EventGroupBox(IContainer Parent) : base(Parent)
        {
            Sprites["box"] = new Sprite(this.Viewport);
        }

        public override void SizeChanged(BaseEventArgs e)
        {
            base.SizeChanged(e);
            Sprites["box"].Bitmap?.Dispose();
            Sprites["box"].Bitmap = new Bitmap(this.Size);
            Sprites["box"].Bitmap.Unlock();
            Sprites["box"].Bitmap.DrawRect(0, 0, Size.Width, Size.Height, Outline);
            Sprites["box"].Bitmap.DrawRect(1, 1, Size.Width - 2, Size.Height - 2, Inline);
            Sprites["box"].Bitmap.FillRect(2, 2, Size.Width - 4, Size.Height - 4, Filler);
            Sprites["box"].Bitmap.Lock();
        }
    }
}
