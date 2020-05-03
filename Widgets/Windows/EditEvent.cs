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
        Button DeletePageButton;

        List<EventPageContainer> EventPageContainers = new List<EventPageContainer>();

        public EditEvent(Map map, Event ev, bool NewEvent = false)
        {
            this.MapData = map;
            this.OldEvent = ev;
            this.EventData = ev.Clone();
            SetTitle($"{(NewEvent ? "New" : "Edit")} event (ID: {Utilities.Digits(EventData.ID, 3)})");
            SetSize(752, 690);
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
            if (EventData.Pages.Count == 1) DeletePageButton.SetClickable(false);
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
            ApplyButton.SetClickable(false);
        }

        public void MarkChanges()
        {
            if (Buttons.Count > 0) ApplyButton.SetClickable(true);
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
            DeletePageButton.SetClickable(true);
        }

        public void CopyPage()
        {

        }

        public void PastePage()
        {

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
            if (EventData.Pages.Count == 1) DeletePageButton.SetClickable(false);
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

    public class EventPageContainer : Container
    {
        public EditEvent EditEventWindow;
        public Event EventData;
        public EventPage PageData;

        public GraphicWidget GraphicWidget;
        public CheckBox PassableBox;

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
            CommandBox.SetSize(438, 493);

            Label PropsLabel = new Label(this);
            PropsLabel.SetPosition(8, 4);
            PropsLabel.SetFont(BoldFont);
            PropsLabel.SetText("Page Properties");
            EventGroupBox PropBox = new EventGroupBox(this);
            PropBox.SetPosition(7, 22);
            PropBox.SetSize(291, 157);
            Label NameLabel = new Label(PropBox);
            NameLabel.SetFont(f);
            NameLabel.SetText("Page Name:");
            NameLabel.SetPosition(7, 12);
            TextBox NameBox = new TextBox(PropBox);
            NameBox.SetPosition(77, 7);
            NameBox.SetSize(208, 27);
            NameBox.SetInitialText(string.IsNullOrWhiteSpace(PageData.Name) ? "Untitled" : PageData.Name);
            NameBox.OnTextChanged += delegate (BaseEventArgs e)
            {
                PageData.Name = NameBox.Text;
                EditEventWindow.UpdateNames();
                MarkChanges();
            };
            Label ConditionsLabel = new Label(PropBox);
            ConditionsLabel.SetPosition(8, 40);
            ConditionsLabel.SetFont(f);
            ConditionsLabel.SetText("Conditions:");
            ListBox ConditionsBox = new ListBox(PropBox);
            ConditionsBox.SetPosition(6, 61);
            ConditionsBox.SetSize(279, 65);
            Button EditConditionsButton = new Button(PropBox);
            EditConditionsButton.SetText("Edit");
            EditConditionsButton.SetPosition(230, 126);
            EditConditionsButton.SetSize(59, 29);
            EditConditionsButton.OnClicked += delegate (BaseEventArgs e) { new MessageBox("Error", "WIP", IconType.Error); };

            Label TriggerLabel = new Label(this);
            TriggerLabel.SetPosition(11, 366);
            TriggerLabel.SetFont(BoldFont);
            TriggerLabel.SetText("Trigger");
            EventGroupBox TriggerGroupBox = new EventGroupBox(this);
            TriggerGroupBox.SetPosition(7, 384);
            TriggerGroupBox.SetSize(121, 71);
            Label TriggerParamLabel = new Label(TriggerGroupBox);
            TriggerParamLabel.SetPosition(11, 41);
            TriggerParamLabel.SetFont(f);
            TriggerParamLabel.SetText("Sight:");
            NumericBox ParamBox = new NumericBox(TriggerGroupBox);
            ParamBox.SetPosition(62, 36);
            ParamBox.SetSize(55, 27);
            ParamBox.MinValue = 0;
            ParamBox.MaxValue = 999;
            if (PageData.TriggerParam is int || PageData.TriggerParam is long) ParamBox.SetValue(Convert.ToInt32(PageData.TriggerParam));
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
            TriggerTypeBox.SetSelectedIndex((int) PageData.TriggerMode);
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
                MarkChanges();
            };
            TriggerTypeBox.OnSelectionChanged.Invoke(new BaseEventArgs());
            TriggerTypeBox.SetPosition(5, 7);
            TriggerTypeBox.SetSize(112, 25);

            Label GraphicLabel = new Label(this);
            GraphicLabel.SetFont(BoldFont);
            GraphicLabel.SetPosition(10, 183);
            GraphicLabel.SetText("Graphic");
            GraphicWidget = new GraphicWidget(this);
            GraphicWidget.SetPosition(7, 201);
            GraphicWidget.SetEvent(EventData, PageData);

            Label AutoMoveLabel = new Label(this);
            AutoMoveLabel.SetFont(BoldFont);
            AutoMoveLabel.SetPosition(140, 183);
            AutoMoveLabel.SetText("Auto-Moveroute");
            EventGroupBox AutoMoveGroupBox = new EventGroupBox(this);
            AutoMoveGroupBox.SetPosition(136, 201);
            AutoMoveGroupBox.SetSize(162, 93);
            Label AutoMoveTypeLabel = new Label(AutoMoveGroupBox);
            AutoMoveTypeLabel.SetFont(f);
            AutoMoveTypeLabel.SetPosition(15, 10);
            AutoMoveTypeLabel.SetText("Type:");
            DropdownBox AutoMoveTypeBox = new DropdownBox(AutoMoveGroupBox);
            AutoMoveTypeBox.SetPosition(49, 7);
            AutoMoveTypeBox.SetSize(110, 25);
            AutoMoveTypeBox.SetItems(new List<ListItem>()
            {
                new ListItem("WIP")
            });
            AutoMoveTypeBox.SetSelectedIndex(0);
            Button EditAutoMoveButton = new Button(AutoMoveGroupBox);
            EditAutoMoveButton.SetPosition(73, 32);
            EditAutoMoveButton.SetSize(90, 31);
            EditAutoMoveButton.SetText("Edit Route");
            EditAutoMoveButton.SetFont(BoldFont);
            EditAutoMoveButton.OnClicked += delegate (BaseEventArgs e)
            {
                new MessageBox("Error", "WIP", IconType.Error);
            };
            Label AutoMoveMoveDelayLabel = new Label(AutoMoveGroupBox);
            AutoMoveMoveDelayLabel.SetPosition(24, 68);
            AutoMoveMoveDelayLabel.SetFont(f);
            AutoMoveMoveDelayLabel.SetText("Move Delay:");
            NumericBox AutoMoveStepDelayBox = new NumericBox(AutoMoveGroupBox);
            AutoMoveStepDelayBox.SetPosition(93, 62);
            AutoMoveStepDelayBox.SetSize(66, 27);
            AutoMoveStepDelayBox.MinValue = 0;
            AutoMoveStepDelayBox.MaxValue = 999;

            Label SettingsLabel = new Label(this);
            SettingsLabel.SetFont(BoldFont);
            SettingsLabel.SetPosition(140, 298);
            SettingsLabel.SetText("Event Settings");
            EventGroupBox SettingsGroupBox = new EventGroupBox(this);
            SettingsGroupBox.SetPosition(136, 316);
            SettingsGroupBox.SetSize(162, 199);
            Label MoveSpeedLabel = new Label(SettingsGroupBox);
            MoveSpeedLabel.SetFont(f);
            MoveSpeedLabel.SetText("Move Speed:");
            MoveSpeedLabel.SetPosition(6, 7);
            DropdownBox MoveSpeedBox = new DropdownBox(SettingsGroupBox);
            MoveSpeedBox.SetPosition(83, 3);
            MoveSpeedBox.SetSize(76, 25);
            MoveSpeedBox.SetItems(new List<ListItem>()
            {
                new ListItem("Walking"),
                new ListItem("Running"),
                new ListItem("Cycling")
            });
            MoveSpeedBox.SetSelectedIndex(PageData.Settings.MoveSpeed == 0.25f ? 0 : PageData.Settings.MoveSpeed == 0.125f ? 1 : 2);
            MoveSpeedBox.OnSelectionChanged += delegate (BaseEventArgs e)
            {
                PageData.Settings.MoveSpeed = new float[] { 0.25f, 0.125f, 0.1f }[MoveSpeedBox.SelectedIndex];
                MarkChanges();
            };
            Label IdleSpeedLabel = new Label(SettingsGroupBox);
            IdleSpeedLabel.SetFont(f);
            IdleSpeedLabel.SetText("Idle Speed:");
            IdleSpeedLabel.SetPosition(6, 37);
            DropdownBox IdleSpeedBox = new DropdownBox(SettingsGroupBox);
            IdleSpeedBox.SetPosition(83, 33);
            IdleSpeedBox.SetSize(76, 25);
            IdleSpeedBox.SetItems(new List<ListItem>()
            {
                new ListItem("Walking"),
                new ListItem("Running"),
                new ListItem("Cycling")
            });
            IdleSpeedBox.SetSelectedIndex(PageData.Settings.IdleSpeed == 0.25f ? 0 : PageData.Settings.IdleSpeed == 0.125f ? 1 : 2);
            IdleSpeedBox.OnSelectionChanged += delegate (BaseEventArgs e)
            {
                PageData.Settings.IdleSpeed = new float[] { 0.25f, 0.125f, 0.1f }[IdleSpeedBox.SelectedIndex];
                MarkChanges();
            };
            CheckBox MoveAnimBox = new CheckBox(SettingsGroupBox);
            MoveAnimBox.SetPosition(5, 63);
            MoveAnimBox.SetText("Move Animation");
            MoveAnimBox.SetFont(f);
            MoveAnimBox.SetChecked(PageData.Settings.MoveAnimation);
            MoveAnimBox.OnCheckChanged += delegate (BaseEventArgs e)
            {
                PageData.Settings.MoveAnimation = MoveAnimBox.Checked;
                MarkChanges();
            };
            CheckBox IdleAnimBox = new CheckBox(SettingsGroupBox);
            IdleAnimBox.SetPosition(5, 83);
            IdleAnimBox.SetText("Idle Animation");
            IdleAnimBox.SetFont(f);
            IdleAnimBox.SetChecked(PageData.Settings.IdleAnimation);
            IdleAnimBox.OnCheckChanged += delegate (BaseEventArgs e)
            {
                PageData.Settings.IdleAnimation = IdleAnimBox.Checked;
                MarkChanges();
            };
            PassableBox = new CheckBox(SettingsGroupBox);
            PassableBox.SetPosition(5, 103);
            PassableBox.SetText("Passable");
            PassableBox.SetFont(f);
            PassableBox.SetChecked(PageData.Settings.Passable);
            PassableBox.OnCheckChanged += delegate (BaseEventArgs e)
            {
                PageData.Settings.Passable = PassableBox.Checked;
                MarkChanges();
            };
            CheckBox SavePositionBox = new CheckBox(SettingsGroupBox);
            SavePositionBox.SetPosition(5, 123);
            SavePositionBox.SetText("Save Position");
            SavePositionBox.SetFont(f);
            SavePositionBox.SetChecked(PageData.Settings.SavePosition);
            SavePositionBox.OnCheckChanged += delegate (BaseEventArgs e)
            {
                PageData.Settings.SavePosition = SavePositionBox.Checked;
                MarkChanges();
            };
            CheckBox DirectionLockBox = new CheckBox(SettingsGroupBox);
            DirectionLockBox.SetPosition(5, 143);
            DirectionLockBox.SetText("Direction Lock");
            DirectionLockBox.SetFont(f);
            DirectionLockBox.SetChecked(PageData.Settings.DirectionLock);
            DirectionLockBox.OnCheckChanged += delegate (BaseEventArgs e)
            {
                PageData.Settings.DirectionLock = DirectionLockBox.Checked;
                MarkChanges();
            };
            Label PriorityLabel = new Label(SettingsGroupBox);
            PriorityLabel.SetFont(f);
            PriorityLabel.SetPosition(6, 168);
            PriorityLabel.SetText("Priority:");
            DropdownBox PriorityBox = new DropdownBox(SettingsGroupBox);
            PriorityBox.SetPosition(54, 164);
            PriorityBox.SetSize(105, 25);
            PriorityBox.SetItems(new List<ListItem>()
            {
                new ListItem("WIP")
            });
            PriorityBox.SetSelectedIndex(0);
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
            SetSize(121, 161);
            EventGroupBox egb = new EventGroupBox(this);
            egb.SetSize(Size);
            GraphicGrid = new GraphicGrid(egb);
            GraphicGrid.SetPosition(2, 2);
            GraphicGrid.SetSize(Size.Width - 4, Size.Height - 4);
            GraphicGrid.SetOffset(22, 30);

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
            GraphicGrid.SetOffset(EventData.Width % 2 == 0 ? 6 : 22, 30);
        }

        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);
            if (WidgetIM.Hovering && e.LeftButton != e.OldLeftButton)
            {
                ChooseGraphic cg = new ChooseGraphic(PageData.Graphic);
                cg.OnClosed += delegate (BaseEventArgs e)
                {
                    if (PageData.Graphic.Type != cg.GraphicData.Type ||
                        PageData.Graphic.Param != cg.GraphicData.Param ||
                        PageData.Graphic.NumDirections != cg.GraphicData.NumDirections ||
                        PageData.Graphic.NumFrames != cg.GraphicData.NumFrames ||
                        PageData.Graphic.Direction != cg.GraphicData.Direction)
                        MarkChanges();
                    if (PageData.Graphic != cg.GraphicData)
                    {
                        if (PageData.Graphic.Type != cg.GraphicData.Type ||
                            PageData.Graphic.Param != cg.GraphicData.Param)
                        {
                            PageData.Settings.Passable = false;
                            ((EventPageContainer) Parent).PassableBox.SetChecked(false);
                        }
                        PageData.Graphic = cg.GraphicData;
                        SetEvent(this.EventData, this.PageData);
                    }
                };
            }
        }

        public void MarkChanges()
        {
            ((EventPageContainer) Parent).MarkChanges();
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
