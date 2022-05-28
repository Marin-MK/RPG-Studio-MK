namespace RPGStudioMK.Widgets;

public class MappingWidget : Widget
{
    public MapSelectPanel MapSelectPanel;
    public MapImageWidget MapImageWidget;

    public Game.Map Map;

    public MapViewer MapViewer;

    public SubmodeView SubmodePicker;
    public TilesPanel TilesPanel;
    public LayerPanel LayerPanel;

    public MappingWidget(IContainer Parent) : base(Parent)
    {
        Editor.MainWindow.MainEditorWidget = this;
        Grid layout = new Grid(this);
        layout.SetColumns(
            new GridSize(222, Unit.Pixels),
            new GridSize(1, Unit.Pixels),
            new GridSize(1)
        );
        layout.SetRows(
            new GridSize(29, Unit.Pixels),
            new GridSize(1)
        );

        // Left sidebar
        MapSelectPanel = new MapSelectPanel(layout);
        MapSelectPanel.SetGridRow(0, 1);

        // Left sidebar divider
        Widget LeftSidebarDivider = new Widget(layout);
        LeftSidebarDivider.SetBackgroundColor(28, 50, 73);
        LeftSidebarDivider.SetGridColumn(1);
        LeftSidebarDivider.SetGridRow(0, 1);

        SubmodePicker = new SubmodeView(layout);
        SubmodePicker.SetPosition(8, 0);
        SubmodePicker.SetGridColumn(2);
        SubmodePicker.SetGridRow(0);
        SubmodePicker.SetBackgroundColor(10, 23, 37);
        SubmodePicker.SetHeaderHeight(33);
        SubmodePicker.SetHeaderWidth(100);
        SubmodePicker.CreateTab("Tiles");
        SubmodePicker.CreateTab("Events");
        SubmodePicker.OnSelectionChanged += _ => SetMode((MapMode) SubmodePicker.SelectedIndex);

        MapViewer = new MapViewer(layout);
        MapViewer.SetGridColumn(2);
        MapViewer.SetGridRow(1);

        MapImageWidget = new MapImageWidget(MapViewer.MainContainer);
        MapImageWidget.SetZIndex(3);

        MapViewer.MapWidget = MapImageWidget;

        SetHorizontalScroll(0.5);
        SetVerticalScroll(0.5);
    }

    public void SetMode(MapMode Mode)
    {
        MapViewer.SetMode(Mode);
    }

    public void SetSelectedLayer(int Index)
    {
        MapViewer.LayerPanel.SetSelectedLayer(Index);
    }

    public void SetZoomFactor(double Factor, bool FromStatusBar = false)
    {
        MapViewer.SetZoomFactor(Factor, FromStatusBar);
    }

    public void SetMap(Game.Map Map)
    {
        this.Map = Map;
        MapImageWidget.LoadLayers(Map);
        MapViewer.SetMap(Map);
    }

    public void SetMapAnimations(bool Animations)
    {
        MapImageWidget.SetMapAnimations(Animations);
    }

    public void SetGridVisibility(bool Visible)
    {
        MapImageWidget.SetGridVisibility(Visible);
    }

    public void SetHorizontalScroll(double Value)
    {
        MapViewer.MainContainer.HScrollBar.SetValue(Value, false);
    }

    public void SetVerticalScroll(double Value)
    {
        MapViewer.MainContainer.VScrollBar.SetValue(Value, false);
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        MapViewer.PositionMap();
        SetHorizontalScroll(0.5);
        SetVerticalScroll(0.5);
    }
}
