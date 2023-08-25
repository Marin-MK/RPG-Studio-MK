using System.Collections.Generic;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class EventPageControl : Widget
{
    CheckBox Switch1Box;
    SwitchPickerBox Switch1VarBox;
    CheckBox Switch2Box;
    SwitchPickerBox Switch2VarBox;
    CheckBox VarBox;
    VariablePickerBox VarPickBox;
    NumericBox VarValueBox;
    CheckBox SelfSwitchBox;
    DropdownBox SelfSwitchVarBox;
    EventGraphicBox EventGraphicBox;
    CheckBox MoveAnimationBox;
    CheckBox StopAnimationBox;
    CheckBox DirectionFixBox;
    CheckBox ThroughBox;
    CheckBox AlwaysOnTopBox;
    DropdownBox TriggerBox;
    DropdownBox AutoMoveTypeBox;
    DropdownBox AutoMoveSpeedBox;
    DropdownBox AutoMoveFrequencyBox;
    Button EditRouteButton;
    EventCommandBox CommandBox;

    Map Map;
    Event Event;
    EventPage Page;

    public EventPageControl(IContainer Parent) : base(Parent)
    {
        SetBackgroundColor(24, 38, 53);

        Label VarLabel1 = null;
        Label VarLabel2 = null;

        Container RightBox = new Container(this);
        RightBox.SetPosition(273, 70);
        RightBox.SetSize(217, 485);
        RightBox.SetBackgroundColor(40, 62, 84);
        Label ConditionsLabel = new Label(RightBox);
        ConditionsLabel.SetFont(Fonts.Header);
        ConditionsLabel.SetPosition(11, 9);
        ConditionsLabel.SetText("Conditions");
        Switch1Box = new CheckBox(RightBox);
        Switch1Box.SetFont(Fonts.Paragraph);
        Switch1Box.SetPosition(20, 34);
        Switch1Box.SetText("Switch is ON");
        Switch1Box.OnCheckChanged += _ =>
        {
            Switch1VarBox.SetEnabled(Switch1Box.Checked);
            Page.Condition.Switch1Valid = Switch1Box.Checked;
        };
        Switch1VarBox = new SwitchPickerBox(RightBox);
        Switch1VarBox.SetFont(Fonts.Paragraph);
        Switch1VarBox.SetPosition(20, 54);
        Switch1VarBox.SetSize(177, 25);
        Switch1VarBox.SetEnabled(false);
        Switch1VarBox.OnSwitchChanged += _ => Page.Condition.Switch1ID = Switch1VarBox.SwitchID;
        Switch2Box = new CheckBox(RightBox);
        Switch2Box.SetFont(Fonts.Paragraph);
        Switch2Box.SetPosition(20, 88);
        Switch2Box.SetText("Switch is ON");
        Switch2Box.OnCheckChanged += _ =>
        {
            Switch2VarBox.SetEnabled(Switch2Box.Checked);
            Page.Condition.Switch2Valid = Switch2Box.Checked;
        };
        Switch2VarBox = new SwitchPickerBox(RightBox);
        Switch2VarBox.SetFont(Fonts.Paragraph);
        Switch2VarBox.SetPosition(20, 108);
        Switch2VarBox.SetSize(177, 25);
        Switch2VarBox.SetEnabled(false);
        Switch2VarBox.OnSwitchChanged += _ => Page.Condition.Switch2ID = Switch2VarBox.SwitchID;
        SelfSwitchBox = new CheckBox(RightBox);
        SelfSwitchBox.SetFont(Fonts.Paragraph);
        SelfSwitchBox.SetPosition(20, 142);
        SelfSwitchBox.SetText("Self Switch is ON");
        SelfSwitchBox.OnCheckChanged += _ =>
        {
            SelfSwitchVarBox.SetEnabled(SelfSwitchBox.Checked);
            Page.Condition.SelfSwitchValid = SelfSwitchBox.Checked;
        };
        SelfSwitchVarBox = new DropdownBox(RightBox);
        SelfSwitchVarBox.SetFont(Fonts.Paragraph);
        SelfSwitchVarBox.SetPosition(20, 162);
        SelfSwitchVarBox.SetSize(177, 25);
        SelfSwitchVarBox.SetItems(new List<ListItem>()
        {
            new ListItem("A"), new ListItem("B"), new ListItem("C"),
            new ListItem("D"), new ListItem("E"), new ListItem("F")
        });
        SelfSwitchVarBox.SetEnabled(false);
        SelfSwitchVarBox.OnSelectionChanged += _ => Page.Condition.SelfSwitchChar = (char)('A' + SelfSwitchVarBox.SelectedIndex);
        VarBox = new CheckBox(RightBox);
        VarBox.SetFont(Fonts.Paragraph);
        VarBox.SetPosition(20, 196);
        VarBox.SetText("Variable");
        VarBox.OnCheckChanged += _ =>
        {
            VarPickBox.SetEnabled(VarBox.Checked);
            VarLabel1.SetEnabled(VarBox.Checked);
            VarValueBox.SetEnabled(VarBox.Checked);
            VarLabel2.SetEnabled(VarBox.Checked);
            Page.Condition.VariableValid = VarBox.Checked;
        };
        VarPickBox = new VariablePickerBox(RightBox);
        VarPickBox.SetFont(Fonts.Paragraph);
        VarPickBox.SetPosition(20, 216);
        VarPickBox.SetSize(177, 25);
        VarPickBox.SetEnabled(false);
        VarPickBox.OnVariableChanged += _ => Page.Condition.VariableID = VarPickBox.VariableID;
        VarLabel1 = new Label(RightBox);
        VarLabel1.SetFont(Fonts.Paragraph);
        VarLabel1.SetPosition(24, 251);
        VarLabel1.SetText("is");
        VarLabel1.SetEnabled(false);
        VarValueBox = new NumericBox(RightBox);
        VarValueBox.SetPosition(42, 248);
        VarValueBox.SetSize(88, 27);
        VarValueBox.SetEnabled(false);
        VarValueBox.OnValueChanged += _ => Page.Condition.VariableValue = VarValueBox.Value;
        VarLabel2 = new Label(RightBox);
        VarLabel2.SetFont(Fonts.Paragraph);
        VarLabel2.SetPosition(136, 251);
        VarLabel2.SetText("or higher");
        VarLabel2.SetEnabled(false);

        Label TriggerLabel = new Label(RightBox);
        TriggerLabel.SetFont(Fonts.Header);
        TriggerLabel.SetPosition(10, 297);
        TriggerLabel.SetText("Trigger");
        TriggerBox = new DropdownBox(RightBox);
        TriggerBox.SetPosition(21, 319);
        TriggerBox.SetSize(147, 25);
        TriggerBox.SetItems(new List<ListItem>()
        {
            new ListItem("Action Button"),
            new ListItem("Player Touch"),
            new ListItem("Event Touch"),
            new ListItem("Autorun"),
            new ListItem("Parallel Process")
        });
        TriggerBox.OnSelectionChanged += _ => { Page.TriggerMode = (TriggerMode) TriggerBox.SelectedIndex; };

        Container LeftBox = new Container(this);
        LeftBox.SetPosition(100, 70);
        LeftBox.SetSize(170, 485);
        LeftBox.SetBackgroundColor(40, 62, 84);
        Label GraphicLabel = new Label(LeftBox);
        GraphicLabel.SetFont(Fonts.Header);
        GraphicLabel.SetPosition(11, 9);
        GraphicLabel.SetText("Graphic");
        EventGraphicBox = new EventGraphicBox(LeftBox);
        EventGraphicBox.SetPosition(21, 27);
        EventGraphicBox.SetSize(126, 126);
        // Use OnDoubleLeftMouseDown to turn into double clicking instead of single clicking
        EventGraphicBox.OnLeftMouseUp += _ =>
        {
            if (EventGraphicBox.Mouse.LeftStartedInside && EventGraphicBox.Mouse.Inside)
            {
                ChooseGraphic cg = new ChooseGraphic(Map, Event, Page, Page.Graphic, false, "Graphics/Characters");
                cg.OnClosed += _ =>
                {
                    if (!cg.Apply) return;
                    Page.Graphic = cg.Graphic;
                    EventGraphicBox.SetGraphic(Map, Event, Page.Graphic);
                };
            }
        };

        Label AutoMoveSpeedLabel = new Label(LeftBox);
        AutoMoveSpeedLabel.SetPosition(19, 285);
        AutoMoveSpeedLabel.SetFont(Fonts.Paragraph);
        AutoMoveSpeedLabel.SetText("Speed:");
        AutoMoveSpeedBox = new DropdownBox(LeftBox);
        AutoMoveSpeedBox.SetPosition(19, 307);
        AutoMoveSpeedBox.SetSize(128, 25);
        AutoMoveSpeedBox.SetFont(Fonts.Paragraph);
        AutoMoveSpeedBox.SetItems(new List<ListItem>()
        {
            new ListItem("1: Slowest"), new ListItem("2: Slower"), new ListItem("3: Slow"),
            new ListItem("4: Fast"), new ListItem("5: Faster"), new ListItem("6: Fastest")
        });
        AutoMoveSpeedBox.OnSelectionChanged += _ =>
        {
            Page.MoveRoute.Speed = AutoMoveSpeedBox.SelectedIndex + 1;
        };
        Label AutoMoveFrequencyLabel = new Label(LeftBox);
        AutoMoveFrequencyLabel.SetPosition(19, 341);
        AutoMoveFrequencyLabel.SetFont(Fonts.Paragraph);
        AutoMoveFrequencyLabel.SetText("Freq:");
        AutoMoveFrequencyBox = new DropdownBox(LeftBox);
        AutoMoveFrequencyBox.SetPosition(19, 363);
        AutoMoveFrequencyBox.SetSize(128, 25);
        AutoMoveFrequencyBox.SetFont(Fonts.Paragraph);
        AutoMoveFrequencyBox.SetItems(new List<ListItem>()
        {
            new ListItem("1: Lowest"), new ListItem("2: Lower"), new ListItem("3: Low"),
            new ListItem("4: High"), new ListItem("5: Higher"), new ListItem("6: Highest")
        });
        AutoMoveFrequencyBox.OnSelectionChanged += _ =>
        {
            Page.MoveRoute.Frequency = AutoMoveFrequencyBox.SelectedIndex + 1;
        };
        Label AutoMoveTypeLabel = new Label(LeftBox);
        AutoMoveTypeLabel.SetPosition(19, 397);
        AutoMoveTypeLabel.SetFont(Fonts.Paragraph);
        AutoMoveTypeLabel.SetText("Type:");
        AutoMoveTypeBox = new DropdownBox(LeftBox);
        AutoMoveTypeBox.SetPosition(19, 419);
        AutoMoveTypeBox.SetSize(128, 25);
        AutoMoveTypeBox.SetFont(Fonts.Paragraph);
        AutoMoveTypeBox.SetItems(new List<ListItem>()
        {
            new ListItem("Fixed"), new ListItem("Random"),
            new ListItem("Approach"), new ListItem("Custom")
        });
        AutoMoveTypeBox.OnSelectionChanged += _ =>
        {
            EditRouteButton.SetEnabled(AutoMoveTypeBox.SelectedIndex == 3);
            Page.MoveRoute.Type = AutoMoveTypeBox.SelectedIndex;
        };
        EditRouteButton = new Button(LeftBox);
        EditRouteButton.SetPosition(60, 445);
        EditRouteButton.SetSize(92, 29);
        EditRouteButton.SetText("Edit Route");
        EditRouteButton.SetEnabled(false);
        EditRouteButton.OnClicked += _ =>
        {
            EditMoveRouteWindow win = new EditMoveRouteWindow(Map, Event, Page, Page.MoveRoute, -1, true);
            win.OnClosed += _ =>
            {
                if (!win.Apply) return;
                Page.MoveRoute = win.MoveRoute;
            };
        };

        Container OptionsBox = new Container(LeftBox);
        OptionsBox.SetPosition(19, 166);
        OptionsBox.SetSize(125, 106);
        MoveAnimationBox = new CheckBox(OptionsBox);
        MoveAnimationBox.SetFont(Fonts.Paragraph);
        MoveAnimationBox.SetText("Move Animation");
        MoveAnimationBox.OnCheckChanged += _ => Page.Settings.WalkAnime = MoveAnimationBox.Checked;
        StopAnimationBox = new CheckBox(OptionsBox);
        StopAnimationBox.SetPosition(0, 22);
        StopAnimationBox.SetFont(Fonts.Paragraph);
        StopAnimationBox.SetText("Idle Animation");
        StopAnimationBox.OnCheckChanged += _ => Page.Settings.StepAnime = StopAnimationBox.Checked;
        DirectionFixBox = new CheckBox(OptionsBox);
        DirectionFixBox.SetPosition(0, 44);
        DirectionFixBox.SetFont(Fonts.Paragraph);
        DirectionFixBox.SetText("Direction Fix");
        DirectionFixBox.OnCheckChanged += _ => Page.Settings.DirectionFix = DirectionFixBox.Checked;
        ThroughBox = new CheckBox(OptionsBox);
        ThroughBox.SetPosition(0, 66);
        ThroughBox.SetFont(Fonts.Paragraph);
        ThroughBox.SetText("Through");
        ThroughBox.OnCheckChanged += _ => Page.Settings.Through = ThroughBox.Checked;
        AlwaysOnTopBox = new CheckBox(OptionsBox);
        AlwaysOnTopBox.SetPosition(0, 88);
        AlwaysOnTopBox.SetFont(Fonts.Paragraph);
        AlwaysOnTopBox.SetText("Always on Top");
        AlwaysOnTopBox.OnCheckChanged += _ => Page.Settings.AlwaysOnTop = AlwaysOnTopBox.Checked;

        CommandBox = new EventCommandBox(this);
        CommandBox.SetDocked(true);
        CommandBox.SetPadding(493, 3, 2, 5);
    }

    public void RedrawGraphic()
    {
        EventGraphicBox.RedrawGraphic();
    }

    public void SetEventPage(Map Map, Event Event, EventPage Page)
    {
        this.Map = Map;
        this.Event = Event;
        this.Page = Page;
        Switch1Box.SetChecked(Page.Condition.Switch1Valid);
        Switch1VarBox.SetSwitchID(Page.Condition.Switch1ID);
        Switch2Box.SetChecked(Page.Condition.Switch2Valid);
        Switch2VarBox.SetSwitchID(Page.Condition.Switch2ID);
        VarBox.SetChecked(Page.Condition.VariableValid);
        VarPickBox.SetVariableID(Page.Condition.VariableID);
        VarValueBox.SetValue(Page.Condition.VariableValue);
        SelfSwitchBox.SetChecked(Page.Condition.SelfSwitchValid);
        SelfSwitchVarBox.SetSelectedIndex(Page.Condition.SelfSwitchChar - 'A');
        EventGraphicBox.SetGraphic(Map, Event, Page.Graphic);
        MoveAnimationBox.SetChecked(Page.Settings.WalkAnime);
        StopAnimationBox.SetChecked(Page.Settings.StepAnime);
        DirectionFixBox.SetChecked(Page.Settings.DirectionFix);
        ThroughBox.SetChecked(Page.Settings.Through);
        AlwaysOnTopBox.SetChecked(Page.Settings.AlwaysOnTop);
        TriggerBox.SetSelectedIndex((int) Page.TriggerMode);
        AutoMoveTypeBox.SetSelectedIndex(Page.MoveRoute.Type);
        AutoMoveSpeedBox.SetSelectedIndex(Page.MoveRoute.Speed - 1);
        AutoMoveFrequencyBox.SetSelectedIndex(Page.MoveRoute.Frequency - 1);
        string str = "";
        for (int i = 0; i < Page.Commands.Count; i++)
        {
            EventCommand cmd = Page.Commands[i];
            for (int j = 0; j < cmd.Indent; j++) str += "    ";
            string parameters = "";
            cmd.Parameters.ForEach(o => parameters += o.ToString() + ", ");
            if (parameters.Length > 2) parameters = parameters.Substring(0, parameters.Length - 2);
            str += $"{cmd.Code}: {parameters}\n";
        }
        CommandBox.SetCommands(Map, Event, Page, Page.Commands);
    }
}
