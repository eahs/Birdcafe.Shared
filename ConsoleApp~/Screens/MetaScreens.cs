
using System;
using BirdCafe.Shared;

namespace BirdCafe.ConsoleApp.Screens
{
    public static class MetaScreens
    {
        public static void ShowMainMenu()
        {
            Console.Clear();
            Console.WriteLine("=========================================");
            Console.WriteLine("       B I R D   C A F E   (CLI)         ");
            Console.WriteLine("=========================================");
            Console.WriteLine("1. New Game");
            Console.WriteLine("2. Load Game");
            Console.WriteLine("3. Exit");
            Console.Write("\nSelect Option: ");

            var key = Console.ReadKey();
            
            switch (key.KeyChar)
            {
                case '1':
                    Console.Write("\nEnter Player Name: ");
                    string pName = Console.ReadLine();
                    Console.Write("Enter Cafe Name: ");
                    string cName = Console.ReadLine();
                    BirdCafeGame.Instance.StartNewGame(pName, cName);
                    break;
                case '2':
                    // Facade switches screen to LoadGame, next loop handles it
                    BirdCafeGame.Instance.LoadGame("dummy_id"); // Mock transition
                    break;
                case '3':
                    Environment.Exit(0);
                    break;
            }
        }

        public static void ShowLoadGame()
        {
            Console.Clear();
            Console.WriteLine("--- LOAD GAME ---");
            var slots = BirdCafeGame.Instance.GetSaveSlots();
            
            if (slots.Count == 0)
            {
                Console.WriteLine("No save files found (Mock). Starting new game instead...");
                Console.ReadKey();
                BirdCafeGame.Instance.StartNewGame("ConsoleUser", "ConsoleCafe");
                return;
            }

            // Logic to pick slot would go here
        }
    }
}