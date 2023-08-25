using System;
using System.Collections.Generic;

namespace RPGStudioMK.Game;

public class System
{
    public List<string> Variables = new List<string>();
    public List<string> Switches = new List<string>();

    public int MagicNumber;
    public int _;
    public int StartMapID;
    public int StartX;
    public int StartY;
    public int EditMapID;
    public int TestTroopID;
    public int BattlerHue;
    public string GameOverName;
    public string TitleName;
    public string WindowskinName;
    public string BattleBackName;
    public string BattlerName;
    public string BattleTransition;

    public AudioFile CancelSE;
    public AudioFile EscapeSE;
    public AudioFile BattleEndME;
    public AudioFile ShopSE;
    public AudioFile DecisionSE;
    public AudioFile BattleStartSE;
    public AudioFile BattleBGM;
    public AudioFile EquipSE;
    public AudioFile EnemyCollapseSE;
    public AudioFile CursorSE;
    public AudioFile LoadSE;
    public AudioFile TitleBGM;
    public AudioFile BuzzerSE;
    public AudioFile ActorCollapseSE;
    public AudioFile GameOverME;
    public AudioFile SaveSE;

    public System()
    {

    }

    public System(IntPtr data)
    {
        this.MagicNumber = (int)Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@magic_number"));
        this._ = (int)Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@_"));
        this.StartMapID = (int)Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@start_map_id"));
        this.StartX = (int)Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@start_x"));
        this.StartY = (int)Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@start_y"));
        this.EditMapID = (int)Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@edit_map_id"));
        this.TestTroopID = (int)Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@test_troop_id"));
        this.BattlerHue = (int)Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@battler_hue"));

        // Ignoring @words, saving dummy data on system save
        // Ignoring @elements, saving dummy data on system save
        // Ignoring @test_battlers, saving dummy data on system save
        // Ignoring @party_members, saving dummy data on system save

        this.GameOverName = Ruby.String.FromPtr(Ruby.GetIVar(data, "@gameover_name"));
        this.TitleName = Ruby.String.FromPtr(Ruby.GetIVar(data, "@title_name"));
        this.WindowskinName = Ruby.String.FromPtr(Ruby.GetIVar(data, "@windowskin_name"));
        this.BattleBackName = Ruby.String.FromPtr(Ruby.GetIVar(data, "@battleback_name"));
        this.BattlerName = Ruby.String.FromPtr(Ruby.GetIVar(data, "@battler_name"));
        this.BattleTransition = Ruby.String.FromPtr(Ruby.GetIVar(data, "@battle_transition"));

        this.CancelSE = new AudioFile(Ruby.GetIVar(data, "@cancel_se"));
        this.EscapeSE = new AudioFile(Ruby.GetIVar(data, "@escape_se"));
        this.BattleEndME = new AudioFile(Ruby.GetIVar(data, "@battle_end_me"));
        this.ShopSE = new AudioFile(Ruby.GetIVar(data, "@shop_se"));
        this.DecisionSE = new AudioFile(Ruby.GetIVar(data, "@decision_se"));
        this.BattleStartSE = new AudioFile(Ruby.GetIVar(data, "@battle_start_se"));
        this.BattleBGM = new AudioFile(Ruby.GetIVar(data, "@battle_bgm"));
        this.EquipSE = new AudioFile(Ruby.GetIVar(data, "@equip_se"));
        this.EnemyCollapseSE = new AudioFile(Ruby.GetIVar(data, "@enemy_collapse_se"));
        this.CursorSE = new AudioFile(Ruby.GetIVar(data, "@cursor_se"));
        this.LoadSE = new AudioFile(Ruby.GetIVar(data, "@load_se"));
        this.TitleBGM = new AudioFile(Ruby.GetIVar(data, "@title_bgm"));
        this.BuzzerSE = new AudioFile(Ruby.GetIVar(data, "@buzzer_se"));
        this.ActorCollapseSE = new AudioFile(Ruby.GetIVar(data, "@actor_collapse_se"));
        this.GameOverME = new AudioFile(Ruby.GetIVar(data, "@gameover_me"));
        this.SaveSE = new AudioFile(Ruby.GetIVar(data, "@save_se"));

        IntPtr variables = Ruby.GetIVar(data, "@variables");
        for (int i = 0; i < Ruby.Array.Length(variables); i++)
        {
            IntPtr e = Ruby.Array.Get(variables, i);
            if (e != Ruby.Nil)
            {
                string name = Ruby.String.FromPtr(e);
                this.Variables.Add(name);
            }
        }

        IntPtr switches = Ruby.GetIVar(data, "@switches");
        for (int i = 0; i < Ruby.Array.Length(switches); i++)
        {
            IntPtr e = Ruby.Array.Get(switches, i);
            if (e != Ruby.Nil)
            {
                string name = Ruby.String.FromPtr(e);
                this.Switches.Add(name);
            }
        }
    }

    public IntPtr Save()
    {
        IntPtr s = Ruby.Funcall(Compatibility.RMXP.System.Class, "new");
        Ruby.Pin(s);

        IntPtr variables = Ruby.Array.Create();
        Ruby.SetIVar(s, "@variables", variables);
        for (int i = 0; i < this.Variables.Count; i++)
        {
            Ruby.Array.Set(variables, i + 1, Ruby.String.ToPtr(this.Variables[i]));
        }

        IntPtr switches = Ruby.Array.Create();
        Ruby.SetIVar(s, "@switches", switches);
        for (int i = 0; i < this.Switches.Count; i++)
        {
            Ruby.Array.Set(switches, i + 1, Ruby.String.ToPtr(this.Switches[i]));
        }

        Ruby.SetIVar(s, "@magic_number", Ruby.Integer.ToPtr(this.MagicNumber));
        Ruby.SetIVar(s, "@_", Ruby.Integer.ToPtr(this._));
        Ruby.SetIVar(s, "@start_map_id", Ruby.Integer.ToPtr(this.StartMapID));
        Ruby.SetIVar(s, "@start_x", Ruby.Integer.ToPtr(this.StartX));
        Ruby.SetIVar(s, "@start_y", Ruby.Integer.ToPtr(this.StartY));
        Ruby.SetIVar(s, "@edit_map_id", Ruby.Integer.ToPtr(this.EditMapID));
        Ruby.SetIVar(s, "@test_troop_id", Ruby.Integer.ToPtr(this.TestTroopID));
        Ruby.SetIVar(s, "@battler_hue", Ruby.Integer.ToPtr(this.BattlerHue));

        IntPtr words = Ruby.Funcall(Compatibility.RMXP.Words.Class, "new");
        Ruby.SetIVar(s, "@words", words);
        Ruby.SetIVar(words, "@str", Ruby.String.ToPtr(""));
        Ruby.SetIVar(words, "@armor3", Ruby.String.ToPtr(""));
        Ruby.SetIVar(words, "@mdef", Ruby.String.ToPtr(""));
        Ruby.SetIVar(words, "@gold", Ruby.String.ToPtr(""));
        Ruby.SetIVar(words, "@sp", Ruby.String.ToPtr(""));
        Ruby.SetIVar(words, "@skill", Ruby.String.ToPtr(""));
        Ruby.SetIVar(words, "@int", Ruby.String.ToPtr(""));
        Ruby.SetIVar(words, "@armor2", Ruby.String.ToPtr(""));
        Ruby.SetIVar(words, "@equip", Ruby.String.ToPtr(""));
        Ruby.SetIVar(words, "@hp", Ruby.String.ToPtr(""));
        Ruby.SetIVar(words, "@pdef", Ruby.String.ToPtr(""));
        Ruby.SetIVar(words, "@attack", Ruby.String.ToPtr(""));
        Ruby.SetIVar(words, "@agi", Ruby.String.ToPtr(""));
        Ruby.SetIVar(words, "@armor1", Ruby.String.ToPtr(""));
        Ruby.SetIVar(words, "@atk", Ruby.String.ToPtr(""));
        Ruby.SetIVar(words, "@item", Ruby.String.ToPtr(""));
        Ruby.SetIVar(words, "@dex", Ruby.String.ToPtr(""));
        Ruby.SetIVar(words, "@armor4", Ruby.String.ToPtr(""));
        Ruby.SetIVar(words, "@weapon", Ruby.String.ToPtr(""));
        Ruby.SetIVar(words, "@guard", Ruby.String.ToPtr(""));

        IntPtr testbattlers = Ruby.Array.Create(0);
        Ruby.SetIVar(s, "@test_battlers", testbattlers);
        IntPtr testbattler = Ruby.Funcall(Compatibility.RMXP.TestBattler.Class, "new");
        Ruby.Array.Set(testbattlers, 0, testbattler);
        Ruby.SetIVar(testbattler, "@actor_id", Ruby.Integer.ToPtr(1));
        Ruby.SetIVar(testbattler, "@weapon_id", Ruby.Integer.ToPtr(0));
        Ruby.SetIVar(testbattler, "@level", Ruby.Integer.ToPtr(1));
        Ruby.SetIVar(testbattler, "@armor4_id", Ruby.Integer.ToPtr(0));
        Ruby.SetIVar(testbattler, "@armor3_id", Ruby.Integer.ToPtr(0));
        Ruby.SetIVar(testbattler, "@armor2_id", Ruby.Integer.ToPtr(0));
        Ruby.SetIVar(testbattler, "@armor1_id", Ruby.Integer.ToPtr(0));

        Ruby.SetIVar(s, "@elements", Ruby.Array.Create(2, Ruby.String.ToPtr("")));
        Ruby.SetIVar(s, "@party_members", Ruby.Array.Create(1, Ruby.Integer.ToPtr(1)));

        Ruby.SetIVar(s, "@gameover_name", Ruby.String.ToPtr(this.GameOverName));
        Ruby.SetIVar(s, "@title_name", Ruby.String.ToPtr(this.TitleName));
        Ruby.SetIVar(s, "@windowskin_name", Ruby.String.ToPtr(this.WindowskinName));
        Ruby.SetIVar(s, "@battleback_name", Ruby.String.ToPtr(this.BattleBackName));
        Ruby.SetIVar(s, "@battler_name", Ruby.String.ToPtr(this.BattlerName));
        Ruby.SetIVar(s, "@battle_transition", Ruby.String.ToPtr(this.BattleTransition));

        Ruby.SetIVar(s, "@cancel_se", this.CancelSE.Save());
        Ruby.SetIVar(s, "@escape_se", this.EscapeSE.Save());
        Ruby.SetIVar(s, "@battle_end_me", this.BattleEndME.Save());
        Ruby.SetIVar(s, "@shop_se", this.ShopSE.Save());
        Ruby.SetIVar(s, "@decision_se", this.DecisionSE.Save());
        Ruby.SetIVar(s, "@battle_start_se", this.BattleStartSE.Save());
        Ruby.SetIVar(s, "@battle_bgm", this.BattleBGM.Save());
        Ruby.SetIVar(s, "@equip_se", this.EquipSE.Save());
        Ruby.SetIVar(s, "@enemy_collapse_se", this.EnemyCollapseSE.Save());
        Ruby.SetIVar(s, "@cursor_se", this.CursorSE.Save());
        Ruby.SetIVar(s, "@load_se", this.LoadSE.Save());
        Ruby.SetIVar(s, "@title_bgm", this.TitleBGM.Save());
        Ruby.SetIVar(s, "@buzzer_se", this.BuzzerSE.Save());
        Ruby.SetIVar(s, "@actor_collapse_se", this.ActorCollapseSE.Save());
        Ruby.SetIVar(s, "@gameover_me", this.GameOverME.Save());
        Ruby.SetIVar(s, "@save_se", this.SaveSE.Save());

        Ruby.Unpin(s);
        return s;
    }
}
