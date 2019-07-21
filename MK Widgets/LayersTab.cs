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
            this.Sprites["header"] = new Sprite(this.Viewport, new Bitmap(314, 22));
            this.Sprites["header"].Bitmap.Unlock();
            this.Sprites["header"].Bitmap.FillRect(0, 0, 314, 22, new Color(135, 135, 135));
            this.Sprites["header"].Bitmap.Font = Font.Get("Fonts/Ubuntu-R", 16);
            this.Sprites["header"].Bitmap.DrawText("Layers", 6, 0, Color.WHITE);
            this.Sprites["header"].Bitmap.Lock();

            SetBackgroundColor(47, 49, 54);

            this.OnWidgetSelect += WidgetSelect;

            layercontainer = new Container(this);
            layercontainer.SetPosition(0, 26);
            layercontainer.SetSize(314, this.Size.Height - 30);
            layercontainer.AutoScroll = true;
            layercontainer.ShowScrollBars = true;

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
                //new Shortcut(new Key(Keycode.F2), new EventHandler<EventArgs>(RenameLayer)),
                new Shortcut(new Key(Keycode.H, Keycode.CTRL), new EventHandler<EventArgs>(ToggleVisibilityLayer), true),
                new Shortcut(new Key(Keycode.DELETE), new EventHandler<EventArgs>(DeleteLayer))
            });
        }

        public void NewLayer(object sender, EventArgs e)
        {
            MapPropertiesWindow mpw = new MapPropertiesWindow(Window);
            return;
            int Index = Layers.Count - SelectedLayer - 1;
            Console.WriteLine("New");
            LayerWidget lw = new LayerWidget(layerstack, "layerWidget", Index);
            lw.LayerIndex = Index + 1;

            for (int i = 0; i < this.Layers.Count; i++)
            {
                if (i >= Index)
                {
                    this.Layers[i].LayerIndex++;
                }
            }

            lw.SetText("Random layer " + new Random().Next().ToString());
            this.Layers.Insert(Index, lw);
            Data.Layer emptylayer = new Data.Layer("Random layer");
            emptylayer.Tiles = new List<Data.TileData>();
            for (int i = 0; i < this.Map.Width * this.Map.Height; i++) emptylayer.Tiles.Add(new Data.TileData());
            this.Map.Layers.Insert(Layers.Count - Index - 1, emptylayer);
            this.MapViewer.AddEmptyLayer(Layers.Count - Index - 1);
            layerstack.Redraw();
            layerstack.Update();
            layerstack.UpdateLayout();
        }

        //public void RenameLayer(object sender, EventArgs e)
        //{
        //    Console.WriteLine("Rename");
        //}

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
            if (this.Selected)
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
