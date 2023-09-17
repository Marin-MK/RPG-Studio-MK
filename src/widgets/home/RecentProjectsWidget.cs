using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Widgets;

public class RecentProjectsWidget : Widget
{
    public int HoveringIndex { get; protected set; } = -1;
    public BaseEvent OnProjectClicked;

    MultilineLabel noProjectsLabel;
    int lastDrawnCount = 0;

    public RecentProjectsWidget(IContainer parent) : base(parent)
    {
		Sprites["sel"] = new Sprite(this.Viewport, new SolidBitmap(4, 38, new Color(0, 205, 255)));
		Sprites["files"] = new Sprite(this.Viewport);

		noProjectsLabel = new MultilineLabel(this);
		noProjectsLabel.SetSize(320, 100);
		noProjectsLabel.SetPosition(40, 170);
		noProjectsLabel.SetText("You haven't opened any projects recently.\nGet started by creating or opening a project!");
		noProjectsLabel.SetFont(Fonts.Paragraph);

		OnSizeChanged += _ => DrawProjects();
	}

	public void DrawProjects()
	{
		Sprites["files"].Bitmap?.Dispose();
        if (Editor.GeneralSettings.RecentFiles.Count == 0)
        {
            noProjectsLabel.SetVisible(true);
            return;
        }
        int height = Size.Height - 190;
        if (height < 1) return;
        int drawCount = (int) Math.Floor(height / 48d);
        if (drawCount == lastDrawnCount) return;
        noProjectsLabel.SetVisible(false);
        Sprites["files"].Bitmap = new Bitmap(314, 48 * drawCount);
        Sprites["files"].Bitmap.Unlock();
        int count = Editor.GeneralSettings.RecentFiles.Count;
        for (int i = 0; i < Math.Min(drawCount, count); i++)
        {
            string name = Editor.GeneralSettings.RecentFiles[count - i - 1][0]; // Project name
            string projectpath = Editor.GeneralSettings.RecentFiles[count - i - 1][1].Replace('\\', '/'); // Project file
            Sprites["files"].Bitmap.Font = Fonts.HomeFont;
            Sprites["files"].Bitmap.DrawText(name, 6, 54 * i + 4, Color.WHITE);
            Sprites["files"].Bitmap.Font = Fonts.Paragraph;
            List<string> folders = projectpath.Split('/').ToList();
            string path = "";
            for (int j = folders.Count - 2; j >= 0; j--)
            {
                string add = "/";
                if (folders[j].Contains(":") || folders[j] == "") add = "";
                Size s = Sprites["files"].Bitmap.Font.TextSize(add + folders[j] + path);
                if (s.Width > Sprites["files"].Bitmap.Width - 39)
                {
                    path = "..." + path;
                    break;
                }
                else
                {
                    path = add + folders[j] + path;
                }
            }
            Sprites["files"].Bitmap.DrawText(path, 36, 54 * i + 26, Color.WHITE);
        }
        Sprites["files"].Bitmap.Lock();
        lastDrawnCount = drawCount;
	}

	public override void MouseMoving(MouseEventArgs e)
	{
		base.MouseMoving(e);
		if (!Mouse.Inside)
		{
			Sprites["sel"].Visible = false;
			HoveringIndex = -1;
			return;
		}
		int rx = e.X - Viewport.X;
		int ry = e.Y - Viewport.Y;
		int index = (int) Math.Floor(ry / 54d);
		if (rx < 30 || rx >= 340 || ry <= 0 || index >= lastDrawnCount || ry % 54 >= 38 || index >= Editor.GeneralSettings.RecentFiles.Count)
		{
			Sprites["sel"].Visible = false;
			HoveringIndex = -1;
			return;
		}
		Sprites["sel"].Visible = true;
		Sprites["sel"].Y = 2 + index * 54;
		HoveringIndex = index;
	}

	public override void MouseDown(MouseEventArgs e)
	{
		base.MouseDown(e);
		if (HoveringIndex != -1) OnProjectClicked?.Invoke(new BaseEventArgs());
	}
}
