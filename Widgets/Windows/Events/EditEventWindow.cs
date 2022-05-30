using System;
using System.Collections.Generic;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class EditEventWindow : PopupWindow
{
    Button DeletePageButton;
    Button ClearPageButton;
    Button PastePageButton;
    Button CopyPageButton;
    Button NewPageButton;
    SubmodeView PageControl;
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

        Font HeaderFont = Fonts.UbuntuBold.Use(16);
        Font SmallFont = Fonts.ProductSansMedium.Use(14);

        Label NameLabel = new Label(this);
        NameLabel.SetFont(SmallFont);
        NameLabel.SetPosition(23, 35);
        NameLabel.SetText("Name:");

        TextBox NameBox = new TextBox(this);
        NameBox.SetPosition(21, 52);
        NameBox.SetFont(SmallFont);
        NameBox.SetSize(161, 27);
        NameBox.SetText(this.Event.Name);
        NameBox.OnTextChanged += _ => this.Event.Name = NameBox.Text;

        Label SizeLabel = new Label(this);
        SizeLabel.SetFont(SmallFont);
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
        SizeXLabel.SetFont(SmallFont);
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

        DeletePageButton = new Button(this);
        DeletePageButton.SetFont(HeaderFont);
        DeletePageButton.SetSize(72, 60);
        DeletePageButton.SetPosition(Size.Width - DeletePageButton.Size.Width - 14, 21);
        DeletePageButton.SetText("Delete\nPage");
        DeletePageButton.SetEnabled(Event.Pages.Count > 1);
        DeletePageButton.OnClicked += _ => DeletePage();

        ClearPageButton = new Button(this);
        ClearPageButton.SetFont(HeaderFont);
        ClearPageButton.SetSize(72, 60);
        ClearPageButton.SetPosition(DeletePageButton.Position.X - ClearPageButton.Size.Width, 21);
        ClearPageButton.SetText("Clear\nPage");
        ClearPageButton.OnClicked += _ => ClearPage();

        PastePageButton = new Button(this);
        PastePageButton.SetFont(HeaderFont);
        PastePageButton.SetSize(72, 60);
        PastePageButton.SetPosition(ClearPageButton.Position.X - PastePageButton.Size.Width, 21);
        PastePageButton.SetText("Paste\nPage");
        PastePageButton.OnClicked += _ => PastePage();

        CopyPageButton = new Button(this);
        CopyPageButton.SetFont(HeaderFont);
        CopyPageButton.SetSize(72, 60);
        CopyPageButton.SetPosition(PastePageButton.Position.X - CopyPageButton.Size.Width, 21);
        CopyPageButton.SetText("Copy\nPage");
        CopyPageButton.OnClicked += _ => CopyPage();

        NewPageButton = new Button(this);
        NewPageButton.SetFont(HeaderFont);
        NewPageButton.SetSize(72, 60);
        NewPageButton.SetPosition(CopyPageButton.Position.X - NewPageButton.Size.Width, 21);
        NewPageButton.SetText("New\nPage");
        NewPageButton.OnClicked += _ => NewPage();

        PageControl = new SubmodeView(this);
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

        RegisterShortcuts(new List<Shortcut>()
        {
            new Shortcut(this, new Key(Keycode.ENTER, Keycode.CTRL), _ => OK(), true)
        });

        CreateButton("Cancel", _ => Cancel());
        CreateButton("OK", _ => OK());
    }

    private void NewPage()
    {
        InsertPage(PageControl.SelectedIndex + 1, new EventPage());
    }

    private void InsertPage(int Index, EventPage Page)
    {
        Event.Pages.Insert(Index, Page);
        for (int i = Index; i < PageControl.Tabs.Count; i++)
        {
            PageControl.Names[i] = (i + 2).ToString();
        }
        PageControl.InsertTab(Index, (Index + 1).ToString());
        PageControl.Redraw();
        PageControl.SelectTab(Index);
        DeletePageButton.SetEnabled(true);
    }

    private void CopyPage()
    {
        EventPage Page = Event.Pages[PageControl.SelectedIndex];
        Utilities.SetClipboard(Page, BinaryData.EVENT_PAGE);
    }

    private void PastePage()
    {
        if (!Utilities.IsClipboardValidBinary(BinaryData.EVENT_PAGE)) return;
        EventPage Page = Utilities.GetClipboard<EventPage>();
        InsertPage(PageControl.SelectedIndex + 1, Page);
    }

    private void ClearPage()
    {
        Event.Pages[PageControl.SelectedIndex] = new EventPage();
        EventPageControl.SetEventPage(Map, Event, Event.Pages[PageControl.SelectedIndex]);
    }

    private void DeletePage()
    {
        Event.Pages.RemoveAt(PageControl.SelectedIndex);
        PageControl.RemoveTab(PageControl.SelectedIndex);
        for (int i = PageControl.SelectedIndex; i < PageControl.Tabs.Count; i++)
        {
            PageControl.Names[i] = (i + 1).ToString();
        }
        PageControl.Redraw();
        DeletePageButton.SetEnabled(Event.Pages.Count > 1);
    }

    private void OK()
    {
        this.Map.Events[Event.ID] = this.Event;
        Apply = true;
        Close();
    }

    private void Cancel()
    {
        Close();
    }
}
