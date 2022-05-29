using System;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class EditEventWindow : PopupWindow
{
    EventPageControl EventPageControl;

    public EditEventWindow(Event Event)
    {
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
        NameBox.SetText(Event.Name);

        SubmodeView PageControl = new SubmodeView(this);
        PageControl.SetPosition(8, 89);
        PageControl.SetSize(Size.Width - 16, 25);
        PageControl.SetHeaderHeight(29);
        PageControl.SetHeaderSelBackgroundColor(new Color(59, 91, 124));
        for (int i = 0; i < Event.Pages.Count; i++)
        {
            PageControl.CreateTab((i + 1).ToString());
        }
        PageControl.OnSelectionChanged += _ =>
        {
            EventPageControl.SetEventPage(Event.Pages[PageControl.SelectedIndex]);
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

        CreateButton("Apply", Apply);
        CreateButton("Cancel", Cancel);
        CreateButton("OK", OK);
    }

    private void OK(BaseEventArgs e)
    {
        Apply(e);
        Dispose();
    }

    private void Cancel(BaseEventArgs e)
    {
        Dispose();
    }

    private void Apply(BaseEventArgs e)
    {

    }
}
