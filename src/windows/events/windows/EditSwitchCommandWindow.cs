using System.Collections.Generic;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class EditSwitchCommandWindow : PopupWindow
{
    public bool Apply = false;
    public int SwitchID1;
    public int SwitchID2;
    public bool Batch => SwitchID1 != SwitchID2;
    public bool Value;
    public EventCommand NewCommand;

    RadioBox SingleLabel;
    RadioBox BatchLabel;
    SwitchPickerBox SwitchBox;
    NumericBox Switch1Box;
    Label RangeLabel;
    NumericBox Switch2Box;
    RadioBox OnBox;
    RadioBox OffBox;

    public EditSwitchCommandWindow(EventCommand Command)
    {
        SetTitle("Set Switch");
        MinimumSize = MaximumSize = new Size(300, 190);
        SetSize(MaximumSize);
        Center();

        Container SwitchContainer = new Container(this);
        SwitchContainer.SetPosition(20, 50);
        SwitchContainer.AutoResize = true;

        SingleLabel = new RadioBox(SwitchContainer);
        SingleLabel.SetFont(Fonts.Paragraph);
        SingleLabel.SetText("Single");
        SingleLabel.SetPosition(0, 4);
        SingleLabel.OnChecked += _ => UpdateLabels();

        SwitchBox = new SwitchPickerBox(SwitchContainer);
        SwitchBox.SetFont(Fonts.Paragraph);
        SwitchBox.SetPosition(64, 0);
        SwitchBox.SetSize(200, 25);
        SwitchBox.SetSwitchID((int) (long) Command.Parameters[0]);

        BatchLabel = new RadioBox(SwitchContainer);
        BatchLabel.SetFont(Fonts.Paragraph);
        BatchLabel.SetText("Batch");
        BatchLabel.SetPosition(0, 36);
        BatchLabel.OnChecked += _ => UpdateLabels();

        Switch1Box = new NumericBox(SwitchContainer);
        Switch1Box.SetPosition(64, 31);
        Switch1Box.SetSize(90, 27);
        Switch1Box.SetValue(SwitchBox.SwitchID);
        Switch1Box.SetMinValue(1);

        RangeLabel = new Label(SwitchContainer);
        RangeLabel.SetFont(Fonts.Paragraph);
        RangeLabel.SetText("to");
        RangeLabel.SetPosition(160, 35);

        Switch2Box = new NumericBox(SwitchContainer);
        Switch2Box.SetPosition(174, 31);
        Switch2Box.SetSize(90, 27);
        Switch2Box.SetValue((int) (long) Command.Parameters[1]);
        Switch2Box.SetMinValue(1);

        Container OnOffContainer = new Container(this);
        OnOffContainer.SetPosition(20, 120);
        OnOffContainer.SetSize(160, 16);

        Label SetStateLabel = new Label(OnOffContainer);
        SetStateLabel.SetFont(Fonts.Paragraph);
        SetStateLabel.SetText("Set to");

        OnBox = new RadioBox(OnOffContainer);
        OnBox.SetFont(Fonts.Paragraph);
        OnBox.SetPosition(SetStateLabel.Size.Width + 10, 0);
        OnBox.SetText("ON");
        OnBox.SetChecked((int) (long) Command.Parameters[2] == 0);

        OffBox = new RadioBox(OnOffContainer);
        OffBox.SetFont(Fonts.Paragraph);
        OffBox.SetPosition(OnBox.Position.X + 50, 0);
        OffBox.SetText("OFF");
        OffBox.SetChecked(!OnBox.Checked);

        SingleLabel.SetChecked((long) Command.Parameters[0] == (long) Command.Parameters[1]);
        BatchLabel.SetChecked(!SingleLabel.Checked);

        CreateButton("Cancel", _ => Cancel());
        CreateButton("OK", _ => OK());
    }

    private void UpdateLabels()
    {
        SwitchBox.SetEnabled(SingleLabel.Checked);
        Switch1Box.SetEnabled(BatchLabel.Checked);
        Switch2Box.SetEnabled(BatchLabel.Checked);
        RangeLabel.SetEnabled(BatchLabel.Checked);
    }

    private void OK()
    {
        Apply = true;
        if (BatchLabel.Checked)
        {
            SwitchID1 = Switch2Box.Value > Switch1Box.Value ? Switch1Box.Value : Switch2Box.Value;
            SwitchID2 = Switch2Box.Value > Switch1Box.Value ? Switch2Box.Value : Switch1Box.Value;
        }
        else
        {
            SwitchID1 = SwitchID2 = SwitchBox.SwitchID;
        }
        Value = OnBox.Checked;
        long state = Value ? 0 : 1;
        NewCommand = new EventCommand(CommandCode.ControlSwitches, 0, new List<object>() { (long) SwitchID1, (long) SwitchID2, state });
        Close();
    }

    private void Cancel()
    {
        Close();
    }
}
