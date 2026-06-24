using BirdCafe.Shared.Engine.Utils;
using BirdCafe.Shared.Enums;
using BirdCafe.Shared.Models.Birds;
using BirdCafe.Shared.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace BirdCafe.Shared
{
    /// <summary>
    /// Contains evening inventory, staffing, and day-finalization operations for
    /// <see cref="BirdCafeGame"/>.
    /// </summary>
    public partial class BirdCafeGame
    {
        /// <summary>
        /// Builds planning data, including projected costs, roster status, and recent sales history.
        /// </summary>
        /// <returns>A UI-ready planning dashboard.</returns>
        public PlanningDashboardViewModel GetPlanningDashboard()
        {
            var state = _controller.CurrentState;
            var plan = state.CurrentDayState.CurrentPlan;

            // Use the same economy helper as the planning manager so the preview matches the amount
            // that will be charged when the plan is finalized.
            decimal costCoffee = EconomyHelper.CalculateRestockCost(
                ProductType.Coffee,
                plan.PlannedCoffeePurchase);
            decimal costBaked = EconomyHelper.CalculateRestockCost(
                ProductType.BakedGoods,
                plan.PlannedBakedGoodsPurchase);
            decimal costMerch = EconomyHelper.CalculateRestockCost(
                ProductType.ThemedMerch,
                plan.PlannedThemedMerchPurchase);
            decimal totalCost = costCoffee + costBaked + costMerch;

            var viewModel = new PlanningDashboardViewModel
            {
                CurrentMoney = state.Economy.CurrentBalance,
                CurrentPopularity = (int)state.Cafe.Popularity,
                ProjectedCost = totalCost
            };

            // Show at most seven completed days, but restore ascending order for natural chart and
            // table presentation after selecting the most recent results.
            var recentDays = state.PastDayResults
                .OrderByDescending(day => day.DayNumber)
                .Take(7)
                .OrderBy(day => day.DayNumber)
                .ToList();

            foreach (var day in recentDays)
            {
                viewModel.RecentHistory.Add(new DailySalesHistoryModel
                {
                    DayNumber = day.DayNumber,
                    CustomersArrived = day.Customers.CustomersArrived,
                    CoffeeSold = day.Customers.CoffeeSold,
                    CoffeeWasted = day.Customers.CoffeeWasted,
                    BakedSold = day.Customers.BakedGoodsSold,
                    BakedWasted = day.Customers.BakedGoodsWasted,
                    MerchSold = day.Customers.MerchSold
                });
            }

            // Inventory rows combine current stock, the editable plan quantity, and its projected
            // financial effect. UnitCost values preserve the existing display contract.
            viewModel.Inventory.Add(new InventoryItemModel
            {
                Type = ProductType.Coffee,
                Name = "Coffee Beans",
                CurrentQuantity = state.Cafe.Inventory.Coffee.QuantityOnHand,
                PlannedPurchase = plan.PlannedCoffeePurchase,
                UnitCost = 1.0m,
                TotalCost = costCoffee
            });

            viewModel.Inventory.Add(new InventoryItemModel
            {
                Type = ProductType.BakedGoods,
                Name = "Baked Goods",
                CurrentQuantity = state.Cafe.Inventory.BakedGoods.QuantityOnHand,
                PlannedPurchase = plan.PlannedBakedGoodsPurchase,
                UnitCost = 2.0m,
                TotalCost = costBaked
            });

            viewModel.Inventory.Add(new InventoryItemModel
            {
                Type = ProductType.ThemedMerch,
                Name = "Merch",
                CurrentQuantity = state.Cafe.Inventory.ThemedMerch.QuantityOnHand,
                PlannedPurchase = plan.PlannedThemedMerchPurchase,
                UnitCost = 8.0m,
                TotalCost = costMerch
            });

            foreach (var bird in state.Birds)
            {
                bool isWorking = plan.BirdIdsWorking.Contains(bird.Id);
                string status = isWorking ? "Working" : "Resting";

                // Severe sickness overrides the planned roster label because the bird cannot work,
                // even if an earlier plan still contains its id.
                if (bird.IsSeverelySick)
                {
                    status = "Sick (Cannot Work)";
                }

                viewModel.Roster.Add(new StaffModel
                {
                    BirdId = bird.Id,
                    Name = bird.Name,
                    IsWorking = isWorking,
                    CanWork = !bird.IsSeverelySick,
                    StatusText = status
                });
            }

            if (totalCost > viewModel.CurrentMoney)
            {
                // This warning is advisory. FinalizeDay remains authoritative and revalidates the
                // live balance before committing any purchases.
                viewModel.Warnings.Add("Not enough money!");
            }

            return viewModel;
        }

        /// <summary>
        /// Sets the planned purchase quantity for a product category.
        /// </summary>
        /// <param name="type">The product category whose order should change.</param>
        /// <param name="quantity">The nonnegative quantity to purchase.</param>
        /// <returns><see langword="true"/> when the manager accepts the change.</returns>
        public bool SetInventory(ProductType type, int quantity)
        {
            // Reject negative values at the facade boundary so obvious invalid UI input never reaches
            // the planning manager. The manager still performs its own authoritative validation.
            if (quantity < 0)
            {
                return false;
            }

            return _controller.Planning.SetInventoryOrder(type, quantity).IsSuccess;
        }

        /// <summary>
        /// Builds owned pet-store inventory grouped for presentation layers.
        /// </summary>
        /// <returns>A UI-ready inventory grouped into food, costumes, and toys.</returns>
        public InventoryViewModel GetInventory()
        {
            var state = _controller.CurrentState;

            // Convert the catalog to a lookup once so every owned item can be enriched with display
            // metadata without repeatedly scanning the offer list.
            Dictionary<string, PetStoreSupplyDefinition> supplyMap = PetStoreCatalog.SupplyOffers
                .ToDictionary(supply => supply.ItemId, supply => supply);

            var viewModel = new InventoryViewModel
            {
                OwnedFood = state.PetStore.BirdFoodByType.Select(entry => new OwnedInventoryItem
                {
                    // Food persistence is keyed by enum, while the catalog uses matching string ids.
                    ItemId = supplyMap[entry.Key.ToString()].ItemId,
                    Name = supplyMap[entry.Key.ToString()].DisplayName,
                    CategoryText = supplyMap[entry.Key.ToString()].CategoryText,
                    SupplyType = supplyMap[entry.Key.ToString()].SupplyType,
                    OwnedQuantity = entry.Value,
                    EffectText = supplyMap[entry.Key.ToString()].EffectText
                }).ToList(),

                OwnedCostumes = state.PetStore.OwnedCostumeQuantities.Select(entry => new OwnedInventoryItem
                {
                    ItemId = supplyMap[entry.Key].ItemId,
                    Name = supplyMap[entry.Key.ToString()].DisplayName,
                    CategoryText = supplyMap[entry.Key.ToString()].CategoryText,
                    SupplyType = supplyMap[entry.Key.ToString()].SupplyType,
                    OwnedQuantity = entry.Value,
                    EffectText = supplyMap[entry.Key.ToString()].EffectText
                }).ToList(),

                OwnedToys = state.PetStore.OwnedToyQuantities.Select(entry => new OwnedInventoryItem
                {
                    ItemId = supplyMap[entry.Key].ItemId,
                    Name = supplyMap[entry.Key.ToString()].DisplayName,
                    CategoryText = supplyMap[entry.Key.ToString()].CategoryText,
                    SupplyType = supplyMap[entry.Key.ToString()].SupplyType,
                    OwnedQuantity = entry.Value,
                    EffectText = supplyMap[entry.Key.ToString()].EffectText
                }).ToList()
            };

            return viewModel;
        }

        /// <summary>
        /// Updates the next-day staffing status for a bird.
        /// </summary>
        /// <param name="birdId">The identifier of the bird whose status should change.</param>
        /// <param name="isWorking">Whether the bird should be assigned to work.</param>
        /// <returns><see langword="true"/> when the roster update succeeds.</returns>
        public bool SetStaffStatus(string birdId, bool isWorking)
        {
            // PlanningManager checks phase, bird existence, and sickness constraints before mutating
            // the current plan.
            var result = _controller.Planning.SetStaffRoster(birdId, isWorking);
            if (!result.IsSuccess)
            {
                FireToast(result.UserMessage);
            }

            return result.IsSuccess;
        }

        /// <summary>
        /// Finalizes evening planning and routes to the next progression screen.
        /// </summary>
        /// <returns><see langword="true"/> when the plan is committed successfully.</returns>
        public bool FinalizeDay()
        {
            decimal oldAmount = _controller.CurrentState.Economy.CurrentBalance;

            // FinalizeDay is the atomic manager operation that validates the plan, purchases stock,
            // updates the economy, records ledger data, and advances persistent day state.
            var result = _controller.Planning.FinalizeDay();
            if (!result.IsSuccess)
            {
                FireToast(result.UserMessage);
                return false;
            }

            // The next day must be simulated afresh. Keeping the old cache would replay yesterday's
            // timeline and report against the newly advanced save state.
            _cachedSimResult = null;

            decimal newAmount = _controller.CurrentState.Economy.CurrentBalance;
            if (newAmount != oldAmount)
            {
                OnMoneyChanged?.Invoke(oldAmount, newAmount);
            }

            TransitionTo(GameScreen.DayIntro);
            return true;
        }
    }
}
