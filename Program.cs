using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using ODL;
using static SDL2.SDL;

namespace MKEditor
{
    public class Program
    {
        static void Main(params string[] args)
        {
            Graphics.Start();
            WidgetWindow win = new WidgetWindow();
            win.Show();
            while (Graphics.CanUpdate())
            {
                Graphics.Update();
            }
            Graphics.Stop();
        }
    }
}