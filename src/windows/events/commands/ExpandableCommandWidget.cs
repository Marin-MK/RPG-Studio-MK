﻿using System.Diagnostics.CodeAnalysis;

namespace RPGStudioMK.Widgets.CommandWidgets;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public class ExpandableCommandWidget : BaseCommandWidget
{
    public bool Expanded => ExpandArrow.Expanded;
    public bool Expandable { get; protected set; } = true;

    bool SilenceEvent = false;
    ExpandArrow ExpandArrow;

    public ExpandableCommandWidget(IContainer Parent, int ParentWidgetIndex) : base(Parent, ParentWidgetIndex)
    {
        ExpandArrow = new ExpandArrow(this);
        ExpandArrow.SetExpanded(Editor.GeneralSettings.ExpandEventCommands);
        ExpandArrow.OnStateChanged += _ =>
        {
            if (SilenceEvent) return;
            LoadCommand();
            UpdateSize();
            ((VStackPanel) Parent).UpdateLayout();
        };
        OnSizeChanged += _ => ExpandArrow.SetPosition(Size.Width - 16, 8);
    }

    public void SetExpanded(bool Expanded, bool SilenceEvent = false)
    {
        if (this.Expanded != Expanded)
        {
            this.SilenceEvent = SilenceEvent;
            ExpandArrow.SetExpanded(Expanded);
            this.SilenceEvent = false;
        }
    }

    public void SetExpandable(bool Expandable)
    {
        if (this.Expandable != Expandable)
        {
            this.Expandable = Expandable;
            ExpandArrow.SetVisible(Expandable);
        }
    }

    public override void WidgetSelected(BaseEventArgs e)
    {
        if (ExpandArrow.Mouse.Inside) return;
        base.WidgetSelected(e);
    }
}
