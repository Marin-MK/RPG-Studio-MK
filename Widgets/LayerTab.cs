using System;
using ODL;

namespace MKEditor.Widgets
{
    public class LayerTab : Widget
    {
        public LayerTab(object Parent, string Name = "layerTab")
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
        }
    }
}
