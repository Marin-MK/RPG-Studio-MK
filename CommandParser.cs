using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MKEditor
{
    public static class CommandParser
    {
        public static List<ODL.Color> Colors;
        public static List<CommandHeader> Headers;
        public static List<CommandType> Types;

        public static void Initialize(string Filename = "commands.json")
        {
            StreamReader sr = new StreamReader(File.OpenRead(Filename));
            string content = sr.ReadToEnd();
            sr.Close();
            Dictionary<string, object> Data = ((JObject) JsonConvert.DeserializeObject(content)).ToObject<Dictionary<string, object>>();
            if (!Data.ContainsKey("headers")) throw new Exception("Command definition JSON must contain a 'headers' key.");
            if (!Data.ContainsKey("types")) throw new Exception("Command definition JSON must contain a 'types' key.");
            foreach (string key in Data.Keys)
            {
                object value = Data[key];
                switch (key)
                {
                    case "colors":
                        if (!(value is JArray)) throw new Exception($"Expected an array, but got a {value.GetType().Name} in key 'colors'.");
                        Colors = ParseColors(((JArray) value).ToObject<List<object>>());
                        break;
                    case "headers":
                        if (!(value is JArray)) throw new Exception($"Expected an array, but got a {value.GetType().Name} in key 'headers'.");
                        Headers = ParseHeaders(((JArray) value).ToObject<List<object>>());
                        break;
                    case "types":
                        if (!(value is JObject)) throw new Exception($"Expected an object, but got a {value.GetType().Name} in key 'types'.");
                        Types = ParseTypes(((JObject) value).ToObject<Dictionary<string, object>>());
                        break;
                    default:
                        throw new Exception($"Unknown key in command definition JSON: '{key}'");
                }
            }
            foreach (CommandHeader h in Headers)
            {
                foreach (string typeid in h.Types)
                {
                    if (Types.Find(t => t.Identifier == typeid) == null)
                        throw new Exception($"Unknown command type '{typeid}' in header '{h.Name}'");
                }
            }
        }

        public static List<ODL.Color> ParseColors(List<object> colors)
        {
            List<ODL.Color> Colors = new List<ODL.Color>();
            for (int i = 0; i < colors.Count; i++)
            {
                object c = colors[i];
                if (!(c is JObject)) throw new Exception($"Expected an object, but got a {c.GetType().Name} in key 'color', element {i}.");
                Colors.Add(ParseColor(((JObject) c).ToObject<Dictionary<string, object>>()));
            }
            return Colors;
        }

        public static ODL.Color ParseColor(Dictionary<string, object> colorobject)
        {
            if (!colorobject.ContainsKey("red")) throw new Exception($"Color definition must have a 'red' key.");
            if (!colorobject.ContainsKey("green")) throw new Exception($"Color definition must have a 'green' key.");
            if (!colorobject.ContainsKey("blue")) throw new Exception($"Color definition must have a 'blue' key.");
            byte R = 0;
            byte G = 0;
            byte B = 0;
            byte A = 255;
            foreach (string colorkey in colorobject.Keys)
            {
                object colorvalue = colorobject[colorkey];
                switch (colorkey)
                {
                    case "red":
                        EnsureType(typeof(long), colorvalue, "red");
                        int red = Convert.ToInt32(colorvalue);
                        if (red < 0 || red > 255) throw new Exception($"Red color value {red} must be in the range of 0-255.");
                        R = (byte) red;
                        break;
                    case "green":
                        EnsureType(typeof(long), colorvalue, "green");
                        int green = Convert.ToInt32(colorvalue);
                        if (green < 0 || green > 255) throw new Exception($"Green color value {green} must be in the range of 0-255.");
                        G = (byte) green;
                        break;
                    case "blue":
                        EnsureType(typeof(long), colorvalue, "blue");
                        int blue = Convert.ToInt32(colorvalue);
                        if (blue < 0 || blue > 255) throw new Exception($"Blue color value {blue} must be in the range of 0-255.");
                        B = (byte) blue;
                        break;
                    case "alpha":
                        EnsureType(typeof(long), colorvalue, "alpha");
                        int alpha = Convert.ToInt32(colorvalue);
                        if (alpha < 0 || alpha > 255) throw new Exception($"Alpha color value {alpha} must be in the range of 0-255.");
                        A = (byte) alpha;
                        break;
                    default:
                        throw new Exception($"Unknown key in color definition: '{colorkey}'");
                }
            }
            return new ODL.Color(R, G, B, A);
        }

        public static List<CommandHeader> ParseHeaders(List<object> headers)
        {
            List<CommandHeader> Headers = new List<CommandHeader>();
            for (int i = 0; i < headers.Count; i++)
            {
                object h = headers[i];
                if (!(h is JObject)) throw new Exception($"Expected an object, but got a {h.GetType().Name} in key 'headers', element {i}.");
                Headers.Add(ParseHeader(((JObject) h).ToObject<Dictionary<string, object>>()));
            }
            return Headers;
        }

        public static CommandHeader ParseHeader(Dictionary<string, object> headerobject)
        {
            string Name = null;
            List<string> Types = new List<string>();
            foreach (string headerkey in headerobject.Keys)
            {
                object headervalue = headerobject[headerkey];
                switch (headerkey)
                {
                    case "name":
                        EnsureType(typeof(string), headervalue, "name");
                        Name = (string) headervalue;
                        break;
                    case "types":
                        if (!(headervalue is JArray)) throw new Exception($"Expected an object, but got a {headervalue.GetType().Name} in key 'types'.");
                        List<object> types = ((JArray) headervalue).ToObject<List<object>>();
                        for (int i = 0; i < types.Count; i++)
                        {
                            object type = types[i];
                            if (!(type is string)) throw new Exception($"Expected a string, but got a {type.GetType().Name} in key 'types', element {i}.");
                            Types.Add((string) type);
                        }
                        break;
                    default:
                        throw new Exception($"Unknown key in header definition: '{headerkey}'");
                }
            }
            return new CommandHeader(Name, Types);
        }

        public static List<CommandType> ParseTypes(Dictionary<string, object> types)
        {
            List<CommandType> Types = new List<CommandType>();
            foreach (string identifier in types.Keys)
            {
                object t = types[identifier];
                if (!(t is JObject)) throw new Exception($"Expected an object, but got a {t.GetType().Name} in key '{identifier}'.");
                CommandType Type = ParseType(((JObject) t).ToObject<Dictionary<string, object>>());
                Type.Identifier = identifier;
                Types.Add(Type);
            }
            return Types;
        }

        public static CommandType ParseType(Dictionary<string, object> typeobject)
        {
            string Name = null;
            object Text = null;
            List<CommandParameter> Parameters = new List<CommandParameter>();
            Dictionary<string, object> UI = new Dictionary<string, object>();
            if (!typeobject.ContainsKey("name")) throw new Exception($"Command type definition must contain a 'name' key.");
            foreach (string typekey in typeobject.Keys)
            {
                object typevalue = typeobject[typekey];
                switch (typekey)
                {
                    case "name":
                        EnsureType(typeof(string), typevalue, "name");
                        Name = (string) typevalue;
                        break;
                    case "parameters":
                        if (!(typevalue is JObject)) throw new Exception($"Expected an object, but got a {typevalue.GetType().Name} in key 'parameters'.");
                        Dictionary<string, object> parameters = ((JObject) typevalue).ToObject<Dictionary<string, object>>();
                        foreach (string paramkey in parameters.Keys)
                        {
                            object paramvalue = parameters[paramkey];
                            EnsureType(typeof(string), paramvalue, paramkey);
                            string type = (string) paramvalue;
                            string defaultvalue = null;
                            if (type.Contains("->"))
                            {
                                defaultvalue = type.Substring(type.IndexOf("->") + 2, type.Length - type.IndexOf("->") - 2);
                                type = type.Substring(0, type.IndexOf("->"));
                            }
                            if (type != "string" && type != "int" &&
                                type != "bool" && type != "object") throw new Exception($"Unknown data type '{type}' in key '{paramkey}'.");
                            Parameters.Add(new CommandParameter(paramkey, type, defaultvalue));
                        }
                        break;
                    case "ui":
                        if (!(typevalue is JObject)) throw new Exception($"Expected an object, but got a {typevalue.GetType().Name} in key 'ui'.");
                        UI = ((JObject) typevalue).ToObject<Dictionary<string, object>>();
                        if (!UI.ContainsKey("readonly")) throw new Exception($"Command UI definition must contain a 'readonly' key.");
                        break;
                    default:
                        throw new Exception($"Unknown key in command type definition: '{typekey}'");
                }
            }
            return new CommandType(null, Name, Text, Parameters, UI);
        }

        public static void EnsureType(Type type, object obj, string key)
        {
            if (obj.GetType() != type) throw new Exception($"Expected a {type.Name}, but got a {obj.GetType().Name} in key '{key}'.");
        }
    }

    public class CommandHeader
    {
        public string Name;
        public List<string> Types;

        public CommandHeader(string Name, List<string> Types)
        {
            this.Name = Name;
            this.Types = Types;
        }
    }

    public class CommandType
    {
        public string Identifier;
        public string Name;
        public object Text;
        public List<CommandParameter> Parameters;
        public Dictionary<string, object> UI;

        public CommandType(string Identifier, string Name, object Text, List<CommandParameter> Parameters, Dictionary<string, object> UI)
        {
            this.Identifier = Identifier;
            this.Name = Name;
            this.Text = Text;
            this.Parameters = Parameters;
            this.UI = UI;
        }

        public Dictionary<string, object> CreateBlankParameters()
        {
            Dictionary<string, object> Data = new Dictionary<string, object>();
            for (int i = 0; i < Parameters.Count; i++)
            {
                Data[":" + Parameters[i].Name] = Parameters[i].DefaultValue;
            }
            return Data;
        }
    }

    public class CommandParameter
    {
        public string Name;
        public string DataType;
        public object DefaultValue;

        public CommandParameter(string Name, string DataType, string DefaultValue)
        {
            this.Name = Name;
            this.DataType = DataType;
            if (!string.IsNullOrEmpty(DefaultValue))
            {
                if (DataType == "int")
                {
                    if (Utilities.IsNumeric(DefaultValue)) this.DefaultValue = Convert.ToInt32(DefaultValue);
                    else throw new Exception($"Command parameter '{Name}' expects a default value of type '{DataType}'");
                }
                else if (DataType == "bool")
                {
                    if (DefaultValue.ToLower() == "true" || DefaultValue.ToLower() == "t" || DefaultValue.ToLower() == "yes" || DefaultValue.ToLower() == "y")
                        this.DefaultValue = true;
                    else if (DefaultValue.ToLower() == "false" || DefaultValue.ToLower() == "f" || DefaultValue.ToLower() == "no" || DefaultValue.ToLower() == "n")
                        this.DefaultValue = false;
                    else throw new Exception($"Command parameter '{Name}' expects a default value of type '{DataType}'");
                }
                else if (DataType == "string")
                {
                    this.DefaultValue = DefaultValue;
                }
                else if (DataType == "object")
                {
                    if (DefaultValue == "null") this.DefaultValue = null;
                    else if (Utilities.IsNumeric(DefaultValue)) this.DefaultValue = Convert.ToInt32(DefaultValue);
                    else if (DefaultValue.ToLower() == "true" || DefaultValue.ToLower() == "false") this.DefaultValue = Convert.ToBoolean(DefaultValue);
                    else this.DefaultValue = DefaultValue;
                }
                else
                {
                    throw new Exception($"No default value available for data type '{DataType}'");
                }
            }
        }
    }
}
