using System;
using System.Collections.Generic;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class NewEventCommandWindow : PopupWindow
{
    public bool Apply = false;
    public List<EventCommand> Commands;

    Map Map;
    Event Event;
    EventPage Page;

    public NewEventCommandWindow(Map Map, Event Event, EventPage Page)
    {
        this.Map = Map;
        this.Event = Event;
        this.Page = Page;

        SetTitle("Event Commands");
        MinimumSize = MaximumSize = new Size(450, 364);
        SetSize(MaximumSize);
        Center();

        SubmodeView CategoryPicker = new SubmodeView(this);
        CategoryPicker.SetPadding(8, 40, 8, 44);
        CategoryPicker.SetDocked(true);
        CategoryPicker.SetFont(Fonts.CabinMedium.Use(11));
        CategoryPicker.SetHeaderHeight(29);
        CategoryPicker.SetHeaderSelBackgroundColor(new Color(59, 91, 124));
        CategoryPicker.SetCentered(true);
        TabContainer General = CategoryPicker.CreateTab("General");
        TabContainer Flow = CategoryPicker.CreateTab("Flow");
        TabContainer Maps = CategoryPicker.CreateTab("Maps");
        TabContainer AudioVisual = CategoryPicker.CreateTab("Audio/visual");
        TabContainer Other = CategoryPicker.CreateTab("Other");

        Widget Divider = new Widget(this);
        Divider.SetHDocked(true);
        Divider.SetPadding(8, 65, 8, 0);
        Divider.SetHeight(4);
        Divider.SetBackgroundColor(new Color(59, 91, 124));

        List<TabContainer> Containers = new List<TabContainer>() { General, Flow, Maps, AudioVisual, Other };

        Containers.ForEach(c =>
        {
            Grid Grid = new Grid(c);
            Grid.SetDocked(true);
            Grid.SetPadding(0, 5, 0, 0);
            Grid.SetColumns(new GridSize(1), new GridSize(1));
            VStackPanel Left = new VStackPanel(Grid);
            VStackPanel Right = new VStackPanel(Grid);
            Right.SetGridColumn(1);
        });

        void Add(string Text, int ContainerIndex, Action<Action<List<EventCommand>>> Clicked)
        {
            Grid grid = (Grid) Containers[ContainerIndex].Widgets[0];
            int idx = grid.Widgets[1].Widgets.Count >= grid.Widgets[0].Widgets.Count ? 0 : 1;
            Button Button = new Button(grid.Widgets[idx]);
            Button.SetFont(Fonts.CabinMedium.Use(11));
            Button.SetMargins(0, 0);
            Button.SetText(Text);
            Button.SetHeight(36);
            Button.OnClicked += _ => Clicked?.Invoke(cmds =>
            {
                this.Commands = cmds;
                OK();
            });
        }

        Add("Show Text", 0, Insert =>
        {
            GenericMultilineTextBoxWindow win = new GenericMultilineTextBoxWindow("Show Text", "", false);
            win.OnClosed += _ =>
            {
                if (!win.Apply) return;
                Insert(CommandWidgets.TextWidget.TextToCommands(CommandCode.ShowText, win.Text));
            };
        });
        Add("Change Text Options", 0, null);
        Add("Show Choices", 0, null);
        Add("Change Windowskin", 0, null);
        Add("Set Move Route", 0, null);
        Add("Input Number", 0, null);
        Add("Wait for Move Completion", 0, null);
        Add("Script", 0, Insert =>
        {
            GenericMultilineTextBoxWindow win = new GenericMultilineTextBoxWindow("Script", "", true);
            win.OnClosed += _ =>
            {
                if (!win.Apply) return;
                Insert(CommandWidgets.TextWidget.TextToCommands(CommandCode.Script, win.Text));
            };
        });
        Add("Change Money", 0, null);
        Add("Comment", 0, Insert =>
        {
            GenericMultilineTextBoxWindow win = new GenericMultilineTextBoxWindow("Comment", "", true);
            win.OnClosed += _ =>
            {
                if (!win.Apply) return;
                Insert(CommandWidgets.TextWidget.TextToCommands(CommandCode.Comment, win.Text));
            };
        });
        Add("Wait", 0, null);

        Add("Set Switch", 1, null);
        Add("Conditional Branch", 1, null);
        Add("Set Variable", 1, null);
        Add("Loop", 1, null);
        Add("Set Self Switch", 1, null);
        Add("Break Loop", 1, null);
        Add("Exit Event Processing", 1, null);
        Add("Set Label", 1, null);
        Add("Erase Event", 1, null);
        Add("Jump to Label", 1, null);
        Add("Call Common Event", 1, null);

        Add("Transfer Player", 2, null);
        Add("Scroll Map", 2, null);
        Add("Transfer Event", 2, null);
        Add("Change Map Settings", 2, null);
        Add("Show Animation", 2, null);
        Add("Change Fog Color", 2, null);
        Add("Set Weather Effects", 2, null);
        Add("Change Fog Opacity", 2, null);
        Add("Change Transparency", 2, null);
        Add("Change Screen Color", 2, null);

        Add("Show Picture", 3, null);
        Add("Play BGM", 3, null);
        Add("Move Picture", 3, null);
        Add("Fade Out BGM", 3, null);
        Add("Rotate Picture", 3, null);
        Add("Play BGS", 3, null);
        Add("Change Picture Color", 3, null);
        Add("Fade Out BGS", 3, null);
        Add("Erase Picture", 3, null);
        Add("Play SE", 3, null);
        Add("Play ME", 3, null);
        Add("Stop SE", 3, null);
        Add("Restore BGM/BGS", 3, null);
        Add("Memorize BGM/BGS", 3, null);

        Add("Show Pause Menu", 4, null);
        Add("Change Pause Access", 4, null);
        Add("Show Save Screen", 4, null);
        Add("Change Save Access", 4, null);
        Add("Show Title Screen", 4, null);
        Add("Prepare for Transition", 4, null);
        Add("Heal Player Party", 4, null);
        Add("Execute Transition", 4, null);
        Add("Trigger Black-out", 4, null);

        CreateButton("Cancel", _ => Cancel());
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
