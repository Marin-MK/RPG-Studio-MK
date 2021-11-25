using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using amethyst;
using odl;

namespace RPGStudioMK.Widgets
{
    public class DataTypeSpecies : Widget
    {
        Grid Grid;
        DataTypeSubList SpeciesList;

        public int SelectedIndex { get { return SpeciesList.SelectedIndex; } }

        public DataTypeSpecies(IContainer Parent) : base(Parent)
        {
            Grid = new Grid(this);
            Grid.SetColumns(
                new GridSize(181, Unit.Pixels),
                new GridSize(1)
            );
            Container ListContainer = new Container(Grid);
            ListContainer.OnSizeChanged += delegate (BaseEventArgs e)
            {
                SpeciesList.SetSize(Size.Width, Size.Height);
            };
            SpeciesList = new DataTypeSubList(ListContainer);
            SpeciesList.SetBackgroundColor(28, 50, 73);

            List<ListItem> SpeciesItems = new List<ListItem>();
            List<string> Keys = Game.Data.Species.Keys.ToList();
            Keys.Sort(delegate (string Species1, string Species2)
            {
                int ID1 = Game.Data.Species[Species1].ID;
                int ID2 = Game.Data.Species[Species2].ID;
                if (ID1 > ID2) return 1;
                else if (ID1 < ID2) return -1;
                else return 0;
            });
            for (int i = 0; i < Keys.Count; i++)
            {
                Game.Species species = Game.Data.Species[Keys[i]];
                SpeciesItems.Add(new ListItem($"{Utilities.Digits(i + 1, 3)}: {species.Name}", species));
                for (int j = 1; j < species.Forms.Count; j++)
                {
                    Game.Species form = species.Forms[j];
                    SpeciesItems.Add(new ListItem($"            {Utilities.Digits(j, 2)}: {form.Name}", form));
                }
            }
            SpeciesList.SetItems(SpeciesItems);

            SubmodeView Tabs = new SubmodeView(Grid);
            Tabs.SetBackgroundColor(10, 23, 37);
            Tabs.SetTextY(8);
            Tabs.SetHeaderHeight(29);
            Tabs.SetHeaderSelHeight(0);
            Tabs.SetGridColumn(1);

            Container Main = Tabs.CreateTab("Main");
            VignetteFade MainFade = new VignetteFade(Main);
            Main.SetBackgroundColor(23, 40, 56);

            VStackPanel MainStackPanel = new VStackPanel(Main);
            Font f = Fonts.ProductSansMedium.Use(14);

            DataContainer MainContainer = new DataContainer(MainStackPanel);
            #region Name + Type + Ability
            MainContainer.SetText("Main");
            MainContainer.SetHeight(36 + 82 * 1);
            Grid MainContainerGrid = new Grid(MainContainer);
            MainContainerGrid.SetColumns(
                new GridSize(1),
                new GridSize(1),
                new GridSize(1)
            );
            MainContainerGrid.SetPosition(0, 36);

            Container NameContainer = new Container(MainContainerGrid);
            Label NameLabel = new Label(NameContainer);
            NameLabel.SetFont(f);
            NameLabel.SetText("Name");
            TextBox NameBox = new TextBox(NameContainer);
            Label IntNameLabel = new Label(NameContainer);
            IntNameLabel.SetFont(f);
            IntNameLabel.SetText("Internal Name");
            TextBox IntNameBox = new TextBox(NameContainer);
            NameContainer.OnSizeChanged += delegate (BaseEventArgs e)
            {
                if (Window.Width < 690)
                {
                    NameLabel.SetPosition(NameContainer.Size.Width / 2 - 47, 17);
                    NameBox.SetPosition(NameContainer.Size.Width / 2, 12);
                    NameBox.SetSize(102, 25);
                    IntNameLabel.SetPosition(NameContainer.Size.Width / 2 - 99, 50);
                    IntNameBox.SetPosition(NameContainer.Size.Width / 2, 45);
                    IntNameBox.SetSize(102, 25);
                }
                else
                {
                    NameLabel.SetPosition(NameContainer.Size.Width / 2 - 102, 17);
                    NameBox.SetPosition(NameContainer.Size.Width / 2 - 55, 12);
                    NameBox.SetSize(142, 25);
                    IntNameLabel.SetPosition(NameContainer.Size.Width / 2 - 154, 50);
                    IntNameBox.SetPosition(NameContainer.Size.Width / 2 - 55, 45);
                    IntNameBox.SetSize(142, 25);
                }
            };

            Container TypeContainer = new Container(MainContainerGrid);
            TypeContainer.SetGridColumn(1);
            Label Type1Label = new Label(TypeContainer);
            Type1Label.SetFont(f);
            Type1Label.SetText("Type 1");
            DropdownBox Type1Box = new DropdownBox(TypeContainer);
            Type1Box.SetReadOnly(true);
            Label Type2Label = new Label(TypeContainer);
            Type2Label.SetFont(f);
            Type2Label.SetText("Type 2");
            DropdownBox Type2Box = new DropdownBox(TypeContainer);
            Type2Box.SetReadOnly(true);
            TypeContainer.OnSizeChanged += delegate (BaseEventArgs e)
            {
                if (Window.Width < 690)
                {
                    Type1Label.SetPosition(TypeContainer.Size.Width / 2 - 47, 17);
                    Type1Box.SetPosition(TypeContainer.Size.Width / 2, 12);
                    Type1Box.SetSize(102, 25);
                    Type2Label.SetPosition(TypeContainer.Size.Width / 2 - 49, 50);
                    Type2Box.SetPosition(TypeContainer.Size.Width / 2, 45);
                    Type2Box.SetSize(102, 25);
                }
                else
                {
                    Type1Label.SetPosition(TypeContainer.Size.Width / 2 - 102, 17);
                    Type1Box.SetPosition(TypeContainer.Size.Width / 2 - 55, 12);
                    Type1Box.SetSize(142, 25);
                    Type2Label.SetPosition(TypeContainer.Size.Width / 2 - 104, 50);
                    Type2Box.SetPosition(TypeContainer.Size.Width / 2 - 55, 45);
                    Type2Box.SetSize(142, 25);
                }
            };

            Container AbilityContainer = new Container(MainContainerGrid);
            AbilityContainer.SetGridColumn(2);
            Label Ability1Label = new Label(AbilityContainer);
            Ability1Label.SetFont(f);
            Ability1Label.SetText("Ability 1");
            DropdownBox Ability1Box = new DropdownBox(AbilityContainer);
            Label Ability2Label = new Label(AbilityContainer);
            Ability2Label.SetFont(f);
            Ability2Label.SetText("Ability 2");
            DropdownBox Ability2Box = new DropdownBox(AbilityContainer);
            AbilityContainer.OnSizeChanged += delegate (BaseEventArgs e)
            {
                if (Window.Width < 690)
                {
                    Ability1Label.SetPosition(AbilityContainer.Size.Width / 2 - 55, 17);
                    Ability1Box.SetPosition(AbilityContainer.Size.Width / 2, 12);
                    Ability1Box.SetSize(102, 25);
                    Ability2Label.SetPosition(AbilityContainer.Size.Width / 2 - 57, 50);
                    Ability2Box.SetPosition(AbilityContainer.Size.Width / 2, 45);
                    Ability2Box.SetSize(102, 25);
                }
                else
                {
                    Ability1Label.SetPosition(AbilityContainer.Size.Width / 2 - 110, 17);
                    Ability1Box.SetPosition(AbilityContainer.Size.Width / 2 - 55, 12);
                    Ability1Box.SetSize(142, 25);
                    Ability2Label.SetPosition(AbilityContainer.Size.Width / 2 - 112, 50);
                    Ability2Box.SetPosition(AbilityContainer.Size.Width / 2 - 55, 45);
                    Ability2Box.SetSize(142, 25);
                }
            };

            MainContainer.OnSizeChanged += delegate (BaseEventArgs e)
            {
                MainContainerGrid.SetWidth(MainContainer.Size.Width);
                if (Window.Width < 1020)
                {
                    MainContainerGrid.SetHeight(82 * 3);
                    if (MainContainer.Size.Height != 36 + 82 * 3 && !MainContainer.Collapsed) MainContainer.SetHeight(36 + 82 * 3);
                    MainContainerGrid.SetColumns(new GridSize(1));
                    MainContainerGrid.SetRows(
                        new GridSize(1),
                        new GridSize(1),
                        new GridSize(1)
                    );
                    TypeContainer.SetGrid(1, 0);
                    AbilityContainer.SetGrid(2, 0);
                    MainContainerGrid.UpdateLayout();
                    MainStackPanel.UpdateLayout();
                }
                else if (Window.Width < 1350)
                {
                    MainContainerGrid.SetHeight(82 * 2);
                    if (MainContainer.Size.Height != 36 + 82 * 2 && !MainContainer.Collapsed) MainContainer.SetHeight(36 + 82 * 2);
                    MainContainerGrid.SetColumns(
                        new GridSize(1),
                        new GridSize(1)
                    );
                    MainContainerGrid.SetRows(
                        new GridSize(1),
                        new GridSize(1)
                    );
                    TypeContainer.SetGrid(0, 1);
                    AbilityContainer.SetGrid(1, 0);
                    MainContainerGrid.UpdateLayout();
                    MainStackPanel.UpdateLayout();
                }
                else
                {
                    MainContainerGrid.SetHeight(82 * 1);
                    if (MainContainer.Size.Height != 36 + 82 * 1 && !MainContainer.Collapsed) MainContainer.SetHeight(36 + 82 * 1);
                    MainContainerGrid.SetColumns(
                        new GridSize(1),
                        new GridSize(1),
                        new GridSize(1)
                    );
                    MainContainerGrid.SetRows(new GridSize(1));
                    TypeContainer.SetGrid(0, 1);
                    AbilityContainer.SetGrid(0, 2);
                    MainContainerGrid.UpdateLayout();
                    MainStackPanel.UpdateLayout();
                }
            };
            #endregion

            DataContainer StatsContainer = new DataContainer(MainStackPanel);
            StatsContainer.SetText("Stats");
            StatsContainer.SetHeight(36 + 116);
            Grid StatsContainerGrid = new Grid(StatsContainer);
            StatsContainerGrid.SetColumns(
                new GridSize(1),
                new GridSize(1)
            );
            StatsContainerGrid.SetPosition(0, 36);

            Container Stats1 = new Container(StatsContainerGrid);
            Label HPLabel = new Label(Stats1);
            HPLabel.SetFont(f);
            HPLabel.SetText("HP");
            NumericDataSlider HPSlider = new NumericDataSlider(Stats1);
            HPSlider.SetSlider(true);
            HPSlider.SetSize(244, 25);
            HPSlider.SetMaxValue(256);
            Label AttackLabel = new Label(Stats1);
            AttackLabel.SetFont(f);
            AttackLabel.SetText("Attack");
            NumericDataSlider AttackSlider = new NumericDataSlider(Stats1);
            AttackSlider.SetSlider(true);
            AttackSlider.SetSize(244, 25);
            AttackSlider.SetMaxValue(256);
            Label DefenseLabel = new Label(Stats1);
            DefenseLabel.SetFont(f);
            DefenseLabel.SetText("Defense");
            NumericDataSlider DefenseSlider = new NumericDataSlider(Stats1);
            DefenseSlider.SetSlider(true);
            DefenseSlider.SetSize(244, 25);
            DefenseSlider.SetMaxValue(256);
            Stats1.OnSizeChanged += delegate (BaseEventArgs e)
            {
                HPLabel.SetPosition(Stats1.Size.Width / 2 - 106, 17);
                HPSlider.SetPosition(Stats1.Size.Width / 2 - 80, 13);
                AttackLabel.SetPosition(Stats1.Size.Width / 2 - 129, 50);
                AttackSlider.SetPosition(Stats1.Size.Width / 2 - 80, 46);
                DefenseLabel.SetPosition(Stats1.Size.Width / 2 - 141, 83);
                DefenseSlider.SetPosition(Stats1.Size.Width / 2 - 80, 79);
            };

            Container Stats2 = new Container(StatsContainerGrid);
            Stats2.SetGridColumn(1);
            Label SpAtkLabel = new Label(Stats2);
            SpAtkLabel.SetFont(f);
            SpAtkLabel.SetText("Special Attack");
            NumericDataSlider SpAtkSlider = new NumericDataSlider(Stats2);
            SpAtkSlider.SetSlider(true);
            SpAtkSlider.SetSize(244, 25);
            SpAtkSlider.SetMaxValue(256);
            Label SpDefLabel = new Label(Stats2);
            SpDefLabel.SetFont(f);
            SpDefLabel.SetText("Special Defense");
            NumericDataSlider SpDefSlider = new NumericDataSlider(Stats2);
            SpDefSlider.SetSlider(true);
            SpDefSlider.SetSize(244, 25);
            SpDefSlider.SetMaxValue(256);
            Label SpeedLabel = new Label(Stats2);
            SpeedLabel.SetFont(f);
            SpeedLabel.SetText("Speed");
            NumericDataSlider SpeedSlider = new NumericDataSlider(Stats2);
            SpeedSlider.SetSlider(true);
            SpeedSlider.SetSize(244, 25);
            SpeedSlider.SetMaxValue(256);
            Stats2.OnSizeChanged += delegate (BaseEventArgs e)
            {
                SpAtkLabel.SetPosition(Stats2.Size.Width / 2 - 176, 17);
                SpAtkSlider.SetPosition(Stats2.Size.Width / 2 - 80, 13);
                SpDefLabel.SetPosition(Stats2.Size.Width / 2 - 188, 50);
                SpDefSlider.SetPosition(Stats2.Size.Width / 2 - 80, 46);
                SpeedLabel.SetPosition(Stats2.Size.Width / 2 - 128, 83);
                SpeedSlider.SetPosition(Stats2.Size.Width / 2 - 80, 79);
            };

            StatsContainer.OnSizeChanged += delegate (BaseEventArgs e)
            {
                StatsContainerGrid.SetWidth(StatsContainer.Size.Width);
            };

            Container Moves = Tabs.CreateTab("Moves");
            VignetteFade MovesFade = new VignetteFade(Moves);
            Moves.SetBackgroundColor(23, 40, 56);
            Container Evolutions = Tabs.CreateTab("Evolutions");
            VignetteFade EvolutionsFade = new VignetteFade(Evolutions);
            Evolutions.SetBackgroundColor(23, 40, 56);
            Container DexInfo = Tabs.CreateTab("Dex Info");
            VignetteFade DexInfoFade = new VignetteFade(DexInfo);
            DexInfo.SetBackgroundColor(23, 40, 56);
            Container Sprites = Tabs.CreateTab("Sprites");
            VignetteFade SpritesFade = new VignetteFade(Sprites);
            Sprites.SetBackgroundColor(23, 40, 56);

            SpeciesList.OnSelectionChanged += delegate (BaseEventArgs e)
            {
                Game.Species s = (Game.Species) SpeciesList.Items[SpeciesList.SelectedIndex].Object;
                NameBox.SetText(s.Name == null ? s.BaseForm.Name : s.Name);
                IntNameBox.SetText(s.IntName == null ? s.BaseForm.IntName : s.IntName);
                HPSlider.SetValue(s.Stats == null ? s.BaseForm.Stats.HP : s.Stats.HP);
                AttackSlider.SetValue(s.Stats == null ? s.BaseForm.Stats.Attack : s.Stats.Attack);
                DefenseSlider.SetValue(s.Stats == null ? s.BaseForm.Stats.Defense : s.Stats.Defense);
                SpAtkSlider.SetValue(s.Stats == null ? s.BaseForm.Stats.SpAtk : s.Stats.SpAtk);
                SpDefSlider.SetValue(s.Stats == null ? s.BaseForm.Stats.SpDef : s.Stats.SpDef);
                SpeedSlider.SetValue(s.Stats == null ? s.BaseForm.Stats.Speed : s.Stats.Speed);
            };
        }

        public void SetSelectedIndex(int SelectedIndex)
        {
            SpeciesList.SetSelectedIndex(SelectedIndex);
        }
    }
}
