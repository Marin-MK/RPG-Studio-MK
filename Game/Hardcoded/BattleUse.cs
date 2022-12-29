using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Game;

public enum BattleUse
{
    None = 0,
    OnPokemon = 1,
    OnMove = 2,
    OnBattler = 3,
    OnFoe = 4,
    Direct = 5
}
