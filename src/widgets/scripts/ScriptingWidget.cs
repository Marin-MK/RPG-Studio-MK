﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using RPGStudioMK.Game;
using RPGStudioMK.Utility;

namespace RPGStudioMK.Widgets;

public class ScriptingWidget : Widget
{
    public List<Script> OpenScripts => ScriptBox.OpenScripts;
    public Script? OpenScript => ScriptBox.OpenScript;
    public Script? PreviewScript => ScriptBox.PreviewScript;
    public List<Script> RecentScripts => ScriptBox.RecentScripts;
    public TreeNode CoreScriptNode => (TreeNode) TreeView.Root.Children[0];
    public TreeNode PluginScriptNode => (TreeNode) TreeView.Root.Children[1];
    public TreeNode CustomScriptNode => (TreeNode) TreeView.Root.Children[2];

    TreeView TreeView;
    ScriptEditorBox ScriptBox;
    GlobalFindReplaceWidget GlobalFindReplaceBox;
    Grid MainGrid;

    ContextMenu ScriptMenu;

    public ScriptingWidget(IContainer Parent) : base(Parent)
    {
        TreeView = new TreeView(this);
        TreeView.SetVDocked(true);
        TreeView.SetWidth(300);
        TreeView.SetNodes(GetNodes());
        TreeView.SetLineHeight(28);
        TreeView.SetFont(FontCache.GetOrCreate("Cabin-Medium", 10));
        TreeView.SetBackgroundColor(10, 23, 37);
        TreeView.OnSelectionChanged += e => ScriptBox.SetScriptBox((Script) ((TreeNode) TreeView.SelectedNode).Object, !e.Value); // e.Value: whether the node was double clicked or single-clicked

        MainGrid = new Grid(this);
        MainGrid.SetPadding(300, 0, 0, 0);
        MainGrid.SetDocked(true);
        MainGrid.SetColumns(new GridSize(1));

        ScriptBox = new ScriptEditorBox(MainGrid);

        RegisterShortcuts(new List<Shortcut>()
        {
            new Shortcut(this, new Key(Keycode.TAB, Keycode.CTRL), _ => ShowScriptMenu(true), true),
            new Shortcut(this, new Key(Keycode.TAB, Keycode.SHIFT, Keycode.CTRL), _ => ShowScriptMenu(true), true),
            new Shortcut(this, new Key(Keycode.F, Keycode.CTRL, Keycode.SHIFT), _ => ShowGlobalFindReplace(false), true),
            new Shortcut(this, new Key(Keycode.H, Keycode.CTRL, Keycode.SHIFT), _ => ShowGlobalFindReplace(true), true)
        });

        var coreNode = (TreeNode) TreeView.Root.Children[0];
        ScriptBox.SetScriptBox((Script) ((TreeNode) coreNode.Children[0]).Object, false);
        TreeView.SetSelectedNode(coreNode.Children[0], false);
    }

    private void ShowGlobalFindReplace(bool openOnReplaceTab)
    {
        if (GlobalFindReplaceBox is not null && !GlobalFindReplaceBox.Disposed)
        {
            if (GlobalFindReplaceBox.SelectedTab == 0 && openOnReplaceTab || GlobalFindReplaceBox.SelectedTab == 1 && !openOnReplaceTab)
            {
                GlobalFindReplaceBox.SelectTab(openOnReplaceTab ? 1 : 0);
                GlobalFindReplaceBox.FocusFindBox();
                return;
            }
        }
        if (GlobalFindReplaceBox is not null && !GlobalFindReplaceBox.Disposed)
        {
    		MainGrid.SetColumns(new GridSize(1));
            GlobalFindReplaceBox.Dispose();
            GlobalFindReplaceBox = null;
            MainGrid.UpdateContainers();
            MainGrid.UpdateLayout();
			ScriptBox.UpdateWidth();
			return;
        }
		MainGrid.SetColumns(
			new GridSize(1),
			new GridSize(500, Unit.Pixels)
		);
		GlobalFindReplaceBox = new GlobalFindReplaceWidget(MainGrid);
        GlobalFindReplaceBox.SetGridColumn(1);
        GlobalFindReplaceBox.SelectTab(openOnReplaceTab ? 1 : 0);
        GlobalFindReplaceBox.OnDisposed += _ =>
        {
            MainGrid.SetColumns(new GridSize(1));
            GlobalFindReplaceBox = null;
            MainGrid.UpdateContainers();
            MainGrid.UpdateLayout();
			ScriptBox.UpdateWidth();
		};
        MainGrid.UpdateContainers();
        MainGrid.UpdateLayout();
        ScriptBox.UpdateWidth();
		GlobalFindReplaceBox.FocusFindBox();
	}

    private static List<TreeNode> GetNodes()
    {
        var nodes = new List<TreeNode>();
        TreeNode coreScripts = new TreeNode("Core");
        coreScripts.SetSelectable(false);
        coreScripts.SetDraggable(false);
        nodes.Add(coreScripts);
        TreeNode? directParent = null;
        TreeNode? olderParent = null;
        Regex sepRegex = new Regex(@"==================");
        Regex catRegex = new Regex(@"\[\[ (.*) \]\]");
        foreach (Script script in Data.Scripts)
        {
            if (string.IsNullOrWhiteSpace(script.Content) && string.IsNullOrWhiteSpace(script.Name)) continue;
            TreeNode parent = directParent is null ? coreScripts : directParent;
            TreeNode newNode = new TreeNode(script.Name, script);
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
        TreeNode pluginScripts = new TreeNode("Plugins");
        pluginScripts.SetSelectable(false);
        pluginScripts.SetDraggable(false);
        foreach (GamePlugin plugin in Data.Plugins)
        {
            TreeNode pluginNode = new TreeNode(plugin.Name);
            pluginNode.SetSelectable(false);
            pluginNode.SetDraggable(false);
            pluginScripts.AddChild(pluginNode);
            foreach (Script script in plugin.Scripts)
            {
                TreeNode pluginScriptNode = new TreeNode(script.Name, script);
                pluginNode.AddChild(pluginScriptNode);
            }
        }
        nodes.Add(pluginScripts);
        TreeNode customScripts = new TreeNode("Custom");
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
                    ScriptBox.Viewport.X + ScriptBox.Size.Width / 2 - ScriptMenu.Size.Width / 2,
                    ScriptBox.Viewport.Y + ScriptBox.Size.Height / 2 - ScriptMenu.Size.Height / 2
                );
            else
                ScriptMenu.SetPosition(
                    ScriptBox.Viewport.X + ScriptBox.Size.Width - ScriptMenu.Size.Width - 8,
                    ScriptBox.Viewport.Y + 32
                );
            ScriptMenu.SetMoveIndex(tabs.Count == 1 ? 0 : 1, true);
            ScriptMenu.OnDisposed += _ =>
            {
                if (Input.Press(Keycode.ESCAPE) || (!ScriptMenu.Mouse.Inside && ScriptMenu.Mouse.LeftMouseTriggered)) return;
                Script script = RecentScripts[RecentScripts.Count - ScriptMenu.MoveIndex - 1];
                if (OpenScripts.Contains(script)) ScriptBox.SetPivot(OpenScripts.IndexOf(script));
                ScriptBox.SetScriptBox(script, false);
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
        ScriptBox.UpdatePositionAndSizeIfDocked();
        ScriptBox.UpdateSize();
    }

    public void PreviewOccurrence(Script script, Occurrence occurrence)
    {
        ScriptBox.PreviewOccurrence(script, occurrence);
    }

    public int ReplaceSingleOccurrence(Occurrence occurrence, string pattern)
    {
        return ScriptBox.ReplaceSingleOccurrence(occurrence, pattern);
    }

    public void ReplaceAllOccurrences(List<Occurrence> occurrences, string pattern)
    {
        ScriptBox.ReplaceAllOccurrences(occurrences, pattern);
    }
}
