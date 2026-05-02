namespace Logic.Interfaces;

public interface IUILinker
{
    public Task<IAuthenticationModal> OpenAuthenticationModal(string? existingUsername = null);
    public Task CloseModals();
}


public class UILinkedAction
{
    public Action<string>? stringCallback;

    public void Invoke(string str) => stringCallback?.Invoke(str);
}
