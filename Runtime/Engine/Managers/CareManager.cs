
using System;
using System.Linq;
using BirdCafe.Shared.Enums;
using BirdCafe.Shared.Models.Birds;
using BirdCafe.Shared.Models.Economy;

namespace BirdCafe.Shared.Engine.Managers
{
    public class CareManager
    {
        private readonly BirdCafeController _controller;

        public CareManager(BirdCafeController controller)
        {
            _controller = controller;
        }

        public EngineResult PerformCareAction(string birdId, string actionId)
        {
            if (_controller.CurrentPhase != GamePhase.EveningLoop)
                return EngineResult.Failure("InvalidPhase", "Can only care for birds in the Evening.");

            var bird = _controller.CurrentState.Birds.FirstOrDefault(b => b.Id == birdId);
            if (bird == null) return EngineResult.Failure("BirdNotFound", "Bird ID not found.");

            // Simplified lookup - in real app, use a config dictionary
            var template = GetTemplate(actionId, _controller.CurrentState.Config);
            if (template == null) return EngineResult.Failure("InvalidAction", "Unknown care action.");

            if (_controller.CurrentState.Economy.CurrentBalance < template.MoneyCost)
                return EngineResult.Failure("InsufficientFunds", "Not enough money.");

            // Execute
            _controller.CurrentState.Economy.CurrentBalance -= template.MoneyCost;
            _controller.CurrentState.Economy.Ledger.Add(new LedgerEntry
            {
                Amount = -template.MoneyCost,
                Reason = template.DisplayName,
                Timestamp = DateTime.Now,
                Category = ExpenseCategory.FoodAndSupplies, // simplified
                RelatedBirdId = bird.Id
            });

            // Apply Stats
            bird.Hunger = Math.Min(100, bird.Hunger + template.HungerChange);
            bird.Mood = Math.Min(100, bird.Mood + template.MoodChange);
            bird.Health = Math.Min(100, bird.Health + template.HealthChange);
            bird.Energy = Math.Min(100, bird.Energy + template.EnergyChange);

            return EngineResult.Success(bird);
        }

        public EngineResult ToggleRest(string birdId)
        {
            if (_controller.CurrentPhase != GamePhase.EveningLoop)
                return EngineResult.Failure("InvalidPhase", "Wrong phase.");

            var bird = _controller.CurrentState.Birds.FirstOrDefault(b => b.Id == birdId);
            if (bird == null) return EngineResult.Failure("BirdNotFound", "Bird ID not found.");

            bird.AssignedDayOffNextDay = !bird.AssignedDayOffNextDay;
            return EngineResult.Success(bird);
        }

        private CareActionTemplate GetTemplate(string id, Models.Meta.GameConfiguration config)
        {
            // Hardcoded for engine example, ideally loaded from config
            if (id == "Feed") return new CareActionTemplate { ActionId="Feed", DisplayName="Feed", MoneyCost=config.BaselineBirdFoodCost, HungerChange=30, MoodChange=5 };
            if (id == "Vet") return new CareActionTemplate { ActionId="Vet", DisplayName="Vet Visit", MoneyCost=config.BaselineVetCost, HealthChange=50, StressChange=-20 };
            return null;
        }
    }
}