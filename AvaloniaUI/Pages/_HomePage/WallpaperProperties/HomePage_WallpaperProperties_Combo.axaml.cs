using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Logic.Data;
using Logic.Database;

namespace AvaloniaUI.Pages._HomePage.WallpaperProperties;

public partial class HomePage_WallpaperProperties_Combo : UserControl, IWallpaperProperty
{
    private string? key;
    private string[]? indexLookup;
    private int? defaultValue;

    public int SelectedIndex => inp.SelectedIndex;
    public string SelectedValue => indexLookup![SelectedIndex];

    private bool isDirty = false;

    public HomePage_WallpaperProperties_Combo()
    {
        InitializeComponent();

        this.Height = HomePage.PROPERTY_DEFAULT_HEIGHT;
        this.lbl.FontSize = HomePage.PROPERTY_DEFAULT_FONT_SIZE;
        this.inp.FontSize = HomePage.PROPERTY_DEFAULT_FONT_SIZE;
    }

    public IWallpaperProperty Init(WorkshopEntry.Properties prop)
    {
        indexLookup = prop.comboOptions!.Select(x => x.value).ToArray();
        return Init(prop.propertyName!, prop.text!, prop.comboOptions!.Select(x => x.label).ToArray(), int.Parse(prop.value!));
    }

    public IWallpaperProperty Init(string name, string label, string[] data, int defaultVal)
    {
        key = name;
        lbl.Content = label;

        inp.ItemsSource = data;
        inp.SelectedIndex = defaultVal;

        defaultValue = defaultVal;

        isDirty = false;
        inp.SelectionChanged += (_, __) => isDirty = true;

        return this;
    }


    public void Load(ref Dictionary<string, string?> options)
    {
        if (!options.TryGetValue(key!, out string? res) || string.IsNullOrEmpty(res))
        {
            inp.SelectedIndex = defaultValue ?? 0;
            isDirty = false;

            return;
        }

        isDirty = true;
        inp.SelectedIndex = int.Parse(res);
    }

    public dbo_WallpaperSettings? Save(long id)
    {
        if (!isDirty)
            return null;

        return new dbo_WallpaperSettings()
        {
            wallpaperId = id,
            settingKey = key!,
            settingValue = inp.SelectedIndex.ToString()
        };
    }

    public string? CreateArgument()
    {
        if (!isDirty)
            return null;

        return $"{key!}={SelectedValue}";
    }
}