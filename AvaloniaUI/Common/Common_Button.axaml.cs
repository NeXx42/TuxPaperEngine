using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;

namespace AvaloniaUI.Common
{
    public partial class Common_Button : UserControl
    {
        private Action? callback;

        public Common_Button()
        {
            InitializeComponent();
            DataContext = this;

            ctrl.PointerPressed += (_, __) => callback?.Invoke();
        }

        public static readonly StyledProperty<string> LabelProperty =
            AvaloniaProperty.Register<Common_Button, string>(nameof(Label), string.Empty);

        public string Label
        {
            get => GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public void ClearCallback() => callback = null;

        public void RegisterClick(Func<Task> callback, string? activeLabel = null)
        {
            this.callback += async () =>
            {
                if (string.IsNullOrEmpty(activeLabel))
                {
                    await callback();
                    return;
                }

                string temp = Label;
                Label = activeLabel;

                await callback();

                Label = temp;
            };
        }

        public void RegisterClick(Action callback)
        {
            this.callback += () => callback?.Invoke();
        }
    }
}