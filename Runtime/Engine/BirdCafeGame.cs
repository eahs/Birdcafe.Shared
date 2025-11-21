
using System;
using System.Collections.Generic;
using System.Linq;
using BirdCafe.Shared.Engine;
using BirdCafe.Shared.Engine.Managers;
using BirdCafe.Shared.Engine.Utils;
using BirdCafe.Shared.Enums;
using BirdCafe.Shared.Models.Simulation;
using BirdCafe.Shared.ViewModels;

namespace BirdCafe.Shared
{
    /// <summary>
    /// The primary Facade for the game engine. 
    /// UI developers should interact ONLY with this class.
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

        /// <summary>
        /// Event fired when the game changes phase/screens (e.g., Simulation -> Evening).
        /// </summary>
        public event Action<GameScreen> OnScreenChanged;
        
        /// <summary>
        /// Event fired when an error or important notification occurs.
        /// </summary>
        public event Action<string> OnToastMessage;
        
        /// <summary>
        /// Event fired whenever the player's money balance changes.
        /// </summary>
        public event Action<decimal> OnMoneyChanged;

        private BirdCafeGame()
        {
            _controller = new BirdCafeController();
        }

        // =================================================================================
        // 1. META & MAIN MENU
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

            TransitionTo(GameScreen.DayIntro);
        }

        public void LoadGame(string saveId)
        {
            // Logic to load would go here...
            TransitionTo(GameScreen.DayIntro);
        }

        // =================================================================================
        // 2. DAY SIMULATION
        // =================================================================================

        public DayIntroViewModel GetDayIntro()
        {
            var state = _controller.CurrentState;
            return new DayIntroViewModel
            {
                DayNumber = state.CurrentDayNumber,
                DayName = state.CurrentDayName.ToString(),
                CafeName = state.Cafe.CafeName, // Populate CafeName
                Popularity = (int)state.Cafe.Popularity,
                Message = $"Welcome to Day {state.CurrentDayNumber}! Good luck."
            };
        }

        public bool StartSimulationPlayback()
        {
            // Idempotency check
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

            // Junior Dev Note: 
            // The simulation uses seconds (0 to 120), but we want to show 
            // friendly times like "07:30 AM". 
            // We map 0s -> 7:00 AM and 120s -> 3:00 PM (15:00).
            
            float simDuration = _controller.CurrentState.Config.DayDurationSeconds;
            TimeSpan startOfDay = TimeSpan.FromHours(7); // 7:00 AM
            double realHoursOpen = 8.0; // Open 8 hours

            return _cachedSimResult.Timeline.Select(t => 
            {
                // Calculate percentage of day complete
                double pct = t.TimeSeconds / simDuration;
                // Add that percentage of 8 hours to 7:00 AM
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
                    FormattedTime = timeString, // Populate formatted string
                    EventType = t.EventType.ToString(),
                    Description = desc,
                    BirdName = birdName,
                    IconId = t.Product.HasValue ? t.Product.Value.ToString() : null
                };
            }).ToList();
        }

        public void FinishSimulation()
        {
            var res = _controller.Simulation.AdvanceFromSimulation();
            if (res.IsSuccess)
            {
                TransitionTo(GameScreen.EveningSummary);
            }
        }

        // =================================================================================
        // 3. EVENING SUMMARY
        // =================================================================================

        public DailyReportViewModel GetDailyReport()
        {
            if (_cachedSimResult == null) return new DailyReportViewModel();

            var popDelta = _cachedSimResult.Popularity.PopularityDelta;
            string narrative = "Popularity remained stable.";
            if (popDelta > 2) narrative = "Word is spreading! Popularity is rising.";
            else if (popDelta < -2) narrative = "We disappointed some folks. Popularity is dropping.";

            var vm = new DailyReportViewModel
            {
                DayNumber = _cachedSimResult.DayNumber,
                CurrentPopularity = (int)_controller.CurrentState.Cafe.Popularity,
                CustomersServed = _cachedSimResult.Customers.CustomersServed,
                CustomersLost = _cachedSimResult.Customers.CustomersLeftUnhappy + _cachedSimResult.Customers.CustomersLeftNoStock,
                
                // Detailed Breakdown
                LostWaitTooLong = _cachedSimResult.Customers.CustomersLeftUnhappy,
                LostNoStock = _cachedSimResult.Customers.CustomersLeftNoStock,
                CoffeeSold = _cachedSimResult.Customers.CoffeeSold,
                BakedSold = _cachedSimResult.Customers.BakedGoodsSold,
                MerchSold = _cachedSimResult.Customers.MerchSold,
                PopularityNarrative = narrative,

                TotalRevenue = _cachedSimResult.Economy.TotalRevenue,
                NetProfit = _cachedSimResult.Economy.NetProfit
            };

            foreach(var b in _cachedSimResult.BirdSummaries)
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

        public void AcknowledgeSummary()
        {
            TransitionTo(GameScreen.EveningCare);
        }

        // =================================================================================
        // 4. EVENING CARE
        // =================================================================================

        public CareDashboardViewModel GetCareDashboard()
        {
            var vm = new CareDashboardViewModel
            {
                CurrentMoney = _controller.CurrentState.Economy.CurrentBalance,
                CurrentPopularity = (int)_controller.CurrentState.Cafe.Popularity
            };

            foreach(var b in _controller.CurrentState.Birds)
            {
                vm.Birds.Add(MapBirdToCareModel(b));
            }

            return vm;
        }

        public List<CareActionViewModel> GetAvailableActions(string birdId)
        {
            var config = _controller.CurrentState.Config;
            var money = _controller.CurrentState.Economy.CurrentBalance;

            // Refactored to use Constants
            var actions = new List<CareActionViewModel>
            {
                new CareActionViewModel { ActionId = CareActionIds.Feed, Label = "Feed Snack", Cost = config.BaselineBirdFoodCost },
                new CareActionViewModel { ActionId = CareActionIds.Play, Label = "Play (Mood)", Cost = config.BaselinePlayCost }, // New Option
                new CareActionViewModel { ActionId = CareActionIds.Vet, Label = "Vet Visit", Cost = config.BaselineVetCost }
            };

            foreach(var a in actions)
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

        public void GoToPlanning()
        {
            TransitionTo(GameScreen.EveningPlanning);
        }

        // =================================================================================
        // 5. EVENING PLANNING
        // =================================================================================

        /// <summary>
        /// Generates the view model for the planning screen.
        /// </summary>
        public PlanningDashboardViewModel GetPlanningDashboard()
        {
            var state = _controller.CurrentState;
            var plan = state.CurrentDayState.CurrentPlan;
            
            // Refactored: Use Shared Helper so UI math matches Engine math
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

            // --- HISTORY ---
            // Populate history for context (Restored per requirements)
            var recentDays = state.PastDayResults
                .OrderByDescending(d => d.DayNumber)
                .Take(7)
                .OrderBy(d => d.DayNumber)
                .ToList();

            foreach(var day in recentDays)
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

            // --- INVENTORY ---
            // Explicitly adding all types so they show up in the UI list
            vm.Inventory.Add(new InventoryItemModel 
            { 
                Type = ProductType.Coffee, Name = "Coffee Beans", 
                CurrentQuantity = state.Cafe.Inventory.Coffee.QuantityOnHand,
                PlannedPurchase = plan.PlannedCoffeePurchase, UnitCost = 1.0m, TotalCost = costCoffee
            });

            vm.Inventory.Add(new InventoryItemModel 
            { 
                Type = ProductType.BakedGoods, Name = "Baked Goods", 
                CurrentQuantity = state.Cafe.Inventory.BakedGoods.QuantityOnHand,
                PlannedPurchase = plan.PlannedBakedGoodsPurchase, UnitCost = 2.0m, TotalCost = costBaked
            });

            vm.Inventory.Add(new InventoryItemModel 
            { 
                Type = ProductType.ThemedMerch, Name = "Merch", 
                CurrentQuantity = state.Cafe.Inventory.ThemedMerch.QuantityOnHand,
                PlannedPurchase = plan.PlannedThemedMerchPurchase, UnitCost = 8.0m, TotalCost = costMerch
            });
            
            // --- ROSTER ---
            foreach(var b in state.Birds)
            {
                bool isWorking = plan.BirdIdsWorking.Contains(b.Id);
                string status = isWorking ? "Working" : "Resting";
                if (b.IsSeverelySick) status = "Sick (Cannot Work)";
                
                vm.Roster.Add(new StaffModel
                {
                    BirdId = b.Id, Name = b.Name, IsWorking = isWorking, CanWork = !b.IsSeverelySick, StatusText = status
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
        // 6. WEEKLY & GAME OVER
        // =================================================================================

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
                Name = b.Name,
                Hunger = (int)b.Hunger,
                Mood = (int)b.Mood,
                Energy = (int)b.Energy,
                Health = (int)b.Health,
                IsSick = b.IsSick,
                WillRestTomorrow = b.AssignedDayOffNextDay
            };
        }
    }
}