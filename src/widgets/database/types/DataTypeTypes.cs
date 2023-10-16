using System.Collections.Generic;
using System.Linq;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public partial class DataTypeTypes : GenericDataTypeBase<Type>
{
	public DataTypeTypes(IContainer Parent) : base(Parent)
	{
		this.Text = "Types";
		this.DataType = BinaryData.TYPES;
		this.DataSource = Data.Types;
		this.GetNodeDataSource = () => Data.Sources.Types;
		this.GetID = tp => tp.ID;
		this.SetID = (tp, id) => tp.ID = id;
		this.InvalidateData = Data.Sources.InvalidateTypes;
		this.GetLastID = () => Editor.ProjectSettings.LastTypeID;
		this.SetLastID = id => Editor.ProjectSettings.LastTypeID = id;
		this.GetLastScroll = () => Editor.ProjectSettings.LastTypeScroll;
		this.SetLastScroll = (x) => Editor.ProjectSettings.LastTypeScroll = x;
		this.Containers = new List<(string Name, string ID, System.Action<DataContainer, Type> CreateContainer, System.Func<Type, bool>? Condition)>()
		{
			("Info", "TYPE_INFO", CreateMainContainer, null),
			("Defensive Relations", "TYPE_DEFENSIVE_RELATIONS", CreateRelationsContainer, null),
			("Flags", "TYPE_FLAGS", CreateFlagsContainer, null)
		};

		LateConstructor();
	}

	protected override Type CreateData()
	{
		Type tp = Type.Create();
		tp.Name = "Missingno.";
		return tp;
	}
}
