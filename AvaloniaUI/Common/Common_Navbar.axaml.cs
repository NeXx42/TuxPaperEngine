using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvaloniaUI.Common;

public partial class Common_Navbar : UserControl
{
    private Action<int>? callback;
    private Border[] options;

    public Common_Navbar()
    {
        InitializeComponent();

        options = [
            btn_Home,
            btn_Workshop,
            btn_Settings,
        ];

        for (int i = 0; i < options.Length; i++)
        {
            int temp = i;
            options[i].PointerPressed += (_, __) => UpdateSelectedPage(temp);
        }
    }

    public void Setup(int startPage, Action<int> onSelect)
    {
        callback = onSelect;

        UpdateSelectedPage(startPage);
    }

    private void UpdateSelectedPage(int index)
    {
        for (int i = 0; i < options.Length; i++)
            if (i == index)
                options[i].Classes.Add("active");
            else
                options[i].Classes.Remove("active");

        callback?.Invoke(index);
    }
}