using System;
using rubydotnet;

namespace RPGStudioMK.Game
{
    [Serializable]
    public class AudioFile : ICloneable
    {
        public string Name = "";
        public int Volume = 100;
        public int Pitch = 100;

        public AudioFile()
        {
            
        }

        public AudioFile(IntPtr data)
        {
            this.Name = Ruby.String.FromPtr(Ruby.GetIVar(data, "@name"));
            this.Volume = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@volume"));
            this.Pitch = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@pitch"));
        }

        public object Clone()
        {
            AudioFile f = new AudioFile();
            f.Name = this.Name;
            f.Volume = this.Volume;
            f.Pitch = this.Pitch;
            return f;
        }

        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            if (obj is AudioFile)
            {
                AudioFile af = (AudioFile) obj;
                return this.Name == af.Name &&
                       this.Volume == af.Volume &&
                       this.Pitch == af.Pitch;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public IntPtr Save()
        {
            IntPtr af = Ruby.Funcall(Compatibility.RMXP.AudioFile.Class, "new");
            Ruby.Pin(af);
            Ruby.SetIVar(af, "@name", Ruby.String.ToPtr(this.Name));
            Ruby.SetIVar(af, "@volume", Ruby.Integer.ToPtr(this.Volume));
            Ruby.SetIVar(af, "@pitch", Ruby.Integer.ToPtr(this.Pitch));
            Ruby.Unpin(af);
            return af;
        }
    }
}
