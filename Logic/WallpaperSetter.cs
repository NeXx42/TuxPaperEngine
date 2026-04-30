using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Text.RegularExpressions;
using CSharpSqliteORM;
using Logic.Data;
using Logic.Database;
using Logic.db;
using Logic.Enums;

namespace Logic;

public enum DefaultProps
{
    DefaultProp_Clamp,
    DefaultProp_Scaling,

    DefaultProp_OffsetX,
    DefaultProp_OffsetY,

    DefaultProp_BGColour,
    DefaultProp_Contrast,
    DefaultProp_Saturation,
}

public static class WallpaperSetter
{
    public static async Task InstallEngineLocally(string fork, string path, IProgress<int> progress, IProgress<int> taskProgress, Action<string> console)
    {
        const string folderName = "linux-wallpaperengine";

        progress.Report(0);
        await RunCommand(path, "git", "clone", "--recursive", fork, folderName, "--progress");
        progress.Report(1);
        taskProgress.Report(0);

        await RunCommand(Path.Combine(path, folderName), "mkdir", "build");
        await RunCommand(Path.Combine(path, folderName, "build"), "cmake", "-DCMAKE_BUILD_TYPE='Release'", "..");

        progress.Report(2);
        taskProgress.Report(0);

        await RunCommand(Path.Combine(path, folderName, "build"), "make");

        await Database_Manager.AddOrUpdate(new dbo_Config()
        {
            key = ConfigManager.ConfigKeys.ExecutableLocation.ToString(),
            value = Path.Combine(path, folderName, "build", "output")
        }, SQLFilter.Equal(nameof(dbo_Config.key), ConfigManager.ConfigKeys.ExecutableLocation.ToString()));

        await Database_Manager.AddOrUpdate(new dbo_Config()
        {
            key = ConfigManager.ConfigKeys.ExecutableType.ToString(),
            value = ((int)EngineType.Directory).ToString()
        }, SQLFilter.Equal(nameof(dbo_Config.key), ConfigManager.ConfigKeys.ExecutableType.ToString()));

        async Task RunCommand(string workingDir, string command, params string[] args)
        {
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = command;
            info.WorkingDirectory = workingDir;

            foreach (string arg in args)
                info.ArgumentList.Add(arg);

            info.UseShellExecute = false;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            info.CreateNoWindow = true;

            Process p = new Process();

            p.OutputDataReceived += UpdateProgress;
            p.ErrorDataReceived += UpdateProgress;

            p.StartInfo = info;
            p.Start();

            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            await p.WaitForExitAsync();
        }

        void UpdateProgress(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;

            console?.Invoke(e.Data);
            var match = Regex.Match(e.Data, @"\d+%");

            if (match.Success)
                taskProgress.Report(int.Parse(match.Value.Replace("%", "")));
        }
    }

    public static async Task<string> TryFindExecutableLocation()
    {
        dbo_Config? engineType = await ConfigManager.GetConfigValue(ConfigManager.ConfigKeys.ExecutableType);
        dbo_Config? enginePath = await ConfigManager.GetConfigValue(ConfigManager.ConfigKeys.ExecutableLocation);

        if (engineType == null || enginePath == null)
        {
            throw new Exception("Invalid install of the engine. Either set the location or command");
        }

        const string exeDir = "linux-wallpaperengine";
        return (engineType?.value == ((int)EngineType.Directory).ToString()) ? Path.Combine(enginePath.value!, exeDir) : (enginePath.value ?? exeDir);
    }

    public static WallpaperOptions.ScreenSettings[] WorkOutScreenOffsets(float offsetX, float offsetY)
    {
        ConfigManager.Screen[] screens = ConfigManager.GetScreensOrdered();
        WallpaperOptions.ScreenSettings[] res = new WallpaperOptions.ScreenSettings[screens.Length];

        float midWay = (screens.Length - 1) * .5f;
        float xDivision = 1f / screens.Length;

        for (int i = 0; i < screens.Length; i++)
        {
            res[i] = new WallpaperOptions.ScreenSettings()
            {
                screenName = screens[i].screenName,
                offsetX = (xDivision * (i - midWay)) + offsetX,
                offsetY = offsetY
            };
        }

        return res;
    }

    public static async Task SetWallpaper(long? id)
    {
        string command = await TryFindExecutableLocation();

        if (!id.HasValue)
        {
            string? lastSet = (await ConfigManager.GetConfigValue(ConfigManager.ConfigKeys.LastSetWallpaper))?.value;
            lastSet.TryParseLong(out id);
        }

        if (!id.HasValue)
        {
            throw new Exception($"Couldn't find wallpaper - {id}");
        }

        dbo_WallpaperSettings[] savedValues = await ConfigManager.GetWallpaperSettings(id.Value);
        WallpaperOptions options = new WallpaperOptions(savedValues);

        KillExistingRuns(command);

        ProcessStartInfo info = options.CreateArgList();
        info.FileName = command;
        info.ArgumentList.Add(id.ToString()!);

        info.UseShellExecute = false;
        info.RedirectStandardOutput = true;
        info.RedirectStandardError = true;
        info.CreateNoWindow = true;

        Process p = new Process();
        p.StartInfo = info;
        p.Start();

        dbo_Config? saveScriptLocation = await ConfigManager.GetConfigValue(ConfigManager.ConfigKeys.SaveStartupScriptLocation);

        if (saveScriptLocation != null)
        {
            await SaveCommandToFile(command, info, saveScriptLocation.value);
        }

        await ConfigManager.SetConfigValue(ConfigManager.ConfigKeys.LastSetWallpaper, id.ToString());
    }

    private static async Task SaveCommandToFile(string command, ProcessStartInfo arguments, string? path)
    {
        if (string.IsNullOrEmpty(path))
            return;

        if (File.Exists(path))
            File.Delete(path);

        string? selfContained = Environment.GetEnvironmentVariable("APPIMAGE");

        if (!string.IsNullOrEmpty(selfContained))
        {
            using (var writer = new StreamWriter(path))
            {
                await writer.WriteLineAsync("#!/bin/bash");
                await writer.WriteLineAsync($"{selfContained} --startup");
            }

            GivePermission(path);
            return;
        }

        ProcessStartInfo info = new ProcessStartInfo();
        info.FileName = "/bin/echo";

        info.ArgumentList.Add(command!);

        foreach (var arg in arguments.ArgumentList)
            info.ArgumentList.Add(arg);

        info.RedirectStandardOutput = true;

        using (Process p = Process.Start(info)!)
        {
            using (var writer = new StreamWriter(path))
            {
                await writer.WriteLineAsync("#!/bin/bash");
                await writer.WriteLineAsync(await p.StandardOutput.ReadToEndAsync());
            }

            p.WaitForExit();
            GivePermission(path);
        }

        void GivePermission(string p)
        {
#pragma warning disable CA1416 // Validate platform compatibility
            File.SetUnixFileMode(p,
                UnixFileMode.UserRead |
                UnixFileMode.UserWrite |
                UnixFileMode.UserExecute |
                UnixFileMode.GroupRead |
                UnixFileMode.GroupWrite |
                UnixFileMode.GroupExecute |
                UnixFileMode.OtherRead |
                UnixFileMode.OtherWrite |
                UnixFileMode.OtherExecute
            );
#pragma warning restore CA1416 // Validate platform compatibility
        }
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

    public class WallpaperOptions
    {
        private ClampOptions? clampOptions;
        private ScalingOptions? scalingOption;
        private ScreenSettings[] screens;

        private double? saturation;
        private double? contrast;
        private string? borderColour;

        private List<string>? customProperties = new List<string>();


        public struct ScreenSettings
        {
            public required string screenName;
            public float? offsetX;
            public float? offsetY;
        }

        public WallpaperOptions(dbo_WallpaperSettings[] overrideOptions)
        {
            float offsetX = 0;
            float offsetY = 0;

            foreach (dbo_WallpaperSettings setting in overrideOptions)
            {
                if (Enum.TryParse(setting.settingKey, out DefaultProps prop))
                {
                    switch (prop)
                    {
                        case DefaultProps.DefaultProp_Clamp: clampOptions = (ClampOptions)int.Parse(setting.settingValue!); break;
                        case DefaultProps.DefaultProp_Scaling: scalingOption = (ScalingOptions)int.Parse(setting.settingValue!); break;
                        case DefaultProps.DefaultProp_OffsetX: float.TryParse(setting.settingValue, out offsetX); break;
                        case DefaultProps.DefaultProp_OffsetY: float.TryParse(setting.settingValue, out offsetY); break;


                        case DefaultProps.DefaultProp_BGColour: borderColour = setting.settingValue; break;

                        case DefaultProps.DefaultProp_Contrast: setting.settingValue.TryParseDouble(out contrast); break;
                        case DefaultProps.DefaultProp_Saturation: setting.settingValue.TryParseDouble(out saturation); break;
                    }

                    continue;
                }

                customProperties.Add($"{setting.settingKey}={setting.settingValue}");
            }

            screens = WorkOutScreenOffsets(-offsetX, -offsetY);
        }


        public ProcessStartInfo CreateArgList(params string[] injectedArgs)
        {
            ProcessStartInfo info = new ProcessStartInfo();

            foreach (string arg in injectedArgs)
                info.ArgumentList.Add(arg);

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

            if (contrast.HasValue)
            {
                info.ArgumentList.Add("--contrast");
                info.ArgumentList.Add((.5f + (contrast / 60) * 1.5f).ToString()!);
            }

            if (saturation.HasValue)
            {
                info.ArgumentList.Add("--saturation");
                info.ArgumentList.Add((saturation / 60f * 1.4f).ToString()!);
            }

            if (!string.IsNullOrEmpty(borderColour))
            {
                info.ArgumentList.Add("--border-colour");
                info.ArgumentList.Add(borderColour);
            }

            if (customProperties != null)
            {
                foreach (string arg in customProperties)
                {
                    info.ArgumentList.Add("--set-property");
                    info.ArgumentList.Add(arg);
                }
            }

            info.ArgumentList.Add("--bg");
            return info;
        }
    }

}
