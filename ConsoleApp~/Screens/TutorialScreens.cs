
using System;
using BirdCafe.Shared;

namespace BirdCafe.ConsoleApp.Screens
{
    public static class TutorialScreens
    {
        public static void ShowTutorial()
        {
            Console.Clear();
            var vm = BirdCafeGame.Instance.GetTutorialContent();

            Console.WriteLine("=========================================");
            Console.WriteLine($"   {vm.Title.ToUpper()}   ");
            Console.WriteLine("=========================================");
            
            foreach(var step in vm.Steps)
            {
                Console.WriteLine($"\n{step.Title}");
                Console.WriteLine(step.Description);
            }

            Console.WriteLine("\n[Got it] (Press Enter)");
            Console.ReadLine();
            
            BirdCafeGame.Instance.CompleteTutorial();
        }
    }
}