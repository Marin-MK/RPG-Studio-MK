using System;
using System.Collections.Generic;

namespace RPGStudioMK.Widgets;

public class DataTypeList : Widget
{
    public static List<(string Name, string IconFilename, DatabaseMode Mode)> DataTypes = new List<(string Name, string IconFilename, DatabaseMode Mode)>()
    {
        ("Species", "species", DatabaseMode.Species),
        ("Moves", "moves", DatabaseMode.Moves),
        ("Abilities", "abilities", DatabaseMode.Abilities),
        ("Items", "items", DatabaseMode.Items),
        ("TMs & HMs", "tms", DatabaseMode.TMs),
        ("Types", "types", DatabaseMode.Types),
        ("Trainers", "trainers", DatabaseMode.Trainers),
        ("Pokedex", "dexes", DatabaseMode.Dexes),
        ("Tilesets", "tilesets", DatabaseMode.Tilesets),
        ("Common Events", "common_events", DatabaseMode.CommonEvents),
        ("Animations", "animations", DatabaseMode.Animations),
        ("System", "system", DatabaseMode.System)
    };

    Container ScrollContainer;
    VStackPanel StackPanel;
    Dictionary<DatabaseMode, DataTypeButton> ModeButtonRegistry = new Dictionary<DatabaseMode, DataTypeButton>();

    public DataTypeList(IContainer Parent) : base(Parent)
    {
        Sprites["line"] = new Sprite(this.Viewport, new SolidBitmap(1, 1, new Color(28, 50, 73)));
        Sprites["line"].X = 198;
        ScrollContainer = new Container(this);
        ScrollContainer.SetPosition(0, 20);
        StackPanel = new VStackPanel(ScrollContainer);
        StackPanel.SetWidth(198);

        foreach (var type in DataTypes)
        {
            DataTypeButton btn = new DataTypeButton(StackPanel);
            btn.SetType(type.IconFilename, type.Name);
            btn.OnSelectionChanged += _ =>
            {
                if (btn.Selected) Editor.SetDatabaseSubmode(type.Mode);
            };
            ModeButtonRegistry.Add(type.Mode, btn);
        }

        SetBackgroundColor(10, 23, 37);
    }

    public void SetSelected(DatabaseMode Mode)
    {
        if (!ModeButtonRegistry.ContainsKey(Mode)) throw new Exception($"Unknown Database Mode: {Mode}");
        ModeButtonRegistry[Mode].SetSelected(true);
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        ScrollContainer.SetSize(Size.Width, Size.Height - 40);
        ((SolidBitmap) Sprites["line"].Bitmap).SetSize(1, Size.Height);
    }
}
