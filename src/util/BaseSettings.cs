using System.Collections.Generic;
using System.Text.Json;

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
            if (!RawData.ContainsKey(kvp.Key))
            {
                RawData.Add(kvp.Key, kvp.Value);
            }
            else if (RawData[kvp.Key] is JsonElement)
                RawData[kvp.Key] = ((JsonElement) RawData[kvp.Key]).Deserialize(kvp.Value.GetType());
        }
        List<string> KeysToRemove = new List<string>();
        foreach (KeyValuePair<string, object> kvp in RawData)
        {
            if (!Schema.ContainsKey(kvp.Key))
                KeysToRemove.Add(kvp.Key);
        }
        for (int i = 0; i < KeysToRemove.Count; i++)
        {
            RawData.Remove(KeysToRemove[i]);
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