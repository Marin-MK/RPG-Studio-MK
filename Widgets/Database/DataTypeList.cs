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
        Moves = new DataTypeButton(StackPanel);
        Moves.SetType("moves", "Moves");
        Abilities = new DataTypeButton(StackPanel);
        Abilities.SetType("abilities", "Abilities");
        Items = new DataTypeButton(StackPanel);
        Items.SetType("items", "Items");
        TMs = new DataTypeButton(StackPanel);
        TMs.SetType("tms", "TMs & HMs");
        Tilesets = new DataTypeButton(StackPanel);
        Tilesets.SetType("tilesets", "Tilesets");
        Autotiles = new DataTypeButton(StackPanel);
        Autotiles.SetType("autotiles", "Autotiles");
        Types = new DataTypeButton(StackPanel);
        Types.SetType("types", "Types");
        Trainers = new DataTypeButton(StackPanel);
        Trainers.SetType("trainers", "Trainers");
        Animations = new DataTypeButton(StackPanel);
        Animations.SetType("animations", "Animations");
        System = new DataTypeButton(StackPanel);
        System.SetType("system", "System");
    }

    public void SetSelected(string Mode)
    {
        if (Mode == "species") Species.SetSelected(true);
        else if (Mode == "moves") Moves.SetSelected(true);
        else if (Mode == "abilities") Abilities.SetSelected(true);
        else if (Mode == "items") Items.SetSelected(true);
        else if (Mode == "tms") TMs.SetSelected(true);
        else if (Mode == "tilesets") Tilesets.SetSelected(true);
        else if (Mode == "autotiles") Autotiles.SetSelected(true);
        else if (Mode == "types") Types.SetSelected(true);
        else if (Mode == "trainers") Trainers.SetSelected(true);
        else if (Mode == "animations") Animations.SetSelected(true);
        else if (Mode == "system") System.SetSelected(true);
        else throw new Exception($"Invalid Mode: {Mode}");
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        ScrollContainer.SetSize(Size.Width, Size.Height - 40);
        ((SolidBitmap)Sprites["line"].Bitmap).SetSize(1, Size.Height);
    }
}
