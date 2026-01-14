
using BirdCafe.Shared;
using System;

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
            Console.WriteLine("[H] Help  [C] Chat");
            Console.Write("\nSelect Option: ");

            var key = Console.ReadKey();

            switch (char.ToUpper(key.KeyChar))
            {
                case '1':
                    Console.Write("\nEnter Player Name: ");
                    string pName = Console.ReadLine();
                    Console.Write("Enter Cafe Name: ");
                    string cName = Console.ReadLine();
                    BirdCafeGame.Instance.StartNewGame(pName, cName);
                    break;
                case '2':
                    BirdCafeGame.Instance.LoadGame("dummy_id");
                    break;
                case '3':
                    Environment.Exit(0);
                    break;
                case 'H':
                    BirdCafeGame.Instance.FireHelpPopup("Main Menu Help");
                    break;
                case 'C':
                    BirdCafeGame.Instance.FireChatPopup();
                    break;
            }
        }

        public static void ShowLoadGame()
        {
            Console.Clear();
            Console.WriteLine("--- LOAD GAME ---");
            Console.WriteLine("[H] Help  [C] Chat");

            var slots = BirdCafeGame.Instance.GetSaveSlots();

            if (slots.Count == 0)
            {
                Console.WriteLine("No save files found (Mock). Starting new game instead...");
                Console.ReadKey();
                BirdCafeGame.Instance.StartNewGame("ConsoleUser", "ConsoleCafe");
                return;
            }

            // Just basic support for global keys here before logic
            var key = Console.ReadKey(true);
            if (char.ToUpper(key.KeyChar) == 'H') { BirdCafeGame.Instance.FireHelpPopup("Load Game Help"); return; }
            if (char.ToUpper(key.KeyChar) == 'C') { BirdCafeGame.Instance.FireChatPopup(); return; }
        }
    }
}