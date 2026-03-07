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

        public void RegisterClick(Func<Task> callback)
        {
            this.callback += async () => await callback();
        }

        public void RegisterClick(Action callback)
        {
            this.callback += () => callback?.Invoke();
        }
    }
}