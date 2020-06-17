using System;
using System.Collections.Generic;
using odl;
using RPGStudioMK.Game;
using amethyst;

namespace RPGStudioMK.Widgets
{
    public class ConnectionsPanel : Widget
    {
        public Container ConnectionContainer;
        public VStackPanel StackPanel;

        public Map MapData;

        Label NoConnectionsLabel;

        public ConnectionsPanel(IContainer Parent) : base(Parent)
        {
            SetBackgroundColor(28, 50, 73);
            Label Header = new Label(this);
            Header.SetText("Connected Maps");
            Header.SetFont(Font.Get("Fonts/Ubuntu-B", 16));
            Header.SetPosition(5, 5);

            Sprites["sep"] = new Sprite(this.Viewport, new SolidBitmap(288, 2, new Color(10, 23, 37)));
            Sprites["sep"].Y = 30;

            Sprites["slider"] = new Sprite(this.Viewport, new SolidBitmap(10, Size.Height - 34, new Color(10, 23, 37)));
            Sprites["slider"].Y = 33;

            ConnectionContainer = new Container(this);
            ConnectionContainer.SetPosition(0, 33);
            ConnectionContainer.VAutoScroll = true;
            VScrollBar vs = new VScrollBar(this);
            ConnectionContainer.SetVScrollBar(vs);

            StackPanel = new VStackPanel(ConnectionContainer);
            StackPanel.SetPosition(2, 1);
        }

        public void SetMap(Map Map)
        {
            this.MapData = Map;
            while (StackPanel.Widgets.Count > 0) StackPanel.Widgets[0].Dispose();
            foreach (MapConnection c in Map.Connections)
            {
                ConnectionWidget w = new ConnectionWidget(StackPanel);
                w.SetConnection(c);
            }
            if (Map.Connections.Count == 0)
            {
                NoConnectionsLabel = new MultilineLabel(StackPanel);
                NoConnectionsLabel.SetWidth(StackPanel.Size.Width);
                NoConnectionsLabel.SetFont(Font.Get("Fonts/ProductSans-M", 14));
                NoConnectionsLabel.SetText("This map does not have any map connections.");
                NoConnectionsLabel.SetMargin(8, 8);
            }
            new Widget(StackPanel).SetHeight(1).SetMargin(0, 4).SetBackgroundColor(17, 27, 38);
            new NewConnectionWidget(StackPanel, MapData);
            if (Map.Connections.Count > 0) ((ConnectionWidget) StackPanel.Widgets[0]).SetSelected(true);
            StackPanel.UpdateLayout();
        }

        public void SelectMap(Map Map)
        {
            foreach (Widget w in StackPanel.Widgets)
            {
                if (w is ConnectionWidget && ((ConnectionWidget) w).MapConnection.MapID == Map.ID) ((ConnectionWidget) w).SetSelected(true);
            }
        }

        public ConnectionWidget GetSelectedConnectionWidget()
        {
            foreach (Widget w in StackPanel.Widgets)
            {
                if (w is ConnectionWidget && ((ConnectionWidget) w).Selected) return (ConnectionWidget) w;
            }
            return null;
        }

        public void Disconnect(int MapID)
        {
            MapData.Connections.RemoveAll(c => c.MapID == MapID);
            Data.Maps[MapID].Connections.RemoveAll(c => c.MapID == MapData.ID);
            Editor.MainWindow.MapWidget.SetMap(MapData);
        }

        public void NewConnection()
        {
            List<int> HiddenMaps = new List<int>() { MapData.ID };
            foreach (MapConnection c in MapData.Connections)
            {
                HiddenMaps.Add(c.MapID);
            }
            MapPicker picker = new MapPicker(HiddenMaps);
            picker.OnClosed += delegate (BaseEventArgs e)
            {
                if (picker.ChosenMap != null)
                {
                    int RelativeX = MapData.Width;
                    int RelativeY = 0;
                    this.MapData.Connections.Add(new MapConnection(picker.ChosenMap.ID, RelativeX, RelativeY));
                    picker.ChosenMap.Connections.Add(new MapConnection(this.MapData.ID, -RelativeX, -RelativeY));
                    Editor.MainWindow.MapWidget.SetMap(MapData);
                }
            };
        }

        public override void SizeChanged(BaseEventArgs e)
        {
            base.SizeChanged(e);
            ConnectionContainer.SetSize(Size.Width - 13, Size.Height - ConnectionContainer.Position.Y);
            ConnectionContainer.VScrollBar.SetPosition(Size.Width - 10, 34);
            ConnectionContainer.VScrollBar.SetSize(8, Size.Height - 36);
            StackPanel.SetWidth(ConnectionContainer.Size.Width - 2);
            Sprites["slider"].X = Size.Width - 11;
            (Sprites["slider"].Bitmap as SolidBitmap).SetSize(10, Size.Height - 34);
            if (NoConnectionsLabel != null && !NoConnectionsLabel.Disposed)
            {
                NoConnectionsLabel.SetWidth(StackPanel.Size.Width);
                // Forces a redraw
                NoConnectionsLabel.SetText("");
                NoConnectionsLabel.SetText("This map does not have any map connections.");
            }
        }
    }

    public class NewConnectionWidget : Widget
    {
        Label Label;
        Button PlusButton;
        Map MapData;

        public NewConnectionWidget(IContainer Parent, Map Map) : base(Parent)
        {
            this.MapData = Map;
            PlusButton = new Button(this);
            PlusButton.SetSize(25, 25);
            PlusButton.SetFont(Font.Get("Fonts/ProductSans-M", 13));
            PlusButton.SetTextColor(Color.WHITE);
            PlusButton.SetText("+");

            Label = new Label(this);
            Label.SetPosition(29, 3);
            Label.SetText("New Connection");
            Label.SetFont(Font.Get("Fonts/Ubuntu-B", 14));
            Label.SetTextColor(Color.WHITE);

            Sprites["hover"] = new Sprite(this.Viewport, new SolidBitmap(2, 25, new Color(47, 160, 193)));
            Sprites["hover"].Visible = false;

            SetHeight(25);
        }

        public override void HoverChanged(MouseEventArgs e)
        {
            base.HoverChanged(e);
            PlusButton.SetTextColor(WidgetIM.Hovering ? new Color(55, 187, 255) : Color.WHITE);
            Label.SetTextColor(WidgetIM.Hovering ? new Color(55, 187, 255) : Color.WHITE);
            Sprites["hover"].Visible = WidgetIM.Hovering;
        }

        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);
            if (WidgetIM.Hovering)
            {
                Editor.MainWindow.MapWidget.MapViewerConnections.ConnectionsPanel.NewConnection();
            }
        }
    }
}
