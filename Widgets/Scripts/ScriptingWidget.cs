using System;
using System.Collections.Generic;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class ScriptingWidget : Widget
{
    ListBox ListBox;
    ScriptEditorTextBox TextBox;

    public ScriptingWidget(IContainer Parent) : base(Parent)
    {
        List<ListItem> Items = new List<ListItem>();
        for (int i = 0; i < Data.Scripts.Count; i++)
        {
            Items.Add(new ListItem(Data.Scripts[i].Name, Data.Scripts[i]));
        }

        ListBox = new ListBox(this);
        ListBox.SetVDocked(true);
        ListBox.SetWidth(192);
        ListBox.SetItems(Items);
        ListBox.OnSelectionChanged += _ => TextBox.SetText(((Script) ListBox.SelectedItem.Object).Content);

        TextBox = new ScriptEditorTextBox(this);
        TextBox.SetDocked(true);
        TextBox.SetPadding(192, 0, 0, 0);
        TextBox.OnTextChanged += _ =>
        {
            ((Script) ListBox.SelectedItem.Object).Content = TextBox.Text;
        };

        ListBox.SetSelectedIndex(0);
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        TextBox.UpdatePositionAndSizeIfDocked();
        TextBox.UpdateSize();
    }
}
