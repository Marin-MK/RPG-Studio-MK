using System;

namespace RPGStudioMK.Widgets;

public class DataTypeList : Widget
{
    Container ScrollContainer;
    VStackPanel StackPanel;

    DataTypeButton Species;
    DataTypeButton Moves;
    DataTypeButton Abilities;
    DataTypeButton Items;
    DataTypeButton TMs;
    DataTypeButton Tilesets;
    DataTypeButton Autotiles;
    DataTypeButton Types;
    DataTypeButton Trainers;
    DataTypeButton Animations;
    DataTypeButton System;

    public DataTypeList(IContainer Parent) : base(Parent)
    {
        Sprites["line"] = new Sprite(this.Viewport, new SolidBitmap(1, 1, new Color(28, 50, 73)));
        Sprites["line"].X = 181;
        ScrollContainer = new Container(this);
        ScrollContainer.SetPosition(0, 20);
        StackPanel = new VStackPanel(ScrollContainer);
        StackPanel.SetWidth(181);
        Species = new DataTypeButton(StackPanel);
        Species.SetType("species", "Species");
        Species.OnSelectionChanged += _ => { if (Species.Selected) Editor.SetDatabaseSubmode(DatabaseMode.Species); };
        Moves = new DataTypeButton(StackPanel);
        Moves.SetType("moves", "Moves");
        Moves.OnSelectionChanged += _ => { if (Moves.Selected) Editor.SetDatabaseSubmode(DatabaseMode.Moves); };
        Abilities = new DataTypeButton(StackPanel);
        Abilities.SetType("abilities", "Abilities");
        Abilities.OnSelectionChanged += _ => { if (Abilities.Selected) Editor.SetDatabaseSubmode(DatabaseMode.Abilities); };
        Items = new DataTypeButton(StackPanel);
        Items.SetType("items", "Items");
        Items.OnSelectionChanged += _ => { if (Items.Selected) Editor.SetDatabaseSubmode(DatabaseMode.Items); };
        TMs = new DataTypeButton(StackPanel);
        TMs.SetType("tms", "TMs & HMs");
        TMs.OnSelectionChanged += _ => { if (TMs.Selected) Editor.SetDatabaseSubmode(DatabaseMode.TMs); };
        Tilesets = new DataTypeButton(StackPanel);
        Tilesets.SetType("tilesets", "Tilesets");
        Tilesets.OnSelectionChanged += _ => { if (Tilesets.Selected) Editor.SetDatabaseSubmode(DatabaseMode.Tilesets); };
        Autotiles = new DataTypeButton(StackPanel);
        Autotiles.SetType("autotiles", "Autotiles");
        Autotiles.OnSelectionChanged += _ => { if (Autotiles.Selected) Editor.SetDatabaseSubmode(DatabaseMode.Autotiles); };
        Types = new DataTypeButton(StackPanel);
        Types.SetType("types", "Types");
        Types.OnSelectionChanged += _ => { if (Types.Selected) Editor.SetDatabaseSubmode(DatabaseMode.Types); };
        Trainers = new DataTypeButton(StackPanel);
        Trainers.SetType("trainers", "Trainers");
        Trainers.OnSelectionChanged += _ => { if (Trainers.Selected) Editor.SetDatabaseSubmode(DatabaseMode.Trainers); };
        Animations = new DataTypeButton(StackPanel);
        Animations.SetType("animations", "Animations");
        Animations.OnSelectionChanged += _ => { if (Animations.Selected) Editor.SetDatabaseSubmode(DatabaseMode.Animations); };
        System = new DataTypeButton(StackPanel);
        System.SetType("system", "System");
        System.OnSelectionChanged += _ => { if (System.Selected) Editor.SetDatabaseSubmode(DatabaseMode.System); };
        SetBackgroundColor(10, 23, 37);
    }

    public void SetSelected(DatabaseMode Mode)
    {
        switch (Mode)
        {
            case DatabaseMode.Species:
                Species.SetSelected(true);
                break;
            case DatabaseMode.Moves:
                Moves.SetSelected(true);
                break;
            case DatabaseMode.Abilities:
                Abilities.SetSelected(true);
                break;
            case DatabaseMode.Items:
                Items.SetSelected(true);
                break;
            case DatabaseMode.TMs:
                TMs.SetSelected(true);
                break;
            case DatabaseMode.Tilesets:
                Tilesets.SetSelected(true);
                break;
            case DatabaseMode.Autotiles:
                Autotiles.SetSelected(true);
                break;
            case DatabaseMode.Types:
                Types.SetSelected(true);
                break;
            case DatabaseMode.Trainers:
                Trainers.SetSelected(true);
                break;
            case DatabaseMode.Animations:
                Animations.SetSelected(true);
                break;
            case DatabaseMode.System:
                System.SetSelected(true);
                break;
            default:
                throw new Exception($"Invalid Mode: {Mode}");
        }
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        ScrollContainer.SetSize(Size.Width, Size.Height - 40);
        ((SolidBitmap)Sprites["line"].Bitmap).SetSize(1, Size.Height);
    }
}
