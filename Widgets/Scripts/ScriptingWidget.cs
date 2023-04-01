using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class ScriptingWidget : Widget
{
    OptimizedTreeView TreeView;
    ScriptEditorTextBox TextBox;

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
        TreeView.OnSelectionChanged += _ => TextBox.SetScript((Script) ((OptimizedNode) TreeView.SelectedNode).Object, true, false);

        TextBox = new ScriptEditorTextBox(this);
        TextBox.SetDocked(true);
        TextBox.SetPadding(300, 0, 0, 0);

        var coreNode = (OptimizedNode) TreeView.Root.Children[0];
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

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        TextBox.UpdatePositionAndSizeIfDocked();
        TextBox.UpdateSize();
    }
}
