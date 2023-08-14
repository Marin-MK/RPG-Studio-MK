using RPGStudioMK.Game;
using System.Linq;

namespace RPGStudioMK.Widgets;

public class EvolutionEntryWidget : Widget
{
    public bool Enabled { get; protected set; } = true;

    Evolution Evolution;

    DropdownBox speciesBox;
    DropdownBox methodBox;
    Widget paramBox;

    Button RemoveButton;

    bool internalSwitch = false;
    bool isPrevo = false;

    public BaseEvent OnButtonClicked;

    public EvolutionEntryWidget(bool isPrevo, Species currentSpecies, IContainer parent, int parentWidgetIndex = -1) : base(parent, parentWidgetIndex)
    {
        this.isPrevo = isPrevo;
        SetSize(900, 30);

        speciesBox = new DropdownBox(this);
        speciesBox.SetWidth(280);
        speciesBox.SetPosition(0, 3);
        speciesBox.SetItems(
            isPrevo ?
				Data.Sources.SpeciesAndFormsListItemsAlphabetical
                : 
				Data.Sources.SpeciesAndFormsListItemsAlphabetical.FindAll(item => (Species) item.Object != currentSpecies)
        );
        speciesBox.OnSelectionChanged += _ =>
        {
            if (isPrevo) return;
            this.Evolution.Species = (SpeciesResolver)(Species)speciesBox.SelectedItem.Object;
        };
		speciesBox.SetShowDisabledText(true);

		methodBox = new DropdownBox(this);
        methodBox.SetPosition(290, 3);
        methodBox.SetWidth(280);
        methodBox.SetItems(Data.HardcodedData.EvolutionMethods.Select(m => new ListItem(m)).ToList());
        methodBox.OnSelectionChanged += _ =>
        {
            if (internalSwitch || isPrevo) return;
            int typeIndex = methodBox.SelectedIndex;
            int oldTypeIndex = Data.HardcodedData.EvolutionMethods.IndexOf(this.Evolution.Type);
            this.Evolution.Type = Data.HardcodedData.EvolutionMethods[typeIndex];
            // Only reset the parameter if the type of the parameter has changed, e.g. number to string
            if (Data.HardcodedData.EvolutionMethodsAndTypes[typeIndex][1] == Data.HardcodedData.EvolutionMethodsAndTypes[oldTypeIndex][1]) return;
            object param = null;
            switch (Data.HardcodedData.EvolutionMethodsAndTypes[typeIndex][1])
            {
                case "number":
                    param = 0L;
                    break;
                case "item":
                    param = ((Item) Data.Sources.ItemsListItemsAlphabetical[0].Object).ID;
                    break;
                case "move":
					param = ((Move) Data.Sources.MovesListItemsAlphabetical[0].Object).ID;
					break;
                case "species":
                    param = ((Species) Data.Sources.SpeciesListItemsAlphabetical[0].Object).ID;
                    break;
                case "type":
                    param = ((Game.Type) Data.Sources.TypesListItemsAlphabetical[0].Object).ID;
                    break;
                default: // string, none, and unknown methods
                    param = "";
                    break;
            }
            this.Evolution.Parameter = param;
            UpdateParamBox();
        };
		methodBox.SetShowDisabledText(true);

		RemoveButton = new Button(this);
        RemoveButton.SetSize(30, 30);
        RemoveButton.SetText("X");
        RemoveButton.SetRightDocked(true);
        RemoveButton.OnClicked += _ => OnButtonClicked?.Invoke(new BaseEventArgs());
    }

    public void SetEvolution(Evolution evolution, SpeciesResolver? Base = null)
    {
        if (this.Evolution != evolution)
        {
            this.Evolution = evolution;
            if (isPrevo && Base is not null) speciesBox.SetSelectedIndex(speciesBox.Items.FindIndex(item => (Species) item.Object == Base.Species));
			else speciesBox.SetSelectedIndex(speciesBox.Items.FindIndex(item => (Species) item.Object == evolution.Species.Species));
            int typeIndex = Data.HardcodedData.EvolutionMethods.IndexOf(evolution.Type);
            internalSwitch = true;
			methodBox.SetSelectedIndex(typeIndex);
            internalSwitch = false;
            UpdateParamBox();
        }
    }

    public void SetEnabled(bool enabled)
    {
        if (this.Enabled != enabled)
        {
            this.Enabled = enabled;
            speciesBox.SetEnabled(this.Enabled);
			methodBox.SetEnabled(this.Enabled);
            if (paramBox is DropdownBox) ((DropdownBox) paramBox).SetEnabled(this.Enabled);
            else if (paramBox is NumericBox) ((NumericBox) paramBox).SetEnabled(this.Enabled);
            else if (paramBox is TextBox) ((TextBox) paramBox).SetEnabled(this.Enabled);
            RemoveButton.SetEnabled(this.Enabled);
        }
    }

    void UpdateParamBox()
    {
		int typeIndex = Data.HardcodedData.EvolutionMethods.IndexOf(Evolution.Type);
		paramBox?.Dispose();
        paramBox = null;
        switch (Data.HardcodedData.EvolutionMethodsAndTypes[typeIndex][1])
        {
            case "number":
                paramBox = new NumericBox(this);
                if (Evolution.Parameter is not null) ((NumericBox) paramBox).SetValue((int) (long) Evolution.Parameter);
                ((NumericBox) paramBox).OnValueChanged += _ => Evolution.Parameter = (long) ((NumericBox) paramBox).Value;
                ((NumericBox) paramBox).SetShowDisabledText(true);
				paramBox.SetHeight(30);
				paramBox.SetPosition(580, 0);
				break;
            case "item":
                ItemResolver itemParam = new ItemResolver((string) Evolution.Parameter);
                paramBox = new DropdownBox(this);
                ((DropdownBox) paramBox).SetItems(Data.Sources.ItemsListItemsAlphabetical);
                ((DropdownBox) paramBox).SetSelectedIndex(Data.Sources.ItemsListItemsAlphabetical.FindIndex(item => (Item) item.Object == itemParam?.Item));
                ((DropdownBox) paramBox).OnSelectionChanged += _ => Evolution.Parameter = ((Item) ((DropdownBox) paramBox).SelectedItem.Object).ID;
                ((DropdownBox) paramBox).SetShowDisabledText(true);
				paramBox.SetHeight(26);
				paramBox.SetPosition(580, 3);
				break;
            case "move":
                MoveResolver moveParam = new MoveResolver((string) Evolution.Parameter);
                paramBox = new DropdownBox(this);
                ((DropdownBox) paramBox).SetItems(Data.Sources.MovesListItemsAlphabetical);
                ((DropdownBox) paramBox).SetSelectedIndex(Data.Sources.MovesListItemsAlphabetical.FindIndex(item => (Move) item.Object == moveParam?.Move));
                ((DropdownBox) paramBox).OnSelectionChanged += _ => Evolution.Parameter = ((Move) ((DropdownBox) paramBox).SelectedItem.Object).ID;
				((DropdownBox) paramBox).SetShowDisabledText(true);
				paramBox.SetHeight(26);
				paramBox.SetPosition(580, 3);
				break;
            case "species":
                SpeciesResolver speciesParam = new SpeciesResolver((string) Evolution.Parameter);
                paramBox = new DropdownBox(this);
                ((DropdownBox) paramBox).SetItems(Data.Sources.SpeciesAndFormsListItemsAlphabetical);
                ((DropdownBox) paramBox).SetSelectedIndex(Data.Sources.SpeciesAndFormsListItemsAlphabetical.FindIndex(item => (Species) item.Object == speciesParam?.Species));
                ((DropdownBox) paramBox).OnSelectionChanged += _ => Evolution.Parameter = ((Species) ((DropdownBox) paramBox).SelectedItem.Object).ID;
				((DropdownBox) paramBox).SetShowDisabledText(true);
				paramBox.SetHeight(26);
				paramBox.SetPosition(580, 3);
				break;
            case "type":
                TypeResolver typeParam = new TypeResolver((string) Evolution.Parameter);
                paramBox = new DropdownBox(this);
                ((DropdownBox) paramBox).SetItems(Data.Sources.TypesListItemsAlphabetical);
                ((DropdownBox) paramBox).SetSelectedIndex(Data.Sources.TypesListItemsAlphabetical.FindIndex(item => (Game.Type) item.Object == typeParam?.Type));
                ((DropdownBox) paramBox).OnSelectionChanged += _ => Evolution.Parameter = ((Game.Type) ((DropdownBox) paramBox).SelectedItem.Object).ID;
				((DropdownBox) paramBox).SetShowDisabledText(true);
				paramBox.SetHeight(26);
				paramBox.SetPosition(580, 3);
				break;
            case "none":
                break;
            default: // string and unknown methods
                paramBox = new TextBox(this);
                ((TextBox) paramBox).SetText((string) Evolution.Parameter);
                ((TextBox) paramBox).OnTextChanged += _ => Evolution.Parameter = ((TextBox) paramBox).Text;
                ((TextBox) paramBox).SetShowDisabledText(true);
				paramBox.SetHeight(26);
				paramBox.SetPosition(580, 3);
				break;
        }
        if (paramBox is not null)
        {
            paramBox.SetWidth(280);
        }
    }
}