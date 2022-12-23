using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Game;

public struct Stats
{
    public int HP;
    public int Attack;
    public int Defense;
    public int SpecialAttack;
    public int SpecialDefense;
    public int Speed;

    public Stats()
    {

    }

    public Stats(nint HashData)
    {
        this.HP = (int) Ruby.Integer.FromPtr(Ruby.Hash.Get(HashData, Ruby.Symbol.ToPtr("HP")));
        this.Attack = (int) Ruby.Integer.FromPtr(Ruby.Hash.Get(HashData, Ruby.Symbol.ToPtr("ATTACK")));
        this.Defense = (int) Ruby.Integer.FromPtr(Ruby.Hash.Get(HashData, Ruby.Symbol.ToPtr("DEFENSE")));
        this.SpecialAttack = (int) Ruby.Integer.FromPtr(Ruby.Hash.Get(HashData, Ruby.Symbol.ToPtr("SPECIAL_ATTACK")));
        this.SpecialDefense = (int) Ruby.Integer.FromPtr(Ruby.Hash.Get(HashData, Ruby.Symbol.ToPtr("SPECIAL_DEFENSE")));
        this.Speed = (int) Ruby.Integer.FromPtr(Ruby.Hash.Get(HashData, Ruby.Symbol.ToPtr("SPEED")));
    }

    public nint Save()
    {
        nint e = Ruby.Hash.Create();
        Ruby.Pin(e);
        Ruby.Hash.Set(e, Ruby.Symbol.ToPtr("HP"), Ruby.Integer.ToPtr(this.HP));
        Ruby.Hash.Set(e, Ruby.Symbol.ToPtr("ATTACK"), Ruby.Integer.ToPtr(this.Attack));
        Ruby.Hash.Set(e, Ruby.Symbol.ToPtr("DEFENSE"), Ruby.Integer.ToPtr(this.Defense));
        Ruby.Hash.Set(e, Ruby.Symbol.ToPtr("SPECIAL_ATTACK"), Ruby.Integer.ToPtr(this.SpecialAttack));
        Ruby.Hash.Set(e, Ruby.Symbol.ToPtr("SPECIAL_DEFENSE"), Ruby.Integer.ToPtr(this.SpecialDefense));
        Ruby.Hash.Set(e, Ruby.Symbol.ToPtr("SPEED"), Ruby.Integer.ToPtr(this.Speed));
        Ruby.Unpin(e);
        return e;
    }
}
