using RPGStudioMK.Utility;
using RPGStudioMK.Widgets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using static rubydotnet.Ruby;

namespace RPGStudioMK.Game;

public class BaseDataManager
{
    public static Dictionary<string, nint> Classes = new Dictionary<string, nint>();
    static nint GameDataModule;

    public string ClassName;
    public string Filename;
    public string PBSFilename;
    public string Message;

    public BaseDataManager(string ClassName, string Filename, string PBSFilename, string Message)
    {
        this.ClassName = ClassName;
        this.Filename = Filename;
        this.PBSFilename = PBSFilename;
        this.Message = Message;
    }

    public virtual void InitializeClass()
    {
        if (GameDataModule == nint.Zero) GameDataModule = Ruby.Module.Define("GameData");
        if (ClassName == null) return;
        if (!Classes.ContainsKey(ClassName))
        {
            Classes.Add(ClassName, Ruby.Class.Define(ClassName, GameDataModule, null));
        }
    }

    protected nint GetClass()
    {
        return Classes[ClassName];
    }

    protected virtual void LoadAsHash(Action<nint, nint> OnItemLoaded)
    {
        SafeLoad(Filename, File =>
        {
            nint Data = Marshal.Load(File);
            Ruby.Pin(Data);
            nint Keys = Ruby.Hash.Keys(Data);
            Ruby.Pin(Keys);
            int KeyCount = (int) Ruby.Array.Length(Keys);
            for (int i = 0; i < KeyCount; i++)
            {
                nint key = Ruby.Array.Get(Keys, i);
                nint robj = Ruby.Hash.Get(Data, key);
                OnItemLoaded(key, robj);
                Game.Data.SetLoadProgress((float) i / (KeyCount - 1));
                if (Game.Data.StopLoading) break;
            }
            Ruby.Unpin(Keys);
            Ruby.Unpin(Data);
        });
    }

    protected virtual void LoadAsArray(Action<nint> OnItemLoaded, bool StartAt1 = false)
    {
        SafeLoad(Filename, File =>
        {
            IntPtr list = Ruby.Marshal.Load(File);
            Ruby.Pin(list);
            int ArrayLength = (int) Ruby.Array.Length(list);
            for (int i = (StartAt1 ? 1 : 0); i < ArrayLength; i++)
            {
                nint robj = Ruby.Array.Get(list, i);
                OnItemLoaded(robj);
                Data.SetLoadProgress((float) i / (ArrayLength - 1));
                if (Game.Data.StopLoading) break;
            }
            Ruby.Unpin(list);
        });
    }

    protected virtual void SaveDataAsHash<T>(IEnumerable<T> Collection, Func<T, nint> OnKeySaved) where T : IGameData
    {
        SafeSave(Filename, File =>
        {
            nint Data = Ruby.Hash.Create();
            Ruby.Pin(Data);
            foreach (T t in Collection)
            {
                nint adata = t.Save();
                nint key = OnKeySaved(t);
                Ruby.Hash.Set(Data, key, adata);
            }
            Ruby.Marshal.Dump(Data, File);
            Ruby.Unpin(Data);
        });
    }

    protected virtual void SaveDataAsArray<T>(IEnumerable<T> Collection, bool StartAt1 = false) where T : IGameData
    {
        SafeSave(Filename, File =>
        {
            IntPtr list = Ruby.Array.Create();
            Ruby.Pin(list);
            int idx = StartAt1 ? 1 : 0;
            foreach (T t in Collection)
            {
                Ruby.Array.Set(list, idx, t.Save());
                idx++;
            }
            Ruby.Marshal.Dump(list, File);
            Ruby.Unpin(list);
        });
    }

    protected virtual void SaveAsPBS<T>(IEnumerable<T> Collection) where T : IGameData
    {
        StreamWriter sw = new StreamWriter(global::System.IO.File.Open(Data.ProjectPath + "/PBS/" + this.PBSFilename, FileMode.Create));
        foreach (var item in Collection)
        {
            sw.Write(item.SaveToString());
        }
        sw.Close();
    }

    public virtual void Load(bool fromPBS)
    {
        Data.SetLoadText($"Loading {Message}...");
        if (fromPBS) LoadPBS();
        else LoadData();
    }

    protected virtual void LoadData()
    {
        
    }

    protected virtual void LoadPBS()
    {
        FormattedTextParser.RootFolder = Data.ProjectPath + "/PBS";
    }

    public virtual void Save()
    {
        SavePBS();
        SaveData();
    }

    protected virtual void SaveData()
    {

    }

    protected virtual void SavePBS()
    {

    }

    public virtual void Clear()
    {

    }

    protected static void LoadError(string File, string ErrorMessage)
    {
        string text = ErrorMessage switch
        {
            "Errno::EACCES" => $"RPG Studio MK was unable to load '{File}' because it was likely in use by another process.\nPlease try again.",
            "Errno::ENOENT" => $"RPG Studio MK was unable to load '{File}' because it does not exist.",
            "TypeError" => $"RPG Studio MK was unable to load '{File}' because it contains incorrect data. Are you sure this file has the correct name?",
            "EOFError" => $"RPG Studio MK was unable to load '{File}' because it was empty or contained invalid data.\nIt may be corrupt or outdated.",
            _ => $"RPG Studio MK was unable to load '{File}'.\n\n" + ErrorMessage + "\n\nPlease try again."
        };
        MessageBox mbox = new MessageBox("Error", text, ButtonType.OK, IconType.Error);
        Data.AbortLoad();
    }

    protected static void SaveError(string File, string ErrorMessage)
    {
        string text = ErrorMessage switch
        {
            "Errno::EACCES" => $"RPG Studio MK was unable to save '{File}' because it was likely in use by another process.\n\n" +
                                "All other data has been saved successfully. Please try again.",
            _ => $"RPG Studio MK was unable to save '{File}'.\n\n{ErrorMessage}\n\nAll other data has been saved successfully. Please try again."
        };
        MessageBox mbox = new MessageBox("Error", text, ButtonType.OK, IconType.Error);
        // Keep saving; prefer corrupting data if something is seriously wrong, which is doubtful, over
        // the prospect of losing all data in memory if the issue is only something minor, in a small section of the program.
    }

    protected static (bool Success, string Error) SafeLoad(string Filename, Action<IntPtr> Action, bool isAbsoluteFilename = false)
    {
        (bool Success, string Error) = SafelyOpenAndCloseFile(isAbsoluteFilename ? Filename : Data.DataPath + "/" + Filename, "rb", Action);
        if (!Success) LoadError(isAbsoluteFilename ? Filename : "Data/" + Filename, Error);
        return (Success, Error);
    }

    protected static (bool Success, string Error) SafeSave(string Filename, Action<IntPtr> Action, bool isAbsoluteFilename = false)
    {
        (bool Success, string Error) = SafelyOpenAndCloseFile(isAbsoluteFilename ? Filename : Data.DataPath + "/" + Filename, "wb", Action);
        if (!Success) SaveError(isAbsoluteFilename ? Filename : "Data/" + Filename, Error);
        return (Success, Error);
    }

    protected static (bool Success, string Error) SafelyOpenAndCloseFile(string Filename, string Mode, Action<IntPtr> Action, int Tries = 10, int DelayInMS = 40)
    {
        int Total = Tries;
        while (Tries > 0)
        {
            IntPtr File = IntPtr.Zero;
            if (Ruby.Protect(_ =>
            {
                File = Ruby.File.Open(Filename, Mode);
                Ruby.Pin(File);
                Action(File);
                Ruby.File.Close(File);
                File = IntPtr.Zero;
                Ruby.Unpin(File);
                return IntPtr.Zero;
            }))
            {
                if (Tries != Total)
                    Logger.WriteLine($"{Filename.Split('/').Last()} opened after {Total - Tries + 1} attempt(s) and {DelayInMS * (Total - Tries + 1)}ms.");
                if (File != IntPtr.Zero) Ruby.File.Close(File);
                return (true, null);
            }
            if (File != IntPtr.Zero) Ruby.File.Close(File);
            string ErrorType = Ruby.GetErrorType();
            if (ErrorType != "Errno::EACCES")
            {
                // Other error than simultaneous access, no point in retrying.
                return (false, Ruby.GetErrorText());
            }
            Thread.Sleep(DelayInMS);
            Tries--;
        }
        Logger.WriteLine($"{Filename.Split('/').Last()} failed to open after {Total} attempt(s) and {DelayInMS * Total}ms.");
        return (false, "Errno::EACCES");
    }
}

public interface IGameData
{
    public nint Save();
    public string SaveToString();
}