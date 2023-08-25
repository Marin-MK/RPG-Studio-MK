using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class MapLocationPicker : PopupWindow
{
    public bool Apply = false;
    public Point Location;

    MiniMapViewer MiniMapViewer;

    public MapLocationPicker(string Title, Map Map, Point StartLocation = null)
    {
        if (StartLocation == null) StartLocation = new Point(0, 0);

        SetTitle(Title);
        MinimumSize = MaximumSize = new Size(700, 700);
        SetSize(MaximumSize);
        Center();

        Label PositionLabel = new Label(this);
        PositionLabel.SetBottomDocked(true);
        PositionLabel.SetFont(Fonts.Paragraph);
        PositionLabel.SetPadding(20, 0, 0, -5);

        MiniMapViewer = new MiniMapViewer(this);
        MiniMapViewer.SetDocked(true);
        MiniMapViewer.SetPadding(10, 40, 10, 50);
        MiniMapViewer.OnTileConfirmed += _ => OK();
        MiniMapViewer.OnTileChanged += _ =>
        {
            PositionLabel.SetText($"(x: {MiniMapViewer.MapTileX}, y: {MiniMapViewer.MapTileY})");
        };
        MiniMapViewer.SetMap(Map);
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
