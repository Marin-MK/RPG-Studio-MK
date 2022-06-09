using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class FogFilePicker : AbstractFilePicker
{
    public string ChosenFogName;
    public int ChosenFogHue;
    public byte ChosenFogOpacity;
    public int ChosenFogBlendType;
    public int ChosenFogZoom;
    public int ChosenFogSX;
    public int ChosenFogSY;

    Label huelabel;
    Label opacitylabel;
    Label blendlabel;
    Label zoomlabel;
    Label sxlabel;
    Label sylabel;

    NumericSlider HueBox;
    NumericBox OpacityBox;
    DropdownBox BlendBox;
    NumericBox ZoomBox;
    NumericBox SXBox;
    NumericBox SYBox;

    public FogFilePicker(string FogName, int FogHue, byte FogOpacity, int FogBlendType, int FogZoom, int FogSX, int FogSY)
        : base("Fogs", Data.ProjectPath + "/Graphics/Fogs", FogName)
    {
        huelabel = new Label(this);
        huelabel.SetFont(Fonts.UbuntuBold.Use(11));
        huelabel.SetText("Hue");
        huelabel.SetPosition(199, Size.Height - 96);
        HueBox = new NumericSlider(this);
        HueBox.SetPosition(199, Size.Height - 74);
        HueBox.SetSize(previewbox.Size.Width, 17);
        HueBox.SetMinimumValue(0);
        HueBox.SetMaximumValue(359);
        HueBox.SetSnapValues(0, 59, 119, 179, 239, 299, 359);
        HueBox.SetPixelSnapDifference(16);
        HueBox.OnValueChanged += _ =>
        {
            UpdateHue();
        };
        HueBox.SetValue(FogHue);
        Font f = Fonts.UbuntuBold.Use(11);
        opacitylabel = new Label(this);
        opacitylabel.SetFont(f);
        opacitylabel.SetText("Opacity");
        OpacityBox = new NumericBox(this);
        OpacityBox.MinValue = 0;
        OpacityBox.MaxValue = 255;
        OpacityBox.SetSize(60, 27);
        OpacityBox.SetValue(FogOpacity);
        blendlabel = new Label(this);
        blendlabel.SetFont(f);
        blendlabel.SetText("Blending");
        BlendBox = new DropdownBox(this);
        BlendBox.SetItems(new List<ListItem>()
        {
            new ListItem("Normal"),
            new ListItem("Add"),
            new ListItem("Sub")
        });
        BlendBox.SetSize(80, 27);
        BlendBox.SetSelectedIndex(FogBlendType);
        zoomlabel = new Label(this);
        zoomlabel.SetFont(f);
        zoomlabel.SetText("Zoom");
        ZoomBox = new NumericBox(this);
        ZoomBox.MinValue = 0;
        ZoomBox.MaxValue = 99999;
        ZoomBox.SetSize(60, 27);
        ZoomBox.SetValue(FogZoom);
        sxlabel = new Label(this);
        sxlabel.SetFont(f);
        sxlabel.SetText("SX");
        SXBox = new NumericBox(this);
        SXBox.MinValue = 0;
        SXBox.MaxValue = 99999;
        SXBox.SetSize(60, 27);
        SXBox.SetValue(FogSX);
        sylabel = new Label(this);
        sylabel.SetFont(f);
        sylabel.SetText("SY");
        SYBox = new NumericBox(this);
        SYBox.MinValue = 0;
        SYBox.MaxValue = 99999;
        SYBox.SetSize(60, 27);
        SYBox.SetValue(FogSY);
        SetSize(700, 700);
        Center();
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        int decrx = 100;
        int decry = 56;
        previewbox.SetSize(previewbox.Size.Width - decrx, previewbox.Size.Height - decry);
        previewbox.Sprites["line1"].X -= decrx;
        previewbox.Sprites["line2"].Y -= decry;
        ((SolidBitmap)previewbox.Sprites["line1"].Bitmap).SetSize(1, ((SolidBitmap)previewbox.Sprites["line1"].Bitmap).BitmapHeight - decry);
        ((SolidBitmap)previewbox.Sprites["line2"].Bitmap).SetSize(((SolidBitmap)previewbox.Sprites["line2"].Bitmap).BitmapWidth - decrx, 1);
        previewbox.Sprites["box"].X -= decrx;
        previewbox.Sprites["box"].Y -= decry;
        scroll.SetSize(scroll.Size.Width - decrx, scroll.Size.Height - decry);
        scroll.VScrollBar.SetHeight(scroll.VScrollBar.Size.Height - decry);
        scroll.HScrollBar.SetWidth(scroll.HScrollBar.Size.Width - decrx);
        scroll.VScrollBar.SetPosition(scroll.VScrollBar.Position.X - decrx, 3);
        scroll.HScrollBar.SetPosition(3, scroll.HScrollBar.Position.Y - decry);
        huelabel?.SetPosition(199, Size.Height - decry - 40);
        HueBox?.SetPosition(199, Size.Height - 74);
        HueBox?.SetSize(previewbox.Size.Width + decrx, 17);
        int x = previewbox.Position.X + previewbox.Size.Width + 16;
        opacitylabel?.SetPosition(x, 46);
        OpacityBox?.SetPosition(x, 68);
        blendlabel?.SetPosition(x, 108);
        BlendBox?.SetPosition(x, 132);
        zoomlabel?.SetPosition(x, 172);
        ZoomBox?.SetPosition(x, 196);
        sxlabel?.SetPosition(x, 236);
        SXBox?.SetPosition(x, 260);
        sylabel?.SetPosition(x, 300);
        SYBox?.SetPosition(x, 324);
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
        ChosenFogName = List.SelectedItem.Object == null ? "" : List.SelectedItem.Name;
        ChosenFogHue = HueBox.Value;
        ChosenFogOpacity = (byte) OpacityBox.Value;
        ChosenFogBlendType = BlendBox.SelectedIndex;
        ChosenFogZoom = ZoomBox.Value;
        ChosenFogSX = SXBox.Value;
        ChosenFogSY = SYBox.Value;
        base.OK();
    }
}
