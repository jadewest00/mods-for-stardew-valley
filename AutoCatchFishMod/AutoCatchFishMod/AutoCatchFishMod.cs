using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Tools;

namespace AutoCatchFishMod
{
    public class AutoCatchFishMod : Mod
    {
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            var player = Game1.player;
            if (player.CurrentTool is FishingRod fishingRod && fishingRod.isFishing)
            {
                if (fishingRod.isNibbling && !fishingRod.isReeling && !fishingRod.fishCaught)
                {
                    // Simulate catching the fish
                    fishingRod.isNibbling = false;
                    fishingRod.isReeling = true;
                    fishingRod.hit = true;
                    fishingRod.timeUntilFishingBite = 0;

                    // Determine the fish to catch based on game mechanics
                    float millisecondsAfterNibble = 0f; // Default value
                    string bait = fishingRod.attachments?[0]?.Name ?? "None"; // Get bait name or "None"
                    int waterDepth = fishingRod.attachments?[1]?.Stack ?? 0; // Default water depth
                    double baitPotency = 0.4; // Default bait potency
                    Vector2 bobberTile = fishingRod.bobber.Value; // Get bobber position
                    string locationName = Game1.currentLocation?.Name ?? "Unknown";

                    StardewValley.Object fishItem = Game1.currentLocation?.getFish(millisecondsAfterNibble, bait, waterDepth, player, baitPotency, bobberTile, locationName) as StardewValley.Object;

                    if (fishItem != null && fishItem.ParentSheetIndex != -1)
                    {
                        int fishSize = Game1.random.Next(1, 10); // Random fish size

                        // Determine if the fish is in the no-quality list
                        int[] noQualityItems = { 132, 133, 134, 167, 168, 169, 170, 171, 172, 344, 793, 794, 795, 796, 797, 798 }; // Include IDs for different types of jellies

                        int fishQuality = 4; // Default fish quality (normal)
                    
                        int fishDifficulty = 50; // Default fish difficulty

                        fishItem.Quality = fishQuality; // Set the fish quality

                        // Handle jelly items by their string IDs
                        string fishId = fishItem.ParentSheetIndex switch
                        {
                            132 => "RiverJelly",
                            133 => "CaveJelly",
                            134 => "SeaJelly",
                            _ => fishItem.ParentSheetIndex.ToString()
                        };

                        fishingRod.pullFishFromWater(
                            fishId,
                            fishSize,
                            fishQuality,
                            fishDifficulty,
                            false, // treasureCaught
                            false, // wasPerfect
                            false, // fromFishPond
                            null,  // setFlagOnCatch
                            false, // isBossFish
                            1      // numCaught
                        );

                        Game1.showGlobalMessage($"{fishItem.DisplayName}을(를) 잡았다!");

                        fishingRod.fishCaught = true;
                        fishingRod.DoFunction(Game1.currentLocation, (int)player.lastClick.X, (int)player.lastClick.Y, 0, player);
                    }
                }
            }
        }
    }
}
