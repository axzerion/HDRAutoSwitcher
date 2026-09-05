using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace HDRAutoSwitcher
{
    internal class GameLauncher
    {
        private readonly SteamService steamService;

        public GameLauncher()
        {
            steamService = new SteamService();
        }

        public void Launch(GameProfile profile)
        {
            Process? initialProcess = null;

            if (!string.IsNullOrWhiteSpace(profile.SteamAppId))
            {
                Debug.WriteLine(
                    $"Launching {profile.Name} through Steam...");

                if (!steamService.LaunchGame(profile.SteamAppId))
                {
                    Debug.WriteLine("Failed to launch Steam game.");
                    return;
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(profile.LaunchPath) ||
                    !File.Exists(profile.LaunchPath))
                {
                    Debug.WriteLine("Game executable not found.");
                    return;
                }

                Debug.WriteLine($"Launching: {profile.LaunchPath}");

                initialProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = profile.LaunchPath,
                    UseShellExecute = true
                });

                if (initialProcess == null)
                {
                    Debug.WriteLine("Failed to start process.");
                    return;
                }

                Debug.WriteLine(
                    $"Initial process started (PID: {initialProcess.Id})");

                string launchedProcessName =
                    Path.GetFileNameWithoutExtension(profile.LaunchPath);

                if (launchedProcessName.Equals(
                        profile.ProcessName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    Debug.WriteLine(
                        "Waiting for initial process to finish...");

                    initialProcess.WaitForExit();

                    Debug.WriteLine("Initial process exited.");
                }
            }

            Debug.WriteLine(
                $"Waiting for {profile.ProcessName}.exe...");

            Process? gameProcess = null;

            for (int i = 0; i < 240; i++)
            {
                Process[] processes =
                    Process.GetProcessesByName(profile.ProcessName);

                foreach (Process process in processes)
                {
                    if (initialProcess != null &&
                        process.Id == initialProcess.Id)
                    {
                        continue;
                    }

                    try
                    {
                        process.Refresh();

                        if (!process.HasExited &&
                            process.MainWindowHandle != IntPtr.Zero)
                        {
                            gameProcess = process;
                            break;
                        }
                    }
                    catch
                    {
                        // Process may have disappeared while checking it.
                    }
                }

                if (gameProcess != null)
                {
                    break;
                }

                Thread.Sleep(500);
            }

            if (gameProcess == null)
            {
                Debug.WriteLine(
                    $"Timed out waiting for {profile.ProcessName}.exe.");

                return;
            }

            Debug.WriteLine(
                $"Game process found (PID: {gameProcess.Id})");

            Debug.WriteLine("Waiting for game to exit...");

            gameProcess.WaitForExit();

            Debug.WriteLine("Game process has exited.");
        }
    }
}