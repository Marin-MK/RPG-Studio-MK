using System.Collections.Generic;
using System.Linq;

namespace RPGStudioMK.Widgets;

public class StringListWidget : DataListWidget
{
    public List<string> AsStrings => Items.Select(item => item.Text).ToList();

    protected TextBox TextBox;

    public StringListWidget(IContainer parent) : base(parent)
    {
        TextBox = new TextBox(this);
        TextBox.SetBottomDocked(true);
        TextBox.SetPadding(2, 0, 2, 38);
        TextBox.SetHeight(24);
        TextBox.SetHDocked(true);

        GetListItemToAdd = e => e.Object = new TreeNode(TextBox.Text);
    }

    public void SetItems(List<string> items)
    {
        SetItems(items.Select(item => new TreeNode(item)).ToList());
    }
}
