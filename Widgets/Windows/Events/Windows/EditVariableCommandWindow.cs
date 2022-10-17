using System;
using System.Collections.Generic;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class EditVariableCommandWindow : PopupWindow
{
    public bool Apply = false;
    public int VariableID1;
    public int VariableID2;
    public bool Batch => VariableID1 != VariableID2;
    public EventCommand NewCommand;

    RadioBox SetOperator;
    RadioBox AddOperator;
    RadioBox SubOperator;
    RadioBox MulOperator;
    RadioBox DivOperator;
    RadioBox ModOperator;

    RadioBox SingleLabel;
    RadioBox BatchLabel;
    VariablePickerBox VariableBox;
    NumericBox Variable1Box;
    Label RangeLabel;
    NumericBox Variable2Box;
    RadioBox ConstantRadioBox;
    NumericBox ConstantBox;
    RadioBox VariableRadioBox;
    VariablePickerBox VariableValueBox;
    RadioBox RandomRadioBox;
    NumericBox RandomBox1;
    Label RandomLabel;
    NumericBox RandomBox2;
    RadioBox CharacterRadioBox;
    DropdownBox CharacterBox;
    DropdownBox CharacterValueBox;
    RadioBox OtherRadioBox;
    DropdownBox OtherBox;

    public EditVariableCommandWindow(Map Map, EventCommand Command)
    {
        SetTitle("Set Variable");
        MinimumSize = MaximumSize = new Size(400, 450);
        SetSize(MaximumSize);
        Center();

        Font Header = Fonts.UbuntuBold.Use(10);
        Font Small = Fonts.CabinMedium.Use(9);

        Label VariableLabel = new Label(this);
        VariableLabel.SetFont(Header);
        VariableLabel.SetPosition(20, 40);
        VariableLabel.SetText("Variable");

        Container VariableContainer = new Container(this);
        VariableContainer.SetPosition(40, 60);
        VariableContainer.AutoResize = true;

        SingleLabel = new RadioBox(VariableContainer);
        SingleLabel.SetFont(Small);
        SingleLabel.SetText("Single");
        SingleLabel.SetPosition(0, 4);
        SingleLabel.SetChecked((long) Command.Parameters[0] == (long) Command.Parameters[1]);
        SingleLabel.OnChecked += _ => UpdateVariableLabels();

        VariableBox = new VariablePickerBox(VariableContainer);
        VariableBox.SetFont(Small);
        VariableBox.SetPosition(64, 0);
        VariableBox.SetSize(200, 25);
        VariableBox.SetVariableID((int) (long) Command.Parameters[0]);

        BatchLabel = new RadioBox(VariableContainer);
        BatchLabel.SetFont(Small);
        BatchLabel.SetText("Batch");
        BatchLabel.SetPosition(0, 36);
        BatchLabel.SetChecked(!SingleLabel.Checked);
        BatchLabel.OnChecked += _ => UpdateVariableLabels();

        Variable1Box = new NumericBox(VariableContainer);
        Variable1Box.SetPosition(64, 31);
        Variable1Box.SetSize(90, 27);
        Variable1Box.SetValue(VariableBox.VariableID);
        Variable1Box.SetMinValue(1);

        RangeLabel = new Label(VariableContainer);
        RangeLabel.SetFont(Small);
        RangeLabel.SetText("to");
        RangeLabel.SetPosition(160, 35);

        Variable2Box = new NumericBox(VariableContainer);
        Variable2Box.SetPosition(174, 31);
        Variable2Box.SetSize(90, 27);
        Variable2Box.SetValue((int) (long) Command.Parameters[1]);
        Variable2Box.SetMinValue(1);

        Label OperatorLabel = new Label(this);
        OperatorLabel.SetFont(Header);
        OperatorLabel.SetPosition(20, 140);
        OperatorLabel.SetText("Operator");

        Container OperatorContainer = new Container(this);
        OperatorContainer.SetPosition(40, 164);
        OperatorContainer.SetSize(295, 32);

        Grid OperatorGrid = new Grid(OperatorContainer);
        OperatorGrid.SetColumns(
            new GridSize(1), new GridSize(1), new GridSize(1), new GridSize(1), new GridSize(1), new GridSize(1)
        );

        int Optr = (int) (long) Command.Parameters[2];

        SetOperator = new RadioBox(OperatorGrid);
        SetOperator.SetFont(Small);
        SetOperator.SetText("Set");
        SetOperator.SetChecked(Optr == 0);

        AddOperator = new RadioBox(OperatorGrid);
        AddOperator.SetFont(Small);
        AddOperator.SetText("Add");
        AddOperator.SetGridColumn(1);
        AddOperator.SetChecked(Optr == 1);

        SubOperator = new RadioBox(OperatorGrid);
        SubOperator.SetFont(Small);
        SubOperator.SetText("Sub");
        SubOperator.SetGridColumn(2);
        SubOperator.SetChecked(Optr == 2);

        MulOperator = new RadioBox(OperatorGrid);
        MulOperator.SetFont(Small);
        MulOperator.SetText("Mul");
        MulOperator.SetGridColumn(3);
        MulOperator.SetChecked(Optr == 3);

        DivOperator = new RadioBox(OperatorGrid);
        DivOperator.SetFont(Small);
        DivOperator.SetText("Div");
        DivOperator.SetGridColumn(4);
        DivOperator.SetChecked(Optr == 4);

        ModOperator = new RadioBox(OperatorGrid);
        ModOperator.SetFont(Small);
        ModOperator.SetText("Mod");
        ModOperator.SetGridColumn(5);
        ModOperator.SetChecked(Optr == 5);

        int Opnd = (int) (long) Command.Parameters[3];
        int Val = (int) (long) Command.Parameters[4];
        if (Opnd == 3 || Opnd == 4 || Opnd == 5) Val = 0;

        Label ValueLabel = new Label(this);
        ValueLabel.SetFont(Header);
        ValueLabel.SetPosition(20, 216);
        ValueLabel.SetText("Value");

        Container ValueContainer = new Container(this);
        ValueContainer.SetPosition(40, 236);
        ValueContainer.SetSize(345, 300);

        ConstantRadioBox = new RadioBox(ValueContainer);
        ConstantRadioBox.SetFont(Small);
        ConstantRadioBox.SetPosition(0, 8);
        ConstantRadioBox.SetText("Constant");
        ConstantRadioBox.OnChecked += _ => UpdateValues();

        ConstantBox = new NumericBox(ValueContainer);
        ConstantBox.SetPosition(84, 0);
        ConstantBox.SetSize(100, 30);

        VariableRadioBox = new RadioBox(ValueContainer);
        VariableRadioBox.SetFont(Small);
        VariableRadioBox.SetPosition(0, 40);
        VariableRadioBox.SetText("Variable");
        VariableRadioBox.OnChecked += _ => UpdateValues();

        VariableValueBox = new VariablePickerBox(ValueContainer);
        VariableValueBox.SetPosition(84, 36);
        VariableValueBox.SetSize(150, 24);

        RandomRadioBox = new RadioBox(ValueContainer);
        RandomRadioBox.SetFont(Small);
        RandomRadioBox.SetPosition(0, 72);
        RandomRadioBox.SetText("Random");
        RandomRadioBox.OnChecked += _ => UpdateValues();

        RandomBox1 = new NumericBox(ValueContainer);
        RandomBox1.SetPosition(84, 64);
        RandomBox1.SetSize(90, 30);

        RandomLabel = new Label(ValueContainer);
        RandomLabel.SetFont(Small);
        RandomLabel.SetPosition(176, 70);
        RandomLabel.SetText("to");

        RandomBox2 = new NumericBox(ValueContainer);
        RandomBox2.SetPosition(190, 64);
        RandomBox2.SetSize(90, 30);

        CharacterRadioBox = new RadioBox(ValueContainer);
        CharacterRadioBox.SetFont(Small);
        CharacterRadioBox.SetPosition(0, 104);
        CharacterRadioBox.SetText("Character");
        CharacterRadioBox.OnChecked += _ => UpdateValues();

        CharacterBox = new DropdownBox(ValueContainer);
        CharacterBox.SetPosition(84, 100);
        CharacterBox.SetSize(125, 24);
        List<ListItem> Items = new List<ListItem>();
        Items.Add(new ListItem("Player", -1));
        Items.Add(new ListItem("This Event", 0));
        foreach (KeyValuePair<int, Event> kvp in Map.Events)
        {
            int id = kvp.Key;
            Event e = kvp.Value;
            Items.Add(new ListItem(Utilities.Digits(id, 3) + ": " + e.Name, id));
        }
        CharacterBox.SetItems(Items);

        CharacterValueBox = new DropdownBox(ValueContainer);
        CharacterValueBox.SetPosition(215, 100);
        CharacterValueBox.SetSize(125, 24);
        CharacterValueBox.SetItems(new List<ListItem>()
        {
            new ListItem("Map X"),
            new ListItem("Map Y"),
            new ListItem("Direction"),
            new ListItem("Screen X"),
            new ListItem("Screen Y"),
            new ListItem("Terrain Tag")
        });

        OtherRadioBox = new RadioBox(ValueContainer);
        OtherRadioBox.SetFont(Small);
        OtherRadioBox.SetPosition(0, 136);
        OtherRadioBox.SetText("Other");
        OtherRadioBox.OnChecked += _ => UpdateValues();

        OtherBox = new DropdownBox(ValueContainer);
        OtherBox.SetPosition(84, 132);
        OtherBox.SetSize(150, 24);
        OtherBox.SetItems(new List<ListItem>()
        {
            new ListItem("Map ID"),
            new ListItem("Party size"),
            new ListItem("Money"),
            new ListItem("Play time"),
            new ListItem("Timer"),
            new ListItem("Save count")
        });

        CreateButton("Cancel", _ => Cancel());
        CreateButton("OK", _ => OK());

        if (Opnd == 0)
        {
            ConstantRadioBox.SetChecked(true);
            ConstantBox.SetValue(Val);
        }
        else if (Opnd == 1)
        {
            VariableRadioBox.SetChecked(true);
            VariableValueBox.SetVariableID(Val);
        }
        else if (Opnd == 2)
        {
            RandomRadioBox.SetChecked(true);
            RandomBox1.SetValue(Val);
            int Val2 = (int) (long) Command.Parameters[5];
            RandomBox2.SetValue(Val2);
        }
        else if (Opnd == 6)
        {
            CharacterRadioBox.SetChecked(true);
            ListItem item = CharacterBox.Items.Find(i => (int) i.Object == Val);
            CharacterBox.SetSelectedIndex(CharacterBox.Items.IndexOf(item));
            int Val2 = (int) (long) Command.Parameters[5];
            CharacterValueBox.SetSelectedIndex(Val2);
        }
        else if (Opnd == 7)
        {
            OtherRadioBox.SetChecked(true);
            if (Val == 3) Val = 0;
            else if (Val > 3) Val--;
            OtherBox.SetSelectedIndex(Val);
        }
        if (Opnd != 1) VariableValueBox.SetVariableID(1);

        UpdateVariableLabels();
    }

    private void UpdateVariableLabels()
    {
        VariableBox.SetEnabled(SingleLabel.Checked);
        Variable1Box.SetEnabled(BatchLabel.Checked);
        Variable2Box.SetEnabled(BatchLabel.Checked);
        RangeLabel.SetEnabled(BatchLabel.Checked);
    }
    
    private void UpdateValues()
    {
        ConstantBox.SetEnabled(ConstantRadioBox.Checked);
        VariableValueBox.SetEnabled(VariableRadioBox.Checked);
        RandomBox1.SetEnabled(RandomRadioBox.Checked);
        RandomLabel.SetEnabled(RandomBox1.Enabled);
        RandomBox2.SetEnabled(RandomBox1.Enabled);
        CharacterBox.SetEnabled(CharacterRadioBox.Checked);
        CharacterValueBox.SetEnabled(CharacterBox.Enabled);
        OtherBox.SetEnabled(OtherRadioBox.Checked);
    }

    private void OK()
    {
        Apply = true;
        if (BatchLabel.Checked)
        {
            VariableID1 = Variable2Box.Value > Variable1Box.Value ? Variable1Box.Value : Variable2Box.Value;
            VariableID2 = Variable2Box.Value > Variable1Box.Value ? Variable2Box.Value : Variable1Box.Value;
        }
        else
        {
            VariableID1 = VariableID2 = VariableBox.VariableID;
        }
        List<object> Params = new List<object>();
        Params.Add((long) VariableID1);
        Params.Add((long) VariableID2);
        int Optr = 0;
        if (SetOperator.Checked) Optr = 0;
        else if (AddOperator.Checked) Optr = 1;
        else if (SubOperator.Checked) Optr = 2;
        else if (MulOperator.Checked) Optr = 3;
        else if (DivOperator.Checked) Optr = 4;
        else if (ModOperator.Checked) Optr = 5;
        Params.Add((long) Optr);
        int Opnd = 0;
        int Val1 = 0;
        int Val2 = 0;
        if (ConstantRadioBox.Checked)
        {
            Opnd = 0;
            Val1 = ConstantBox.Value;
        }
        else if (VariableRadioBox.Checked)
        {
            Opnd = 1;
            Val1 = VariableValueBox.VariableID;
            Val2 = Val1;
        }
        else if (RandomRadioBox.Checked)
        {
            Opnd = 2;
            Val1 = Math.Min(RandomBox1.Value, RandomBox2.Value);
            Val2 = Math.Max(RandomBox1.Value, RandomBox2.Value);
        }
        else if (CharacterRadioBox.Checked)
        {
            Opnd = 6;
            Val1 = (int) CharacterBox.Items[CharacterBox.SelectedIndex].Object;
            Val2 = CharacterValueBox.SelectedIndex;
        }
        else if (OtherRadioBox.Checked)
        {
            Opnd = 7;
            Val1 = OtherBox.SelectedIndex;
            if (Val1 >= 3) Val1++; // We do not have the "Steps" option
        }
        Params.Add((long) Opnd);
        Params.Add((long) Val1);
        Params.Add((long) Val2);
        NewCommand = new EventCommand(CommandCode.ControlVariables, 0, Params);
        Close();
    }

    private void Cancel()
    {
        Close();
    }
}
