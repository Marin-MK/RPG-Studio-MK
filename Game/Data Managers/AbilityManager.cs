using RPGStudioMK.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static rubydotnet.Ruby;

namespace RPGStudioMK.Game;

public class AbilityManager : BaseDataManager
{
    public AbilityManager(bool FromPBS = false) :
        base("Ability", "abilities.dat", "abilities.txt", "abilitites", FromPBS) { }

    protected override void LoadData()
    {
        base.LoadData();
        LoadAsHash((key, value) =>
        {
            string ckey = Ruby.Symbol.FromPtr(key);
            Data.Abilities.Add(ckey, new Ability(value));
        });
    }

    protected override void LoadPBS()
    {
        base.LoadPBS();
        FormattedTextParser.ParseSectionBasedFile(PBSFilename, (id, hash) =>
        {
            Data.Abilities.Add(id, new Ability(id, hash));
        }, Data.SetLoadProgress);
    }

    protected override void SaveData()
    {
        base.SaveData();
        SaveAsHash(Data.Abilities.Values, abil => Ruby.Symbol.ToPtr(abil.ID));
    }

    public override void Clear()
    {
        base.Clear();
        Data.Abilities.Clear();
    }
}
