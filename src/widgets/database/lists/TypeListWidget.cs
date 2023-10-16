using RPGStudioMK.Game;
using System.Collections.Generic;
using System.Linq;

namespace RPGStudioMK.Widgets;

public class TypeListWidget : DropdownListWidget<TypeDropdownBox>
{
	public List<TypeResolver> AsResolvers => Items.Select(item => (TypeResolver) (Type) item.Object).ToList();
	public List<Type> AsTypes => Items.Select(item => (Type) item.Object).ToList();

    public TypeListWidget(IContainer parent) : base(parent)
    {
		SetAvailableItems(Data.Sources.Types);
	}

	public void SetItems(List<TypeResolver> items)
	{
		SetItems(items.Select(item => new TreeNode(item.Valid ? item.Type.Name : item.ID, item.Valid ? item.Type : item.ID)).ToList());
	}

	public void SetItems(List<Type> items)
	{
		SetItems(items.Select(item => new TreeNode(item.Name, item)).ToList());
	}
}
