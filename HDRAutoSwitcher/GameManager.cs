using System;
using System.Diagnostics;

namespace HDRAutoSwitcher
{
    internal class GameManager
    {
        private readonly HDRController hdrController;
        private readonly GameLauncher gameLauncher;

        public GameManager()
        {
            hdrController = new HDRController();
            gameLauncher = new GameLauncher();
        }

        public void LaunchGame(GameProfile profile)
        {
            Debug.WriteLine($"Preparing to launch: {profile.Name}");
            Debug.WriteLine("");

            // Remember the HDR state before doing anything.
            bool wasHDRAlreadyEnabled = hdrController.IsHDREnabled();

            Debug.WriteLine(
                $"HDR before launch: {(wasHDRAlreadyEnabled ? "ON" : "OFF")}");

            // Enable HDR if the game requires it.
            if (profile.RequiresHDR && !wasHDRAlreadyEnabled)
            {
                Debug.WriteLine("Game requires HDR. Enabling HDR...");

                if (!hdrController.EnableHDR())
                {
                    Debug.WriteLine("Failed to enable HDR.");
                    return;
                }
            }

            Debug.WriteLine("");
            Debug.WriteLine("Launching game...");

            // Launch and wait for the game to exit.
            gameLauncher.Launch(profile);

            // Restore the original HDR state.
            bool isHDREnabled = hdrController.IsHDREnabled();

            if (wasHDRAlreadyEnabled != isHDREnabled)
            {
                Debug.WriteLine("");
                Debug.WriteLine("Restoring previous HDR state...");

                hdrController.SetHDRState(wasHDRAlreadyEnabled);
            }

            Debug.WriteLine("");
            Debug.WriteLine("Game session finished.");
        }
    }
}