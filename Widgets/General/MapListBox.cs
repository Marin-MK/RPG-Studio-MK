using System;
using System.Collections.Generic;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class MapListBox : ListBox
{
    public Map SelectedMap => (Map) SelectedItem?.Object;

    public BaseEvent OnMapChanged;

    public MapListBox(IContainer Parent) : base(Parent)
    {
        List<ListItem> Items = new List<ListItem>();
        foreach (KeyValuePair<int, Map> kvp in Data.Maps)
        {
            Items.Add(new ListItem($"{Utilities.Digits(kvp.Key, 3)}: {kvp.Value}", kvp.Value));
        }
        SetItems(Items);
        OnSelectionChanged += _ => OnMapChanged?.Invoke(new BaseEventArgs());
    }

    public void SetSelectedMap(Map Map)
    {
        int idx = Items.FindIndex(i => (Map)i.Object == Map);
        if (idx == -1) idx = 0;
        SetSelectedIndex(idx);
    }
}
