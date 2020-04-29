using System;
using System.Collections.Generic;
using ODL;
using MKEditor.Game;

namespace MKEditor.Widgets
{
    public class EditEvent : PopupWindow
    {
        public Map MapData;
        public Event OldEvent;
        public Event EventData;

        public Button ApplyButton { get { return Buttons[0]; } }

        TabView TabController;
        public List<EventPageContainer> EventPageContainers = new List<EventPageContainer>();

        public EditEvent(Map map, Event ev, bool NewEvent = false)
        {
            this.MapData = map;
            this.OldEvent = ev;
            this.EventData = ev.Clone();
            SetTitle($"{(NewEvent ? "New" : "Edit")} event (ID: {Utilities.Digits(EventData.ID, 3)})");
            SetSize(752, 619);
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
            HeightBox.SetValue(EventData.Height);
            HeightBox.OnValueChanged += delegate (BaseEventArgs e)
            {
                EventData.Height = HeightBox.Value;
                MarkChanges();
            };

            Button NewPage = new Button(this);
            NewPage.SetPosition(414, 43);
            NewPage.SetSize(67, 59);
            NewPage.SetText("New\nPage");

            Button CopyPage = new Button(this);
            CopyPage.SetPosition(481, 43);
            CopyPage.SetSize(67, 59);
            CopyPage.SetText("Copy\nPage");

            Button PastePage = new Button(this);
            PastePage.SetPosition(548, 43);
            PastePage.SetSize(67, 59);
            PastePage.SetText("Paste\nPage");

            Button ClearPage = new Button(this);
            ClearPage.SetPosition(615, 43);
            ClearPage.SetSize(67, 59);
            ClearPage.SetText("Clear\nPage");

            Button DeletePage = new Button(this);
            DeletePage.SetPosition(682, 43);
            DeletePage.SetSize(67, 59);
            DeletePage.SetText("Delete\nPage");

            TabController = new TabView(this);
            TabController.SetXOffset(8);
            TabController.SetPosition(1, 106);
            TabController.SetSize(750, 473);
            TabController.SetHeaderColor(59, 91, 124);
            for (int i = 0; i < EventData.Pages.Count; i++)
            {
                TabContainer tc = TabController.CreateTab(string.IsNullOrWhiteSpace(EventData.Pages[i].Name) ? "Untitled" : EventData.Pages[i].Name);
                EventPageContainer epc = new EventPageContainer(this, EventData, EventData.Pages[i], tc);
                epc.SetSize(750, 444);
                EventPageContainers.Add(epc);
            }

            CreateButton("Apply", Apply);
            CreateButton("Cancel", Cancel);
            CreateButton("OK", OK);
        }

        public void MarkChanges()
        {
            if (Buttons.Count > 0) ApplyButton.SetClickable(true);
        }

        public void Apply(BaseEventArgs e)
        {
            MapData.Events[EventData.ID] = EventData;
            OldEvent = EventData;
            EventData = EventData.Clone();
            ApplyButton.SetClickable(false);
        }

        public void Cancel(BaseEventArgs e)
        {
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

    public class EventPageContainer : Container
    {
        public EditEvent EditEventWindow;
        public Event EventData;
        public EventPage PageData;

        public GraphicWidget GraphicWidget;

        public EventPageContainer(EditEvent eew, Event EventData, EventPage PageData, IContainer Parent) : base(Parent)
        {
            this.EditEventWindow = eew;
            this.EventData = EventData;
            this.PageData = PageData;
            Font BoldFont = Font.Get("Fonts/Ubuntu-B", 14);
            Font f = Font.Get("Fonts/ProductSans-M", 12);

            Label CommandsLabel = new Label(this);
            CommandsLabel.SetFont(BoldFont);
            CommandsLabel.SetPosition(306, 4);
            CommandsLabel.SetText("Commands");
            ListBox CommandBox = new ListBox(this);
            CommandBox.SetPosition(305, 22);
            CommandBox.SetSize(438, 422);

            Label PropsLabel = new Label(this);
            PropsLabel.SetPosition(8, 4);
            PropsLabel.SetFont(BoldFont);
            PropsLabel.SetText("Page Properties");
            EventGroupBox NameGroupBox = new EventGroupBox(this);
            NameGroupBox.SetPosition(7, 22);
            NameGroupBox.SetSize(291, 41);
            Label NameLabel = new Label(NameGroupBox);
            NameLabel.SetFont(f);
            NameLabel.SetText("Page Name:");
            NameLabel.SetPosition(7, 12);
            TextBox NameBox = new TextBox(NameGroupBox);
            NameBox.SetPosition(77, 7);
            NameBox.SetSize(208, 27);
            NameBox.SetInitialText(string.IsNullOrWhiteSpace(PageData.Name) ? "Untitled" : PageData.Name);
            NameBox.OnTextChanged += delegate (BaseEventArgs e)
            {
                PageData.Name = NameBox.Text;
                MarkChanges();
            };

            Label ConditionsLabel = new Label(this);
            ConditionsLabel.SetPosition(8, 67);
            ConditionsLabel.SetFont(BoldFont);
            ConditionsLabel.SetText("Conditions");
            ListBox ConditionsBox = new ListBox(this);
            ConditionsBox.SetPosition(7, 85);
            ConditionsBox.SetSize(163, 70);

            Label TriggerLabel = new Label(this);
            TriggerLabel.SetPosition(177, 67);
            TriggerLabel.SetFont(BoldFont);
            TriggerLabel.SetText("Trigger");
            EventGroupBox TriggerGroupBox = new EventGroupBox(this);
            TriggerGroupBox.SetPosition(176, 85);
            TriggerGroupBox.SetSize(122, 70);
            Label TriggerParamLabel = new Label(TriggerGroupBox);
            TriggerParamLabel.SetPosition(5, 41);
            TriggerParamLabel.SetFont(f);
            TriggerParamLabel.SetText("Sight:");
            NumericBox ParamBox = new NumericBox(TriggerGroupBox);
            ParamBox.SetPosition(62, 36);
            ParamBox.SetSize(55, 27);
            ParamBox.SetValue(PageData.TriggerParam is int ? (int) PageData.TriggerParam : 0);
            ParamBox.OnValueChanged += delegate (BaseEventArgs e)
            {
                if (PageData.TriggerMode == TriggerMode.PlayerTouch || PageData.TriggerMode == TriggerMode.EventTouch)
                {
                    PageData.TriggerParam = ParamBox.Value;
                    MarkChanges();
                }
            };
            DropdownBox TriggerTypeBox = new DropdownBox(TriggerGroupBox);
            TriggerTypeBox.SetItems(new List<ListItem>()
            {
                new ListItem("Action"),
                new ListItem("Player Touch"),
                new ListItem("Event Touch"),
                new ListItem("Autorun"),
                new ListItem("Parallel Process")
            });
            TriggerTypeBox.SetSelectedIndex((int)PageData.TriggerMode);
            TriggerTypeBox.OnSelectionChanged += delegate (BaseEventArgs e)
            {
                PageData.TriggerMode = (TriggerMode) TriggerTypeBox.SelectedIndex;
                if (TriggerTypeBox.SelectedIndex != 1 && TriggerTypeBox.SelectedIndex != 2)
                {
                    TriggerParamLabel.SetVisible(false);
                    ParamBox.SetVisible(false);
                    PageData.TriggerParam = null;
                }
                else
                {
                    TriggerParamLabel.SetVisible(true);
                    ParamBox.SetVisible(true);
                    if (PageData.TriggerParam == null)
                    {
                        PageData.TriggerParam = 0;
                        ParamBox.SetValue(0);
                    }
                }
            };
            TriggerTypeBox.OnSelectionChanged.Invoke(new BaseEventArgs());
            TriggerTypeBox.SetPosition(5, 7);
            TriggerTypeBox.SetSize(112, 25);

            Label GraphicLabel = new Label(this);
            GraphicLabel.SetFont(BoldFont);
            GraphicLabel.SetPosition(8, 159);
            GraphicLabel.SetText("Graphic");
            GraphicWidget = new GraphicWidget(this);
            GraphicWidget.SetPosition(7, 177);
            GraphicWidget.SetEvent(EventData, PageData);

            Label AutoMoveLabel = new Label(this);
            AutoMoveLabel.SetFont(BoldFont);
            AutoMoveLabel.SetPosition(138, 159);
            AutoMoveLabel.SetText("Auto-Moveroute");
            EventGroupBox AutoMoveGroupBox = new EventGroupBox(this);
            AutoMoveGroupBox.SetPosition(134, 177);
            AutoMoveGroupBox.SetSize(164, 126);
            Label AutoMoveTypeLabel = new Label(AutoMoveGroupBox);
            AutoMoveTypeLabel.SetFont(f);
            AutoMoveTypeLabel.SetPosition(15, 10);
            AutoMoveTypeLabel.SetText("Type:");
            DropdownBox AutoMoveTypeBox = new DropdownBox(AutoMoveGroupBox);
            AutoMoveTypeBox.SetPosition(49, 7);
            AutoMoveTypeBox.SetSize(110, 25);
            Button EditAutoMoveButton = new Button(AutoMoveGroupBox);
            EditAutoMoveButton.SetPosition(73, 32);
            EditAutoMoveButton.SetSize(90, 31);
            EditAutoMoveButton.SetText("Edit Route");
            EditAutoMoveButton.SetFont(BoldFont);
            Label AutoMoveSpeedLabel = new Label(AutoMoveGroupBox);
            AutoMoveSpeedLabel.SetPosition(6, 66);
            AutoMoveSpeedLabel.SetFont(f);
            AutoMoveSpeedLabel.SetText("Speed:");
            DropdownBox AutoMoveSpeedBox = new DropdownBox(AutoMoveGroupBox);
            AutoMoveSpeedBox.SetPosition(49, 63);
            AutoMoveSpeedBox.SetSize(110, 25);
            Label AutoMoveStepDelayLabel = new Label(AutoMoveGroupBox);
            AutoMoveStepDelayLabel.SetPosition(24, 98);
            AutoMoveStepDelayLabel.SetFont(f);
            AutoMoveStepDelayLabel.SetText("Step Delay:");
            NumericBox AutoMoveStepDelayBox = new NumericBox(AutoMoveGroupBox);
            AutoMoveStepDelayBox.SetPosition(93, 92);
            AutoMoveStepDelayBox.SetSize(66, 27);

            Label SettingsLabel = new Label(this);
            SettingsLabel.SetFont(BoldFont);
            SettingsLabel.SetPosition(8, 307);
            SettingsLabel.SetText("Event Settings");
            EventGroupBox SettingsGroupBox = new EventGroupBox(this);
            SettingsGroupBox.SetPosition(7, 325);
            SettingsGroupBox.SetSize(291, 119);
            CheckBox MoveAnimBox = new CheckBox(SettingsGroupBox);
            MoveAnimBox.SetPosition(5, 7);
            MoveAnimBox.SetText("Move Animation");
            MoveAnimBox.SetFont(f);
            MoveAnimBox.SetChecked(true);
            CheckBox IdleAnimBox = new CheckBox(SettingsGroupBox);
            IdleAnimBox.SetPosition(5, 27);
            IdleAnimBox.SetText("Idle Animation");
            IdleAnimBox.SetFont(f);
            IdleAnimBox.SetChecked(PageData.Settings.IdleAnimation);
            CheckBox PassableBox = new CheckBox(SettingsGroupBox);
            PassableBox.SetPosition(5, 47);
            PassableBox.SetText("Passable");
            PassableBox.SetFont(f);
            PassableBox.SetChecked(PageData.Settings.Passable);
            CheckBox DirectionLockBox = new CheckBox(SettingsGroupBox);
            DirectionLockBox.SetPosition(5, 67);
            DirectionLockBox.SetText("Direction Lock");
            DirectionLockBox.SetFont(f);
            DirectionLockBox.SetChecked(PageData.Settings.DirectionLock);
            Label PriorityLabel = new Label(SettingsGroupBox);
            PriorityLabel.SetFont(f);
            PriorityLabel.SetPosition(4, 91);
            PriorityLabel.SetText("Priority:");
            DropdownBox PriorityBox = new DropdownBox(SettingsGroupBox);
            PriorityBox.SetPosition(54, 87);
            PriorityBox.SetSize(133, 25);

            CheckBox SavePositionBox = new CheckBox(SettingsGroupBox);
            SavePositionBox.SetPosition(136, 7);
            SavePositionBox.SetText("Save Position");
            SavePositionBox.SetFont(f);
            SavePositionBox.SetChecked(PageData.Settings.SavePosition);
        }

        public void MarkChanges()
        {
            EditEventWindow.MarkChanges();
        }
    }

    public class GraphicWidget : Widget
    {
        public Event EventData;
        public EventPage PageData;

        GraphicGrid GraphicGrid;
        PictureBox Graphic;

        public GraphicWidget(IContainer Parent) : base(Parent)
        {
            SetSize(121, 126);
            EventGroupBox egb = new EventGroupBox(this);
            egb.SetSize(Size);
            GraphicGrid = new GraphicGrid(egb);
            GraphicGrid.SetPosition(2, 2);
            GraphicGrid.SetSize(Size.Width - 4, Size.Height - 4);

            Graphic = new PictureBox(egb);
            Graphic.SetPosition(2, 2);
            Graphic.AutoResize = false;
            Graphic.SetSize(Size.Width - 4, Size.Height - 4);
        }

        public void SetEvent(Event EventData, EventPage PageData)
        {
            this.EventData = EventData;
            this.PageData = PageData;
            Graphic.Sprite.Bitmap?.Dispose();
            if (PageData.Graphic.Type == ":file")
            {
                Graphic.Sprite.Bitmap = new Bitmap(Data.ProjectPath + "/" + PageData.Graphic.Param.ToString());
                Graphic.Sprite.SrcRect.Width = Graphic.Sprite.Bitmap.Width / PageData.Graphic.NumFrames;
                Graphic.Sprite.SrcRect.Height = Graphic.Sprite.Bitmap.Height / PageData.Graphic.NumDirections;
                int dir = 0;
                if (PageData.Graphic.NumDirections == 4) dir = ((PageData.Graphic.Direction / 2) - 1);
                else if (PageData.Graphic.NumDirections == 8) dir = PageData.Graphic.Direction - 1;
                Graphic.Sprite.SrcRect.Y = Graphic.Sprite.SrcRect.Height * dir;
                Graphic.Sprite.X = Size.Width / 2 - Graphic.Sprite.SrcRect.Width / 2 - 2;
                Graphic.Sprite.Y = Size.Height / 2 - Graphic.Sprite.SrcRect.Height / 2 - 2;
                ConfigureGrid();
            }
        }

        public void ConfigureGrid()
        {
            GraphicGrid.SetOffset(EventData.Width % 2 == 0 ? 6 : 22, 15);
        }

        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);
            if (WidgetIM.Hovering)
            {
                ChooseGraphic cg = new ChooseGraphic(PageData.Graphic);
                cg.OnClosed += delegate (BaseEventArgs e)
                {
                    if (PageData.Graphic != cg.GraphicData)
                    {
                        PageData.Graphic = cg.GraphicData;
                        SetEvent(this.EventData, this.PageData);
                    }
                };
            }
        }
    }

    public class GraphicGrid : Widget
    {
        public int TileSize = 32;
        public int OffsetX = 0;
        public int OffsetY = 0;
        public string Border;

        public GraphicGrid(IContainer Parent) : base(Parent)
        {
            Sprites["one"] = new Sprite(this.Viewport, new SolidBitmap(32, 32, new Color(226, 226, 226)));
            Sprites["two"] = new Sprite(this.Viewport, new SolidBitmap(32, 32, new Color(246, 246, 246)));
            RedrawGrid();
        }

        public override void SizeChanged(BaseEventArgs e)
        {
            base.SizeChanged(e);
            RedrawGrid();
        }

        public void SetOffset(int x, int y)
        {
            if (this.OffsetX != x || this.OffsetY != y)
            {
                this.OffsetX = x % 32;
                this.OffsetY = y % 32;
                RedrawGrid();
            }
        }

        public void RedrawGrid()
        {
            Sprites["one"].MultiplePositions.Clear();
            Sprites["two"].MultiplePositions.Clear();

            for (int y = 0; y < (int) Math.Ceiling(Size.Height / 32d) + 1; y++)
            {
                for (int x = 0; x < (int) Math.Ceiling(Size.Width / 32d) + 1; x++)
                {
                    if (x % 2 == y % 2) Sprites["one"].MultiplePositions.Add(new Point(x * 32 - OffsetX, y * 32 - OffsetY));
                    else Sprites["two"].MultiplePositions.Add(new Point(x * 32 - OffsetX, y * 32 - OffsetY));
                }
            }
        }
    }
}
