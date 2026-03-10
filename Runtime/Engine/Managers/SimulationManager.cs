using BirdCafe.Shared.Enums;
using BirdCafe.Shared.Models.Birds;
using BirdCafe.Shared.Models.Meta;
using BirdCafe.Shared.Models.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BirdCafe.Shared.Engine.Managers
{
    /// <summary>
    /// Core engine logic for simulating a day of work.
    /// Logic is broken down into: Setup -> Generate Customers -> Process Loop -> Finalize.
    /// </summary>
    public class SimulationManager
    {
        /// <summary>
        /// Reference to the main controller.
        /// </summary>
        private readonly BirdCafeController _controller;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimulationManager"/> class.
        /// </summary>
        /// <param name="controller">The main game controller.</param>
        public SimulationManager(BirdCafeController controller)
        {
            _controller = controller;
        }

        /// <summary>
        /// Runs the complete simulation for the current day.
        /// </summary>
        /// <returns>A detailed result object containing timeline events and stats.</returns>
        public EngineResult RunDaySimulation()
        {
            // Verify correct phase.
            if (_controller.CurrentPhase != GamePhase.DayLoop)
                return EngineResult.Failure("InvalidPhase", "Cannot run simulation outside of DayLoop phase.");

            var state = _controller.CurrentState;
            var plan = state.CurrentDayState.CurrentPlan;
            // Initialize Random Number Generator with the specific seed for this day to ensure deterministic results.
            var rng = new Random(plan.DaySeed);
            var config = state.Config;

            // --- Step 1: Initialize Result Object ---
            var result = InitializeDayResult(state);

            // --- Step 2: Snapshot Bird State ---
            // Use LINQ Where to filter the birds list:
            // We only want birds whose IDs are in the "BirdIdsWorking" list AND are not Severely Sick.
            var workingBirds = state.Birds
                .Where(b => plan.BirdIdsWorking.Contains(b.Id) && !b.IsSeverelySick)
                .ToList();

            // Create a Dictionary to track when each bird will be free.
            // Key = Bird ID, Value = The time (in seconds) they finish their current task.
            var birdAvailability = workingBirds.ToDictionary(b => b.Id, b => 0.0f);

            // Create initial summary records for all birds (even those resting).
            foreach (var b in state.Birds)
            {
                result.BirdSummaries.Add(new DayBirdSummary
                {
                    BirdId = b.Id,
                    BirdName = b.Name,
                    // Check if the dictionary has this bird's ID to see if they are working.
                    WorkedToday = birdAvailability.ContainsKey(b.Id),
                    MoodAtStart = b.Mood,
                    HealthAtStart = b.Health,
                    EnergyAtStart = b.Energy
                });
            }

            // --- Step 3: Generate Customer Queue ---
            var customers = GenerateDailyCustomers(state, rng);
            result.Customers.CustomersArrived = customers.Count;

            // --- Step 4: Process Simulation Loop ---
            // Iterate through every generated customer and simulate their interaction.
            foreach (var cust in customers)
            {
                ProcessCustomerInteraction(state, cust, workingBirds, birdAvailability, result);
            }

            // --- Step 5: End of Day Cleanup (Decay, Waste, Profit) ---
            FinalizeDayStats(state, result, rng);

            // Add the final result to the history list.
            state.PastDayResults.Add(result);

            // Sort the timeline events by time so they play back in order.
            result.Timeline = result.Timeline.OrderBy(t => t.TimeSeconds).ToList();

            return EngineResult.Success(result);
        }

        /// <summary>
        /// Advances the game phase after the user has viewed the simulation results.
        /// </summary>
        /// <returns>Success result.</returns>
        public EngineResult AdvanceFromSimulation()
        {
            if (_controller.CurrentPhase != GamePhase.DayLoop)
                return EngineResult.Failure("InvalidPhase", "Current phase is not DayLoop.");

            _controller.SetPhase(GamePhase.EveningLoop);
            return EngineResult.Success();
        }

        // ============================================================================
        // Private Helpers
        // ============================================================================

        /// <summary>
        /// Creates the initial empty result object with start-of-day stats.
        /// </summary>
        /// <param name="state">The game state.</param>
        /// <returns>A new DaySimulationResult object.</returns>
        private DaySimulationResult InitializeDayResult(GameSave state)
        {
            return new DaySimulationResult
            {
                DayNumber = state.CurrentDayNumber,
                DayName = state.CurrentDayName.ToString(),
                WeekNumber = state.CurrentWeekNumber,
                Economy = new DayEconomySummary { StartingMoney = state.Economy.CurrentBalance },
                Popularity = new DayPopularitySummary { PopularityAtStart = state.Cafe.Popularity }
            };
        }

        /// <summary>
        /// Generates a list of customers who will visit the cafe today.
        /// </summary>
        /// <param name="state">The game state.</param>
        /// <param name="rng">The random number generator.</param>
        /// <returns>A list of customer transaction records.</returns>
        private List<CustomerTransactionRecord> GenerateDailyCustomers(GameSave state, Random rng)
        {
            var config = state.Config;

            // Calculate count: Base + (Popularity * Factor).
            int count = (int)(config.BaseCustomersPerDay + (state.Cafe.Popularity * config.PopularityToCustomerFactor));
            // Add slight randomness (-2 to +2 customers).
            count = Math.Max(1, count + rng.Next(-2, 3));

            var customers = new List<CustomerTransactionRecord>();

            for (int i = 0; i < count; i++)
            {
                var newCustomer = new CustomerTransactionRecord
                {
                    CustomerId = i,
                    // Determine arrival time randomly within the day's duration.
                    ArrivalTimeSeconds = (float)rng.NextDouble() * config.DayDurationSeconds,
                    DesiredProducts = new List<ProductType>()
                };

                // Add Primary Item.
                newCustomer.DesiredProducts.Add(RollForProduct(rng));

                // Check random chance for a second item.
                if (rng.NextDouble() < config.ChanceForSecondaryItem)
                {
                    newCustomer.DesiredProducts.Add(RollForProduct(rng));
                }

                customers.Add(newCustomer);
            }

            // Sort customers by arrival time so the simulation processes them in order.
            return customers.OrderBy(c => c.ArrivalTimeSeconds).ToList();
        }

        /// <summary>
        /// Randomly selects a product type based on weighted probabilities.
        /// </summary>
        /// <param name="rng">The random number generator.</param>
        /// <returns>The selected product type.</returns>
        private ProductType RollForProduct(Random rng)
        {
            var prodRoll = rng.NextDouble();
            // 10% chance for Merch (0.9 to 1.0)
            if (prodRoll > 0.9) return ProductType.ThemedMerch;
            // 20% chance for Baked Goods (0.7 to 0.9)
            else if (prodRoll > 0.7) return ProductType.BakedGoods;
            // 70% chance for Coffee (0.0 to 0.7)
            return ProductType.Coffee;
        }

        /// <summary>
        /// Simulates a single customer interaction: assigning a bird, checking stock, and recording outcome.
        /// </summary>
        /// <param name="state">Game state.</param>
        /// <param name="cust">Customer record.</param>
        /// <param name="workingBirds">List of birds on duty.</param>
        /// <param name="birdAvailability">Dictionary tracking when birds are free.</param>
        /// <param name="result">The result object to update.</param>
        private void ProcessCustomerInteraction(
            GameSave state,
            CustomerTransactionRecord cust,
            List<Bird> workingBirds,
            Dictionary<string, float> birdAvailability,
            DaySimulationResult result)
        {
            // Log Arrival event for the timeline.
            result.Timeline.Add(new SimulationTimelineEvent
            {
                TimeSeconds = cust.ArrivalTimeSeconds,
                EventType = SimulationTimelineEventType.CustomerArrived,
                CustomerId = cust.CustomerId,
                Product = cust.DesiredProducts.FirstOrDefault()
            });

            // 1. Find a bird who is free soon enough (Patience Check).
            var patienceLimit = cust.ArrivalTimeSeconds + state.Config.CustomerPatienceSeconds;

            // LINQ Query:
            // 1. Filter birds whose 'Next Free Time' is less than the customer's 'Patience Limit'.
            // 2. Filter birds who have enough energy to work (> 5).
            // 3. Sort by who is free soonest.
            // 4. Take the first one found, or null if none match.
            var candidate = workingBirds
                .Where(b => birdAvailability[b.Id] <= patienceLimit)
                .Where(b => b.Energy > 5f)
                .OrderBy(b => birdAvailability[b.Id])
                .FirstOrDefault();

            if (candidate == null)
            {
                // Outcome: Walked Out due to waiting too long.
                RecordFailedService(cust, result, "WaitTooLong", -1, cust.ArrivalTimeSeconds + state.Config.CustomerPatienceSeconds);
                result.Customers.CustomersLeftUnhappy++;
                return;
            }

            // 2. Check Inventory (Iterate all items desired).
            var fulfillableItems = new List<ProductType>();
            var unfulfillableItems = new List<ProductType>();

            foreach (var desired in cust.DesiredProducts)
            {
                bool hasStock = CheckAndConsumeInventory(state, desired);
                if (hasStock) fulfillableItems.Add(desired);
                else unfulfillableItems.Add(desired);
            }

            // 3. Resolve Outcome.
            if (fulfillableItems.Count == 0)
            {
                // Outcome: No Stock for ANY item.
                // The fail time is the later of: when the customer arrived OR when the bird became free to check stock.
                float failTime = Math.Max(cust.ArrivalTimeSeconds, birdAvailability[candidate.Id]) + 1.0f;
                RecordFailedService(cust, result, "NoStock", -2, failTime, candidate.Id);
                result.Customers.CustomersLeftNoStock++;

                // Bird still wasted time checking stock.
                birdAvailability[candidate.Id] = failTime;
            }
            else
            {
                // Outcome: Success (At least partially).
                RecordSuccessfulService(state, cust, result, candidate, workingBirds, birdAvailability, fulfillableItems);
            }
        }

        /// <summary>
        /// Records a failed service interaction to the timeline and customer record.
        /// </summary>
        private void RecordFailedService(CustomerTransactionRecord cust, DaySimulationResult result, string reason, int popHit, float time, string birdId = null)
        {
            cust.Outcome = CustomerOutcome.LeftUnhappy;
            cust.PopularityDelta = popHit;

            result.Timeline.Add(new SimulationTimelineEvent
            {
                TimeSeconds = time,
                EventType = SimulationTimelineEventType.ServiceFailed,
                CustomerId = cust.CustomerId,
                BirdId = birdId,
                ReasonCode = reason,
                PopularityDelta = popHit
            });
        }

        /// <summary>
        /// Records a successful transaction, updates revenue, and updates bird fatigue.
        /// </summary>
        private void RecordSuccessfulService(
            GameSave state,
            CustomerTransactionRecord cust,
            DaySimulationResult result,
            Bird bird,
            List<Bird> workingBirds,
            Dictionary<string, float> birdAvailability,
            List<ProductType> servedItems)
        {
            cust.Outcome = CustomerOutcome.Served;
            cust.ServingBirdId = bird.Id;

            // Calculate Service Duration.
            // Base duration depends on Productivity (higher productivity = lower duration).
            float duration = (100f / bird.Productivity);
            // Add 20% penalty for each extra item.
            duration += duration * 0.2f * (servedItems.Count - 1);

            // Start time is whichever is later: Arrival or Bird Available.
            float startTime = Math.Max(cust.ArrivalTimeSeconds, birdAvailability[bird.Id]);
            float endTime = startTime + duration;

            cust.ServiceStartTimeSeconds = startTime;
            cust.ServiceEndTimeSeconds = endTime;

            // Update Bird Availability to the new end time.
            birdAvailability[bird.Id] = endTime;

            // Log Service Start event.
            result.Timeline.Add(new SimulationTimelineEvent
            {
                TimeSeconds = startTime,
                EventType = SimulationTimelineEventType.ServiceStarted,
                CustomerId = cust.CustomerId,
                BirdId = bird.Id
            });

            // Process each item sold.
            decimal totalRevenue = 0;
            decimal trustRevenueBonus = GetTrustRevenueBonusMultiplier(bird);
            decimal friendshipRevenueBonus = GetFriendshipRevenueBonusMultiplier(bird, workingBirds);
            decimal revenueMultiplier = 1m + trustRevenueBonus + friendshipRevenueBonus;

            foreach (var item in servedItems)
            {
                decimal price = decimal.Round(GetProductPrice(state, item) * revenueMultiplier, 2);
                totalRevenue += price;

                // Bird gets tired.
                bird.ConsumeEnergy(state.Config.EnergyCostPerService);

                // Update aggregate counts.
                UpdateProductSales(result.Customers, item);

                float friendlinessBonus = (bird.Friendliness / 100f) + (state.PetStore.TotalBirdBuffStacks * 0.02f);

                // Log completion event for this specific item.
                result.Timeline.Add(new SimulationTimelineEvent
                {
                    TimeSeconds = endTime,
                    EventType = SimulationTimelineEventType.ServiceCompleted,
                    CustomerId = cust.CustomerId,
                    BirdId = bird.Id,
                    Product = item,
                    MoneyDelta = price,
                    PopularityDelta = (1f / servedItems.Count) + friendlinessBonus // Split base popularity gain and add bird-based charm bonus.
                });
            }

            cust.Revenue = totalRevenue;
            cust.PopularityDelta = 1 + (bird.Friendliness / 100f) + (state.PetStore.TotalBirdBuffStacks * 0.02f);
            result.Customers.CustomersServed++;

            result.CustomerTransactions.Add(cust);
        }

        /// <summary>
        /// Calculates final totals, applies waste logic, and handles bird daily decay.
        /// </summary>
        private void FinalizeDayStats(GameSave state, DaySimulationResult result, Random rng)
        {
            var config = state.Config;

            // 1. Waste Perishables (Coffee/Baked Goods die, Merch stays).
            var inv = state.Cafe.Inventory;

            result.Customers.CoffeeWasted = inv.Coffee.QuantityOnHand;
            result.Customers.BakedGoodsWasted = inv.BakedGoods.QuantityOnHand;

            // Reset perishable quantities to 0.
            inv.Coffee.QuantityOnHand = 0;
            inv.BakedGoods.QuantityOnHand = 0;

            result.Timeline.Add(new SimulationTimelineEvent
            {
                TimeSeconds = config.DayDurationSeconds + 1,
                EventType = SimulationTimelineEventType.ItemPerishedAtEndOfDay,
                ReasonCode = "EndOfDay"
            });

            // 2. Calculate Money Stats.
            result.Economy.TotalRevenue = result.CustomerTransactions.Sum(t => t.Revenue);
            state.Economy.CurrentBalance += result.Economy.TotalRevenue;
            result.Economy.EndingMoney = state.Economy.CurrentBalance;

            // Simple Cost of Goods Sold (COGS) calculation.
            result.Economy.InventoryCost = (result.Customers.CoffeeSold * 1.0m) + (result.Customers.BakedGoodsSold * 2.0m) + (result.Customers.MerchSold * 8.0m);
            // Cost of wasted items.
            result.Economy.WasteCost = (result.Customers.CoffeeWasted * 1.0m) + (result.Customers.BakedGoodsWasted * 2.0m);
            // Net Profit = Revenue - Expenses.
            result.Economy.NetProfit = result.Economy.TotalRevenue - (result.Economy.InventoryCost + result.Economy.WasteCost);

            // 3. Update Popularity.
            float popDelta = result.CustomerTransactions.Sum(t => t.PopularityDelta);
            // Clamp popularity between 0 and 100.
            state.Cafe.Popularity = Math.Clamp(state.Cafe.Popularity + popDelta, 0, 100);
            result.Popularity.PopularityAtEnd = state.Cafe.Popularity;

            GrowWorkingBirdFriendships(result, state);

            // 4. Bird Decay & Sickness.
            foreach (var summary in result.BirdSummaries)
            {
                var bird = state.Birds.First(b => b.Id == summary.BirdId);
                // Count how many times this bird served a customer.
                summary.CustomersServed = result.CustomerTransactions.Count(t => t.ServingBirdId == bird.Id);

                // Apply generic daily decay (hunger/mood).
                bird.ApplyDailyDecay(config.DailyHungerDecay, config.DailyMoodDecay);

                // Starvation Check: If hunger hits 0, bird loses health directly.
                if (bird.Hunger <= 0)
                {
                    bird.Health = Math.Max(0, bird.Health - config.StarvationHealthDamage);
                    result.Timeline.Add(new SimulationTimelineEvent
                    {
                        TimeSeconds = config.DayDurationSeconds + 2,
                        EventType = SimulationTimelineEventType.BirdStateChanged,
                        BirdId = bird.Id,
                        ReasonCode = "StarvationHealthLoss"
                    });
                }

                // Overnight Sleep Recovery (Applies to ALL birds).
                bird.RecoverEnergy(config.BaseNightlyEnergyRecovery);

                // Additional Recovery if they didn't work.
                if (!summary.WorkedToday)
                {
                    bird.RecoverEnergy(config.RestDayEnergyBonus);
                }

                // Check if they get sick.
                RollForSickness(bird, summary, config, rng);

                // Snapshot final stats.
                summary.MoodAtEnd = bird.Mood;
                summary.HealthAtEnd = bird.Health;
                summary.EnergyAtEnd = bird.Energy;
            }
        }

        /// <summary>
        /// Determines if a bird gets sick based on chance and stats.
        /// </summary>
        private void RollForSickness(Bird bird, DayBirdSummary summary, GameConfiguration config, Random rng)
        {
            float chance = config.BaselineSicknessChance;

            // Increase chance if stats are low.
            if (bird.Hunger < 20 && bird.Hunger > 0) chance *= config.LowHungerSicknessMultiplier;
            if (bird.Energy < 10) chance *= config.LowEnergySicknessMultiplier;

            // Roll the dice.
            if (rng.NextDouble() < chance)
            {
                bird.IsSick = true;
                summary.BecameSick = true;
                // Reduce health.
                bird.Health = Math.Clamp(bird.Health - 20, 0, 100);
            }
        }

        private decimal GetTrustRevenueBonusMultiplier(Bird bird)
        {
            // Max +30% at 100 trust.
            return Math.Min(0.30m, bird.Trust * 0.003m);
        }

        private decimal GetFriendshipRevenueBonusMultiplier(Bird bird, List<Bird> workingBirds)
        {
            if (workingBirds.Count <= 1)
            {
                return 0m;
            }

            decimal bonus = 0m;
            foreach (var teammate in workingBirds)
            {
                if (teammate.Id == bird.Id)
                {
                    continue;
                }

                int friendshipScore = bird.GetFriendshipScore(teammate.Id);
                bonus += friendshipScore * 0.001m;
            }

            return Math.Min(0.20m, bonus);
        }

        private void GrowWorkingBirdFriendships(DaySimulationResult result, GameSave state)
        {
            var workingBirds = result.BirdSummaries
                .Where(s => s.WorkedToday)
                .Select(s => state.Birds.First(b => b.Id == s.BirdId))
                .ToList();

            for (int i = 0; i < workingBirds.Count; i++)
            {
                for (int j = i + 1; j < workingBirds.Count; j++)
                {
                    workingBirds[i].GrowFriendship(workingBirds[j].Id, 4);
                    workingBirds[j].GrowFriendship(workingBirds[i].Id, 4);
                }
            }
        }

        // --- Trivial Helpers ---

        /// <summary>
        /// Checks if an item is in stock and decrements it if true.
        /// </summary>
        private bool CheckAndConsumeInventory(GameSave state, ProductType type)
        {
            var inv = state.Cafe.Inventory;
            switch (type)
            {
                case ProductType.Coffee:
                    if (inv.Coffee.QuantityOnHand > 0) { inv.Coffee.QuantityOnHand--; return true; }
                    break;
                case ProductType.BakedGoods:
                    if (inv.BakedGoods.QuantityOnHand > 0) { inv.BakedGoods.QuantityOnHand--; return true; }
                    break;
                case ProductType.ThemedMerch:
                    if (inv.ThemedMerch.QuantityOnHand > 0) { inv.ThemedMerch.QuantityOnHand--; return true; }
                    break;
            }
            return false;
        }

        /// <summary>
        /// Gets the sale price of a product from config.
        /// </summary>
        private decimal GetProductPrice(GameSave state, ProductType type)
        {
            var config = state.Config;
            return type switch
            {
                ProductType.Coffee => config.BasePriceCoffee,
                ProductType.BakedGoods => config.BasePriceBakedGoods,
                ProductType.ThemedMerch => config.BasePriceThemedMerch,
                _ => 0m
            };
        }

        /// <summary>
        /// Updates the sales counters in the summary object.
        /// </summary>
        private void UpdateProductSales(DayCustomerSummary summary, ProductType type)
        {
            switch (type)
            {
                case ProductType.Coffee: summary.CoffeeSold++; break;
                case ProductType.BakedGoods: summary.BakedGoodsSold++; break;
                case ProductType.ThemedMerch: summary.MerchSold++; break;
            }
        }
    }
}
