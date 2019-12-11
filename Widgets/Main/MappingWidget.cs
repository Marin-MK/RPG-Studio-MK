using System;
using ODL;

namespace MKEditor.Widgets
{
    public class MappingWidget : Widget
    {
        public MapViewer mv;
        public TilesetsPanel tt;
        public MapSelectPanel mst;
        public LayersPanel lt;

        public MappingWidget(object Parent, string Name = "mappingWidget")
            : base(Parent, Name)
        {
            Grid layout = new Grid(this);
            layout.SetColumns(
                new GridSize(222, Unit.Pixels),
                new GridSize(1, Unit.Pixels),
                new GridSize(1),
                new GridSize(1, Unit.Pixels),
                new GridSize(283, Unit.Pixels)
            );

            Color DividerColor = new Color(79, 108, 159);

            // Left sidebar
            Sidebar LeftSidebar = new Sidebar(layout);
            LeftSidebar.SetGridColumn(0);

            LeftSidebar.TabControl.CreateTab("Maps");
            mst = new MapSelectPanel(LeftSidebar.TabControl.GetTab(0));

            // Left sidebar divider
            Widget LeftSidebarDivider = new Widget(layout);
            LeftSidebarDivider.SetBackgroundColor(79, 108, 159);
            LeftSidebarDivider.SetGridColumn(1);

            // Right sidebar divider
            Widget RightSidebarDivider = new Widget(layout);
            RightSidebarDivider.SetBackgroundColor(DividerColor);
            RightSidebarDivider.SetGridColumn(3);

            // Right sidebar
            Grid rightcontainer = new Grid(layout);
            rightcontainer.SetGridColumn(4);
            rightcontainer.SetRows(new GridSize(5), new GridSize(1, Unit.Pixels), new GridSize(2));
            rightcontainer.SetColumns(new GridSize(1));
            rightcontainer.SetBackgroundColor(40, 44, 52);


            // Top-Right sidebar
            Sidebar TopRightSidebar = new Sidebar(rightcontainer);

            // Tileset part of right sidebar
            TopRightSidebar.TabControl.CreateTab("Tilesets");
            tt = new TilesetsPanel(TopRightSidebar.TabControl.GetTab(0));

            // Inner right sidebar divider
            Widget InnerRightSidebarDivider = new Widget(rightcontainer);
            InnerRightSidebarDivider.SetBackgroundColor(DividerColor);
            InnerRightSidebarDivider.SetGrid(1, 0);

            // Bottom-Right sidebar
            Sidebar BottomRightSidebar = new Sidebar(rightcontainer);
            BottomRightSidebar.SetGrid(2, 0);

            // Layers part of right sidebar
            BottomRightSidebar.TabControl.CreateTab("Layers");
            lt = new LayersPanel(BottomRightSidebar.TabControl.GetTab(0));


            // Center map viewer
            mv = new MapViewer(layout);
            mv.SetGridColumn(2);
        }
    }
}
