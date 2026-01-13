
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        /// Provides direct controller access, if needed.
        /// </summary>
        public BirdCafeController Controller => _controller;

        /// <summary>
        /// Current screen/phase of the game
        /// </summary>
        public GameScreen CurrentScreen => _currentScreen;

        /// <summary>
        /// The active chat history for the current session.
        /// </summary>
        public List<ChatMessage> ChatHistory { get; private set; } = new List<ChatMessage>();

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

        /// <summary>
        /// Event fired when the user requests Help.
        /// </summary>
        public event Action<string> OnHelpPopup;

        /// <summary>
        /// Event fired when the user requests Chat.
        /// </summary>
        public event Action OnChatPopup;

        /// <summary>
        /// Event fired when the user sends a chat message.
        /// </summary>
        public event Action<ChatMessage> OnChatUserMessage;

        /// <summary>
        /// Event fired when the system/AI responds to a chat message.
        /// </summary>
        public event Action<ChatMessage> OnChatSystemMessage;

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

            // Show Tutorial first for new games
            TransitionTo(GameScreen.Tutorial);
        }

        public void LoadGame(string saveId)
        {
            // Logic to load would go here...
            TransitionTo(GameScreen.DayIntro);
        }

        public void FireHelpPopup(string context = "General")
        {
            OnHelpPopup?.Invoke(context);
        }

        /// <summary>
        /// Opens the chat window, clears previous history, and sets the greeting.
        /// </summary>
        public void FireChatPopup()
        {
            ChatHistory.Clear();

            var greeting = new ChatMessage
            {
                Sender = "System",
                Content = "I'm happy to help you out with your business and bird care. What can I do for you today?",
                Timestamp = DateTime.Now,
                IsUser = false
            };
            
            ChatHistory.Add(greeting);
            
            // Notify UI that chat opened
            OnChatPopup?.Invoke();

            // Notify UI of the greeting message so it renders
            OnChatSystemMessage?.Invoke(greeting);
        }

        /// <summary>
        /// Sends a message from the user to the game's chat system (e.g. LLM).
        /// </summary>
        public async Task SendChatMessage(string message)
        {
            // 1. Fire User Message Event immediately
            var userMsg = new ChatMessage 
            { 
                Sender = "You", 
                Content = message, 
                Timestamp = DateTime.Now, 
                IsUser = true 
            };
            
            ChatHistory.Add(userMsg);
            OnChatUserMessage?.Invoke(userMsg);

            // 2. Simulate processing delay (Mock LLM)
            await Task.Delay(2000);

            // 3. Fire System Response
            var sysMsg = new ChatMessage 
            { 
                Sender = "System", 
                Content = "Thanks for chatting with me!", 
                Timestamp = DateTime.Now, 
                IsUser = false 
            };
            
            ChatHistory.Add(sysMsg);
            OnChatSystemMessage?.Invoke(sysMsg);
        }

        // =================================================================================
        // 2. TUTORIAL
        // =================================================================================

        public TutorialViewModel GetTutorialContent()
        {
            return new TutorialViewModel
            {
                Title = "Your First Day at the Bird Cafe",
                Steps = new List<TutorialStep>
                {
                    new TutorialStep { Title = "Step 1: Plan inventory", Description = "We gave you starter coffee. Choose how much to sell for each day after." },
                    new TutorialStep { Title = "Step 2: Start the work day", Description = "Open the cafe and let your birds serve customers." },
                    new TutorialStep { Title = "Step 3: Take care of your birds at night", Description = "Feed, rest, and heal birds so they are ready for tomorrow." }
                }
            };
        }

        public void CompleteTutorial()
        {
            TransitionTo(GameScreen.DayIntro);
        }

        // =================================================================================
        // 3. DAY SIMULATION
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
                Message = $"Good morning <#008DD4>{state.Profile.DisplayName}</color>! Today is {state.CurrentDayName}, day <#6c18a3>{state.CurrentDayNumber}</color>. Let's make it a great day at {state.Cafe.CafeName}. Good luck!"
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
                TransitionTo(GameScreen.EveningSummary);
            }
        }

        // =================================================================================
        // 4. EVENING SUMMARY
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

        public void AcknowledgeSummary()
        {
            TransitionTo(GameScreen.EveningCare);
        }

        // =================================================================================
        // 5. EVENING CARE
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

            var actions = new List<CareActionViewModel>
            {
                new CareActionViewModel { ActionId = CareActionIds.Feed, Label = "Feed Snack", Cost = config.BaselineBirdFoodCost },
                new CareActionViewModel { ActionId = CareActionIds.Play, Label = "Play (Mood)", Cost = config.BaselinePlayCost },
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
        // 6. EVENING PLANNING
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
        // 7. WEEKLY & GAME OVER
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