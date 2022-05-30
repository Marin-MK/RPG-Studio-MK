using System;
using System.Collections.Generic;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class AutotilePicker : PopupWindow
{
    List<int> OldIDs;

    public List<int> ResultIDs;

    Autotile SelectedAutotile { get { return Available.SelectedItem is null ? InUse.SelectedItem.Object as Autotile : Available.SelectedItem.Object as Autotile; } }

    int AnimateCount = 0;

    Button ActionButton;
    ListBox Available;
    ListBox InUse;

    public AutotilePicker(Map Map)
    {
        SetTitle("Change Autotiles");
        MinimumSize = MaximumSize = new Size(506, 498);
        SetSize(MaximumSize);
        Center();

        OldIDs = new List<int>(Map.AutotileIDs);

        ColoredBox box1 = new ColoredBox(this);
        box1.SetOuterColor(59, 91, 124);
        box1.SetSize(280, 409);
        box1.SetPosition(200, 44);

        ColoredBox box2 = new ColoredBox(this);
        box2.SetOuterColor(17, 27, 38);
        box2.SetInnerColor(24, 38, 53);
        box2.SetSize(278, 407);
        box2.SetPosition(201, 45);

        Sprites["preview"] = new Sprite(this.Viewport);

        Font f = Fonts.UbuntuBold.Use(14);

        Label labelavail = new Label(this);
        labelavail.SetText("Available");
        labelavail.SetPosition(16, 24);
        labelavail.SetFont(f);

        Label labelinuse = new Label(this);
        labelinuse.SetText("In-use");
        labelinuse.SetPosition(16, 250);
        labelinuse.SetFont(f);

        Label labelprev = new Label(this);
        labelprev.SetText("Preview");
        labelprev.SetPosition(192, 24);
        labelprev.SetFont(f);

        ActionButton = new Button(this);
        ActionButton.SetPosition(52, 225);
        ActionButton.SetSize(85, 30);
        ActionButton.OnClicked += ActionButtonClicked;

        Available = new ListBox(this);
        Available.SetPosition(25, 44);
        Available.SetSize(151, 179);
        Available.OnSelectionChanged += delegate (BaseEventArgs e)
        {
            if (Available.SelectedIndex != -1)
            {
                InUse.SetSelectedIndex(-1);
                SelectionChanged(e);
            }
        };
        Available.ListDrawer.SetContextMenuList(new List<IMenuItem>()
            {
                new MenuItem("Add Autotile")
                {
                    IsClickable = delegate (BoolEventArgs e)
                    {
                        e.Value = SelectedAutotile is not null;
                    },
                    OnLeftClick = ActionButtonClicked
                }
            });

        InUse = new ListBox(this);
        InUse.SetPosition(25, 274);
        InUse.SetSize(151, 179);
        InUse.OnSelectionChanged += delegate (BaseEventArgs e)
        {
            if (InUse.SelectedIndex != -1)
            {
                Available.SetSelectedIndex(-1);
                SelectionChanged(e);
            }
        };
        InUse.ListDrawer.SetContextMenuList(new List<IMenuItem>()
            {
                new MenuItem("Move Autotile Up")
                {
                    IsClickable = delegate (BoolEventArgs e)
                    {
                        e.Value = InUse.SelectedIndex > 0;
                    },
                    OnLeftClick = MoveAutotileUp
                },
                new MenuItem("Move Autotile Down")
                {
                    IsClickable = delegate (BoolEventArgs e)
                    {
                        e.Value = InUse.SelectedIndex < InUse.Items.Count - 1;
                    },
                    OnLeftClick = MoveAutotileDown
                },
                new MenuSeparator(),
                new MenuItem("Remove Autotile")
                {
                    OnLeftClick = ActionButtonClicked
                }
            });

        List<ListItem> AvailableList = new List<ListItem>();
        List<ListItem> InUseList = new List<ListItem>();

        // Populate lists
        for (int i = 0; i < Map.AutotileIDs.Count; i++)
        {
            InUseList.Add(new ListItem($"{Utilities.Digits(Map.AutotileIDs[i], 3)}: {Data.Autotiles[Map.AutotileIDs[i]].Name}", Data.Autotiles[Map.AutotileIDs[i]]));
        }
        for (int i = 1; i < Data.Autotiles.Count; i++)
        {
            if (!Map.AutotileIDs.Contains(i))
            {
                AvailableList.Add(new ListItem($"{Utilities.Digits(i, 3)}: {Data.Autotiles[i]?.Name}", Data.Autotiles[i]));
            }
        }

        Available.SetItems(AvailableList);
        InUse.SetItems(InUseList);

        if (Available.Items.Count > 0)
        {
            Available.SetSelectedIndex(0);
            ActionButton.SetText("Add");
        }
        else
        {
            InUse.SetSelectedIndex(0);
            ActionButton.SetText("Remove");
        }

        SetTimer("frame", (long)Math.Round(1000 / 60d));

        CreateButton("Cancel", _ => Cancel());
        CreateButton("OK", _ => OK());

        RegisterShortcuts(new List<Shortcut>()
        {
            new Shortcut(this, new Key(Keycode.ENTER, Keycode.CTRL), _ => OK(), true)
        });
    }

    private void Cancel()
    {
        ResultIDs = OldIDs;
        Close();
    }

    private void OK()
    {
        ResultIDs = new List<int>();
        for (int i = 0; i < InUse.Items.Count; i++)
        {
            ResultIDs.Add(Data.Autotiles.IndexOf((InUse.Items[i].Object as Autotile)));
        }
        Close();
    }

    public void SelectionChanged(BaseEventArgs e)
    {
        ActionButton.SetEnabled(true);
        if (InUse.SelectedIndex == -1)
        {
            ActionButton.SetText("Add");
        }
        else
        {
            ActionButton.SetText("Remove");
        }
        Autotile autotile = SelectedAutotile;
        if (autotile is null)
        {
            if (Sprites["preview"].Bitmap != null) Sprites["preview"].Bitmap.Dispose();
            ActionButton.SetEnabled(false);
            return;
        }
        Sprites["preview"].Bitmap = autotile.AutotileBitmap.Clone();
        DrawAutotile(autotile, 0);
        Sprites["preview"].X = 340;
        Sprites["preview"].Y = 248;
        Sprites["preview"].OX = Sprites["preview"].SrcRect.Width / 2;
        Sprites["preview"].OY = Sprites["preview"].SrcRect.Height / 2;
    }

    public void DrawAutotile(Autotile autotile, int Frame)
    {
        int AnimX = 0;
        if (autotile.Format == AutotileFormat.Single)
        {
            AnimX = (Frame * 32) % Sprites["preview"].Bitmap.Width;
            Sprites["preview"].SrcRect = new Rect(AnimX, 0, 32, 32);
        }
        else
        {
            AnimX = (Frame * 96) % Sprites["preview"].Bitmap.Width;
            int height = 0;
            if (autotile.Format == AutotileFormat.FullCorners) height = 192;
            else if (autotile.Format == AutotileFormat.RMXP) height = 128;
            else if (autotile.Format == AutotileFormat.RMVX) height = 96;
            Sprites["preview"].SrcRect = new Rect(AnimX, 0, 96, height);
        }
    }

    public override void Update()
    {
        base.Update();
        if (TimerPassed("frame"))
        {
            ResetTimer("frame");
            AnimateCount++;
            if (SelectedAutotile is null) return;
            if (AnimateCount % SelectedAutotile.AnimateSpeed == 0)
            {
                DrawAutotile(SelectedAutotile, (int)Math.Floor((double)AnimateCount / SelectedAutotile.AnimateSpeed));
            }
        }
    }

    private void ActionButtonClicked(BaseEventArgs e)
    {
        if (InUse.SelectedIndex == -1) // Add
        {
            if (SelectedAutotile is null) return;
            Autotile autotile = SelectedAutotile;
            ListItem item = Available.SelectedItem;
            Available.Items.Remove(item);
            InUse.Items.Add(item);
            Available.SetItems(Available.Items);
            InUse.SetItems(InUse.Items);
            if (Available.SelectedIndex == -1 && InUse.SelectedIndex == -1)
                InUse.SetSelectedIndex(0);
        }
        else // Remove
        {
            if (SelectedAutotile is null) return;
            Autotile autotile = SelectedAutotile;
            ListItem item = InUse.SelectedItem;
            InUse.Items.Remove(item);
            List<ListItem> availitems = new List<ListItem>();
            for (int i = 1; i < Data.Autotiles.Count; i++)
            {
                if (InUse.Items.Find(item => item.Object == Data.Autotiles[i]) is null)
                {
                    availitems.Add(new ListItem($"{Utilities.Digits(i, 3)}: {Data.Autotiles[i]?.Name}", Data.Autotiles[i]));
                }
            }
            Available.SetItems(availitems);
            InUse.SetItems(InUse.Items);
            if (Available.SelectedIndex == -1 && InUse.SelectedIndex == -1)
                Available.SetSelectedIndex(0);
        }
        SelectionChanged(e);
    }

    public void MoveAutotileUp(BaseEventArgs e)
    {
        if (InUse.SelectedIndex > 0)
        {
            InUse.Items.Swap(InUse.SelectedIndex - 1, InUse.SelectedIndex);
            InUse.Redraw();
            InUse.SetSelectedIndex(InUse.SelectedIndex - 1);
        }
    }

    public void MoveAutotileDown(BaseEventArgs e)
    {
        if (InUse.SelectedIndex < InUse.Items.Count - 1)
        {
            InUse.Items.Swap(InUse.SelectedIndex + 1, InUse.SelectedIndex);
            InUse.Redraw();
            InUse.SetSelectedIndex(InUse.SelectedIndex + 1);
        }
    }
}
