using System;
using System.Collections.Generic;
using System.Linq;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class HomeScreen : Widget
{
    public int SelectedIndex = -1;
    int RecentCapacity = 0;

    HomeScreenButton NewProjectButton;
    HomeScreenButton OpenProjectButton;
    HomeScreenButton TutorialsButton;

    VignetteFade VignetteFade;

    ImageBox YoutubeButton;
    ImageBox TwitterButton;

    MultilineLabel NoProjects;

    public HomeScreen(IContainer Parent) : base(Parent)
    {
        if (System.IO.File.Exists("assets/img/home_map.png"))
        {
            ImageBox npb = new ImageBox(this);
            npb.SetBitmap("assets/img/home_map.png");
            npb.SetFillMode(FillMode.FillMaintainAspectAndCenter);
            npb.SetZIndex(-1);
        }
        else SetBackgroundColor(28, 50, 73);

        Sprites["sidebar"] = new Sprite(this.Viewport, "assets/img/home_side.png");
        Sprites["logo"] = new Sprite(this.Viewport, "assets/img/home_logo.png");
        Sprites["logo"].X = 33;
        Sprites["logo"].Y = 4;
        Sprites["text"] = new Sprite(this.Viewport, new Bitmap(360, 160));
        Sprites["text"].Bitmap.Font = Fonts.UbuntuBold.Use(15);
        Sprites["text"].Bitmap.Unlock();
        Sprites["text"].Bitmap.DrawText(Editor.GetVersionString(), 348, 88, Color.WHITE, DrawOptions.RightAlign);
        Sprites["text"].Bitmap.Font = Fonts.UbuntuBold.Use(19);
        Sprites["text"].Bitmap.DrawText("Recent Projects:", 38, 126, Color.WHITE, DrawOptions.Underlined);
        Sprites["text"].Bitmap.Lock();
        Sprites["filesel"] = new Sprite(this.Viewport, new SolidBitmap(2, 38, new Color(0, 205, 255)));
        Sprites["filesel"].X = 30;
        Sprites["filesel"].Visible = false;
        Sprites["files"] = new Sprite(this.Viewport);
        Sprites["files"].X = 42;
        Sprites["files"].Y = 168;

        NewProjectButton = new HomeScreenButton(this);
        NewProjectButton.SetPosition(445, 108);
        NewProjectButton.SetText("New Project");
        NewProjectButton.SetIcon("assets/img/home_icon_new");
        NewProjectButton.SetHelpText("Create a new project.");
        NewProjectButton.OnLeftMouseDownInside += _ => NewProject();

        OpenProjectButton = new HomeScreenButton(this);
        OpenProjectButton.SetPosition(690, 108);
        OpenProjectButton.SetText("Open Project");
        OpenProjectButton.SetIcon("assets/img/home_icon_openfile");
        OpenProjectButton.SetHelpText("Open an existing project by selecting its project file.");
        OpenProjectButton.OnLeftMouseDownInside += _ => OpenProject();

        TutorialsButton = new HomeScreenButton(this);
        TutorialsButton.SetPosition(935, 108);
        TutorialsButton.SetText("Tutorials");
        TutorialsButton.SetHelpText("Click this button to be directed to various tutorials and documentation for RPG Studio MK.");
        TutorialsButton.SetIcon("assets/img/home_icon_tutorials");
        TutorialsButton.OnLeftMouseDownInside += _ => ShowTutorials();

        YoutubeButton = new ImageBox(this);
        YoutubeButton.SetBitmap("assets/img/home_icon_youtube.png");
        YoutubeButton.SetHelpText("Visit RPG Studio MK's YouTube account.");
        YoutubeButton.OnLeftMouseDownInside += _ => new MessageBox("Oops!", "MK does not have a YouTube channel yet!", IconType.Error);
        YoutubeButton.OnHoverChanged += _ =>
        {
            YoutubeButton.DisposeBitmap();
            if (YoutubeButton.Mouse.Inside) YoutubeButton.SetBitmap("assets/img/home_icon_youtube_hover.png");
            else YoutubeButton.SetBitmap("assets/img/home_icon_youtube.png");
        };

        TwitterButton = new ImageBox(this);
        TwitterButton.SetBitmap("assets/img/home_icon_twitter.png");
        TwitterButton.SetHelpText("Visit MK's Twitter account.");
        TwitterButton.OnLeftMouseDownInside += _ => Utilities.OpenLink("http://twitter.com/RPGStudioMK");
        TwitterButton.OnHoverChanged += _ =>
        {
            TwitterButton.DisposeBitmap();
            if (TwitterButton.Mouse.Inside) TwitterButton.SetBitmap("assets/img/home_icon_twitter_hover.png");
            else TwitterButton.SetBitmap("assets/img/home_icon_twitter.png");
        };

        NoProjects = new MultilineLabel(this);
        NoProjects.SetSize(320, 100);
        NoProjects.SetPosition(40, 170);
        NoProjects.SetText("You haven't opened any projects recently.\nGet started by creating or opening a project!");
        NoProjects.SetFont(Fonts.CabinMedium.Use(12));

        VignetteFade = new VignetteFade(this);
        VignetteFade.SetDocked(true);
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);

        #region Sidebar
        Sprites["sidebar"].SrcRect.Height = Sprites["sidebar"].Bitmap.Height;
        if (Size.Height <= Sprites["sidebar"].Bitmap.Height) Sprites["sidebar"].SrcRect.Height = Size.Height;
        else
        {
            double factor = Size.Height / (double)Sprites["sidebar"].Bitmap.Height;
            Sprites["sidebar"].ZoomY = factor;
        }
        #endregion

        #region Recent Files
        if (Sprites["files"].Bitmap != null) Sprites["files"].Bitmap.Dispose();
        int height = Size.Height - 190;
        if (height < 1) return;
        RecentCapacity = (int)Math.Floor(height / 48d);
        Sprites["files"].Bitmap = new Bitmap(314, 48 * RecentCapacity);
        Sprites["files"].Bitmap.Unlock();
        Font boldfont = Fonts.UbuntuBold.Use(13);
        Font regularfont = Fonts.CabinMedium.Use(11);
        int count = Editor.GeneralSettings.RecentFiles.Count;
        for (int i = 0; i < count; i++)
        {
            if (i >= RecentCapacity) break;
            string name = Editor.GeneralSettings.RecentFiles[count - i - 1].ProjectName;
            string projectpath = Editor.GeneralSettings.RecentFiles[count - i - 1].ProjectFile;
            while (projectpath.Contains("\\")) projectpath = projectpath.Replace("\\", "/");
            Sprites["files"].Bitmap.Font = boldfont;
            Sprites["files"].Bitmap.DrawText(name, 0, 48 * i + 4, Color.WHITE);
            Sprites["files"].Bitmap.Font = regularfont;
            List<string> folders = projectpath.Split('/').ToList();
            string path = "";
            for (int j = folders.Count - 2; j >= 0; j--)
            {
                string add = "/";
                if (folders[j].Contains(":")) add = "";
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
            Sprites["files"].Bitmap.DrawText(path, 30, 48 * i + 22, Color.WHITE);
        }
        Sprites["files"].Bitmap.Lock();
        NoProjects.SetVisible(Editor.GeneralSettings.RecentFiles.Count == 0);
        #endregion

        #region Buttons
        int windowheight = Window.Height;

        bool Hor3 = true;
        bool CenterY = false;
        bool Hor2Ver1 = false;
        bool Ver3 = false;
        bool Invis = false;
        bool HorSM = true;
        bool VerSM = false;
        bool InvisSM = false;

        if (Size.Width < 1180)
        {
            Hor3 = false;
            Hor2Ver1 = true;
        }
        if (Size.Width < 930)
        {
            Hor2Ver1 = false;
            Ver3 = true;
            HorSM = false;
            VerSM = true;
        }
        if (Size.Width < 860)
        {
            VerSM = false;
            InvisSM = true;
        }
        if (Size.Width < 780)
        {
            Ver3 = false;
            Invis = true;
            InvisSM = false;
            VerSM = true;
        }

        if (Hor3 && HorSM && windowheight < 500)
        {
            HorSM = false;
            InvisSM = true;
            CenterY = true;
            if (Size.Width < 1210)
            {
                Hor3 = false;
                CenterY = false;
                VerSM = true;
                InvisSM = false;
                Invis = true;
            }
            else if (Size.Width >= 1300)
            {
                VerSM = true;
                InvisSM = false;
            }
        }

        if (Hor2Ver1 && windowheight < 650)
        {
            Hor2Ver1 = false;
            Invis = true;
            HorSM = false;
            VerSM = true;
        }

        if (Ver3)
        {
            if (windowheight < 720)
            {
                Ver3 = false;
                Invis = true;
                HorSM = false;
                VerSM = true;
            }
            else if (windowheight >= 810)
            {
                VerSM = false;
                HorSM = true;
                InvisSM = false;
            }
        }

        NewProjectButton.SetVisible(true);
        OpenProjectButton.SetVisible(true);
        TutorialsButton.SetVisible(true);
        if (Hor3)
        {
            int y = CenterY ? Size.Height / 2 - NewProjectButton.Size.Height / 2 : 108;
            int addx = CenterY ? 30 : 0;
            NewProjectButton.SetPosition(445 + addx, y);
            OpenProjectButton.SetPosition(690 + addx, y);
            TutorialsButton.SetPosition(935 + addx, y);
        }
        else if (Hor2Ver1)
        {
            NewProjectButton.SetPosition(445, 108);
            OpenProjectButton.SetPosition(690, 108);
            TutorialsButton.SetPosition(445, 329);
        }
        else if (Ver3)
        {
            NewProjectButton.SetPosition(520, 4);
            OpenProjectButton.SetPosition(520, 210);
            TutorialsButton.SetPosition(520, 415);
        }
        else if (Invis)
        {
            NewProjectButton.SetVisible(false);
            OpenProjectButton.SetVisible(false);
            TutorialsButton.SetVisible(false);
        }

        YoutubeButton.SetVisible(true);
        TwitterButton.SetVisible(true);
        if (HorSM)
        {
            YoutubeButton.SetPosition(Size.Width - 89, Size.Height - 87);
            TwitterButton.SetPosition(Size.Width - 182, Size.Height - 87);
        }
        else if (VerSM)
        {
            YoutubeButton.SetPosition(Size.Width - 89, Size.Height - 87);
            TwitterButton.SetPosition(Size.Width - 89, Size.Height - 180);
        }
        else if (InvisSM)
        {
            YoutubeButton.SetVisible(false);
            TwitterButton.SetVisible(false);
        }
        #endregion
    }

    public override void HoverChanged(MouseEventArgs e)
    {
        base.HoverChanged(e);
        if (!Mouse.Inside)
        {
            Sprites["filesel"].Visible = false;
            SelectedIndex = -1;
        }
    }

    public override void MouseMoving(MouseEventArgs e)
    {
        base.MouseMoving(e);
        if (!Mouse.Inside)
        {
            Sprites["filesel"].Visible = false;
            SelectedIndex = -1;
            return;
        }
        int rx = e.X - Viewport.X;
        int ry = e.Y - Viewport.Y - Sprites["files"].Y;
        int index = (int)Math.Floor(ry / 48d);
        if (rx < 30 || rx >= 340 || ry <= 0 || index >= RecentCapacity || ry % 48 >= 38 || index >= Editor.GeneralSettings.RecentFiles.Count)
        {
            Sprites["filesel"].Visible = false;
            SelectedIndex = -1;
            return;
        }
        Sprites["filesel"].Visible = true;
        Sprites["filesel"].Y = 2 + Sprites["files"].Y + index * 48;
        SelectedIndex = index;
    }

    public override void MouseDown(MouseEventArgs e)
    {
        base.MouseDown(e);
        if (Sprites["filesel"].Visible && SelectedIndex > -1)
        {
            LoadRecentProject(SelectedIndex);
        }
    }

    public void NewProject()
    {
        Editor.NewProject();
    }

    public void OpenProject()
    {
        Editor.OpenProject();
    }

    public void LoadRecentProject(int index)
    {
        string ProjectFilePath = Editor.GeneralSettings.RecentFiles[Editor.GeneralSettings.RecentFiles.Count - index - 1].ProjectFile;
        if (!System.IO.File.Exists(ProjectFilePath))
        {
            MessageBox mbox = new MessageBox("Not Found", "No project could be found at that location.\nDo you want to delete the project from the list of recent projects?", ButtonType.YesNoCancel, IconType.Warning);
            mbox.OnClosed += _ =>
            {
                if (mbox.Result == 0) // Yes, remove project from list of recent projects
                {
                    Editor.GeneralSettings.RecentFiles.RemoveAt(Editor.GeneralSettings.RecentFiles.Count - index - 1);
                    SizeChanged(new BaseEventArgs());
                }
            };
        }
        else
        {
            Data.SetProjectPath(ProjectFilePath);
            ((MainEditorWindow)Window).CreateEditor();
            Editor.MakeRecentProject();
        }
    }

    public void ShowTutorials()
    {
        new MessageBox("Oops!", "MK does not have a dedicated wiki yet. You may find the information you're looking for on Discord, Twitter or GitHub, however.", IconType.Error);
    }
}
