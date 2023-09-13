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
        MinimumSize = MaximumSize = new Size(456, 223);
        SetSize(MaximumSize);
        Center();

        Label namelabel = new Label(this);
        namelabel.SetPosition(25, 44);
        namelabel.SetFont(Fonts.Paragraph);
        namelabel.SetText("Game Title:");

        namebox = new TextBox(this);
        namebox.SetFont(Fonts.Paragraph);
        namebox.SetPosition(25, 70);
        namebox.SetSize(190, 27);

        Label kitlabel = new Label(this);
        kitlabel.SetPosition(246, 44);
        kitlabel.SetFont(Fonts.Paragraph);
        kitlabel.SetText("Kit:");

        kitbox = new DropdownBox(this);
        kitbox.SetFont(Fonts.Paragraph);
        kitbox.SetPosition(246, 70);
        kitbox.SetSize(190, 24);
        List<ListItem> Items = KitManager.GetAvailableKits().Select(kit => new ListItem(kit.DisplayName, kit)).ToList();
        kitbox.SetItems(Items);

        Label folderlabel = new Label(this);
        folderlabel.SetPosition(25, 111);
        folderlabel.SetFont(Fonts.Paragraph);
        folderlabel.SetText("Project Location:");

        folderbox = new BrowserBox(this);
        folderbox.SetFont(Fonts.Paragraph);
        folderbox.SetPosition(25, 135);
        folderbox.SetSize(411, 25);
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

        CreateButton("Cancel", _ => Cancel());
        CreateButton("OK", _ => OK());

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
