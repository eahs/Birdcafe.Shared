
using System;
using System.Collections.Generic;
using System.Linq;
using BirdCafe.Shared.Enums;
using BirdCafe.Shared.Models.Simulation;
using BirdCafe.Shared.Models.Economy;

namespace BirdCafe.Shared.Engine.Managers
{
    public class PlanningManager
    {
        private readonly BirdCafeController _controller;

        public PlanningManager(BirdCafeController controller)
        {
            _controller = controller;
        }

        public EngineResult SetInventoryOrder(ProductType type, int quantity)
        {
            if (_controller.CurrentPhase != GamePhase.EveningLoop)
                return EngineResult.Failure("InvalidPhase", "Wrong phase.");

            var plan = _controller.CurrentState.CurrentDayState.CurrentPlan;
            if (type == ProductType.Coffee) plan.PlannedCoffeePurchase = quantity;
            else if (type == ProductType.BakedGoods) plan.PlannedBakedGoodsPurchase = quantity;
            else if (type == ProductType.ThemedMerch) plan.PlannedThemedMerchPurchase = quantity;

            return EngineResult.Success();
        }

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

        public EngineResult FinalizeDay()
        {
            if (_controller.CurrentPhase != GamePhase.EveningLoop)
                return EngineResult.Failure("InvalidPhase", "Wrong phase.");

            var state = _controller.CurrentState;
            var plan = state.CurrentDayState.CurrentPlan;

            // 1. Calculate Costs
            // (In real app, get unit costs from config)
            decimal costCoffee = plan.PlannedCoffeePurchase * 1.0m; 
            decimal costBaked = plan.PlannedBakedGoodsPurchase * 2.0m;
            decimal costMerch = plan.PlannedThemedMerchPurchase * 8.0m;
            decimal totalCost = costCoffee + costBaked + costMerch;

            if (state.Economy.CurrentBalance < totalCost)
                return EngineResult.Failure("InsufficientFunds", "Cannot afford inventory order.");

            // 2. Execute Purchase
            state.Economy.CurrentBalance -= totalCost;
            
            // Add Inventory
            state.Cafe.Inventory.Coffee.QuantityOnHand += plan.PlannedCoffeePurchase;
            state.Cafe.Inventory.BakedGoods.QuantityOnHand += plan.PlannedBakedGoodsPurchase;
            state.Cafe.Inventory.ThemedMerch.QuantityOnHand += plan.PlannedThemedMerchPurchase;

            // Log Expense
            state.Economy.Ledger.Add(new LedgerEntry 
            { 
                Amount = -totalCost, 
                Reason = "Inventory Restock", 
                Timestamp = DateTime.Now,
                Category = ExpenseCategory.Miscellaneous // Should split in real app
            });

            // 3. Advance Day
            state.CurrentDayNumber++;
            var nextDay = state.CurrentDayName + 1;
            if ((int)nextDay > 6) nextDay = DayOfWeek.Sunday;
            state.CurrentDayName = nextDay;

            if (state.CurrentDayName == DayOfWeek.Sunday)
                state.CurrentWeekNumber++;

            // 4. Create Next Plan
            var r = new Random();
            state.CurrentDayState.CurrentPlan = new DailyPlan
            {
                TargetDayNumber = state.CurrentDayNumber,
                DaySeed = r.Next(),
                BirdIdsWorking = new List<string>(plan.BirdIdsWorking) // Carry over roster
            };

            // 5. Phase Transition
            if (state.CurrentDayName == DayOfWeek.Sunday && state.CurrentDayNumber > 1)
            {
                _controller.SetPhase(GamePhase.Reporting); // Weekly Summary
            }
            else
            {
                _controller.SetPhase(GamePhase.DayLoop);
            }

            return EngineResult.Success();
        }
    }
}