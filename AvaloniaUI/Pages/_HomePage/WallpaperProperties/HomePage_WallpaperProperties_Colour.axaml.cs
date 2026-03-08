using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.Immutable;
using Avalonia.Platform;
using Logic.Data;
using Logic.Database;

namespace AvaloniaUI.Pages._HomePage.WallpaperProperties;

public partial class HomePage_WallpaperProperties_Colour : UserControl, IWallpaperProperty
{
    private string? key;
    private bool isDirty = false;

    private Color? defaultColour;
    private Color? currentColour;

    public string? StringColour => currentColour != null ? GetColourAsString(currentColour.Value) : string.Empty;

    public HomePage_WallpaperProperties_Colour()
    {
        InitializeComponent();

        this.Height = HomePage.PROPERTY_DEFAULT_HEIGHT;
        this.lbl.FontSize = HomePage.PROPERTY_DEFAULT_FONT_SIZE;
        this.inp.FontSize = HomePage.PROPERTY_DEFAULT_FONT_SIZE;



        int SIZE = 150;

        var bitmap = new WriteableBitmap(
            new PixelSize(SIZE, SIZE),
            new Vector(96, 96),
            PixelFormat.Rgba8888,
            AlphaFormat.Opaque);

        byte[] pixels = new byte[SIZE * SIZE * 4]; // RGBA

        for (int py = 0; py < SIZE; py++)
        {
            for (int px = 0; px < SIZE; px++)
            {
                double h = (px / (double)SIZE) * 360;
                double v = 1 - py / (double)SIZE;

                var (r, g, b) = HsvToRgb(h, 1, v);

                int i = (py * SIZE + px) * 4;

                pixels[i] = (byte)r;
                pixels[i + 1] = (byte)g;
                pixels[i + 2] = (byte)b;
                pixels[i + 3] = 255;
            }
        }

        // Copy safely row by row to handle potential row padding
        using (var fb = bitmap.Lock())
        {
            for (int py = 0; py < SIZE; py++)
            {
                IntPtr rowPtr = fb.Address + py * fb.RowBytes;
                Marshal.Copy(pixels, py * SIZE * 4, rowPtr, SIZE * 4);
            }
        }

        inp_Picker.Source = bitmap;
    }

    static (int r, int g, int b) HsvToRgb(double h, double s, double v)
    {
        Func<int, double> f = (n) =>
        {
            double k = (n + h / 60) % 6;
            return v - v * s * Math.Max(0, Math.Min(Math.Min(k, 4d - k), 1d));
        };
        return ((int)Math.Round(f(5) * 255d), (int)Math.Round(f(3) * 255d), (int)Math.Round(f(1) * 255d));
    }

    public IWallpaperProperty Init(WorkshopEntry.Properties prop)
    {
        Color? parsedColour = null;

        if (prop.value!.StartsWith("#"))
        {
            if (Color.TryParse(prop.value, out Color _c))
                parsedColour = _c;
        }
        else
        {
            parsedColour = GetColourFromString(prop.value!, " "[0]);
        }

        return Init(prop.propertyName!, prop.text!, parsedColour ?? Color.FromRgb(0, 0, 0));
    }

    public IWallpaperProperty Init(string settingName, string title, Color defaultColour)
    {
        key = settingName;
        lbl.Content = title;

        this.defaultColour = defaultColour;

        inp.KeyUp += (_, __) => UpdateColourFromInput();
        inp.Text = ColorToHex(defaultColour);

        RedrawColourDescendants();
        isDirty = false;

        return this;
    }

    public void Load(ref Dictionary<string, string?> options)
    {
        if (!options.TryGetValue(key!, out string? res) || string.IsNullOrEmpty(res))
        {
            currentColour = defaultColour;
            RedrawColourDescendants();

            isDirty = false;
            return;
        }

        currentColour = GetColourFromString(res, ',') ?? defaultColour;

        isDirty = true;
        RedrawColourDescendants();
    }

    public dbo_WallpaperSettings? Save(long id)
    {
        if (currentColour == null || !isDirty)
            return null;

        return new dbo_WallpaperSettings()
        {
            wallpaperId = id,
            settingKey = key!,
            settingValue = GetColourAsString(currentColour.Value)
        };
    }

    // avalonia puts alpha first?
    private static string ColorToHex(Color colour)
    {
        return $"{colour.R:X2}{colour.G:X2}{colour.B:X2}";
    }

    private static string? GetColourAsString(Color c) => $"{c.R / 255f},{c.G / 255f},{c.B / 255f}";
    private static Color? GetColourFromString(string inp, char divider)
    {
        string[] channels = inp.Split(divider);

        if (channels.Length == 3)
        {
            byte[] comps = channels.Select(x => (byte)Math.Round(double.Parse(x) * 255)).ToArray();
            return Color.FromRgb(comps[0], comps[1], comps[2]);
        }

        return null;
    }

    private void UpdateColourFromInput()
    {
        string hexCol = $"#{inp.Text?.Replace("#", string.Empty) ?? string.Empty}";

        if (Color.TryParse(hexCol, out Color c))
        {
            currentColour = c;
            isDirty = true;
        }

        RedrawColourDescendants();
    }

    private void RedrawColourDescendants()
    {
        var colour = new ImmutableSolidColorBrush(currentColour ?? new Color(0, 0, 0, 0));

        inp_Showcase.Background = colour;
        inp_Btn.Background = colour;
    }

    public string? CreateArgument()
    {
        if (!isDirty || currentColour == null)
            return null;

        return $"{key!}={GetColourAsString(currentColour.Value)}";
    }
}