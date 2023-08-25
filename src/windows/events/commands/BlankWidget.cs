using System.Diagnostics.CodeAnalysis;

namespace RPGStudioMK.Widgets.CommandWidgets;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public class BlankWidget : BaseCommandWidget
{
    public BlankWidget(IContainer Parent, int ParentWidgetIndex) : base(Parent, ParentWidgetIndex)
    {

    }

    public override void LoadCommand()
    {
        base.LoadCommand();
        HeaderLabel.SetVisible(false);
        ScaleGradientWithSize = true;
        SetWidth(40);
    }

    protected override bool IsEditable()
    {
        return false;
    }
}
