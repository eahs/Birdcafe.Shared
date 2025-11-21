
using System;
using System.Collections.Generic;
using System.Linq;
using BirdCafe.Shared.Enums;
using BirdCafe.Shared.Engine.Utils; // Use shared math
using BirdCafe.Shared.Models.Simulation;
using BirdCafe.Shared.Models.Economy;

namespace BirdCafe.Shared.Engine.Managers
{
    /// <summary>
    /// Handles the "Evening Planning" phase: buying stock and setting rosters.
    /// </summary>
    public class PlanningManager
    {
        private readonly BirdCafeController _controller;

        public PlanningManager(BirdCafeController controller)
        {
            _controller = controller;
        }

        /// <summary>
        /// Updates the planned purchase quantity for a specific item.
        /// </summary>
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
        /// Refactored to be a sequence of clear steps.
        /// </summary>
        public EngineResult FinalizeDay()
        {
            if (_controller.CurrentPhase != GamePhase.EveningLoop)
                return EngineResult.Failure("InvalidPhase", "Wrong phase.");

            var state = _controller.CurrentState;
            var plan = state.CurrentDayState.CurrentPlan;

            // 1. Calculate & Validate Costs (Using Shared Helper)
            decimal totalCost = EconomyHelper.CalculateTotalPlanCost(
                plan.PlannedCoffeePurchase, 
                plan.PlannedBakedGoodsPurchase, 
                plan.PlannedThemedMerchPurchase
            );

            if (state.Economy.CurrentBalance < totalCost)
                return EngineResult.Failure("InsufficientFunds", "Cannot afford inventory order.");

            // 2. Execute Financial Transaction
            ProcessRestockPayment(state, totalCost, plan);

            // 3. Advance Calendar
            AdvanceCalendar(state);

            // 4. Prepare Next Day's Plan Object
            PrepareNextDayPlan(state, plan);

            // 5. Determine Next Game Phase
            TransitionPhase(state);

            return EngineResult.Success();
        }

        // --- Private Helpers for Readability ---

        private void ProcessRestockPayment(GameSave state, decimal cost, DailyPlan plan)
        {
            // Deduct money
            state.Economy.CurrentBalance -= cost;
            
            // Add physical items
            state.Cafe.Inventory.Coffee.QuantityOnHand += plan.PlannedCoffeePurchase;
            state.Cafe.Inventory.BakedGoods.QuantityOnHand += plan.PlannedBakedGoodsPurchase;
            state.Cafe.Inventory.ThemedMerch.QuantityOnHand += plan.PlannedThemedMerchPurchase;

            // Log to Ledger
            state.Economy.Ledger.Add(new LedgerEntry 
            { 
                Amount = -cost, 
                Reason = "Inventory Restock", 
                Timestamp = DateTime.Now,
                Category = ExpenseCategory.Miscellaneous 
            });
        }

        private void AdvanceCalendar(GameSave state)
        {
            state.CurrentDayNumber++;
            var nextDay = state.CurrentDayName + 1;
            
            // Wrap around Sunday -> Monday
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
                // Quality of Life: Automatically carry over the roster from yesterday
                BirdIdsWorking = new List<string>(previousPlan.BirdIdsWorking) 
            };
        }

        private void TransitionPhase(GameSave state)
        {
            // If it's Sunday (and not the very first day), we do a Weekly Report.
            if (state.CurrentDayName == DayOfWeek.Sunday && state.CurrentDayNumber > 1)
            {
                _controller.SetPhase(GamePhase.Reporting);
            }
            else
            {
                _controller.SetPhase(GamePhase.DayLoop);
            }
        }
    }
}