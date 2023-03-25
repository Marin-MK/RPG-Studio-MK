using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Game;

public class DataManager
{
    private List<BaseDataManager> dataManagers;
    private bool firstTime = true;

    public DataManager(List<BaseDataManager> dataManagers)
    {
        this.dataManagers = dataManagers;
        int tilesetIdx = dataManagers.FindIndex(x => x is TilesetManager);
        int mapIdx = dataManagers.FindIndex(x => x is MapManager);
        if (tilesetIdx >= mapIdx) throw new Exception("TilesetManager must occur before MapManager");
    }

    private void CreateClasses()
    {
        dataManagers.ForEach(x => x.InitializeClass());
    }

    public void Load(bool fromPBS)
    {
        if (firstTime)
        {
            CreateClasses();
            firstTime = false;
        }
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
