using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using ODL;
using System.Text;
using MKEditor.Widgets;
using System.IO;
using MKEditor.Game;

namespace MKEditor
{
    public class ConditionUIParser : IUIParser
    {
        public ConditionHandlerWidget HandlerWidget;
        public int Width = -1;
        public int Height = -1;
        public Dictionary<string, Font> Fonts = new Dictionary<string, Font>();
        public Dictionary<string, Widget> MainWidgets = new Dictionary<string, Widget>();
        public Dictionary<string, Widget> WidgetLookup = new Dictionary<string, Widget>();
        public List<Dictionary<string, string>> Buttons = new List<Dictionary<string, string>>();
        public Dictionary<string, object> LoadFormat = new Dictionary<string, object>();
        public Dictionary<string, object> SaveFormat = new Dictionary<string, object>();
        public Dictionary<string, bool> EnabledLookup = new Dictionary<string, bool>();
        public BasicCondition Condition;
        public bool NeedUpdate = false;
        public bool LoadingData = false;
        public bool WasNull = false;

        public ConditionUIParser(Dictionary<string, object> UIData, ConditionHandlerWidget HandlerWidget)
        {
            if (UIData.Count == 0)
            {
                new MessageBox("Undefined UI", "No UI has been defined for this condition type, so it can't be edited or created.", ButtonType.OK, IconType.Error);
                return;
            }
            this.HandlerWidget = HandlerWidget;
            foreach (string Key in UIData.Keys)
            {
                object value = UIData[Key];
                switch (Key)
                {
                    //case "title":
                    //    EnsureType(typeof(string), value, "title");
                    //    this.Title = (string) value;
                    //    break;
                    case "width":
                        EnsureType(typeof(long), value, "width");
                        this.Width = Convert.ToInt32(value);
                        if (this.Width < 1) throw new Exception($"Window width ({this.Width}) must be greater than 0.");
                        break;
                    case "height":
                        EnsureType(typeof(long), value, "height");
                        this.Height = Convert.ToInt32(value);
                        if (this.Height < 1) throw new Exception($"Window height ({this.Height}) must be greater than 0.");
                        break;
                    case "fonts":
                        if (!(value is JObject)) throw new Exception($"Expected an Object, but got a {value.GetType().Name} in key 'fonts'");
                        Dictionary<string, object> fonts = ((JObject) value).ToObject<Dictionary<string, object>>();
                        foreach (string font in fonts.Keys)
                        {
                            if (!(fonts[font] is JObject)) throw new Exception($"Expected an Object, but got a {fonts[font].GetType().Name} in key '{font}' inside 'fonts'");
                            this.Fonts.Add(font, ParseFont(((JObject) fonts[font]).ToObject<Dictionary<string, object>>()));
                        }
                        break;
                    //case "buttons":
                    //    if (!(value is JArray)) throw new Exception($"Expected an Array, but got a {value.GetType().Name} in key 'buttons'");
                    //    List<object> buttons = ((JArray) value).ToObject<List<object>>();
                    //    foreach (object buttonobject in buttons)
                    //    {
                    //        if (!(buttonobject is JObject)) throw new Exception($"Expected an Object, but got a {buttonobject.GetType().Name} in key 'buttons'");
                    //        Dictionary<string, object> button = ((JObject) buttonobject).ToObject<Dictionary<string, object>>();
                    //        string type = null;
                    //        string name = null;
                    //        if (!button.ContainsKey("type")) throw new Exception($"Button definition must contain a 'type' key");
                    //        if (!button.ContainsKey("name")) throw new Exception($"Button definition must contain a 'name' key");
                    //        foreach (string buttonkey in button.Keys)
                    //        {
                    //            object buttonvalue = button[buttonkey];
                    //            switch (buttonkey)
                    //            {
                    //                case "type":
                    //                    EnsureType(typeof(string), buttonvalue, "type");
                    //                    type = (string) buttonvalue;
                    //                    if (type != "ok" && type != "cancel" && type != "apply") throw new Exception($"Unknown button type '{type}' in button definition");
                    //                    break;
                    //                case "name":
                    //                    EnsureType(typeof(string), buttonvalue, "name");
                    //                    name = (string) buttonvalue;
                    //                    break;
                    //                default:
                    //                    throw new Exception($"Unknown key '{buttonkey}' in button definition in key 'buttons'");
                    //            }
                    //        }
                    //        Buttons.Add(new Dictionary<string, string>() { { "type", type }, { "name", name } });
                    //    }
                    //    break;
                    case "widgets":
                        if (!(value is JObject)) throw new Exception($"Expected an Object, but got a {value.GetType().Name} in key 'widgets'");
                        ParseWidgets(HandlerWidget, ((JObject)value).ToObject<Dictionary<string, object>>());
                        break;
                    case "load":
                        if (!(value is JObject)) throw new Exception($"Expected an Object, but got a {value.GetType().Name} in key 'load'");
                        LoadFormat = (Dictionary<string, object>) Utilities.JsonToNative(value);
                        break;
                    case "save":
                        if (!(value is JObject)) throw new Exception($"Expected an Object, but got a {value.GetType().Name} in key 'save'");
                        SaveFormat = (Dictionary<string, object>) Utilities.JsonToNative(value);
                        break;
                    default:
                        throw new Exception($"Unknown key '{Key}' inside window definition.");
                }
            }
            //if (!string.IsNullOrEmpty(this.Title)) Window.SetTitle(this.Title);
            if (this.Width != -1) HandlerWidget.SetWidth(this.Width);
            if (this.Height != -1) HandlerWidget.SetHeight(this.Height);
            //if (Buttons.Count == 0) Buttons = new List<Dictionary<string, string>>() { new Dictionary<string, string>() { { "type", "cancel" }, { "name", "Cancel" } }, new Dictionary<string, string>() { { "type", "ok" }, { "name", "OK" } } };
            //foreach (Dictionary<string, string> button in Buttons)
            //{
            //    BaseEvent method = button["type"] == "ok" ? OK : (button["type"] == "apply" ? Apply : (BaseEvent) Cancel);
            //    Window.CreateButton(button["name"], method);
            //}
            //Window.Center();
        }

        //public void OK(BaseEventArgs e)
        //{
        //    Apply(e);
        //    this.Window.Close();
        //}

        //public void Apply(BaseEventArgs e)
        //{
        //    foreach (string key in SaveFormat.Keys)
        //    {
        //        string variable = key;
        //        string widget = (string) SaveFormat[key];
        //        string identifier = null;
        //        if (widget.Contains('.'))
        //        {
        //            identifier = widget.Substring(widget.IndexOf('.') + 1, widget.Length - widget.IndexOf('.') - 1);
        //            widget = widget.Substring(0, widget.IndexOf('.'));
        //        }
        //        if (!WidgetLookup.ContainsKey(widget)) throw new Exception($"Could not find widget with identifier '{widget}'");
        //        Widget w = WidgetLookup[widget];
        //        Condition.Parameters[":" + variable] = w.GetValue(identifier);
        //    }
        //    NeedUpdate = true;
        //}

        //public void Cancel(BaseEventArgs e)
        //{
        //    this.Condition = this.OldCondition;
        //    this.Window.Close();
        //}

        public Font ParseFont(Dictionary<string, object> Data)
        {
            string FontName = null;
            int FontSize = -1;
            foreach (string fontkey in Data.Keys)
            {
                switch (fontkey)
                {
                    case "name":
                        EnsureType(typeof(string), Data[fontkey], "name");
                        FontName = (string) Data[fontkey];
                        break;
                    case "size":
                        EnsureType(typeof(long), Data[fontkey], "size");
                        FontSize = Convert.ToInt32(Data[fontkey]);
                        if (FontSize < 0 || FontSize > 999) throw new Exception($"Font size ({FontSize}) must be between 1 and 999.");
                        break;
                    default:
                        throw new Exception($"Unknown key '{fontkey}' inside font definition.");
                }
            }
            return Font.Get("Fonts/" + FontName, FontSize);
        }

        public void ParseWidgets(IContainer ParentWidget, Dictionary<string, object> Data)
        {
            foreach (string name in Data.Keys)
            {
                if (!(Data[name] is JObject)) throw new Exception($"Expected an Object, but got {Data[name].GetType().Name} in key '{name}' inside 'widgets'");
                Dictionary<string, object> config = ((JObject) Data[name]).ToObject<Dictionary<string, object>>();
                if (!config.ContainsKey("type")) throw new Exception($"Widget definition must contain a 'type' key in widget '{name}'");
                if (!(config["type"] is string)) throw new Exception($"Widget type must be a string.");
                Widget w = null;
                string type = (string) config["type"];
                switch (type)
                {
                    case "container":
                        w = new Container(ParentWidget);
                        break;
                    case "label":
                        w = new DynamicLabel(ParentWidget);
                        break;
                    case "numeric":
                        w = new NumericBox(ParentWidget);
                        break;
                    case "textbox":
                        w = new TextBox(ParentWidget);
                        break;
                    case "button":
                        w = new Button(ParentWidget);
                        break;
                    case "dropdown":
                        w = new DropdownBox(ParentWidget);
                        break;
                    case "checkbox":
                        w = new CheckBox(ParentWidget);
                        break;
                    case "radiobox":
                        w = new RadioBox(ParentWidget);
                        break;
                    case "switch_picker":
                        w = new GameSwitchBox(ParentWidget);
                        break;
                    case "variable_picker":
                        w = new GameVariableBox(ParentWidget);
                        break;
                    case "multitextbox":
                        w = new MultilineTextBox(ParentWidget);
                        break;
                    case "multilabel":
                        w = new MultilineDynamicLabel(ParentWidget);
                        break;
                    default:
                        throw new Exception($"Unknown widget type '{type}' in widget '{name}'");
                }
                if (WidgetLookup.ContainsKey(name)) throw new Exception($"Two or more widgets with the same were found: '{name}'");
                WidgetLookup.Add(name, w);
                int X = 0;
                int Y = 0;
                int Width = -1;
                int Height = -1;
                string text = null;
                int wvalue = 0;
                int? min_value = null;
                int? max_value = null;
                Font font = null;
                List<ListItem> items = null;
                int idx = -1;
                bool enabled = true;
                List<ClickAction> clickactions = new List<ClickAction>();
                foreach (string key in config.Keys)
                {
                    if (key == "type") continue;
                    object value = config[key];
                    switch (key)
                    {
                        case "x":
                            EnsureType(typeof(long), value, "x");
                            X = Convert.ToInt32(value);
                            break;
                        case "y":
                            EnsureType(typeof(long), value, "y");
                            Y = Convert.ToInt32(value);
                            break;
                        case "width":
                            EnsureType(typeof(long), value, "width");
                            Width = Convert.ToInt32(value);
                            if (Width < 1) throw new Exception($"Widget width ({Width}) must be greater than 0.");
                            break;
                        case "height":
                            EnsureType(typeof(long), value, "height");
                            Height = Convert.ToInt32(value);
                            if (Height < 1) throw new Exception($"Widget height ({Height}) must be greater than 0.");
                            break;
                        case "font":
                            if (type != "label" && type != "textbox" && type != "button" && type != "dropdown" &&
                                type != "checkbox" && type != "radiobox" && type != "multitextbox" && type != "multilabel") throw new Exception($"Widget type ({type}) can not contain a 'font' field.");
                            if (value is string)
                            {
                                if (!Fonts.ContainsKey((string) value)) throw new Exception($"Undefined font name '{(string) value}");
                                font = Fonts[(string) value];
                            }
                            else if (value is JObject)
                            {
                                font = ParseFont(((JObject) value).ToObject<Dictionary<string, object>>());
                            }
                            else
                            {
                                throw new Exception($"Expected a String or Object, but got {value.GetType().Name} in 'font'");
                            }
                            break;
                        case "value":
                            if (type != "numeric") throw new Exception($"Widget type ({type}) can not contain a 'value' field.");
                            EnsureType(typeof(long), value, "value");
                            wvalue = Convert.ToInt32(value);
                            break;
                        case "min_value":
                            if (type != "numeric") throw new Exception($"Widget type ({type}) can not contain a 'min_value' field.");
                            EnsureType(typeof(long), value, "min_value");
                            min_value = Convert.ToInt32(value);
                            break;
                        case "max_value":
                            if (type != "numeric") throw new Exception($"Widget type ({type}) can not contain a 'max_value' field.");
                            EnsureType(typeof(long), value, "max_value");
                            max_value = Convert.ToInt32(value);
                            break;
                        case "text":
                            if (type != "label" && type != "textbox" && type != "button" && type != "checkbox" &&
                                type != "radiobox" && type != "multitextbox" && type != "multilabel") throw new Exception($"Widget type ({type}) can not contain a 'text' field.");
                            EnsureType(typeof(string), value, "text");
                            text = (string) value;
                            break;
                        case "items":
                            if (type != "dropdown") throw new Exception($"Widget type ({type}) can not contain an 'items' field.");
                            if (!(value is JArray)) throw new Exception($"Expected an Array, but got {value.GetType().Name} in key 'items'");
                            List<object> ary = ((JArray) value).ToObject<List<object>>();
                            items = new List<ListItem>();
                            foreach (object o in ary)
                            {
                                EnsureType(typeof(string), o, "items");
                                items.Add(new ListItem((string) o));
                            }
                            break;
                        case "index":
                            if (type != "dropdown") throw new Exception($"Widget type ({type}) can not contain an 'index' field.");
                            EnsureType(typeof(long), value, "index");
                            idx = Convert.ToInt32(value);
                            if (idx < 0) throw new Exception($"Index field must be greater than or equal to 0.");
                            break;
                        case "enabled":
                            if (type != "label" && type != "dropdown" && type != "switch_picker" && type != "variable_picker" &&
                                type != "checkbox" && type != "radiobox" && type != "textbox" && type != "numeric" &&
                                type != "button" && type != "multilabel") throw new Exception($"Widget type ({type}) can not contain an 'enabled' field.");
                            EnsureType(typeof(bool), value, "enabled");
                            enabled = Convert.ToBoolean(value);
                            break;
                        case "widgets":
                            if (!(value is JObject)) throw new Exception($"Expected an Object, but got a {value.GetType().Name} in key 'widgets'");
                            ParseWidgets(w, ((JObject) value).ToObject<Dictionary<string, object>>());
                            break;
                        case "clicked":
                            if (type != "radiobox" && type != "dropdown") throw new Exception($"Widget type ({type}) can not contain a 'clicked' field.");
                            if (!(value is JArray)) throw new Exception($"Expected an Array, but got {value.GetType().Name} in key 'clicked'");
                            List<object> clickdata = ((JArray) value).ToObject<List<object>>();
                            for (int i = 0; i < clickdata.Count; i++)
                            {
                                object action = clickdata[i];
                                if (!(action is JObject)) throw new Exception($"Expected an Object, but got {action.GetType().Name} in key 'clicked', element {i}");
                                Dictionary<string, object> actionobject = ((JObject) action).ToObject<Dictionary<string, object>>();
                                if (!actionobject.ContainsKey("action")) throw new Exception($"Click definition in 'clicked', element {i} must contain an 'action' key.");
                                string actiontype = null;
                                List<object> actionparams = new List<object>();
                                string actioncondition = null;
                                foreach (string actionkey in actionobject.Keys)
                                {
                                    object actionvalue = actionobject[actionkey];
                                    switch (actionkey)
                                    {
                                        case "action":
                                            EnsureType(typeof(string), actionvalue, "action");
                                            actiontype = (string) actionvalue;
                                            if (actiontype != "enable" && actiontype != "disable" && actiontype != "check" && actiontype != "uncheck" &&
                                                actiontype != "set") throw new Exception($"Unknown action type '{actiontype}'.");
                                            break;
                                        case "parameter":
                                            if (!(actionvalue is string) && !(actionvalue is JArray)) throw new Exception($"Expected a string or Array, but got {actionvalue.GetType().Name} in key 'parameter'.");
                                            if (actionvalue is string) actionparams.Add((string) actionvalue);
                                            else
                                            {
                                                List<object> paramlist = ((JArray) actionvalue).ToObject<List<object>>();
                                                foreach (object paramvalue in paramlist)
                                                {
                                                    actionparams.Add(paramvalue);
                                                }
                                            }
                                            break;
                                        case "condition":
                                            EnsureType(typeof(string), actionvalue, "condition");
                                            actioncondition = (string) actionvalue;
                                            break;
                                        default:
                                            throw new Exception($"Unknown key '{actionkey}' inside 'clicked' definition");
                                    }
                                }
                                clickactions.Add(new ClickAction(actiontype, actionparams, actioncondition));
                            }
                            break;
                        default:
                            throw new Exception($"Unknown key '{key}' inside widget definition");
                    }
                }
                EnabledLookup.Add(name, enabled);
                w.SetPosition(X, Y);
                if (Width != -1 && Height != -1) w.SetSize(Width, Height);
                else if (Width != -1) w.SetWidth(Width);
                else if (Height != -1) w.SetHeight(Height);
                if (type == "label")
                {
                    if (!string.IsNullOrEmpty(text)) ((DynamicLabel) w).SetText(text);
                    if (font != null) ((DynamicLabel) w).SetFont(font);
                    ((DynamicLabel) w).SetParser(this);
                    ((DynamicLabel) w).SetColors(ConditionParser.Colors);
                    ((DynamicLabel) w).SetEnabled(enabled);
                }
                if (type == "multilabel")
                {
                    if (!string.IsNullOrEmpty(text)) ((MultilineDynamicLabel) w).SetText(text);
                    if (font != null) ((MultilineDynamicLabel) w).SetFont(font);
                    ((MultilineDynamicLabel) w).SetParser(this);
                    ((MultilineDynamicLabel) w).SetColors(CommandParser.Colors);
                    ((MultilineDynamicLabel) w).SetEnabled(enabled);
                }
                if (type == "textbox")
                {
                    if (!string.IsNullOrEmpty(text)) ((TextBox) w).SetInitialText(text);
                    if (font != null) ((TextBox) w).TextArea.SetFont(font);
                    ((TextBox) w).OnTextChanged += Save;
                }
                if (type == "numeric")
                {
                    ((NumericBox) w).SetValue(wvalue);
                    if (min_value != null) ((NumericBox) w).MinValue = (int) min_value;
                    if (max_value != null) ((NumericBox) w).MaxValue = (int) max_value;
                    ((NumericBox) w).OnValueChanged += Save;
                }
                if (type == "button")
                {
                    if (!string.IsNullOrEmpty(text)) ((Button) w).SetText(text);
                    if (font != null) ((Button) w).SetFont(font);
                }
                if (type == "dropdown")
                {
                    if (items == null && idx != -1 || items != null && idx >= items.Count) throw new Exception($"Index cannot be greater than or equal to the total item size.");
                    if (items != null) ((DropdownBox) w).SetItems(items);
                    if (idx != -1) ((DropdownBox) w).SetSelectedIndex(idx);
                    if (font != null) ((DropdownBox) w).SetFont(font);
                    ((DropdownBox) w).OnSelectionChanged += Save;
                    ((DropdownBox) w).OnSelectionChanged += delegate (BaseEventArgs e)
                    {
                        EvaluateAction(clickactions);
                    };
                }
                if (type == "checkbox")
                {
                    if (font != null) ((CheckBox) w).SetFont(font);
                    if (!string.IsNullOrEmpty(text)) ((CheckBox) w).SetText(text);
                }
                if (type == "radiobox")
                {
                    if (font != null) ((RadioBox) w).SetFont(font);
                    if (!string.IsNullOrEmpty(text)) ((RadioBox) w).SetText(text);
                    ((RadioBox) w).OnCheckChanged += delegate (BaseEventArgs e)
                    {
                        if (((RadioBox) w).Checked) EvaluateAction(clickactions);
                    };
                }
                if (type == "switch_picker")
                {
                    ((GameSwitchBox) w).OnSwitchChanged += Save;
                }
                if (type == "variable_picker")
                {
                    ((GameVariableBox) w).OnVariableChanged += Save;
                }
                if (type == "multitextbox")
                {
                    if (font != null) ((MultilineTextBox) w).SetFont(font);
                    if (!string.IsNullOrEmpty(text)) ((MultilineTextBox) w).SetText(text);
                }
                SetWidgetEnabled(w, enabled);
            }
        }

        public void EvaluateAction(List<ClickAction> Actions)
        {
            Actions.ForEach(action =>
            {
                if (action.EvaluteCondition(HandlerWidget.ActiveCondition.Parameters, this))
                {
                    if (action.Type == "enable" || action.Type == "disable")
                    {
                        action.Parameters.ForEach(e =>
                        {
                            Widget w = GetWidgetFromNameStrict((string) e);
                            SetWidgetEnabled(w, action.Type == "enable");
                        });
                    }
                    else if (action.Type == "check" || action.Type == "uncheck")
                    {
                        action.Parameters.ForEach(e =>
                        {
                            bool value = action.Type == "check";
                            Widget w = GetWidgetFromNameStrict((string) e);
                            if (w is RadioBox)
                                ((RadioBox) w).SetChecked(value);
                            else if (w is CheckBox) ((CheckBox) w).SetChecked(value);
                        });
                    }
                    else if (action.Type == "set")
                    {
                        for (int i = 0; i < action.Parameters.Count - 1; i += 2)
                        {
                            string param = (string) action.Parameters[i];
                            object value = action.Parameters[i + 1];
                            if (Condition.Parameters.ContainsKey(":" + param)) Condition.Parameters[":" + param] = value;
                            else
                            {
                                Widget w = GetWidgetFromNameStrict(param);
                                w.SetValue(GetIdentifierFromName(param), value.ToString());
                            }
                        }
                        Save(new BaseEventArgs());
                    }
                }
            });
        }

        public Widget GetWidgetFromName(string Name)
        {
            if (Name.Contains('.'))
            {
                Name = Name.Substring(0, Name.IndexOf('.'));
            }
            if (WidgetLookup.ContainsKey(Name)) return WidgetLookup[Name];
            return null;
        }

        public Widget GetWidgetFromNameStrict(string Name)
        {
            Widget w = GetWidgetFromName(Name);
            if (w == null) throw new Exception($"Could not find widget with identifier '{Name}'");
            return w;
        }

        public string GetIdentifierFromName(string Name)
        {
            if (Name.Contains('.')) return Name.Substring(Name.IndexOf('.') + 1, Name.Length - Name.IndexOf('.') - 1);
            return null;
        }

        public void Load(BasicCondition Condition)
        {
            this.Condition = Condition;
            this.LoadingData = true;
            if (this.Condition != null)
            {
                if (this.WasNull)
                {
                    foreach (string name in WidgetLookup.Keys)
                    {
                        SetWidgetEnabled(WidgetLookup[name], EnabledLookup[name]);
                    }
                }
                foreach (Widget w in WidgetLookup.Values)
                {
                    if (w is DynamicLabel)
                    {
                        ((DynamicLabel) w).SetParameters(Condition.Parameters);
                    }
                }
                this.WasNull = false;
                foreach (string key in LoadFormat.Keys)
                {
                    Widget w = GetWidgetFromName(key);
                    if (w == null) throw new Exception($"Could not find widget with identifier '{key}'");
                    object value = null;
                    if (LoadFormat[key] is Dictionary<string, object>)
                    {
                        foreach (string conditional in ((Dictionary<string, object>)LoadFormat[key]).Keys)
                        {
                            object conditionvalue = ((Dictionary<string, object>)LoadFormat[key])[conditional];
                            if (Utilities.EvaluateBooleanExpression(conditional, Condition.Parameters, this))
                            {
                                string cvaluestring = conditionvalue.ToString();
                                bool text = cvaluestring.StartsWith("$");
                                if (text)
                                {
                                    value = Utilities.ProcessText(cvaluestring.Substring(1), Condition.Parameters, this, true);
                                }
                                else
                                {
                                    value = Utilities.EvaluateExpression(conditionvalue.ToString(), Condition.Parameters, this);
                                    if (value is string && (string) value == conditionvalue.ToString() && !Utilities.IsNumeric((string) value))
                                        value = Utilities.EvaluateBooleanExpression((string) value, Condition.Parameters, this).ToString().ToLower();
                                }
                                break;
                            }
                        }
                    }
                    else if (LoadFormat[key] is string)
                    {
                        string fmt = (string) LoadFormat[key];
                        bool text = fmt.StartsWith("$");
                        if (text)
                        {
                            value = Utilities.ProcessText(fmt.Substring(1), Condition.Parameters, this, true);
                        }
                        else
                        {
                            value = Utilities.EvaluateExpression(fmt, Condition.Parameters, this);
                            if (value is string && (string) value == fmt && !Utilities.IsNumeric((string) value))
                                value = Utilities.EvaluateBooleanExpression((string) value, Condition.Parameters, this).ToString().ToLower();
                        }
                    }
                    string identifier = GetIdentifierFromName(key);
                    w.SetValue(identifier, value);
                }
            }
            else
            {
                foreach (Widget w in WidgetLookup.Values)
                {
                    SetWidgetEnabled(w, false);
                }
                this.WasNull = true;
            }
            this.LoadingData = false;
        }

        public void Save(BaseEventArgs e)
        {
            if (this.LoadingData) return;
            foreach (string key in SaveFormat.Keys)
            {
                object value = null;
                if (SaveFormat[key] is string)
                {
                    Widget w = GetWidgetFromName((string) SaveFormat[key]);
                    if (w == null)  value = Utilities.EvaluateExpression((string) SaveFormat[key], Condition.Parameters, this);
                    else value = w.GetValue(GetIdentifierFromName((string) SaveFormat[key]));
                }
                else if (SaveFormat[key] is Dictionary<string, object>)
                {
                    foreach (string savekey in ((Dictionary<string, object>) SaveFormat[key]).Keys)
                    {
                        if (Utilities.EvaluateBooleanExpression(savekey, Condition.Parameters, this))
                        {
                            value = Utilities.EvaluateExpression(((Dictionary<string, object>) SaveFormat[key])[savekey].ToString(), Condition.Parameters, this);
                            break;
                        }
                    }
                }
                Condition.Parameters[":" + key] = value;
            }
            this.HandlerWidget.UpdateParentList();
        }

        public void SetWidgetEnabled(Widget w, bool Enabled)
        {
            if (w is Label) ((Label) w).SetEnabled(Enabled);
            else if (w is DropdownBox) ((DropdownBox) w).SetEnabled(Enabled);
            else if (w is CheckBox) ((CheckBox) w).SetEnabled(Enabled);
            else if (w is RadioBox) ((RadioBox) w).SetEnabled(Enabled);
            else if (w is GameSwitchBox) ((GameSwitchBox) w).SetEnabled(Enabled);
            else if (w is GameVariableBox) ((GameVariableBox) w).SetEnabled(Enabled);
            else if (w is TextBox) ((TextBox) w).SetEnabled(Enabled);
            else if (w is NumericBox) ((NumericBox) w).SetEnabled(Enabled);
            else if (w is Button) ((Button) w).SetEnabled(Enabled);
        }

        public Dictionary<string, object> ReplaceDataObject(Dictionary<string, object> format)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            foreach (string key in format.Keys)
            {
                object value = format[key];
                if (value is string)
                {
                    data[key] = WidgetNameToValue((string) value);
                }
                else if (value is List<object>)
                {
                    data[key] = ReplaceDataArray((List<object>) value);
                }
                else if (value is Dictionary<string, object>)
                {
                    data[key] = ReplaceDataObject((Dictionary<string, object>) value);
                }
            }
            return data;
        }

        public List<object> ReplaceDataArray(List<object> format)
        {
            List<object> data = new List<object>();
            foreach (object value in format)
            {
                if (value is string)
                {
                    data.Add(WidgetNameToValue((string) value));
                }
                else if (value is List<object>)
                {
                    data.Add(ReplaceDataArray((List<object>) value));
                }
                else if (value is Dictionary<string, object>)
                {
                    data.Add(ReplaceDataObject((Dictionary<string, object>) value));
                }
            }
            return data;
        }

        public object WidgetNameToValue(string Name)
        {
            if (!WidgetLookup.ContainsKey(Name)) return Name;
            Widget w = WidgetLookup[Name];
            if (w is TextBox) return ((TextBox) w).Text;
            if (w is NumericBox) return ((NumericBox) w).Value;
            if (w is DropdownBox) return ((DropdownBox) w).SelectedIndex;
            throw new Exception($"Cannot replace '{Name}' with a valid value from widget type '{w.GetType().Name}'");
        }

        public void EnsureType(System.Type type, object obj, string key)
        {
            if (obj.GetType() != type) throw new Exception($"Expected a {type.Name}, but got a {obj.GetType().Name} in key '{key}'.");
        }
    }

    public class ClickAction
    {
        public string Type;
        public List<object> Parameters;
        public string Condition;

        public ClickAction(string Type, List<object> Parameters, string Condition = null)
        {
            this.Type = Type;
            this.Parameters = Parameters;
            this.Condition = Condition;
        }

        public bool EvaluteCondition(Dictionary<string, object> Parameters, IUIParser Parser)
        {
            return string.IsNullOrEmpty(this.Condition) ? true : Utilities.EvaluateBooleanExpression(this.Condition, Parameters, Parser);
        }
    }
}
