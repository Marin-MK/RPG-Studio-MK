﻿using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class PanoramaFilePicker : AbstractFilePicker
{
    public string ChosenPanoramaName;
    public int ChosenPanoramaHue;

    Label huelabel;
    NumericSlider HueBox;

    public PanoramaFilePicker(string PanoramaName, int PanoramaHue)
        : base("Panoramas", Data.ProjectPath + "/Graphics/Panoramas", PanoramaName)
    {
        huelabel = new Label(this);
        huelabel.SetText("Hue");
        huelabel.SetPosition(199, Size.Height - 96);
        huelabel.SetFont(Fonts.Paragraph);
        HueBox = new NumericSlider(this);
        HueBox.SetPosition(199, Size.Height - 74);
        HueBox.SetSize(previewbox.Size.Width, 17);
        HueBox.SetMinimumValue(0);
        HueBox.SetMaximumValue(359);
        HueBox.SetSnapValues(0, 59, 119, 179, 239, 299, 359);
        HueBox.SetSnapStrength(16);
        HueBox.OnValueChanged += _ =>
        {
            UpdateHue();
        };
        HueBox.SetValue(PanoramaHue);
        SetSize(700, 700);
        Center();
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        int decry = 56;
        previewbox.SetHeight(previewbox.Size.Height - decry);
        previewbox.Sprites["line2"].Y -= decry;
        ((SolidBitmap)previewbox.Sprites["line1"].Bitmap).SetSize(1, ((SolidBitmap) previewbox.Sprites["line1"].Bitmap).BitmapHeight - decry);
        previewbox.Sprites["box"].Y -= decry;
        scroll.SetHeight(scroll.Size.Height - decry);
        scroll.VScrollBar.SetHeight(scroll.VScrollBar.Size.Height - decry);
        scroll.HScrollBar.SetPosition(3, scroll.HScrollBar.Position.Y - decry);
        huelabel?.SetPosition(199, Size.Height - decry - 40);
        HueBox?.SetPosition(199, Size.Height - 74);
        HueBox?.SetSize(previewbox.Size.Width, 17);
    }

    public override void UpdatePreview()
    {
        base.UpdatePreview();
        UpdateHue();
    }

    void UpdateHue()
    {
        if (CurrentBitmap == null || HueBox == null) return;
        if (image.Bitmap != CurrentBitmap) image.DisposeBitmap();
        image.SetBitmap(HueBox.Value == 0 ? CurrentBitmap : CurrentBitmap.ApplyHue(HueBox.Value));
    }

    public override void OK()
    {
        ChosenPanoramaName = List.SelectedItem.Object == null ? "" : List.SelectedItem.Text;
        ChosenPanoramaHue = HueBox.Value;
        base.OK();
    }
}
