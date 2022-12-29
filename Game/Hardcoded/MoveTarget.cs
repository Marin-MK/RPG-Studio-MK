using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Game;

public enum MoveTarget
{
    None,
    User,
    NearAlly,
    UserOrNearAlly,
    AllAllies,
    UserAndAllies,
    NearFoe,
    RandomNearFoe,
    AllNearFoes,
    Foe,
    AllFoes,
    NearOther,
    AllNearOthers,
    Other,
    AllBattlers,
    UserSide,
    FoeSide,
    BothSides
}