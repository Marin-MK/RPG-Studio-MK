using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class ScriptingWidget : Widget
{
    public List<Script> OpenScripts => TextBox.OpenScripts;
    public Script? OpenScript => TextBox.OpenScript;
    public Script? PreviewScript => TextBox.PreviewScript;
    public List<Script> RecentScripts => TextBox.RecentScripts;

    OptimizedTreeView TreeView;
    ScriptEditorBox TextBox;

    ContextMenu ScriptMenu;
    int ScriptMenuIndex;

    public ScriptingWidget(IContainer Parent) : base(Parent)
    {
        List<ListItem> Items = new List<ListItem>();
        for (int i = 0; i < Data.Scripts.Count; i++)
        {
            Items.Add(new ListItem(Data.Scripts[i].Name, Data.Scripts[i]));
        }

        TreeView = new OptimizedTreeView(this);
        TreeView.SetVDocked(true);
        TreeView.SetWidth(300);
        TreeView.SetNodes(GetNodes());
        TreeView.SetLineHeight(28);
        TreeView.SetFont(Font.Get("Cabin-Medium", 10));
        TreeView.OnSelectionChanged += e => TextBox.SetScript((Script) ((OptimizedNode) TreeView.SelectedNode).Object, !e.Value); // e.Value: whether the node was double clicked or single-clicked

        TextBox = new ScriptEditorBox(this);
        TextBox.SetDocked(true);
        TextBox.SetPadding(300, 0, 0, 0);

        RegisterShortcuts(new List<Shortcut>()
        {
            new Shortcut(this, new Key(Keycode.TAB, Keycode.CTRL), _ => ShowScriptMenu(true), true),
            new Shortcut(this, new Key(Keycode.TAB, Keycode.SHIFT, Keycode.CTRL), _ => ShowScriptMenu(true), true)
        });

        var coreNode = (OptimizedNode) TreeView.Root.Children[0];
        TextBox.SetScript((Script) ((OptimizedNode) coreNode.Children[0]).Object, false);
        TreeView.SetSelectedNode(coreNode.Children[0], false);
    }

    private static List<OptimizedNode> GetNodes()
    {
        var nodes = new List<OptimizedNode>();
        OptimizedNode coreScripts = new OptimizedNode("Core");
        coreScripts.SetSelectable(false);
        coreScripts.SetDraggable(false);
        nodes.Add(coreScripts);
        OptimizedNode? directParent = null;
        OptimizedNode? olderParent = null;
        Regex sepRegex = new Regex(@"==================");
        Regex catRegex = new Regex(@"\[\[ (.*) \]\]");
        foreach (Script script in Data.Scripts)
        {
            if (string.IsNullOrWhiteSpace(script.Content) && string.IsNullOrWhiteSpace(script.Name)) continue;
            OptimizedNode parent = directParent is null ? coreScripts : directParent;
            OptimizedNode newNode = new OptimizedNode(script.Name, script);
            if (sepRegex.IsMatch(script.Name))
            {
                olderParent = null;
                directParent = null;
                continue;
            }
            else
            {
                Match m = catRegex.Match(script.Name);
                if (m.Success)
                {
                    newNode.SetText(m.Groups[1].Value);
                    if (directParent is not null)
                    {
                        if (olderParent is null) olderParent = directParent;
                        else
                        {
                            directParent = olderParent;
                            parent = olderParent;
                        }
                        directParent = newNode;
                    }
                    directParent = newNode;
                }
            }
            parent.AddChild(newNode);
        }
        OptimizedNode pluginScripts = new OptimizedNode("Plugins");
        pluginScripts.SetSelectable(false);
        pluginScripts.SetDraggable(false);
        nodes.Add(pluginScripts);
        OptimizedNode customScripts = new OptimizedNode("Custom");
        customScripts.SetSelectable(false);
        customScripts.SetDraggable(false);
        nodes.Add(customScripts);
        return nodes;
    }

    public void ShowScriptMenu(bool centered, bool selectSelf = false)
    {
        List<string> tabs = RecentScripts.Select(s => s.Name).Reverse().ToList();
        if (tabs.Count > 0)
        {
            ScriptMenu = new ContextMenu(Window.UI);
            ScriptMenu.SetItems(tabs.Select(txt => (IMenuItem) new MenuItem(txt)).ToList());
            ScriptMenu.SetFont(ContextMenuFont ?? DefaultContextMenuFont);
            ScriptMenu.CanMoveWithTab = true;
            ScriptMenu.CanMoveWithUpDown = true;
            if (centered)
                ScriptMenu.SetPosition(
                    TextBox.Viewport.X + TextBox.Size.Width / 2 - ScriptMenu.Size.Width / 2,
                    TextBox.Viewport.Y + TextBox.Size.Height / 2 - ScriptMenu.Size.Height / 2
                );
            else
                ScriptMenu.SetPosition(
                    TextBox.Viewport.X + TextBox.Size.Width - ScriptMenu.Size.Width - 8,
                    TextBox.Viewport.Y + 32
                );
            ScriptMenu.SetMoveIndex(tabs.Count == 1 ? 0 : 1, true);
            ScriptMenu.OnDisposed += _ =>
            {
                if (Input.Press(Keycode.ESCAPE) || (!ScriptMenu.Mouse.Inside && ScriptMenu.Mouse.LeftMouseTriggered)) return;
                Script script = RecentScripts[RecentScripts.Count - ScriptMenu.MoveIndex - 1];
                if (OpenScripts.Contains(script)) TextBox.SetPivot(OpenScripts.IndexOf(script));
                TextBox.SetScript(script, this.PreviewScript == script);
            };
            // Auto-close if tab is released within 100ms
            //if (TimerExists("short_tab")) DestroyTimer("short_tab");
            //SetTimer("short_tab", 100);
        }
    }

    public override void Update()
    {
        base.Update();
        if (ScriptMenu is null) return;
        if (ScriptMenu.Disposed) ScriptMenu = null;
        if (!Input.Press(Keycode.CTRL))
        {
            ScriptMenu.Dispose();
            ScriptMenu = null;
            return;
        }
        //if (TimerExists("short_tab") && !TimerPassed("short_tab") && !Input.Press(Keycode.TAB))
        //{
        //    ScriptMenu.Dispose();
        //    ScriptMenu = null;
        //}
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        TextBox.UpdatePositionAndSizeIfDocked();
        TextBox.UpdateSize();
    }
}
