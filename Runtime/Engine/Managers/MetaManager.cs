
using System;
using System.Collections.Generic;
using BirdCafe.Shared.Enums;
using BirdCafe.Shared.Models.Birds;
using BirdCafe.Shared.Models.Meta;
using BirdCafe.Shared.Models.Simulation;

namespace BirdCafe.Shared.Engine.Managers
{
    public class MetaManager
    {
        private readonly BirdCafeController _controller;

        public MetaManager(BirdCafeController controller)
        {
            _controller = controller;
        }

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
            
            // 3. Create Starter Bird
            var starterBird = new Bird 
            { 
                Name = "Peep", 
                SpeciesId = "Sparrow_Standard",
                PrimaryColorHex = "#FFCC00",
                Productivity = 20,
                Energy = 100,
                Mood = 80,
                Hunger = 100
            };
            save.Birds.Add(starterBird);

            // 4. Default Inventory (Free starter stock for Day 1 tutorial feel)
            save.Cafe.Inventory.Coffee.QuantityOnHand = save.Config.DefaultDay1Coffee;

            // 5. Create Day 1 Plan immediately
            // This bypasses the evening planning phase for the first day
            var r = new Random();
            save.CurrentDayState.CurrentPlan = new DailyPlan
            {
                TargetDayNumber = 1,
                DaySeed = r.Next(),
                PlannedCoffeePurchase = 0, // Already gifted via inventory
                BirdIdsWorking = new List<string> { starterBird.Id }
            };

            // 6. Inject State
            _controller.SetState(save);
            
            // 7. Set Phase
            // We start in DayLoop so the player sees the "Day 1" banner and can run the sim.
            _controller.SetPhase(GamePhase.DayLoop);

            return EngineResult.Success();
        }

        public EngineResult LoadGame(GameSave saveFile)
        {
            if (saveFile == null)
                return EngineResult.Failure("InvalidData", "Save file is null.");

            _controller.SetState(saveFile);
            
            // Determine phase based on save state.
            // For simplicity in this engine, we always load into the start of the saved Day.
            _controller.SetPhase(GamePhase.DayLoop);
            
            return EngineResult.Success();
        }
        
        public List<ViewModels.SaveSlotViewModel> GetAvailableSaves()
        {
            // In a real implementation involving file I/O, this would scan the disk.
            // Since this is a shared POCO library, actual File I/O often happens 
            // in a platform-specific layer (Unity), but the logic to Parse headers goes here.
            
            // Returning empty list as placeholder for the shared logic.
            return new List<ViewModels.SaveSlotViewModel>();
        }
    }
}