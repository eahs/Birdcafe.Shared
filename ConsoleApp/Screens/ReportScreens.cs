
using System;
using BirdCafe.Shared;

namespace BirdCafe.ConsoleApp.Screens
{
    public static class ReportScreens
    {
        public static void ShowWeeklyReport()
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
            Console.ReadLine();
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
            Console.ReadLine();
            BirdCafeGame.Instance.ReturnToMainMenu();
        }
    }
}