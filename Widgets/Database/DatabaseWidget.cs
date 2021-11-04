using System;
using odl;
using amethyst;

namespace RPGStudioMK.Widgets
{
    public class DatabaseWidget : Widget
    {
        public string Mode { get; protected set; }

        Grid MainGrid;
        DataTypeList DataTypeList;

        Widget ActiveDatabaseWidget;

        public DatabaseWidget(IContainer Parent) : base(Parent)
        {
            MainGrid = new Grid(this);
            MainGrid.SetColumns(
                new GridSize(183, Unit.Pixels),
                new GridSize(1)
            );

            DataTypeList = new DataTypeList(MainGrid);
        }

        public void SetMode(string Mode)
        {
            if (this.Mode == Mode) return;
            this.Mode = Mode;
            ActiveDatabaseWidget?.Dispose();
            ActiveDatabaseWidget = null;
            if (Mode == "species")
            {
                DataTypeList.SetSelected(Mode);
                ActiveDatabaseWidget = new DataTypeSpecies(MainGrid);
                ActiveDatabaseWidget.SetGridColumn(1);
                ((DataTypeSpecies) ActiveDatabaseWidget).SetSelectedIndex(0);
             }
        }
    }
}
