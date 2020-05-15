using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MKEditor.Game
{
    public class BasicCondition
    {
        public ConditionType Type;
        public string Identifier;
        public Dictionary<string, object> Parameters;

        public BasicCondition()
        {

        }

        public BasicCondition(ConditionType Type, string Identifier, Dictionary<string, object> Parameters)
        {
            this.Type = Type;
            this.Identifier = Identifier;
            this.Parameters = Parameters;
        }

        public static BasicCondition IDToCondition(string Identifier, Dictionary<string, object> Parameters)
        {
            string nativeid = Identifier.Substring(1);
            ConditionType type = ConditionParser.Types.Find(t => t.Identifier == nativeid);
            if (type == null) throw new Exception($"Invalid condition identifier: '{nativeid}'");
            return new BasicCondition(type, Identifier, Parameters);
        }

        public List<object> ToJSON()
        {
            List<object> Data = new List<object>();
            Data.Add(Identifier);
            Data.Add(Parameters);
            return Data;
        }

        public bool EvaluateBooleanExpression(string Expression, ConditionUIParser Parser)
        {
            if (Expression.Contains("&"))
            {
                List<string> ands = Expression.Split('&').ToList();
                for (int i = 0; i < ands.Count; i++)
                {
                    bool result = EvaluateBooleanExpression(ands[i], Parser);
                    if (!result) return false;
                }
                return true;
            }
            bool normal = !Expression.StartsWith('!');
            if (!normal) Expression = Expression.Substring(1);
            if (Parameters.ContainsKey(":" + Expression))
            {
                object value = Parameters[":" + Expression];
                if (value is false || value == null) return normal ? false : true;
                return normal ? true : false;
            }
            else if (Regex.IsMatch(Expression, @"([a-zA-Z0-9_]*=[a-zA-Z0-9_:])|([a-zA-Z0-9_]*![a-zA-Z0-9_:])"))
            {
                char c = Expression.Contains('=') ? '=' : '!';
                string varname = Expression.Substring(0, Expression.IndexOf(c));
                string value = Expression.Substring(Expression.IndexOf(c) + 1, Expression.Length - Expression.IndexOf(c) - 1);
                string variable = null;
                if (!Parameters.ContainsKey(":" + varname))
                {
                    Widgets.Widget w = Parser.GetWidgetFromName(varname);
                    string Identifier = Parser.GetIdentifierFromName(varname);
                    if (w == null) return false;
                    variable = w.GetValue(Identifier).ToString();
                }
                else variable = Parameters[":" + varname].ToString();
                bool result = variable == value;
                if (c == '!') result = !result;
                return normal ? result : !result;
            }
            else if (Regex.IsMatch(Expression, @"[a-zA-Z0-9_]*\?[a-zA-Z0-9_:]"))
            {
                string varname = Expression.Substring(0, Expression.IndexOf('?'));
                string value = Expression.Substring(Expression.IndexOf('?') + 1, Expression.Length - Expression.IndexOf('?') - 1);
                object variable = null;
                if (!Parameters.ContainsKey(":" + varname))
                {
                    Widgets.Widget w = Parser.GetWidgetFromName(varname);
                    string Identifier = Parser.GetIdentifierFromName(varname);
                    if (w == null) return false;
                    variable = w.GetValue(Identifier);
                }
                else variable = Parameters[":" + varname];
                bool result = false;
                if (value == "string") result = variable is string;
                else if (value == "int") result = variable is int || variable is long;
                else if (value == "hash")
                    result = variable is Dictionary<string, object>;
                else if (value == "array") result = variable is List<object>;
                else throw new Exception($"Invalid type: '{value}'");
                return normal ? result : !result;
            }
            else
            {
                Widgets.Widget w = Parser.GetWidgetFromName(Expression);
                if (w == null) return true;
                object v = w.GetValue(Parser.GetIdentifierFromName(Expression));
                if (v == null || v is bool && (bool) v == false) return normal ? false : true;
                return normal ? true : false;
            }
        }

        public object EvaluateExpression(string Expression, ConditionUIParser Parser = null)
        {
            if (Regex.IsMatch(Expression, @"[a-zA-Z0-9_\?]*->[\-a-zA-Z0-9_@:\.]*\|[\-a-zA-Z0-9_@:\.]*"))
            {
                int leftidx = Expression.IndexOf('-');
                int mididx = Expression.IndexOf('|');
                string left = Expression.Substring(0, leftidx);
                string mid = Expression.Substring(leftidx + 2, mididx - leftidx - 2);
                string right = Expression.Substring(mididx + 1, Expression.Length - mididx - 1);
                if (EvaluateBooleanExpression(left, Parser)) return EvaluateExpression(mid, Parser);
                else return EvaluateExpression(right, Parser);
            }
            else if (Regex.IsMatch(Expression, @"switch\([a-zA-Z0-9_]*, [a-zA-Z0-9_]*\)"))
            {
                int pid = Expression.IndexOf('(');
                int cid = Expression.IndexOf(',');
                string arg1 = Expression.Substring(pid + 1, cid - pid - 1);
                string arg2 = Expression.Substring(cid + 2, Expression.Length - cid - 3);
                int group_id = -1;
                int switch_id = -1;
                if (Parameters.ContainsKey(":" + arg1)) group_id = (int) Parameters[":" + arg1];
                if (Parameters.ContainsKey(":" + arg2)) switch_id = (int) Parameters[":" + arg2];
                if (group_id != -1 && switch_id != -1)
                {
                    return Editor.ProjectSettings.Switches[group_id - 1].Switches[switch_id - 1].Name;
                }
            }
            else if (Regex.IsMatch(Expression, @"variable\([a-zA-Z0-9_]*, [a-zA-Z0-9_]*\)"))
            {
                int pid = Expression.IndexOf('(');
                int cid = Expression.IndexOf(',');
                string arg1 = Expression.Substring(pid + 1, cid - pid - 1);
                string arg2 = Expression.Substring(cid + 2, Expression.Length - cid - 3);
                int group_id = -1;
                int variable_id = -1;
                if (Parameters.ContainsKey(":" + arg1)) group_id = (int) Parameters[":" + arg1];
                if (Parameters.ContainsKey(":" + arg2)) variable_id = (int) Parameters[":" + arg2];
                if (group_id != -1 && variable_id != -1)
                {
                    return Editor.ProjectSettings.Variables[group_id - 1].Variables[variable_id - 1].Name;
                }
            }
            else
            {
                string type = null;
                string settings = null;
                string identifier = null;
                if (Expression.Contains('@'))
                {
                    settings = Expression.Substring(Expression.IndexOf('@') + 1);
                    Expression = Expression.Substring(0, Expression.IndexOf('@'));
                }
                if (Expression.Contains('?'))
                {
                    type = Expression.Substring(Expression.IndexOf('?') + 1);
                    Expression = Expression.Substring(0, Expression.IndexOf('?'));
                }
                if (Expression.Contains('.'))
                {
                    identifier = Expression.Substring(Expression.IndexOf('.') + 1);
                    Expression = Expression.Substring(0, Expression.IndexOf('.'));
                }
                if (Parameters.ContainsKey(":" + Expression))
                {
                    object param = Parameters[":" + Expression];
                    if (!string.IsNullOrEmpty(identifier))
                    {
                        if (!(param is Dictionary<string, object>)) throw new Exception($"Failed to apply identifier '{identifier}' to parameter of type '{param.GetType().Name}'.");
                        param = ((Dictionary<string, object>) param)[":" + identifier];
                    }
                    if (!string.IsNullOrEmpty(settings))
                    {
                        if (param is int)
                        {
                            if (Utilities.IsNumeric(settings)) return Utilities.Digits((int) param, Convert.ToInt32(settings));
                        }
                        else if (param is bool)
                        {
                            if (settings == "ON") return (bool) param ? "ON" : "OFF";
                        }
                    }
                    if (!string.IsNullOrEmpty(type))
                    {
                        bool result = false;
                        if (type == "int") result = param is int || param is long;
                        else if (type == "string") result = param is string;
                        else if (type == "hash") result = param is Dictionary<string, object> || param is Dictionary<object, object>;
                        else if (type == "array") result = param is List<object>;
                        else throw new Exception($"Invalid type: '{type}'");
                        return result ? "true" : "false";
                    }
                    if (param == null) return "";
                    return param;
                }
                else if (Parser != null)
                {
                    Widgets.Widget w = Parser.GetWidgetFromName(Expression);
                    if (w == null) return Expression;
                    return w.GetValue(Parser.GetIdentifierFromName(Expression));
                }
            }
            return Expression;
        }

        public string ToString(ConditionUIParser Parser)
        {
            string Str = "";
            string format = null;
            if (this.Type.Text is Dictionary<string, string>)
            {
                foreach (string condition in ((Dictionary<string, string>) this.Type.Text).Keys)
                {
                    if (EvaluateBooleanExpression(condition, Parser))
                    {
                        format = ((Dictionary<string, string>) this.Type.Text)[condition];
                        break;
                    }
                }
            }
            else if (this.Type.Text is string)
            {
                format = (string) this.Type.Text;
            }
            if (format == null) return "";
            for (int i = 0; i < format.Length; i++)
            {
                if (format[i] == '[' && (i == 0 || format[i - 1] != '\\') && format[i + 1] == 'c' && format[i + 2] == '=')
                {
                    i += 3;
                    int endidx = -1;
                    for (int j = i; j < format.Length; j++)
                    {
                        if ((j == 0 || format[j - 1] != '\\') && format[j] == ']')
                        {
                            endidx = j;
                            break;
                        }
                    }
                    int length = endidx - i;
                    string color = format.Substring(i, length);
                    if (!Utilities.IsNumeric(color))
                    {
                        color = (string) EvaluateExpression(color);
                    }
                    Str += $"[c={color}]";
                    i += length;
                }
                else if (format[i] == '{' && (i == 0 || format[i - 1] != '\\'))
                {
                    i += 1;
                    int endidx = -1;
                    for (int j = i; j < format.Length; j++)
                    {
                        if (format[j] == '}' && (j == 0 || format[j - 1] != '\\'))
                        {
                            endidx = j;
                            break;
                        }
                    }
                    int length = endidx - i;
                    string var = format.Substring(i, length);
                    Str += EvaluateExpression(var);
                    i += length;
                }
                else if (format[i] == '\\' && (i == 0 || format[i - 1] != '\\'))
                {

                }
                else
                {
                    Str += format[i];
                }
            }
            return Str;
        }

        public BasicCondition Clone()
        {
            BasicCondition bc = new BasicCondition();
            bc.Type = this.Type;
            bc.Identifier = this.Identifier;
            bc.Parameters = new Dictionary<string, object>(this.Parameters);
            return bc;
        }
    }
}
