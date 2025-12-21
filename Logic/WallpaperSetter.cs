using System.Diagnostics;
using System.IO.IsolatedStorage;

namespace Logic;

public static class WallpaperSetter
{
    public const string LINUX_WALLPAPERENGINE = "/home/matth/Downloads/linux-wallpaperengine/build/output/linux-wallpaperengine";

    public static void SetWallpaper(string path, WallpaperOptions options)
    {
        KillExistingRuns();

        ProcessStartInfo info = options.CreateArgList();
        info.FileName = LINUX_WALLPAPERENGINE;
        info.ArgumentList.Add(path);

        info.UseShellExecute = false;
        info.RedirectStandardOutput = true;
        info.RedirectStandardError = true;
        info.CreateNoWindow = true;

        Process p = new Process();
        p.StartInfo = info;
        p.Start();
    }

    private static void KillExistingRuns()
    {
        foreach (var process in Process.GetProcesses())
        {
            try
            {
                var processExe = process.MainModule?.FileName;

                if (processExe != null && Path.GetFullPath(processExe) == LINUX_WALLPAPERENGINE)
                {
                    process.Kill();
                    process.WaitForExit();
                }
            }
            catch { }
        }
    }

    public enum ScalingOptions
    {
        stretch,
        fit,
        fill,
    }

    public enum ClampOptions
    {
        clamp,
        border,
        repeat
    }

    public struct WallpaperOptions
    {
        public ClampOptions? clampOptions;
        public ScalingOptions? scalingOption;
        public ScreenSettings[] screens;

        public struct ScreenSettings
        {
            public required string screenName;
            public float? offsetX;
            public float? offsetY;
        }


        public ProcessStartInfo CreateArgList()
        {
            ProcessStartInfo info = new ProcessStartInfo();

            if (scalingOption.HasValue)
            {
                info.ArgumentList.Add("--scaling");
                info.ArgumentList.Add(scalingOption.ToString()!);
            }

            info.ArgumentList.Add("--clamp");
            info.ArgumentList.Add((clampOptions ?? ClampOptions.clamp).ToString());

            foreach (ScreenSettings screen in screens)
            {
                info.ArgumentList.Add("--screen-root");
                info.ArgumentList.Add(screen.screenName);

                if (screen.offsetX.HasValue)
                {
                    info.ArgumentList.Add("--offset-x");
                    info.ArgumentList.Add(screen.offsetX.ToString()!);
                }

                if (screen.offsetY.HasValue)
                {
                    info.ArgumentList.Add("--offset-y");
                    info.ArgumentList.Add(screen.offsetY.ToString()!);
                }
            }

            info.ArgumentList.Add("--bg");
            return info;
        }
    }

}
