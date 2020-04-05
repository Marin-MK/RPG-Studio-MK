using System;
using System.Collections.Generic;
using MKEditor.Game;
using ODL;

namespace MKEditor.Widgets
{
    public class MappingWidget : Widget
    {
        public MapSelectPanel MapSelectPanel;
        public MapImageWidget MapImageWidget;

        public SubmodeView Submodes;

        public Game.Map Map;

        public MapViewerTiles MapViewerTiles;
        //public MapViewerEvents MapViewerEvents;
        public MapViewerConnections MapViewerConnections;
        //public MapViewerEncounters MapViewerEncounters;
        //public MapViewerProperties MapViewerProperties;

        public MapViewerBase ActiveMapViewer { get { return Submodes.Tabs[Submodes.SelectedIndex].Widgets[0] as MapViewerBase; } }

        // MapViewerTiles
        public TilesPanel TilesPanel;
        public LayerPanel LayerPanel;

        public MappingWidget(IContainer Parent) : base(Parent)
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
            LeftSidebarDivider.SetBackgroundColor(28, 50, 73);
            LeftSidebarDivider.SetGridColumn(1);

            Submodes = new SubmodeView(layout);
            Submodes.OnSelectionChanged += delegate (BaseEventArgs e)
            {
                ChangeSubmode();
            };
            Submodes.SetHeaderHeight(31);
            Submodes.SetHeaderSelHeight(1);
            Submodes.SetTextY(6);
            
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
            MapImageWidget.SetZIndex(3); // 1 for normal map connections, 2 for the selected map connection, so 3 for the main map.

            MapViewerTiles.MapWidget = MapImageWidget;
            //MapViewerEvents.MapWidget = MapImageWidget;
            MapViewerConnections.MapWidget = MapImageWidget;
            //MapViewerEncounters.MapWidget = MapImageWidget;
            //mapViewerProperties.MapWidget = MapImageWidget;

            SetHorizontalScroll(0.5);
            SetVerticalScroll(0.5);
        }

        int OldSelectedIndex = -1;

        public void SetSelectedIndex(int Index)
        {
            if (Index != Submodes.SelectedIndex) Submodes.SelectTab(Index);
            if (MapImageWidget.Parent == ActiveMapViewer.MainContainer) return; // Already selected
            string Submode = new List<string>() { "TILES", "CONNECTIONS" }[Index];
            Editor.ProjectSettings.LastMappingSubmode = Submode;
            MapImageWidget.SetParent(ActiveMapViewer.MainContainer);
            MapImageWidget.SetVisible(true);
            ActiveMapViewer.ZoomByScroll = true; // Ensures it retains the scroll bar scroll value
            if (ActiveMapViewer.Map != null) ActiveMapViewer.PositionMap();

            if (Submode == "TILES") // Select Tiles submode
            {
                for (int i = 0; i < Map.Layers.Count; i++) MapImageWidget.Sprites[i.ToString()].Visible = Map.Layers[i].Visible;
            }
            else // Deselect Tiles submode
            {

            }
            if (Submode == "CONNECTIONS") // Select Connections submode
            {
                MapImageWidget.GridBackground.SetVisible(false);
                for (int i = 0; i < Map.Layers.Count; i++) MapImageWidget.Sprites[i.ToString()].Visible = true;
            }
            else // Deselect Selections submode
            {
                MapImageWidget.GridBackground.SetVisible(Editor.GeneralSettings.ShowGrid);
            }
            MapImageWidget.MapViewer = ActiveMapViewer;
            OldSelectedIndex = Submodes.SelectedIndex;
        }

        public void ChangeSubmode()
        {
            if (OldSelectedIndex == Submodes.SelectedIndex) return;
            this.SetSelectedIndex(Submodes.SelectedIndex);
        }

        public void SetSubmode(string Submode)
        {
            if (Submode == "TILES")
            {
                SetSelectedIndex(0);
            }
            else if (Submode == "CONNECTIONS")
            {
                SetSelectedIndex(1);
                Editor.MainWindow.UI.SetSelectedWidget(MapViewerConnections.ConnectionsPanel.GetSelectedConnectionWidget());
            }
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
            MapViewerConnections.MainContainer.HScrollBar.SetValue(Value, false);
        }

        public void SetVerticalScroll(double Value)
        {
            MapViewerTiles.MainContainer.VScrollBar.SetValue(Value, false);
            MapViewerConnections.MainContainer.VScrollBar.SetValue(Value, false);
        }

        public override void SizeChanged(BaseEventArgs e)
        {
            base.SizeChanged(e);
            if (Submodes.SelectedIndex != -1) ActiveMapViewer.PositionMap();
        }
    }
}
