using System;
using ODL;

namespace MKEditor.Widgets
{
    public class EventingWidget : Widget
    {
        public MapSelectPanel MapSelectPanel;
        public EventMapImageWidget EventMapImageWidget;
        public MapViewerEvents MapViewer;
        public Grid MainGrid;
        public Game.Map Map { get { return MapViewer.Map; } }
        public EventListPanel EventListPanel;

        public EventingWidget(IContainer Parent) : base(Parent)
        {
            MainGrid = new Grid(this);
            MainGrid.SetColumns(
                new GridSize(222, Unit.Pixels),
                new GridSize(1, Unit.Pixels),
                new GridSize(1),
                new GridSize(288, Unit.Pixels)
            );

            MapSelectPanel = new MapSelectPanel(MainGrid);
            MapSelectPanel.SetGridColumn(0);

            Widget w = new Widget(MainGrid);
            w.SetBackgroundColor(28, 50, 73);
            w.SetGridColumn(1);

            MapViewer = new MapViewerEvents(MainGrid);
            MapViewer.SetGridColumn(2);

            EventMapImageWidget = new EventMapImageWidget(MapViewer.MainContainer);
            MapViewer.MapWidget = EventMapImageWidget;

            EventListPanel = new EventListPanel(MainGrid);
            EventListPanel.SetGridColumn(3);
        }

        public void SetMap(Game.Map Map)
        {
            EventMapImageWidget.LoadLayers(Map);
            MapViewer.SetMap(Map);
            EventListPanel.SetMap(Map);
        }

        public void SetZoomFactor(double Factor, bool FromStatusBar = false)
        {
            MapViewer.SetZoomFactor(Factor, FromStatusBar);
        }
    }
}
