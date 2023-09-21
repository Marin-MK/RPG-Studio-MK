using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

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

	public string SaveToString(bool writtenOut)
	{
        if (writtenOut)
        {
            List<string> values = new List<string>();
            if (this.HP != 0) values.Add($"HP,{this.HP}");
			if (this.Attack != 0) values.Add($"ATTACK,{this.Attack}");
			if (this.Defense != 0) values.Add($"DEFENSE,{this.Defense}");
			if (this.SpecialAttack != 0) values.Add($"SPECIAL_ATTACK,{this.SpecialAttack}");
			if (this.SpecialDefense != 0) values.Add($"SPECIAL_DEFENSE,{this.SpecialDefense}");
			if (this.Speed != 0) values.Add($"SPEED,{this.Speed}");
            return values.Count > 0 ? values.Aggregate((a, b) => a + "," + b) : "";
		}
        return $"{HP},{Attack},{Defense},{Speed},{SpecialAttack},{SpecialDefense}";
    }

	public override bool Equals(object obj)
	{
        if (obj is Stats)
        {
            Stats s = (Stats) obj;
            return this.HP == s.HP &&
                this.Attack == s.Attack &&
                this.Defense == s.Defense &&
                this.SpecialAttack == s.SpecialAttack &&
                this.SpecialDefense == s.SpecialDefense &&
                this.Speed == s.Speed;
        }
        return false;
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
