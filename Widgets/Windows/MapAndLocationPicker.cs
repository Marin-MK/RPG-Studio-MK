using System;
using System.Collections.Generic;
using System.Linq;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class MapAndLocationPicker : PopupWindow
{
    public bool Apply = false;
    public int MapID;
    public Point Location;

    MapListBox MapListBox;
    MiniMapViewer MiniMapViewer;

    public MapAndLocationPicker(string Title, Map StartMap = null, Point StartLocation = null)
    {
        SetTitle(Title);
        MinimumSize = MaximumSize = new Size(800, 700);
        SetSize(MaximumSize);
        Center();

        if (StartMap is null) StartMap = Data.Maps.Values.First();
        if (StartLocation is null) StartLocation = new Point(0, 0);

        MapListBox = new MapListBox(this);
        MapListBox.SetVDocked(true);
        MapListBox.SetPadding(10, 40, 0, 50);
        MapListBox.SetWidth(196);
        MapListBox.SetSelectedMap(StartMap);
        MapListBox.OnMapChanged += _ =>
        {
            MiniMapViewer.SetMap(MapListBox.SelectedMap);
            MiniMapViewer.SetCursorPosition(MapListBox.SelectedMap.Width / 2, MapListBox.SelectedMap.Height / 2);
        };

        Label PositionLabel = new Label(this);
        PositionLabel.SetBottomDocked(true);
        PositionLabel.SetFont(Fonts.CabinMedium.Use(11));
        PositionLabel.SetPadding(220, 0, 0, -5);

        MiniMapViewer = new MiniMapViewer(this);
        MiniMapViewer.SetDocked(true);
        MiniMapViewer.SetPadding(210, 40, 10, 50);
        MiniMapViewer.OnTileConfirmed += _ => OK();
        MiniMapViewer.OnTileChanged += _ =>
        {
            PositionLabel.SetText($"(x: {MiniMapViewer.MapTileX}, y: {MiniMapViewer.MapTileY})");
        };
        MiniMapViewer.SetMap(StartMap);
        MiniMapViewer.SetCursorPosition(StartLocation.X, StartLocation.Y);

        CreateButton("Cancel", _ => Cancel());
        CreateButton("OK", _ => OK());
    }

    private void OK()
    {
        Apply = true;
        Location = new Point(MiniMapViewer.MapTileX, MiniMapViewer.MapTileY);
        Close();
    }

    private void Cancel()
    {
        Close();
    }
}
