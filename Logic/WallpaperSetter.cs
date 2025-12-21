using System.Diagnostics;
using System.IO.IsolatedStorage;
using Logic.db;

namespace Logic;

public static class WallpaperSetter
{
    public static WallpaperOptions.ScreenSettings[] WorkOutScreenOffsets(ConfigManager.Screen[] screens)
    {
        screens = screens.OrderBy(x => x.priority).ToArray();
        WallpaperOptions.ScreenSettings[] res = new WallpaperOptions.ScreenSettings[screens.Length];

        float xDivision = 1f / screens.Length;

        for (int i = 0; i < screens.Length; i++)
        {
            res[i] = new WallpaperOptions.ScreenSettings()
            {
                screenName = screens[i].screenName,
                offsetX = (i + .5f) * xDivision
            };
        }

        return res;
    }

    public static async void SetWallpaper(string path, WallpaperOptions options)
    {
        dbo_Config? val = await ConfigManager.GetConfigValue(ConfigManager.ConfigKeys.ExecutableLocation);

        if (val == null || string.IsNullOrEmpty(val.value))
        {
            throw new Exception("No executable linked");
        }

        KillExistingRuns(val.value);

        ProcessStartInfo info = options.CreateArgList();
        info.FileName = val.value;
        info.ArgumentList.Add(path);

        info.UseShellExecute = false;
        info.RedirectStandardOutput = true;
        info.RedirectStandardError = true;
        info.CreateNoWindow = true;

        Process p = new Process();
        p.StartInfo = info;
        p.Start();
    }

    private static void KillExistingRuns(string exeName)
    {
        foreach (var process in Process.GetProcesses())
        {
            try
            {
                var processExe = process.MainModule?.FileName;

                if (processExe != null && Path.GetFullPath(processExe) == exeName)
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
