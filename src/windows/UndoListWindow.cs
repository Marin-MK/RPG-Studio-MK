using System.Collections.Generic;

namespace RPGStudioMK.Widgets;

public class UndoListWindow : PopupWindow
{
    List<Undo.BaseUndoAction> Actions;
    bool IsRedo;

    public bool Apply = false;
    public Undo.BaseUndoAction ActionToRevertTo;

    public UndoListWindow(List<Undo.BaseUndoAction> Actions, bool IsRedo)
    {
        this.Actions = Actions;
        this.IsRedo = IsRedo;

        SetTitle($"{(IsRedo ? "Redo List" : "Undo List")}");
        MinimumSize = MaximumSize = new Size(500, 500);
        SetSize(MaximumSize);
        Center();

        List<ListItem> Items = new List<ListItem>();
        if (IsRedo) Actions.ForEach(a => Items.Add(new ListItem(a.Title, a)));
        else
        {
            for (int i = Actions.Count - 1; i >= 0; i--)
            {
                Items.Add(new ListItem(Actions[i].Title, Actions[i]));
            }
        }

        Label PresentLabel = new Label(this);
        PresentLabel.SetFont(Fonts.Paragraph);
        PresentLabel.SetPosition(16, 30);
        PresentLabel.SetText("Present");

        Label PastLabel = new Label(this);
        PastLabel.SetFont(Fonts.Paragraph);
        PastLabel.SetBottomDocked(true);
        PastLabel.SetPadding(16, 0, 0, -18);
        PastLabel.SetText("Past");

        ListBox ListBox = new ListBox(this);
        ListBox.SetVDocked(true);
        ListBox.SetPadding(14, 50, 0, 34);
        ListBox.SetWidth(160);
        ListBox.SetItems(Items);

        Container DescContainer = new Container(this);
        DescContainer.SetDocked(true);
        DescContainer.SetPadding(ListBox.Padding.Left + ListBox.Size.Width + 6, ListBox.Padding.Up, 20, 80);

        VScrollBar vs = new VScrollBar(this);
        vs.SetVDocked(true);
        vs.SetRightDocked(true);
        vs.SetPadding(0, DescContainer.Padding.Up, 9, DescContainer.Padding.Down);
        DescContainer.SetVScrollBar(vs);
        DescContainer.VAutoScroll = true;

        MultilineLabel DescLabel = new MultilineLabel(DescContainer);
        DescLabel.SetFont(Fonts.Paragraph);
        DescLabel.SetHDocked(true);

        Button RevertButton = new Button(this);
        RevertButton.SetBottomDocked(true);
        RevertButton.SetRightDocked(true);
        RevertButton.SetPadding(0, 0, 31, 40);
        RevertButton.SetWidth(105);
        RevertButton.SetText("Revert To");
        RevertButton.OnClicked += _ =>
        {
            MessageBox box = new MessageBox("Warning", "Reverting to an arbitrary point in history can be considered dangerous, because you cannot go back to the current point once you make even a single change. Are you sure you would like to continue?", ButtonType.YesNoCancel, IconType.Warning);
            box.OnClosed += _ =>
            {
                if (box.Result == 0)
                {
                    ActionToRevertTo = (Undo.BaseUndoAction) ListBox.SelectedItem.Object;
                    Apply = true;
                    Close();
                }
            };
        };

        CreateButton("Cancel", _ => Cancel());
        Buttons[0].SetWidth(Buttons[0].Size.Width + 20);
        Buttons[0].SetPosition(Buttons[0].Position.X - 20, Buttons[0].Position.Y);

        ListBox.OnSelectionChanged += _ =>
        {
            Undo.BaseUndoAction Action = (Undo.BaseUndoAction) ListBox.SelectedItem.Object;
            DescLabel.SetText(Action.Title + "\n\n" + Action.Description);
        };

        ListBox.SetSelectedIndex(0);
    }

    private void Cancel()
    {
        Close();
    }
}
