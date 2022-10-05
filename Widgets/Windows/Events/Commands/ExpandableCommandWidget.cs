using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Widgets.CommandWidgets;

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

    public override void LeftMouseDownInside(MouseEventArgs e)
    {
        base.LeftMouseDownInside(e);
        if (e.Handled || this.Indentation == -1 || ExpandArrow.Mouse.Inside)
        {
            CancelDoubleClick();
            return;
        }
        SelectNormally();
    }
}
