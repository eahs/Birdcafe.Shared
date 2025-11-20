
using System;
using System.Collections.Generic;
using BirdCafe.Shared.Enums;

namespace BirdCafe.Shared.ViewModels
{
    // --- SUMMARY ---
    [Serializable]
    public class DailyReportViewModel
    {
        public int DayNumber { get; set; }
        public int CurrentPopularity { get; set; } // Added for visibility
        public int CustomersServed { get; set; }
        public int CustomersLost { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal NetProfit { get; set; }
        public List<BirdPerformanceModel> Birds { get; set; } = new List<BirdPerformanceModel>();
    }

    [Serializable]
    public class BirdPerformanceModel
    {
        public string BirdId { get; set; }
        public string Name { get; set; }
        public bool Worked { get; set; }
        public int CustomersServed { get; set; }
        public bool BecameSick { get; set; }
    }

    // --- CARE ---
    [Serializable]
    public class CareDashboardViewModel
    {
        public decimal CurrentMoney { get; set; }
        public int CurrentPopularity { get; set; } // Added for visibility
        public List<BirdCareViewModel> Birds { get; set; } = new List<BirdCareViewModel>();
    }

    [Serializable]
    public class BirdCareViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        
        // Stats 0-100
        public int Hunger { get; set; }
        public int Mood { get; set; }
        public int Energy { get; set; }
        public int Health { get; set; }
        
        public bool IsSick { get; set; }
        public bool WillRestTomorrow { get; set; }
    }

    [Serializable]
    public class CareActionViewModel
    {
        public string ActionId { get; set; }
        public string Label { get; set; }
        public decimal Cost { get; set; }
        public bool IsAffordable { get; set; }
    }

    // --- PLANNING ---
    [Serializable]
    public class PlanningDashboardViewModel
    {
        public decimal CurrentMoney { get; set; }
        public int CurrentPopularity { get; set; } // Critical for estimating customer count
        public decimal ProjectedCost { get; set; }
        
        // New: History for decision making
        public List<DailySalesHistoryModel> RecentHistory { get; set; } = new List<DailySalesHistoryModel>();

        public List<InventoryItemModel> Inventory { get; set; } = new List<InventoryItemModel>();
        public List<StaffModel> Roster { get; set; } = new List<StaffModel>();
        public List<string> Warnings { get; set; } = new List<string>();
    }

    [Serializable]
    public class DailySalesHistoryModel
    {
        public int DayNumber { get; set; }
        public int CustomersArrived { get; set; }
        
        public int CoffeeSold { get; set; }
        public int CoffeeWasted { get; set; }
        
        public int BakedSold { get; set; }
        public int BakedWasted { get; set; }
        
        public int MerchSold { get; set; }
    }

    [Serializable]
    public class InventoryItemModel
    {
        public ProductType Type { get; set; }
        public string Name { get; set; }
        public int CurrentQuantity { get; set; }
        public int PlannedPurchase { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalCost { get; set; }
    }

    [Serializable]
    public class StaffModel
    {
        public string BirdId { get; set; }
        public string Name { get; set; }
        public bool IsWorking { get; set; }
        public string StatusText { get; set; } // "Ready", "Resting", "Sick"
        public bool CanWork { get; set; }
    }
}