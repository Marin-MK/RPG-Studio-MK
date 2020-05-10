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
    public class ConditionUIParser
    {
        public PopupWindow Window;
        public string Title;
        public int Width = -1;
        public int Height = -1;
        public Dictionary<string, Font> Fonts = new Dictionary<string, Font>();
        public Dictionary<string, Widget> MainWidgets = new Dictionary<string, Widget>();
        public Dictionary<string, Widget> WidgetLookup = new Dictionary<string, Widget>();
        public List<Dictionary<string, string>> Buttons = new List<Dictionary<string, string>>();
        public Dictionary<string, object> LoadFormat = new Dictionary<string, object>();
        public Dictionary<string, object> SaveFormat = new Dictionary<string, object>();
        public BasicCondition Condition;
        public BasicCondition OldCondition;
        public bool NeedUpdate = false;

        public ConditionUIParser(Dictionary<string, object> UIData, BasicCondition Condition)
        {
            this.Window = new PopupWindow();
            this.OldCondition = Condition.Clone();
            this.Condition = Condition;
            foreach (string Key in UIData.Keys)
            {
                object value = UIData[Key];
                switch (Key)
                {
                    case "title":
                        EnsureType(typeof(string), value, "title");
                        this.Title = (string) value;
                        break;
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
                    case "buttons":
                        if (!(value is JArray)) throw new Exception($"Expected an Array, but got a {value.GetType().Name} in key 'buttons'");
                        List<object> buttons = ((JArray) value).ToObject<List<object>>();
                        foreach (object buttonobject in buttons)
                        {
                            if (!(buttonobject is JObject)) throw new Exception($"Expected an Object, but got a {buttonobject.GetType().Name} in key 'buttons'");
                            Dictionary<string, object> button = ((JObject) buttonobject).ToObject<Dictionary<string, object>>();
                            string type = null;
                            string name = null;
                            if (!button.ContainsKey("type")) throw new Exception($"Button definition must contain a 'type' key");
                            if (!button.ContainsKey("name")) throw new Exception($"Button definition must contain a 'name' key");
                            foreach (string buttonkey in button.Keys)
                            {
                                object buttonvalue = button[buttonkey];
                                switch (buttonkey)
                                {
                                    case "type":
                                        EnsureType(typeof(string), buttonvalue, "type");
                                        type = (string) buttonvalue;
                                        if (type != "ok" && type != "cancel" && type != "apply") throw new Exception($"Unknown button type '{type}' in button definition");
                                        break;
                                    case "name":
                                        EnsureType(typeof(string), buttonvalue, "name");
                                        name = (string) buttonvalue;
                                        break;
                                    default:
                                        throw new Exception($"Unknown key '{buttonkey}' in button definition in key 'buttons'");
                                }
                            }
                            Buttons.Add(new Dictionary<string, string>() { { "type", type }, { "name", name } });
                        }
                        break;
                    case "widgets":
                        if (!(value is JObject)) throw new Exception($"Expected an Object, but got a {value.GetType().Name} in key 'widgets'");
                        this.MainWidgets = ParseWidgets(Window, ((JObject) value).ToObject<Dictionary<string, object>>());
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
            if (!string.IsNullOrEmpty(this.Title)) Window.SetTitle(this.Title);
            if (this.Width != -1) Window.SetWidth(this.Width);
            if (this.Height != -1) Window.SetHeight(this.Height);
            if (Buttons.Count == 0) Buttons = new List<Dictionary<string, string>>() { new Dictionary<string, string>() { { "type", "cancel" }, { "name", "Cancel" } }, new Dictionary<string, string>() { { "type", "ok" }, { "name", "OK" } } };
            foreach (Dictionary<string, string> button in Buttons)
            {
                BaseEvent method = button["type"] == "ok" ? OK : (button["type"] == "apply" ? Apply : (BaseEvent) Cancel);
                Window.CreateButton(button["name"], method);
            }
            Window.Center();
            foreach (string key in LoadFormat.Keys)
            {
                string widget = key;
                string identifier = null;
                if (key.Contains('.'))
                {
                    identifier = widget.Substring(widget.IndexOf('.') + 1, widget.Length - widget.IndexOf('.') - 1);
                    widget = widget.Substring(0, widget.IndexOf('.'));
                }
                if (!WidgetLookup.ContainsKey(widget)) throw new Exception($"Could not find widget with identifier '{widget}'");
                Widget w = WidgetLookup[widget];
                object value = Condition.Parameters[":" + LoadFormat[key]];
                w.SetValue(identifier, value);
            }
        }

        public void OK(BaseEventArgs e)
        {
            Apply(e);
            this.Window.Close();
        }

        public void Apply(BaseEventArgs e)
        {
            foreach (string key in SaveFormat.Keys)
            {
                string variable = key;
                string widget = (string) SaveFormat[key];
                string identifier = null;
                if (widget.Contains('.'))
                {
                    identifier = widget.Substring(widget.IndexOf('.') + 1, widget.Length - widget.IndexOf('.') - 1);
                    widget = widget.Substring(0, widget.IndexOf('.'));
                }
                if (!WidgetLookup.ContainsKey(widget)) throw new Exception($"Could not find widget with identifier '{widget}'");
                Widget w = WidgetLookup[widget];
                Condition.Parameters[":" + variable] = w.GetValue(identifier);
            }
            NeedUpdate = true;
        }

        public void Cancel(BaseEventArgs e)
        {
            this.Condition = this.OldCondition;
            this.Window.Close();
        }

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

        public Dictionary<string, Widget> ParseWidgets(IContainer ParentWidget, Dictionary<string, object> Data)
        {
            Dictionary<string, Widget> Widgets = new Dictionary<string, Widget>();
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
                    case "label":
                        w = new Label(ParentWidget);
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
                    case "switch_picker":
                        w = new GameSwitchPicker(ParentWidget);
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
                            if (type != "label" && type != "textbox" && type != "button" && type != "dropdown") throw new Exception($"Widget type ({type}) can not contain a 'font' field.");
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
                            if (type != "label" && type != "textbox" && type != "button") throw new Exception($"Widget type ({type}) can not contain a 'text' field.");
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
                        case "clicked":
                            if (type != "button") throw new Exception($"Widget type ({type}) can not contain a 'clicked' field.");
                            if (!(value is JObject)) throw new Exception($"Expected an Object, but got {value.GetType().Name} in key 'clicked'");
                            Dictionary<string, object> clickdata = ((JObject) value).ToObject<Dictionary<string, object>>();
                            if (!clickdata.ContainsKey("type")) throw new Exception("'clicked' definition must contain a 'type' key.");
                            if (!(clickdata["type"] is string)) throw new Exception($"Expected a String, but got {clickdata["type"].GetType().Name} in key 'type'");
                            string clicktype = (string) clickdata["type"];
                            bool save = false;
                            bool close = false;
                            foreach (string clickkey in clickdata.Keys)
                            {
                                if (clickkey == "type") continue;
                                object clickvalue = clickdata[clickkey];
                                switch (clickkey)
                                {
                                    case "action":
                                        EnsureType(typeof(string), clickvalue, "action");
                                        string action = (string) clickvalue;
                                        switch (action)
                                        {
                                            case "save":
                                                save = true;
                                                break;
                                            case "close":
                                                close = true;
                                                break;
                                            case "save_and_close":
                                                save = true;
                                                close = true;
                                                break;
                                            default:
                                                throw new Exception($"Unknown value '{action}' inside 'action' definition.");
                                        }
                                        break;
                                    default:
                                        throw new Exception($"Unknown key '{key}' inside 'clicked' definition");
                                }
                            }
                            ((Button) w).OnClicked += delegate (BaseEventArgs e)
                            {
                                if (save) Save();
                                if (close) Window.Close();
                            };
                            break;
                        default:
                            throw new Exception($"Unknown key '{key}' inside widget definition");
                    }
                }
                w.SetPosition(X, Y);
                if (Width != -1 && Height != -1) w.SetSize(Width, Height);
                else if (Width != -1) w.SetWidth(Width);
                else if (Height != -1) w.SetHeight(Height);
                if (type == "label")
                {
                    if (!string.IsNullOrEmpty(text)) ((Label) w).SetText(text);
                    if (font != null) ((Label) w).SetFont(font);
                }
                if (type == "textbox")
                {
                    if (!string.IsNullOrEmpty(text)) ((TextBox) w).SetInitialText(text);
                    if (font != null) ((TextBox) w).TextArea.SetFont(font);
                }
                if (type == "numeric")
                {
                    ((NumericBox) w).SetValue(wvalue);
                    if (min_value != null) ((NumericBox) w).MinValue = (int) min_value;
                    if (max_value != null) ((NumericBox) w).MaxValue = (int) max_value;
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
                }
                Widgets.Add(name, w);
            }
            return Widgets;
        }

        public void Save()
        {
            Dictionary<string, object> data = ReplaceDataObject((Dictionary<string, object>) SaveFormat["format"]);
            StreamWriter sw = new StreamWriter(File.OpenWrite((string) SaveFormat["filename"]));
            sw.Write(JsonConvert.SerializeObject(data));
            sw.Close();
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
}
