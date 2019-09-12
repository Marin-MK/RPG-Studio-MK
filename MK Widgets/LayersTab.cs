using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.Widgets
{
    public class LayersTab : Widget
    {
        public TilesetTab TilesetTab;
        public MapViewer MapViewer;
        public Data.Map Map { get { return this.MapViewer.Map; } }

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
            SetBackgroundColor(27, 28, 32);
            this.Sprites["header"] = new Sprite(this.Viewport, new Bitmap(314, 22));
            this.Sprites["header"].Bitmap.Unlock();
            this.Sprites["header"].Bitmap.FillRect(0, 0, 314, 22, new Color(40, 44, 52));
            this.Sprites["header"].Bitmap.Font = Font.Get("Fonts/Ubuntu-R", 16);
            this.Sprites["header"].Bitmap.DrawText("Layers", 6, 1, Color.WHITE);
            this.Sprites["header"].Bitmap.Lock();

            this.OnWidgetSelect += WidgetSelect;

            layercontainer = new Container(this);
            layercontainer.SetPosition(0, 26);
            layercontainer.SetSize(314, this.Size.Height - 30);
            layercontainer.VAutoScroll = true;
            layercontainer.ShowScrollBars = true;

            VScrollBar vs = new VScrollBar(this);
            layercontainer.SetVScrollBar(vs);

            layerstack = new VStackPanel(layercontainer);
            layerstack.SetWidth(293);
            layerstack.SetContextMenuList(new List<IMenuItem>()
            {
                new MenuItem("New Layer")
                {
                    Icon = Icon.New,
                    OnLeftClick = NewLayer
                },
                new MenuSeparator(),
                new MenuItem("Toggle Visibility")
                {
                    Icon = Icon.Eye,
                    Shortcut = "Ctrl+H",
                    OnLeftClick = ToggleVisibilityLayer
                },
                new MenuItem("Move Layer Up")
                {
                    Icon = Icon.Up,
                    OnLeftClick = MoveLayerUp,
                    IsClickable = delegate (object sender, ConditionEventArgs e) { e.ConditionValue = SelectedLayer < this.Map.Layers.Count - 1; }
                },
                new MenuItem("Move Layer Down")
                {
                    Icon = Icon.Down,
                    OnLeftClick = MoveLayerDown,
                    IsClickable = delegate (object sender, ConditionEventArgs e) { e.ConditionValue = SelectedLayer > 0; }
                },
                new MenuSeparator(),
                new MenuItem("Delete Layer")
                {
                    Icon = Icon.Delete,
                    Shortcut = "Del",
                    OnLeftClick = DeleteLayer,
                    IsClickable = delegate (object sender, ConditionEventArgs e) { e.ConditionValue = Layers.Count > 1; }
                }
            });

            RegisterShortcuts(new List<Shortcut>()
            {
                new Shortcut(new Key(Keycode.H, Keycode.CTRL), new EventHandler<EventArgs>(ToggleVisibilityLayer), true),
                new Shortcut(new Key(Keycode.DELETE), new EventHandler<EventArgs>(DeleteLayer)),
                new Shortcut(new Key(Keycode.RETURN), new EventHandler<EventArgs>(delegate (object sender, EventArgs e)
                {
                    new MapPropertiesWindow(Window);
                }))
            });
        }

        public void UpdateNames()
        {
            for (int i = 0; i < Map.Layers.Count; i++) Map.Layers[i].Name = $"Layer {i + 1}";
        }

        public void NewLayer(object sender, EventArgs e)
        {
            // Temporarily opens map properties window for debugging purposes.
            //MapPropertiesWindow mpw = new MapPropertiesWindow(Window);
            Data.Layer layer = new Data.Layer($"Layer {SelectedLayer + 2}");
            layer.Tiles = new List<Data.TileData>();
            for (int i = 0; i < Map.Width * Map.Height; i++) layer.Tiles.Add(null);
            Map.Layers.Insert(SelectedLayer + 1, layer);
            UpdateNames();
            int oldselected = SelectedLayer;
            MapViewer.RedrawLayers();
            CreateLayers();
            Layers[Map.Layers.Count - oldselected - 2].SetLayerSelected(true);
        }

        public void ToggleVisibilityLayer(object sender, EventArgs e)
        {
            int layeridx = this.Map.Layers.Count - SelectedLayer - 1;
            this.Layers[layeridx].SetLayerVisible(!this.Layers[layeridx].LayerVisible);
        }

        public void MoveLayerUp(object sender, EventArgs e)
        {
            if (SelectedLayer >= Map.Layers.Count - 1) return;
            Data.Layer layer1 = Map.Layers[SelectedLayer + 1];
            Map.Layers[SelectedLayer + 1] = Map.Layers[SelectedLayer];
            Map.Layers[SelectedLayer] = layer1;
            UpdateNames();
            int oldselected = SelectedLayer;
            MapViewer.RedrawLayers();
            CreateLayers();
            Layers[Map.Layers.Count - oldselected - 2].SetLayerSelected(true);
        }

        public void MoveLayerDown(object sender, EventArgs e)
        {
            if (SelectedLayer <= 0) return;
            Data.Layer layer1 = Map.Layers[SelectedLayer - 1];
            Map.Layers[SelectedLayer - 1] = Map.Layers[SelectedLayer];
            Map.Layers[SelectedLayer] = layer1;
            UpdateNames();
            int oldselected = SelectedLayer;
            MapViewer.RedrawLayers();
            CreateLayers();
            Layers[Map.Layers.Count - oldselected].SetLayerSelected(true);
        }

        public void DeleteLayer(object sender, EventArgs e)
        {
            if (Map.Layers.Count > 0)
            {
                Map.Layers.RemoveAt(SelectedLayer);
                UpdateNames();
                int oldselected = SelectedLayer;
                MapViewer.RedrawLayers();
                CreateLayers();
                if (oldselected == 0) oldselected += 1;
                Layers[Map.Layers.Count - oldselected].SetLayerSelected(true);
            }
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            layercontainer.SetHeight(this.Size.Height - 30);
            base.SizeChanged(sender, e);
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
                layerwidget.LayerIndex = this.Map.Layers.Count - layer;
                layerwidget.SetText(this.Map.Layers[layer].Name);
                this.Layers.Add(layerwidget);
            }
            this.Layers[this.Map.Layers.Count - 1].SetLayerSelected(true);
        }

        public override void Update()
        {
            if (this.SelectedWidget)
            {
                if (SelectedLayer > 0 && Input.Trigger(SDL2.SDL.SDL_Keycode.SDLK_DOWN))
                {
                    this.Layers[this.Map.Layers.Count - SelectedLayer].SetLayerSelected(true);
                }
                if (SelectedLayer < this.Map.Layers.Count - 1 && Input.Trigger(SDL2.SDL.SDL_Keycode.SDLK_UP))
                {
                    this.Layers[this.Map.Layers.Count - SelectedLayer - 2].SetLayerSelected(true);
                }
            }
            base.Update();
        }
    }
}
