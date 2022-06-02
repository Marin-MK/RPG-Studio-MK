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

        SetSize(283, 200); // Dummy size so the sprites can be drawn properly
    }

    public void SetSelectedLayer(int LayerIndex)
    {
        layerwidget.SetSelectedLayer(LayerIndex);
    }

    public void ToggleVisibilityLayer(BaseEventArgs e)
    {
        layerwidget.SetLayerVisible(SelectedLayer, !Map.Layers[SelectedLayer].Visible);
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
