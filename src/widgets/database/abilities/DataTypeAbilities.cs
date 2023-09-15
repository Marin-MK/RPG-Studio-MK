using System.Collections.Generic;
using System.Linq;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public partial class DataTypeAbilities : GenericDataTypeBase<Ability>
{
	public DataTypeAbilities(IContainer Parent) : base(Parent)
	{
		this.Text = "Abilities";
		this.DataType = BinaryData.ABILITIES;
		this.DataSource = Data.Abilities;
		this.GetNodeDataSource = () => Data.Sources.Abilities;
		this.GetID = abil => abil.ID;
		this.SetID = (abil, id) => abil.ID = id;
		this.InvalidateData = Data.Sources.InvalidateAbilities;
		this.GetLastID = () => Editor.ProjectSettings.LastAbilityID;
		this.SetLastID = id => Editor.ProjectSettings.LastAbilityID = id;
		this.GetLastScroll = () => Editor.ProjectSettings.LastAbilityScroll;
		this.SetLastScroll = (x) => Editor.ProjectSettings.LastAbilityScroll = x;
		this.Containers = new List<(string Name, string ID, System.Action<DataContainer, Ability> CreateContainer, System.Func<Ability, bool>? Condition)>()
		{
			("Main", "ABILITY_MAIN", CreateMainContainer, null)
		};

		LateConstructor();
	}

	protected override Ability CreateData()
	{
		Ability abil = Game.Ability.Create();
		abil.Name = "Missingno.";
		return abil;
	}
}
