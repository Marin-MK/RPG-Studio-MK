using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Widgets;

public class ColorPickerWindow : PopupWindow
{
    public bool Apply = false;
    public Color Value;

	ColorPickerWidget ColorPicker;
    NumericBox RBox;
    NumericBox GBox;
    NumericBox BBox;
    NumericBox ABox;

    bool Override = true;

    public ColorPickerWindow(Color Color, bool IncludeTransparency)
	{
		SetTitle("Pick a Color");
		MinimumSize = MaximumSize = new Size(580, 400);
		SetSize(MinimumSize);
		Center();

		ColorPicker = new ColorPickerWidget(this);
        ColorPicker.SetPosition(WindowEdges + 4, WindowEdges + 34);
		ColorPicker.SetSize(320, 336);
        ColorPicker.SetColor(Color);
        ColorPicker.OnColorChanged += _ =>
        {
            if (Override)
            {
                RBox.SetValue(ColorPicker.Color.Red);
                GBox.SetValue(ColorPicker.Color.Green);
                BBox.SetValue(ColorPicker.Color.Blue);
            }
        };

        Label RLabel = new Label(this);
        RLabel.SetFont(Fonts.Paragraph);
        RLabel.SetPosition(370, 48);
        RLabel.SetText("Red:");

        RBox = new NumericBox(this);
        RBox.SetPosition(440, 44);
        RBox.SetWidth(120);
        RBox.SetMinValue(0);
        RBox.SetMaxValue(255);
        RBox.SetValue(Color.Red);
        RBox.OnValueChanged += _ =>
        {
            Override = false;
            ColorPicker.SetColor(new Color((byte) RBox.Value, ColorPicker.Color.Green, ColorPicker.Color.Blue));
            Override = true;
        };

        Label GLabel = new Label(this);
        GLabel.SetFont(Fonts.Paragraph);
        GLabel.SetPosition(370, 80);
        GLabel.SetText("Green:");

        GBox = new NumericBox(this);
        GBox.SetPosition(440, 76);
        GBox.SetWidth(120);
        GBox.SetMinValue(0);
        GBox.SetMaxValue(255);
        GBox.SetValue(Color.Green);
        GBox.OnValueChanged += _ =>
        {
            Override = false;
            ColorPicker.SetColor(new Color(ColorPicker.Color.Red, (byte) GBox.Value, ColorPicker.Color.Blue));
            Override = true;
        };

        Label BLabel = new Label(this);
        BLabel.SetFont(Fonts.Paragraph);
        BLabel.SetPosition(370, 112);
        BLabel.SetText("Blue:");

        BBox = new NumericBox(this);
        BBox.SetPosition(440, 108);
        BBox.SetWidth(120);
        BBox.SetMinValue(0);
        BBox.SetMaxValue(255);
        BBox.SetValue(Color.Blue);
        BBox.OnValueChanged += _ =>
        {
            Override = false;
            ColorPicker.SetColor(new Color(ColorPicker.Color.Red, ColorPicker.Color.Green, (byte) BBox.Value));
            Override = true;
        };

        Label ALabel = new Label(this);
        ALabel.SetFont(Fonts.Paragraph);
        ALabel.SetPosition(370, 144);
        ALabel.SetText("Alpha:");
        ALabel.SetVisible(IncludeTransparency);

        ABox = new NumericBox(this);
        ABox.SetPosition(440, 140);
        ABox.SetWidth(120);
        ABox.SetMinValue(0);
        ABox.SetMaxValue(255);
        ABox.SetValue(Color.Alpha);
        ABox.OnValueChanged += _ =>
        {
            Override = false;
            ColorPicker.SetColor(new Color(ColorPicker.Color.Red, ColorPicker.Color.Green, ColorPicker.Color.Blue, (byte) ABox.Value));
            Override = true;
        };
        ABox.SetVisible(IncludeTransparency);

        CreateButton("Cancel", _ => Cancel());
		CreateButton("OK", _ => OK());
	}

    private void OK()
    {
        Apply = true;
        Value = new Color((byte) RBox.Value, (byte) GBox.Value, (byte) BBox.Value, (byte) ABox.Value);
        Close();
    }

    private void Cancel()
    {
        Close();
    }
}
