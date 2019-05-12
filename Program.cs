using System;
using ODL;

namespace MKEditor
{
    public class Program
    {
        static void Main(params string[] args)
        {
            Graphics.Start();
            WidgetWindow w = new WidgetWindow();
            w.Show();
            while (Graphics.CanUpdate())
            {
                Graphics.Update();
            }
            Graphics.Stop();
        }
    }
}
