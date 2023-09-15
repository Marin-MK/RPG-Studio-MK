using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public partial class DataTypeTMs : GenericDataTypeBase<Item>
{
	public DataTypeTMs(IContainer Parent) : base(Parent)
	{
		this.Text = "TMs & HMs";
		this.DataType = BinaryData.TMS;
		this.DataSource = Data.TMsHMs;
		this.GetNodeDataSource = () => Data.Sources.TMsHMs;
		this.GetID = tm => tm.ID;
		this.SetID = (tm, id) => tm.ID = id;
		this.InvalidateData = Data.Sources.InvalidateTMs;
        this.GetLastID = () => Editor.ProjectSettings.LastTMID;
		this.SetLastID = id => Editor.ProjectSettings.LastTMID = id;
		this.GetLastScroll = () => Editor.ProjectSettings.LastTMScroll;
		this.SetLastScroll = (x) => Editor.ProjectSettings.LastTMScroll = x;
		this.Containers = new List<(string Name, string ID, System.Action<DataContainer, Item> CreateContainer, System.Func<Item, bool>? Condition)>()
		{
			("Main", "TM_MAIN", CreateMainContainer, null),
		};

		LateConstructor();
	}

	protected override Item CreateData()
	{
		Item item = Game.Item.CreateTM();
		foreach (TreeNode listItem in Data.Sources.Moves)
		{
			Move move = (Move) listItem.Object;
			if (Data.TMsHMs.Any(tm => tm.Value.Move.ID == move.ID)) continue;
			item.Move = (MoveResolver) move;
			break;
		}
		item.ID = "TM" + Utilities.Digits((int) GetFreeMachineNumber("TM", 0, 1), 2);
		item.Name = item.ID;
		item.Plural = item.Name + "s";
        return item;
	}

	protected override void NewData()
	{
		Item data = CreateData();
		DataSource.Add(GetID(data), data);
		InvalidateData();
		RedrawList(data);
	}

	protected override void PasteData()
	{
		if (base.DataList.HoveringItem is null || !Clipboard.IsValid(DataType)) return;
		List<Item> data = Clipboard.GetObject<List<Item>>();
		foreach (Item itm in data)
		{
			Match m = Regex.Match(itm.ID, @"(TM|TR|HM)(\d+)");
			if (m.Success) itm.ID = m.Groups[1].Value + Utilities.Digits((int) GetFreeMachineNumber(m.Groups[1].Value, Convert.ToInt32(m.Groups[2].Value) - 1, 1), 2);
			else itm.ID = "TM" + Utilities.Digits((int) GetFreeMachineNumber("TM", 0, 1), 2);
			itm.Name = itm.ID;
			itm.Plural = itm.Name + "s";
			DataSource.Add(itm.ID, itm);
		}
		InvalidateData();
		RedrawList(data[0]);
		base.DataList.UnlockGraphics();
		base.DataList.Root.GetAllChildren(true).ForEach(n =>
		{
			if (data.Contains((Item) ((TreeNode) n).Object)) base.DataList.SetSelectedNode((TreeNode) n, true, true, true, false);
		});
		base.DataList.LockGraphics();
	}
}
