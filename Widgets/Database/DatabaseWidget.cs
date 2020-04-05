using System;
using ODL;

namespace MKEditor.Widgets
{
    public class DatabaseWidget : Widget
    {
        Container MainContainer;
        public Widget MainWidget;

        Container DBContainer;
        public DatabaseModeList DBModeList;
        public DatabaseDataList DBDataList;

        public DatabaseWidget(IContainer Parent) : base(Parent)
        {
            Grid MainGrid = new Grid(this);
            MainGrid.SetColumns(
                new GridSize(353, Unit.Pixels),
                new GridSize(1, Unit.Relative)
            );

            VScrollBar vslist = new VScrollBar(this);
            vslist.SetZIndex(1);
            vslist.SetPosition(1, 1);

            DBContainer = new Container(MainGrid);
            DBContainer.OnSizeChanged += delegate (BaseEventArgs e)
            {
                DBModeList.SetSize(DBContainer.Size);
                DBDataList.SetSize(197, DBContainer.Size.Height);
                DBModeList.Redraw();
                if (DBModeList.Size.Height > DBContainer.Size.Height)
                {
                    MainGrid.Columns[0] = new GridSize(353 + 10, Unit.Pixels);
                    DBModeList.SetPosition(10, 0);
                    DBDataList.SetPosition(156 + 10, 0);
                    vslist.MouseInputRect = new Rect(DBContainer.Viewport.X, DBContainer.Viewport.Y, 156, DBContainer.Viewport.Height);
                    MainGrid.UpdateContainers();
                    MainGrid.UpdateLayout();
                }
                else
                {
                    MainGrid.Columns[0] = new GridSize(353, Unit.Pixels);
                    DBModeList.SetPosition(0, 0);
                    DBDataList.SetPosition(156, 0);
                    MainGrid.UpdateContainers();
                    MainGrid.UpdateLayout();
                }
            };
            DBContainer.VAutoScroll = true;
            DBContainer.SetVScrollBar(vslist);
            DBModeList = new DatabaseModeList(DBContainer);
            DBModeList.DBWidget = this;
            DBDataList = new DatabaseDataList(DBContainer);
            DBDataList.ConsiderInAutoScrollPositioning = DBDataList.ConsiderInAutoScrollCalculation = false;
            DBDataList.SetPosition(156, 0);
            DBDataList.DBWidget = this;

            MainContainer = new Container(MainGrid);
            MainContainer.SetGridColumn(1);
            MainContainer.OnSizeChanged += delegate (BaseEventArgs e) { MainWidget.SetSize(MainContainer.Size); };
            
            TilesetEditor te = new TilesetEditor(MainContainer);
            MainWidget = te;

            DBDataList.TilesetEditor = te;
            te.DBDataList = DBDataList;

            DBModeList.SetSelectedIndex(5);
        }

        public override void SizeChanged(BaseEventArgs e)
        {
            base.SizeChanged(e);
            DBContainer.VScrollBar.SetHeight(Size.Height - 2);
        }
    }
}
