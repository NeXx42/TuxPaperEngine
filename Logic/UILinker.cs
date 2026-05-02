using Logic.Interfaces;

namespace Logic;

public class UILinker
{
    private static IUILinker? instance;

    public static void Register(IUILinker instance) => UILinker.instance = instance;

    public static async Task<IAuthenticationModal> GetAuthenticationModal(string? existingUsername = null)
        => await instance!.OpenAuthenticationModal(existingUsername);

    public static async Task CloseModal() => await instance!.CloseModals();
}
