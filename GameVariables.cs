using System;
using System.Collections.Generic;
using System.Text;

namespace RPGStudioMK
{
    [Serializable]
    public class GameVariableGroup
    {
        public int ID;
        public string Name;
        public List<GameVariable> Variables = new List<GameVariable>();
        public int VariableCapacity = 25;

        public GameVariableGroup()
        {
            for (int i = 0; i < VariableCapacity; i++) Variables.Add(new GameVariable() { ID = i + 1 });
        }

        public override string ToString()
        {
            return $"{Utilities.Digits(ID, 3)}: {Name?? ""}";
        }
    }

    [Serializable]
    public class GameVariable
    {
        public int ID;
        public string Name;

        public override string ToString()
        {
            return $"{Utilities.Digits(ID, 3)}: {Name ?? ""}";
        }
    }
}
