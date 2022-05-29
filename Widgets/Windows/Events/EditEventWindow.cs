using System;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class EditEventWindow : PopupWindow
{
    EventPageControl EventPageControl;

    public Map Map;
    public Event Event;
    public bool UpdateGraphic = false;
    public bool Apply = false;

    public EditEventWindow(Map Map, Event ev)
    {
        this.Map = Map;
        this.Event = (Event) ev.Clone();

        SetTitle($"Edit event (ID: {Utilities.Digits(Event.ID, 3)})");
        MinimumSize = MaximumSize = new Size(740, 614);
        SetSize(MaximumSize);
        Center();

        Font f = Fonts.ProductSansMedium.Use(14);

        Label NameLabel = new Label(this);
        NameLabel.SetFont(f);
        NameLabel.SetPosition(23, 35);
        NameLabel.SetText("Name:");

        TextBox NameBox = new TextBox(this);
        NameBox.SetPosition(21, 52);
        NameBox.SetFont(f);
        NameBox.SetSize(161, 27);
        NameBox.SetText(this.Event.Name);
        NameBox.OnTextChanged += _ => this.Event.Name = NameBox.Text;

        Label SizeLabel = new Label(this);
        SizeLabel.SetFont(f);
        SizeLabel.SetPosition(NameBox.Position.X + NameBox.Size.Width + 12, 35);
        SizeLabel.SetText("Size:");

        NumericBox WidthBox = new NumericBox(this);
        WidthBox.SetPosition(NameBox.Position.X + NameBox.Size.Width + 12, 52);
        WidthBox.SetSize(48, 27);
        WidthBox.MinValue = 1;
        WidthBox.SetValue(this.Event.Width);
        WidthBox.OnValueChanged += _ =>
        {
            this.Event.Width = WidthBox.Value;
            EventPageControl.RedrawGraphic();
        };

        Label SizeXLabel = new Label(this);
        SizeXLabel.SetFont(f);
        SizeXLabel.SetPosition(WidthBox.Position.X + WidthBox.Size.Width + 4, 56);
        SizeXLabel.SetText("x");

        NumericBox HeightBox = new NumericBox(this);
        HeightBox.SetPosition(WidthBox.Position.X + WidthBox.Size.Width + 16, 52);
        HeightBox.SetSize(48, 27);
        HeightBox.MinValue = 1;
        HeightBox.SetValue(this.Event.Height);
        HeightBox.OnValueChanged += _ =>
        {
            this.Event.Height = HeightBox.Value;
            EventPageControl.RedrawGraphic();
        };

        SubmodeView PageControl = new SubmodeView(this);
        PageControl.SetPosition(8, 89);
        PageControl.SetSize(Size.Width - 16, 25);
        PageControl.SetHeaderHeight(29);
        PageControl.SetHeaderSelBackgroundColor(new Color(59, 91, 124));
        for (int i = 0; i < this.Event.Pages.Count; i++)
        {
            PageControl.CreateTab((i + 1).ToString());
        }
        PageControl.OnSelectionChanged += _ =>
        {
            EventPageControl.SetEventPage(Map, this.Event, this.Event.Pages[PageControl.SelectedIndex]);
        };

        Widget PageDivider = new Widget(this);
        PageDivider.SetPosition(0, PageControl.Position.Y + PageControl.Size.Height);
        PageDivider.SetHDocked(true);
        PageDivider.SetMargins(8, 0);
        PageDivider.SetBackgroundColor(new Color(59, 91, 124));
        PageDivider.SetHeight(4);

        EventPageControl = new EventPageControl(this);
        EventPageControl.SetMargins(8, PageDivider.Position.Y + PageDivider.Size.Height, 8, 41);
        EventPageControl.SetDocked(true);

        PageControl.SelectTab(0);

        CreateButton("Cancel", Cancel);
        CreateButton("OK", OK);
    }

    private void OK(BaseEventArgs e)
    {
        this.Map.Events[Event.ID] = this.Event;
        Apply = true;
        Close();
    }

    private void Cancel(BaseEventArgs e)
    {
        Close();
    }
}
