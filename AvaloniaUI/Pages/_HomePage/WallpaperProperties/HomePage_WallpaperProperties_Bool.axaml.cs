using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Logic.Data;
using Logic.Database;

namespace AvaloniaUI.Pages._HomePage.WallpaperProperties;

public partial class HomePage_WallpaperProperties_Bool : UserControl, IWallpaperProperty
{
    private string? key;
    private bool? defaultVal;

    private bool isDirty = false;

    public HomePage_WallpaperProperties_Bool()
    {
        InitializeComponent();

        this.Height = HomePage.PROPERTY_DEFAULT_HEIGHT;
        this.lbl.FontSize = HomePage.PROPERTY_DEFAULT_FONT_SIZE;
        this.inp.FontSize = HomePage.PROPERTY_DEFAULT_FONT_SIZE;
    }



    public IWallpaperProperty Init(WorkshopEntry.Properties prop)
    {
        return Init(prop.propertyName!, prop.text!, prop.value == "1");
    }

    public IWallpaperProperty Init(string propertyName, string label, bool value)
    {
        key = propertyName;

        lbl.Content = label;
        inp.IsChecked = value;

        defaultVal = inp.IsChecked;

        isDirty = false;
        inp.IsCheckedChanged += (_, __) => isDirty = true;

        return this;
    }

    public void Load(ref Dictionary<string, string?> options)
    {
        if (!options.TryGetValue(key!, out string? res) || string.IsNullOrEmpty(res))
        {
            inp.IsChecked = defaultVal;
            isDirty = false;

            return;
        }

        isDirty = true;
        inp.IsChecked = res == "1";
    }

    public dbo_WallpaperSettings? Save(long id)
    {
        if (!isDirty)
            return null;

        return new dbo_WallpaperSettings()
        {
            wallpaperId = id,
            settingKey = key!,
            settingValue = (inp.IsChecked ?? false) ? "1" : "0"
        };
    }
}