using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Logic;

namespace AvaloniaUI.Common;

public partial class AuthenticationModal : UserControl, IAuthenticationModal
{
    private TaskCompletionSource<string>? passwordRequest;

    public AuthenticationModal()
    {
        InitializeComponent();
        btn_login.Click += (_, __) => passwordRequest?.SetResult(inp_password.Text!);
        cont.PointerPressed += (_, __) => Exit();

        Complete();
    }

    public Task Open()
    {
        IsVisible = true;
        lbl_error.Content = "";

        return Task.CompletedTask;
    }

    public Task<string> GetPassword()
    {
        passwordRequest = new TaskCompletionSource<string>();
        return passwordRequest.Task;
    }

    public Task SetMessage(string to)
    {
        lbl_error.Content = to;
        return Task.CompletedTask;
    }

    public Task Complete()
    {
        IsVisible = false;
        inp_password.Text = "";

        return Task.CompletedTask;
    }

    private void Exit()
    {
        passwordRequest?.SetResult(string.Empty);
        passwordRequest = null;

        Complete();
    }
}