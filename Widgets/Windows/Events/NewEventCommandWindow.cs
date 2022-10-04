using System;
using System.Collections.Generic;
using RPGStudioMK.Game;
using static System.Net.Mime.MediaTypeNames;

namespace RPGStudioMK.Widgets;

public class NewEventCommandWindow : PopupWindow
{
    public bool Apply = false;
    public List<EventCommand> Commands;

    List<(CommandCode, string, int, Action<Action<List<EventCommand>>>)> AllCommands =
            new List<(CommandCode, string, int, Action<Action<List<EventCommand>>>)>();

    Map Map;
    Event Event;
    EventPage Page;

    VStackPanel Left;
    VStackPanel Right;

    public NewEventCommandWindow(Map Map, Event Event, EventPage Page)
    {
        this.Map = Map;
        this.Event = Event;
        this.Page = Page;

        SetTitle("Event Commands");
        MinimumSize = MaximumSize = new Size(506, 506);
        SetSize(MaximumSize);
        Center();

        Container CategoryButtonContainer = new Container(this);
        CategoryButtonContainer.SetHDocked(true);
        CategoryButtonContainer.SetPadding(8, 43, 8, 0);
        CategoryButtonContainer.SetHeight(74);
        Grid CategoryButtonGrid = new Grid(CategoryButtonContainer);
        CategoryButtonGrid.SetColumns(
            new GridSize(1),
            new GridSize(3, Unit.Pixels),
            new GridSize(1),
            new GridSize(3, Unit.Pixels),
            new GridSize(1),
            new GridSize(3, Unit.Pixels),
            new GridSize(1),
            new GridSize(3, Unit.Pixels),
            new GridSize(1)
        );

        EventCategoryButton GeneralButton = new EventCategoryButton(CommandCategory.General, CategoryButtonGrid);
        GeneralButton.SetGridColumn(0);
        GeneralButton.OnSelected += _ => ShowList(0);
        EventCategoryButton FlowButton = new EventCategoryButton(CommandCategory.Flow, CategoryButtonGrid);
        FlowButton.SetGridColumn(2);
        FlowButton.OnSelected += _ => ShowList(1);
        EventCategoryButton MapButton = new EventCategoryButton(CommandCategory.Map, CategoryButtonGrid);
        MapButton.SetGridColumn(4);
        MapButton.OnSelected += _ => ShowList(2);
        EventCategoryButton ImageSoundButton = new EventCategoryButton(CommandCategory.ImageSound, CategoryButtonGrid);
        ImageSoundButton.SetGridColumn(6);
        ImageSoundButton.OnSelected += _ => ShowList(3);
        EventCategoryButton OtherButton = new EventCategoryButton(CommandCategory.Other, CategoryButtonGrid);
        OtherButton.SetGridColumn(8);
        OtherButton.OnSelected += _ => ShowList(4);

        Grid MainGrid = new Grid(this);
        MainGrid.SetDocked(true);
        MainGrid.SetPadding(17, 130, 17, 50);
        MainGrid.SetColumns(new GridSize(1), new GridSize(1));
        Left = new VStackPanel(MainGrid);
        Right = new VStackPanel(MainGrid);
        Right.SetGridColumn(1);

        void Add(CommandCode Code, string Text, int ContainerIndex, Action<Action<List<EventCommand>>> Clicked)
        {
            AllCommands.Add((Code, Text, ContainerIndex, Clicked));
        }

        Add(CommandCode.ShowText, "Show Text", 0, Insert =>
        {
            GenericMultilineTextBoxWindow win = new GenericMultilineTextBoxWindow("Show Text", "", false);
            win.OnClosed += _ =>
            {
                if (!win.Apply) return;
                Insert(CommandWidgets.TextWidget.TextToCommands(CommandCode.ShowText, win.Text));
            };
        });
        Add(CommandCode.ChangeTextOptions, "Change Text Options", 0, null);
        Add(CommandCode.ShowChoices, "Show Choices", 0, null);
        Add(CommandCode.ChangeWindowskin, "Change Windowskin", 0, null);
        Add(CommandCode.SetMoveRoute, "Set Move Route", 0, Insert =>
        {
            MoveRoute mr = new MoveRoute();
            EditMoveRouteWindow win = new EditMoveRouteWindow(Map, Event, Page, mr, -1, false);
            win.OnClosed += _ =>
            {
                if (!win.Apply) return;
                Insert(win.NewCommands);
            };
        });
        Add(CommandCode.InputNumber, "Input Number", 0, null);
        Add(CommandCode.WaitForMoveCompletion, "Wait for Move Completion", 0, null);
        Add(CommandCode.Script, "Script", 0, Insert =>
        {
            GenericMultilineTextBoxWindow win = new GenericMultilineTextBoxWindow("Script", "", true);
            win.OnClosed += _ =>
            {
                if (!win.Apply) return;
                Insert(CommandWidgets.TextWidget.TextToCommands(CommandCode.Script, win.Text));
            };
        });
        Add(CommandCode.ChangeMoney, "Change Money", 0, null);
        Add(CommandCode.Comment, "Comment", 0, Insert =>
        {
            GenericMultilineTextBoxWindow win = new GenericMultilineTextBoxWindow("Comment", "", true);
            win.OnClosed += _ =>
            {
                if (!win.Apply) return;
                Insert(CommandWidgets.TextWidget.TextToCommands(CommandCode.Comment, win.Text));
            };
        });
        Add(CommandCode.Wait, "Wait", 0, Insert =>
        {
            GenericNumberPicker win = new GenericNumberPicker("Wait", "Frames", 4, 1, null);
            win.OnClosed += _ =>
            {
                if (!win.Apply) return;
                Insert(new List<EventCommand>() { new EventCommand(CommandCode.Wait, 0, new List<object>() { (long) win.Value }) });
            };
        });

        Add(CommandCode.ControlSwitches, "Set Switch", 1, Insert =>
        {
            EventCommand cmd = new EventCommand(CommandCode.ControlSwitches, 0, new List<object>() { 1L, 1L, 0L });
            EditSwitchCommandWindow win = new EditSwitchCommandWindow(cmd);
            win.OnClosed += _ =>
            {
                if (!win.Apply) return;
                Insert(new List<EventCommand>() { win.NewCommand });
            };
        });
        Add(CommandCode.ConditionalBranch, "Conditional Branch", 1, null);
        Add(CommandCode.ControlVariables, "Set Variable", 1, null);
        Add(CommandCode.Loop, "Loop", 1, null);
        Add(CommandCode.ControlSelfSwitch, "Set Self Switch", 1, Insert =>
        {
            EventCommand cmd = new EventCommand(CommandCode.ControlSelfSwitch, 0, new List<object>() { "A", 0L, -1L });
            EditSelfSwitchCommandWindow win = new EditSelfSwitchCommandWindow(Map, Event, cmd);
            win.OnClosed += _ =>
            {
                if (!win.Apply) return;
                Insert(new List<EventCommand>() { win.NewCommand });
            };
        });
        Add(CommandCode.BreakLoop, "Break Loop", 1, null);
        Add(CommandCode.ExitEventProcessing, "Exit Event Processing", 1, null);
        Add(CommandCode.Label, "Set Label", 1, null);
        Add(CommandCode.EraseEvent, "Erase Event", 1, null);
        Add(CommandCode.JumpToLabel, "Jump to Label", 1, null);
        Add(CommandCode.CallCommonEvent, "Call Common Event", 1, null);

        Add(CommandCode.TransferPlayer, "Transfer Player", 2, null);
        Add(CommandCode.ScrollMap, "Scroll Map", 2, null);
        Add(CommandCode.SetEventLocation, "Transfer Event", 2, null);
        Add(CommandCode.ChangeMapSettings, "Change Map Settings", 2, null);
        Add(CommandCode.ShowAnimation, "Show Animation", 2, null);
        Add(CommandCode.ChangeFogColorTone, "Change Fog Color", 2, null);
        Add(CommandCode.SetWeatherEffects, "Set Weather Effects", 2, null);
        Add(CommandCode.ChangeFogOpacity, "Change Fog Opacity", 2, null);
        Add(CommandCode.ChangeTransparencyFlag, "Change Transparency", 2, null);
        Add(CommandCode.ChangeScreenColorTone, "Change Screen Color", 2, null);

        Add(CommandCode.ShowPicture, "Show Picture", 3, null);
        Add(CommandCode.PlayBGM, "Play BGM", 3, null);
        Add(CommandCode.MovePicture, "Move Picture", 3, null);
        Add(CommandCode.FadeOutBGM, "Fade Out BGM", 3, null);
        Add(CommandCode.RotatePicture, "Rotate Picture", 3, null);
        Add(CommandCode.PlayBGS, "Play BGS", 3, null);
        Add(CommandCode.ChangePictureColorTone, "Change Picture Color", 3, null);
        Add(CommandCode.FadeOutBGS, "Fade Out BGS", 3, null);
        Add(CommandCode.ErasePicture, "Erase Picture", 3, null);
        Add(CommandCode.PlaySE, "Play SE", 3, null);
        Add(CommandCode.PlayME, "Play ME", 3, null);
        Add(CommandCode.StopSE, "Stop SE", 3, null);
        Add(CommandCode.RestoreBGMBGS, "Restore BGM/BGS", 3, null);
        Add(CommandCode.MemorizeBGMBGS, "Memorize BGM/BGS", 3, null);

        Add(CommandCode.CallMenuScreen, "Show Pause Menu", 4, null);
        Add(CommandCode.ChangeMenuAccess, "Change Pause Access", 4, null);
        Add(CommandCode.CallSaveScreen, "Show Save Screen", 4, null);
        Add(CommandCode.ChangeSaveAccess, "Change Save Access", 4, null);
        Add(CommandCode.ReturnToTitleScreen, "Show Title Screen", 4, null);
        Add(CommandCode.PrepareForTransition, "Prepare for Transition", 4, null);
        Add(CommandCode.HealAll, "Heal Player Party", 4, null);
        Add(CommandCode.ExecuteTransition, "Execute Transition", 4, null);
        Add(CommandCode.GameOver, "Trigger Black-out", 4, null);

        CreateButton("Cancel", _ => Cancel());

        GeneralButton.SetSelected(true);
    }

    private void ShowList(int ContainerIndex)
    {
        while (Left.Widgets.Count > 0) Left.Widgets[0].Dispose();
        while (Right.Widgets.Count > 0) Right.Widgets[0].Dispose();
        foreach ((CommandCode Code, string Text, int cIdx, Action<Action<List<EventCommand>>> Clicked) item in AllCommands)
        {
            if (item.cIdx != ContainerIndex) continue;
            VStackPanel Parent = Right.Widgets.Count >= Left.Widgets.Count ? Left : Right;
            Button Button = new Button(Parent);
            Button.SetLeftAlign(true);
            Button.SetTextX(28);
            Button.SetText(item.Text);
            Button.SetHeight(42);
            Button.OnClicked += _ => item.Clicked?.Invoke(cmds =>
            {
                this.Commands = cmds;
                OK();
            });
            EventCommandIcon Icon = new EventCommandIcon(Button);
            Icon.SetEventCommand(item.Code);
            Icon.SetPosition(10, 8);
            Icon.SetColor(new Color(212, 212, 75));
        }
    }

    private void OK()
    {
        Apply = true;
        Close();
    }

    private void Cancel()
    {
        Commands = null;
        Close();
    }
}
