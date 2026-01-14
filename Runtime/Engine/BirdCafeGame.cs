
using BirdCafe.Shared.Engine;
using BirdCafe.Shared.Engine.Utils;
using BirdCafe.Shared.Enums;
using BirdCafe.Shared.Models.Simulation;
using BirdCafe.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// The internal controller that manages game logic and state.
        /// </summary>
        private readonly BirdCafeController _controller;

        /// <summary>
        /// Tracks the current screen being displayed to the user.
        /// </summary>
        private GameScreen _currentScreen = GameScreen.MainMenu;

        /// <summary>
        /// Caches the results of the last simulation run so we don't have to recalculate it if the UI refreshes.
        /// </summary>
        private DaySimulationResult _cachedSimResult;

        /// <summary>
        /// Provides direct controller access, if needed for advanced debugging or extensions.
        /// </summary>
        public BirdCafeController Controller => _controller;

        /// <summary>
        /// Gets the current screen/phase of the game.
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

        /// <summary>
        /// Private constructor to enforce the Singleton pattern.
        /// </summary>
        private BirdCafeGame()
        {
            // Initialize the main controller.
            _controller = new BirdCafeController();
        }

        // =================================================================================
        // 1. META & MAIN MENU
        // =================================================================================

        /// <summary>
        /// Retrieves a list of available save slots.
        /// </summary>
        /// <returns>A list of save slot view models.</returns>
        public List<SaveSlotViewModel> GetSaveSlots()
        {
            // Ask the Meta manager for available saves.
            return _controller.Meta.GetAvailableSaves();
        }

        /// <summary>
        /// Starts a new game session with the provided player and cafe details.
        /// </summary>
        /// <param name="playerName">The player's display name.</param>
        /// <param name="cafeName">The name of the cafe.</param>
        public void StartNewGame(string playerName, string cafeName)
        {
            // Attempt to start a new game via the Meta manager.
            var result = _controller.Meta.StartNewGame(playerName, cafeName);

            // Check if the operation failed.
            if (!result.IsSuccess)
            {
                // Notify the user of the failure.
                FireToast(result.UserMessage);
                return;
            }

            // Show the Tutorial screen first for new games.
            TransitionTo(GameScreen.Tutorial);
        }

        /// <summary>
        /// Loads an existing game from a save ID.
        /// </summary>
        /// <param name="saveId">The unique identifier of the save file.</param>
        public void LoadGame(string saveId)
        {
            // Placeholder logic to load the game.
            // Eventually, this would call _controller.Meta.LoadGame().
            TransitionTo(GameScreen.DayIntro);
        }

        /// <summary>
        /// Triggers a help popup with context-specific information.
        /// </summary>
        /// <param name="context">The topic or screen triggering the help request.</param>
        public void FireHelpPopup(string context = "General")
        {
            // Invoke the help event if there are any subscribers.
            OnHelpPopup?.Invoke(context);
        }

        /// <summary>
        /// Opens the chat window, clears previous history, and sets the greeting.
        /// </summary>
        public void FireChatPopup()
        {
            // Clear any old messages from the history list.
            ChatHistory.Clear();

            // Create a default greeting message from the system.
            var greeting = new ChatMessage
            {
                Sender = "System",
                Content = "I'm happy to help you out with your business and bird care. What can I do for you today?",
                Timestamp = DateTime.Now,
                IsUser = false
            };

            // Add the greeting to the history.
            ChatHistory.Add(greeting);

            // Notify the UI that the chat window should open.
            OnChatPopup?.Invoke();

            // Notify the UI to display the system's greeting message.
            OnChatSystemMessage?.Invoke(greeting);
        }

        /// <summary>
        /// Sends a message from the user to the game's chat system (e.g. LLM).
        /// </summary>
        /// <param name="message">The text content of the message.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SendChatMessage(string message)
        {
            // Create the user's message object.
            var userMsg = new ChatMessage
            {
                Sender = "You",
                Content = message,
                Timestamp = DateTime.Now,
                IsUser = true
            };

            // Add it to history and notify listeners immediately so it appears on screen.
            ChatHistory.Add(userMsg);
            OnChatUserMessage?.Invoke(userMsg);

            // Simulate processing delay (Mock LLM) to make it feel realistic.
            await Task.Delay(2000);

            // Create a hardcoded system response.
            var sysMsg = new ChatMessage
            {
                Sender = "System",
                Content = "Thanks for chatting with me!",
                Timestamp = DateTime.Now,
                IsUser = false
            };

            // Add the response to history and notify listeners.
            ChatHistory.Add(sysMsg);
            OnChatSystemMessage?.Invoke(sysMsg);
        }

        // =================================================================================
        // 2. TUTORIAL
        // =================================================================================

        /// <summary>
        /// Gets the content for the tutorial screen.
        /// </summary>
        /// <returns>The view model containing tutorial steps.</returns>
        public TutorialViewModel GetTutorialContent()
        {
            // Create and return a hardcoded tutorial structure.
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

        /// <summary>
        /// Marks the tutorial as complete and transitions to the game intro.
        /// </summary>
        public void CompleteTutorial()
        {
            // Move the screen state to the Day Intro.
            TransitionTo(GameScreen.DayIntro);
        }

        // =================================================================================
        // 3. DAY SIMULATION
        // =================================================================================

        /// <summary>
        /// Gets the data needed to display the "Day Start" banner/intro screen.
        /// </summary>
        /// <returns>The day intro view model.</returns>
        public DayIntroViewModel GetDayIntro()
        {
            // Access the current game state from the controller.
            var state = _controller.CurrentState;

            // Map the state data to the view model.
            return new DayIntroViewModel
            {
                DayNumber = state.CurrentDayNumber,
                DayName = state.CurrentDayName.ToString(),
                CafeName = state.Cafe.CafeName,
                Popularity = (int)state.Cafe.Popularity,
                // Construct a welcoming message with color tags for rich text displays.
                Message = $"Good morning <#008DD4>{state.Profile.DisplayName}</color>! Today is {state.CurrentDayName}, day <#6c18a3>{state.CurrentDayNumber}</color>. Let's make it a great day at {state.Cafe.CafeName}. Good luck!"
            };
        }

        /// <summary>
        /// Starts the simulation logic and prepares the results for playback.
        /// </summary>
        /// <returns>True if the simulation started or was already cached; otherwise, false.</returns>
        public bool StartSimulationPlayback()
        {
            // Check if we already have a result for the current day to avoid re-running it accidentally.
            if (_cachedSimResult != null && _cachedSimResult.DayNumber == _controller.CurrentState.CurrentDayNumber)
            {
                TransitionTo(GameScreen.DaySimulation);
                return true;
            }

            // Run the actual simulation via the manager.
            var result = _controller.Simulation.RunDaySimulation();

            // If the simulation failed (e.g., wrong phase), show an error.
            if (!result.IsSuccess)
            {
                FireToast(result.UserMessage);
                return false;
            }

            // Cast the result payload to the expected type and cache it.
            _cachedSimResult = (DaySimulationResult)result.Payload;

            // Switch screen to the simulation view.
            TransitionTo(GameScreen.DaySimulation);
            return true;
        }

        /// <summary>
        /// Converts the raw simulation timeline into a UI-friendly format.
        /// </summary>
        /// <returns>A list of formatted timeline events for display.</returns>
        public List<UiTimelineEvent> GetDayTimeline()
        {
            // If there's no result cached, return an empty list.
            if (_cachedSimResult == null) return new List<UiTimelineEvent>();

            float simDuration = _controller.CurrentState.Config.DayDurationSeconds;
            TimeSpan startOfDay = TimeSpan.FromHours(7); // Cafe opens at 7:00 AM
            double realHoursOpen = 8.0; // The cafe stays open for 8 real-world hours equivalents

            // Use LINQ 'Select' to transform each raw event into a UI event.
            // This loops through every item in 'Timeline' and creates a new 'UiTimelineEvent' for it.
            return _cachedSimResult.Timeline.Select(t =>
            {
                // Calculate how far through the day this event happened (0.0 to 1.0).
                double pct = t.TimeSeconds / simDuration;

                // Convert that percentage into a real clock time relative to 7:00 AM.
                TimeSpan eventTime = startOfDay.Add(TimeSpan.FromHours(realHoursOpen * pct));

                // Format the time nicely, e.g., "08:30 AM".
                string timeString = DateTime.Today.Add(eventTime).ToString("hh:mm tt");

                // Find the bird's name using the ID, or default to "Unknown" if not found.
                var birdName = _controller.CurrentState.Birds.FirstOrDefault(b => b.Id == t.BirdId)?.Name ?? "Unknown";

                string desc = t.ReasonCode;

                // If there is no specific reason code, build a description from the event type.
                if (string.IsNullOrEmpty(desc))
                {
                    desc = t.EventType.ToString();

                    // Add details for customer arrival.
                    if (t.EventType == SimulationTimelineEventType.CustomerArrived && t.Product.HasValue)
                        desc = $"Arrived wanting {t.Product}";

                    // Add details for service completion and money earned.
                    if (t.EventType == SimulationTimelineEventType.ServiceCompleted && t.MoneyDelta > 0)
                        desc = $"Served {t.Product} (+${t.MoneyDelta:F2})";
                }

                // Return the formatted object.
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
            }).ToList(); // Convert the LINQ result back into a standard List.
        }

        /// <summary>
        /// Completes the simulation viewing phase and moves to the evening summary.
        /// </summary>
        public void FinishSimulation()
        {
            // Tell the controller to advance the state machine from Simulation to Evening.
            var res = _controller.Simulation.AdvanceFromSimulation();

            // If successful, update the UI screen.
            if (res.IsSuccess)
            {
                TransitionTo(GameScreen.EveningSummary);
            }
        }

        // =================================================================================
        // 4. EVENING SUMMARY
        // =================================================================================

        /// <summary>
        /// Generates the daily report view model from the cached simulation results.
        /// </summary>
        /// <returns>The view model containing stats for the day.</returns>
        public DailyReportViewModel GetDailyReport()
        {
            // Safety check: return empty model if no results exist.
            if (_cachedSimResult == null) return new DailyReportViewModel();

            var stats = _cachedSimResult.Customers;
            var eco = _cachedSimResult.Economy;

            // Map internal data structures to the View Model for the UI.
            var vm = new DailyReportViewModel
            {
                DayNumber = _cachedSimResult.DayNumber,
                DayName = _cachedSimResult.DayName,
                CurrentPopularity = (int)_controller.CurrentState.Cafe.Popularity,
                CurrentMoney = _controller.CurrentState.Economy.CurrentBalance,

                CustomersServed = stats.CustomersServed,
                // Combine wait times and stock issues for total lost customers.
                CustomersLost = stats.CustomersLeftUnhappy + stats.CustomersLeftNoStock,
                LostWaitTooLong = stats.CustomersLeftUnhappy,
                LostNoStock = stats.CustomersLeftNoStock,

                TotalRevenue = eco.TotalRevenue,
                NetProfit = eco.NetProfit,

                CoffeeSold = stats.CoffeeSold,
                BakedSold = stats.BakedGoodsSold,
                MerchSold = stats.MerchSold,

                // Calculate total available stock by adding sold items back to wasted/remaining items.
                CoffeeTotal = stats.CoffeeSold + stats.CoffeeWasted,
                BakedTotal = stats.BakedGoodsSold + stats.BakedGoodsWasted,
                MerchTotal = stats.MerchSold + _controller.CurrentState.Cafe.Inventory.ThemedMerch.QuantityOnHand
            };

            // Determine narrative text based on popularity change.
            float popDelta = _cachedSimResult.Popularity.PopularityDelta;
            if (popDelta > 2) vm.PopularityNarrative = "Popularity is rising! People love the cafe.";
            else if (popDelta < -2) vm.PopularityNarrative = "Popularity is dropping. Customers are unhappy.";
            else vm.PopularityNarrative = "Popularity is stable.";

            // Add performance data for each bird.
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

        /// <summary>
        /// Acknowledges the summary screen and moves to the Care Dashboard.
        /// </summary>
        public void AcknowledgeSummary()
        {
            TransitionTo(GameScreen.EveningCare);
        }

        // =================================================================================
        // 5. EVENING CARE
        // =================================================================================

        /// <summary>
        /// Gets the data for the Care Dashboard (Tamagotchi-style screen).
        /// </summary>
        /// <returns>The care dashboard view model.</returns>
        public CareDashboardViewModel GetCareDashboard()
        {
            var vm = new CareDashboardViewModel
            {
                CurrentMoney = _controller.CurrentState.Economy.CurrentBalance,
                CurrentPopularity = (int)_controller.CurrentState.Cafe.Popularity
            };

            // Map each bird in the state to a care view model.
            foreach (var b in _controller.CurrentState.Birds)
            {
                vm.Birds.Add(MapBirdToCareModel(b));
            }

            return vm;
        }

        /// <summary>
        /// Returns a list of available actions for a specific bird, checking affordability.
        /// </summary>
        /// <param name="birdId">The ID of the bird to query actions for.</param>
        /// <returns>A list of action view models.</returns>
        public List<CareActionViewModel> GetAvailableActions(string birdId)
        {
            var config = _controller.CurrentState.Config;
            var money = _controller.CurrentState.Economy.CurrentBalance;

            // Define the list of standard actions.
            var actions = new List<CareActionViewModel>
            {
                new CareActionViewModel { ActionId = CareActionIds.Feed, Label = "Feed Snack", Cost = config.BaselineBirdFoodCost },
                new CareActionViewModel { ActionId = CareActionIds.Play, Label = "Play (Mood)", Cost = config.BaselinePlayCost },
                new CareActionViewModel { ActionId = CareActionIds.Vet, Label = "Vet Visit", Cost = config.BaselineVetCost }
            };

            // Check if the player can afford each action.
            foreach (var a in actions)
            {
                a.IsAffordable = money >= a.Cost;
            }

            return actions;
        }

        /// <summary>
        /// Executes a care action on a bird.
        /// </summary>
        /// <param name="birdId">The ID of the bird.</param>
        /// <param name="actionId">The ID of the action to perform.</param>
        /// <returns>True if the action succeeded; otherwise, false.</returns>
        public bool PerformCare(string birdId, string actionId)
        {
            var result = _controller.Care.PerformCareAction(birdId, actionId);

            if (!result.IsSuccess)
            {
                FireToast(result.UserMessage);
                return false;
            }

            // Notify UI that money has changed so headers can update.
            OnMoneyChanged?.Invoke(_controller.CurrentState.Economy.CurrentBalance);
            return true;
        }

        /// <summary>
        /// Toggles the rest status for a bird for the next day.
        /// </summary>
        /// <param name="birdId">The ID of the bird.</param>
        /// <returns>True if successful.</returns>
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

        /// <summary>
        /// Transitions the game to the Planning/Shop screen.
        /// </summary>
        public void GoToPlanning()
        {
            TransitionTo(GameScreen.EveningPlanning);
        }

        // =================================================================================
        // 6. EVENING PLANNING
        // =================================================================================

        /// <summary>
        /// Gets the data for the Planning/Shop dashboard.
        /// </summary>
        /// <returns>The planning dashboard view model.</returns>
        public PlanningDashboardViewModel GetPlanningDashboard()
        {
            var state = _controller.CurrentState;
            var plan = state.CurrentDayState.CurrentPlan;

            // Calculate costs for currently planned purchases using the shared helper.
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

            // Retrieve the last 7 days of history for the graph/table.
            var recentDays = state.PastDayResults
                .OrderByDescending(d => d.DayNumber) // Sort newest first
                .Take(7) // Take top 7
                .OrderBy(d => d.DayNumber) // Re-sort chronologically for display
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

            // Populate inventory items.
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

            // Populate staff roster.
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

            // Add a warning if user is over budget.
            if (totalCost > vm.CurrentMoney) vm.Warnings.Add("Not enough money!");

            return vm;
        }

        /// <summary>
        /// Updates the planned purchase quantity for a specific product.
        /// </summary>
        /// <param name="type">The product type.</param>
        /// <param name="quantity">The new quantity to buy.</param>
        /// <returns>True if update was successful.</returns>
        public bool SetInventory(ProductType type, int quantity)
        {
            if (quantity < 0) return false;
            return _controller.Planning.SetInventoryOrder(type, quantity).IsSuccess;
        }

        /// <summary>
        /// Updates the work status of a bird in the roster.
        /// </summary>
        /// <param name="birdId">The ID of the bird.</param>
        /// <param name="isWorking">True to work, false to rest.</param>
        /// <returns>True if the update was successful.</returns>
        public bool SetStaffStatus(string birdId, bool isWorking)
        {
            var res = _controller.Planning.SetStaffRoster(birdId, isWorking);
            if (!res.IsSuccess) FireToast(res.UserMessage);
            return res.IsSuccess;
        }

        /// <summary>
        /// Commits the evening plan, purchases inventory, and advances the day.
        /// </summary>
        /// <returns>True if the day was finalized successfully.</returns>
        public bool FinalizeDay()
        {
            var res = _controller.Planning.FinalizeDay();
            if (!res.IsSuccess)
            {
                FireToast(res.UserMessage);
                return false;
            }

            // Clear the cached result since a new day has started.
            _cachedSimResult = null;
            OnMoneyChanged?.Invoke(_controller.CurrentState.Economy.CurrentBalance);

            // Check if we need to show a weekly report or go straight to the next day.
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

        /// <summary>
        /// Gets the weekly report data.
        /// </summary>
        /// <returns>The weekly report view model.</returns>
        public WeeklyReportViewModel GetWeeklyReport()
        {
            int currentWeek = _controller.CurrentState.CurrentWeekNumber - 1;
            return _controller.Reporting.GenerateWeeklyReport(currentWeek);
        }

        /// <summary>
        /// Completes the weekly review and starts the next week loop.
        /// </summary>
        public void CompleteWeek()
        {
            _controller.SetPhase(GamePhase.DayLoop);
            TransitionTo(GameScreen.DayIntro);
        }

        /// <summary>
        /// Gets details for the Game Over screen.
        /// </summary>
        /// <returns>The game over view model.</returns>
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
        /// Resets the game state by returning to the main menu.
        /// </summary>
        public void ReturnToMainMenu()
        {
            TransitionTo(GameScreen.MainMenu);
        }

        // --- Helpers ---

        /// <summary>
        /// Internal helper to switch screens and fire the event.
        /// </summary>
        /// <param name="screen">The new screen to show.</param>
        private void TransitionTo(GameScreen screen)
        {
            _currentScreen = screen;
            OnScreenChanged?.Invoke(screen);
        }

        /// <summary>
        /// Internal helper to fire a toast message safely.
        /// </summary>
        /// <param name="message">The message to display.</param>
        private void FireToast(string message)
        {
            OnToastMessage?.Invoke(message ?? "Unknown error");
        }

        /// <summary>
        /// Helper to map the internal Bird model to the ViewModel used by the Care UI.
        /// </summary>
        /// <param name="b">The bird model.</param>
        /// <returns>The care view model.</returns>
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