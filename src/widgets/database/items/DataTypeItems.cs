using System.Collections.Generic;
using System.Linq;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public partial class DataTypeItems : GenericDataTypeBase<Item>
{
    public DataTypeItems(IContainer Parent) : base(Parent)
    {
        this.Text = "Items";
        this.DataType = BinaryData.ITEMS;
        this.DataSource = Data.Items;
        this.GetNodeDataSource = () => Data.Sources.Items;
        this.GetID = item => item.ID;
        this.SetID = (item, id) => item.ID = id;
        this.InvalidateData = Data.Sources.InvalidateItems;
        this.GetLastID = () => Editor.ProjectSettings.LastItemID;
        this.SetLastID = id => Editor.ProjectSettings.LastItemID = id;
        this.GetLastScroll = () => Editor.ProjectSettings.LastItemScroll;
        this.SetLastScroll = (x) => Editor.ProjectSettings.LastItemScroll = x;
        this.Containers = new List<(string Name, string ID, System.Action<DataContainer, Item> CreateContainer, System.Func<Item, bool>? Condition)>()
        {
            ("Main", "ITEM_MAIN", CreateMainContainer, null)
        };

        LateConstructor();
    }

    protected override Item CreateData()
    {
        Item item = Game.Item.Create();
        item.Name = "Missingno.";
        return item;
    }
}
