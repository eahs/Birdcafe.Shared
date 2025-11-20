
using System;
using System.Linq;
using System.Collections.Generic;
using BirdCafe.Shared.Enums;
using BirdCafe.Shared.Models.Simulation;
using BirdCafe.Shared.Models.Birds;

namespace BirdCafe.Shared.Engine.Managers
{
    public class SimulationManager
    {
        private readonly BirdCafeController _controller;

        public SimulationManager(BirdCafeController controller)
        {
            _controller = controller;
        }

        public EngineResult RunDaySimulation()
        {
            if (_controller.CurrentPhase != GamePhase.DayLoop)
                return EngineResult.Failure("InvalidPhase", "Cannot run simulation outside of DayLoop phase.");

            var state = _controller.CurrentState;
            var plan = state.CurrentDayState.CurrentPlan;
            var rng = new Random(plan.DaySeed);
            var config = state.Config;

            var result = new DaySimulationResult
            {
                DayNumber = state.CurrentDayNumber,
                DayName = state.CurrentDayName.ToString(),
                WeekNumber = state.CurrentWeekNumber,
                Economy = new DayEconomySummary { StartingMoney = state.Economy.CurrentBalance },
                Popularity = new DayPopularitySummary { PopularityAtStart = state.Cafe.Popularity }
            };

            // --- 1. Snapshot Birds ---
            var workingBirds = state.Birds
                .Where(b => plan.BirdIdsWorking.Contains(b.Id) && !b.IsSeverelySick)
                .ToList();

            var birdAvailability = workingBirds.ToDictionary(b => b.Id, b => 0.0f);

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

            // --- 2. Generate Customers ---
            int customerCount = (int)(config.BaseCustomersPerDay + (state.Cafe.Popularity * config.PopularityToCustomerFactor));
            customerCount = Math.Max(1, customerCount + rng.Next(-2, 3)); 

            result.Customers.CustomersArrived = customerCount;
            var dayDurationSeconds = 120f; 
            var customers = new List<CustomerTransactionRecord>();

            for(int i = 0; i < customerCount; i++)
            {
                var prodRoll = rng.NextDouble();
                ProductType pref = ProductType.Coffee;
                if (prodRoll > 0.7) pref = ProductType.BakedGoods;
                else if (prodRoll > 0.9) pref = ProductType.ThemedMerch;

                customers.Add(new CustomerTransactionRecord
                {
                    CustomerId = i,
                    ArrivalTimeSeconds = (float)rng.NextDouble() * dayDurationSeconds,
                    DesiredProduct = pref
                });
            }
            customers = customers.OrderBy(c => c.ArrivalTimeSeconds).ToList();

            // --- 3. Simulation Loop ---
            foreach(var cust in customers)
            {
                result.Timeline.Add(new SimulationTimelineEvent
                {
                    TimeSeconds = cust.ArrivalTimeSeconds,
                    EventType = SimulationTimelineEventType.CustomerArrived,
                    CustomerId = cust.CustomerId,
                    Product = cust.DesiredProduct
                });

                var candidate = workingBirds
                    .Where(b => birdAvailability[b.Id] <= cust.ArrivalTimeSeconds + 5.0f)
                    .Where(b => b.Energy > 5f)
                    .OrderBy(b => birdAvailability[b.Id]) 
                    .FirstOrDefault();

                if (candidate == null)
                {
                    cust.Outcome = CustomerOutcome.LeftUnhappy;
                    cust.PopularityDelta = -1;
                    result.Customers.CustomersLeftUnhappy++;
                    result.Timeline.Add(new SimulationTimelineEvent
                    {
                        TimeSeconds = cust.ArrivalTimeSeconds + 5.0f,
                        EventType = SimulationTimelineEventType.ServiceFailed,
                        CustomerId = cust.CustomerId,
                        ReasonCode = "WaitTooLong",
                        PopularityDelta = -1
                    });
                }
                else
                {
                    bool hasStock = CheckAndConsumeInventory(state, cust.DesiredProduct);
                    if (!hasStock)
                    {
                        cust.Outcome = CustomerOutcome.LeftNoStock;
                        cust.PopularityDelta = -2;
                        result.Customers.CustomersLeftNoStock++;
                        
                        float failTime = Math.Max(cust.ArrivalTimeSeconds, birdAvailability[candidate.Id]) + 1.0f;
                        result.Timeline.Add(new SimulationTimelineEvent
                        {
                            TimeSeconds = failTime,
                            EventType = SimulationTimelineEventType.ServiceFailed,
                            CustomerId = cust.CustomerId,
                            BirdId = candidate.Id,
                            ReasonCode = "NoStock",
                            PopularityDelta = -2
                        });
                        birdAvailability[candidate.Id] = failTime;
                    }
                    else
                    {
                        cust.Outcome = CustomerOutcome.Served;
                        cust.ServingBirdId = candidate.Id;
                        
                        decimal price = GetProductPrice(state, cust.DesiredProduct);
                        cust.Revenue = price;
                        
                        float duration = 100f / candidate.Productivity; 
                        float startTime = Math.Max(cust.ArrivalTimeSeconds, birdAvailability[candidate.Id]);
                        float endTime = startTime + duration;

                        cust.ServiceStartTimeSeconds = startTime;
                        cust.ServiceEndTimeSeconds = endTime;

                        birdAvailability[candidate.Id] = endTime;
                        
                        // Work costs energy
                        candidate.Energy = Math.Max(0, candidate.Energy - 2f);
                        
                        result.Customers.CustomersServed++;
                        UpdateProductSales(result.Customers, cust.DesiredProduct);
                        
                        result.Timeline.Add(new SimulationTimelineEvent
                        {
                            TimeSeconds = startTime,
                            EventType = SimulationTimelineEventType.ServiceStarted,
                            CustomerId = cust.CustomerId,
                            BirdId = candidate.Id
                        });

                        result.Timeline.Add(new SimulationTimelineEvent
                        {
                            TimeSeconds = endTime,
                            EventType = SimulationTimelineEventType.ServiceCompleted,
                            CustomerId = cust.CustomerId,
                            BirdId = candidate.Id,
                            MoneyDelta = price,
                            PopularityDelta = 1
                        });

                        cust.PopularityDelta = 1;
                    }
                }
                result.CustomerTransactions.Add(cust);
            }

            // --- 4. End of Day Logic ---
            
            // Waste
            var inv = state.Cafe.Inventory;
            int wastedCoffee = inv.Coffee.QuantityOnHand;
            int wastedBaked = inv.BakedGoods.QuantityOnHand;
            inv.Coffee.QuantityOnHand = 0;
            inv.BakedGoods.QuantityOnHand = 0;

            result.Customers.CoffeeWasted = wastedCoffee;
            result.Customers.BakedGoodsWasted = wastedBaked;
            result.Timeline.Add(new SimulationTimelineEvent
            {
                TimeSeconds = dayDurationSeconds + 1,
                EventType = SimulationTimelineEventType.ItemPerishedAtEndOfDay,
                ReasonCode = "EndOfDay"
            });

            // Financials
            result.Economy.TotalRevenue = result.CustomerTransactions.Sum(t => t.Revenue);
            state.Economy.CurrentBalance += result.Economy.TotalRevenue;
            result.Economy.EndingMoney = state.Economy.CurrentBalance;

            decimal costCoffeeSold = result.Customers.CoffeeSold * 1.0m;
            decimal costBakedSold = result.Customers.BakedGoodsSold * 2.0m;
            decimal costMerchSold = result.Customers.MerchSold * 8.0m;
            result.Economy.InventoryCost = costCoffeeSold + costBakedSold + costMerchSold;

            decimal costCoffeeWasted = result.Customers.CoffeeWasted * 1.0m;
            decimal costBakedWasted = result.Customers.BakedGoodsWasted * 2.0m;
            result.Economy.WasteCost = costCoffeeWasted + costBakedWasted;

            result.Economy.NetProfit = result.Economy.TotalRevenue - (result.Economy.InventoryCost + result.Economy.WasteCost);

            // Popularity
            float popDelta = result.CustomerTransactions.Sum(t => t.PopularityDelta);
            state.Cafe.Popularity = Math.Clamp(state.Cafe.Popularity + popDelta, 0, 100);
            result.Popularity.PopularityAtEnd = state.Cafe.Popularity;

            // Final Bird Stats & Decay
            foreach(var summary in result.BirdSummaries)
            {
                var bird = state.Birds.First(b => b.Id == summary.BirdId);
                
                summary.CustomersServed = result.CustomerTransactions.Count(t => t.ServingBirdId == bird.Id);

                // 1. Decay: Living costs regardless of work
                // Hunger drops daily
                bird.Hunger = Math.Clamp(bird.Hunger - 30, 0, 100);
                
                // Mood drops slightly from boredom/stress of day
                bird.Mood = Math.Clamp(bird.Mood - 10, 0, 100);

                // 2. Recovery
                if (!summary.WorkedToday)
                {
                    bird.Energy = Math.Min(100, bird.Energy + 50);
                    bird.Stress = Math.Max(0, bird.Stress - 30);
                }

                // 3. Sickness Roll
                // Chance increases if Health low, Hunger low, or Energy low
                float sickChance = config.BaselineSicknessChance;
                if (bird.Hunger < 20) sickChance *= config.LowHungerSicknessMultiplier;
                if (bird.Energy < 10) sickChance *= config.LowEnergySicknessMultiplier;
                
                if (rng.NextDouble() < sickChance)
                {
                    bird.IsSick = true;
                    summary.BecameSick = true;
                    
                    // Sickness hurts health
                    bird.Health = Math.Clamp(bird.Health - 20, 0, 100);
                }

                summary.MoodAtEnd = bird.Mood;
                summary.HealthAtEnd = bird.Health;
                summary.EnergyAtEnd = bird.Energy;
            }

            state.PastDayResults.Add(result);
            result.Timeline = result.Timeline.OrderBy(t => t.TimeSeconds).ToList();

            return EngineResult.Success(result);
        }

        public EngineResult AdvanceFromSimulation()
        {
            if (_controller.CurrentPhase != GamePhase.DayLoop)
                return EngineResult.Failure("InvalidPhase", "Current phase is not DayLoop.");

            _controller.SetPhase(GamePhase.EveningLoop);
            return EngineResult.Success();
        }

        // Helpers (CheckAndConsumeInventory, GetProductPrice, UpdateProductSales unchanged)
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
            return type switch { ProductType.Coffee => config.BasePriceCoffee, ProductType.BakedGoods => config.BasePriceBakedGoods, ProductType.ThemedMerch => config.BasePriceThemedMerch, _ => 0m };
        }

        private void UpdateProductSales(DayCustomerSummary summary, ProductType type)
        {
            switch(type) { case ProductType.Coffee: summary.CoffeeSold++; break; case ProductType.BakedGoods: summary.BakedGoodsSold++; break; case ProductType.ThemedMerch: summary.MerchSold++; break; }
        }
    }
}