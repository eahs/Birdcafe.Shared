
using BirdCafe.Shared;
using BirdCafe.Shared.ViewModels;
using System;
using System.Text.RegularExpressions;

namespace BirdCafe.ConsoleApp.Screens
{
    public static class ChatScreens
    {
        public static void ShowChatScreen()
        {
            bool stayingInChat = true;

            while (stayingInChat)
            {
                Console.Clear();
                Console.WriteLine("=========================================");
                Console.WriteLine("       O R A C L E   C H A T             ");
                Console.WriteLine("=========================================");

                var node = BirdCafeGame.Instance.GetCurrentChatNode();

                // Simple parser to strip TMP tags for Console display
                string cleanText = StripTags(node.OracleText);
                Console.WriteLine("\nORACLE: " + cleanText + "\n");
                
                Console.WriteLine("-----------------------------------------");
                for (int i = 0; i < node.Options.Count; i++)
                {
                    var opt = node.Options[i];
                    Console.WriteLine($"{i + 1}. {opt.ResponseText}");
                }
                Console.WriteLine("-----------------------------------------");

                Console.Write("\nChoose an option (or Q to Force Quit Chat): ");
                var key = Console.ReadKey(true);

                if (char.ToUpper(key.KeyChar) == 'Q')
                {
                    stayingInChat = false;
                }
                else if (char.IsDigit(key.KeyChar))
                {
                    int idx = int.Parse(key.KeyChar.ToString()) - 1;
                    if (idx >= 0 && idx < node.Options.Count)
                    {
                        var selected = node.Options[idx];
                        if (selected.IsExit)
                        {
                            stayingInChat = false;
                        }
                        else
                        {
                            BirdCafeGame.Instance.SelectChatOption(idx);
                        }
                    }
                }
            }
        }

        private static string StripTags(string input)
        {
            // Regex to remove <...> tags
            return Regex.Replace(input, "<.*?>", string.Empty);
        }
    }
}