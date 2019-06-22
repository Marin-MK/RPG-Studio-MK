using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.Widgets
{
    public class LayersTab : Widget
    {
        public TilesetTab TilesetTab;
        public MapViewer MapViewer;
        public MKD.Map Map { get { return this.MapViewer.Map; } }

        public int SelectedLayer
        {
            get
            {
                return Layers.Count - Layers.Find(lw => lw.LayerSelected).LayerIndex;
            }
        }

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

            this.OnWidgetSelect += WidgetSelect;

            layercontainer = new Container(this);
            layercontainer.SetPosition(30, 45);
            layercontainer.AutoScroll = true;

            layerstack = new VStackPanel(layercontainer);
            layerstack.SetContextMenuList(new List<IMenuItem>()
            {
                new MenuItem("New Layer") { OnLeftClick = NewLayer },
                new MenuItem("Rename Layer") { Shortcut = "F2", OnLeftClick = RenameLayer },
                new MenuItem("Toggle Visibility") { Shortcut = "Ctrl+H", OnLeftClick = ToggleVisibilityLayer },
                new MenuItem("Move Layer Up") { OnLeftClick = MoveLayerUp },
                new MenuItem("Move Layer Down") { OnLeftClick = MoveLayerDown },
                new MenuSeparator(),
                new MenuItem("Delete Layer") { Shortcut = "Del", OnLeftClick = DeleteLayer }
            });

            RegisterShortcuts(new Dictionary<Key, EventHandler<EventArgs>>()
            {
                { new Key(Keycode.F2), new EventHandler<EventArgs>(RenameLayer) },
                { new Key(Keycode.H, Keycode.CTRL), new EventHandler<EventArgs>(ToggleVisibilityLayer) },
                { new Key(Keycode.DELETE), new EventHandler<EventArgs>(DeleteLayer) }
            });
        }

        public void NewLayer(object sender, EventArgs e)
        {
            Console.WriteLine("New");
        }

        public void RenameLayer(object sender, EventArgs e)
        {
            Console.WriteLine("Rename");
        }

        public void MoveLayerUp(object sender, EventArgs e)
        {
            Console.WriteLine("Move Up");
        }

        public void MoveLayerDown(object sender, EventArgs e)
        {
            Console.WriteLine("Move Down");
        }

        public void ToggleVisibilityLayer(object sender, EventArgs e)
        {
            Console.WriteLine("Toggle Visibility");
            int layeridx = this.Map.Layers.Count - SelectedLayer - 1;
            this.Layers[layeridx].SetLayerVisible(!this.Layers[layeridx].LayerVisible);
        }

        public void DeleteLayer(object sender, EventArgs e)
        {
            Console.WriteLine("Delete");
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            layercontainer.SetSize(279, this.Size.Height - 55);
            layerstack.SetWidth(263);
        }

        public void CreateLayers()
        {
            for (int i = 0; i < Layers.Count; i++)
            {
                Layers[i].Dispose();
            }
            Layers.Clear();
            for (int layer = this.Map.Layers.Count - 1; layer >= 0; layer--)
            {
                LayerWidget layerwidget = new LayerWidget(layerstack);
                layerwidget.SetText(this.Map.Layers[layer].Name);
                this.Layers.Add(layerwidget);
            }
            this.Layers[this.Map.Layers.Count - 1].SetLayerSelected(true);
        }
    }
}
