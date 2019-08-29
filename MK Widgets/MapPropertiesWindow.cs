﻿using System;
using ODL;

namespace MKEditor.Widgets
{
    public class MapPropertiesWindow : PopupWindow
    {
        public MapPropertiesWindow(object Parent, string Name = "mapPropertiesWindow")
            : base(Parent, Name)
        {
            this.SetName("New Map");
            this.SetSize(554, 474);
            this.Center();
            Label settings = new Label(this);
            settings.SetText("Settings");
            settings.SetFont(Font.Get("Fonts/Ubuntu-B", 14));
            settings.SetPosition(12, 22);
            ColoredBox sb1 = new ColoredBox(this);
            sb1.SetOuterColor(88, 83, 90);
            sb1.SetPosition(19, 43);
            sb1.SetSize(310, 203);
            ColoredBox sb2 = new ColoredBox(this);
            sb2.SetOuterColor(54, 52, 54);
            sb2.SetInnerColor(64, 60, 64);
            sb2.SetPosition(20, 44);
            sb2.SetSize(308, 201);
            NumericBox nb = new NumericBox(this);
            nb.SetPosition(120, 120);
            nb.SetSize(64, 27);
            TextBox tb = new TextBox(this);
            tb.SetPosition(120, 150);
            tb.SetSize(128, 27);
            CheckBox cb = new CheckBox(this);
            cb.SetPosition(120, 180);
            cb.SetSize(16, 16);
            cb.SetText("Autoplay BGM");
        }
    }
}
