using System;

namespace RPGStudioMK.Widgets;

public class FloatNumericBox : Widget
{
    public float Value { get; protected set; } = 0;
    public float MaxValue { get; protected set; } = 999999;
    public float MinValue { get; protected set; } = -999999;
    public float Increment { get; protected set; } = 0.1f;
    public bool Enabled { get; protected set; } = true;
	public bool ShowDisabledText { get; protected set; } = false;
    public int SignificantBits { get; protected set; } = 3;

	public BaseEvent OnValueChanged;
    public BaseEvent OnEnterPressed { get => TextArea.OnEnterPressed; set => TextArea.OnEnterPressed = value; }

    Button DownButton;
    Button UpButton;
    Container TextBG;
    TextArea TextArea;

    public FloatNumericBox(IContainer Parent) : base(Parent)
    {
        DownButton = new Button(this);
        DownButton.SetText("-");
        DownButton.SetSize(30, 30);
        DownButton.SetRepeatable(true);
        DownButton.OnClicked += _ =>
        {
            SetValue(Value - Increment);
        };

        UpButton = new Button(this);
        UpButton.SetText("+");
        UpButton.SetSize(30, 30);
        UpButton.SetRightDocked(true);
        UpButton.SetRepeatable(true);
        UpButton.OnClicked += _ =>
        {
            SetValue(Value + Increment);
        };

        TextBG = new Container(this);
        TextBG.SetDocked(true);
        TextBG.SetPadding(28, 1);
        TextBG.Sprites["box"] = new Sprite(TextBG.Viewport);
        TextBG.OnHoverChanged += _ => Redraw();

        TextArea = new TextArea(TextBG);
        TextArea.SetDocked(true);
        TextArea.SetPadding(1, 4);
        TextArea.SetFont(Fonts.Paragraph);
        TextArea.SetNumericOnly(true);
        TextArea.SetAllowFloats(true);
        TextArea.SetDefaultNumericValue(0);
        TextArea.OnWidgetSelected += _ => Redraw();
        TextArea.OnWidgetDeselected += _ =>
        {
            float? f = TextToFloat(TextArea.Text);
            if (f is not null) SetValue((float) f);
            else SetValue(0);
            Redraw();
        };
        TextArea.OnTextChanged += _ => RepositionText();
        TextArea.OnPressingUp += _ =>
        {
            SetValue(Value + Increment);
        };
        TextArea.OnPressingDown += _ =>
        {
            SetValue(Value - Increment);
        };
        TextArea.SetText(Math.Round(this.Value, this.SignificantBits).ToString());

        TextBG.OnSizeChanged += _ => RepositionText();

        MinimumSize.Height = MaximumSize.Height = 30;
        SetSize(66, 30);
    }

    void RepositionText()
    {
        Size s = TextArea.Font.TextSize(TextArea.Text);
        if (s.Width >= TextBG.Size.Width) TextArea.SetTextX(0);
        else TextArea.SetTextX(TextBG.Size.Width / 2 - s.Width / 2);
    }

    float? TextToFloat(string text)
    {
        text = text.Replace(',', '.');
        float result;
        if (!float.TryParse(text, out result)) return null;
        return result;
    }

    public void SetEnabled(bool Enabled)
    {
        if (this.Enabled != Enabled)
        {
            this.Enabled = Enabled;
            TextArea.SetEnabled(this.Enabled);
            DownButton.SetEnabled(this.Enabled);
            UpButton.SetEnabled(this.Enabled);
            this.Redraw();
        }
    }

    public void SetValue(float Value, bool Force = false)
    {
        if (Value > MaxValue) Value = MaxValue;
        if (Value < MinValue) Value = MinValue;
        if (Math.Abs(this.Value - Value) > float.Epsilon || Force)
        {
            this.Value = Value;
            this.OnValueChanged?.Invoke(new BaseEventArgs());
            TextArea.SetText(Math.Round(this.Value, this.SignificantBits).ToString());
            RepositionText();
        }
    }

    public void SetMinValue(int MinValue)
    {
        if (this.MinValue != MinValue)
        {
            this.MinValue = MinValue;
            if (this.Value < this.MinValue)
            {
                SetValue(this.MinValue);
            }
            TextArea.SetAllowMinusSigns(MinValue < 0);
        }
    }

    public void SetMaxValue(int MaxValue)
    {
        if (this.MaxValue != MaxValue)
        {
            this.MaxValue = MaxValue;
            if (this.Value > this.MaxValue)
            {
                SetValue(this.MaxValue);
            }
        }
    }

    public void SetIncrement(int Increment)
    {
        if (this.Increment != Increment)
        {
            this.Increment = this.Increment;
        }
    }

    public void SetShowDisabledText(bool ShowDisabledText)
    {
        TextArea.SetShowDisabledText(ShowDisabledText);
    }

    public void SetSignificantBits(int SignificantBits)
    {
        if (this.SignificantBits !=  SignificantBits)
        {
            this.SignificantBits = SignificantBits;
        }
    }

    protected override void Draw()
    {
        TextBG.Sprites["box"].Bitmap?.Dispose();
        TextBG.Sprites["box"].Bitmap = new Bitmap(TextBG.Size);
        TextBG.Sprites["box"].Bitmap.Unlock();
        Color Edge = this.Enabled && TextArea.SelectedWidget ? new Color(32, 170, 221) : this.Enabled && TextBG.Mouse.Inside ? Color.WHITE : new Color(86, 108, 134);
        Color Filler = this.Enabled ? new Color(86, 108, 134) : new Color(40, 62, 84); // or Color.ALPHA
        int w = TextBG.Size.Width;
        int h = TextBG.Size.Height;
        TextBG.Sprites["box"].Bitmap.DrawLine(0, 0, w - 1, 0, Edge);
        TextBG.Sprites["box"].Bitmap.DrawLine(0, 1, w - 1, 1, new Color(36, 34, 36));
        TextBG.Sprites["box"].Bitmap.DrawLine(0, h - 1, w - 1, h - 1, Edge);
        TextBG.Sprites["box"].Bitmap.DrawLine(0, h - 2, w - 1, h - 2, new Color(36, 34, 36));
        TextBG.Sprites["box"].Bitmap.FillRect(0, 2, w, h - 4, Filler);
        TextBG.Sprites["box"].Bitmap.Lock();
        base.Draw();
    }

    public override void MouseDown(MouseEventArgs e)
    {
        base.MouseDown(e);
        if (!Mouse.Inside && TextArea.SelectedWidget)
        {
            Window.UI.SetSelectedWidget(null);
        }
    }
}
