using System;
using System.Collections.Generic;
using System.Text;

namespace MKEditor.Game
{
    public class Serializable
    {
        string path;

        public Serializable() { }

        public Serializable(string path)
        {
            this.path = path;
        }

        public dynamic Exec(string Code)
        {
            return Data.Exec(Code);
        }

        public string GetPath(string Variable, VariableType VarType = VariableType.Normal)
        {
            if (VarType == VariableType.Normal) return $"{path}.instance_eval {{ @{Variable} }}";
            else if (VarType == VariableType.Struct) return $"{path}.{Variable}";
            else if (VarType == VariableType.HashSymbol) return $"{path}[:{Variable}]";
            else if (VarType == VariableType.HashString) return $"{path}[\"{Variable}\"]";
            else if (VarType == VariableType.ArrayElement) return $"{path}[{Variable}]";
            return null;
        }

        public bool Nil(string Variable, VariableType VarType = VariableType.Normal)
        {
            return Data.Exec($"{GetPath(Variable, VarType)}.nil?");
        }

        public dynamic GetVar<T>(string Variable, VariableType VarType = VariableType.Normal)
        {
            if (Nil(Variable, VarType)) return null;
            dynamic obj = Data.Exec(GetPath(Variable, VarType));
            System.Type type = typeof(T);
            if (type == typeof(string))
            {
                byte[] _data = Encoding.Default.GetBytes(obj.ToString());
                string data = Encoding.UTF8.GetString(_data);
                return data;
            }
            else if (type == typeof(int))
            {
                return Convert.ToInt32(obj);
            }
            else if (type == typeof(float))
            {
                return (float) Convert.ToDouble(obj);
            }
            else if (type == typeof(byte))
            {
                return Convert.ToByte(obj);
            }
            else if (type == typeof(bool))
            {
                return Convert.ToBoolean(obj.ToString());
            }
            else if (type.IsEnum)
            {
                return (T) Convert.ToInt32(obj);
            }
            return obj;
        }

        public List<T> GetList<T>(string Variable, VariableType VarType = VariableType.Normal)
        {
            System.Type type = typeof(T);
            if (type == typeof(string))
            {
                object[] _obj = GetVar<object[]>(Variable, VarType).ToArray();
                List<string> obj = new List<string>(Array.ConvertAll(_obj, x =>
                {
                    byte[] _data = Encoding.Default.GetBytes(x.ToString());
                    string data = Encoding.UTF8.GetString(_data);
                    return data;
                }));
                return obj as List<T>;
            }
            else if (type == typeof(int))
            {
                object[] _obj = GetVar<object[]>(Variable, VarType).ToArray();
                List<int> obj = new List<int>(Array.ConvertAll(_obj, x =>
                {
                    return Convert.ToInt32(x);
                }));
                return obj as List<T>;
            }
            else if (type == typeof(object))
            {
                List<object> obj = new List<object>();
                if (Nil(Variable, VarType)) return null;
                foreach (object o in GetVar<object[]>(Variable, VarType).ToArray())
                {
                    obj.Add(o);
                }
                return obj as List<T>;
            }
            return null;
        }

        public List<T> GetKeys<T>(string Variable, VariableType VarType = VariableType.Normal)
        {
            System.Type type = typeof(T);
            object[] _obj = Data.Exec($"{GetPath(Variable, VarType)}.keys").ToArray();
            if (type == typeof(string))
            {
                List<string> obj = new List<string>(Array.ConvertAll(_obj, x =>
                {
                    byte[] _data = Encoding.Default.GetBytes(x.ToString());
                    string data = Encoding.UTF8.GetString(_data);
                    return data;
                }));
                return obj as List<T>;
            }
            else if (type == typeof(int))
            {
                List<int> obj = new List<int>(Array.ConvertAll(_obj, x => Convert.ToInt32(x)));
                return obj as List<T>;
            }
            return null;
        }

        public int GetCount(string Variable, VariableType VarType = VariableType.Normal)
        {
            return Data.Exec(GetPath(Variable, VarType)).ToArray().Length;
        }
    }

    public enum VariableType
    {
        Normal,
        Struct,
        HashSymbol,
        HashString,
        ArrayElement
    }
}
