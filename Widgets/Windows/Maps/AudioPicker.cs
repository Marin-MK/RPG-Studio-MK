using System;
using System.IO;
using System.Collections.Generic;

namespace RPGStudioMK.Widgets;

public class AudioPicker : PopupWindow
{
    public (string Filename, int Volume, int Pitch)? Result = null;

    int Volume = 100;
    int Pitch = 100;

    ListBox FileList;
    Button PlayButton;
    Button StopButton;
    NumericSlider VolumeSlider;
    NumericSlider PitchSlider;

    Sound ActiveSound;

    public AudioPicker(string Folder, string File, int Volume, int Pitch)
    {
        SetTitle("Pick Audio");
        MinimumSize = MaximumSize = new Size(442, 418);
        SetSize(MaximumSize);
        Center();

        this.Volume = Volume;
        this.Pitch = Pitch;

        Label pickerlabel = new Label(this);
        pickerlabel.SetText("Files");
        pickerlabel.SetPosition(22, 33);
        pickerlabel.SetFont(Fonts.ProductSansMedium.Use(14));

        FileList = new ListBox(this);
        FileList.SetPosition(25, 51);
        FileList.SetSize(151, 355);
        List<ListItem> items = new List<ListItem>() { new ListItem("(None)", null) };
        int i = 1;
        int selidx = 0;
        foreach (string origfile in Directory.GetFiles(Game.Data.ProjectPath + "/" + Folder))
        {
            string file = origfile;
            while (file.Contains('\\')) file = file.Replace('\\', '/');
            string name = Path.GetFileNameWithoutExtension(file);
            if (name == File) selidx = i;
            items.Add(new ListItem(name, file));
            i++;
        }
        FileList.SetItems(items);
        FileList.SetSelectedIndex(selidx);
        FileList.OnSelectionChanged += delegate (BaseEventArgs e)
        {
            PlayButton.SetEnabled(FileList.SelectedIndex != 0);
        };

        Label previewlabel = new Label(this);
        previewlabel.SetText("Preview");
        previewlabel.SetPosition(192, 33);
        previewlabel.SetFont(Fonts.ProductSansMedium.Use(14));

        PlayButton = new Button(this);
        PlayButton.SetText("Play");
        PlayButton.SetPosition(192, 51);
        PlayButton.SetSize(99, 28);
        PlayButton.OnClicked += _ => PlaySound();
        PlayButton.SetEnabled(FileList.SelectedIndex != 0);

        StopButton = new Button(this);
        StopButton.SetText("Stop");
        StopButton.SetPosition(192, 77);
        StopButton.SetSize(99, 28);
        StopButton.OnClicked += _ => StopSound();

        Label VolumeGroupLabel = new Label(this);
        VolumeGroupLabel.SetText("Volume");
        VolumeGroupLabel.SetFont(Fonts.ProductSansMedium.Use(14));
        VolumeGroupLabel.SetPosition(192, 139);
        GroupBox VolumeGroup = new GroupBox(this);
        VolumeGroup.SetPosition(197, 158);
        VolumeGroup.SetSize(222, 54);
        Label MinVolumeLabel = new Label(VolumeGroup);
        MinVolumeLabel.SetText("-");
        MinVolumeLabel.SetFont(Fonts.UbuntuRegular.Use(26));
        MinVolumeLabel.SetPosition(27, 10);
        Label MaxVolumeLabel = new Label(VolumeGroup);
        MaxVolumeLabel.SetText("+");
        MaxVolumeLabel.SetFont(Fonts.UbuntuRegular.Use(26));
        MaxVolumeLabel.SetPosition(147, 10);
        Label VolumeLabel = new Label(VolumeGroup);
        VolumeLabel.SetText(Volume.ToString() + "%");
        VolumeLabel.SetFont(Fonts.UbuntuRegular.Use(14));
        VolumeLabel.SetPosition(167, 18);
        VolumeSlider = new NumericSlider(VolumeGroup);
        VolumeSlider.SetPosition(38, 18);
        VolumeSlider.SetValue(Volume);
        VolumeSlider.SetSnapValues(0, 25, 50, 75, 100);
        VolumeSlider.OnValueChanged += delegate (BaseEventArgs e)
        {
            this.Volume = VolumeSlider.Value;
            VolumeLabel.SetText(this.Volume.ToString() + "%");
            if (ActiveSound != null && ActiveSound.Alive) ActiveSound.Volume = this.Volume;
        };

        Label PitchGroupLabel = new Label(this);
        PitchGroupLabel.SetText("Pitch");
        PitchGroupLabel.SetFont(Fonts.ProductSansMedium.Use(14));
        PitchGroupLabel.SetPosition(192, 239);
        GroupBox PitchGroup = new GroupBox(this);
        PitchGroup.SetPosition(197, 258);
        PitchGroup.SetSize(222, 54);
        Label MinPitchLabel = new Label(PitchGroup);
        MinPitchLabel.SetText("-");
        MinPitchLabel.SetFont(Fonts.UbuntuRegular.Use(26));
        MinPitchLabel.SetPosition(27, 10);
        Label MaxPitchLabel = new Label(PitchGroup);
        MaxPitchLabel.SetText("+");
        MaxPitchLabel.SetFont(Fonts.UbuntuRegular.Use(26));
        MaxPitchLabel.SetPosition(147, 10);
        Label PitchLabel = new Label(PitchGroup);
        PitchLabel.SetText(Pitch.ToString() + "%");
        PitchLabel.SetFont(Fonts.UbuntuRegular.Use(14));
        PitchLabel.SetPosition(167, 18);
        PitchSlider = new NumericSlider(PitchGroup);
        PitchSlider.SetPosition(38, 18);
        PitchSlider.SetValue(Pitch);
        PitchSlider.SetMinimumValue(50);
        PitchSlider.SetMaximumValue(150);
        PitchSlider.SetSnapValues(50, 75, 100, 125, 150);
        PitchSlider.OnValueChanged += delegate (BaseEventArgs e)
        {
            this.Pitch = PitchSlider.Value;
            PitchLabel.SetText(this.Pitch.ToString() + "%");
            if (ActiveSound != null && ActiveSound.Alive) ActiveSound.SampleRate = (int)Math.Round(this.Pitch / 100d * ActiveSound.OriginalSampleRate);
        };

        RegisterShortcuts(new List<Shortcut>()
        {
            new Shortcut(this, new Key(Keycode.ENTER, Keycode.CTRL), _ => OK(), true)
        });

        CreateButton("Cancel", _ => Cancel());
        CreateButton("OK", _ => OK());
    }

    void PlaySound()
    {
        if (FileList.SelectedItem.Object == null) return;
        StopSound();
        ActiveSound = new Sound((string)FileList.SelectedItem.Object, this.Volume);
        ActiveSound.SampleRate = (int)Math.Round(this.Pitch / 100d * ActiveSound.OriginalSampleRate);
        Audio.BGMPlay(ActiveSound);
    }

    void StopSound()
    {
        if (ActiveSound != null && ActiveSound.Alive) ActiveSound.Stop();
    }

    public void OK()
    {
        StopSound();
        this.Result = (FileList.SelectedItem.Object == null ? "" : FileList.SelectedItem.Name, this.Volume, this.Pitch);
        Close();
    }

    public void Cancel()
    {
        StopSound();
        this.Result = null;
        Close();
    }
}
