using System;
using System.Collections.Generic;
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
            if (type == null) throw new Exception($"Invalid condition identifier: '{Identifier}'");
            return new BasicCondition(type, Identifier, Parameters);
        }

        public List<object> ToJSON()
        {
            List<object> Data = new List<object>();
            Data.Add(Identifier);
            Data.Add(Parameters);
            return Data;
        }

        public bool EvaluateBooleanExpression(string Expression)
        {
            if (Parameters.ContainsKey(":" + Expression))
            {
                object value = Parameters[":" + Expression];
                if (value is false || value == null) return false;
                return true;
            }
            return true;
        }

        public string EvaluateExpression(string Expression)
        {
            if (Regex.IsMatch(Expression, @"[a-zA-Z0-9_]*->[a-zA-Z0-9_@]*:[a-zA-Z0-9_@]*"))
            {
                int leftidx = Expression.IndexOf('-');
                int mididx = Expression.IndexOf(':');
                string left = Expression.Substring(0, leftidx);
                string mid = Expression.Substring(leftidx + 2, mididx - leftidx - 2);
                string right = Expression.Substring(mididx + 1, Expression.Length - mididx - 1);
                if (EvaluateBooleanExpression(left)) return EvaluateExpression(mid);
                else return EvaluateExpression(right);
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
                    return "Untitled Switch";
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
                    return "Untitled Variable";
                }
            }
            else
            {
                string settings = null;
                if (Expression.Contains('@'))
                {
                    settings = Expression.Substring(Expression.IndexOf('@') + 1);
                    Expression = Expression.Substring(0, Expression.IndexOf('@'));
                }
                if (Parameters.ContainsKey(":" + Expression))
                {
                    object param = Parameters[":" + Expression];
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
                    if (param == null) return "nil";
                    return param.ToString();
                }
            }
            return Expression;
        }

        public override string ToString()
        {
            string Str = "";
            // Script: [c=1]{code}
            string format = this.Type.Text;
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
                        color = EvaluateExpression(color);
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
