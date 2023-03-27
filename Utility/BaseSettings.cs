using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RPGStudioMK;

public abstract class BaseSettings
{
    public abstract Dictionary<string, object> Schema { get; }

    public Dictionary<string, object> RawData;

    public BaseSettings(Dictionary<string, object> rawData)
    {
        this.RawData = rawData;
    }

    public BaseSettings()
    {
        RawData = new Dictionary<string, object>();
        foreach (KeyValuePair<string, object> kvp in Schema)
        {
            RawData.Add(kvp.Key, kvp.Value);
        }
    }

    public virtual void Update()
    {
        foreach (KeyValuePair<string, object> kvp in Schema)
        {
            if (RawData[kvp.Key] is JsonElement)
                RawData[kvp.Key] = ((JsonElement)RawData[kvp.Key]).Deserialize(kvp.Value.GetType());
        }
    }

    protected T Get<T>(string key)
    {
        return (T)RawData[key];
    }

    protected void Set(string key, object value)
    {
        RawData[key] = value;
    }
}