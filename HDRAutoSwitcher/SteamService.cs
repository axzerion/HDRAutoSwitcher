using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace HDRAutoSwitcher
{
    internal class SteamService
    {
        public string? GetSteamPath()
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(
                @"Software\Valve\Steam");

            string? steamPath =
                key?.GetValue("SteamPath") as string;

            if (string.IsNullOrWhiteSpace(steamPath))
            {
                return null;
            }

            return steamPath.Replace('/', '\\');
        }

        public List<string> GetSteamLibraries()
        {
            List<string> libraries = new List<string>();

            string? steamPath = GetSteamPath();

            if (steamPath == null)
            {
                return libraries;
            }

            libraries.Add(steamPath);

            string libraryFile = Path.Combine(
                steamPath,
                "steamapps",
                "libraryfolders.vdf");

            if (!File.Exists(libraryFile))
            {
                return libraries;
            }

            string content = File.ReadAllText(libraryFile);

            MatchCollection matches = Regex.Matches(
                content,
                "\"path\"\\s+\"([^\"]+)\"");

            foreach (Match match in matches)
            {
                string path = match.Groups[1].Value
                    .Replace(@"\\", @"\")
                    .Replace('/', '\\');

                if (!libraries.Contains(
                        path,
                        StringComparer.OrdinalIgnoreCase))
                {
                    libraries.Add(path);
                }
            }

            return libraries;
        }

        public List<SteamGame> GetInstalledGames()
        {
            List<SteamGame> games = new List<SteamGame>();

            List<string> libraries = GetSteamLibraries();

            foreach (string library in libraries)
            {
                string steamAppsPath = Path.Combine(
                    library,
                    "steamapps");

                if (!Directory.Exists(steamAppsPath))
                {
                    continue;
                }

                string[] manifestFiles = Directory.GetFiles(
                    steamAppsPath,
                    "appmanifest_*.acf");

                foreach (string manifestFile in manifestFiles)
                {
                    string content = File.ReadAllText(manifestFile);

                    Match appIdMatch = Regex.Match(
                        content,
                        "\"appid\"\\s+\"([^\"]+)\"");

                    Match nameMatch = Regex.Match(
                        content,
                        "\"name\"\\s+\"([^\"]+)\"");

                    Match installDirMatch = Regex.Match(
                        content,
                        "\"installdir\"\\s+\"([^\"]+)\"");

                    if (!appIdMatch.Success ||
                        !nameMatch.Success ||
                        !installDirMatch.Success)
                    {
                        continue;
                    }

                    string appId = appIdMatch.Groups[1].Value;
                    string name = nameMatch.Groups[1].Value;
                    string installDir = installDirMatch.Groups[1].Value;

                    games.Add(new SteamGame(
                        appId,
                        name,
                        installDir));
                }
            }

            return games;
        }
        public List<string> GetGameExecutables(SteamGame game)
        {
            List<string> executables = new List<string>();

            foreach (string library in GetSteamLibraries())
            {
                string gamePath = Path.Combine(
                    library,
                    "steamapps",
                    "common",
                    game.InstallDir);

                if (!Directory.Exists(gamePath))
                {
                    continue;
                }

                try
                {
                    executables.AddRange(
                        Directory.GetFiles(
                            gamePath,
                            "*.exe",
                            SearchOption.AllDirectories));
                }
                catch (UnauthorizedAccessException)
                {
                    // Skip folders we cannot access.
                }
            }

            return executables;
        }
        public string? GetLikelyGameExecutable(SteamGame game)
        {
            List<string> executables = GetGameExecutables(game);

            if (executables.Count == 0)
            {
                return null;
            }

            string[] ignoredNames =
            {
        "launcher",
        "crash",
        "uninstall",
        "updater",
        "update",
        "setup",
        "installer"
    };

            foreach (string executable in executables)
            {
                string fileName =
                    Path.GetFileNameWithoutExtension(executable);

                bool shouldIgnore = false;

                foreach (string ignoredName in ignoredNames)
                {
                    if (fileName.Contains(
                            ignoredName,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        shouldIgnore = true;
                        break;
                    }
                }

                if (!shouldIgnore)
                {
                    return executable;
                }
            }

            return executables[0];
        }

        public bool LaunchGame(string appId)
        {
            if (string.IsNullOrWhiteSpace(appId))
            {
                return false;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = $"steam://run/{appId}",
                    UseShellExecute = true
                });

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}