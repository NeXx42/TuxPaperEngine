using System.IO;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Logic;
using Logic.Data;

namespace AvaloniaUI.Utils;

public static class ImageFetcher
{
    public static Task<ImageBrush?> GetIcon(IWorkshopEntry game, CancellationToken? cancellationToken = null)
    {
        TaskCompletionSource<ImageBrush?> req = new TaskCompletionSource<ImageBrush?>(TaskCreationOptions.RunContinuationsAsynchronously);
        ImageFetchingManager.QueueFetchWallpaperIcon(game, HandleReturn);

        return req.Task;

        Task HandleReturn(long gameId, object? brush)
        {
            if (cancellationToken?.IsCancellationRequested ?? false)
                return Task.CompletedTask;

            req.SetResult(brush != null ? (ImageBrush)brush : null);
            return Task.CompletedTask;
        }
    }

    public static async Task<object?> HandleWebBrushCreation(MemoryStream stream)
    {
        try
        {
            var bitmap = new Bitmap(stream);
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
        catch
        {
            return null;
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
