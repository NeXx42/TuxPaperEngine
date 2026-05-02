using System.Data.SQLite;
using System.Diagnostics;
using System.Security.Authentication;
using System.Text;
using CSharpSqliteORM;
using Logic.Interfaces;

namespace Logic;

public interface IAuthenticationModal
{
    public Task<(string usr, string pass)> GetCredentials();
    public Task UpdateMessage(string to, bool isError);
    public Task UpdateStatus(AuthenticationStatus to);

    public Task Complete();
}

public enum AuthenticationStatus
{
    Login,
    LoggingIn,
    WaitingForGuard,
    Success,
    Fail
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
    private static bool isTryingToAuthenticate;
    private static SemaphoreSlim downloadLock = new SemaphoreSlim(1);

    private static Session? activeSession;
    private static Dictionary<long, DownloadStatus> activeDownloads = new Dictionary<long, DownloadStatus>();

    private static Thread? downloadThread;

    public static DownloadStatus? GetActiveStatus(long id) => activeDownloads.TryGetValue(id, out DownloadStatus status) ? status : null;
    public static Action<long, DownloadStatus>? onDownloadChange;


    public static async Task DownloadAsset(long assetId)
    {
        if (activeDownloads.ContainsKey(assetId))
            return;

        onDownloadChange?.Invoke(assetId, DownloadStatus.Waiting);
        activeDownloads.Add(assetId, DownloadStatus.Waiting);

        if (downloadThread == null)
        {
            downloadThread = new Thread(() => _ = SteamCMDThread());
            downloadThread.Start();
        }
    }

    public static async Task<bool> TryToAuthenticate()
    {
        if (activeSession != null)
            return true;

        if (downloadThread == null)
        {
            downloadThread = new Thread(() => _ = SteamCMDThread());
            downloadThread.Start();
        }

        return await HandleSteamCMDAuthentication();
    }

    private static async Task SteamCMDThread()
    {
        while (true)
        {
            if (activeDownloads.Count == 0)
            {
                await Task.Delay(100);
                continue;
            }

            long assetId = activeDownloads.Keys.First();
            await downloadLock.WaitAsync();

            try
            {
                if (!await HandleSteamCMDAuthentication())
                    throw new AuthenticationException("Failed to authenticate with SteamCMD");

                if (activeSession != null)
                {
                    onDownloadChange?.Invoke(assetId, DownloadStatus.Downloading);
                    activeDownloads[assetId] = DownloadStatus.Downloading;

                    string response = await activeSession.SendAsync($"workshop_download_item {ConfigManager.WALLPAPER_ENGINE_ID} {assetId}");

                    if (response.Contains($"Downloaded item {assetId}"))
                    {
                        onDownloadChange?.Invoke(assetId, DownloadStatus.Finished);
                        activeDownloads[assetId] = DownloadStatus.Finished;
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
                activeDownloads[assetId] = DownloadStatus.Failed;
            }
            finally
            {
                activeDownloads.Remove(assetId);
                downloadLock.Release();
            }
        }
    }

    private static async Task<bool> HandleSteamCMDAuthentication()
    {
        if (isTryingToAuthenticate)
            return false;

        isTryingToAuthenticate = true;

        if (activeSession != null)
            return true;

        IAuthenticationModal? authenticationModal = null;

        string? username = (await ConfigManager.GetConfigValue(ConfigManager.ConfigKeys.SteamUsername))?.value;
        string? password = string.Empty;

        try
        {
            if (string.IsNullOrEmpty(username))
                await GetCredentials();

            Session session = new Session(Path.Combine(AppContext.BaseDirectory, "steamcmd_bridge"));
            string response;

            await session.ReadAsync();
            response = await session.SendAsync($"login {username}");

            if (response.Contains("Cached credentials not found."))
            {
                if (await HandleLogin(false))
                {
                    activeSession = session;
                    authenticationModal?.UpdateStatus(AuthenticationStatus.Success);
                    authenticationModal?.UpdateMessage("Logged in", false);

                    isTryingToAuthenticate = false;
                    return true;
                }
                else
                {
                    isTryingToAuthenticate = false;
                    return false;
                }
            }
            else if (response.Contains("Waiting for user info"))
            {
                activeSession = session;
                authenticationModal?.UpdateStatus(AuthenticationStatus.Success);
                authenticationModal?.UpdateMessage("Logged in", false);

                isTryingToAuthenticate = false;
                return true;
            }

            authenticationModal?.UpdateMessage("Unknown error", true);

            // funcs

            async Task GetCredentials()
            {
                if (authenticationModal == null)
                    authenticationModal = await UILinker.GetAuthenticationModal(username);

                await authenticationModal.UpdateStatus(AuthenticationStatus.Login);
                (username, password) = await authenticationModal.GetCredentials();
                await authenticationModal.UpdateStatus(AuthenticationStatus.LoggingIn);

                await ConfigManager.SetConfigValue(ConfigManager.ConfigKeys.SteamUsername, username);
            }

            async Task<bool> HandleLogin(bool isRetry)
            {
                if (string.IsNullOrEmpty(password))
                    await GetCredentials();

                if (isRetry)
                {
                    await session.SendAsync($"login {username}");
                }

                response = await session.SendAsync(password!);

                if (response.Contains("This account is protected by a Steam Guard mobile authenticator."))
                {
                    authenticationModal?.UpdateMessage("Waiting for steam guard confirmation", false);
                    response = await session.SendAsync("");

                    if (response.Contains("Waiting for user info"))
                        return true;

                    authenticationModal?.UpdateStatus(AuthenticationStatus.Fail);
                    authenticationModal?.UpdateMessage("Failed to login", true);
                    return false;
                }
                else if (response.Contains("Invalid Password"))
                {
                    authenticationModal?.UpdateMessage("Invalid password", true);
                    password = null;

                    return await HandleLogin(true);
                }
                else if (response.Contains("Rate Limit Exceeded"))
                {
                    authenticationModal?.UpdateStatus(AuthenticationStatus.Fail);
                    authenticationModal?.UpdateMessage("Rate limited", true);

                    return false;
                }

                return false;
            }
        }
        catch (TaskCanceledException)
        {
            isTryingToAuthenticate = false;
            return false;
        }

        isTryingToAuthenticate = false;
        return false;
    }

    public class Session
    {
        private readonly Process process;
        private StringBuilder stream;

        public Session(string bridgeScriptPath)
        {
            process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = bridgeScriptPath,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            process.Start();
            stream = new StringBuilder();

            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data == null)
                    return;

                lock (stream)
                {
                    stream.Append(e.Data);
                }
            };

            process.BeginOutputReadLine();
        }

        public async Task<string> ReadAsync()
        {
            while (true)
            {
                await Task.Delay(50);
                lock (stream)
                {
                    string msg = stream.ToString();
                    int idx = msg.IndexOf('\x1E');
                    if (idx >= 0)
                    {
                        string result = msg[..idx];
                        stream.Clear();
                        return result;
                    }
                }
            }
        }

        public async Task<string> SendAsync(string command)
        {
            await SendWithoutWaiting(command);
            return await ReadAsync();
        }

        public async Task SendWithoutWaiting(string command)
        {
            lock (stream)
            {
                stream.Clear();
            }

            await process.StandardInput.WriteLineAsync(command);
            await process.StandardInput.FlushAsync();
        }

        public void Dispose()
        {
            if (!process.HasExited) process.Kill();
            process.Dispose();
        }
    }
}
