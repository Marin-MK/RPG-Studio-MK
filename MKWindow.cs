using System;
using System.Collections.Generic;
using MKEditor.Widgets;
using ODL;

namespace MKEditor
{
    public class MKWindow : Window
    {
        public UIManager UI;

        public MKWindow()
        {
            this.Initialize();

            this.SetBackgroundColor(240, 240, 240);

            this.UI = new UIManager(this);

            GroupBox box = new GroupBox(this);
            box.SetPosition(130, 10);
            box.SetSize(300, 300);

            Button b = new Button(box);
            b.SetPosition(10, 20);
            b.SetSize(100, 60);

            CheckBox cbox = new CheckBox(box);
            cbox.SetPosition(10, 90);

            ProgressBar pbar = new ProgressBar(box);
            pbar.SetPosition(10, 120);
            pbar.SetSize(100, 20);
            pbar.SetValue(0.6745);

            ListBox list = new ListBox(box);
            list.SetPosition(120, 20);
            list.SetSize(120, 13 * 13 + 4);
            for (int i = 0; i < 999; i++)
            {
                list.AddItem("Item " + (i + 1).ToString());
            }

            VScrollBar vsbar = new VScrollBar(box);
            vsbar.SetPosition(260, 20);
            vsbar.SetSize(17, 120);

            this.OnMouseDown += UI.MouseDown;
            this.OnMousePress += UI.MousePress;
            this.OnMouseUp += UI.MouseUp;
            this.OnMouseMoving += UI.MouseMoving;
            this.OnMouseWheel += UI.MouseWheel;

            this.OnTick += Tick;

            this.UI.Update();

            this.Start();
        }

        private void Tick(object sender, EventArgs e)
        {
            this.UI.Update();
        }
    }
}
