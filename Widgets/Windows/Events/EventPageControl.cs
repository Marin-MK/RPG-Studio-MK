using System;
using System.Collections.Generic;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class EventPageControl : Widget
{
    CheckBox Switch1Box;
    BrowserBox Switch1VarBox;
    CheckBox Switch2Box;
    BrowserBox Switch2VarBox;
    CheckBox VarBox;
    BrowserBox VarPickBox;
    NumericBox VarValueBox;
    CheckBox SelfSwitchBox;
    DropdownBox SelfSwitchVarBox;
    CheckBox MoveAnimationBox;
    CheckBox StopAnimationBox;
    CheckBox DirectionFixBox;
    CheckBox ThroughBox;
    CheckBox AlwaysOnTopBox;
    RadioBox ActionBox;
    RadioBox PlayerBox;
    RadioBox EventBox;
    RadioBox AutorunBox;
    RadioBox ParallelBox;
    DropdownBox AutoMoveTypeBox;
    DropdownBox AutoMoveSpeedBox;
    DropdownBox AutoMoveFrequencyBox;
    Button EditRouteButton;

    public EventPageControl(IContainer Parent) : base(Parent)
    {
        Font HeaderFont = Fonts.ProductSansMedium.Use(16);
        Font SmallFont = Fonts.ProductSansMedium.Use(14);
        Font TinyFont = Fonts.ProductSansMedium.Use(12);

        Label Switch1Label = null;
        Label Switch2Label = null;
        Label VarLabel1 = null;
        Label VarLabel2 = null;
        Label SelfSwitchLabel = null;

        Label ConditionsLabel = new Label(this);
        ConditionsLabel.SetFont(HeaderFont);
        ConditionsLabel.SetPosition(7, 2);
        ConditionsLabel.SetText("Conditions");
        GroupBox ConditionsBox = new GroupBox(this);
        ConditionsBox.SetPosition(7, 22);
        ConditionsBox.SetSize(261, 157);
        Switch1Box = new CheckBox(ConditionsBox);
        Switch1Box.SetFont(SmallFont);
        Switch1Box.SetPosition(7, 7);
        Switch1Box.SetText("Switch");
        Switch1Box.OnCheckChanged += _ =>
        {
            Switch1VarBox.SetEnabled(Switch1Box.Checked);
            Switch1Label.SetEnabled(Switch1Box.Checked);
        };
        Switch1VarBox = new BrowserBox(ConditionsBox);
        Switch1VarBox.SetFont(SmallFont);
        Switch1VarBox.SetPosition(102, 3);
        Switch1VarBox.SetSize(118, 25);
        Switch1VarBox.SetEnabled(false);
        Switch1Label = new Label(ConditionsBox);
        Switch1Label.SetFont(SmallFont);
        Switch1Label.SetPosition(224, 7);
        Switch1Label.SetText("is ON");
        Switch1Label.SetEnabled(false);
        Switch2Box = new CheckBox(ConditionsBox);
        Switch2Box.SetFont(SmallFont);
        Switch2Box.SetPosition(7, 7 + 32);
        Switch2Box.SetText("Switch");
        Switch2Box.OnCheckChanged += _ =>
        {
            Switch2VarBox.SetEnabled(Switch2Box.Checked);
            Switch2Label.SetEnabled(Switch2Box.Checked);
        };
        Switch2VarBox = new BrowserBox(ConditionsBox);
        Switch2VarBox.SetFont(SmallFont);
        Switch2VarBox.SetPosition(102, 3 + 32);
        Switch2VarBox.SetSize(118, 25);
        Switch2VarBox.SetEnabled(false);
        Switch2Label = new Label(ConditionsBox);
        Switch2Label.SetFont(SmallFont);
        Switch2Label.SetPosition(224, 7 + 32);
        Switch2Label.SetText("is ON");
        Switch2Label.SetEnabled(false);
        VarBox = new CheckBox(ConditionsBox);
        VarBox.SetFont(SmallFont);
        VarBox.SetPosition(7, 7 + 32 * 2);
        VarBox.SetText("Switch");
        VarBox.OnCheckChanged += _ =>
        {
            VarPickBox.SetEnabled(VarBox.Checked);
            VarLabel1.SetEnabled(VarBox.Checked);
            VarValueBox.SetEnabled(VarBox.Checked);
            VarLabel2.SetEnabled(VarBox.Checked);
        };
        VarPickBox = new BrowserBox(ConditionsBox);
        VarPickBox.SetFont(SmallFont);
        VarPickBox.SetPosition(102, 3 + 32 * 2);
        VarPickBox.SetSize(118, 25);
        VarPickBox.SetEnabled(false);
        VarLabel1 = new Label(ConditionsBox);
        VarLabel1.SetFont(SmallFont);
        VarLabel1.SetPosition(224, 7 + 32 * 2);
        VarLabel1.SetText("is");
        VarLabel1.SetEnabled(false);
        VarValueBox = new NumericBox(ConditionsBox);
        VarValueBox.SetPosition(102, 3 + 32 * 2 + 27);
        VarValueBox.SetSize(55, 27);
        VarValueBox.SetEnabled(false);
        VarLabel2 = new Label(ConditionsBox);
        VarLabel2.SetFont(SmallFont);
        VarLabel2.SetPosition(160, 7 + 32 * 2 + 27);
        VarLabel2.SetText("or higher");
        VarLabel2.SetEnabled(false);
        SelfSwitchBox = new CheckBox(ConditionsBox);
        SelfSwitchBox.SetFont(SmallFont);
        SelfSwitchBox.SetPosition(7, 5 + 32 * 4);
        SelfSwitchBox.SetText("Self Switch");
        SelfSwitchBox.OnCheckChanged += _ =>
        {
            SelfSwitchVarBox.SetEnabled(SelfSwitchBox.Checked);
            SelfSwitchLabel.SetEnabled(SelfSwitchBox.Checked);
        };
        SelfSwitchVarBox = new DropdownBox(ConditionsBox);
        SelfSwitchVarBox.SetFont(SmallFont);
        SelfSwitchVarBox.SetPosition(102, 1 + 32 * 4);
        SelfSwitchVarBox.SetSize(55, 25);
        SelfSwitchVarBox.SetItems(new List<ListItem>()
        {
            new ListItem("A"), new ListItem("B"), new ListItem("C"),
            new ListItem("D"), new ListItem("E"), new ListItem("F")
        });
        SelfSwitchVarBox.SetEnabled(false);
        SelfSwitchLabel = new Label(ConditionsBox);
        SelfSwitchLabel.SetFont(SmallFont);
        SelfSwitchLabel.SetPosition(160, 5 + 32 * 4);
        SelfSwitchLabel.SetText("is ON");
        SelfSwitchLabel.SetEnabled(false);

        Label GraphicLabel = new Label(this);
        GraphicLabel.SetFont(HeaderFont);
        GraphicLabel.SetPosition(7, 181);
        GraphicLabel.SetText("Graphic");
        GroupBox GraphicBox = new GroupBox(this);
        GraphicBox.SetPosition(7, 201);
        GraphicBox.SetSize(121, 121);

        Label AutoMoveLabel = new Label(this);
        AutoMoveLabel.SetFont(HeaderFont);
        AutoMoveLabel.SetPosition(136, 181);
        AutoMoveLabel.SetText("Auto-Moveroute");
        GroupBox AutoMoveBox = new GroupBox(this);
        AutoMoveBox.SetPosition(136, 201);
        AutoMoveBox.SetSize(132, 121);
        Label AutoMoveTypeLabel = new Label(AutoMoveBox);
        AutoMoveTypeLabel.SetPosition(4, 9);
        AutoMoveTypeLabel.SetFont(TinyFont);
        AutoMoveTypeLabel.SetText("Type:");
        AutoMoveTypeBox = new DropdownBox(AutoMoveBox);
        AutoMoveTypeBox.SetPosition(43, 5);
        AutoMoveTypeBox.SetSize(87, 25);
        AutoMoveTypeBox.SetFont(TinyFont);
        AutoMoveTypeBox.SetItems(new List<ListItem>()
        {
            new ListItem("Fixed"), new ListItem("Random"),
            new ListItem("Approach"), new ListItem("Custom")
        });
        AutoMoveTypeBox.OnSelectionChanged += _ => EditRouteButton.SetEnabled(AutoMoveTypeBox.SelectedIndex == 3);
        EditRouteButton = new Button(AutoMoveBox);
        EditRouteButton.SetPosition(43, 31);
        EditRouteButton.SetSize(92, 29);
        EditRouteButton.SetFont(SmallFont);
        EditRouteButton.SetText("Edit Route");
        EditRouteButton.SetEnabled(false);
        Label AutoMoveSpeedLabel = new Label(AutoMoveBox);
        AutoMoveSpeedLabel.SetPosition(4, 65);
        AutoMoveSpeedLabel.SetFont(TinyFont);
        AutoMoveSpeedLabel.SetText("Speed:");
        AutoMoveSpeedBox = new DropdownBox(AutoMoveBox);
        AutoMoveSpeedBox.SetPosition(43, 61);
        AutoMoveSpeedBox.SetSize(87, 25);
        AutoMoveSpeedBox.SetFont(TinyFont);
        AutoMoveSpeedBox.SetItems(new List<ListItem>()
        {
            new ListItem("1: Slowest"), new ListItem("2: Slower"), new ListItem("3: Slow"),
            new ListItem("4: Fast"), new ListItem("5: Faster"), new ListItem("6: Fastest")
        });
        Label AutoMoveFrequencyLabel = new Label(AutoMoveBox);
        AutoMoveFrequencyLabel.SetPosition(4, 95);
        AutoMoveFrequencyLabel.SetFont(TinyFont);
        AutoMoveFrequencyLabel.SetText("Freq:");
        AutoMoveFrequencyBox = new DropdownBox(AutoMoveBox);
        AutoMoveFrequencyBox.SetPosition(43, 91);
        AutoMoveFrequencyBox.SetSize(87, 25);
        AutoMoveFrequencyBox.SetFont(TinyFont);
        AutoMoveFrequencyBox.SetItems(new List<ListItem>()
        {
            new ListItem("1: Lowest"), new ListItem("2: Lower"), new ListItem("3: Low"),
            new ListItem("4: High"), new ListItem("5: Higher"), new ListItem("6: Highest")
        });

        Label OptionsLabel = new Label(this);
        OptionsLabel.SetFont(HeaderFont);
        OptionsLabel.SetPosition(7, 323);
        OptionsLabel.SetText("Options");
        GroupBox OptionsBox = new GroupBox(this);
        OptionsBox.SetPosition(7, 343);
        OptionsBox.SetSize(119, 106);
        MoveAnimationBox = new CheckBox(OptionsBox);
        MoveAnimationBox.SetPosition(5, 5);
        MoveAnimationBox.SetFont(TinyFont);
        MoveAnimationBox.SetText("Move Animation");
        StopAnimationBox = new CheckBox(OptionsBox);
        StopAnimationBox.SetPosition(5, 25);
        StopAnimationBox.SetFont(TinyFont);
        StopAnimationBox.SetText("Idle Animation");
        DirectionFixBox = new CheckBox(OptionsBox);
        DirectionFixBox.SetPosition(5, 45);
        DirectionFixBox.SetFont(TinyFont);
        DirectionFixBox.SetText("Direction Fix");
        ThroughBox = new CheckBox(OptionsBox);
        ThroughBox.SetPosition(5, 65);
        ThroughBox.SetFont(TinyFont);
        ThroughBox.SetText("Through");
        AlwaysOnTopBox = new CheckBox(OptionsBox);
        AlwaysOnTopBox.SetPosition(5, 85);
        AlwaysOnTopBox.SetFont(TinyFont);
        AlwaysOnTopBox.SetText("Always on Top");

        Label TriggerLabel = new Label(this);
        TriggerLabel.SetFont(HeaderFont);
        TriggerLabel.SetPosition(136, 323);
        TriggerLabel.SetText("Trigger");
        GroupBox TriggerBox = new GroupBox(this);
        TriggerBox.SetPosition(136, 343);
        TriggerBox.SetSize(132, 106);
        ActionBox = new RadioBox(TriggerBox);
        ActionBox.SetPosition(5, 5);
        ActionBox.SetFont(TinyFont);
        ActionBox.SetText("Action Button");
        PlayerBox = new RadioBox(TriggerBox);
        PlayerBox.SetPosition(5, 25);
        PlayerBox.SetFont(TinyFont);
        PlayerBox.SetText("Player Touch");
        EventBox = new RadioBox(TriggerBox);
        EventBox.SetPosition(5, 45);
        EventBox.SetFont(TinyFont);
        EventBox.SetText("Event Touch");
        AutorunBox = new RadioBox(TriggerBox);
        AutorunBox.SetPosition(5, 65);
        AutorunBox.SetFont(TinyFont);
        AutorunBox.SetText("Autorun");
        ParallelBox = new RadioBox(TriggerBox);
        ParallelBox.SetPosition(5, 85);
        ParallelBox.SetFont(TinyFont);
        ParallelBox.SetText("Parallel Process");
    }

    public void SetEventPage(EventPage Page)
    {
        Switch1Box.SetChecked(Page.Condition.Switch1Valid);
        Switch1VarBox.SetText($"[{Utilities.Digits(Page.Condition.Switch1ID, 3)}]: {Data.System.Switches[Page.Condition.Switch1ID]}");
        Switch2Box.SetChecked(Page.Condition.Switch2Valid);
        Switch2VarBox.SetText($"[{Utilities.Digits(Page.Condition.Switch2ID, 3)}]: {Data.System.Switches[Page.Condition.Switch2ID]}");
        VarBox.SetChecked(Page.Condition.VariableValid);
        VarPickBox.SetText($"[{Utilities.Digits(Page.Condition.VariableID, 3)}]: {Data.System.Variables[Page.Condition.VariableID]})");
        VarValueBox.SetValue(Page.Condition.VariableValue);
        SelfSwitchBox.SetChecked(Page.Condition.SelfSwitchValid);
        SelfSwitchVarBox.SetSelectedIndex(Page.Condition.SelfSwitchChar - 'A');
        MoveAnimationBox.SetChecked(Page.Settings.WalkAnime);
        StopAnimationBox.SetChecked(Page.Settings.StepAnime);
        DirectionFixBox.SetChecked(Page.Settings.DirectionFix);
        ThroughBox.SetChecked(Page.Settings.Through);
        AlwaysOnTopBox.SetChecked(Page.Settings.AlwaysOnTop);
        switch (Page.TriggerMode)
        {
            case TriggerMode.Action:
                ActionBox.SetChecked(true);
                break;
            case TriggerMode.PlayerTouch:
                PlayerBox.SetChecked(true);
                break;
            case TriggerMode.EventTouch:
                EventBox.SetChecked(true);
                break;
            case TriggerMode.Autorun:
                AutorunBox.SetChecked(true);
                break;
            case TriggerMode.ParallelProcess:
                ParallelBox.SetChecked(true);
                break;
            default:
                throw new Exception("Invalid trigger mode");
        }
        AutoMoveTypeBox.SetSelectedIndex(Page.MoveRoute.Type);
        AutoMoveSpeedBox.SetSelectedIndex(Page.MoveRoute.Speed - 1);
        AutoMoveFrequencyBox.SetSelectedIndex(Page.MoveRoute.Frequency - 1);
    }
}
