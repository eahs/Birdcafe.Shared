
using BirdCafe.Shared.Engine.Utils;
using BirdCafe.Shared.Enums;
using BirdCafe.Shared.Models.Simulation;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace BirdCafe.Shared.Engine.Managers
{
    /// <summary>
    /// Manages global game state, save/load operations, and session initialization.
    /// </summary>
    public class MetaManager
    {
        /// <summary>
        /// Reference to the main controller.
        /// </summary>
        private readonly BirdCafeController _controller;

        /// <summary>
        /// Initializes a new instance of the <see cref="MetaManager"/> class.
        /// </summary>
        /// <param name="controller">The main game controller.</param>
        public MetaManager(BirdCafeController controller)
        {
            _controller = controller;
        }

        /// <summary>
        /// Initializes a brand new game session with default values.
        /// </summary>
        /// <param name="playerName">Name of the player.</param>
        /// <param name="cafeName">Name of the cafe.</param>
        /// <returns>Success result.</returns>
        public EngineResult StartNewGame(string playerName, string cafeName)
        {
            var save = new GameSave();

            // Set up player profile data.
            save.Profile.DisplayName = playerName;
            save.Cafe.CafeName = cafeName;

            // Set starting money from the profile defaults.
            save.Economy.CurrentBalance = save.Profile.StartingFunds;

            // Set initial calendar date.
            save.CurrentDayNumber = 1;
            save.CurrentDayName = DayOfWeek.Monday;
            save.CurrentWeekNumber = 1;

            // Create the first bird using the factory utility.
            var starterBird = BirdFactory.CreateStarterBird();
            save.Birds.Add(starterBird);

            // Give the player some starting coffee inventory.
            save.Cafe.Inventory.Coffee.QuantityOnHand = save.Config.DefaultDay1Coffee;

            // Create the plan for Day 1 immediately so the simulation can run.
            // We generate a random seed here so the first day's random events are fixed.
            var r = new Random();
            save.CurrentDayState.CurrentPlan = new DailyPlan
            {
                TargetDayNumber = 1,
                DaySeed = r.Next(),
                BirdIdsWorking = new List<string> { starterBird.Id }
            };

            // Inject the new save state into the controller.
            _controller.SetState(save);
            _controller.BirdVisualStates.EnsureRuntimeStateForAllBirds();

            // Set the game phase to DayLoop so the day intro can start.
            _controller.SetPhase(GamePhase.DayLoop);

            return EngineResult.Success();
        }

        /// <summary>
        /// Loads an existing save file into the controller.
        /// </summary>
        /// <param name="saveFile">The game save object to load.</param>
        /// <returns>Success result, or failure if the file is invalid.</returns>
        public EngineResult LoadGame(GameSave saveFile)
        {
            // Basic validation to ensure we don't load nothing.
            if (saveFile == null)
                return EngineResult.Failure("InvalidData", "Save file is null.");

            // Replace the current state.
            _controller.SetState(saveFile);
            _controller.BirdVisualStates.EnsureRuntimeStateForAllBirds();

            // For simplicity, we always resume at the start of the DayLoop phase.
            _controller.SetPhase(GamePhase.DayLoop);

            return EngineResult.Success();
        }

        /// <summary>
        /// Mock method to return available save slots.
        /// Currently returns an empty list as saving to disk is not implemented.
        /// </summary>
        /// <returns>A list of save slot view models.</returns>
        public List<ViewModels.SaveSlotViewModel> GetAvailableSaves()
        {
            List<ViewModels.SaveSlotViewModel> slots = new List<ViewModels.SaveSlotViewModel>();

            return slots;
        }
    }
}
