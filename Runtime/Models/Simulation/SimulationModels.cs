
using System;
using System.Collections.Generic;
using BirdCafe.Shared.Enums;

namespace BirdCafe.Shared.Models.Simulation
{
    [Serializable]
    public class DaySimulationResult
    {
        // Identity
        public int DayNumber { get; set; }
        public string DayName { get; set; }
        public int WeekNumber { get; set; }

        // Aggregates
        public DayEconomySummary Economy { get; set; } = new DayEconomySummary();
        public DayCustomerSummary Customers { get; set; } = new DayCustomerSummary();
        public DayPopularitySummary Popularity { get; set; } = new DayPopularitySummary();
        public List<DayBirdSummary> BirdSummaries { get; set; } = new List<DayBirdSummary>();

        // Detailed records
        public List<CustomerTransactionRecord> CustomerTransactions { get; set; } = new List<CustomerTransactionRecord>();

        // For animation
        public List<SimulationTimelineEvent> Timeline { get; set; } = new List<SimulationTimelineEvent>();
    }

    [Serializable]
    public class DayEconomySummary
    {
        public decimal StartingMoney { get; set; }
        public decimal EndingMoney { get; set; }

        public decimal TotalRevenue { get; set; }          
        public decimal TotalTips { get; set; }
        
        /// <summary>
        /// Cost of items successfully sold (COGS).
        /// </summary>
        public decimal InventoryCost { get; set; }         
        
        /// <summary>
        /// Cost of items that perished.
        /// </summary>
        public decimal WasteCost { get; set; }             
        
        /// <summary>
        /// Operational Profit: Revenue - (InventoryCost + WasteCost).
        /// </summary>
        public decimal NetProfit { get; set; }
    }

    [Serializable]
    public class DayCustomerSummary
    {
        public int CustomersArrived { get; set; }
        public int CustomersServed { get; set; }
        public int CustomersLeftUnhappy { get; set; }
        public int CustomersLeftNoStock { get; set; }

        public int CoffeeSold { get; set; }
        public int CoffeeWasted { get; set; }
        public int BakedGoodsSold { get; set; }
        public int BakedGoodsWasted { get; set; }
        public int MerchSold { get; set; }
    }

    [Serializable]
    public class DayPopularitySummary
    {
        public float PopularityAtStart { get; set; }
        public float PopularityAtEnd { get; set; }
        public float PopularityDelta => PopularityAtEnd - PopularityAtStart;
    }

    [Serializable]
    public class DayBirdSummary
    {
        public string BirdId { get; set; }
        public string BirdName { get; set; }

        public bool WorkedToday { get; set; }

        public int CustomersServed { get; set; }   
        public int ItemsServed { get; set; }       

        public float MoodAtStart { get; set; }
        public float MoodAtEnd { get; set; }

        public float HealthAtStart { get; set; }
        public float HealthAtEnd { get; set; }

        public float EnergyAtStart { get; set; }
        public float EnergyAtEnd { get; set; }

        public bool BecameSick { get; set; }
        public bool RecoveredFromSickness { get; set; }
    }

    [Serializable]
    public class CustomerTransactionRecord
    {
        public int CustomerId { get; set; }             
        public float ArrivalTimeSeconds { get; set; }   

        public ProductType DesiredProduct { get; set; } 

        public float? ServiceStartTimeSeconds { get; set; }
        public float? ServiceEndTimeSeconds { get; set; }
        public string ServingBirdId { get; set; }

        public CustomerOutcome Outcome { get; set; }

        public decimal Revenue { get; set; }   
        public decimal Tip { get; set; }

        public float PopularityDelta { get; set; } 
    }

    [Serializable]
    public class SimulationTimelineEvent
    {
        public float TimeSeconds { get; set; }             
        public SimulationTimelineEventType EventType { get; set; }
        public int? CustomerId { get; set; }
        public string BirdId { get; set; }
        public ProductType? Product { get; set; }
        public decimal MoneyDelta { get; set; }            
        public float PopularityDelta { get; set; }           
        public string ReasonCode { get; set; }            
    }
}