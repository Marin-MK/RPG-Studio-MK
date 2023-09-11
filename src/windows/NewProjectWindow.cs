using System;
using System.Collections.Generic;
using System.Linq;

namespace RPGStudioMK.Widgets;

public class NewProjectWindow : PopupWindow
{
    public bool PressedOK;

    public string Name;
    public Kit Kit;
    public string Folder;

    TextBox namebox;
    DropdownBox kitbox;
    BrowserBox folderbox;

    public NewProjectWindow()
    {
        SetTitle("New Project");
        MinimumSize = MaximumSize = new Size(300, 190);
        SetSize(MaximumSize);
        Center();

        Label namelabel = new Label(this);
        namelabel.SetPosition(16, 42);
        namelabel.SetFont(Fonts.Paragraph);
        namelabel.SetText("Name:");
        namebox = new TextBox(this);
        namebox.SetFont(Fonts.Paragraph);
        namebox.SetPosition(70, 39);
        namebox.SetSize(210, 27);

        Label kitlabel = new Label(this);
        kitlabel.SetPosition(16, 76);
        kitlabel.SetFont(Fonts.Paragraph);
        kitlabel.SetText("Kit:");
        kitbox = new DropdownBox(this);
        kitbox.SetFont(Fonts.Paragraph);
        kitbox.SetPosition(70, 73);
        kitbox.SetSize(210, 27);
        List<ListItem> Items = KitManager.GetAvailableKits().Select(kit => new ListItem(kit.DisplayName, kit)).ToList();
        kitbox.SetItems(Items);

        Label folderlabel = new Label(this);
        folderlabel.SetPosition(16, 110);
        folderlabel.SetFont(Fonts.Paragraph);
        folderlabel.SetText("Folder:");
        folderbox = new BrowserBox(this);
        folderbox.SetFont(Fonts.Paragraph);
        folderbox.SetPosition(70, 107);
        folderbox.SetSize(210, 27);
        folderbox.SetText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
        folderbox.OnDropDownClicked += _ =>
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Choose the folder in which to create your game folder.";
            ofd.SetInitialDirectory(folderbox.Text);
            string result = ofd.ChooseFolder();
            if (result != null)
            {
                folderbox.SetText(result);
            }
        };

        CreateButton("OK", _ => OK());
        CreateButton("Cancel", _ => Cancel());

        RegisterShortcuts(new List<Shortcut>()
        {
            new Shortcut(this, new Key(Keycode.ENTER, Keycode.CTRL), _ => OK(), true)
        });
    }

    void OK()
    {
        if (string.IsNullOrEmpty(namebox.Text))
        {
            new MessageBox("Error", "Please pick a title for your game.", ButtonType.OK, IconType.Warning);
            return;
        }
        if (string.IsNullOrEmpty(folderbox.Text))
        {
            new MessageBox("Error", "You need to specify the folder you want to create your game folder in.", ButtonType.OK, IconType.Warning);
            return;
        }
        PressedOK = true;
        Name = namebox.Text;
        Kit = (Kit) kitbox.Items[kitbox.SelectedIndex].Object;
        Folder = folderbox.Text;
        Close();
    }

    void Cancel()
    {
        Close();
    }
}
