using System;
using System.Collections.Generic;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class MapPropertiesWindow : PopupWindow
{
    public Map Map;
    public Map OldMap;

    public bool UnsavedChanges = false;
    public bool UpdateMapViewer = false;

    bool MakeUndoEvent;

    TextBox MapName;
    NumericBox Width;
    NumericBox Height;
    BrowserBox BGM;
    BrowserBox BGS;
    BrowserBox Tilesets;

    public MapPropertiesWindow(Map OldMap, bool MakeUndoEvent = true)
    {
        this.OldMap = OldMap;
        this.Map = (Map) OldMap.Clone();
        this.MakeUndoEvent = MakeUndoEvent;
        this.SetTitle($"Map Properties");
        MinimumSize = MaximumSize = new Size(578, 344);
        SetSize(MaximumSize);
        this.Center();

        Label namelabel = new Label(this);
        namelabel.SetText("Map Name:");
        namelabel.SetFont(Fonts.Paragraph);
        namelabel.SetPosition(27, 41);
        MapName = new TextBox(this);
        MapName.SetPosition(27, 65);
        MapName.SetSize(225, 27);
        MapName.SetText(this.Map.Name);
        MapName.OnTextChanged += _ =>
        {
            this.Map.Name = MapName.Text;
        };

        Label widthlabel = new Label(this);
        widthlabel.SetText("Width:");
        widthlabel.SetFont(Fonts.Paragraph);
        widthlabel.SetPosition(27, 112);
        Width = new NumericBox(this);
        Width.SetPosition(27, 134);
        Width.SetMinValue(1);
        Width.SetMaxValue(255);
        Width.SetSize(97, 30);
        Width.SetValue(this.Map.Width);
        Width.OnValueChanged += delegate (BaseEventArgs e)
        {
            this.Map.Width = Width.Value;
        };

        Label heightlabel = new Label(this);
        heightlabel.SetText("Height:");
        heightlabel.SetFont(Fonts.Paragraph);
        heightlabel.SetPosition(155, 112);
        Height = new NumericBox(this);
        Height.SetPosition(155, 132);
        Height.SetMinValue(1);
        Height.SetMaxValue(255);
        Height.SetSize(90, 27);
        Height.SetValue(this.Map.Height);
        Height.OnValueChanged += delegate (BaseEventArgs e)
        {
            this.Map.Height = Height.Value;
        };

        CheckBox autoplaybgm = new CheckBox(this);
        autoplaybgm.SetPosition(27, 187);
        autoplaybgm.SetText("Autoplay BGM");
        autoplaybgm.SetFont(Fonts.Paragraph);
        autoplaybgm.SetChecked(this.Map.AutoplayBGM);
        autoplaybgm.OnCheckChanged += delegate (BaseEventArgs e)
        {
            this.Map.AutoplayBGM = autoplaybgm.Checked;
            BGM.SetEnabled(autoplaybgm.Checked);
            BGM.SetText(autoplaybgm.Checked ? Map.BGM.Name : "");
        };
        BGM = new BrowserBox(this);
        BGM.SetPosition(27, 209);
        BGM.SetSize(225, 24);
        BGM.SetText(this.Map.AutoplayBGM ? this.Map.BGM.Name : "");
        BGM.SetEnabled(this.Map.AutoplayBGM);
        BGM.OnDropDownClicked += _ =>
        {
            AudioPicker picker = new AudioPicker("Audio/BGM", this.Map.BGM.Name, this.Map.BGM.Volume, this.Map.BGM.Pitch);
            picker.OnClosed += _ =>
            {
                if (picker.Result != null)
                {
                    this.Map.BGM.Name = picker.Result.Value.Filename;
                    this.Map.BGM.Volume = picker.Result.Value.Volume;
                    this.Map.BGM.Pitch = picker.Result.Value.Pitch;
                    BGM.SetText(this.Map.BGM.Name);
                }
            };
        };

        CheckBox autoplaybgs = new CheckBox(this);
        autoplaybgs.SetPosition(27, 240);
        autoplaybgs.SetText("Autoplay BGS");
        autoplaybgs.SetFont(Fonts.Paragraph);
        autoplaybgs.SetChecked(this.Map.AutoplayBGS);
        autoplaybgs.OnCheckChanged += delegate (BaseEventArgs e)
        {
            this.Map.AutoplayBGS = autoplaybgs.Checked;
            BGS.SetEnabled(autoplaybgs.Checked);
            BGS.SetText(autoplaybgs.Checked ? this.Map.BGS.Name : "");
        };
        BGS = new BrowserBox(this);
        BGS.SetPosition(27, 262);
        BGS.SetSize(225, 24);
        BGS.SetText(this.Map.AutoplayBGS ? this.Map.BGS.Name : "");
        BGS.SetEnabled(this.Map.AutoplayBGS);
        BGS.OnDropDownClicked += _ =>
        {
            AudioPicker picker = new AudioPicker("Audio/BGS", this.Map.BGS.Name, this.Map.BGS.Volume, this.Map.BGS.Pitch);
            picker.OnClosed += _ =>
            {
                if (picker.Result != null)
                {
                    this.Map.BGS.Name = picker.Result.Value.Filename;
                    this.Map.BGS.Volume = picker.Result.Value.Volume;
                    this.Map.BGS.Pitch = picker.Result.Value.Pitch;
                    BGS.SetText(this.Map.BGS.Name);
                }
            };
        };

        Label tilesetslabel = new Label(this);
        tilesetslabel.SetText("Tileset:");
        tilesetslabel.SetFont(Fonts.Paragraph);
        tilesetslabel.SetPosition(324, 41);

        Tilesets = new BrowserBox(this);
        Tilesets.SetPosition(324, 65);
        Tilesets.SetSize(225, 25);
        Tilesets.SetText(Data.Tilesets[this.Map.TilesetIDs[0]]?.Name ?? "");
        Tilesets.OnDropDownClicked += _ =>
        {
            TilesetPickerWindow win = new TilesetPickerWindow(Data.Tilesets[this.Map.TilesetIDs[0]]);
            win.OnClosed += _ =>
            {
                if (!win.Apply) return;
                this.Map.TilesetIDs = new List<int>() { win.Tileset.ID };
                Tilesets.SetText(win.Tileset.Name);
            };
        };

        CreateButton("Cancel", _ => Cancel());
        CreateButton("OK", _ => OK());

        RegisterShortcuts(new List<Shortcut>()
        {
            new Shortcut(this, new Key(Keycode.ENTER, Keycode.CTRL), _ => OK(), true)
        });
    }

    public void OK()
    {
        this.UpdateMapViewer = true;
        Action Finalize = delegate
        {
            Close();
        };
        Action Continue = delegate
        {
            #region Autotile conversion for multiple tilesets per map
            // Updates autotiles
            /*bool autotileschanged = false;
            if (Map.AutotileIDs.Count != OldMap.AutotileIDs.Count) autotileschanged = true;
            if (!autotileschanged)
            {
                for (int i = 0; i < Map.AutotileIDs.Count; i++)
                {
                    if (Map.AutotileIDs[i] != OldMap.AutotileIDs[i])
                    {
                        autotileschanged = true;
                        break;
                    }
                }
            }
            if (autotileschanged)
            {
                UnsavedChanges = true;
                bool warn = false;
                for (int layer = 0; layer < Map.Layers.Count; layer++)
                {
                    for (int i = 0; i < Map.Width * Map.Height; i++)
                    {
                        if (Map.Layers[layer].Tiles[i] == null || Map.Layers[layer].Tiles[i].TileType == TileType.Tileset) continue;
                        int autotileID = OldMap.AutotileIDs[Map.Layers[layer].Tiles[i].Index];
                        if (!Map.AutotileIDs.Contains(autotileID))
                        {
                            warn = true;
                            break;
                        }
                    }
                }
                if (warn)
                {
                    MessageBox msg = new MessageBox("Warning", "One of the deleted autotiles was still in use. By choosing to continue, tiles of that autotile will be deleted.", new List<string>() { "Continue", "Cancel" }, IconType.Warning);
                    msg.OnButtonPressed += delegate (BaseEventArgs e2)
                    {
                        if (msg.Result == 0) // Continue
                        {
                            for (int layer = 0; layer < Map.Layers.Count; layer++)
                            {
                                for (int i = 0; i < Map.Width * Map.Height; i++)
                                {
                                    if (Map.Layers[layer].Tiles[i] == null || Map.Layers[layer].Tiles[i].TileType == TileType.Tileset) continue;
                                    int autotileID = OldMap.AutotileIDs[Map.Layers[layer].Tiles[i].Index];
                                    if (!Map.AutotileIDs.Contains(autotileID))
                                    {
                                        Map.Layers[layer].Tiles[i] = null;
                                    }
                                    else Map.Layers[layer].Tiles[i].Index = Map.AutotileIDs.IndexOf(autotileID);
                                }
                            }
                            Finalize();
                        }
                        else if (msg.Result == 1) // Cancel
                        {
                            UnsavedChanges = false;
                            UpdateMapViewer = false;
                        }
                    };
                }
                else
                {
                    for (int layer = 0; layer < Map.Layers.Count; layer++)
                    {
                        for (int i = 0; i < Map.Width * Map.Height; i++)
                        {
                            if (Map.Layers[layer].Tiles[i] == null || Map.Layers[layer].Tiles[i].TileType == TileType.Tileset) continue;
                            int autotileID = OldMap.AutotileIDs[Map.Layers[layer].Tiles[i].Index];
                            if (!Map.AutotileIDs.Contains(autotileID))
                            {
                                throw new Exception("Unknown");
                            }
                            else Map.Layers[layer].Tiles[i].Index = Map.AutotileIDs.IndexOf(autotileID);
                        }
                    }
                    Finalize();
                }
            }
            if (!autotileschanged) Finalize();*/
            #endregion
            Finalize();
        };
        // Resizes Map
        if (Map.Width != OldMap.Width || Map.Height != OldMap.Height)
        {
            List<Layer> OldLayers = OldMap.Layers.ConvertAll(l => (Layer) l.Clone());
            Size OldSize = new Size(OldMap.Width, OldMap.Height);
            Map.Resize(OldMap.Width, Map.Width, OldMap.Height, Map.Height);
            UnsavedChanges = true;
        }
        // Marks name change
        if (Map.Name != OldMap.Name)
        {
            UnsavedChanges = true;
        }
        // Marks BGM changes
        if (!Map.BGM.Equals(OldMap.BGM) || Map.AutoplayBGM != OldMap.AutoplayBGM)
        {
            UnsavedChanges = true;
        }
        // Marks BGS changes
        if (!Map.BGS.Equals(OldMap.BGS) || Map.AutoplayBGS != OldMap.AutoplayBGS)
        {
            UnsavedChanges = true;
        }
        // Updates tilesets
        bool tilesetschanged = false;
        if (Map.TilesetIDs.Count != OldMap.TilesetIDs.Count) tilesetschanged = true;
        if (!tilesetschanged)
        {
            for (int i = 0; i < Map.TilesetIDs.Count; i++)
            {
                if (Map.TilesetIDs[i] != OldMap.TilesetIDs[i])
                {
                    tilesetschanged = true;
                    break;
                }
            }
        }
        if (tilesetschanged)
        {
            UnsavedChanges = true;
            #region Tileset conversion for multiple tilesets per map
            /*bool warn = false;
            for (int layer = 0; layer < Map.Layers.Count; layer++)
            {
                for (int i = 0; i < Map.Width * Map.Height; i++)
                {
                    if (Map.Layers[layer].Tiles[i] == null || Map.Layers[layer].Tiles[i].TileType == TileType.Autotile) continue;
                    int tilesetID = OldMap.TilesetIDs[Map.Layers[layer].Tiles[i].Index];
                    if (!Map.TilesetIDs.Contains(tilesetID))
                    {
                        warn = true;
                        break;
                    }
                }
            }
            if (warn)
            {
                MessageBox msg = new MessageBox("Warning", "One of the deleted tilesets was still in use. By choosing to continue, tiles of that tileset will be deleted.", new List<string>() { "Continue", "Cancel" }, IconType.Warning);
                msg.OnButtonPressed += delegate (BaseEventArgs e2)
                {
                    if (msg.Result == 0) // Continue
                    {
                        for (int layer = 0; layer < Map.Layers.Count; layer++)
                        {
                            for (int i = 0; i < Map.Width * Map.Height; i++)
                            {
                                if (Map.Layers[layer].Tiles[i] == null || Map.Layers[layer].Tiles[i].TileType == TileType.Autotile) continue;
                                int tilesetID = OldMap.TilesetIDs[Map.Layers[layer].Tiles[i].Index];
                                if (!Map.TilesetIDs.Contains(tilesetID))
                                {
                                    Map.Layers[layer].Tiles[i] = null;
                                }
                                else Map.Layers[layer].Tiles[i].Index = Map.TilesetIDs.IndexOf(tilesetID);
                            }
                        }
                        Continue();
                    }
                    else if (msg.Result == 1) // Cancel
                    {
                        UnsavedChanges = false;
                        UpdateMapViewer = false;
                    }
                };
            }
            else
            {
                for (int layer = 0; layer < Map.Layers.Count; layer++)
                {
                    for (int i = 0; i < Map.Width * Map.Height; i++)
                    {
                        if (Map.Layers[layer].Tiles[i] == null || Map.Layers[layer].Tiles[i].TileType == TileType.Autotile) continue;
                        int tilesetID = OldMap.TilesetIDs[Map.Layers[layer].Tiles[i].Index];
                        if (!Map.TilesetIDs.Contains(tilesetID))
                        {
                            throw new Exception("Impossible-to-reach code has been reached.");
                        }
                        else Map.Layers[layer].Tiles[i].Index = Map.TilesetIDs.IndexOf(tilesetID);
                    }
                }
                Continue();
            }*/
            #endregion
            Continue();
        }
        if (!tilesetschanged) Continue();
    }

    public void Cancel()
    {
        Close();
    }
}
