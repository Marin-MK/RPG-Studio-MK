using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Widgets;

public class DropdownListWidget : DataListWidget
{
    protected DropdownBox DropdownBox;

    public DropdownListWidget(IContainer parent) : base(parent)
    {
        DropdownBox = new DropdownBox(this);
        DropdownBox.SetBottomDocked(true);
        DropdownBox.SetPadding(2, 0, 2, 38);
        DropdownBox.SetHDocked(true);

        GetListItemToAdd = e => e.Object = DropdownBox.SelectedItem;
    }

    public void SetAvailableItems(List<ListItem> items)
    {
        DropdownBox.SetItems(items);
    }
}
