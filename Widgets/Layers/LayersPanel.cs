using System;
using System.Collections.Generic;
using ODL;
using MKEditor.Game;

namespace MKEditor.Widgets
{
    public class LayersPanel : Widget
    {
        public TilesetsPanel TilesetTab;
        public MapViewer MapViewer;
        public Map Map { get { return this.MapViewer.Map; } }

        public int SelectedLayer { get { return layerwidget.SelectedLayer; } }

        private Container layercontainer;
        public LayerWidget layerwidget;

        public LayersPanel(object Parent, string Name = "layersTab")
            : base(Parent, Name)
        {
            Sprites["bar1"] = new Sprite(this.Viewport, new SolidBitmap(1, 1, new Color(28, 50, 73)));
            Sprites["bar2"] = new Sprite(this.Viewport, new SolidBitmap(1, 1, new Color(28, 50, 73)));
            Sprites["bar2"].X = 42;

            this.OnWidgetSelected = WidgetSelected;

            layercontainer = new Container(this);
            layercontainer.SetPosition(1, 1);
            layercontainer.VAutoScroll = true;

            VScrollBar vs = new VScrollBar(this);
            layercontainer.SetVScrollBar(vs);

            layerwidget = new LayerWidget(layercontainer);

            this.SetContextMenuList(new List<IMenuItem>()
            {
                new MenuItem("New Layer")
                {
                    OnLeftClick = NewLayer
                },
                new MenuItem("Rename Layer")
                {
                    Shortcut = "F2",
                    OnLeftClick = RenameLayer
                },
                new MenuSeparator(),
                new MenuItem("Toggle Visibility")
                {
                    //Shortcut = "Ctrl+H",
                    OnLeftClick = ToggleVisibilityLayer,
                    IsClickable = delegate (object sender, ConditionEventArgs e ) { e.ConditionValue = layerwidget.HoveringIndex >= 0; }
                },
                new MenuItem("Move Layer Up")
                {
                    OnLeftClick = MoveLayerUp,
                    IsClickable = delegate (object sender, ConditionEventArgs e) { e.ConditionValue = SelectedLayer < Map.Layers.Count - 1 && layerwidget.HoveringIndex >= 0; }
                },
                new MenuItem("Move Layer Down")
                {
                    OnLeftClick = MoveLayerDown,
                    IsClickable = delegate (object sender, ConditionEventArgs e) { e.ConditionValue = SelectedLayer > 0 && layerwidget.HoveringIndex >= 0; }
                },
                new MenuSeparator(),
                new MenuItem("Delete Layer")
                {
                    Shortcut = "Del",
                    OnLeftClick = DeleteLayer,
                    IsClickable = delegate (object sender, ConditionEventArgs e) { e.ConditionValue = Map.Layers.Count > 1 && layerwidget.HoveringIndex >= 0; }
                }
            });

            RegisterShortcuts(new List<Shortcut>()
            {
                //new Shortcut(this, new Key(Keycode.H, Keycode.CTRL), new EventHandler<EventArgs>(ToggleVisibilityLayer), true),
                new Shortcut(this, new Key(Keycode.DELETE), new EventHandler<EventArgs>(DeleteLayer)),
                new Shortcut(this, new Key(Keycode.F2), new EventHandler<EventArgs>(RenameLayer))
            });

            SetSize(283, 200); // Dummy size so the sprites can be drawn properly
        }

        public void SetSelectedLayer(int LayerIndex)
        {
            layerwidget.SetSelectedLayer(LayerIndex);
        }

        public void NewLayer(object sender, EventArgs e)
        {
            int selected = SelectedLayer;
            Editor.UnsavedChanges = true;
            if (layerwidget.HoveringIndex == -1) // Add to top (highest layer) if not hovering over a layer
                selected = -1;
            Layer layer = new Layer($"New Layer");
            layer.Tiles = new List<TileData>(Map.Width * Map.Height);
            for (int i = 0; i < Map.Width * Map.Height; i++) layer.Tiles.Add(null);
            MapViewer.CreateNewLayer(selected + 1, layer);
            int oldselected = selected;
            CreateLayers(); // Updates list to reflect new layer
            layerwidget.SetSelectedLayer(oldselected + 1); // Update selected layer
        }

        public void RenameLayer(object sender, EventArgs e)
        {
            layerwidget.RenameLayer(SelectedLayer);
        }

        public void ToggleVisibilityLayer(object sender, EventArgs e)
        {
            layerwidget.SetLayerVisible(SelectedLayer, !Map.Layers[SelectedLayer].Visible);
        }

        public void MoveLayerUp(object sender, EventArgs e)
        {
            if (SelectedLayer >= Map.Layers.Count - 1) return;
            Editor.UnsavedChanges = true;
            MapViewer.SwapLayers(SelectedLayer + 1, SelectedLayer);
            int oldselected = SelectedLayer;
            CreateLayers();
            layerwidget.SetSelectedLayer(oldselected + 1);
        }

        public void MoveLayerDown(object sender, EventArgs e)
        {
            if (SelectedLayer <= 0) return;
            Editor.UnsavedChanges = true;
            MapViewer.SwapLayers(SelectedLayer - 1, SelectedLayer);
            int oldselected = SelectedLayer;
            CreateLayers();
            layerwidget.SetSelectedLayer(oldselected - 1);
        }

        public void DeleteLayer(object sender, EventArgs e)
        {
            if (Map.Layers.Count > 1)
            {
                Editor.UnsavedChanges = true;
                MapViewer.DeleteLayer(SelectedLayer);
                int oldselected = SelectedLayer - 1;
                CreateLayers();
                if (oldselected < 0) oldselected = 0;
                layerwidget.UpdateLayers();
                layerwidget.SetSelectedLayer(oldselected);
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
            layercontainer.SetSize(Size.Width - 12, Size.Height - 1);
            layercontainer.VScrollBar.SetPosition(Size.Width - 9, 1);
            layercontainer.VScrollBar.SetSize(8, Size.Height - 2);
            (Sprites["bar1"].Bitmap as SolidBitmap).SetSize(1, Size.Height);
            (Sprites["bar2"].Bitmap as SolidBitmap).SetSize(1, Size.Height);
            Sprites["bar1"].X = Size.Width - 11;
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

        public override void WidgetSelected(object sender, MouseEventArgs e)
        {
            if (layerwidget.RenameBox == null || !layerwidget.RenameBox.WidgetIM.Hovering)
                Window.UI.SetSelectedWidget(this);
        }
    }
}
