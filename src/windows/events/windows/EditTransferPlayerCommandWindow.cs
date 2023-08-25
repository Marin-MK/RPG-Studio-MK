using RPGStudioMK.Game;
using System.Collections.Generic;

namespace RPGStudioMK.Widgets;

public class EditTransferPlayerCommandWindow : PopupWindow
{
    public bool Apply = false;
    int MapID;
    int MapX;
    int MapY;
    public EventCommand NewCommand;

    RadioBox DirectRadioBox;
    BrowserBox DirectBox;
    RadioBox VariableRadioBox;
    Label MapIDLabel;
    VariablePickerBox MapIDBox;
    Label MapXLabel;
    VariablePickerBox MapXBox;
    Label MapYLabel;
    VariablePickerBox MapYBox;
    DropdownBox DirectionBox;
    CheckBox FadeBox;

	public EditTransferPlayerCommandWindow(EventCommand Command)
	{
        SetTitle("Transfer Player");
        MinimumSize = MaximumSize = new Size(300, 340);
        SetSize(MaximumSize);
        Center();

        bool DirectAppointment = (long) Command.Parameters[0] == 0;
        MapID = (int) (long) Command.Parameters[1];
        MapX = (int) (long) Command.Parameters[2];
        MapY = (int) (long) Command.Parameters[3];
        int Direction = (int) (long) Command.Parameters[4];
        bool Fading = (long) Command.Parameters[5] == 0;

        DirectRadioBox = new RadioBox(this);
        DirectRadioBox.SetFont(Fonts.Paragraph);
        DirectRadioBox.SetPosition(20, 40);
        DirectRadioBox.SetText("Direct appointment");
        DirectRadioBox.OnChecked += _ => UpdateStates();

        DirectBox = new BrowserBox(this);
        DirectBox.SetFont(Fonts.Paragraph);
        DirectBox.SetPosition(40, 60);
        DirectBox.SetWidth(200);
        DirectBox.OnDropDownClicked += _ =>
        {
            Map StartMap = Data.Maps[MapID];
            Point StartLocation = new Point(MapX, MapY);
            MapAndLocationPicker win = new MapAndLocationPicker("Location Picker", StartMap, StartLocation);
            win.OnClosed += _ =>
            {
                if (!win.Apply) return;
                MapID = win.MapID;
                MapX = win.Location.X;
                MapY = win.Location.Y;
                UpdateDirectText();
            };
        };

        VariableRadioBox = new RadioBox(this);
        VariableRadioBox.SetFont(Fonts.Paragraph);
        VariableRadioBox.SetPosition(20, 100);
        VariableRadioBox.SetText("Appoint with variables");
        VariableRadioBox.OnChecked += _ => UpdateStates();

        MapIDLabel = new Label(this);
        MapIDLabel.SetFont(Fonts.Paragraph);
        MapIDLabel.SetText("Map ID:");
        MapIDLabel.SetPosition(40, 124);

        MapIDBox = new VariablePickerBox(this);
        MapIDBox.SetFont(Fonts.Paragraph);
        MapIDBox.SetPosition(100, 120);
        MapIDBox.SetWidth(180);
        if (!DirectAppointment) MapIDBox.SetVariableID(MapID);

        MapXLabel = new Label(this);
        MapXLabel.SetFont(Fonts.Paragraph);
        MapXLabel.SetText("Map X:");
        MapXLabel.SetPosition(40, 156);

        MapXBox = new VariablePickerBox(this);
        MapXBox.SetFont(Fonts.Paragraph);
        MapXBox.SetPosition(100, 152);
        MapXBox.SetWidth(180);
        if (!DirectAppointment) MapXBox.SetVariableID(MapX);

        MapYLabel = new Label(this);
        MapYLabel.SetFont(Fonts.Paragraph);
        MapYLabel.SetText("Map Y:");
        MapYLabel.SetPosition(40, 188);

        MapYBox = new VariablePickerBox(this);
        MapYBox.SetFont(Fonts.Paragraph);
        MapYBox.SetPosition(100, 184);
        MapYBox.SetWidth(180);
        if (!DirectAppointment) MapYBox.SetVariableID(MapY);

        Label DirectionLabel = new Label(this);
        DirectionLabel.SetFont(Fonts.Paragraph);
        DirectionLabel.SetText("Direction:");
        DirectionLabel.SetPosition(20, 236);

        DirectionBox = new DropdownBox(this);
        DirectionBox.SetFont(Fonts.Paragraph);
        DirectionBox.SetItems(new List<ListItem>()
        {
            new ListItem("Retain"),
            new ListItem("Down"),
            new ListItem("Left"),
            new ListItem("Right"),
            new ListItem("Up")
        });
        DirectionBox.SetWidth(90);
        DirectionBox.SetPosition(80, 232);
        DirectionBox.SetSelectedIndex(Direction == 0 ? 0 : Direction / 2);

        FadeBox = new CheckBox(this);
        FadeBox.SetMirrored(true);
        FadeBox.SetFont(Fonts.Paragraph);
        FadeBox.SetText("With Fade:");
        FadeBox.SetPosition(20, 265);
        FadeBox.SetChecked(Fading);

        CreateButton("Cancel", _ => Cancel());
        CreateButton("OK", _ => OK());

        DirectRadioBox.SetChecked(DirectAppointment);
        VariableRadioBox.SetChecked(!DirectRadioBox.Checked);

        UpdateStates();
        UpdateDirectText();
    }

    private void UpdateStates()
    {
        DirectBox.SetEnabled(DirectRadioBox.Checked);
        MapIDLabel.SetEnabled(VariableRadioBox.Checked);
        MapIDBox.SetEnabled(VariableRadioBox.Checked);
        MapXLabel.SetEnabled(VariableRadioBox.Checked);
        MapXBox.SetEnabled(VariableRadioBox.Checked);
        MapYLabel.SetEnabled(VariableRadioBox.Checked);
        MapYBox.SetEnabled(VariableRadioBox.Checked);
        if (DirectRadioBox.Checked)
        {
            MapIDBox.SetVariableID(1);
            MapXBox.SetVariableID(1);
            MapYBox.SetVariableID(1);
        }
    }

    private void UpdateDirectText()
    {
        DirectBox.SetText($"{Utilities.Digits(MapID, 3)}: {(Data.Maps.ContainsKey(MapID) ? Data.Maps[MapID] : "")} ({MapX}, {MapY})");
    }

    private void OK()
    {
        Apply = true;
        List<object> Params = new List<object>();
        if (DirectRadioBox.Checked)
        {
            Params.Add(0L);
            Params.Add((long) MapID);
            Params.Add((long) MapX);
            Params.Add((long) MapY);
        }
        else
        {
            Params.Add(1L);
            Params.Add((long) MapIDBox.VariableID);
            Params.Add((long) MapXBox.VariableID);
            Params.Add((long) MapYBox.VariableID);
        }
        if (DirectionBox.SelectedIndex == 0) Params.Add(0L);
        else Params.Add((long) DirectionBox.SelectedIndex * 2);
        Params.Add((long) (FadeBox.Checked ? 0 : 1));
        NewCommand = new EventCommand(CommandCode.TransferPlayer, 0, Params);
        Close();
    }

    private void Cancel()
    {
        Close();
    }
}
