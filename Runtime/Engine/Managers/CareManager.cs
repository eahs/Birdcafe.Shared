
using System;
using System.Linq;
using BirdCafe.Shared.Enums;
using BirdCafe.Shared.Models.Birds;
using BirdCafe.Shared.Models.Economy;
using BirdCafe.Shared.Models.Meta;

namespace BirdCafe.Shared.Engine.Managers
{
    /// <summary>
    /// Handles interactions with birds during the Evening Loop (Feeding, Vet, etc.).
    /// </summary>
    public class CareManager
    {
        private readonly BirdCafeController _controller;

        public CareManager(BirdCafeController controller)
        {
            _controller = controller;
        }

        /// <summary>
        /// Executes a care action (e.g. "Feed") on a specific bird.
        /// </summary>
        public EngineResult PerformCareAction(string birdId, string actionId)
        {
            if (_controller.CurrentPhase != GamePhase.EveningLoop)
                return EngineResult.Failure("InvalidPhase", "Can only care for birds in the Evening.");

            var bird = _controller.CurrentState.Birds.FirstOrDefault(b => b.Id == birdId);
            if (bird == null) return EngineResult.Failure("BirdNotFound", "Bird ID not found.");

            // 1. Get Template (Refactored to use Constants)
            var template = GetTemplate(actionId, _controller.CurrentState.Config);
            if (template == null) return EngineResult.Failure("InvalidAction", "Unknown care action.");

            // 2. Check Funds
            if (_controller.CurrentState.Economy.CurrentBalance < template.MoneyCost)
                return EngineResult.Failure("InsufficientFunds", "Not enough money.");

            // 3. Deduct Money
            if (template.MoneyCost > 0)
            {
                _controller.CurrentState.Economy.CurrentBalance -= template.MoneyCost;
                _controller.CurrentState.Economy.Ledger.Add(new LedgerEntry
                {
                    Amount = -template.MoneyCost,
                    Reason = template.DisplayName,
                    Timestamp = DateTime.Now,
                    Category = ExpenseCategory.FoodAndSupplies,
                    RelatedBirdId = bird.Id
                });
            }

            // 4. Apply Stats (Refactored: Moved logic to Bird class for safety)
            bird.ApplyCareEffect(template);

            return EngineResult.Success(bird);
        }

        /// <summary>
        /// Toggles whether a bird is flagged to rest (take a day off) tomorrow.
        /// </summary>
        public EngineResult ToggleRest(string birdId)
        {
            if (_controller.CurrentPhase != GamePhase.EveningLoop)
                return EngineResult.Failure("InvalidPhase", "Wrong phase.");

            var bird = _controller.CurrentState.Birds.FirstOrDefault(b => b.Id == birdId);
            if (bird == null) return EngineResult.Failure("BirdNotFound", "Bird ID not found.");

            bird.AssignedDayOffNextDay = !bird.AssignedDayOffNextDay;
            return EngineResult.Success(bird);
        }

        /// <summary>
        /// Look up the details/costs for an action ID.
        /// </summary>
        private CareActionTemplate GetTemplate(string id, GameConfiguration config)
        {
            // Refactor: Use Constants instead of Magic Strings
            if (id == CareActionIds.Feed) 
                return new CareActionTemplate 
                { 
                    ActionId = CareActionIds.Feed, 
                    DisplayName = "Feed", 
                    MoneyCost = config.BaselineBirdFoodCost, 
                    HungerChange = 30, 
                    MoodChange = 5 
                };
            
            if (id == CareActionIds.Vet) 
                return new CareActionTemplate 
                { 
                    ActionId = CareActionIds.Vet, 
                    DisplayName = "Vet Visit", 
                    MoneyCost = config.BaselineVetCost, 
                    HealthChange = 50, 
                    StressChange = -20 
                };

            if (id == CareActionIds.Play)
                return new CareActionTemplate
                {
                    ActionId = CareActionIds.Play,
                    DisplayName = "Play Time",
                    MoneyCost = config.BaselinePlayCost,
                    MoodChange = 20, // Play improves mood significantly
                    EnergyChange = -5, // Play tires them out a little
                    StressChange = -10
                };
                
            return null;
        }
    }
}