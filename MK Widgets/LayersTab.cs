using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.Widgets
{
    public class LayersTab : Widget
    {
        public MapViewer MapViewer;

        private Container layercontainer;
        private VStackPanel layerstack;

        public List<LayerWidget> Layers = new List<LayerWidget>();

        public LayersTab(object Parent, string Name = "layersTab")
            : base(Parent, Name)
        {
            this.Sprites["text"] = new Sprite(this.Viewport);
            this.Sprites["text"].X = 6;
            this.Sprites["text"].Y = 14;
            Font f = Font.Get("Fonts/Quicksand Bold", 16);
            Size s = f.TextSize("Layers");
            this.Sprites["text"].Bitmap = new Bitmap(s);
            this.Sprites["text"].Bitmap.Unlock();
            this.Sprites["text"].Bitmap.Font = f;
            this.Sprites["text"].Bitmap.DrawText("Layers", 0, 0, Color.WHITE);
            this.Sprites["text"].Bitmap.Lock();

            layercontainer = new Container(this);
            layercontainer.SetPosition(30, 45);
            layercontainer.AutoScroll = true;

            layerstack = new VStackPanel(layercontainer);

            Layers = new List<LayerWidget>()
            {
                new LayerWidget(layerstack),
                new LayerWidget(layerstack),
                new LayerWidget(layerstack),
                new LayerWidget(layerstack),
                new LayerWidget(layerstack)
            };
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            layercontainer.SetSize(279, this.Size.Height - 55);
            layerstack.SetWidth(263);
        }

        public void SetMapViewer(MapViewer mv)
        {
            this.MapViewer = mv;
            Layers[Layers.Count - 1].SetLayerSelected(true);
        }
    }
}
