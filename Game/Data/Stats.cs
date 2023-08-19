using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Game;

public struct Stats : ICloneable
{
    public int HP;
    public int Attack;
    public int Defense;
    public int SpecialAttack;
    public int SpecialDefense;
    public int Speed;
	public int Total => this.HP + this.Attack + this.Defense + this.SpecialAttack + this.SpecialDefense + this.Speed;

	public Stats()
    {

    }

    public Stats(List<int> Stats)
    {
        if (Stats.Count != 6) throw new Exception("Invalid length of stats List.");
        this.HP = Stats[0];
        this.Attack = Stats[1];
        this.Defense = Stats[2];
        this.SpecialAttack = Stats[3];
        this.SpecialDefense = Stats[4];
        this.Speed = Stats[5];
    }

    public Stats(int Stats)
    {
        this.HP = this.Attack = this.Defense = this.SpecialAttack = this.SpecialDefense = this.Speed = Stats;
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

    public object Clone()
    {
        Stats s = new Stats();
        s.HP = this.HP;
        s.Attack = this.Attack;
        s.Defense = this.Defense;
        s.SpecialAttack = this.SpecialAttack;
        s.SpecialDefense = this.SpecialDefense;
        s.Speed = this.Speed;
        return s;
    }
}
