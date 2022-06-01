using System.Collections.Generic;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class LayerPanel : Widget
{
    public TilesPanel TilesPanel;
    public MapViewer MapViewer;
    public Map Map { get { return this.MapViewer.Map; } }

    public int SelectedLayer { get { return layerwidget.SelectedLayer; } }

    public bool UsingLeft = false;
    public bool UsingRight = false;

    private Container layercontainer;
    public LayerWidget layerwidget;

    public LayerPanel(IContainer Parent) : base(Parent)
    {
        Label Header = new Label(this);
        Header.SetText("Layers");
        Header.SetFont(Fonts.UbuntuBold.Use(13));
        Header.SetPosition(5, 5);

        Sprites["sep"] = new Sprite(this.Viewport, new SolidBitmap(288, 2, new Color(10, 23, 37)));
        Sprites["sep"].Y = 30;

        Sprites["slider"] = new Sprite(this.Viewport, new SolidBitmap(10, Size.Height - 34, new Color(10, 23, 37)));
        Sprites["slider"].Y = 33;

        this.OnWidgetSelected = WidgetSelected;

        layercontainer = new Container(this);
        layercontainer.SetDocked(true);
        layercontainer.SetMargins(1, 33, 13, 0);
        layercontainer.VAutoScroll = true;

        VScrollBar vs = new VScrollBar(this);
        vs.SetVDocked(true);
        vs.SetRightDocked(true);
        vs.SetMargins(0, 34, 0, 2);
        layercontainer.SetVScrollBar(vs);

        layerwidget = new LayerWidget(layercontainer);

        this.SetContextMenuList(new List<IMenuItem>()
            {
                new MenuItem("New Layer")
                {
                    OnClicked = NewLayerEvent,
                    IsClickable = delegate (BoolEventArgs e) { e.Value = false; }
                },
                new MenuItem("Rename Layer")
                {
                    Shortcut = "F2",
                    OnClicked = RenameLayer,
                    IsClickable = delegate (BoolEventArgs e) { e.Value = false; }
                },
                new MenuSeparator(),
                new MenuItem("Toggle Visibility")
                {
                    OnClicked = ToggleVisibilityLayer,
                    IsClickable = delegate (BoolEventArgs e) { e.Value = layerwidget.HoveringIndex >= 0; }
                },
                new MenuItem("Move Layer Up")
                {
                    OnClicked = MoveLayerUpEvent,
                    IsClickable = delegate (BoolEventArgs e) { e.Value = SelectedLayer < Map.Layers.Count - 1 && layerwidget.HoveringIndex >= 0; }
                },
                new MenuItem("Move Layer Down")
                {
                    OnClicked = MoveLayerDownEvent,
                    IsClickable = delegate (BoolEventArgs e) { e.Value = SelectedLayer > 0 && layerwidget.HoveringIndex >= 0; }
                },
                new MenuSeparator(),
                new MenuItem("Delete Layer")
                {
                    Shortcut = "Del",
                    OnClicked = DeleteLayerEvent,
                    IsClickable = delegate (BoolEventArgs e) { e.Value = false && Map.Layers.Count > 1 && layerwidget.HoveringIndex >= 0; },
                }
            });

        //RegisterShortcuts(new List<Shortcut>()
        //{
        //    new Shortcut(this, new Key(Keycode.DELETE), DeleteLayerEvent),
        //    new Shortcut(this, new Key(Keycode.F2), RenameLayer)
        //});

        SetSize(283, 200); // Dummy size so the sprites can be drawn properly
    }

    public void SetSelectedLayer(int LayerIndex)
    {
        layerwidget.SetSelectedLayer(LayerIndex);
    }

    private void NewLayerEvent(BaseEventArgs e)
    {
        Layer layer = new Layer($"New Layer");
        layer.Tiles = new List<TileData>(Map.Width * Map.Height);
        for (int i = 0; i < Map.Width * Map.Height; i++) layer.Tiles.Add(null);
        NewLayer(layerwidget.HoveringIndex, layer);
    }

    public void NewLayer(int Index, Layer LayerData, bool IsUndoAction = false)
    {
        Editor.UnsavedChanges = true;
        if (Index == -1) // Add to top (highest layer) if not hovering over a layer
            Index = -1;
        MapViewer.CreateNewLayer(Index + 1, LayerData, IsUndoAction);
        int oldselected = Index;
        CreateLayers(); // Updates list to reflect new layer
        layerwidget.SetSelectedLayer(oldselected + 1); // Update selected layer
    }

    public void RenameLayer(BaseEventArgs e)
    {
        layerwidget.RenameLayer(SelectedLayer);
    }

    public void ToggleVisibilityLayer(BaseEventArgs e)
    {
        layerwidget.SetLayerVisible(SelectedLayer, !Map.Layers[SelectedLayer].Visible);
    }

    private void MoveLayerUpEvent(BaseEventArgs e)
    {
        MoveLayerUp(SelectedLayer);
    }

    public void MoveLayerUp(int LayerIndex, bool IsUndoAction = false)
    {
        if (LayerIndex >= Map.Layers.Count - 1) return;
        Editor.UnsavedChanges = true;
        MapViewer.SwapLayers(LayerIndex + 1, LayerIndex);
        CreateLayers();
        layerwidget.SetSelectedLayer(LayerIndex + 1);
        if (!IsUndoAction) Undo.LayerSwapUndoAction.Create(Editor.MainWindow.MapWidget.Map.ID, LayerIndex, true);
    }

    private void MoveLayerDownEvent(BaseEventArgs e)
    {
        MoveLayerDown(SelectedLayer);
    }

    public void MoveLayerDown(int LayerIndex, bool IsUndoAction = false)
    {
        if (LayerIndex <= 0) return;
        Editor.UnsavedChanges = true;
        MapViewer.SwapLayers(LayerIndex - 1, LayerIndex);
        CreateLayers();
        layerwidget.SetSelectedLayer(LayerIndex - 1);
        if (!IsUndoAction) Undo.LayerSwapUndoAction.Create(Editor.MainWindow.MapWidget.Map.ID, LayerIndex, false);
    }

    private void DeleteLayerEvent(BaseEventArgs e)
    {
        DeleteLayer(SelectedLayer);
    }

    public void DeleteLayer(int Index, bool IsUndoAction = false)
    {
        if (Map.Layers.Count > 1)
        {
            Editor.UnsavedChanges = true;
            MapViewer.DeleteLayer(Index, IsUndoAction);
            int oldselected = Index - 1;
            CreateLayers();
            if (oldselected < 0) oldselected = 0;
            layerwidget.UpdateLayers();
            layerwidget.SetSelectedLayer(oldselected);
        }
    }

    public void FocusLayer(BaseEventArgs e)
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

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        Sprites["slider"].X = Size.Width - 11;
        (Sprites["slider"].Bitmap as SolidBitmap).SetSize(10, Size.Height - 34);
    }

    public void CreateLayers()
    {
        layerwidget.SetLayers(Map.Layers);
    }

    public override void Update()
    {
        if (this.SelectedWidget)
        {
            if (SelectedLayer > 0 && Input.Trigger(Keycode.DOWN))
            {
                layerwidget.SetSelectedLayer(SelectedLayer - 1);
            }
            if (SelectedLayer < this.Map.Layers.Count - 1 && Input.Trigger(Keycode.UP))
            {
                layerwidget.SetSelectedLayer(SelectedLayer + 1);
            }
        }
        base.Update();
    }

    public override void WidgetSelected(BaseEventArgs e)
    {
        if (layerwidget.RenameBox == null || !layerwidget.RenameBox.Mouse.Inside)
            Window.UI.SetSelectedWidget(this);
    }
}
