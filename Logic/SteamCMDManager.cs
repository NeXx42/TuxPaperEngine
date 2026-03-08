using System.Diagnostics;
using System.Text;

namespace Logic;

public interface IAuthenticationModal
{
    public Task Open();

    public Task<string> GetPassword();
    public Task SetMessage(string to);

    public Task Complete();
}

public enum DownloadStatus
{
    Waiting,
    Downloading,
    Finished,
    Failed
}

public static class SteamCMDManager
{
    private static SemaphoreSlim downloadLock = new SemaphoreSlim(1);

    private static Session? activeSession;
    private static HashSet<long> activeDownloads = new HashSet<long>();

    public static bool IsBeingDownloaded(long id) => activeDownloads.Contains(id);

    public static Action<long, DownloadStatus>? onDownloadChange;

    public static async Task DownloadAsset(long assetId, IAuthenticationModal authentication)
    {
        if (activeDownloads.Contains(assetId))
            return;

        activeDownloads.Add(assetId);
        await downloadLock.WaitAsync();

        try
        {
            if (activeSession == null)
            {
                activeSession = await HandleSteamCMDAuthentication(authentication);
            }

            if (activeSession != null)
            {
                onDownloadChange?.Invoke(assetId, DownloadStatus.Downloading);

                await activeSession.SendCommand($"workshop_download_item {ConfigManager.WALLPAPER_ENGINE_ID} {assetId}");

                if (await activeSession.WaitForResponse($"Downloaded item {assetId}"))
                {
                    onDownloadChange?.Invoke(assetId, DownloadStatus.Finished);
                }
                else
                {
                    throw new Exception($"Gave up on waiting for the download of {assetId}");
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            onDownloadChange?.Invoke(assetId, DownloadStatus.Failed);
        }
        finally
        {
            activeDownloads.Remove(assetId);
            downloadLock.Release();
        }

    }

    private static async Task<Session?> HandleSteamCMDAuthentication(IAuthenticationModal authentication)
    {
        string? username = (await ConfigManager.GetConfigValue(ConfigManager.ConfigKeys.SteamUsername))?.value;

        if (string.IsNullOrEmpty(username))
        {
            throw new Exception("No username set");
        }

        Session session = new Session();
        await session.SendCommand($"login {username}");

        if (await session.WaitForResponse("password:", 10000))
        {
            bool validSession = false;
            await authentication.Open();

            string password = await authentication.GetPassword();

            if (string.IsNullOrEmpty(password))
            {
                await session.Quit(true);
                return null;
            }

            await session.SendCommand(password);
            await session.WaitForResponse("Logging in user");

            await authentication.SetMessage("Logging in...");

            // this doesnt work?
            if (await session.WaitForResponse("Please confirm the login in the Steam Mobile app on your phone.", 20000))
            {
                for (int i = 0; i < 10; i++)
                {
                    if (await session.WaitForResponse("Waiting for user info...OK"))
                    {
                        validSession = true;
                        break;
                    }
                }
            }
            else if (await session.WaitForResponse("Rate Limit Exceeded"))
            {
                await authentication.SetMessage("Rate limited");
                return null;
            }
            else
            {
                validSession = true;
            }

            if (!validSession)
            {
                await authentication.SetMessage("Failed to login");
                return null;
            }
            else
            {
                await authentication.Complete();
            }
        }

        return session;
    }


    private class Session : IDisposable
    {
        private SemaphoreSlim outputLock = new SemaphoreSlim(1);
        private StringBuilder output = new StringBuilder();

        private Process process;
        private Thread readingThread;

        public Session()
        {
            process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/home/matth/Steam/steamcmd.sh",

                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,

                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            readingThread = new Thread(ReadProgressOfExtraction);
            readingThread.Start();
        }

        private void ReadProgressOfExtraction()
        {
            int charNumber;

            while (!process.HasExited)
            {
                while ((charNumber = process.StandardOutput.Read()) != -1)
                {
                    outputLock.Wait();
                    Console.Write((char)charNumber);
                    output.Append((char)charNumber);
                    outputLock.Release();
                }
            }
        }

        public async Task SendCommand(string command)
        {
            await outputLock.WaitAsync();
            output.Clear();
            outputLock.Release();

            await process.StandardInput.WriteLineAsync(command);
            await process.StandardInput.FlushAsync();
        }

        public async Task<bool> WaitForResponse(string res, int timeoutMs = 3000)
        {
            using var cts = new CancellationTokenSource(timeoutMs);

            try
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    await outputLock.WaitAsync();
                    string line = output.ToString();
                    outputLock.Release();

                    if (line.Contains(res))
                        return true;
                }
            }
            catch (OperationCanceledException)
            {
                // timed out
            }

            return false;
        }

        public async Task Quit(bool force)
        {
            if (force)
            {
                process.Kill();
                return;
            }

            await SendCommand("quit");
            await process.WaitForExitAsync();
        }

        public void Dispose()
        {
            _ = Quit(false);
        }
    }
}
