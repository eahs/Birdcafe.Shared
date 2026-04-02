
using BirdCafe.Shared.Engine;
using BirdCafe.Shared.Engine.Utils;
using BirdCafe.Shared.Enums;
using BirdCafe.Shared.Models.Birds;
using BirdCafe.Shared.Models.Economy;
using BirdCafe.Shared.Models.Reporting;
using BirdCafe.Shared.Models.Simulation;
using BirdCafe.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace BirdCafe.Shared
{
    /// <summary>
    /// The primary Facade for the game engine. 
    /// UI developers should interact ONLY with this class to communicate with the engine.
    /// </summary>
    public class BirdCafeGame
    {
        /// <summary>
        /// Singleton instance of the game.
        /// </summary>
        public static BirdCafeGame Instance { get; } = new BirdCafeGame();

        private readonly BirdCafeController _controller;
        private GameScreen _currentScreen = GameScreen.MainMenu;
        private DaySimulationResult _cachedSimResult;

        // --- NEW CHAT STATE ---
        /// <summary>
        /// The ID of the current conversation node in the Oracle Chat.
        /// </summary>
        public string CurrentChatStateKey { get; private set; } = ChatData.ROOT_ID;

        /// <summary>
        /// Exposes the underlying controller for legacy integrations.
        /// New UI code should prefer facade methods to preserve layering.
        /// </summary>
        public BirdCafeController Controller => _controller;
        /// <summary>
        /// Gets the currently active screen in the facade-driven UI flow.
        /// </summary>
        public GameScreen CurrentScreen => _currentScreen;
        

        // Events
        /// <summary>
        /// Raised after the facade transitions to a different screen.
        /// </summary>
        public event Action<GameScreen> OnScreenChanged;
        /// <summary>
        /// Raised when an action needs short user-facing feedback.
        /// </summary>
        public event Action<string> OnToastMessage;
        /// <summary>
        /// Raised when the observable money balance changes.
        /// </summary>
        public event Action<decimal> OnMoneyChanged;
        /// <summary>
        /// Raised to request contextual help popup content.
        /// </summary>
        public event Action<string> OnHelpPopup;
        
        // Chat event no longer passes messages, it just signals the UI to open/refresh
        /// <summary>
        /// Raised when Oracle chat should open or refresh.
        /// </summary>
        public event Action OnChatPopup; 

        private BirdCafeGame()
        {
            _controller = new BirdCafeController();
        }

        // =================================================================================
        // META & MAIN MENU
        // =================================================================================

        /// <summary>
        /// Retrieves available save slots for load-game UI screens.
        /// </summary>
        public List<SaveSlotViewModel> GetSaveSlots()
        {
            return _controller.Meta.GetAvailableSaves();
        }

        /// <summary>
        /// Creates a new game and transitions into tutorial on success.
        /// </summary>
        public void StartNewGame(string playerName, string cafeName)
        {
            var result = _controller.Meta.StartNewGame(playerName, cafeName);
            if (!result.IsSuccess)
            {
                FireToast(result.UserMessage);
                return;
            }
            TransitionTo(GameScreen.Tutorial);
        }

        /// <summary>
        /// Continues from a selected save slot and enters day intro flow.
        /// </summary>
        public void LoadGame(string saveId)
        {
            TransitionTo(GameScreen.DayIntro);
        }

        /// <summary>
        /// Requests the help popup for a specific context key.
        /// </summary>
        public void FireHelpPopup(string context = "General")
        {
            OnHelpPopup?.Invoke(context);
        }

        /// <summary>
        /// Opens the Oracle Chat interface and resets to the root topic.
        /// </summary>
        public void FireChatPopup()
        {
            // Reset to start
            CurrentChatStateKey = ChatData.ROOT_ID;
            OnChatPopup?.Invoke();
        }

        /// <summary>
        /// Retrieves the full data for the current state of the conversation.
        /// </summary>
        public ChatMessage GetCurrentChatNode()
        {
            return ChatData.GetNode(CurrentChatStateKey);
        }

        /// <summary>
        /// Processes a player's selection in the chat menu.
        /// </summary>
        /// <param name="optionIndex">The index of the option chosen.</param>
        public void SelectChatOption(int optionIndex)
        {
            var node = GetCurrentChatNode();
            if (optionIndex >= 0 && optionIndex < node.Options.Count)
            {
                var selection = node.Options[optionIndex];
                
                // Update internal state
                CurrentChatStateKey = selection.NextStateId;

                // Signal UI to refresh (handled by same event usually, or UI polls)
                // In console app, we call GetCurrentChatNode again loop.
            }
        }

        // =================================================================================
        // TUTORIAL
        // =================================================================================

        /// <summary>
        /// Builds tutorial copy shown to first-time players.
        /// </summary>
        public TutorialViewModel GetTutorialContent()
        {
            return new TutorialViewModel
            {
                Title = "Your First Day at the Bird Cafe",
                Steps = new List<TutorialStep>
                {
                    new TutorialStep { Title = "Step 1: Start the work day", Description = "We gave you starter coffee. Open the cafe and let your birds serve customers." },
                    new TutorialStep { Title = "Step 2: Take care of your birds at night", Description = "Feed, rest, and heal birds so they are ready for tomorrow." },
                    new TutorialStep { Title = "Step 3: Plan inventory", Description = "Choose how much to sell for each day after." }
                }
            };
        }

        /// <summary>
        /// Completes tutorial navigation and enters day intro.
        /// </summary>
        public void CompleteTutorial()
        {
            TransitionTo(GameScreen.DayIntro);
        }

        // =================================================================================
        // DAY SIMULATION
        // =================================================================================

        /// <summary>
        /// Builds the day-intro view model shown before simulation playback.
        /// </summary>
        public DayIntroViewModel GetDayIntro()
        {
            var state = _controller.CurrentState;
            return new DayIntroViewModel
            {
                DayNumber = state.CurrentDayNumber,
                DayName = state.CurrentDayName.ToString(),
                CafeName = state.Cafe.CafeName,
                Popularity = (int)state.Cafe.Popularity,
                Message = $"Good morning <#008DD4>{state.Profile.DisplayName}</color>! Today is {state.CurrentDayName}, day <#6c18a3>{state.CurrentDayNumber}</color>. Let's make it a great day at {state.Cafe.CafeName}. Good luck!"
            };
        }

        /// <summary>
        /// Starts daily simulation or reuses a cached result for the current day.
        /// </summary>
        public bool StartSimulationPlayback()
        {
            if (_cachedSimResult != null && _cachedSimResult.DayNumber == _controller.CurrentState.CurrentDayNumber)
            {
                TransitionTo(GameScreen.DaySimulation);
                return true;
            }

            var result = _controller.Simulation.RunDaySimulation();

            if (!result.IsSuccess)
            {
                FireToast(result.UserMessage);
                return false;
            }

            _cachedSimResult = (DaySimulationResult)result.Payload;
            TransitionTo(GameScreen.DaySimulation);
            return true;
        }

        /// <summary>
        /// Projects simulation timeline records into UI playback events.
        /// </summary>
        public List<UiTimelineEvent> GetDayTimeline()
        {
            if (_cachedSimResult == null) return new List<UiTimelineEvent>();

            float simDuration = _controller.CurrentState.Config.DayDurationSeconds;
            TimeSpan startOfDay = TimeSpan.FromHours(7); 
            double realHoursOpen = 8.0; 

            return _cachedSimResult.Timeline.Select(t =>
            {
                double pct = t.TimeSeconds / simDuration;
                TimeSpan eventTime = startOfDay.Add(TimeSpan.FromHours(realHoursOpen * pct));
                string timeString = DateTime.Today.Add(eventTime).ToString("hh:mm tt");
                var birdName = _controller.CurrentState.Birds.FirstOrDefault(b => b.Id == t.BirdId)?.Name ?? "Unknown";

                // Fall back to generated descriptions so timeline playback remains readable
                // even when simulation events did not assign an explicit reason code.
                string desc = t.ReasonCode;
                if (string.IsNullOrEmpty(desc))
                {
                    desc = t.EventType.ToString();
                    if (t.EventType == SimulationTimelineEventType.CustomerArrived && t.Product.HasValue)
                        desc = $"Arrived wanting {t.Product}";
                    if (t.EventType == SimulationTimelineEventType.ServiceCompleted && t.MoneyDelta > 0)
                        desc = $"Served {t.Product} (+${t.MoneyDelta:F2})";
                }

                return new UiTimelineEvent
                {
                    TimeSeconds = t.TimeSeconds,
                    FormattedTime = timeString,
                    EventType = t.EventType.ToString(),
                    Description = desc,
                    BirdName = birdName,
                    IconId = t.Product.HasValue ? t.Product.Value.ToString() : null,
                    MoneyDelta = t.MoneyDelta,
                    PopularityDelta = t.PopularityDelta
                };
            }).ToList(); 
        }

        /// <summary>
        /// Advances from simulation into evening progression when valid.
        /// </summary>
        public void FinishSimulation()
        {
            var res = _controller.Simulation.AdvanceFromSimulation();
            if (res.IsSuccess)
            {
                TransitionTo(GameScreen.Hub);
            }
        }

        // =================================================================================
        // EVENING HUB NAVIGATION
        // =================================================================================

        /// <summary>
        /// Navigates to the evening hub screen.
        /// </summary>
        public void GoToHub()
        {
            TransitionTo(GameScreen.Hub);
        }

        /// <summary>
        /// Navigates to the evening summary screen.
        /// </summary>
        public void GoToSummary()
        {
            TransitionTo(GameScreen.EveningSummary);
        }

        /// <summary>
        /// Navigates to the evening care screen.
        /// </summary>
        public void GoToCare()
        {
            TransitionTo(GameScreen.EveningCare);
        }

        /// <summary>
        /// Navigates to the evening planning screen.
        /// </summary>
        public void GoToPlanning()
        {
            TransitionTo(GameScreen.EveningPlanning);
        }

        /// <summary>
        /// Navigates to the pet-store dashboard screen.
        /// </summary>
        public void GoToPetStore()
        {
            TransitionTo(GameScreen.EveningPetStore);
        }

        /// <summary>
        /// Navigates to the pet-store bird offers screen.
        /// </summary>
        public void GoToPetStoreBirds()
        {
            TransitionTo(GameScreen.EveningPetStoreBirds);
        }

        /// <summary>
        /// Navigates to the pet-store supply offers screen.
        /// </summary>
        public void GoToPetStoreSupplies()
        {
            TransitionTo(GameScreen.EveningPetStoreSupplies);
        }

        /// <summary>
        /// Builds summary data for the evening hub screen.
        /// </summary>
        public EveningHubViewModel GetEveningHub()
        {
            var state = _controller.CurrentState;
            return new EveningHubViewModel
            {
                DayNumber = state.CurrentDayNumber,
                CurrentMoney = state.Economy.CurrentBalance,
                CurrentPopularity = (int)state.Cafe.Popularity
            };
        }

        /// <summary>
        /// Builds pet-store dashboard data including ownership and latest reward context.
        /// </summary>
        public PetStoreDashboardViewModel GetPetStoreDashboard()
        {
            var state = _controller.CurrentState;
            var lastReward = state.PetStore.EggRewardHistory.LastOrDefault();
            return new PetStoreDashboardViewModel
            {
                CurrentMoney = state.Economy.CurrentBalance,
                CurrentPopularity = (int)state.Cafe.Popularity,
                OwnedBirdCount = state.Birds.Count,
                BirdFoodUnits = state.PetStore.GetTotalFoodUnits(),
                SpecialEggToysOwned = state.PetStore.SpecialEggToysOwned,
                LastEggRewardText = lastReward == null ? "No egg reward opened yet." : $"Last egg reward: {lastReward.RewardName}"
            };
        }

        /// <summary>
        /// Returns bird offers projected with affordability for store UI.
        /// </summary>
        public List<PetStoreBirdOfferViewModel> GetPetStoreBirdOffers()
        {
            var money = _controller.CurrentState.Economy.CurrentBalance;
            return PetStoreCatalog.BirdOffers.Select(o => new PetStoreBirdOfferViewModel
            {
                SpeciesId = o.SpeciesId,
                Name = o.DisplayName,
                RarityText = o.Rarity.ToString(),
                Price = o.Price,
                EffectText = o.FlavorDescription,
                IsAffordable = money >= o.Price
            }).ToList();
        }

        /// <summary>
        /// Returns supply offers projected with ownership and affordability information.
        /// </summary>
        public List<PetStoreSupplyOfferViewModel> GetPetStoreSupplyOffers()
        {
            var money = _controller.CurrentState.Economy.CurrentBalance;
            var store = _controller.CurrentState.PetStore;
            var offers = new List<PetStoreSupplyOfferViewModel>();

            foreach (var supply in PetStoreCatalog.GetSupplyOffers())
            {
                var ownedQuantity = GetOwnedSupplyQuantity(store, supply);
                var projectedBuyable = supply.Buyable;
                if (supply.SupplyType == PetStoreSupplyType.Costume && ownedQuantity > 0)
                {
                    projectedBuyable = false;
                }

                offers.Add(new PetStoreSupplyOfferViewModel
                {
                    ItemId = supply.ItemId,
                    Name = supply.DisplayName,
                    CategoryText = supply.CategoryText,
                    SupplyType = supply.SupplyType,
                    Price = supply.Price,
                    OwnedQuantity = ownedQuantity,
                    EffectText = supply.EffectText,
                    IsAffordable = projectedBuyable && money >= supply.Price,
                    Buyable = projectedBuyable
                });
            }

            return offers;
        }

        private int GetOwnedSupplyQuantity(PetStoreState store, PetStoreSupplyDefinition supply)
        {
            if (supply.SupplyType == PetStoreSupplyType.BirdFood && supply.BirdFoodType.HasValue)
            {
                return store.GetFoodUnits(supply.BirdFoodType.Value);
            }

            if (supply.SupplyType == PetStoreSupplyType.Toy)
            {
                return store.OwnedToyQuantities.TryGetValue(supply.ItemId, out var toyCount) ? toyCount : 0;
            }

            if (supply.SupplyType == PetStoreSupplyType.Costume)
            {
                return store.OwnedCostumeQuantities.TryGetValue(supply.ItemId, out var costumeCount) ? costumeCount : 0;
            }

            return store.SpecialEggToysOwned;
        }

        /// <summary>
        /// Attempts to buy a bird offer and emits toast/money events as needed.
        /// </summary>
        public bool BuyPetStoreBird(string speciesId)
        {
            var result = _controller.PetStore.BuyBird(speciesId);
            if (!result.IsSuccess)
            {
                FireToast(result.UserMessage);
                return false;
            }

            OnMoneyChanged?.Invoke(_controller.CurrentState.Economy.CurrentBalance);
            return true;
        }

        /// <summary>
        /// Attempts to buy one supply offer and emits toast/money events as needed.
        /// </summary>
        public bool BuyPetStoreSupply(string itemId, PetStoreSupplyType supplyType)
        {
            var result = _controller.PetStore.BuySupply(itemId, supplyType);
            if (!result.IsSuccess)
            {
                FireToast(result.UserMessage);
                return false;
            }

            OnMoneyChanged?.Invoke(_controller.CurrentState.Economy.CurrentBalance);
            return true;
        }

        /// <summary>
        /// Equips a costume on a bird, or unequips when <paramref name="costumeId"/> is null.
        /// </summary>
        public bool EquipBirdCostume(string birdId, string costumeId)
        {
            var result = _controller.PetStore.EquipCostume(birdId, costumeId);
            if (!result.IsSuccess)
            {
                FireToast(result.UserMessage);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Opens a special egg toy and maps the resolved reward for UI display.
        /// </summary>
        public EggRewardResultViewModel OpenSpecialEggToy()
        {
            var result = _controller.PetStore.OpenSpecialEggToy();
            if (!result.IsSuccess)
            {
                FireToast(result.UserMessage);
                return new EggRewardResultViewModel { HasReward = false };
            }

            var reward = (EggRewardRecord)result.Payload;
            return new EggRewardResultViewModel
            {
                HasReward = true,
                RewardTypeText = reward.RewardType.ToString(),
                RewardName = reward.RewardName,
                RewardDescription = reward.RewardDescription
            };
        }

        // =================================================================================
        // EVENING SUMMARY
        // =================================================================================

        /// <summary>
        /// Builds the evening day-report view model from cached simulation output.
        /// </summary>
        public DailyReportViewModel GetDailyReport()
        {
            if (_cachedSimResult == null) return new DailyReportViewModel();

            var stats = _cachedSimResult.Customers;
            var eco = _cachedSimResult.Economy;

            var vm = new DailyReportViewModel
            {
                DayNumber = _cachedSimResult.DayNumber,
                DayName = _cachedSimResult.DayName,
                CurrentPopularity = (int)_controller.CurrentState.Cafe.Popularity,
                CurrentMoney = _controller.CurrentState.Economy.CurrentBalance,

                CustomersServed = stats.CustomersServed,
                CustomersLost = stats.CustomersLeftUnhappy + stats.CustomersLeftNoStock,
                LostWaitTooLong = stats.CustomersLeftUnhappy,
                LostNoStock = stats.CustomersLeftNoStock,

                TotalRevenue = eco.TotalRevenue,
                NetProfit = eco.NetProfit,

                CoffeeSold = stats.CoffeeSold,
                BakedSold = stats.BakedGoodsSold,
                MerchSold = stats.MerchSold,

                CoffeeTotal = stats.CoffeeSold + stats.CoffeeWasted,
                BakedTotal = stats.BakedGoodsSold + stats.BakedGoodsWasted,
                MerchTotal = stats.MerchSold + _controller.CurrentState.Cafe.Inventory.ThemedMerch.QuantityOnHand
            };

            float popDelta = _cachedSimResult.Popularity.PopularityDelta;
            if (popDelta > 2) vm.PopularityNarrative = "Popularity is rising! People love the cafe.";
            else if (popDelta < -2) vm.PopularityNarrative = "Popularity is dropping. Customers are unhappy.";
            else vm.PopularityNarrative = "Popularity is stable.";

            foreach (var b in _cachedSimResult.BirdSummaries)
            {
                vm.Birds.Add(new BirdPerformanceModel
                {
                    BirdId = b.BirdId,
                    Name = b.BirdName,
                    Worked = b.WorkedToday,
                    CustomersServed = b.CustomersServed,
                    BecameSick = b.BecameSick
                });
            }

            return vm;
        }

        // =================================================================================
        // EVENING CARE
        // =================================================================================

        /// <summary>
        /// Builds the care dashboard for all owned birds and current economy context.
        /// </summary>
        public CareDashboardViewModel GetCareDashboard()
        {
            var vm = new CareDashboardViewModel
            {
                CurrentMoney = _controller.CurrentState.Economy.CurrentBalance,
                CurrentPopularity = (int)_controller.CurrentState.Cafe.Popularity,
                StoredBirdFoodUnits = _controller.CurrentState.PetStore.GetTotalFoodUnits()
            };

            foreach (var b in _controller.CurrentState.Birds)
            {
                vm.Birds.Add(MapBirdToCareModel(b));
            }

            return vm;
        }

        
        /// <summary>
        /// Returns care actions with affordability/readiness values precomputed for UI.
        /// </summary>
        public List<CareActionViewModel> GetAvailableActions(string birdId)
        {
            var config = _controller.CurrentState.Config;
            var money = _controller.CurrentState.Economy.CurrentBalance;
            var foodInStorage = _controller.CurrentState.PetStore.GetTotalFoodUnits();

            var actions = new List<CareActionViewModel>
            {
                new CareActionViewModel { ActionId = CareActionIds.Feed, Label = "Feed (Use Stored Food)", Cost = 0, IsAffordable = foodInStorage > 0 },
                new CareActionViewModel { ActionId = CareActionIds.Play, Label = "Play (Mood)", Cost = config.BaselinePlayCost },
                new CareActionViewModel { ActionId = CareActionIds.Vet, Label = "Vet Visit", Cost = config.BaselineVetCost }
            };

            foreach (var a in actions.Where(a => a.ActionId != CareActionIds.Feed))
            {
                a.IsAffordable = money >= a.Cost;
            }

            return actions;
        }

        /// <summary>
        /// Executes an evening care action via the manager layer.
        /// </summary>
        public bool PerformCare(string birdId, string actionId)
        {
            var result = _controller.Care.PerformCareAction(birdId, actionId);

            if (!result.IsSuccess)
            {
                FireToast(result.UserMessage);
                return false;
            }
            OnMoneyChanged?.Invoke(_controller.CurrentState.Economy.CurrentBalance);
            return true;
        }

        /// <summary>
        /// Toggles whether a bird is assigned to rest tomorrow.
        /// </summary>
        public bool ToggleRest(string birdId)
        {
            var result = _controller.Care.ToggleRest(birdId);
            if (!result.IsSuccess)
            {
                FireToast(result.UserMessage);
                return false;
            }
            return true;
        }

        // =================================================================================
        // EVENING PLANNING
        // =================================================================================

        /// <summary>
        /// Builds planning data including projections, roster, and recent performance history.
        /// </summary>
        public PlanningDashboardViewModel GetPlanningDashboard()
        {
            var state = _controller.CurrentState;
            var plan = state.CurrentDayState.CurrentPlan;

            decimal costCoffee = EconomyHelper.CalculateRestockCost(ProductType.Coffee, plan.PlannedCoffeePurchase);
            decimal costBaked = EconomyHelper.CalculateRestockCost(ProductType.BakedGoods, plan.PlannedBakedGoodsPurchase);
            decimal costMerch = EconomyHelper.CalculateRestockCost(ProductType.ThemedMerch, plan.PlannedThemedMerchPurchase);
            decimal totalCost = costCoffee + costBaked + costMerch;

            var vm = new PlanningDashboardViewModel
            {
                CurrentMoney = state.Economy.CurrentBalance,
                CurrentPopularity = (int)state.Cafe.Popularity,
                ProjectedCost = totalCost
            };

            var recentDays = state.PastDayResults
                .OrderByDescending(d => d.DayNumber)
                .Take(7)
                .OrderBy(d => d.DayNumber)
                .ToList();

            foreach (var day in recentDays)
            {
                vm.RecentHistory.Add(new DailySalesHistoryModel
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

            vm.Inventory.Add(new InventoryItemModel
            {
                Type = ProductType.Coffee,
                Name = "Coffee Beans",
                CurrentQuantity = state.Cafe.Inventory.Coffee.QuantityOnHand,
                PlannedPurchase = plan.PlannedCoffeePurchase,
                UnitCost = 1.0m,
                TotalCost = costCoffee
            });

            vm.Inventory.Add(new InventoryItemModel
            {
                Type = ProductType.BakedGoods,
                Name = "Baked Goods",
                CurrentQuantity = state.Cafe.Inventory.BakedGoods.QuantityOnHand,
                PlannedPurchase = plan.PlannedBakedGoodsPurchase,
                UnitCost = 2.0m,
                TotalCost = costBaked
            });

            vm.Inventory.Add(new InventoryItemModel
            {
                Type = ProductType.ThemedMerch,
                Name = "Merch",
                CurrentQuantity = state.Cafe.Inventory.ThemedMerch.QuantityOnHand,
                PlannedPurchase = plan.PlannedThemedMerchPurchase,
                UnitCost = 8.0m,
                TotalCost = costMerch
            });

            foreach (var b in state.Birds)
            {
                bool isWorking = plan.BirdIdsWorking.Contains(b.Id);
                string status = isWorking ? "Working" : "Resting";
                if (b.IsSeverelySick) status = "Sick (Cannot Work)";

                vm.Roster.Add(new StaffModel
                {
                    BirdId = b.Id,
                    Name = b.Name,
                    IsWorking = isWorking,
                    CanWork = !b.IsSeverelySick,
                    StatusText = status
                });
            }

            if (totalCost > vm.CurrentMoney) vm.Warnings.Add("Not enough money!");

            return vm;
        }

        /// <summary>
        /// Sets planned inventory purchase quantity for a product type.
        /// </summary>
        public bool SetInventory(ProductType type, int quantity)
        {
            if (quantity < 0) return false;
            return _controller.Planning.SetInventoryOrder(type, quantity).IsSuccess;
        }
        
        /// <summary>
        /// Builds owned pet-store inventory grouped for presentation layers.
        /// </summary>
        public InventoryViewModel GetInventory()
        {
            var state = _controller.CurrentState;
            Dictionary<string, PetStoreSupplyDefinition> supplyMap = PetStoreCatalog.SupplyOffers.ToDictionary(x => x.ItemId, x => x);

            /*
            var vm = new PetStoreState {
                BirdFoodByType = state.PetStore.BirdFoodByType,
                OwnedToyQuantities = state.PetStore.OwnedToyQuantities,
                OwnedCostumeQuantities = state.PetStore.OwnedCostumeQuantities
            };
            */
            var vm = new InventoryViewModel
            {
                OwnedFood = state.PetStore.BirdFoodByType.Select(kvp => new OwnedInventoryItem
                {
                    ItemId = supplyMap[kvp.Key.ToString()].ItemId,
                    Name = supplyMap[kvp.Key.ToString()].DisplayName,
                    CategoryText = supplyMap[kvp.Key.ToString()].CategoryText,
                    SupplyType = supplyMap[kvp.Key.ToString()].SupplyType,
                    OwnedQuantity = kvp.Value,
                    EffectText = supplyMap[kvp.Key.ToString()].EffectText,
                }).ToList(),

                OwnedCostumes = state.PetStore.OwnedCostumeQuantities.Select(kvp => new OwnedInventoryItem
                {
                    ItemId = supplyMap[kvp.Key].ItemId,
                    Name = supplyMap[kvp.Key.ToString()].DisplayName,
                    CategoryText = supplyMap[kvp.Key.ToString()].CategoryText,
                    SupplyType = supplyMap[kvp.Key.ToString()].SupplyType,
                    OwnedQuantity = kvp.Value,
                    EffectText = supplyMap[kvp.Key.ToString()].EffectText,
                }).ToList(),

                OwnedToys = state.PetStore.OwnedToyQuantities.Select(kvp => new OwnedInventoryItem
                {
                    ItemId = supplyMap[kvp.Key].ItemId,
                    Name = supplyMap[kvp.Key.ToString()].DisplayName,
                    CategoryText = supplyMap[kvp.Key.ToString()].CategoryText,
                    SupplyType = supplyMap[kvp.Key.ToString()].SupplyType,
                    OwnedQuantity = kvp.Value,
                    EffectText = supplyMap[kvp.Key.ToString()].EffectText,
                }).ToList()
            };

            
            return vm;
        }
        /// <summary>
        /// Updates next-day staffing status for a bird.
        /// </summary>
        public bool SetStaffStatus(string birdId, bool isWorking)
        {
            var res = _controller.Planning.SetStaffRoster(birdId, isWorking);
            if (!res.IsSuccess) FireToast(res.UserMessage);
            return res.IsSuccess;
        }

        /// <summary>
        /// Finalizes evening planning and routes to the next progression screen.
        /// </summary>
        public bool FinalizeDay()
        {
            var res = _controller.Planning.FinalizeDay();
            if (!res.IsSuccess)
            {
                FireToast(res.UserMessage);
                return false;
            }

            _cachedSimResult = null;
            OnMoneyChanged?.Invoke(_controller.CurrentState.Economy.CurrentBalance);

            if (_controller.CurrentPhase == GamePhase.Reporting)
            {
                if (_controller.Reporting.CheckGameOver()) TransitionTo(GameScreen.GameOver);
                else TransitionTo(GameScreen.WeeklySummary);
            }
            else
            {
                TransitionTo(GameScreen.DayIntro);
            }

            return true;
        }

        // =================================================================================
        // WEEKLY & GAME OVER
        // =================================================================================


        /// <summary>
        /// Builds a customizable expense report using the shared reporting manager.
        /// </summary>
        /// <param name="request">Options controlling report scope, filters, and grouping.</param>
        /// <returns>A UI-ready expense report.</returns>
        public ExpenseReportViewModel GetExpenseReport(ExpenseReportRequest request)
        {
            return _controller.Reporting.GenerateExpenseReport(request ?? new ExpenseReportRequest());
        }

        /// <summary>
        /// Builds a bird-specific expense report while preserving the shared facade boundary.
        /// </summary>
        /// <param name="birdId">The bird to filter report results by.</param>
        /// <param name="request">Optional report options. When omitted, a current-week transaction report is used.</param>
        /// <returns>A UI-ready expense report filtered to the requested bird.</returns>
        public ExpenseReportViewModel GetBirdExpenseReport(string birdId, ExpenseReportRequest request = null)
        {
            var effectiveRequest = request ?? new ExpenseReportRequest
            {
                Scope = ExpenseReportScope.CurrentWeek,
                GroupBy = ExpenseReportGroupBy.ByTransaction,
                IncludeCareExpenses = true,
                IncludeInventoryExpenses = true
            };

            effectiveRequest.BirdId = birdId;
            return _controller.Reporting.GenerateExpenseReport(effectiveRequest);
        }

        /// <summary>
        /// Builds the unified "Cost of Care Report" modal payload for the selected time filter.
        /// </summary>
        /// <param name="timeFilter">The report scope selector (Today, This Week, or All Time).</param>
        /// <returns>A single view model containing all tab data for the Cost of Care modal.</returns>
        public CostOfCareReportViewModel GetCostOfCareReportViewModel(CostOfCareReportTimeFilter timeFilter)
        {
            return _controller.Reporting.GenerateCostOfCareReport(timeFilter);
        }

        /// <summary>
        /// Builds weekly report data for the completed week.
        /// </summary>
        public WeeklyReportViewModel GetWeeklyReport()
        {
            int currentWeek = _controller.CurrentState.CurrentWeekNumber - 1;
            return _controller.Reporting.GenerateWeeklyReport(currentWeek);
        }

        /// <summary>
        /// Completes weekly report flow and resumes day-loop progression.
        /// </summary>
        public void CompleteWeek()
        {
            _controller.SetPhase(GamePhase.DayLoop);
            TransitionTo(GameScreen.DayIntro);
        }

        /// <summary>
        /// Builds game-over summary details for final results UI.
        /// </summary>
        public GameOverViewModel GetGameOverDetails()
        {
            var state = _controller.CurrentState;
            return new GameOverViewModel
            {
                Reason = state.Cafe.Popularity <= 0 ? "Popularity Collapse" : "Bankruptcy",
                DaysSurvived = state.CurrentDayNumber,
                FinalScore = state.Economy.CurrentBalance
            };
        }

        /// <summary>
        /// Returns navigation to the main menu screen.
        /// </summary>
        public void ReturnToMainMenu()
        {
            TransitionTo(GameScreen.MainMenu);
        }

        // --- Helpers ---

        private void TransitionTo(GameScreen screen)
        {
            _currentScreen = screen;
            OnScreenChanged?.Invoke(screen);
        }

        private void FireToast(string message)
        {
            OnToastMessage?.Invoke(message ?? "Unknown error");
        }

        private BirdCareViewModel MapBirdToCareModel(Models.Birds.Bird b)
        {
            return new BirdCareViewModel
            {
                Id = b.Id,
                SpeciesId = b.SpeciesId,
                Name = b.Name,
                Hunger = (int)b.Hunger,
                Mood = (int)b.Mood,
                Energy = (int)b.Energy,
                Health = (int)b.Health,
                Trust = (int)b.Trust,
                CostumeId = b.CostumeId,
                PreferredFoodsText = b.PreferredFoods.Count == 0 ? "None" : string.Join(", ", b.PreferredFoods),
                FriendshipCount = b.FriendBirdIds.Count,
                IsSick = b.IsSick,
                WillRestTomorrow = b.AssignedDayOffNextDay
            };
        }

        /// <summary>
        /// Debug helper that adds money directly to the active save balance.
        /// </summary>
        public void AddMoney(int v)
        {
            Console.WriteLine($"Adding ${v} to balance.");
            _controller.CurrentState.Economy.CurrentBalance += v;
        }

        /// <summary>
        /// Placeholder facade hook for opening a bird wardrobe interaction.
        /// </summary>
        public void OpenWardrobe(string birdId)
        {
            var bird = _controller.CurrentState.Birds.FirstOrDefault(b => b.Id == birdId);
            Console.WriteLine($"Opening wardrobe for {bird.Name}...");

            

           
        }
    }
}
