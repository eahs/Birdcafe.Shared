
using BirdCafe.Shared.Engine.Utils;
using BirdCafe.Shared.Enums;
using BirdCafe.Shared.Models.Economy;
using BirdCafe.Shared.Models.PetStore;
using BirdCafe.Shared.Models.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;

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
            // Ensure we are in the correct phase.
            if (_controller.CurrentPhase != GamePhase.EveningLoop)
                return EngineResult.Failure("InvalidPhase", "Wrong phase.");

            // Access the current daily plan object.
            var plan = _controller.CurrentState.CurrentDayState.CurrentPlan;

            // Update the correct property based on the product type.
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
        /// <param name="birdId">The unique ID of the bird.</param>
        /// <param name="isWorking">True if the bird should work; false to rest.</param>
        /// <returns>Success result.</returns>
        public EngineResult SetStaffRoster(string birdId, bool isWorking)
        {
            if (_controller.CurrentPhase != GamePhase.EveningLoop)
                return EngineResult.Failure("InvalidPhase", "Wrong phase.");

            var plan = _controller.CurrentState.CurrentDayState.CurrentPlan;

            // If setting to work, add to working list and remove from resting list.
            if (isWorking && !plan.BirdIdsWorking.Contains(birdId))
            {
                plan.BirdIdsWorking.Add(birdId);
                plan.BirdIdsResting.Remove(birdId);
            }
            // If setting to rest, add to resting list and remove from working list.
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
        /// <returns>Success if funds allow and day is advanced.</returns>
        public EngineResult FinalizeDay()
        {
            if (_controller.CurrentPhase != GamePhase.EveningLoop)
                return EngineResult.Failure("InvalidPhase", "Wrong phase.");

            var state = _controller.CurrentState;
            var plan = state.CurrentDayState.CurrentPlan;

            // Calculate & Validate Costs (Using Shared Helper).
            decimal totalCost = EconomyHelper.CalculateTotalPlanCost(
                plan.PlannedCoffeePurchase,
                plan.PlannedBakedGoodsPurchase,
                plan.PlannedThemedMerchPurchase
            );

            // Check if player has enough money.
            if (state.Economy.CurrentBalance < totalCost)
                return EngineResult.Failure("InsufficientFunds", "Cannot afford inventory order.");

            // Process the payment and update stock.
            ProcessRestockPayment(state, totalCost, plan);

            // Advance the date to the next day.
            AdvanceCalendar(state);

            // Create the empty plan object for the new day.
            PrepareNextDayPlan(state, plan);

            // Decide which game phase comes next (Reporting or DayLoop).
            TransitionPhase(state);

            return EngineResult.Success();
        }

        /// <summary>
        /// Purchases an entertainer bird from Rick's Pet Store.
        /// </summary>
        public EngineResult PurchaseEntertainerBird(string birdId)
        {
            if (_controller.CurrentPhase != GamePhase.EveningLoop)
                return EngineResult.Failure("InvalidPhase", "Rick's Pet Store is only open during evening.");

            var state = _controller.CurrentState;
            var definition = PetStoreCatalog.EntertainerBirds.FirstOrDefault(b => b.BirdId == birdId);
            if (definition == null)
                return EngineResult.Failure("InvalidPetBird", "That bird is not sold in Rick's Pet Store.");

            if (state.PetStore.OwnedEntertainerBirdIds.Contains(birdId))
                return EngineResult.Failure("AlreadyOwned", "You already own this entertainer bird.");

            if (state.Economy.CurrentBalance < definition.Price)
                return EngineResult.Failure("InsufficientFunds", "Not enough money for that bird.");

            state.Economy.CurrentBalance -= definition.Price;
            state.PetStore.OwnedEntertainerBirdIds.Add(birdId);
            state.Economy.Ledger.Add(new LedgerEntry
            {
                Amount = -definition.Price,
                Reason = $"Rick's Pet Store Bird: {definition.SpeciesName}",
                ShortDescription = definition.EffectDescription,
                Timestamp = DateTime.Now,
                Category = ExpenseCategory.ToysAndActivities
            });

            return EngineResult.Success(definition);
        }

        /// <summary>
        /// Purchases a store supply item.
        /// </summary>
        public EngineResult PurchaseSupply(PetStoreSupplyType supplyType)
        {
            if (_controller.CurrentPhase != GamePhase.EveningLoop)
                return EngineResult.Failure("InvalidPhase", "Rick's Pet Store is only open during evening.");

            if (supplyType == PetStoreSupplyType.MysteryEgg)
                return PurchaseMysteryEgg();

            var state = _controller.CurrentState;
            var def = PetStoreCatalog.Supplies.FirstOrDefault(s => s.SupplyType == supplyType);
            if (def == null)
                return EngineResult.Failure("InvalidSupply", "That item is not sold in the store.");

            if (state.Economy.CurrentBalance < def.Price)
                return EngineResult.Failure("InsufficientFunds", "Not enough money for that supply.");

            state.Economy.CurrentBalance -= def.Price;
            AddSupplyToInventory(state.PetStore.Supplies, supplyType);
            state.Economy.Ledger.Add(new LedgerEntry
            {
                Amount = -def.Price,
                Reason = $"Rick's Supply: {def.DisplayName}",
                ShortDescription = def.EffectDescription,
                Timestamp = DateTime.Now,
                Category = ResolveCategoryForSupply(supplyType)
            });

            return EngineResult.Success(def);
        }

        /// <summary>
        /// Purchases and opens one mystery egg using deterministic seeded selection.
        /// </summary>
        public EngineResult PurchaseMysteryEgg()
        {
            if (_controller.CurrentPhase != GamePhase.EveningLoop)
                return EngineResult.Failure("InvalidPhase", "Rick's Pet Store is only open during evening.");

            var state = _controller.CurrentState;
            var eggDef = PetStoreCatalog.Supplies.First(s => s.SupplyType == PetStoreSupplyType.MysteryEgg);
            if (state.Economy.CurrentBalance < eggDef.Price)
                return EngineResult.Failure("InsufficientFunds", "Not enough money for a mystery egg.");

            state.Economy.CurrentBalance -= eggDef.Price;
            var reward = ResolveDeterministicEggReward(state);

            if (!state.PetStore.UnlockedEggRewardIds.Contains(reward.RewardId))
                state.PetStore.UnlockedEggRewardIds.Add(reward.RewardId);

            state.PetStore.LastUnlockedEggRewardId = reward.RewardId;
            state.PetStore.MysteryEggOpenCount++;

            state.Economy.Ledger.Add(new LedgerEntry
            {
                Amount = -eggDef.Price,
                Reason = "Rick's Supply: Mystery Egg Toy",
                ShortDescription = $"Unlocked: {reward.DisplayName}",
                Timestamp = DateTime.Now,
                Category = ExpenseCategory.UpgradesAndCustomization
            });

            return EngineResult.Success(reward);
        }

        // --- Private Helpers for Readability ---

        /// <summary>
        /// Deducts funds and adds inventory to the cafe state.
        /// </summary>
        /// <param name="state">The game state.</param>
        /// <param name="cost">The total cost.</param>
        /// <param name="plan">The plan containing purchase quantities.</param>
        private void ProcessRestockPayment(GameSave state, decimal cost, DailyPlan plan)
        {
            // Deduct money from balance.
            state.Economy.CurrentBalance -= cost;

            // Add physical items to inventory.
            state.Cafe.Inventory.Coffee.QuantityOnHand += plan.PlannedCoffeePurchase;
            state.Cafe.Inventory.BakedGoods.QuantityOnHand += plan.PlannedBakedGoodsPurchase;
            state.Cafe.Inventory.ThemedMerch.QuantityOnHand += plan.PlannedThemedMerchPurchase;

            // Log the transaction to the Ledger.
            state.Economy.Ledger.Add(new LedgerEntry
            {
                Amount = -cost,
                Reason = "Inventory Restock",
                Timestamp = DateTime.Now,
                Category = ExpenseCategory.Miscellaneous
            });
        }

        /// <summary>
        /// Increments the day and week numbers and handles Sunday rollover.
        /// </summary>
        /// <param name="state">The game state.</param>
        private void AdvanceCalendar(GameSave state)
        {
            state.CurrentDayNumber++;
            var nextDay = state.CurrentDayName + 1;

            // Wrap around Sunday -> Monday (Enum values 0-6).
            if ((int)nextDay > 6) nextDay = DayOfWeek.Sunday;

            state.CurrentDayName = nextDay;

            // If we just became Sunday, increment the week counter.
            if (state.CurrentDayName == DayOfWeek.Sunday)
                state.CurrentWeekNumber++;
        }

        /// <summary>
        /// Creates a fresh DailyPlan object for the new day.
        /// </summary>
        /// <param name="state">The game state.</param>
        /// <param name="previousPlan">The plan from the previous day (used to copy roster settings).</param>
        private void PrepareNextDayPlan(GameSave state, DailyPlan previousPlan)
        {
            var r = new Random();
            state.CurrentDayState.CurrentPlan = new DailyPlan
            {
                TargetDayNumber = state.CurrentDayNumber,
                DaySeed = r.Next(),
                // Copy the list of working birds from yesterday so the player doesn't have to re-select them every time.
                BirdIdsWorking = new List<string>(previousPlan.BirdIdsWorking)
            };
        }

        /// <summary>
        /// Determines the next phase based on the current day.
        /// </summary>
        /// <param name="state">The game state.</param>
        private void TransitionPhase(GameSave state)
        {
            // If it's Sunday (and not the very first day), we do a Weekly Report phase.
            if (state.CurrentDayName == DayOfWeek.Sunday && state.CurrentDayNumber > 1)
            {
                _controller.SetPhase(GamePhase.Reporting);
            }
            else
            {
                // Otherwise, go straight to the Day Loop.
                _controller.SetPhase(GamePhase.DayLoop);
            }
        }

        private static void AddSupplyToInventory(SupplyInventory inventory, PetStoreSupplyType supplyType)
        {
            if (supplyType == PetStoreSupplyType.BirdFood) inventory.BirdFoodCount++;
            if (supplyType == PetStoreSupplyType.Toys) inventory.ToyCount++;
            if (supplyType == PetStoreSupplyType.Costumes) inventory.CostumeCount++;
        }

        private static ExpenseCategory ResolveCategoryForSupply(PetStoreSupplyType supplyType)
        {
            if (supplyType == PetStoreSupplyType.BirdFood) return ExpenseCategory.FoodAndSupplies;
            if (supplyType == PetStoreSupplyType.Toys) return ExpenseCategory.ToysAndActivities;
            return ExpenseCategory.UpgradesAndCustomization;
        }

        private static EggRewardDefinition ResolveDeterministicEggReward(GameSave state)
        {
            var seed = state.CurrentDayState.CurrentPlan.DaySeed + (state.PetStore.MysteryEggOpenCount * 97) + (state.CurrentDayNumber * 31);
            var rng = new Random(seed);
            int index = rng.Next(PetStoreCatalog.EggRewards.Count);
            return PetStoreCatalog.EggRewards[index];
        }
    }
}
