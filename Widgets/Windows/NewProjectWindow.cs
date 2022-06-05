using System;
using System.Collections.Generic;

namespace RPGStudioMK.Widgets;

public class NewProjectWindow : PopupWindow
{
    public bool PressedOK;

    public string Name;
    public (string Name, string Download) Kit;
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

        Font f = Fonts.CabinMedium.Use(11);

        Label namelabel = new Label(this);
        namelabel.SetPosition(16, 42);
        namelabel.SetFont(f);
        namelabel.SetText("Name:");
        namebox = new TextBox(this);
        namebox.SetFont(f);
        namebox.SetPosition(70, 39);
        namebox.SetSize(210, 27);

        Label kitlabel = new Label(this);
        kitlabel.SetPosition(16, 76);
        kitlabel.SetFont(f);
        kitlabel.SetText("Kit:");
        kitbox = new DropdownBox(this);
        kitbox.SetFont(f);
        kitbox.SetPosition(70, 73);
        kitbox.SetSize(210, 27);
        List<ListItem> Items = new List<ListItem>();
        Items.Add(new ListItem(
            "Pokémon Essentials v20",
            "https://www.dropbox.com/s/s7qe2vn3bpoes5j/Pokemon%20Essentials%20v20%202022-05-19.zip?dl=1"
        ));
        Items.Add(new ListItem(
            "Pokémon Essentials v19.1",
            "https://www.dropbox.com/s/vlxkcwtcejpa1w3/Pokemon%20Essentials%20v19.1%202021-05-22.zip?dl=1"
        ));
        kitbox.SetItems(Items);

        Label folderlabel = new Label(this);
        folderlabel.SetPosition(16, 110);
        folderlabel.SetFont(f);
        folderlabel.SetText("Folder:");
        folderbox = new BrowserBox(this);
        folderbox.SetFont(f);
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
        ListItem Item = kitbox.Items[kitbox.SelectedIndex];
        Kit = (Item.Name, (string) Item.Object);
        Folder = folderbox.Text;
        Close();
    }

    void Cancel()
    {
        Close();
    }
}
