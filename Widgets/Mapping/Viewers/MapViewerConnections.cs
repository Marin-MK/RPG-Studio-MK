using System;
using System.Collections.Generic;
using MKEditor.Game;
using ODL;

namespace MKEditor.Widgets
{
    public class MapViewerConnections : MapViewerBase
    {
        public List<MapImageWidget> ConnWidgets;

        public MapViewerConnections(object Parent, string Name = "mapViewerConnections")
            : base(Parent, Name)
        {
            ConnWidgets = new List<MapImageWidget>();
        }

        public void RedrawConnections()
        {
            ConnWidgets.ForEach(w => w.Dispose());
            ConnWidgets.Clear();
            foreach (KeyValuePair<string, List<Connection>> kvp in Map.Connections)
            {
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    MapImageWidget miw = new MapImageWidget(MainContainer, "miw");
                    miw.GridBackground.SetVisible(false);
                    miw.SetDarkOverlay(200);
                    miw.LoadLayers(Data.Maps[kvp.Value[i].MapID], kvp.Key, kvp.Value[i].Offset);
                    ConnWidgets.Add(miw);
                }
            }
            UpdateConnections();
        }

        public void UpdateConnections()
        {
            for (int i = 0; i < ConnWidgets.Count; i++)
            {
                MapImageWidget miw = ConnWidgets[i];
                int Offset = (int) Math.Round(32d * miw.Offset * ZoomFactor);
                if (miw.Side == ":north") miw.SetPosition(MapWidget.Position.X + Offset, MapWidget.Position.Y - miw.Size.Height);
                else if (miw.Side == ":east") miw.SetPosition(MapWidget.Position.X + MapWidget.Size.Width, MapWidget.Position.Y + Offset);
                else if (miw.Side == ":south") miw.SetPosition(MapWidget.Position.X + Offset, MapWidget.Position.Y + MapWidget.Size.Height);
                else if (miw.Side == ":west") miw.SetPosition(MapWidget.Position.X - miw.Size.Width, MapWidget.Position.Y + Offset);
            }
        }

        public override void PositionMap()
        {
            base.PositionMap();
            UpdateConnections();
        }

        public override void SetMap(Map Map)
        {
            base.SetMap(Map);
            RedrawConnections();
        }

        public override void SetZoomFactor(double factor, bool FromStatusBar = false)
        {
            base.SetZoomFactor(factor, FromStatusBar);
            ConnWidgets.ForEach(w => w.SetZoomFactor(factor));
        }
    }
}
