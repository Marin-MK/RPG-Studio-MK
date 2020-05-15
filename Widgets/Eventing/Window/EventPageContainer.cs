using System;
using System.Collections.Generic;
using System.Text;
using MKEditor.Game;
using ODL;

namespace MKEditor.Widgets
{
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
            ConditionBox ConditionBox = new ConditionBox(PropBox);
            ConditionBox.SetPosition(6, 61);
            ConditionBox.SetSize(279, 65);
            ConditionBox.SetEventPage(EventData, PageData);
            Button EditConditionsButton = new Button(PropBox);
            EditConditionsButton.SetText("Edit");
            EditConditionsButton.SetPosition(230, 126);
            EditConditionsButton.SetSize(59, 29);
            EditConditionsButton.OnClicked += delegate (BaseEventArgs e) 
            {
                EditConditionsWindow edw = new EditConditionsWindow(EventData, PageData);
                edw.OnClosed += delegate (BaseEventArgs e)
                {
                    if (edw.NeedUpdate) ConditionBox.SetEventPage(EventData, PageData);
                };
            };

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
            TriggerTypeBox.SetSelectedIndex((int)PageData.TriggerMode);
            TriggerTypeBox.OnSelectionChanged += delegate (BaseEventArgs e)
            {
                PageData.TriggerMode = (TriggerMode)TriggerTypeBox.SelectedIndex;
                if (TriggerTypeBox.SelectedIndex != 1 && TriggerTypeBox.SelectedIndex != 2)
                {
                    TriggerParamLabel.SetEnabled(false);
                    ParamBox.SetEnabled(false);
                    PageData.TriggerParam = null;
                }
                else
                {
                    TriggerParamLabel.SetEnabled(true);
                    ParamBox.SetEnabled(true);
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
            AutoMoveLabel.SetEnabled(false);
            AutoMoveTypeLabel.SetEnabled(false);
            AutoMoveTypeBox.SetEnabled(false);
            EditAutoMoveButton.SetEnabled(false);
            AutoMoveMoveDelayLabel.SetEnabled(false);
            AutoMoveStepDelayBox.SetEnabled(false);

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
}
