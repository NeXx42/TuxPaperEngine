using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using AvaloniaUI.Interfaces;
using Logic;
using Logic.Interfaces;

namespace AvaloniaUI.Common;

public partial class AuthenticationModal : UserControl, IModal, IAuthenticationModal
{
    private AuthenticationStatus currentStatus;

    private TaskCompletionSource<(string username, string password)>? response;
    public bool isBlocking => false;

    public AuthenticationModal()
    {
        InitializeComponent();
        btn_Act.RegisterClick(TryToCompleteLogin);
        btn_Close.RegisterClick(MainWindow.CloseModal);
    }

    public void Open(string? existingUsername = null)
    {
        lbl_msg.Content = "";
        inp_username.Text = existingUsername;
    }

    private async Task TryToCompleteLogin()
    {
        switch (currentStatus)
        {
            case AuthenticationStatus.Login: break;
            case AuthenticationStatus.LoggingIn: return;
            default:
                await MainWindow.CloseModal();
                return;
        }

        if (string.IsNullOrEmpty(inp_username.Text) || string.IsNullOrEmpty(inp_password.Text))
        {
            await UpdateMessage("Please enter a username and password", true);
            return;
        }

        response?.SetResult((inp_username.Text!, inp_password.Text!));
    }


    public Task SetMessage(string to)
    {
        lbl_msg.Content = to;
        return Task.CompletedTask;
    }

    public Task Exit()
    {
        if (!(response?.Task?.IsCompleted ?? false))
        {
            response?.SetCanceled();
        }

        return Task.CompletedTask;
    }

    public Task<(string usr, string pass)> GetCredentials()
    {
        response = new TaskCompletionSource<(string username, string password)>();
        return response.Task;
    }

    public Task UpdateMessage(string to, bool isError)
    {
        Dispatcher.UIThread.Post(() =>
        {
            lbl_msg.Classes.Clear();
            lbl_msg.Content = to;

            if (isError)
                lbl_msg.Classes.Add("Error");
        });
        return Task.CompletedTask;
    }

    public Task Complete()
    {
        throw new NotImplementedException();
    }

    public Task UpdateStatus(AuthenticationStatus to)
    {
        currentStatus = to;

        Dispatcher.UIThread.Post(() =>
        {
            switch (to)
            {
                case AuthenticationStatus.Login:
                    btn_Act.Label = "Login";
                    break;

                case AuthenticationStatus.Success:
                case AuthenticationStatus.Fail:
                    btn_Act.Label = "Close";
                    break;

                default:
                    btn_Act.Label = "Logging in";
                    break;
            }
        });
        return Task.CompletedTask;
    }
}