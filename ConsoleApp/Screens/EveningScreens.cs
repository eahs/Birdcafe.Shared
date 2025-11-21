
using System;
using System.Linq;
using BirdCafe.Shared;
using BirdCafe.Shared.Enums;

namespace BirdCafe.ConsoleApp.Screens
{
    /// <summary>
    /// Handles the UI for Evening activities: Care and Planning.
    /// Refactored to avoid infinite loop traps in rendering logic.
    /// </summary>
    public static class EveningScreens
    {
        public static void ShowDailySummary()
        {
            Console.Clear();
            var vm = BirdCafeGame.Instance.GetDailyReport();

            Console.WriteLine("=== EVENING REPORT ===");
            Console.WriteLine($"Day: {vm.DayNumber} | Popularity: {vm.CurrentPopularity}/100");
            Console.WriteLine($"{vm.PopularityNarrative}");
            Console.WriteLine($"Revenue:   ${vm.TotalRevenue:F2}");
            Console.WriteLine($"Net Profit: ${vm.NetProfit:F2}");
            
            // Detailed breakdown of customers
            Console.WriteLine($"\nTraffice: Served: {vm.CustomersServed} customers | Lost: {vm.CustomersLost} customers");
            if (vm.CustomersLost > 0)
            {
                Console.WriteLine($"  -> Walked out (Wait): {vm.LostWaitTooLong}");
                Console.WriteLine($"  -> Walked out (Stock): {vm.LostNoStock}");
            }

            // Breakdown of sales
            Console.WriteLine("\n-- Sales Breakdown --");
            Console.WriteLine($"Coffee: {vm.CoffeeSold}");
            Console.WriteLine($"Baked Goods: {vm.BakedSold}");
            Console.WriteLine($"Merch: {vm.MerchSold}");
            
            Console.WriteLine("\n-- Bird Performance --");
            foreach(var b in vm.Birds)
            {
                Console.WriteLine($"- {b.Name}: Served {b.CustomersServed} {(b.BecameSick ? "[GOT SICK!]" : "")}");
            }

            Console.WriteLine("\nPress any key to continue to Care...");
            Console.ReadKey();
            BirdCafeGame.Instance.AcknowledgeSummary();
        }

        /// <summary>
        /// Displays the Care Dashboard.
        /// Uses a separated Render/Input loop for clarity.
        /// </summary>
        public static void ShowCareDashboard()
        {
            bool stayOnScreen = true;
            while (stayOnScreen)
            {
                RenderCareDashboard();
                stayOnScreen = HandleCareInput();
            }
        }

        private static void RenderCareDashboard()
        {
            Console.Clear();
            var vm = BirdCafeGame.Instance.GetCareDashboard();
            
            Console.WriteLine($"=== BIRD CARE (Funds: ${vm.CurrentMoney:F2}) | Pop: {vm.CurrentPopularity} ===");
            Console.WriteLine("ID | Name           | Hunger | Energy | Health | Mood | Status");
            Console.WriteLine("---|----------------|--------|--------|--------|------|-------");
            
            foreach (var b in vm.Birds)
            {
                string status = b.IsSick ? "SICK" : "OK";
                if (b.WillRestTomorrow) status += " (REST)";
                
                Console.WriteLine($"{b.Id.Substring(0,2)} | {b.Name.PadRight(14)} | {b.Hunger,6} | {b.Energy,6} | {b.Health,6} | {b.Mood,4} | {status}");
            }

            Console.WriteLine("\n[N] Next Phase (Planning)");
            Console.WriteLine("[Enter ID] to interact with a bird");
            Console.Write("> ");
        }

        private static bool HandleCareInput()
        {
            string input = Console.ReadLine().Trim();
            if (input.ToUpper() == "N")
            {
                BirdCafeGame.Instance.GoToPlanning();
                return false; // Exit loop
            }

            var vm = BirdCafeGame.Instance.GetCareDashboard();
            var bird = vm.Birds.FirstOrDefault(b => b.Id.StartsWith(input));
            if (bird != null) 
            {
                InteractWithBird(bird.Id);
            }
            return true; // Stay in loop
        }

        private static void InteractWithBird(string birdId)
        {
            Console.WriteLine("\nFetching actions...");
            var actions = BirdCafeGame.Instance.GetAvailableActions(birdId);
            
            Console.WriteLine("Available Actions:");
            for (int i = 0; i < actions.Count; i++)
            {
                var a = actions[i];
                string costColor = a.IsAffordable ? "" : "(EXPENSIVE)";
                Console.WriteLine($"{i+1}. {a.Label} (${a.Cost}) {costColor}");
            }
            Console.WriteLine("R. Toggle Rest Next Day");
            Console.WriteLine("C. Cancel");

            var key = Console.ReadKey();
            if (key.Key == ConsoleKey.R) 
            {
                BirdCafeGame.Instance.ToggleRest(birdId);
            }
            else if (char.IsDigit(key.KeyChar))
            {
                int idx = int.Parse(key.KeyChar.ToString()) - 1;
                if (idx >= 0 && idx < actions.Count) 
                {
                    BirdCafeGame.Instance.PerformCare(birdId, actions[idx].ActionId);
                }
            }
        }

        /// <summary>
        /// Displays the Planning Dashboard.
        /// Separates render and input logic.
        /// </summary>
        public static void ShowPlanning()
        {
            bool stayOnScreen = true;
            while (stayOnScreen)
            {
                RenderPlanning();
                stayOnScreen = HandlePlanningInput();
            }
        }

        private static void RenderPlanning()
        {
            Console.Clear();
            var vm = BirdCafeGame.Instance.GetPlanningDashboard();

            Console.WriteLine("=== PREPARE FOR TOMORROW ===");
            Console.WriteLine($"Funds: ${vm.CurrentMoney:F2}  |  Popularity: {vm.CurrentPopularity}  |  Projected Cost: ${vm.ProjectedCost:F2}");

            if (vm.Warnings.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                foreach(var w in vm.Warnings) Console.WriteLine($"Warning: {w}");
                Console.ResetColor();
            }

            // --- HISTORY TABLE (Added back per requirements) ---
            if (vm.RecentHistory.Count > 0)
            {
                Console.WriteLine("\n--- RECENT SALES HISTORY ---");
                Console.WriteLine("Day | Traff | Coffee (S/W) | Baked (S/W) | Merch");
                Console.WriteLine("----|-------|--------------|-------------|------");
                foreach(var h in vm.RecentHistory)
                {
                    Console.WriteLine($"{h.DayNumber,3} | {h.CustomersArrived,5} | {h.CoffeeSold,3} / {h.CoffeeWasted,3}    | {h.BakedSold,3} / {h.BakedWasted,3}   | {h.MerchSold,4}");
                }
            }
            else
            {
                Console.WriteLine("\n(No history available yet)");
            }

            // Render Inventory Table
            Console.WriteLine("\n--- INVENTORY ---");
            for(int i=0; i<vm.Inventory.Count; i++)
            {
                var item = vm.Inventory[i];
                Console.WriteLine($"{i+1}. {item.Name}: Have {item.CurrentQuantity} | Buy {item.PlannedPurchase} (${item.TotalCost:F2})");
            }

            // Render Roster
            Console.WriteLine("\n--- ROSTER ---");
            for(int i=0; i<vm.Roster.Count; i++)
            {
                var bird = vm.Roster[i];
                string check = bird.IsWorking ? "[X]" : "[ ]";
                Console.WriteLine($"{i+4}. {check} {bird.Name} ({bird.StatusText})");
            }

            Console.WriteLine("\n[S] START DAY");
            Console.Write("> ");
        }

        private static bool HandlePlanningInput()
        {
            var key = Console.ReadKey().KeyChar;
            if (key == 's' || key == 'S')
            {
                return !BirdCafeGame.Instance.FinalizeDay(); // If success, return false (exit loop)
            }

            if (key == '1') ChangeInventory(ProductType.Coffee);
            if (key == '2') ChangeInventory(ProductType.BakedGoods);
            if (key == '3') ChangeInventory(ProductType.ThemedMerch);
            
            if (char.IsDigit(key))
            {
                var vm = BirdCafeGame.Instance.GetPlanningDashboard();
                int index = int.Parse(key.ToString()) - 4;
                if (index >= 0 && index < vm.Roster.Count)
                {
                    var b = vm.Roster[index];
                    BirdCafeGame.Instance.SetStaffStatus(b.BirdId, !b.IsWorking);
                }
            }
            return true;
        }

        private static void ChangeInventory(ProductType type)
        {
            Console.Write($"\nSet quantity for {type}: ");
            if (int.TryParse(Console.ReadLine(), out int qty))
            {
                BirdCafeGame.Instance.SetInventory(type, qty);
            }
        }
    }
}