
using System;
using System.Collections.Generic;
using BirdCafe.Shared.Enums;
using BirdCafe.Shared.Engine.Utils; // Use the new Utils
using BirdCafe.Shared.Models.Birds;
using BirdCafe.Shared.Models.Meta;
using BirdCafe.Shared.Models.Simulation;

namespace BirdCafe.Shared.Engine.Managers
{
    /// <summary>
    /// Manages global game state, save/load, and initialization.
    /// </summary>
    public class MetaManager
    {
        private readonly BirdCafeController _controller;

        /// <summary>
        /// Constructor injecting the main controller.
        /// </summary>
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
            
            // 1. Setup Profile
            save.Profile.DisplayName = playerName;
            save.Cafe.CafeName = cafeName;
            save.Economy.CurrentBalance = save.Profile.StartingFunds;
            
            // 2. Setup Time
            save.CurrentDayNumber = 1;
            save.CurrentDayName = DayOfWeek.Monday;
            save.CurrentWeekNumber = 1;
            
            // 3. Create Starter Bird using Factory (Refactor: No hardcoding here)
            var starterBird = BirdFactory.CreateStarterBird();
            save.Birds.Add(starterBird);

            // 4. Default Inventory
            save.Cafe.Inventory.Coffee.QuantityOnHand = save.Config.DefaultDay1Coffee;

            // 5. Create Day 1 Plan immediately
            // We generate a random seed here so the first day is deterministic if reloaded.
            var r = new Random();
            save.CurrentDayState.CurrentPlan = new DailyPlan
            {
                TargetDayNumber = 1,
                DaySeed = r.Next(),
                BirdIdsWorking = new List<string> { starterBird.Id }
            };

            // 6. Inject State into Controller
            _controller.SetState(save);
            _controller.SetPhase(GamePhase.DayLoop);

            return EngineResult.Success();
        }

        /// <summary>
        /// Loads an existing save file into the controller.
        /// </summary>
        public EngineResult LoadGame(GameSave saveFile)
        {
            if (saveFile == null)
                return EngineResult.Failure("InvalidData", "Save file is null.");

            _controller.SetState(saveFile);
            
            // For simplicity, we always resume at the start of the DayLoop.
            _controller.SetPhase(GamePhase.DayLoop);
            
            return EngineResult.Success();
        }
        
        /// <summary>
        /// Mock method to return available save slots.
        /// </summary>
        public List<ViewModels.SaveSlotViewModel> GetAvailableSaves()
        {
            return new List<ViewModels.SaveSlotViewModel>();
        }
    }
}