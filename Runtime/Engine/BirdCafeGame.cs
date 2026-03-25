
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

        public BirdCafeController Controller => _controller;
        public GameScreen CurrentScreen => _currentScreen;

        // Events
        public event Action<GameScreen> OnScreenChanged;
        public event Action<string> OnToastMessage;
        public event Action<decimal> OnMoneyChanged;
        public event Action<string> OnHelpPopup;
        
        // Chat event no longer passes messages, it just signals the UI to open/refresh
        public event Action OnChatPopup; 

        private BirdCafeGame()
        {
            _controller = new BirdCafeController();
        }

        // =================================================================================
        // META & MAIN MENU
        // =================================================================================

        public List<SaveSlotViewModel> GetSaveSlots()
        {
            return _controller.Meta.GetAvailableSaves();
        }

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

        public void LoadGame(string saveId)
        {
            TransitionTo(GameScreen.DayIntro);
        }

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

        public void CompleteTutorial()
        {
            TransitionTo(GameScreen.DayIntro);
        }

        // =================================================================================
        // DAY SIMULATION
        // =================================================================================

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

        public void GoToHub()
        {
            TransitionTo(GameScreen.Hub);
        }

        public void GoToSummary()
        {
            TransitionTo(GameScreen.EveningSummary);
        }

        public void GoToCare()
        {
            TransitionTo(GameScreen.EveningCare);
        }

        public void GoToPlanning()
        {
            TransitionTo(GameScreen.EveningPlanning);
        }

        public void GoToPetStore()
        {
            TransitionTo(GameScreen.EveningPetStore);
        }

        public void GoToPetStoreBirds()
        {
            TransitionTo(GameScreen.EveningPetStoreBirds);
        }

        public void GoToPetStoreSupplies()
        {
            TransitionTo(GameScreen.EveningPetStoreSupplies);
        }

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

        public PetStoreDashboardViewModel GetPetStoreDashboard()
        {
            var state = _controller.CurrentState;
            var lastReward = state.PetStore.EggRewardHistory.LastOrDefault();
            return new PetStoreDashboardViewModel
            {
                CurrentMoney = state.Economy.CurrentBalance,
                OwnedBirdCount = state.Birds.Count,
                BirdFoodUnits = state.PetStore.GetTotalFoodUnits(),
                SpecialEggToysOwned = state.PetStore.SpecialEggToysOwned,
                LastEggRewardText = lastReward == null ? "No egg reward opened yet." : $"Last egg reward: {lastReward.RewardName}"
            };
        }

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

        public List<PetStoreSupplyOfferViewModel> GetPetStoreSupplyOffers()
        {
            var money = _controller.CurrentState.Economy.CurrentBalance;
            var store = _controller.CurrentState.PetStore;
            var offers = new List<PetStoreSupplyOfferViewModel>();

            foreach (var supply in PetStoreCatalog.GetSupplyOffers())
            {
                offers.Add(new PetStoreSupplyOfferViewModel
                {
                    ItemId = supply.ItemId,
                    Name = supply.DisplayName,
                    CategoryText = supply.CategoryText,
                    SupplyType = supply.SupplyType,
                    Price = supply.Price,
                    OwnedQuantity = GetOwnedSupplyQuantity(store, supply),
                    EffectText = supply.EffectText,
                    IsAffordable = money >= supply.Price
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

        public bool SetInventory(ProductType type, int quantity)
        {
            if (quantity < 0) return false;
            return _controller.Planning.SetInventoryOrder(type, quantity).IsSuccess;
        }
        
        public PetStoreState GetInventory()
        {
            var state = _controller.CurrentState;

            var vm = new PetStoreState {
                BirdFoodByType = state.PetStore.BirdFoodByType,
                OwnedToyQuantities = state.PetStore.OwnedToyQuantities,
                OwnedCostumeQuantities = state.PetStore.OwnedCostumeQuantities
            };


          
            
            return vm;
        }
        public bool SetStaffStatus(string birdId, bool isWorking)
        {
            var res = _controller.Planning.SetStaffRoster(birdId, isWorking);
            if (!res.IsSuccess) FireToast(res.UserMessage);
            return res.IsSuccess;
        }

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

        public WeeklyReportViewModel GetWeeklyReport()
        {
            int currentWeek = _controller.CurrentState.CurrentWeekNumber - 1;
            return _controller.Reporting.GenerateWeeklyReport(currentWeek);
        }

        public void CompleteWeek()
        {
            _controller.SetPhase(GamePhase.DayLoop);
            TransitionTo(GameScreen.DayIntro);
        }

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
                PreferredFoodsText = b.PreferredFoods.Count == 0 ? "None" : string.Join(", ", b.PreferredFoods),
                FriendshipCount = b.FriendBirdIds.Count,
                IsSick = b.IsSick,
                WillRestTomorrow = b.AssignedDayOffNextDay
            };
        }

        public void AddMoney(int v)
        {
            Console.WriteLine($"Adding ${v} to balance.");
            _controller.CurrentState.Economy.CurrentBalance += v;
        }

    }
}
