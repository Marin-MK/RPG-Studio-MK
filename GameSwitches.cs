using System;
using System.Collections.Generic;
using System.Text;

namespace RPGStudioMK
{
    [Serializable]
    public class GameSwitchGroup
    {
        public int ID;
        public string Name;
        public List<GameSwitch> Switches = new List<GameSwitch>();
        public int SwitchCapacity = 25;

        public GameSwitchGroup()
        {
            for (int i = 0; i < SwitchCapacity; i++) Switches.Add(new GameSwitch() { ID = i + 1 });
        }

        public override string ToString()
        {
            return $"{Utilities.Digits(ID, 3)}: {Name?? ""}";
        }
    }

    [Serializable]
    public class GameSwitch
    {
        public int ID;
        public string Name;

        public override string ToString()
        {
            return $"{Utilities.Digits(ID, 3)}: {Name ?? ""}";
        }
    }
}
