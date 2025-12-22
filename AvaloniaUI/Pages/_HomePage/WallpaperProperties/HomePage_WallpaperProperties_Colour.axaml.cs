using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Logic.Data;

namespace AvaloniaUI.Pages._HomePage.WallpaperProperties;

public partial class HomePage_WallpaperProperties_Colour : UserControl, IWallpaperProperty
{
    public HomePage_WallpaperProperties_Colour()
    {
        InitializeComponent();
    }

    public void Draw(WorkshopEntry.Properties prop)
    {
        lbl_Name.Content = prop.propertyName;
    }
}