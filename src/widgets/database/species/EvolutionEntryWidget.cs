using RPGStudioMK.Game;
using System.Linq;

namespace RPGStudioMK.Widgets;

public class EvolutionEntryWidget : Widget
{
    public bool Enabled { get; protected set; } = true;

    Evolution Evolution;

    SpeciesDropdownBox speciesBox;
    DropdownBox methodBox;
    Widget paramBox;

    Button RemoveButton;

    bool internalSwitch = false;
    bool isPrevo = false;
    Species currentSpecies;

    public BaseEvent OnButtonClicked;

    public EvolutionEntryWidget(bool isPrevo, Species currentSpecies, IContainer parent, int parentWidgetIndex = -1) : base(parent, parentWidgetIndex)
    {
        this.isPrevo = isPrevo;
        this.currentSpecies = currentSpecies;
        SetSize(900, 30);

        speciesBox = new SpeciesDropdownBox(false, this);
        speciesBox.SetWidth(280);
        speciesBox.SetPosition(0, 3);
        speciesBox.SetItems(
            isPrevo ?
				Data.Sources.SpeciesAndForms
                : 
				Data.Sources.SpeciesAndForms.FindAll(item => (Species) item.Object != currentSpecies)
        );
        speciesBox.OnSelectionChanged += _ =>
        {
            if (internalSwitch || isPrevo) return;
            if (this.Evolution.Species.Valid) this.Evolution.Species.Species.Prevolutions.RemoveAll(ev => ev.Species.Species == currentSpecies && ev.Type == this.Evolution.Type && ev.Parameter == this.Evolution.Parameter);
            this.Evolution.Species = speciesBox.Species;
            this.Evolution.Species.Species.Prevolutions.Add(new Evolution((SpeciesResolver) currentSpecies, this.Evolution.Type, this.Evolution.Parameter, true));
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
            if (oldTypeIndex != -1 && Data.HardcodedData.EvolutionMethodsAndTypes[typeIndex][1] == Data.HardcodedData.EvolutionMethodsAndTypes[oldTypeIndex][1]) return;
            object param = null;
            switch (typeIndex == -1 ? null : Data.HardcodedData.EvolutionMethodsAndTypes[typeIndex][1])
            {
                case "number":
                    param = 0L;
                    break;
                case "item":
                    param = ((Item) Data.Sources.Items[0].Object).ID;
                    break;
                case "move":
					param = ((Move) Data.Sources.Moves[0].Object).ID;
					break;
                case "species":
                    param = ((Species) Data.Sources.Species[0].Object).ID;
                    break;
                case "type":
                    param = ((Game.Type) Data.Sources.Types[0].Object).ID;
                    break;
                default: // string, none, and unknown methods
                    param = "";
                    break;
            }
            this.Evolution.Parameter = param;
            if (this.Evolution.Species.Valid)
            {
			    Evolution prevo = this.Evolution.Species.Species.Prevolutions.Find(ev => ev.Species.Species == currentSpecies);
                prevo.Type = this.Evolution.Type;
                prevo.Parameter = this.Evolution.Parameter;
            }
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
            internalSwitch = true;
            if (isPrevo && Base is not null && Base.Valid) speciesBox.SetSelectedIndex(speciesBox.Items.FindIndex(item => (Species) item.Object == Base.Species));
            else if (isPrevo && Base is not null) speciesBox.SetText(Base.ID);
			else if (evolution.Species.Valid) speciesBox.SetSelectedIndex(speciesBox.Items.FindIndex(item => (Species) item.Object == evolution.Species.Species));
            else speciesBox.SetText(evolution.Species.ID);
            if (Data.HardcodedData.EvolutionMethods.Contains(evolution.Type))
            {
                int typeIndex = Data.HardcodedData.EvolutionMethods.IndexOf(evolution.Type);
                methodBox.SetSelectedIndex(typeIndex);
            }
            else methodBox.SetText(evolution.Type);
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
        switch (typeIndex == -1 ? null : Data.HardcodedData.EvolutionMethodsAndTypes[typeIndex][1])
        {
            case "number":
                paramBox = new NumericBox(this);
                if (Evolution.Parameter is not null) ((NumericBox) paramBox).SetValue((int) (long) Evolution.Parameter);
                ((NumericBox) paramBox).OnValueChanged += _ =>
                {
                    Evolution.Parameter = (long) ((NumericBox) paramBox).Value;
                    if (this.Evolution.Species.Valid)
                    {
					    Evolution prevo = this.Evolution.Species.Species.Prevolutions.Find(ev => ev.Species.Species == currentSpecies);
					    prevo.Parameter = this.Evolution.Parameter;
                    }
				};
                ((NumericBox) paramBox).SetShowDisabledText(true);
				paramBox.SetHeight(30);
				paramBox.SetPosition(580, 0);
				break;
            case "item":
                ItemResolver itemParam = new ItemResolver((string) Evolution.Parameter);
                paramBox = new ItemDropdownBox(this);
                ((ItemDropdownBox) paramBox).SetItem(itemParam);
                ((ItemDropdownBox) paramBox).OnItemChanged += e =>
                {
                    Evolution.Parameter = e.Object.ID;
					if (this.Evolution.Species.Valid)
					{
						Evolution prevo = this.Evolution.Species.Species.Prevolutions.Find(ev => ev.Species.Species == currentSpecies);
						prevo.Parameter = this.Evolution.Parameter;
					}
				};
                ((DropdownBox) paramBox).SetShowDisabledText(true);
				paramBox.SetHeight(26);
				paramBox.SetPosition(580, 3);
				break;
            case "move":
                MoveResolver moveParam = new MoveResolver((string) Evolution.Parameter);
                paramBox = new MoveDropdownBox(this);
                ((MoveDropdownBox) paramBox).SetMove(moveParam);
                ((MoveDropdownBox) paramBox).OnMoveChanged += e =>
                {
                    Evolution.Parameter = e.Object.ID;
					if (this.Evolution.Species.Valid)
					{
						Evolution prevo = this.Evolution.Species.Species.Prevolutions.Find(ev => ev.Species.Species == currentSpecies);
						prevo.Parameter = this.Evolution.Parameter;
					}
				};
				((DropdownBox) paramBox).SetShowDisabledText(true);
				paramBox.SetHeight(26);
				paramBox.SetPosition(580, 3);
				break;
            case "species":
                SpeciesResolver speciesParam = new SpeciesResolver((string) Evolution.Parameter);
                paramBox = new SpeciesDropdownBox(true, this);
                ((SpeciesDropdownBox) paramBox).SetSpecies(speciesParam);
                ((SpeciesDropdownBox) paramBox).OnSpeciesChanged += e =>
                {
                    Evolution.Parameter = e.Object.ID;
					if (this.Evolution.Species.Valid)
					{
						Evolution prevo = this.Evolution.Species.Species.Prevolutions.Find(ev => ev.Species.Species == currentSpecies);
						prevo.Parameter = this.Evolution.Parameter;
					}
				};
				((DropdownBox) paramBox).SetShowDisabledText(true);
				paramBox.SetHeight(26);
				paramBox.SetPosition(580, 3);
				break;
            case "type":
                TypeResolver typeParam = new TypeResolver((string) Evolution.Parameter);
                paramBox = new TypeDropdownBox(this);
                ((TypeDropdownBox) paramBox).SetItems(Data.Sources.Types);
                ((TypeDropdownBox) paramBox).SetType(typeParam);
                ((TypeDropdownBox) paramBox).OnTypeChanged += e =>
                {
                    Evolution.Parameter = e.Object.ID;
					if (this.Evolution.Species.Valid)
					{
						Evolution prevo = this.Evolution.Species.Species.Prevolutions.Find(ev => ev.Species.Species == currentSpecies);
						prevo.Parameter = this.Evolution.Parameter;
					}
				};
				((DropdownBox) paramBox).SetShowDisabledText(true);
				paramBox.SetHeight(26);
				paramBox.SetPosition(580, 3);
				break;
            case "none":
                break;
            default: // string and unknown methods
                paramBox = new TextBox(this);
                ((TextBox) paramBox).SetText((string) Evolution.Parameter);
                ((TextBox) paramBox).OnTextChanged += _ =>
                {
                    Evolution.Parameter = ((TextBox) paramBox).Text;
                    if (this.Evolution.Species.Valid)
                    {
					    Evolution prevo = this.Evolution.Species.Species.Prevolutions.Find(ev => ev.Species.Species == currentSpecies);
					    prevo.Parameter = this.Evolution.Parameter;
                    }
				};
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