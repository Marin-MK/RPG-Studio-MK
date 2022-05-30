using System;

namespace RPGStudioMK.Game;

[Serializable]
public class AudioFile : ICloneable
{
    public string Name;
    public int Volume;
    public int Pitch;

    public AudioFile(string Name = "", int Volume = 80, int Pitch = 100)
    {
        this.Name = Name;
        this.Volume = Volume;
        this.Pitch = Pitch;
    }

    public AudioFile(IntPtr data)
    {
        this.Name = Ruby.String.FromPtr(Ruby.GetIVar(data, "@name"));
        this.Volume = (int)Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@volume"));
        this.Pitch = (int)Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@pitch"));
    }

    public object Clone()
    {
        return new AudioFile(this.Name, this.Volume, this.Pitch);
    }

    public override bool Equals(object obj)
    {
        if (this == obj) return true;
        if (obj is AudioFile)
        {
            AudioFile af = (AudioFile)obj;
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
