using System;
using System.Collections.Generic;

namespace RPGStudioMK.Game;

public class DataManager
{
    private ScriptManager scriptManager;
    private List<BaseDataManager> dataManagers;
    
    public DataManager(List<BaseDataManager> dataManagers)
    {
        this.dataManagers = dataManagers;
        this.scriptManager = (ScriptManager) dataManagers.Find(x => x is ScriptManager);
        int tilesetIdx = dataManagers.FindIndex(x => x is TilesetManager);
        int mapIdx = dataManagers.FindIndex(x => x is MapManager);
        if (tilesetIdx >= mapIdx) throw new Exception("TilesetManager must occur before MapManager");
    }

    public void SaveScriptsRXDATA(string filename)
    {
        scriptManager.SaveScriptsRXDATA(filename);
    }

    public void Setup()
    {
        dataManagers.ForEach(x => x.InitializeClass());
    }

    public void Load(bool fromPBS)
    {
        dataManagers.ForEach(x =>
        {
            if (Data.StopLoading) return;
            x.Load(fromPBS);
        });
    }

    public void Save()
    {
        dataManagers.ForEach(x => x.Save());
    }

    public void Clear()
    {
        dataManagers.ForEach(x => x.Clear());
    }
}
