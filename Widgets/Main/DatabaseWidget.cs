using System;
using ODL;

namespace MKEditor.Widgets
{
    public class DatabaseWidget : Widget
    {
        Container MainContainer;
        public Widget MainWidget;

        Container DBContainer;
        public DatabaseList DBList;

        public DatabaseWidget(object Parent, string Name = "databaseWidget")
            : base(Parent, Name)
        {
            Grid MainGrid = new Grid(this);
            MainGrid.SetColumns(
                new GridSize(355, Unit.Pixels),
                new GridSize(1, Unit.Relative)
            );

            DBContainer = new Container(MainGrid);
            DBContainer.OnSizeChanged += delegate (object sender, SizeEventArgs e) { DBList.SetSize(DBContainer.Size); DBList.Redraw(); };
            DBList = new DatabaseList(DBContainer);

            MainContainer = new Container(MainGrid);
            MainContainer.SetGridColumn(1);
            VignetteFade Fade = new VignetteFade(MainContainer);
            MainContainer.OnSizeChanged += delegate (object sender, SizeEventArgs e) { MainWidget.SetSize(MainContainer.Size); Fade.SetSize(MainContainer.Size); };
            
            TilesetEditor te = new TilesetEditor(MainContainer);
            MainWidget = te;

            DBList.TilesetEditor = te;
            te.DBList = DBList;

            DBList.DataList.SetSelectedIndex(0);
        }
    }
}
