namespace RPGStudioMK.Widgets;

public class MappingWidget : Widget
{
    public MapSelectPanel MapSelectPanel;
    public MapImageWidget MapImageWidget;

    public Game.Map Map;

    public MapViewerTiles MapViewerTiles;

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

        // Left sidebar
        MapSelectPanel = new MapSelectPanel(layout);

        // Left sidebar divider
        Widget LeftSidebarDivider = new Widget(layout);
        LeftSidebarDivider.SetBackgroundColor(28, 50, 73);
        LeftSidebarDivider.SetGridColumn(1);

        MapViewerTiles = new MapViewerTiles(layout);
        MapViewerTiles.SetGridColumn(2);

        MapImageWidget = new MapImageWidget(MapViewerTiles.MainContainer);
        MapImageWidget.SetZIndex(3);

        MapViewerTiles.MapWidget = MapImageWidget;

        SetHorizontalScroll(0.5);
        SetVerticalScroll(0.5);
    }

    public void SetSelectedLayer(int Index)
    {
        MapViewerTiles.LayerPanel.SetSelectedLayer(Index);
    }

    public void SetZoomFactor(double Factor, bool FromStatusBar = false)
    {
        MapViewerTiles.SetZoomFactor(Factor, FromStatusBar);
    }

    public void SetMap(Game.Map Map)
    {
        this.Map = Map;
        MapImageWidget.LoadLayers(Map);
        MapViewerTiles.SetMap(Map);
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
        MapViewerTiles.MainContainer.HScrollBar.SetValue(Value, false);
    }

    public void SetVerticalScroll(double Value)
    {
        MapViewerTiles.MainContainer.VScrollBar.SetValue(Value, false);
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        MapViewerTiles.PositionMap();
        SetHorizontalScroll(0.5);
        SetVerticalScroll(0.5);
    }
}
