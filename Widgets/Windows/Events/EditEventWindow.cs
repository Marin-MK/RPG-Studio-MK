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
    EventPageList EPL;
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
        MinimumSize = MaximumSize = new Size(994, 654);
        SetSize(MaximumSize);
        Center();

        Font HeaderFont = Fonts.UbuntuBold.Use(13);
        Font SmallFont = Fonts.CabinMedium.Use(11);

        EventPageControl = new EventPageControl(this);
        EventPageControl.SetPadding(8, 42, 9, 52);
        EventPageControl.SetDocked(true);

        Label NameLabel = new Label(this);
        NameLabel.SetFont(SmallFont);
        NameLabel.SetPosition(27, 50);
        NameLabel.SetText("Name:");

        TextBox NameBox = new TextBox(this);
        NameBox.SetPosition(27, 68);
        NameBox.SetFont(SmallFont);
        NameBox.SetSize(161, 27);
        NameBox.SetText(this.Event.Name);
        NameBox.OnTextChanged += _ => this.Event.Name = NameBox.Text;

        Label WidthLabel = new Label(this);
        WidthLabel.SetFont(SmallFont);
        WidthLabel.SetPosition(287, 50);
        WidthLabel.SetText("Width:");
        WidthLabel.SetVisible(Data.EssentialsAtLeast(EssentialsVersion.v19));
        NumericBox WidthBox = new NumericBox(this);
        WidthBox.SetPosition(287, 68);
        WidthBox.SetSize(91, 27);
        WidthBox.SetMinValue(1);
        WidthBox.SetValue(this.Event.Width);
        WidthBox.OnValueChanged += _ =>
        {
            this.Event.Width = WidthBox.Value;
            EventPageControl.RedrawGraphic();
        };
        WidthBox.SetVisible(Data.EssentialsAtLeast(EssentialsVersion.v19));

        Label XLabel = new Label(this);
        XLabel.SetFont(SmallFont);
        XLabel.SetPosition(390, 71);
        XLabel.SetText("x");
        XLabel.SetVisible(Data.EssentialsAtLeast(EssentialsVersion.v19));

        Label HeightLabel = new Label(this);
        HeightLabel.SetFont(SmallFont);
        HeightLabel.SetPosition(410, 50);
        HeightLabel.SetText("Height:");
        HeightLabel.SetVisible(Data.EssentialsAtLeast(EssentialsVersion.v19));
        NumericBox HeightBox = new NumericBox(this);
        HeightBox.SetPosition(410, 68);
        HeightBox.SetSize(91, 27);
        HeightBox.SetMinValue(1);
        HeightBox.SetValue(this.Event.Height);
        HeightBox.OnValueChanged += _ =>
        {
            this.Event.Height = HeightBox.Value;
            EventPageControl.RedrawGraphic();
        };
        HeightBox.SetVisible(Data.EssentialsAtLeast(EssentialsVersion.v19));

        Label PagesLabel = new Label(this);
        PagesLabel.SetFont(HeaderFont);
        PagesLabel.SetPosition(14, 99);
        PagesLabel.SetText("Pages");
        Container EPLContainer = new Container(this);
        EPLContainer.SetVDocked(true);
        EPLContainer.SetPadding(12, 135, 0, 207);
        EPLContainer.SetWidth(97);

        // Dummy VScrollBar for scrolling functionality, remains invisible
        VScrollBar vs = new VScrollBar(this);
        vs.KeepInvisible = true;
        vs.MinScrollStep = vs.ScrollStep = 26 / 3f;

        EPLContainer.VAutoScroll = true;
        EPLContainer.SetVScrollBar(vs);
        EPL = new EventPageList(EPLContainer);
        EPL.SetHDocked(true);
        EPL.SetEvent(this.Event);
        EPL.OnSelectedPageChanged += _ =>
        {
            EventPageControl.SetEventPage(Map, this.Event, this.Event.Pages[EPL.SelectedPage]);
        };

        DeletePageButton = new Button(this);
        DeletePageButton.SetPosition(19, 567);
        DeletePageButton.SetSize(84, 32);
        DeletePageButton.SetText("Delete");
        DeletePageButton.SetEnabled(Event.Pages.Count > 1);
        DeletePageButton.OnClicked += _ => DeletePage();

        ClearPageButton = new Button(this);
        ClearPageButton.SetPosition(19, 539);
        ClearPageButton.SetSize(84, 32);
        ClearPageButton.SetText("Clear");
        ClearPageButton.OnClicked += _ => ClearPage();

        PastePageButton = new Button(this);
        PastePageButton.SetPosition(19, 511);
        PastePageButton.SetSize(84, 32);
        PastePageButton.SetText("Paste");
        PastePageButton.OnClicked += _ => PastePage();

        CopyPageButton = new Button(this);
        CopyPageButton.SetPosition(19, 483);
        CopyPageButton.SetSize(84, 32);
        CopyPageButton.SetText("Copy");
        CopyPageButton.OnClicked += _ => CopyPage();

        NewPageButton = new Button(this);
        NewPageButton.SetPosition(19, 455);
        NewPageButton.SetSize(84, 32);
        NewPageButton.SetText("New");
        NewPageButton.OnClicked += _ => NewPage();

        EPL.SetSelectedPage(0);

        RegisterShortcuts(new List<Shortcut>()
        {
            new Shortcut(this, new Key(Keycode.ENTER, Keycode.CTRL), _ => OK(), true)
        });

        CreateButton("Cancel", _ => Cancel());
        CreateButton("OK", _ => OK());
    }

    private void NewPage()
    {
        InsertPage(EPL.SelectedPage + 1, new EventPage());
    }

    private void InsertPage(int Index, EventPage Page)
    {
        Event.Pages.Insert(Index, Page);
        EPL.SetEvent(Event);
        EPL.SetSelectedPage(Index);
        DeletePageButton.SetEnabled(true);
    }

    private void CopyPage()
    {
        EventPage Page = Event.Pages[EPL.SelectedPage];
        Utilities.SetClipboard(Page, BinaryData.EVENT_PAGE);
    }

    private void PastePage()
    {
        if (!Utilities.IsClipboardValidBinary(BinaryData.EVENT_PAGE)) return;
        EventPage Page = Utilities.GetClipboard<EventPage>();
        InsertPage(EPL.SelectedPage + 1, Page);
    }

    private void ClearPage()
    {
        Event.Pages[EPL.SelectedPage] = new EventPage();
        EventPageControl.SetEventPage(Map, Event, Event.Pages[EPL.SelectedPage]);
    }

    private void DeletePage()
    {
        Event.Pages.RemoveAt(EPL.SelectedPage);
        EPL.SetEvent(Event);
        if (EPL.SelectedPage >= Event.Pages.Count) EPL.SetSelectedPage(Event.Pages.Count - 1, true);
        else EPL.SetSelectedPage(EPL.SelectedPage, true);
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