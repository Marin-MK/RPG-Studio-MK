using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;

namespace RPGStudioMK.Game;

public class MapManager : BaseDataManager
{
    public MapManager() : base(null, null, null, "maps") { }

    /// <summary>
    /// Returns a list of map files in the given folder.
    /// </summary>
    private static List<(string, int)> GetMapIDs(string path)
    {
        List<(string, int)> Filenames = new List<(string, int)>();
        foreach (string file in Directory.GetFiles(path))
        {
            string realfile = file;
            while (realfile.Contains('\\')) realfile = realfile.Replace('\\', '/');
            string name = realfile.Split('/').Last();
            Match match = Regex.Match(name, @"Map(\d+).rxdata");
            if (match.Success)
                Filenames.Add((name, Convert.ToInt32(match.Groups[1].Value)));
        }
        // Subdirectories
        //foreach (string dir in Directory.GetDirectories(path))
        //{
        //    string realdir = dir;
        //    while (realdir.Contains('\\')) realdir = realdir.Replace('\\', '/');
        //    Filenames.AddRange(GetMapIDs(realdir));
        //}
        return Filenames;
    }

    public override void Load(bool fromPBS = false)
    {
        base.Load(fromPBS);
        Logger.Write("Loading maps");
        SafeLoad("MapInfos.rxdata", InfoFile =>
        {
            IntPtr mapinfo = Ruby.Marshal.Load(InfoFile);
            Ruby.Pin(mapinfo);
            List<(string, int)> Filenames = GetMapIDs(Data.DataPath);
            int total = Filenames.Count;
            int count = 0;
            foreach ((string name, int id) tuple in Filenames)
            {
                SafeLoad(tuple.name, MapFile =>
                {
                    IntPtr mapdata = Ruby.Marshal.Load(MapFile);
                    Ruby.Pin(mapdata);
                    int id = tuple.id;
                    Map map = new Map(id, mapdata, Ruby.Hash.Get(mapinfo, Ruby.Integer.ToPtr(id)));
                    Data.Maps[map.ID] = map;
                    Ruby.Unpin(mapdata);
                    count++;
                    Data.SetLoadProgress(count / (float)total);
                });
                if (Data.StopLoading) break;
            }
            Ruby.Unpin(mapinfo);
            Editor.AssignOrderToNewMaps();
        });
    }

    public override void Save()
    {
        base.Save();
        Logger.Write("Saving maps and map metadata");
        (bool Success, string Error) = SafeSave("MapInfos.rxdata", InfoFile =>
        {
            IntPtr mapinfos = Ruby.Hash.Create();
            Ruby.Pin(mapinfos);
            foreach (Map map in Data.Maps.Values)
            {
                SafeSave($"Map{Utilities.Digits(map.ID, 3)}.rxdata", MapFile =>
                {
                    IntPtr mapinfo = Ruby.Funcall(Compatibility.RMXP.MapInfo.Class, "new");
                    Ruby.Hash.Set(mapinfos, Ruby.Integer.ToPtr(map.ID), mapinfo);
                    Ruby.SetIVar(mapinfo, "@name", Ruby.String.ToPtr(map.Name));
                    Ruby.SetIVar(mapinfo, "@parent_id", Ruby.Integer.ToPtr(map.ParentID));
                    Ruby.SetIVar(mapinfo, "@order", Ruby.Integer.ToPtr(map.Order));
                    Ruby.SetIVar(mapinfo, "@expanded", map.Expanded ? Ruby.True : Ruby.False);
                    Ruby.SetIVar(mapinfo, "@scroll_x", Ruby.Integer.ToPtr(map.ScrollX));
                    Ruby.SetIVar(mapinfo, "@scroll_y", Ruby.Integer.ToPtr(map.ScrollY));

                    IntPtr mapdata = map.Save();
                    Ruby.Marshal.Dump(mapdata, MapFile);
                });
                if (Data.StopLoading) break;
            }
            Ruby.Marshal.Dump(mapinfos, InfoFile);
            Ruby.Unpin(mapinfos);
        });
        if (Success)
        {
            // Delete all maps that are not part of of the data anymore
            // At least, only if saving all the other maps and info was a success.
            foreach ((string filename, int id) map in GetMapIDs(Data.DataPath))
            {
                try
                {
                    if (!Data.Maps.ContainsKey(map.id)) File.Delete(Data.DataPath + "/" + map.filename);
                }
                catch (Exception)
                {
                    Console.WriteLine($"Failed to delete '{Data.DataPath}/{map.filename}'.");
                }
            }
        }
    }

    public override void Clear()
    {
        base.Clear();
        Logger.Write("Clearing map data");
        Data.Maps.Clear();
    }
}