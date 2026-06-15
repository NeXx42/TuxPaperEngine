using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AnimatedImage.Avalonia;
using Logic;
using Logic.Data;

namespace AvaloniaUI.Utils;

public static class ImageFetcher
{
    public static Task<AnimatedImageSource?> GetIcon(IWorkshopEntry game, CancellationToken? cancellationToken = null)
    {
        TaskCompletionSource<AnimatedImageSource?> req = new TaskCompletionSource<AnimatedImageSource?>(TaskCreationOptions.RunContinuationsAsynchronously);
        ImageFetchingManager.QueueFetchWallpaperIcon(game, HandleReturn);

        return req.Task;

        Task HandleReturn(long gameId, object? brush)
        {
            if (cancellationToken?.IsCancellationRequested ?? false)
                return Task.CompletedTask;

            req.SetResult(brush != null ? (AnimatedImageSource)brush : null);
            return Task.CompletedTask;
        }
    }

    public static async Task<object?> HandleWebBrushCreation(MemoryStream stream)
    {
        return new AnimatedImageSourceStream(stream);
    }


    public static async Task<object?> HandleBrushCreation(string path)
    {
        if (!File.Exists(path))
            return null;

        return new AnimatedImageSourceUri(new Uri(path));
    }
}
