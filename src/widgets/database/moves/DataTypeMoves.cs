using System.Collections.Generic;
using System.Linq;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public partial class DataTypeMoves : GenericDataTypeBase<Move>
{
    public DataTypeMoves(IContainer Parent) : base(Parent)
	{
		this.Text = "Moves";
		this.DataType = BinaryData.MOVES;
		this.DataSource = Data.Moves;
		this.GetNodeDataSource = () => Data.Sources.Moves;
		this.GetID = mov => mov.ID;
		this.SetID = (mov, id) => mov.ID = id;
		this.InvalidateData = Data.Sources.InvalidateMoves;
		this.GetLastID = () => Editor.ProjectSettings.LastMoveID;
		this.SetLastID = id => Editor.ProjectSettings.LastMoveID = id;
		this.GetLastScroll = () => Editor.ProjectSettings.LastMoveScroll;
		this.SetLastScroll = (x) => Editor.ProjectSettings.LastMoveScroll = x;
		this.Containers = new List<(string Name, string ID, System.Action<DataContainer, Move> CreateContainer, System.Func<Move, bool>? Condition)>()
		{
			("Main", "MOVES_MAIN", CreateMainContainer, null),
			("Description", "MOVES_DESC", CreateDescContainer, null),
			("Effect", "MOVES_EFFECT", CreateEffectContainer, null)
		};

		LateConstructor();
	}

	protected override Move CreateData()
	{
		Move move = Game.Move.Create();
		move.Name = "Missingno.";
		return move;
	}
}
