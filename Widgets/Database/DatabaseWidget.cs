namespace RPGStudioMK.Widgets;

public class DatabaseWidget : Widget
{
    public DatabaseMode Mode { get; protected set; }

    Grid MainGrid;
    DataTypeList DataTypeList;

    public Widget ActiveDatabaseWidget;

    public DatabaseWidget(IContainer Parent) : base(Parent)
    {
        MainGrid = new Grid(this);
        MainGrid.SetColumns(
            new GridSize(183, Unit.Pixels),
            new GridSize(1)
        );

        DataTypeList = new DataTypeList(MainGrid);
    }

    public void SetMode(DatabaseMode Mode)
    {
        if (this.Mode == Mode) return;
        this.Mode = Mode;
        ActiveDatabaseWidget?.Dispose();
        ActiveDatabaseWidget = null;
        Editor.ProjectSettings.LastDatabaseSubmode = Mode;
        DataTypeList.SetSelected(Mode);
        switch (Mode)
        {
            case DatabaseMode.Species:
                ActiveDatabaseWidget = new DataTypeSpecies(MainGrid);
                break;
            case DatabaseMode.Tilesets:
                ActiveDatabaseWidget = new DataTypeTilesets(MainGrid);
                break;
		}
		ActiveDatabaseWidget?.SetGridColumn(1);
	}
}

