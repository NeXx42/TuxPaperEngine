using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvaloniaUI.Pages.Common;

public partial class Common_Tag : UserControl
{
    public string? tag { private set; get; }
    public bool isSelected { private set; get; }

    private Action? callback;
    private bool isToggleable;

    public Common_Tag()
    {
        InitializeComponent();
        DataContext = this;

        ctrl.PointerPressed += (_, __) => Toggle(false);
    }

    public void Draw(string name, bool selected, Action? onToggle = null)
    {
        if (isToggleable = onToggle != null)
            callback = onToggle;

        tag = name;
        tagName.Content = name;

        isSelected = !selected;
        Toggle(true);
    }

    private void Toggle(bool force)
    {
        if (!isToggleable && !force)
            return;

        isSelected = !isSelected;
        callback?.Invoke();

        if (isSelected)
        {
            this.Classes.Add("active");
        }
        else
        {
            this.Classes.Remove("active");
        }
    }
}