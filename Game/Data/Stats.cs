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

    public Stats(nint HashData)
    {
        this.HP = (int) Ruby.Integer.FromPtr(Ruby.Hash.Get(HashData, Ruby.Symbol.ToPtr("HP")));
        this.Attack = (int) Ruby.Integer.FromPtr(Ruby.Hash.Get(HashData, Ruby.Symbol.ToPtr("ATTACK")));
        this.Defense = (int) Ruby.Integer.FromPtr(Ruby.Hash.Get(HashData, Ruby.Symbol.ToPtr("DEFENSE")));
        this.SpecialAttack = (int) Ruby.Integer.FromPtr(Ruby.Hash.Get(HashData, Ruby.Symbol.ToPtr("SPECIAL_ATTACK")));
        this.SpecialDefense = (int) Ruby.Integer.FromPtr(Ruby.Hash.Get(HashData, Ruby.Symbol.ToPtr("SPECIAL_DEFENSE")));
        this.Speed = (int) Ruby.Integer.FromPtr(Ruby.Hash.Get(HashData, Ruby.Symbol.ToPtr("SPEED")));
    }
}
