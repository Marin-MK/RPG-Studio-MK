using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Game;

public class Evolution : ICloneable
{
    public SpeciesResolver Species;
    public string Type;
    public object Parameter;
    public bool Prevolution;

    private Evolution() { }

    public Evolution(SpeciesResolver Species, string Type, object Parameter, bool Prevolution = false)
    {
        this.Species = Species;
        this.Type = Type;
        this.Parameter = Parameter;
        this.Prevolution = Prevolution;
    }

    public Evolution(nint Array)
    {
        this.Species = (SpeciesResolver)Ruby.Symbol.FromPtr(Ruby.Array.Get(Array, 0));
        string rtype = Ruby.Symbol.FromPtr(Ruby.Array.Get(Array, 1));
        this.Type = Data.HardcodedData.Assert(rtype, Data.HardcodedData.EvolutionMethods);
        this.Parameter = Utilities.RubyToNative(Ruby.Array.Get(Array, 2));
        this.Prevolution = Ruby.Array.Get(Array, (int)Ruby.Array.Length(Array) - 1) == Ruby.True;
    }

    public nint Save()
    {
        nint e = Ruby.Array.Create();
        Ruby.Pin(e);
        Ruby.Array.Push(e, Ruby.Symbol.ToPtr(Species));
        Ruby.Array.Push(e, Ruby.Symbol.ToPtr(this.Type));
        Ruby.Array.Push(e, Utilities.NativeToRuby(this.Parameter));
        Ruby.Array.Push(e, Prevolution ? Ruby.True : Ruby.False);
        Ruby.Unpin(e);
        return e;
    }

    public object Clone()
    {
        Evolution e = new Evolution();
        e.Species = (SpeciesResolver)this.Species.ID;
        e.Type = this.Type;
        e.Parameter = this.Parameter; // Will realistically always be a primitive data type
        e.Prevolution = this.Prevolution;
        return e;
    }
}