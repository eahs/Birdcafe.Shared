
using BirdCafe.Shared;
using BirdCafe.Shared.Engine.Managers;
using BirdCafe.Shared.Models.Economy;
using BirdCafe.Shared.Enums;
using System;
using System.Dynamic;
using System.Linq;
using BirdCafe.Shared.Models.Birds;
using System.Collections.Generic;
using BirdCafe.Shared.Engine.Utils;

namespace BirdCafe.ConsoleApp.Screens
{
    /// <summary>
    /// Handles the UI for Evening activities: Care and Planning.
    /// Refactored to avoid infinite loop traps in rendering logic.
    /// </summary>
    public static class EveningScreens
    {
        public static void ShowHub()
        {
            bool stayOnScreen = true;
            while (stayOnScreen)
            {
                Console.Clear();
                var hub = BirdCafeGame.Instance.GetEveningHub();
                Console.WriteLine("=========================================");
                Console.WriteLine($"   EVENING HUB - Day {hub.DayNumber}");
                Console.WriteLine("=========================================");
                Console.WriteLine($"Funds: ${hub.CurrentMoney:F2}  |  Popularity: {hub.CurrentPopularity}");
                Console.WriteLine("-----------------------------------------");
                Console.WriteLine("1. View Daily Summary");
                Console.WriteLine("2. Care for Birds");
                Console.WriteLine("3. Plan Tomorrow's Shop & Roster");
                Console.WriteLine("4. Pete's Pet Store");
                Console.WriteLine("5. Start Next Day (End Evening)");
                Console.WriteLine("\n[H] Help  [C] Chat");
                Console.Write("> ");

                var key = Console.ReadKey().KeyChar;

                if (char.ToUpper(key) == 'H') { BirdCafeGame.Instance.FireHelpPopup("Evening Hub"); }
                else if (char.ToUpper(key) == 'C') { BirdCafeGame.Instance.FireChatPopup(); }
                else if (key == '1') { BirdCafeGame.Instance.GoToSummary(); stayOnScreen = false; }
                else if (key == '2') { BirdCafeGame.Instance.GoToCare(); stayOnScreen = false; }
                else if (key == '3') { BirdCafeGame.Instance.GoToPlanning(); stayOnScreen = false; }
                else if (key == '4') { BirdCafeGame.Instance.GoToPetStore(); stayOnScreen = false; }
                else if (key == '5') 
                { 
                    if (BirdCafeGame.Instance.FinalizeDay())
                    {
                        stayOnScreen = false; 
                    }
                }
            }
        }

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
            Console.WriteLine($"\nTraffic: Served: {vm.CustomersServed} customers | Lost: {vm.CustomersLost} customers");
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
            foreach (var b in vm.Birds)
            {
                Console.WriteLine($"- {b.Name}: Served {b.CustomersServed} {(b.BecameSick ? "[GOT SICK!]" : "")}");
            }

            Console.WriteLine("\nPress any key to return to Hub... ([H] Help, [C] Chat)");

            while (true)
            {
                var k = Console.ReadKey(true);
                if (char.ToUpper(k.KeyChar) == 'H') { BirdCafeGame.Instance.FireHelpPopup("Daily Summary"); return; } 
                if (char.ToUpper(k.KeyChar) == 'C') { BirdCafeGame.Instance.FireChatPopup(); return; }
                break; // Any other key continues
            }

            BirdCafeGame.Instance.GoToHub();
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

            Console.WriteLine($"=== BIRD CARE (Funds: ${vm.CurrentMoney:F2}) | Pop: {vm.CurrentPopularity} | Stored Food: {vm.StoredBirdFoodUnits} ===");
            Console.WriteLine("ID | Name           | Hunger | Energy | Health | Mood | Trust | Friends | Pref Food         | Status");
            Console.WriteLine("---|----------------|--------|--------|--------|------|-------|---------|-------------------|-------");

            foreach (var b in vm.Birds)
            {
                string status = b.IsSick ? "SICK" : "OK";
                if (b.WillRestTomorrow) status += " (REST)";

                Console.WriteLine($"{b.Id.Substring(0, 2)} | {b.Name.PadRight(14)} | {b.Hunger,6} | {b.Energy,6} | {b.Health,6} | {b.Mood,4} | {b.Trust,5} | {b.FriendshipCount,7} | {b.PreferredFoodsText.PadRight(17)} | {status}");
            }

            Console.WriteLine("\n[B] Back to Hub");
            Console.WriteLine("[Enter ID] to interact with a bird");
            Console.WriteLine("[I] View Inventory");
            Console.WriteLine("[H] Help  [C] Chat");
            Console.Write("> ");
        }

        private static bool HandleCareInput()
        {
            string input = Console.ReadLine().Trim();

            if (input.ToUpper() == "H") { BirdCafeGame.Instance.FireHelpPopup("Bird Care"); return true; }
            if (input.ToUpper() == "C") { BirdCafeGame.Instance.FireChatPopup(); return true; }
            if (input.ToUpper() == "I")
            {
                Console.WriteLine("Displaying Current Inventories:");
                ShowInventory();
            } 
            if (input.ToUpper() == "B")
            {
                BirdCafeGame.Instance.GoToHub();
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
                string costColor = a.IsAffordable ? "" : "(UNAVAILABLE)";
                Console.WriteLine($"{i + 1}. {a.Label} (${a.Cost}) {costColor}");
            }
            Console.WriteLine("R. Toggle Rest Next Day");
            Console.WriteLine("C. Cancel (Or global Chat via main menu)");

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
        /// Displays supplies, costumes, and toys owned by the player.
        /// </summary>
        public static void ShowInventory()
        {
            Console.Clear();
            var vm = BirdCafeGame.Instance.GetInventory();
            
            Console.WriteLine("=== CURRENT INVENTORY ===");
            Console.WriteLine("\n-- Bird Food --");
            foreach (var food in vm.BirdFoodByType)
            {
                String name = "";
                List<PetStoreSupplyDefinition> catalog = PetStoreCatalog.SupplyOffers;
                foreach (PetStoreSupplyDefinition item in catalog)
                {
                    if (food.Key.ToString() == item.ItemId)
                        name = item.DisplayName;
                }
                Console.WriteLine($"{name}: {food.Value} units");
            }

            Console.WriteLine("\n-- Toys --");
            foreach (var toy in vm.OwnedToyQuantities)
            {
                String name = "";
                List<PetStoreSupplyDefinition> catalog = PetStoreCatalog.SupplyOffers;
                foreach (PetStoreSupplyDefinition item in catalog)
                {
                    if (toy.Key.ToString() == item.ItemId)
                        name = item.DisplayName;
                }
                Console.WriteLine($"{name}: {toy.Value} owned");
            }

            Console.WriteLine("\n-- Costumes --");
            foreach (var costume in vm.OwnedCostumeQuantities)
            {
                String name = "";
                List<PetStoreSupplyDefinition> catalog = PetStoreCatalog.SupplyOffers;
                foreach (PetStoreSupplyDefinition item in catalog)
                {
                    if (costume.Key.ToString() == item.ItemId)
                        name = item.DisplayName;
                }
                Console.WriteLine($"{name}: {costume.Value} owned");
            }
            */
            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
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
                foreach (var w in vm.Warnings) Console.WriteLine($"Warning: {w}");
                Console.ResetColor();
            }

            // --- HISTORY TABLE ---
            if (vm.RecentHistory.Count > 0)
            {
                Console.WriteLine("\n--- RECENT SALES HISTORY ---");
                Console.WriteLine("Day | Traff | Coffee (S/W) | Baked (S/W) | Merch");
                Console.WriteLine("----|-------|--------------|-------------|------");
                foreach (var h in vm.RecentHistory)
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
            for (int i = 0; i < vm.Inventory.Count; i++)
            {
                var item = vm.Inventory[i];
                Console.WriteLine($"{i + 1}. {item.Name}: Have {item.CurrentQuantity} | Buy {item.PlannedPurchase} (${item.TotalCost:F2})");
            }

            // Render Roster
            Console.WriteLine("\n--- ROSTER ---");
            for (int i = 0; i < vm.Roster.Count; i++)
            {
                var bird = vm.Roster[i];
                string check = bird.IsWorking ? "[X]" : "[ ]";
                Console.WriteLine($"{i + 4}. {check} {bird.Name} ({bird.StatusText})");
            }

            Console.WriteLine("\n[B] Back to Hub  |  [S] START DAY");
            Console.WriteLine("[H] Help  [C] Chat");
            Console.Write("> ");
        }

        private static bool HandlePlanningInput()
        {
            var key = Console.ReadKey().KeyChar;

            if (char.ToUpper(key) == 'H') { BirdCafeGame.Instance.FireHelpPopup("Planning"); return true; }
            if (char.ToUpper(key) == 'C') { BirdCafeGame.Instance.FireChatPopup(); return true; }

            if (char.ToUpper(key) == 'B') 
            {
                BirdCafeGame.Instance.GoToHub();
                return false;
            }

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

        public static void ShowPetStore()
        {
            bool stayOnScreen = true;
            while (stayOnScreen)
            {
                Console.Clear();
                var vm = BirdCafeGame.Instance.GetPetStoreDashboard();
                Console.WriteLine("=== PETE'S PET STORE ===");
                Console.WriteLine($"Funds: ${vm.CurrentMoney:F2} | Birds Owned: {vm.OwnedBirdCount}");
                Console.WriteLine($"Bird Food: {vm.BirdFoodUnits} | Special Egg Toys: {vm.SpecialEggToysOwned}");
                Console.WriteLine(vm.LastEggRewardText);
                Console.WriteLine("\n1. Buy Birds");
                Console.WriteLine("2. Buy Supplies");
                Console.WriteLine("B. Back to Hub");
                Console.Write("> ");

                var key = Console.ReadKey().KeyChar;
                if (key == 'P') { BirdCafeGame.Instance.AddMoney(1000); } // Money cheat for testing
                else if (char.ToUpper(key) == 'B') { BirdCafeGame.Instance.GoToHub(); stayOnScreen = false; }
                else if (key == '1') { BirdCafeGame.Instance.GoToPetStoreBirds(); stayOnScreen = false; }
                else if (key == '2') { BirdCafeGame.Instance.GoToPetStoreSupplies(); stayOnScreen = false; }
            }
        }

        public static void ShowPetStoreBirds()
        {
            bool stayOnScreen = true;
            while (stayOnScreen)
            {
                Console.Clear();
                var offers = BirdCafeGame.Instance.GetPetStoreBirdOffers();
                Console.WriteLine("=== PETE'S PET STORE / BUY BIRDS ===");
                for (int i = 0; i < offers.Count; i++)
                {
                    var offer = offers[i];
                    Console.WriteLine($"{i + 1}. {offer.Name} [{offer.RarityText}] - ${offer.Price:F2}");
                    Console.WriteLine($"   {offer.EffectText} {(offer.IsAffordable ? "" : "[Cannot Afford]")}");
                }
                Console.WriteLine("B. Back");
                Console.Write("> ");
                var key = Console.ReadKey().KeyChar;
                if (char.ToUpper(key) == 'B') { BirdCafeGame.Instance.GoToPetStore(); stayOnScreen = false; }
                else if (char.IsDigit(key))
                {
                    int idx = int.Parse(key.ToString()) - 1;
                    if (idx >= 0 && idx < offers.Count)
                    {
                        BirdCafeGame.Instance.BuyPetStoreBird(offers[idx].SpeciesId);
                    }
                }
            }
        }

        public static void ShowPetStoreSupplies()
        {
            bool stayOnScreen = true;
            while (stayOnScreen)
            {
                Console.Clear();
                var offers = BirdCafeGame.Instance.GetPetStoreSupplyOffers();
                Console.WriteLine("=== PETE'S PET STORE / BUY SUPPLIES ===");

                string currentCategory = null;
                for (int i = 0; i < offers.Count; i++)
                {
                    var offer = offers[i];
                    if (offer.CategoryText != currentCategory)
                    {
                        if (currentCategory != null)
                        {
                            Console.WriteLine();
                        }

                        currentCategory = offer.CategoryText;
                        Console.WriteLine(currentCategory.ToUpperInvariant());
                    }

                    Console.WriteLine($"{i + 1}. {offer.Name} (${offer.Price:F2}) - Owned: {offer.OwnedQuantity}");
                    Console.WriteLine($"   {offer.EffectText} {(offer.IsAffordable ? string.Empty : "[Cannot Afford]")}");
                }

                var specialEggOffer = offers.FirstOrDefault(offer => offer.SupplyType == PetStoreSupplyType.SpecialEggToy);
                if (specialEggOffer != null)
                {
                    Console.WriteLine();
                    Console.WriteLine($"O. Open owned Special Egg Toy (x{specialEggOffer.OwnedQuantity})");
                }

                Console.WriteLine("B. Back");
                Console.Write("> ");

                var key = Console.ReadKey().KeyChar;
                if (char.ToUpper(key) == 'B') { BirdCafeGame.Instance.GoToPetStore(); stayOnScreen = false; }
                else if (char.ToUpper(key) == 'O')
                {
                    var reward = BirdCafeGame.Instance.OpenSpecialEggToy();
                    if (reward.HasReward)
                    {
                        Console.WriteLine($"\nEgg Reward: {reward.RewardName} ({reward.RewardTypeText})");
                        Console.WriteLine(reward.RewardDescription);
                        Console.ReadKey();
                    }
                }
                else if (char.IsDigit(key))
                {
                    int idx = int.Parse(key.ToString()) - 1;
                    if (idx >= 0 && idx < offers.Count)
                    {
                        var selectedOffer = offers[idx];
                        BirdCafeGame.Instance.BuyPetStoreSupply(selectedOffer.ItemId, selectedOffer.SupplyType);
                    }
                }
            }
        }
    }
}
