using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public abstract class SimpleDataTypeBase : Widget
{
	public SimpleDataTypeBase(IContainer parent) : base(parent)
	{

	}

	public abstract void Initialize();
	public abstract void SelectData<T>(T data) where T : DataResolver;
}
