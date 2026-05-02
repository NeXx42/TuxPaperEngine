using System.Threading.Tasks;
using Avalonia.Controls;

namespace AvaloniaUI.Interfaces;

public interface IModal
{
    public bool isBlocking { get; }

    public Task Exit();
}
