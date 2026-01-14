
using BirdCafe.Shared;
using System;
using System.Threading;

namespace BirdCafe.ConsoleApp.Screens
{
    public static class SimulationScreens
    {
        public static void ShowDayIntro()
        {
            Console.Clear();
            var vm = BirdCafeGame.Instance.GetDayIntro();

            Console.WriteLine("#########################################");
            Console.WriteLine($"   START OF DAY {vm.DayNumber}: {vm.DayName}");
            // Junior Dev Note: Added Cafe Name per UI request
            Console.WriteLine($"   {vm.CafeName.ToUpper()}");
            Console.WriteLine("#########################################");
            Console.WriteLine($"Popularity: {vm.Popularity}");
            Console.WriteLine($"Message: {vm.Message}");
            Console.WriteLine("\nPress [ENTER] to open the Cafe. ([H] Help)");

            var input = Console.ReadLine();
            if (input?.ToUpper() == "H")
            {
                BirdCafeGame.Instance.FireHelpPopup("Day Intro");
                // Just proceed after help
            }

            // Triggers engine to calculate day
            BirdCafeGame.Instance.StartSimulationPlayback();
        }

        public static void RunSimulationPlayback()
        {
            Console.Clear();
            Console.WriteLine("--- SIMULATION RUNNING ---");
            Console.WriteLine("(Press 'H' to pause for Help)");

            var timeline = BirdCafeGame.Instance.GetDayTimeline();

            // In console, we just print the log with small delays to simulate time
            foreach (var evt in timeline)
            {
                // Check for interruption
                if (Console.KeyAvailable)
                {
                    var k = Console.ReadKey(true);
                    if (char.ToUpper(k.KeyChar) == 'H')
                        BirdCafeGame.Instance.FireHelpPopup("Simulation Paused");
                    if (char.ToUpper(k.KeyChar) == 'C')
                        BirdCafeGame.Instance.FireChatPopup();
                }

                // Updated to use the Formatted Time string (e.g. 7:30 AM) instead of raw seconds
                Console.Write($"[{evt.FormattedTime}] ");

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"{evt.EventType.PadRight(20)} ");
                Console.ResetColor();

                if (!string.IsNullOrEmpty(evt.BirdName) && evt.BirdName != "Unknown")
                    Console.Write($"Bird: {evt.BirdName} | ");

                Console.WriteLine(evt.Description);

                // Fake animation delay
                Thread.Sleep(50);
            }

            Console.WriteLine("\n--- DAY COMPLETE ---");
            Console.WriteLine("Press any key to see results...");
            Console.ReadKey();

            BirdCafeGame.Instance.FinishSimulation();
        }
    }
}