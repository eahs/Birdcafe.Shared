
using System;
using BirdCafe.Shared;

namespace BirdCafe.ConsoleApp.Screens
{
    public static class ReportScreens
    {
        public static void ShowWeeklyReport()
        {
            // Simple loop to allow help/chat support without progressing
            while(true)
            {
                Console.Clear();
                var vm = BirdCafeGame.Instance.GetWeeklyReport();

                Console.WriteLine("===============================");
                Console.WriteLine($"   WEEKLY REPORT: WEEK {vm.WeekNumber}");
                Console.WriteLine("===============================");
                Console.WriteLine($"Narrative: {vm.Narrative}");
                Console.WriteLine($"Total Profit: ${vm.TotalProfit:F2}");
                Console.WriteLine($"Avg Flock Health: {vm.AvgBirdHealth}/100");

                Console.WriteLine("\nPress [ENTER] to start next week.");
                Console.WriteLine("[H] Help  [C] Chat");
                
                string input = Console.ReadLine();
                if (input?.ToUpper() == "H") 
                { 
                    BirdCafeGame.Instance.FireHelpPopup("Weekly Report"); 
                    continue; 
                }
                if (input?.ToUpper() == "C") 
                { 
                    BirdCafeGame.Instance.FireChatPopup(); 
                    continue; 
                }

                // Any other input (usually empty Enter) proceeds
                break;
            }
            
            BirdCafeGame.Instance.CompleteWeek();
        }

        public static void ShowGameOver()
        {
            Console.Clear();
            var vm = BirdCafeGame.Instance.GetGameOverDetails();

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("===============================");
            Console.WriteLine("        G A M E   O V E R      ");
            Console.WriteLine("===============================");
            Console.ResetColor();
            Console.WriteLine($"Reason: {vm.Reason}");
            Console.WriteLine($"You survived {vm.DaysSurvived} days.");
            Console.WriteLine($"Final Score: ${vm.FinalScore:F2}");

            Console.WriteLine("\nPress [ENTER] to return to menu.");
            Console.WriteLine("[H] Help");

            var input = Console.ReadLine();
             if (input?.ToUpper() == "H") 
            { 
                BirdCafeGame.Instance.FireHelpPopup("Game Over"); 
                // Don't need loop here, user will likely just hit enter again or see menu next
            }

            BirdCafeGame.Instance.ReturnToMainMenu();
        }
    }
}