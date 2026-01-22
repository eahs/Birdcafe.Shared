
using BirdCafe.Shared;
using BirdCafe.Shared.ViewModels;
using System;

namespace BirdCafe.ConsoleApp
{
    class Program
    {
        // State tracking for the console loop
        private static bool _isRunning = true;
        private static GameScreen _currentScreen = GameScreen.MainMenu;
        private static GameScreen _previousScreen = GameScreen.MainMenu;

        static void Main(string[] args)
        {
            Console.Title = "Bird Cafe - Console Edition";

            // 1. Setup Hooks
            BirdCafeGame.Instance.OnScreenChanged += HandleScreenChange;

            BirdCafeGame.Instance.OnToastMessage += (msg) =>
            {
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n>>> ALERT: {msg} <<<\n");
                Console.ForegroundColor = oldColor;
                Console.WriteLine("(Press any key to continue...)");
                Console.ReadKey();
            };

            BirdCafeGame.Instance.OnHelpPopup += (msg) =>
            {
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n>>> HELP: {msg} <<<\n");
                Console.ForegroundColor = oldColor;
                Console.WriteLine("(Press any key to close help...)");
                Console.ReadKey();
            };

            BirdCafeGame.Instance.OnChatPopup += () =>
            {
                // In console, we handle this by treating Chat as a distinct screen
                // We save the previous screen to return to it later if we wanted a stack,
                // but for now the chat exits back to the current game state via flow logic or we handle it here.
                // Actually, the easiest way for Console is to just manually invoke the screen
                // then redraw the current screen when done.
                
                Screens.ChatScreens.ShowChatScreen();
                // After chat loop finishes, we just let the main loop redraw the current screen state
            };

            BirdCafeGame.Instance.OnMoneyChanged += (amount) =>
            {
                // In a console app, we can't easily update a header in real-time 
                // without clearing screen, so we ignore this for now or log it.
            };

            // 2. Game Loop
            while (_isRunning)
            {
                try
                {
                    DrawCurrentScreen();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"CRITICAL ERROR: {ex.Message}");
                    Console.WriteLine(ex.StackTrace);
                    Console.ResetColor();
                    Console.ReadLine();
                    _isRunning = false;
                }
            }
        }

        private static void HandleScreenChange(GameScreen newScreen)
        {
            _previousScreen = _currentScreen;
            _currentScreen = newScreen;
        }

        private static void DrawCurrentScreen()
        {
            // Route to specific screen logic
            switch (_currentScreen)
            {
                case GameScreen.MainMenu:
                    Screens.MetaScreens.ShowMainMenu();
                    break;
                case GameScreen.LoadGame:
                    Screens.MetaScreens.ShowLoadGame();
                    break;
                case GameScreen.Tutorial:
                    Screens.TutorialScreens.ShowTutorial();
                    break;
                case GameScreen.DayIntro:
                    Screens.SimulationScreens.ShowDayIntro();
                    break;
                case GameScreen.DaySimulation:
                    Screens.SimulationScreens.RunSimulationPlayback();
                    break;
                case GameScreen.EveningSummary:
                    Screens.EveningScreens.ShowDailySummary();
                    break;
                case GameScreen.EveningCare:
                    Screens.EveningScreens.ShowCareDashboard();
                    break;
                case GameScreen.EveningPlanning:
                    Screens.EveningScreens.ShowPlanning();
                    break;
                case GameScreen.WeeklySummary:
                    Screens.ReportScreens.ShowWeeklyReport();
                    break;
                case GameScreen.GameOver:
                    Screens.ReportScreens.ShowGameOver();
                    break;
                case GameScreen.Chat:
                    // If state was somehow set to chat directly
                    Screens.ChatScreens.ShowChatScreen();
                    // Restore previous screen state logic manually or by just relying on game controller state
                    // (Since Chat isn't a "GamePhase" in the engine controller, the engine is likely still in DayLoop/Evening etc)
                    // We just loop back.
                    break;
            }
        }
    }
}