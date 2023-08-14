using RPGStudioMK.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Widgets;

public class ItemListWidget : DropdownListWidget
{
	public List<ItemResolver> AsResolvers => Items.Select(item => (ItemResolver) (Item) item.Object).ToList();
	public List<Item> AsItems => Items.Select(item => (Item) item.Object).ToList();

    public ItemListWidget(IContainer parent) : base(parent)
    {
		SetAvailableItems(Data.Sources.ItemsListItemsAlphabetical);
	}

	public void SetItems(List<ItemResolver> items)
	{
		SetItems(items.Select(item => new ListItem(item.Item.Name, item.Item)).ToList());
	}

	public void SetItems(List<Item> items)
	{
		SetItems(items.Select(item => new ListItem(item.Name, item)).ToList());
	}
}
