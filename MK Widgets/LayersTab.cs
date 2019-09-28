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
        public TabView TabView;
        public TabContainer MainContainer;

        public int SelectedLayer { get { return layerwidget.SelectedLayer; } }

        private Container layercontainer;
        private LayerWidget layerwidget;

        private IconButton FocusButton;
        private NewLayerButton NewButton;
        private IconButton MoveUp;
        private IconButton MoveDown;
        private IconButton DeleteButton;

        public LayersTab(object Parent, string Name = "layersTab")
            : base(Parent, Name)
        {
            SetBackgroundColor(10, 23, 37);

            Sprites["bg"] = new Sprite(this.Viewport);
            Sprites["bg"].Y = 25;

            TabView = new TabView(this);
            TabView.CreateTab("Layers");
            MainContainer = TabView.GetTab(0);

            FocusButton = new IconButton(MainContainer);
            FocusButton.SetIcon(13, 0);
            FocusButton.SetPosition(10, 8);
            FocusButton.SetSelectorOffset(4);
            FocusButton.Selectable = false;
            FocusButton.OnLeftClick += FocusLayer;

            NewButton = new NewLayerButton(MainContainer);
            NewButton.SetPosition(46, 3);

            MoveUp = new IconButton(MainContainer);
            MoveUp.SetIcon(10, 0);
            MoveUp.SetPosition(178, 8);
            MoveUp.SetSelectorOffset(3);
            MoveUp.Selectable = false;
            MoveUp.OnLeftClick += MoveLayerUp;

            MoveDown = new IconButton(MainContainer);
            MoveDown.SetIcon(11, 0);
            MoveDown.SetPosition(213, 8);
            MoveDown.SetSelectorOffset(3);
            MoveDown.Selectable = false;
            MoveDown.OnLeftClick += MoveLayerDown;

            DeleteButton = new IconButton(MainContainer);
            DeleteButton.SetIcon(12, 0);
            DeleteButton.SetPosition(248, 8);
            DeleteButton.SetSelectorOffset(3);
            DeleteButton.Selectable = false;
            DeleteButton.OnLeftClick += DeleteLayer;

            this.OnWidgetSelect += WidgetSelect;

            layercontainer = new Container(MainContainer);
            layercontainer.SetPosition(2, 40);
            layercontainer.VAutoScroll = true;

            VScrollBar vs = new VScrollBar(this);
            layercontainer.SetVScrollBar(vs);

            layerwidget = new LayerWidget(layercontainer);
            layerwidget.SetContextMenuList(new List<IMenuItem>()
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
                    IsClickable = delegate (object sender, ConditionEventArgs e) { e.ConditionValue = SelectedLayer < Map.Layers.Count - 1; }
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
                    IsClickable = delegate (object sender, ConditionEventArgs e) { e.ConditionValue = Map.Layers.Count > 1; }
                }
            });

            RegisterShortcuts(new List<Shortcut>()
            {
                new Shortcut(new Key(Keycode.H, Keycode.CTRL), new EventHandler<EventArgs>(ToggleVisibilityLayer), true),
                new Shortcut(new Key(Keycode.DELETE), new EventHandler<EventArgs>(DeleteLayer))
            });

            SetSize(293, 200); // Dummy size so the sprites can be drawn properly
        }

        public void UpdateNames()
        {
            for (int i = 0; i < Map.Layers.Count; i++) Map.Layers[i].Name = $"Layer {i + 1}";
        }

        public void NewLayer(object sender, EventArgs e)
        {
            Data.Layer layer = new Data.Layer($"Layer {SelectedLayer + 2}");
            layer.Tiles = new List<Data.TileData>();
            for (int i = 0; i < Map.Width * Map.Height; i++) layer.Tiles.Add(null);
            Map.Layers.Insert(SelectedLayer + 1, layer);
            UpdateNames();
            int oldselected = SelectedLayer;
            MapViewer.RedrawLayers();
            CreateLayers();
            layerwidget.UpdateLayers();
            layerwidget.SetSelectedLayer(oldselected + 1);
        }

        public void ToggleVisibilityLayer(object sender, EventArgs e)
        {
            layerwidget.SetLayerVisible(SelectedLayer, !Map.Layers[SelectedLayer].Visible);
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
            layerwidget.SetSelectedLayer(oldselected + 1);
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
            layerwidget.SetSelectedLayer(oldselected - 1);
        }

        public void DeleteLayer(object sender, EventArgs e)
        {
            if (Map.Layers.Count > 1)
            {
                Map.Layers.RemoveAt(SelectedLayer);
                UpdateNames();
                int oldselected = SelectedLayer;
                MapViewer.RedrawLayers();
                CreateLayers();
                if (oldselected == 0) oldselected += 1;
                layerwidget.UpdateLayers();
                layerwidget.SetSelectedLayer(Map.Layers.Count - 1);
            }
        }

        public void FocusLayer(object sender, EventArgs e)
        {
            bool OnlySelectedIsVisible = !Map.Layers.Exists(layer => layer != Map.Layers[SelectedLayer] && layer.Visible);
            if (!Map.Layers[SelectedLayer].Visible)
            {
                OnlySelectedIsVisible = true;
                layerwidget.SetLayerVisible(SelectedLayer, true);
            }
            for (int i = 0; i < Map.Layers.Count; i++)
            {
                if (i != SelectedLayer)
                {
                    layerwidget.SetLayerVisible(i, OnlySelectedIsVisible);
                }
            }
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            TabView.SetSize(Size);
            layercontainer.SetSize(Size.Width - 14, Size.Height - 64);
            layercontainer.VScrollBar.SetPosition(Size.Width - 10, 66);
            layercontainer.VScrollBar.SetSize(8, Size.Height - 67);
            if (Sprites["bg"].Bitmap != null) Sprites["bg"].Bitmap.Dispose();
            Sprites["bg"].Bitmap = new Bitmap(Size.Width, Size.Height - 25);
            Sprites["bg"].Bitmap.Unlock();
            Sprites["bg"].Bitmap.FillRect(0, 0, Size.Width, 40, new Color(28, 50, 73));
            Sprites["bg"].Bitmap.DrawLine(0, 40, 0, Size.Height - 26, new Color(28, 50, 73));
            Sprites["bg"].Bitmap.DrawLine(Size.Width - 1, 40, Size.Width - 1, Size.Height - 26, new Color(28, 50, 73));
            Sprites["bg"].Bitmap.DrawLine(Size.Width - 12, 40, Size.Width - 12, Size.Height - 26, new Color(28, 50, 73));
            Sprites["bg"].Bitmap.DrawLine(43, 40, 43, Size.Height - 26, new Color(28, 50, 73));

            Sprites["bg"].Bitmap.Lock();
        }

        public void CreateLayers()
        {
            layerwidget.SetLayers(Map.Layers);
        }

        public override void Update()
        {
            if (this.SelectedWidget)
            {
                if (SelectedLayer > 0 && Input.Trigger(SDL2.SDL.SDL_Keycode.SDLK_DOWN))
                {
                    layerwidget.SetSelectedLayer(SelectedLayer - 1);
                }
                if (SelectedLayer < this.Map.Layers.Count - 1 && Input.Trigger(SDL2.SDL.SDL_Keycode.SDLK_UP))
                {
                    layerwidget.SetSelectedLayer(SelectedLayer + 1);
                }
            }
            base.Update();
        }
    }
}
