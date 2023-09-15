using System.Collections.Generic;
using System.IO;

namespace RPGStudioMK.Widgets;

public abstract class AbstractFilePicker : PopupWindow
{
    public bool PressedOK;
    public string ChosenFilename;

    protected ListBox List;
    protected GroupBox previewbox;
    protected ImageBox image;
    protected Container scroll;

    protected Bitmap CurrentBitmap;

    public AbstractFilePicker(string Label, string Directory, string InitialFilename = null)
    {
        SetTitle("Pick File");

        Label pickerlabel = new Label(this);
        pickerlabel.SetText(Label);
        pickerlabel.SetPosition(25, 31);
        pickerlabel.SetFont(Fonts.ParagraphBold);
        List = new ListBox(this);
        List.SetPosition(32, 51);
        List<TreeNode> items = new List<TreeNode>();
        foreach (string filename in System.IO.Directory.GetFiles(Directory))
        {
            items.Add(new TreeNode(Path.GetFileNameWithoutExtension(filename), filename));
        }
        items.Sort(delegate (TreeNode l1, TreeNode l2) { return l1.Text.CompareTo(l2.Text); });
        items.Insert(0, new TreeNode("(None)", null));
        List.SetItems(items);
        List.OnSelectionChanged += delegate (BaseEventArgs e)
        {
            UpdatePreview();
        };

        Label previewlabel = new Label(this);
        previewlabel.SetText("Preview");
        previewlabel.SetPosition(199, 31);
        previewlabel.SetFont(Fonts.ParagraphBold);
        previewbox = new GroupBox(this);
        previewbox.SetPosition(207, 51);
        previewbox.Sprites["line1"] = new Sprite(previewbox.Viewport, new SolidBitmap(1, 1, new Color(40, 62, 84)));
        previewbox.Sprites["line1"].Y = 2;
        previewbox.Sprites["line2"] = new Sprite(previewbox.Viewport, new SolidBitmap(1, 1, new Color(40, 62, 84)));
        previewbox.Sprites["line2"].X = 2;
        previewbox.Sprites["box"] = new Sprite(previewbox.Viewport, new SolidBitmap(11, 11, new Color(64, 104, 164)));
        scroll = new Container(previewbox);
        scroll.SetPosition(3, 3);
        scroll.SetVScrollBar(new VScrollBar(previewbox));
        scroll.VAutoScroll = true;
        scroll.SetHScrollBar(new HScrollBar(previewbox));
        scroll.HAutoScroll = true;

        image = new ImageBox(scroll);

        CreateButton("Cancel", _ => Cancel());
        CreateButton("OK", _ => OK());

        RegisterShortcuts(new List<Shortcut>()
        {
            new Shortcut(this, new Key(Keycode.ENTER, Keycode.CTRL), _ => OK(), true)
        });

        if (!string.IsNullOrEmpty(InitialFilename))
        {
			TreeNode item = List.Items.Find(i => i.Text == InitialFilename);
            if (item == null) List.SetSelectedIndex(0);
            else List.SetSelectedIndex(List.Items.IndexOf(item));
        }
        else List.SetSelectedIndex(0);

        SetSize(506, 512);
        Center();
    }

    public virtual void UpdatePreview()
    {
        string filename = (string)List.SelectedItem.Object;
        image.DisposeBitmap();
        image.SetSize(1, 1);
        CurrentBitmap?.Dispose();
        CurrentBitmap = null;
        if (string.IsNullOrEmpty(filename)/* || !FileExistsCaseSensitive(filename)*/) return;
        CurrentBitmap = new Bitmap(filename);
        image.SetBitmap(CurrentBitmap);
        scroll.VScrollBar.SetValue(0);
        scroll.HScrollBar.SetValue(0);
    }

    public virtual void OK()
    {
        this.PressedOK = true;
        this.ChosenFilename = List.SelectedItem.Object == null ? "" : List.SelectedItem.Text;
        Close();
    }

    public virtual void Cancel()
    {
        this.PressedOK = false;
        this.ChosenFilename = null;
        Close();
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        List.SetSize(151, Size.Height - 96);
        previewbox.SetSize(Size.Width - 233, Size.Height - 96);
        previewbox.Sprites["line1"].X = Size.Width - 246;
        previewbox.Sprites["line2"].Y = Size.Height - 109;
        ((SolidBitmap) previewbox.Sprites["line1"].Bitmap).SetSize(1, Size.Height - 100);
        ((SolidBitmap) previewbox.Sprites["line2"].Bitmap).SetSize(Size.Width - 237, 1);
        previewbox.Sprites["box"].X = Size.Width - 246;
        previewbox.Sprites["box"].Y = Size.Height - 109;
        scroll.SetSize(Size.Width - 250, Size.Height - 113);
        scroll.VScrollBar.SetPosition(Size.Width - 244, 3);
        scroll.VScrollBar.SetSize(8, Size.Height - 113);
        scroll.HScrollBar.SetPosition(3, Size.Height - 107);
        scroll.HScrollBar.SetSize(Size.Width - 250, 8);
    }
}
