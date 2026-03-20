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
            if (bird == null)
                return EngineResult.Failure("BirdNotFound", "Bird ID not found.");

            var template = GetTemplate(actionId, _controller.CurrentState.Config);
            if (template == null)
                return EngineResult.Failure("InvalidAction", "Unknown care action.");

            if (actionId == CareActionIds.Feed)
            {
                return PerformFeedAction(bird, template);
            }

            var state = _controller.CurrentState;
            if (state.Economy.CurrentBalance < template.MoneyCost)
                return EngineResult.Failure("InsufficientFunds", "Not enough money.");

            if (template.MoneyCost > 0)
            {
                state.Economy.CurrentBalance -= template.MoneyCost;
                state.Economy.Ledger.Add(new LedgerEntry
                {
                    DayNumber = state.CurrentDayNumber,
                    WeekNumber = state.CurrentWeekNumber,
                    Amount = -template.MoneyCost,
                    Reason = template.DisplayName,
                    Timestamp = DateTime.Now,
                    Category = GetExpenseCategoryForCareAction(actionId),
                    RelatedBirdId = bird.Id,
                    ShortDescription = $"{template.DisplayName} for {bird.Name}"
                });
            }

            bird.ApplyCareEffect(template);

            if (actionId == CareActionIds.Vet)
            {
                bird.IsSick = false;
                bird.IsSeverelySick = false;
            }

            return EngineResult.Success(bird);
        }

        public EngineResult ToggleRest(string birdId)
        {
            if (_controller.CurrentPhase != GamePhase.EveningLoop)
                return EngineResult.Failure("InvalidPhase", "Wrong phase.");

            var bird = _controller.CurrentState.Birds.FirstOrDefault(b => b.Id == birdId);
            if (bird == null)
                return EngineResult.Failure("BirdNotFound", "Bird ID not found.");

            bird.AssignedDayOffNextDay = !bird.AssignedDayOffNextDay;

            return EngineResult.Success(bird);
        }

        private EngineResult PerformFeedAction(Bird bird, CareActionTemplate template)
        {
            var store = _controller.CurrentState.PetStore;
            if (store.GetTotalFoodUnits() <= 0)
                return EngineResult.Failure("NoStoredFood", "No bird food in storage. Buy food at Pete's Pet Store first.");

            var selectedFoodType = SelectFoodTypeForBird(bird, store);
            if (selectedFoodType == null)
                return EngineResult.Failure("NoStoredFood", "No bird food in storage. Buy food at Pete's Pet Store first.");

            if (!store.TryConsumeFood(selectedFoodType.Value, 1))
                return EngineResult.Failure("NoStoredFood", "No bird food in storage. Buy food at Pete's Pet Store first.");

            bird.ApplyCareEffect(template);

            float trustGain = bird.PrefersFood(selectedFoodType.Value) ? 10f : 2f;
            bird.IncreaseTrust(trustGain);

            return EngineResult.Success(bird);
        }

        private ExpenseCategory GetExpenseCategoryForCareAction(string actionId)
        {
            switch (actionId)
            {
                case CareActionIds.Vet:
                    return ExpenseCategory.VetCare;
                case CareActionIds.Play:
                    return ExpenseCategory.ToysAndActivities;
                default:
                    return ExpenseCategory.FoodAndSupplies;
            }
        }

        private BirdFoodType? SelectFoodTypeForBird(Bird bird, PetStoreState store)
        {
            foreach (var preferred in bird.PreferredFoods)
            {
                if (store.GetFoodUnits(preferred) > 0)
                    return preferred;
            }

            foreach (BirdFoodType type in Enum.GetValues(typeof(BirdFoodType)))
            {
                if (store.GetFoodUnits(type) > 0)
                    return type;
            }

            return null;
        }

        private CareActionTemplate GetTemplate(string id, GameConfiguration config)
        {
            if (id == CareActionIds.Feed)
                return new CareActionTemplate
                {
                    ActionId = CareActionIds.Feed,
                    DisplayName = "Feed",
                    MoneyCost = 0,
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
                    MoodChange = 20,
                    EnergyChange = -5,
                    StressChange = -10
                };

            return null;
        }
    }
}
