using System;
using ODL;

namespace MKEditor.Widgets
{
    public class MappingWidget : Widget
    {
        public MapSelectPanel MapSelectPanel;
        public MapImageWidget MapImageWidget;

        public TabView Submodes;

        public Game.Map Map;

        public MapViewerTiles MapViewerTiles;
        //public MapViewerEvents MapViewerEvents;
        public MapViewerConnections MapViewerConnections;
        //public MapViewerEncounters MapViewerEncounters;
        //public MapViewerProperties MapViewerProperties;

        public MapViewerBase ActiveMapViewer { get { return Submodes.Tabs[Submodes.SelectedIndex].Widgets[0] as MapViewerBase; } }

        // MapViewerTiles
        public TilesetPanel TilesetPanel;
        public LayerPanel LayerPanel;

        public MappingWidget(object Parent, string Name = "mappingWidget")
            : base(Parent, Name)
        {
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
            LeftSidebarDivider.SetBackgroundColor(79, 108, 159);
            LeftSidebarDivider.SetGridColumn(1);

            Submodes = new TabView(layout);
            Submodes.OnSelectionChanged += delegate (object sender, EventArgs e)
            {
                ChangeSubmode();
            };
            Submodes.SetGridColumn(2);
            Submodes.CreateTab("Tiles");
            //Submodes.CreateTab("Events");
            Submodes.CreateTab("Connections");
            //Submodes.CreateTab("Encounters");
            //Submodes.CreateTab("Properties");
            MapViewerTiles = new MapViewerTiles(Submodes.GetTab(0));
            //MapViewerEvents = new MapViewerEvents(Submodes.GetTab(1));
            MapViewerConnections = new MapViewerConnections(Submodes.GetTab(1));
            //MapViewerEncounters = new MapViewerEncounters(Submodes.GetTab(3));
            //MapViewerProperties = new MapViewerProperties(Submodes.GetTab(4));

            MapImageWidget = new MapImageWidget(MapViewerTiles.MainContainer);

            MapViewerTiles.MapWidget = MapImageWidget;
            //MapViewerEvents.MapWidget = MapImageWidget;
            MapViewerConnections.MapWidget = MapImageWidget;
            //MapViewerEncounters.MapWidget = MapImageWidget;
            //mapViewerProperties.MapWidget = MapImageWidget;
        }

        int OldSelectedIndex = -1;

        public void ChangeSubmode()
        {
            if (OldSelectedIndex == Submodes.SelectedIndex) return;
            MapImageWidget.SetParent(ActiveMapViewer.MainContainer);
            MapImageWidget.SetVisible(true);
            if (ActiveMapViewer.Map != null) ActiveMapViewer.PositionMap();
            if (ActiveMapViewer is MapViewerConnections) MapImageWidget.GridBackground.SetVisible(false);
            else MapImageWidget.GridBackground.SetVisible(true);
            MapImageWidget.MapViewer = ActiveMapViewer;
            OldSelectedIndex = Submodes.SelectedIndex;
        }

        public void SetSelectedLayer(int Index)
        {
            MapViewerTiles.LayerPanel.SetSelectedLayer(Index);
        }

        public void SetZoomFactor(double Factor, bool FromStatusBar = false)
        {
            MapViewerTiles.SetZoomFactor(Factor, FromStatusBar);
            //MapViewerEvents.SetZoomFactor(Factor, FromStatusBar);
            MapViewerConnections.SetZoomFactor(Factor, FromStatusBar);
            //MapViewerEncounters.SetZoomFactor(Factor, FromStatusBar);
            //MapViewerProperties.SetZoomFactor(Factor, FromStatusBar);
            //if (Submodes.SelectedIndex != -1) ActiveMapViewer.PositionMap();
        }

        public void SetMap(Game.Map Map)
        {
            this.Map = Map;
            MapImageWidget.LoadLayers(Map);
            MapViewerTiles.SetMap(Map);
            //MapViewerEvents.SetMap(Map);
            MapViewerConnections.SetMap(Map);
            //MapViewerEncounters.SetMap(Map);
            //MapViewerProperties.SetMap(Map);
            //if (Submodes.SelectedIndex != -1) ActiveMapViewer.PositionMap();
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            //if (Submodes.SelectedIndex != -1) ActiveMapViewer.PositionMap();
        }
    }
}
