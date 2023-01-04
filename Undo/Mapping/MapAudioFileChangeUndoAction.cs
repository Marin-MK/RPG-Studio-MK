using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPGStudioMK.Game;
using RPGStudioMK.Widgets;

namespace RPGStudioMK.Undo;

public class MapAudioFileChangeUndoAction : BaseUndoAction
{
    public override string Title => $"Changed map {(IsBGM ? "BGM" : "BGS")}";
    public override string Description
    {
        get
        {
            string s = $"Map: {(Data.Maps.ContainsKey(MapID) ? Data.Maps[MapID].Name : Utilities.Digits(MapID, 3))}\n";
            s += $"Old {(IsBGM ? "BGM" : "BGS")}: ";
            s += $"(name: '{OldAudioFile.Name}', volume: {OldAudioFile.Volume}, pitch: {OldAudioFile.Pitch})\n";
            s += $"New {(IsBGM ? "BGM" : "BGS")}: ";
            s += $"(name: '{NewAudioFile.Name}', volume: {NewAudioFile.Volume}, pitch: {NewAudioFile.Pitch})\n\n";
            s += $"Old Autoplay: {OldAutoplay}\nNew Autoplay: {NewAutoplay}";
            return s;
        }
    }

    public int MapID;
    public AudioFile OldAudioFile;
    public bool OldAutoplay;
    public AudioFile NewAudioFile;
    public bool NewAutoplay;
    public bool IsBGM;

    public MapAudioFileChangeUndoAction(int MapID, AudioFile OldAudioFile, bool OldAutoplay, AudioFile NewAudioFile, bool NewAutoplay, bool IsBGM)
    {
        this.MapID = MapID;
        this.OldAudioFile = OldAudioFile;
        this.OldAutoplay = OldAutoplay;
        this.NewAudioFile = NewAudioFile;
        this.NewAutoplay = NewAutoplay;
        this.IsBGM = IsBGM;
    }

    public static void Create(int MapID, AudioFile OldAudioFile, bool OldAutoplay, AudioFile NewAudioFile, bool NewAutoplay, bool IsBGM)
    {
        var c = new MapAudioFileChangeUndoAction(MapID, OldAudioFile, OldAutoplay, NewAudioFile, NewAutoplay, IsBGM);
        c.Register();
    }

    public override bool Trigger(bool IsRedo)
    {
        // Ensure we're in the Mapping mode
        bool Continue = true;
        if (!InMode(EditorMode.Mapping))
        {
            SetMappingMode(MapMode.Tiles);
            Continue = false;
        }
        // Ensure we're on the map this action was taken on
        if (Editor.MainWindow.MapWidget.Map.ID != MapID)
        {
            Editor.MainWindow.MapWidget.SetMap(Data.Maps[this.MapID]);
            Continue = false;
        }
        if (!Continue) return false;
        TriggerLogical(IsRedo);
        return true;
    }

    public override void TriggerLogical(bool IsRedo)
    {
        if (IsRedo)
        {
            if (IsBGM)
            {
                Data.Maps[this.MapID].BGM = NewAudioFile;
                Data.Maps[this.MapID].AutoplayBGM = NewAutoplay;
            }
            else
            {
                Data.Maps[this.MapID].BGS = NewAudioFile;
                Data.Maps[this.MapID].AutoplayBGS = NewAutoplay;
            }
        }
        else
        {
            if (IsBGM)
            {
                Data.Maps[this.MapID].BGM = OldAudioFile;
                Data.Maps[this.MapID].AutoplayBGM = OldAutoplay;
            }
            else
            {
                Data.Maps[this.MapID].BGS = OldAudioFile;
                Data.Maps[this.MapID].AutoplayBGS = OldAutoplay;
            }
        }
    }
}
