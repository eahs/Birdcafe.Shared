using BirdCafe.Shared.Enums;
using BirdCafe.Shared.Models.Birds;
using BirdCafe.Shared.Models.Economy;
using BirdCafe.Shared.Models.Meta;
using System;
using System.Linq;

namespace BirdCafe.Shared.Engine.Managers
{
    /// <summary>
    /// Handles interactions with birds during the Evening Loop (Feeding, Vet, etc.).
    /// </summary>
    public class CareManager
    {
        /// <summary>
        /// Reference to the main controller to access state and economy.
        /// </summary>
        private readonly BirdCafeController _controller;

        /// <summary>
        /// Initializes a new instance of the <see cref="CareManager"/> class.
        /// </summary>
        /// <param name="controller">The main game controller.</param>
        public CareManager(BirdCafeController controller)
        {
            _controller = controller;
        }

        /// <summary>
        /// Executes a care action (e.g. "Feed") on a specific bird.
        /// </summary>
        /// <param name="birdId">The unique ID of the bird.</param>
        /// <param name="actionId">The ID of the action to perform (e.g., "Feed").</param>
        /// <returns>Result indicating success or failure.</returns>
        public EngineResult PerformCareAction(string birdId, string actionId)
        {
            // Verify that we are in the correct game phase (Evening).
            if (_controller.CurrentPhase != GamePhase.EveningLoop)
                return EngineResult.Failure("InvalidPhase", "Can only care for birds in the Evening.");

            // Find the bird in the list using LINQ's FirstOrDefault.
            // This searches the list for a bird where the Id matches birdId.
            var bird = _controller.CurrentState.Birds.FirstOrDefault(b => b.Id == birdId);

            // If bird is null, it means no match was found.
            if (bird == null) return EngineResult.Failure("BirdNotFound", "Bird ID not found.");

            // Look up the template for the requested action to know costs and effects.
            var template = GetTemplate(actionId, _controller.CurrentState.Config);
            if (template == null) return EngineResult.Failure("InvalidAction", "Unknown care action.");

            // Check if the player has enough money.
            if (_controller.CurrentState.Economy.CurrentBalance < template.MoneyCost)
                return EngineResult.Failure("InsufficientFunds", "Not enough money.");

            // If the action costs money, process the payment.
            if (template.MoneyCost > 0)
            {
                // Subtract cost from balance.
                _controller.CurrentState.Economy.CurrentBalance -= template.MoneyCost;

                // Record the transaction in the ledger for history.
                _controller.CurrentState.Economy.Ledger.Add(new LedgerEntry
                {
                    Amount = -template.MoneyCost,
                    Reason = template.DisplayName,
                    Timestamp = DateTime.Now,
                    Category = ExpenseCategory.FoodAndSupplies,
                    RelatedBirdId = bird.Id
                });
            }

            // Apply the statistical changes (Health, Mood, etc.) to the bird object.
            bird.ApplyCareEffect(template);

            // If this was a Vet visit, clear the sickness flags so the bird is healthy again.
            if (actionId == CareActionIds.Vet)
            {
                bird.IsSick = false;
                bird.IsSeverelySick = false;
            }

            return EngineResult.Success(bird);
        }

        /// <summary>
        /// Toggles whether a bird is flagged to rest (take a day off) tomorrow.
        /// </summary>
        /// <param name="birdId">The unique ID of the bird.</param>
        /// <returns>Result indicating success or failure.</returns>
        public EngineResult ToggleRest(string birdId)
        {
            // Verify phase correctness.
            if (_controller.CurrentPhase != GamePhase.EveningLoop)
                return EngineResult.Failure("InvalidPhase", "Wrong phase.");

            // Locate the bird object.
            var bird = _controller.CurrentState.Birds.FirstOrDefault(b => b.Id == birdId);
            if (bird == null) return EngineResult.Failure("BirdNotFound", "Bird ID not found.");

            // Flip the boolean value (true becomes false, false becomes true).
            bird.AssignedDayOffNextDay = !bird.AssignedDayOffNextDay;

            return EngineResult.Success(bird);
        }

        /// <summary>
        /// Look up the details/costs for an action ID based on configuration.
        /// </summary>
        /// <param name="id">The action ID.</param>
        /// <param name="config">The game configuration.</param>
        /// <returns>A template describing the action, or null if not found.</returns>
        private CareActionTemplate GetTemplate(string id, GameConfiguration config)
        {
            // Check if the ID matches the Feed constant.
            if (id == CareActionIds.Feed)
                return new CareActionTemplate
                {
                    ActionId = CareActionIds.Feed,
                    DisplayName = "Feed",
                    MoneyCost = config.BaselineBirdFoodCost,
                    HungerChange = 30,
                    MoodChange = 5
                };

            // Check if the ID matches the Vet constant.
            if (id == CareActionIds.Vet)
                return new CareActionTemplate
                {
                    ActionId = CareActionIds.Vet,
                    DisplayName = "Vet Visit",
                    MoneyCost = config.BaselineVetCost,
                    HealthChange = 50,
                    StressChange = -20
                };

            // Check if the ID matches the Play constant.
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