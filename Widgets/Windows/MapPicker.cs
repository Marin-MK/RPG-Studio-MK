using System;
using System.Collections.Generic;
using ODL;
using MKEditor.Game;

namespace MKEditor.Widgets
{
    public class MapPicker : PopupWindow
    {
        public Map ChosenMap;

        ListBox Maps;
        Container PreviewContainer;
        PictureBox MapBox;

        public List<Map> MapList;

        public MapPicker(List<int> HiddenMapIDs, string Title = "Pick a Map", bool ShowIDs = true)
        {
            this.MapList = new List<Map>();
            foreach (KeyValuePair<int, Map> kvp in Data.Maps)
            {
                if (!HiddenMapIDs.Contains(kvp.Key)) this.MapList.Add(kvp.Value);
            }
            Initialize(Title, ShowIDs);
        }

        public MapPicker(List<Map> Maps, string Title = "Pick a Map", bool ShowIDs = true)
        {
            this.MapList = Maps;
            Initialize(Title, ShowIDs);
        }

        protected void Initialize(string Title, bool ShowIDs)
        {
            SetTitle(Title);
            SetSize(600, 469);
            Center();

            Label pickerlabel = new Label(this);
            pickerlabel.SetText("Maps");
            pickerlabel.SetPosition(18, 24);
            pickerlabel.SetFont(Font.Get("Fonts/Ubuntu-B", 14));
            Maps = new ListBox(this);
            Maps.SetPosition(25, 44);
            Maps.SetSize(151, 380);
            List<ListItem> items = new List<ListItem>();
            foreach (Map Map in this.MapList)
            {
                string Name = ShowIDs ? $"{Utilities.Digits(Map.ID, 3)}: {Map.DevName}" : Map.DevName;
                items.Add(new ListItem(Name, Map));
            }
            Maps.SetItems(items);
            Maps.OnSelectionChanged += delegate (object sender, EventArgs e)
            {
                UpdatePreview();
            };

            Label previewlabel = new Label(this);
            previewlabel.SetText("Preview");
            previewlabel.SetPosition(192, 24);
            previewlabel.SetFont(Font.Get("Fonts/Ubuntu-B", 14));

            ColoredBox outline = new ColoredBox(this);
            outline.SetPosition(194, 44);
            outline.SetSize(380, 380);
            outline.SetOuterColor(59, 91, 124);
            outline.SetInnerColor(24, 38, 53);
            PreviewContainer = new Container(this);
            PreviewContainer.SetBackgroundColor(17, 27, 38);
            PreviewContainer.SetPosition(196, 46);
            PreviewContainer.SetSize(376, 376);

            MapBox = new PictureBox(PreviewContainer);
            MapBox.AutoResize = false;

            CreateButton("Cancel", Cancel);
            CreateButton("OK", OK);

            if (Maps.Items.Count > 0) Maps.SetSelectedIndex(0);
        }

        public void UpdatePreview()
        {
            Map data = null;
            if (Maps.SelectedIndex >= 0) data = Maps.Items[Maps.SelectedIndex].Object as Map;
            MapBox.SetSize(1, 1);
            if (data == null) return;
            if (MapBox.Sprite.Bitmap != null) MapBox.Sprite.Bitmap.Dispose();
            MapBox.Sprite.Bitmap = Utilities.CreateMapPreview(data);
            double MultX = (double) PreviewContainer.Size.Width / MapBox.Sprite.Bitmap.Width;
            double MultY = (double) PreviewContainer.Size.Height / MapBox.Sprite.Bitmap.Height;
            double FinalMult = MultX > MultY ? MultY : MultX;
            MapBox.Sprite.ZoomX = MapBox.Sprite.ZoomY = FinalMult;
            int fullw = (int) Math.Round(MapBox.Sprite.Bitmap.Width * FinalMult);
            int fullh = (int) Math.Round(MapBox.Sprite.Bitmap.Height * FinalMult);
            int x = PreviewContainer.Size.Width / 2 - fullw / 2;
            int y = PreviewContainer.Size.Height / 2 - fullh / 2;
            MapBox.SetPosition(x, y);
            MapBox.SetSize(fullw, fullh);
        }

        public void OK(object sender, EventArgs e)
        {
            if (Maps.SelectedIndex >= 0)
            {
                Map map = (Maps.Items[Maps.SelectedIndex].Object as Map);
                this.ChosenMap = map;
            }
            else
            {
                this.ChosenMap = null;
            }
            Close();
        }

        public void Cancel(object sender, EventArgs e)
        {
            this.ChosenMap = null;
            Close();
        }
    }
}
