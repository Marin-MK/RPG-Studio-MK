using System.Collections.Generic;

namespace RPGStudioMK.Widgets;

public abstract class DataListWidget : Widget
{
    public List<TreeNode> Items => ListBox.Items;

    protected Label HeaderLabel;
    protected ListBox ListBox;
    protected Button AddButton;
    protected Button RemoveButton;
    protected bool init = true;

    public GenericObjectEvent<TreeNode> OnItemAdded;
    public GenericObjectEvent<TreeNode> OnItemRemoved;
    public BaseEvent OnListChanged;

    protected GenericObjectEvent<TreeNode> GetListItemToAdd;

    public DataListWidget(IContainer parent) : base(parent)
    {
        HeaderLabel = new Label(this);
        HeaderLabel.SetFont(Fonts.Paragraph);

        SetSize(100, 100);
        init = false;

        ListBox = new ListBox(this);
        ListBox.SetDocked(true);
        ListBox.SetPadding(0, 24, 0, 68);
        ListBox.SetFont(Fonts.Paragraph);

        AddButton = new Button(this);
        AddButton.SetBottomDocked(true);
        AddButton.SetHDocked(true);
        AddButton.SetFont(Fonts.ParagraphBold);
        AddButton.SetText("Add");
        AddButton.OnClicked += _ =>
        {
            GenericObjectEventArgs<TreeNode> args = new GenericObjectEventArgs<TreeNode>(null);
            GetListItemToAdd.Invoke(args);
            ListBox.AddItem(new TreeNode(args.Object.Text, args.Object.Object));
            OnItemAdded?.Invoke(args);
            OnListChanged?.Invoke(new BaseEventArgs());
            RemoveButton.SetEnabled(true);
        };

        RemoveButton = new Button(this);
        RemoveButton.SetBottomDocked(true);
        RemoveButton.SetHDocked(true);
        RemoveButton.SetFont(Fonts.ParagraphBold);
        RemoveButton.SetText("Remove");
        RemoveButton.SetEnabled(false);
        RemoveButton.OnClicked += _ =>
        {
            if (ListBox.SelectedItem == null) return;
            ListBox.RemoveItem(ListBox.SelectedItem);
            OnItemRemoved?.Invoke(new GenericObjectEventArgs<TreeNode>(ListBox.SelectedItem));
            OnListChanged?.Invoke(new BaseEventArgs());
            if (ListBox.Items.Count == 0) RemoveButton.SetEnabled(false);
        };
    }

    public void SetText(string text)
    {
        HeaderLabel.SetText(text);
        HeaderLabel.RedrawText(true);
        HeaderLabel.SetPosition(Size.Width / 2 - HeaderLabel.Size.Width / 2, 0);
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        if (init) return;
        base.SizeChanged(e);
        HeaderLabel.SetPosition(Size.Width / 2 - HeaderLabel.Size.Width / 2, 0);
        AddButton.SetPadding(0, 0, Size.Width / 2 - 2, 0);
        RemoveButton.SetPadding(Size.Width / 2 + 2, 0, 0, 0);
    }

    public void SetItems(List<TreeNode> items)
    {
        ListBox.SetItems(items);
        RemoveButton.SetEnabled(items.Count > 0);
    }
}
