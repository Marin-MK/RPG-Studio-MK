﻿using System;
using System.Collections.Generic;

namespace RPGStudioMK.Widgets;

public class SetMaximumWindow : PopupWindow
{
    public bool PressedOK { get; protected set; } = false;
    public int Maximum { get; protected set; }

    public SetMaximumWindow(int InitialMaximum)
    {
        this.Maximum = InitialMaximum;

        SetTitle("Set Maximum");
        MinimumSize = MaximumSize = new Size(200, 134);
        SetSize(MaximumSize);
        Center();

        Label MaxLabel = new Label(this);
        MaxLabel.SetFont(Fonts.ProductSansMedium.Use(14));
        MaxLabel.SetText("Set maximum:");
        MaxLabel.SetPosition(Size.Width / 2 - MaxLabel.Size.Width / 2, 34);

        NumericBox maxBox = new NumericBox(this);
        maxBox.SetPosition(Size.Width / 2 - 48, 56);
        maxBox.SetSize(96, 27);
        maxBox.SetValue(InitialMaximum);
        maxBox.MinValue = 1;
        maxBox.MaxValue = 9999;
        maxBox.OnValueChanged += _ => Maximum = maxBox.Value;

        CreateButton("OK", _ => OK());
        CreateButton("Cancel", _ => Cancel());

        RegisterShortcuts(new List<Shortcut>()
        {
            new Shortcut(this, new Key(Keycode.ENTER, Keycode.CTRL), _ => OK(), true)
        });
    }

    void OK()
    {
        PressedOK = true;
        Close();
    }

    void Cancel()
    {
        Close();
    }
}
