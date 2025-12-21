using System.IO;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Logic;

namespace AvaloniaUI.Utils;

public static class ImageFetcher
{
    public static Task<ImageBrush?> GetIcon(long gameId)
    {
        TaskCompletionSource<ImageBrush?> req = new TaskCompletionSource<ImageBrush?>(TaskCreationOptions.RunContinuationsAsynchronously);
        WorkshopManager.QueueFetchWallpaperIcon(gameId, HandleReturn);

        return req.Task;

        Task HandleReturn(long gameId, object? brush)
        {
            req.SetResult(brush != null ? (ImageBrush)brush : null);
            return Task.CompletedTask;
        }
    }

    public static async Task<object?> HandleBrushCreation(string path)
    {
        if (!File.Exists(path))
            return null;

        var bitmap = new Bitmap(path);
        ImageBrush? brush = null;

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            brush = new ImageBrush(bitmap)
            {
                Stretch = Stretch.UniformToFill
            };
        });

        return brush;
    }
}
