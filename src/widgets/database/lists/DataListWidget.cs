using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Widgets;

public abstract class DataListWidget : Widget
{
    public List<ListItem> Items => ListBox.Items;

    protected Label HeaderLabel;
    protected ListBox ListBox;
    protected Button AddButton;
    protected Button RemoveButton;
    protected bool init = true;

    public GenericObjectEvent<ListItem> OnItemAdded;
    public GenericObjectEvent<ListItem> OnItemRemoved;
    public BaseEvent OnListChanged;

    protected GenericObjectEvent<ListItem> GetListItemToAdd;

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
            GenericObjectEventArgs<ListItem> args = new GenericObjectEventArgs<ListItem>(null);
            GetListItemToAdd.Invoke(args);
            ListBox.Items.Add(args.Object);
            OnItemAdded?.Invoke(args);
            OnListChanged?.Invoke(new BaseEventArgs());
            RemoveButton.SetEnabled(true);
            if (ListBox.Items.Count == 1 && ListBox.SelectedIndex == -1) ListBox.SetSelectedIndex(0);
            ListBox.Redraw();
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
            ListBox.Items.Remove(ListBox.SelectedItem);
            if (ListBox.SelectedIndex >= ListBox.Items.Count) ListBox.SetSelectedIndex(ListBox.Items.Count - 1);
            OnItemRemoved?.Invoke(new GenericObjectEventArgs<ListItem>(ListBox.SelectedItem));
            OnListChanged?.Invoke(new BaseEventArgs());
            ListBox.Redraw();
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

    public void SetItems(List<ListItem> items)
    {
        ListBox.SetItems(items);
        RemoveButton.SetEnabled(items.Count > 0);
    }
}
