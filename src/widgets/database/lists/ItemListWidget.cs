﻿using RPGStudioMK.Game;
using System.Collections.Generic;
using System.Linq;

namespace RPGStudioMK.Widgets;

public class ItemListWidget : DropdownListWidget<ItemDropdownBox>
{
	public List<ItemResolver> AsResolvers => Items.Select(item => (ItemResolver) (Item) item.Object).ToList();
	public List<Item> AsItems => Items.Select(item => (Item) item.Object).ToList();

    public ItemListWidget(IContainer parent) : base(parent)
    {
		SetAvailableItems(Data.Sources.Items);
	}

	public void SetItems(List<ItemResolver> items)
	{
		SetItems(items.Select(item => new TreeNode(item.Valid ? item.Item.Name : item.ID, item.Valid ? item.Item : item.ID)).ToList());
	}

	public void SetItems(List<Item> items)
	{
		SetItems(items.Select(item => new TreeNode(item.Name, item)).ToList());
	}
}
