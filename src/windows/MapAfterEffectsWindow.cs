using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class MapAfterEffectsWindow : PopupWindow
{
	public Map Map { get; }

    GroupBoxWithScrollBars MapContainer;
    ImageBox MapBox;
    Container BloomContainer;
    CheckBox BloomCheckBox;
    NumericBox BloomScaleBox;
    NumericBox BloomThresholdBox;
    NumericBox BloomWindowSizeBox;
    CheckBox BloomHideBelowThresholdBox;

    bool Ready = false;

	public MapAfterEffectsWindow(Map Map)
	{
        this.Map = Map;

        MapContainer = new GroupBoxWithScrollBars(this);
        MapContainer.SetDocked(true);
        MapContainer.SetPadding(10, 40, 10, 180);

        Container ScrollContainer = new Container(MapContainer);
        ScrollContainer.SetDocked(true);
        ScrollContainer.SetPadding(2, 2, 15, 15);

        VScrollBar vs = new VScrollBar(MapContainer);
        vs.SetRightDocked(true);
        vs.SetVDocked(true);
        vs.SetPadding(0, 3, 1, 16);
        ScrollContainer.SetVScrollBar(vs);
        ScrollContainer.VAutoScroll = true;

        HScrollBar hs = new HScrollBar(MapContainer);
        hs.SetBottomDocked(true);
        hs.SetHDocked(true);
        hs.SetPadding(3, 0, 16, 1);
        ScrollContainer.SetHScrollBar(hs);
        ScrollContainer.HAutoScroll = true;

        MapBox = new ImageBox(ScrollContainer);
        MapBox.SetBitmap(Utilities.CreateSmallMapPreview(Map));
        MapBox.SetFillMode(FillMode.None);
        MapBox.SetZoomX(2);
        MapBox.SetZoomY(2);

        ScrollContainer.OnSizeChanged += _ =>
        {
            if (MapBox.Size.Width < ScrollContainer.Size.Width) MapBox.SetPosition(ScrollContainer.Size.Width / 2 - MapBox.Size.Width / 2, MapBox.Position.Y);
            if (MapBox.Size.Height < ScrollContainer.Size.Height) MapBox.SetPosition(MapBox.Position.X, ScrollContainer.Size.Height / 2 - MapBox.Size.Height / 2);
        };

        BloomContainer = new Container(this);
        BloomContainer.AutoResize = true;

        BloomCheckBox = new CheckBox(BloomContainer);
        BloomCheckBox.SetFont(Fonts.Paragraph);
        BloomCheckBox.SetText("Bloom Filter");

        Label BloomScaleLabel = new Label(BloomContainer);
        BloomScaleLabel.SetFont(Fonts.Paragraph);
        BloomScaleLabel.SetText("Scaling:");
        BloomScaleLabel.SetPosition(30, 24);

        Label BloomScalePercentageLabel = new Label(BloomContainer);
        BloomScalePercentageLabel.SetFont(Fonts.Paragraph);
        BloomScalePercentageLabel.SetText("%");
        BloomScalePercentageLabel.SetPosition(224, 24);

        BloomScaleBox = new NumericBox(BloomContainer);
        BloomScaleBox.SetWidth(100);
        BloomScaleBox.SetMinValue(0);
        BloomScaleBox.SetValue(75);
        BloomScaleBox.SetPosition(120, 20);

        Label BloomThresholdLabel = new Label(BloomContainer);
        BloomThresholdLabel.SetFont(Fonts.Paragraph);
        BloomThresholdLabel.SetText("Threshold:");
        BloomThresholdLabel.SetPosition(30, 54);

        Label BloomThresholdPercentageLabel = new Label(BloomContainer);
        BloomThresholdPercentageLabel.SetFont(Fonts.Paragraph);
        BloomThresholdPercentageLabel.SetText("%");
        BloomThresholdPercentageLabel.SetPosition(224, 54);

        BloomThresholdBox = new NumericBox(BloomContainer);
        BloomThresholdBox.SetWidth(100);
        BloomThresholdBox.SetMinValue(0);
        BloomThresholdBox.SetMaxValue(100);
        BloomThresholdBox.SetValue(90);
        BloomThresholdBox.SetPosition(120, 50);

        Label BloomWindowSizeLabel = new Label(BloomContainer);
        BloomWindowSizeLabel.SetFont(Fonts.Paragraph);
        BloomWindowSizeLabel.SetText("Window Size:");
        BloomWindowSizeLabel.SetPosition(30, 84);

        Label BloomWindowSizePixelsLabel = new Label(BloomContainer);
        BloomWindowSizePixelsLabel.SetFont(Fonts.Paragraph);
        BloomWindowSizePixelsLabel.SetText("px");
        BloomWindowSizePixelsLabel.SetPosition(224, 84);

        BloomWindowSizeBox = new NumericBox(BloomContainer);
        BloomWindowSizeBox.SetWidth(100);
        BloomWindowSizeBox.SetMinValue(1);
        BloomWindowSizeBox.SetValue(5);
        BloomWindowSizeBox.SetPosition(120, 80);

        BloomHideBelowThresholdBox = new CheckBox(BloomContainer);
        BloomHideBelowThresholdBox.SetFont(Fonts.Paragraph);
        BloomHideBelowThresholdBox.SetText("Hide below threshold");
        BloomHideBelowThresholdBox.SetMirrored(true);
        BloomHideBelowThresholdBox.SetPosition(30, 114);

        BloomCheckBox.OnCheckChanged += _ =>
        {
            BloomScaleLabel.SetEnabled(BloomCheckBox.Checked);
            BloomScaleBox.SetEnabled(BloomCheckBox.Checked);
            BloomScalePercentageLabel.SetEnabled(BloomCheckBox.Checked);
            BloomThresholdLabel.SetEnabled(BloomCheckBox.Checked);
            BloomThresholdBox.SetEnabled(BloomCheckBox.Checked);
            BloomThresholdPercentageLabel.SetEnabled(BloomCheckBox.Checked);
            BloomWindowSizeLabel.SetEnabled(BloomCheckBox.Checked);
            BloomWindowSizeBox.SetEnabled(BloomCheckBox.Checked);
            BloomWindowSizePixelsLabel.SetEnabled(BloomCheckBox.Checked);
            BloomHideBelowThresholdBox.SetEnabled(BloomCheckBox.Checked);
        };
        // Disable all bloom widgets
        BloomCheckBox.OnCheckChanged(new BaseEventArgs());

        CreateButton("Cancel", _ => Cancel());
        CreateButton("OK", _ => OK());
        CreateButton("Apply", _ => Apply());

        SetTitle("After Effects");
        MinimumSize = MaximumSize = new Size(1200, 1200);
        SetSize(MinimumSize);
        Center();
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        BloomContainer.SetPosition(20, Size.Height - 170);
    }

    private void Apply()
    {
        MapBox.DisposeBitmap();
        Bitmap NewBitmap = Utilities.CreateSmallMapPreview(this.Map);
        if (BloomCheckBox.Checked)
        {
            Bitmap Filtered;
            if (BloomHideBelowThresholdBox.Checked)
            {
                Filtered = NewBitmap.WithThreshold(BloomThresholdBox.Value / 100f);
                NewBitmap.Dispose();
                NewBitmap = Filtered.Blur(BloomWindowSizeBox.Value, BloomScaleBox.Value / 100f);
                Filtered.Dispose();
            }
            else
            {
                Filtered = NewBitmap.Bloom(BloomScaleBox.Value / 100f, BloomThresholdBox.Value / 100f, BloomWindowSizeBox.Value);
                NewBitmap.Dispose();
                NewBitmap = Filtered;
            }
        }
        MapBox.SetBitmap(NewBitmap);
    }

    private void OK()
    {
        Close();
    }

    private void Cancel()
    {
        Close();
    }
}
