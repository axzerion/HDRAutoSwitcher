using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace HDRAutoSwitcher
{
    internal class GameConfig
    {
        private readonly string configPath;

        public GameConfig()
        {
            configPath = Path.Combine(
                AppContext.BaseDirectory,
                "games.json");
        }

        public List<GameProfile> LoadGames()
        {
            if (!File.Exists(configPath))
            {
                return new List<GameProfile>();
            }

            string json = File.ReadAllText(configPath);

            return JsonSerializer.Deserialize<List<GameProfile>>(json)
                   ?? new List<GameProfile>();
        }

        public void SaveGames(List<GameProfile> games)
        {
            string json = JsonSerializer.Serialize(
                games,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            File.WriteAllText(configPath, json);
        }

        public void AddGame(GameProfile game)
        {
            List<GameProfile> games = LoadGames();

            if (!string.IsNullOrWhiteSpace(game.SteamAppId))
            {
                GameProfile? existingGame = games.Find(
                    existing =>
                        existing.SteamAppId == game.SteamAppId);

                if (existingGame != null)
                {
                    games.Remove(existingGame);
                }
            }

            games.Add(game);

            SaveGames(games);
        }

        public void RemoveGame(GameProfile game)
        {
            List<GameProfile> games = LoadGames();

            if (!string.IsNullOrWhiteSpace(game.SteamAppId))
            {
                games.RemoveAll(
                    existing =>
                        existing.SteamAppId == game.SteamAppId);
            }
            else
            {
                games.RemoveAll(
                    existing =>
                        existing.LaunchPath == game.LaunchPath);
            }

            SaveGames(games);
        }

        public void UpdateGame(GameProfile game)
        {
            List<GameProfile> games = LoadGames();

            int index;

            if (!string.IsNullOrWhiteSpace(game.SteamAppId))
            {
                index = games.FindIndex(
                    existing =>
                        existing.SteamAppId == game.SteamAppId);
            }
            else
            {
                index = games.FindIndex(
                    existing =>
                        existing.LaunchPath == game.LaunchPath);
            }

            if (index == -1)
            {
                return;
            }

            games[index] = game;

            SaveGames(games);
        }
    }
}