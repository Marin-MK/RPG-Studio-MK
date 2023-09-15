using System.Collections.Generic;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class MapPicker : PopupWindow
{
    public Map ChosenMap;

    ListBox Maps;
    Container PreviewContainer;
    ImageBox MapBox;

    public List<Map> MapList;

    public MapPicker(List<int> HiddenMapIDs, string Title = "Pick a Map", bool ShowIDs = true)
    {
        this.MapList = new List<Map>();
        foreach (KeyValuePair<int, Map> kvp in Data.Maps)
        {
            if (!HiddenMapIDs.Contains(kvp.Key)) this.MapList.Add(kvp.Value);
        }
        Initialize(Title, ShowIDs);
    }

    public MapPicker(List<Map> Maps, string Title = "Pick a Map", bool ShowIDs = true)
    {
        this.MapList = Maps;
        Initialize(Title, ShowIDs);
    }

    protected void Initialize(string Title, bool ShowIDs)
    {
        SetTitle(Title);
        MinimumSize = MaximumSize = new Size(600, 469);
        SetSize(MaximumSize);
        Center();

        Label pickerlabel = new Label(this);
        pickerlabel.SetText("Maps");
        pickerlabel.SetPosition(18, 24);
        pickerlabel.SetFont(Fonts.ParagraphBold);
        Maps = new ListBox(this);
        Maps.SetPosition(25, 44);
        Maps.SetSize(151, 380);
        List<TreeNode> items = new List<TreeNode>();
        foreach (Map Map in this.MapList)
        {
            string Name = ShowIDs ? $"{Utilities.Digits(Map.ID, 3)}: {Map.Name}" : Map.Name;
            items.Add(new TreeNode(Name, Map));
        }
        Maps.SetItems(items);
        Maps.OnSelectionChanged += delegate (BaseEventArgs e)
        {
            UpdatePreview();
        };

        Label previewlabel = new Label(this);
        previewlabel.SetText("Preview");
        previewlabel.SetPosition(192, 24);
        previewlabel.SetFont(Fonts.ParagraphBold);

        ColoredBox outline = new ColoredBox(this);
        outline.SetPosition(194, 44);
        outline.SetSize(380, 380);
        outline.SetOuterColor(59, 91, 124);
        outline.SetInnerColor(24, 38, 53);
        PreviewContainer = new Container(this);
        PreviewContainer.SetBackgroundColor(17, 27, 38);
        PreviewContainer.SetPosition(196, 46);
        PreviewContainer.SetSize(376, 376);

        MapBox = new ImageBox(PreviewContainer);
        MapBox.SetFillMode(FillMode.FillMaintainAspectAndCenter);

        CreateButton("Cancel", _ => Cancel());
        CreateButton("OK", _ => OK());

        RegisterShortcuts(new List<Shortcut>()
        {
            new Shortcut(this, new Key(Keycode.ENTER, Keycode.CTRL), _ => OK(), true)
        });

        if (Maps.Items.Count > 0) Maps.SetSelectedIndex(0);
    }

    public void UpdatePreview()
    {
        Map data = null;
        if (Maps.SelectedIndex >= 0) data = (Map) Maps.SelectedItem.Object;
        MapBox.SetSize(1, 1);
        if (data == null) return;
        MapBox.DisposeBitmap();
        MapBox.SetBitmap(Utilities.CreateMapPreview(data));
    }

    public void OK()
    {
        if (Maps.SelectedIndex >= 0)
        {
            Map map = (Map)Maps.SelectedItem.Object;
            this.ChosenMap = map;
        }
        else
        {
            this.ChosenMap = null;
        }
        Close();
    }

    public void Cancel()
    {
        this.ChosenMap = null;
        Close();
    }
}
