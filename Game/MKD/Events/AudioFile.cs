using System;
using rubydotnet;

namespace RPGStudioMK.Game
{
    public class AudioFile
    {
        public string Name;
        public int Volume;
        public int Pitch;

        public AudioFile()
        {
            this.Volume = 100;
            this.Pitch = 100;
        }

        public AudioFile(IntPtr data)
        {
            this.Name = Ruby.String.FromPtr(Ruby.GetIVar(data, "@name"));
            this.Volume = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@volume"));
            this.Pitch = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@pitch"));
        }
    }
}
