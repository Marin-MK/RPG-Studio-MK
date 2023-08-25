using System;
using System.Collections.Generic;
using System.Linq;

namespace RPGStudioMK.Widgets;

public class DropdownListWidget<T> : DataListWidget where T : DropdownBox
{
    protected T DropdownBox;

    public DropdownListWidget(IContainer parent, object[]? args = null) : base(parent) 
    {
        List<object> ctorArgs = args?.ToList() ?? new List<object>();
        ctorArgs.Add(this);
        DropdownBox = (T) Activator.CreateInstance(typeof(T), ctorArgs.ToArray());
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
