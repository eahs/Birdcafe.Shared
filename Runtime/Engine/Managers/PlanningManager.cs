using BirdCafe.Shared.Engine.Utils;
using BirdCafe.Shared.Enums;
using BirdCafe.Shared.Models.Economy;
using BirdCafe.Shared.Models.Simulation;
using System;
using System.Collections.Generic;

namespace BirdCafe.Shared.Engine.Managers
{
    /// <summary>
    /// Handles the "Evening Planning" phase: buying stock and setting rosters.
    /// </summary>
    public class PlanningManager
    {
        /// <summary>
        /// Reference to the main controller.
        /// </summary>
        private readonly BirdCafeController _controller;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanningManager"/> class.
        /// </summary>
        /// <param name="controller">The main game controller.</param>
        public PlanningManager(BirdCafeController controller)
        {
            _controller = controller;
        }

        /// <summary>
        /// Updates the planned purchase quantity for a specific item.
        /// </summary>
        /// <param name="type">The product type (Coffee, Baked Goods, etc.).</param>
        /// <param name="quantity">The amount to buy.</param>
        /// <returns>Success result.</returns>
        public EngineResult SetInventoryOrder(ProductType type, int quantity)
        {
            if (_controller.CurrentPhase != GamePhase.EveningLoop)
                return EngineResult.Failure("InvalidPhase", "Wrong phase.");

            var plan = _controller.CurrentState.CurrentDayState.CurrentPlan;

            switch (type)
            {
                case ProductType.Coffee: plan.PlannedCoffeePurchase = quantity; break;
                case ProductType.BakedGoods: plan.PlannedBakedGoodsPurchase = quantity; break;
                case ProductType.ThemedMerch: plan.PlannedThemedMerchPurchase = quantity; break;
            }

            return EngineResult.Success();
        }

        
        /// <summary>
        /// Toggles a bird's working status in the daily roster.
        /// </summary>
        public EngineResult SetStaffRoster(string birdId, bool isWorking)
        {
            if (_controller.CurrentPhase != GamePhase.EveningLoop)
                return EngineResult.Failure("InvalidPhase", "Wrong phase.");

            var plan = _controller.CurrentState.CurrentDayState.CurrentPlan;

            if (isWorking && !plan.BirdIdsWorking.Contains(birdId))
            {
                plan.BirdIdsWorking.Add(birdId);
                plan.BirdIdsResting.Remove(birdId);
            }
            else if (!isWorking && !plan.BirdIdsResting.Contains(birdId))
            {
                plan.BirdIdsResting.Add(birdId);
                plan.BirdIdsWorking.Remove(birdId);
            }

            return EngineResult.Success();
        }

        /// <summary>
        /// Commits the plan, pays for inventory, and advances the calendar.
        /// </summary>
        public EngineResult FinalizeDay()
        {
            if (_controller.CurrentPhase != GamePhase.EveningLoop)
                return EngineResult.Failure("InvalidPhase", "Wrong phase.");

            var state = _controller.CurrentState;
            var plan = state.CurrentDayState.CurrentPlan;

            decimal totalCost = EconomyHelper.CalculateTotalPlanCost(
                plan.PlannedCoffeePurchase,
                plan.PlannedBakedGoodsPurchase,
                plan.PlannedThemedMerchPurchase);

            if (state.Economy.CurrentBalance < totalCost)
                return EngineResult.Failure("InsufficientFunds", "Cannot afford inventory order.");

            ProcessRestockPayment(state, plan);
            AdvanceCalendar(state);
            PrepareNextDayPlan(state, plan);
            TransitionPhase(state);

            return EngineResult.Success();
        }

        private void ProcessRestockPayment(GameSave state, DailyPlan plan)
        {
            decimal coffeeCost = EconomyHelper.CalculateRestockCost(ProductType.Coffee, plan.PlannedCoffeePurchase);
            decimal bakedGoodsCost = EconomyHelper.CalculateRestockCost(ProductType.BakedGoods, plan.PlannedBakedGoodsPurchase);
            decimal merchCost = EconomyHelper.CalculateRestockCost(ProductType.ThemedMerch, plan.PlannedThemedMerchPurchase);
            decimal totalCost = coffeeCost + bakedGoodsCost + merchCost;

            state.Economy.CurrentBalance -= totalCost;
            state.Cafe.Inventory.Coffee.QuantityOnHand += plan.PlannedCoffeePurchase;
            state.Cafe.Inventory.BakedGoods.QuantityOnHand += plan.PlannedBakedGoodsPurchase;
            state.Cafe.Inventory.ThemedMerch.QuantityOnHand += plan.PlannedThemedMerchPurchase;

            AddInventoryLedgerEntry(state, ProductType.Coffee, plan.PlannedCoffeePurchase, coffeeCost, ExpenseCategory.InventoryCoffee, "Restocked coffee beans");
            AddInventoryLedgerEntry(state, ProductType.BakedGoods, plan.PlannedBakedGoodsPurchase, bakedGoodsCost, ExpenseCategory.InventoryBakedGoods, "Restocked baked goods");
            AddInventoryLedgerEntry(state, ProductType.ThemedMerch, plan.PlannedThemedMerchPurchase, merchCost, ExpenseCategory.InventoryThemedMerch, "Restocked themed merch");
        }

        private void AddInventoryLedgerEntry(GameSave state, ProductType productType, int quantity, decimal cost, ExpenseCategory category, string description)
        {
            if (quantity <= 0 || cost <= 0)
            {
                return;
            }

            state.Economy.Ledger.Add(new LedgerEntry
            {
                DayNumber = state.CurrentDayNumber,
                WeekNumber = state.CurrentWeekNumber,
                Amount = -cost,
                Quantity = quantity,
                Reason = $"Inventory Restock: {productType}",
                Timestamp = DateTime.Now,
                Category = category,
                RelatedProduct = productType,
                ShortDescription = $"{description} x{quantity}"
            });
        }

        private void AdvanceCalendar(GameSave state)
        {
            state.CurrentDayNumber++;
            var nextDay = state.CurrentDayName + 1;

            if ((int)nextDay > 6) nextDay = DayOfWeek.Sunday;

            state.CurrentDayName = nextDay;

            if (state.CurrentDayName == DayOfWeek.Sunday)
                state.CurrentWeekNumber++;
        }

        private void PrepareNextDayPlan(GameSave state, DailyPlan previousPlan)
        {
            var r = new Random();
            state.CurrentDayState.CurrentPlan = new DailyPlan
            {
                TargetDayNumber = state.CurrentDayNumber,
                DaySeed = r.Next(),
                BirdIdsWorking = new List<string>(previousPlan.BirdIdsWorking)
            };
        }

        private void TransitionPhase(GameSave state)
        {
           /* if (state.CurrentDayName == DayOfWeek.Sunday && state.CurrentDayNumber > 1)
            {
                _controller.SetPhase(GamePhase.Reporting);
            }
            else
            { */
                _controller.SetPhase(GamePhase.DayLoop);
            //}
        }
    }
}
