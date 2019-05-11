using System;
using System.Collections.Generic;
using MKEditor.Widgets;
using ODL;

namespace MKEditor
{
    public class WidgetWindow : Window
    {
        public UIManager UI;

        public WidgetWindow()
        {
            this.Initialize();

            this.SetBackgroundColor(240, 240, 240);

            this.UI = new UIManager(this);

            /*GroupBox box = new GroupBox(this);
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

            TextBox txt = new TextBox(box);
            txt.SetPosition(10, 150);
            txt.SetSize(100, 20);

            RadioButton rbtn1 = new RadioButton(box);
            rbtn1.SetPosition(10, 180);

            RadioButton rbtn2 = new RadioButton(box);
            rbtn2.SetPosition(10, 200);*/

            GroupBox panel = new GroupBox(this);
            panel.SetPosition(100, 100);
            panel.SetSize(90, 90);

            Button btn = new Button(panel);
            btn.SetPosition(-5, -5);
            btn.SetSize(100, 100);

            Console.WriteLine(btn.Viewport.ToString());

            this.OnMouseDown += UI.MouseDown;
            this.OnMousePress += UI.MousePress;
            this.OnMouseUp += UI.MouseUp;
            this.OnMouseMoving += UI.MouseMoving;
            this.OnMouseWheel += UI.MouseWheel;
            this.OnTextInput += UI.TextInput;

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
