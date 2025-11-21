
using System;
using System.Linq;
using System.Collections.Generic;
using BirdCafe.Shared.Enums;
using BirdCafe.Shared.Models.Simulation;
using BirdCafe.Shared.Models.Birds;
using BirdCafe.Shared.Models.Meta;

namespace BirdCafe.Shared.Engine.Managers
{
    /// <summary>
    /// Core engine logic for simulating a day of work.
    /// Logic is broken down into: Setup -> Generate Customers -> Process Loop -> Finalize.
    /// </summary>
    public class SimulationManager
    {
        private readonly BirdCafeController _controller;

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
            if (_controller.CurrentPhase != GamePhase.DayLoop)
                return EngineResult.Failure("InvalidPhase", "Cannot run simulation outside of DayLoop phase.");

            var state = _controller.CurrentState;
            var plan = state.CurrentDayState.CurrentPlan;
            var rng = new Random(plan.DaySeed);
            var config = state.Config;

            // --- Step 1: Initialize Result Object ---
            var result = InitializeDayResult(state);
            
            // --- Step 2: Snapshot Bird State ---
            var workingBirds = state.Birds
                .Where(b => plan.BirdIdsWorking.Contains(b.Id) && !b.IsSeverelySick)
                .ToList();

            // Track when each bird will be free (TimeSeconds)
            var birdAvailability = workingBirds.ToDictionary(b => b.Id, b => 0.0f);
            
            // Initialize summaries for report
            foreach(var b in state.Birds)
            {
                result.BirdSummaries.Add(new DayBirdSummary
                {
                    BirdId = b.Id,
                    BirdName = b.Name,
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
            foreach(var cust in customers)
            {
                ProcessCustomerInteraction(state, cust, workingBirds, birdAvailability, result);
            }

            // --- Step 5: End of Day Cleanup (Decay, Waste, Profit) ---
            FinalizeDayStats(state, result, rng);

            // Save result to history
            state.PastDayResults.Add(result);
            result.Timeline = result.Timeline.OrderBy(t => t.TimeSeconds).ToList();

            return EngineResult.Success(result);
        }

        /// <summary>
        /// Advances the game phase after the user has viewed the simulation results.
        /// </summary>
        public EngineResult AdvanceFromSimulation()
        {
            if (_controller.CurrentPhase != GamePhase.DayLoop)
                return EngineResult.Failure("InvalidPhase", "Current phase is not DayLoop.");

            _controller.SetPhase(GamePhase.EveningLoop);
            return EngineResult.Success();
        }

        // ============================================================================
        // Private Helpers (Broken down for Junior Developer readability)
        // ============================================================================

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

        private List<CustomerTransactionRecord> GenerateDailyCustomers(GameSave state, Random rng)
        {
            var config = state.Config;
            
            // Calculate count based on config (No magic numbers here!)
            int count = (int)(config.BaseCustomersPerDay + (state.Cafe.Popularity * config.PopularityToCustomerFactor));
            count = Math.Max(1, count + rng.Next(-2, 3)); 

            var customers = new List<CustomerTransactionRecord>();
            
            for(int i = 0; i < count; i++)
            {
                // Determine Product Preference
                var prodRoll = rng.NextDouble();
                ProductType pref = ProductType.Coffee;
                if (prodRoll > 0.7) pref = ProductType.BakedGoods;
                else if (prodRoll > 0.9) pref = ProductType.ThemedMerch;

                customers.Add(new CustomerTransactionRecord
                {
                    CustomerId = i,
                    // Spread customers out over the day duration
                    ArrivalTimeSeconds = (float)rng.NextDouble() * config.DayDurationSeconds,
                    DesiredProduct = pref
                });
            }
            
            return customers.OrderBy(c => c.ArrivalTimeSeconds).ToList();
        }

        private void ProcessCustomerInteraction(
            GameSave state, 
            CustomerTransactionRecord cust, 
            List<Bird> workingBirds, 
            Dictionary<string, float> birdAvailability, 
            DaySimulationResult result)
        {
            // Log Arrival
            result.Timeline.Add(new SimulationTimelineEvent
            {
                TimeSeconds = cust.ArrivalTimeSeconds,
                EventType = SimulationTimelineEventType.CustomerArrived,
                CustomerId = cust.CustomerId,
                Product = cust.DesiredProduct
            });

            // 1. Find a bird who is free soon enough (Patience Check)
            var patienceLimit = cust.ArrivalTimeSeconds + state.Config.CustomerPatienceSeconds;
            
            var candidate = workingBirds
                .Where(b => birdAvailability[b.Id] <= patienceLimit) // Must be free before patience runs out
                .Where(b => b.Energy > 5f) // Must have energy
                .OrderBy(b => birdAvailability[b.Id]) // Pick the one free soonest
                .FirstOrDefault();

            if (candidate == null)
            {
                // Outcome: Walked Out
                RecordFailedService(cust, result, "WaitTooLong", -1, cust.ArrivalTimeSeconds + state.Config.CustomerPatienceSeconds);
                result.Customers.CustomersLeftUnhappy++;
                return;
            }

            // 2. Check Inventory
            bool hasStock = CheckAndConsumeInventory(state, cust.DesiredProduct);
            if (!hasStock)
            {
                // Outcome: No Stock
                float failTime = Math.Max(cust.ArrivalTimeSeconds, birdAvailability[candidate.Id]) + 1.0f;
                RecordFailedService(cust, result, "NoStock", -2, failTime, candidate.Id);
                result.Customers.CustomersLeftNoStock++;
                
                // Bird still wasted time checking stock
                birdAvailability[candidate.Id] = failTime;
                return;
            }

            // 3. Success: Serve Customer
            RecordSuccessfulService(state, cust, result, candidate, birdAvailability);
        }

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

        private void RecordSuccessfulService(
            GameSave state, 
            CustomerTransactionRecord cust, 
            DaySimulationResult result, 
            Bird bird, 
            Dictionary<string, float> birdAvailability)
        {
            cust.Outcome = CustomerOutcome.Served;
            cust.ServingBirdId = bird.Id;
            
            // Pricing
            decimal price = GetProductPrice(state, cust.DesiredProduct);
            cust.Revenue = price;
            
            // Time Calculation
            float duration = 100f / bird.Productivity; 
            float startTime = Math.Max(cust.ArrivalTimeSeconds, birdAvailability[bird.Id]);
            float endTime = startTime + duration;

            cust.ServiceStartTimeSeconds = startTime;
            cust.ServiceEndTimeSeconds = endTime;

            // Update Bird Availability
            birdAvailability[bird.Id] = endTime;
            
            // Reduce Bird Energy (Using Domain Method)
            bird.ConsumeEnergy(state.Config.EnergyCostPerService);
            
            // Stats
            result.Customers.CustomersServed++;
            UpdateProductSales(result.Customers, cust.DesiredProduct);
            
            // Timeline: Start
            result.Timeline.Add(new SimulationTimelineEvent
            {
                TimeSeconds = startTime,
                EventType = SimulationTimelineEventType.ServiceStarted,
                CustomerId = cust.CustomerId,
                BirdId = bird.Id
            });

            // Timeline: End
            result.Timeline.Add(new SimulationTimelineEvent
            {
                TimeSeconds = endTime,
                EventType = SimulationTimelineEventType.ServiceCompleted,
                CustomerId = cust.CustomerId,
                BirdId = bird.Id,
                MoneyDelta = price,
                PopularityDelta = 1
            });

            cust.PopularityDelta = 1;
            result.CustomerTransactions.Add(cust);
        }

        private void FinalizeDayStats(GameSave state, DaySimulationResult result, Random rng)
        {
            var config = state.Config;

            // 1. Waste Perishables (Coffee/Baked Goods die, Merch stays)
            var inv = state.Cafe.Inventory;
            
            result.Customers.CoffeeWasted = inv.Coffee.QuantityOnHand;
            result.Customers.BakedGoodsWasted = inv.BakedGoods.QuantityOnHand;
            
            inv.Coffee.QuantityOnHand = 0;
            inv.BakedGoods.QuantityOnHand = 0;

            result.Timeline.Add(new SimulationTimelineEvent
            {
                TimeSeconds = config.DayDurationSeconds + 1,
                EventType = SimulationTimelineEventType.ItemPerishedAtEndOfDay,
                ReasonCode = "EndOfDay"
            });

            // 2. Calculate Money
            result.Economy.TotalRevenue = result.CustomerTransactions.Sum(t => t.Revenue);
            state.Economy.CurrentBalance += result.Economy.TotalRevenue;
            result.Economy.EndingMoney = state.Economy.CurrentBalance;

            // Simple COGS calc
            result.Economy.InventoryCost = (result.Customers.CoffeeSold * 1.0m) + (result.Customers.BakedGoodsSold * 2.0m) + (result.Customers.MerchSold * 8.0m);
            result.Economy.WasteCost = (result.Customers.CoffeeWasted * 1.0m) + (result.Customers.BakedGoodsWasted * 2.0m);
            result.Economy.NetProfit = result.Economy.TotalRevenue - (result.Economy.InventoryCost + result.Economy.WasteCost);

            // 3. Popularity
            float popDelta = result.CustomerTransactions.Sum(t => t.PopularityDelta);
            state.Cafe.Popularity = Math.Clamp(state.Cafe.Popularity + popDelta, 0, 100);
            result.Popularity.PopularityAtEnd = state.Cafe.Popularity;

            // 4. Bird Decay & Sickness (Logic moved to Bird class where possible)
            foreach(var summary in result.BirdSummaries)
            {
                var bird = state.Birds.First(b => b.Id == summary.BirdId);
                summary.CustomersServed = result.CustomerTransactions.Count(t => t.ServingBirdId == bird.Id);

                // Apply generic daily decay
                bird.ApplyDailyDecay(config.DailyHungerDecay, config.DailyMoodDecay);

                // Recovery if resting
                if (!summary.WorkedToday)
                {
                    bird.RecoverEnergy(config.RestEnergyRecovery);
                }

                // Sickness Roll
                RollForSickness(bird, summary, config, rng);

                // Final snapshot
                summary.MoodAtEnd = bird.Mood;
                summary.HealthAtEnd = bird.Health;
                summary.EnergyAtEnd = bird.Energy;
            }
        }

        private void RollForSickness(Bird bird, DayBirdSummary summary, GameConfiguration config, Random rng)
        {
            float chance = config.BaselineSicknessChance;
            
            if (bird.Hunger < 20) chance *= config.LowHungerSicknessMultiplier;
            if (bird.Energy < 10) chance *= config.LowEnergySicknessMultiplier;
            
            if (rng.NextDouble() < chance)
            {
                bird.IsSick = true;
                summary.BecameSick = true;
                // Health hit
                bird.Health = Math.Clamp(bird.Health - 20, 0, 100);
            }
        }
        
        // --- Trivial Helpers ---

        private bool CheckAndConsumeInventory(GameSave state, ProductType type)
        {
            var inv = state.Cafe.Inventory;
            switch(type)
            {
                case ProductType.Coffee: if (inv.Coffee.QuantityOnHand > 0) { inv.Coffee.QuantityOnHand--; return true; } break;
                case ProductType.BakedGoods: if (inv.BakedGoods.QuantityOnHand > 0) { inv.BakedGoods.QuantityOnHand--; return true; } break;
                case ProductType.ThemedMerch: if (inv.ThemedMerch.QuantityOnHand > 0) { inv.ThemedMerch.QuantityOnHand--; return true; } break;
            }
            return false;
        }

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

        private void UpdateProductSales(DayCustomerSummary summary, ProductType type)
        {
            switch(type) 
            { 
                case ProductType.Coffee: summary.CoffeeSold++; break; 
                case ProductType.BakedGoods: summary.BakedGoodsSold++; break; 
                case ProductType.ThemedMerch: summary.MerchSold++; break; 
            }
        }
    }
}