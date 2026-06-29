using BirdCafe.Shared.Enums;
using BirdCafe.Shared.ViewModels;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace BirdCafe.Shared.Tests
{
    public class MinigameFlowTests
    {
        private BirdCafeGame _game;
        private string _birdId;

        [SetUp]
        public void Setup()
        {
            _game = BirdCafeGame.Instance;
            _game.StartNewGame("MiniTester", "Cafe");

            while (_game.HasActiveMinigame())
            {
                _game.CancelCurrentMinigame();
            }

            _birdId = _game.Controller.CurrentState.Birds.First().Id;
            _game.Controller.SetPhaseForTests(GamePhase.EveningLoop);
            _game.GoToCare();
        }

        [Test]
        public void TryStartMinigame_ValidBirdDuringEveningLoop_StartsSession()
        {
            var started = _game.TryStartMinigame(MinigameId.Flappy, _birdId);

            Assert.IsTrue(started);
            Assert.IsTrue(_game.HasActiveMinigame());
            Assert.AreEqual(GameScreen.Minigame, _game.CurrentScreen);

            var session = _game.GetCurrentMinigameSession();
            Assert.NotNull(session);
            Assert.AreEqual(MinigameId.Flappy, session.Minigame);
            Assert.AreEqual(_birdId, session.BirdId);
        }

        [Test]
        public void TryStartMinigame_InvalidBird_Fails()
        {
            var started = _game.TryStartMinigame(MinigameId.Flappy, "missing-bird");

            Assert.IsFalse(started);
            Assert.IsFalse(_game.HasActiveMinigame());
        }

        [Test]
        public void TryStartMinigame_WrongPhase_Fails()
        {
            _game.Controller.SetPhaseForTests(GamePhase.DayLoop);

            var started = _game.TryStartMinigame(MinigameId.Flappy, _birdId);

            Assert.IsFalse(started);
            Assert.IsFalse(_game.HasActiveMinigame());
        }

        [Test]
        public void TryStartMinigame_SecondStartWhileActive_Fails()
        {
            var first = _game.TryStartMinigame(MinigameId.Flappy, _birdId);
            var second = _game.TryStartMinigame(MinigameId.TimingBarGame, _birdId);

            Assert.IsTrue(first);
            Assert.IsFalse(second);
            Assert.IsTrue(_game.HasActiveMinigame());

            var session = _game.GetCurrentMinigameSession();
            Assert.AreEqual(MinigameId.Flappy, session.Minigame);
            Assert.AreEqual(_birdId, session.BirdId);
        }

        [Test]
        public void SetDefaultCareMinigame_TimingBarGame_IsUsedByPlayCareLaunch()
        {
            Assert.IsTrue(_game.SetDefaultCareMinigame(MinigameId.TimingBarGame));

            var started = _game.TryStartCareMinigame(_birdId, CareActionIds.Play);

            Assert.IsTrue(started);
            var session = _game.GetCurrentMinigameSession();
            Assert.NotNull(session);
            Assert.AreEqual(MinigameId.TimingBarGame, session.Minigame);
        }

        [Test]
        public void CompleteCurrentMinigame_SuccessStatus_AppliesPlayRewardExactlyOnce()
        {
            var bird = _game.Controller.CurrentState.Birds.First(b => b.Id == _birdId);
            bird.Mood = 10;
            bird.Energy = 50;
            bird.Stress = 50;

            var started = _game.TryStartCareMinigame(_birdId, CareActionIds.Play);
            Assert.IsTrue(started);

            var beforeMood = bird.Mood;
            var beforeEnergy = bird.Energy;
            var beforeStress = bird.Stress;

            var completed = _game.CompleteCurrentMinigame(new MinigameCompletionViewModel
            {
                Status = MinigameCompletionStatus.Success,
                Score = 100,
                ResultMessage = "Success"
            });

            Assert.IsTrue(completed);
            Assert.AreEqual(beforeMood + 20f, bird.Mood);
            Assert.AreEqual(beforeEnergy - 5f, bird.Energy);
            Assert.AreEqual(beforeStress - 10f, bird.Stress);
            Assert.IsFalse(_game.HasActiveMinigame());
            Assert.IsNull(_game.GetCurrentMinigameSession());
            Assert.AreEqual(GameScreen.EveningCare, _game.CurrentScreen);
        }

        [Test]
        public void CompleteCurrentMinigame_Failure_DoesNotApplyReward()
        {
            var bird = _game.Controller.CurrentState.Birds.First(b => b.Id == _birdId);
            bird.Mood = 10;
            bird.Energy = 50;
            bird.Stress = 50;

            Assert.IsTrue(_game.TryStartCareMinigame(_birdId, CareActionIds.Play));

            var completed = _game.CompleteCurrentMinigame(new MinigameCompletionViewModel
            {
                Status = MinigameCompletionStatus.Failure,
                Score = 5,
                ResultMessage = "Failed"
            });

            Assert.IsTrue(completed);
            Assert.AreEqual(10f, bird.Mood);
            Assert.AreEqual(50f, bird.Energy);
            Assert.AreEqual(50f, bird.Stress);
            Assert.IsFalse(_game.HasActiveMinigame());
            Assert.AreEqual(GameScreen.EveningCare, _game.CurrentScreen);
        }

        [Test]
        public void CancelCurrentMinigame_DoesNotApplyReward()
        {
            var bird = _game.Controller.CurrentState.Birds.First(b => b.Id == _birdId);
            bird.Mood = 10;
            bird.Energy = 50;
            bird.Stress = 50;

            Assert.IsTrue(_game.TryStartCareMinigame(_birdId, CareActionIds.Play));

            var cancelled = _game.CancelCurrentMinigame();

            Assert.IsTrue(cancelled);
            Assert.AreEqual(10f, bird.Mood);
            Assert.AreEqual(50f, bird.Energy);
            Assert.AreEqual(50f, bird.Stress);
            Assert.IsFalse(_game.HasActiveMinigame());
            Assert.AreEqual(GameScreen.EveningCare, _game.CurrentScreen);
        }

        [Test]
        public void CompleteCurrentMinigame_SuccessWhenRewardFails_StillExitsMinigameFlow()
        {
            var bird = _game.Controller.CurrentState.Birds.First(b => b.Id == _birdId);
            bird.Mood = 10;
            bird.Energy = 50;
            bird.Stress = 50;

            Assert.IsTrue(_game.TryStartCareMinigame(_birdId, CareActionIds.Play));
            _game.Controller.SetPhaseForTests(GamePhase.DayLoop);

            var toasts = new List<string>();
            void HandleToast(string message) => toasts.Add(message);
            _game.OnToastMessage += HandleToast;

            try
            {
                var completed = _game.CompleteCurrentMinigame(new MinigameCompletionViewModel
                {
                    Status = MinigameCompletionStatus.Success,
                    Score = 100,
                    ResultMessage = "Success"
                });

                Assert.IsTrue(completed);
                Assert.IsFalse(_game.HasActiveMinigame());
                Assert.AreEqual(GameScreen.EveningCare, _game.CurrentScreen);
                Assert.AreEqual(10f, bird.Mood);
                Assert.AreEqual(50f, bird.Energy);
                Assert.AreEqual(50f, bird.Stress);
                Assert.IsTrue(toasts.Any(t => t.Contains("reward could not be granted")));
            }
            finally
            {
                _game.OnToastMessage -= HandleToast;
            }
        }

        [Test]
        public void StartNewGame_ClearsAnyActiveMinigameSession()
        {
            Assert.IsTrue(_game.TryStartMinigame(MinigameId.Flappy, _birdId));
            Assert.IsTrue(_game.HasActiveMinigame());

            _game.StartNewGame("MiniTester2", "Cafe2");

            Assert.IsFalse(_game.HasActiveMinigame());
            Assert.IsNull(_game.GetCurrentMinigameSession());
        }

        [Test]
        public void LoadGame_ClearsAnyActiveMinigameSession()
        {
            Assert.IsTrue(_game.TryStartMinigame(MinigameId.Flappy, _birdId));
            Assert.IsTrue(_game.HasActiveMinigame());

            _game.LoadGame("slot-1");

            Assert.IsFalse(_game.HasActiveMinigame());
            Assert.IsNull(_game.GetCurrentMinigameSession());
        }

        [Test]
        public void ReturnToMainMenu_ClearsAnyActiveMinigameSession()
        {
            Assert.IsTrue(_game.TryStartMinigame(MinigameId.Flappy, _birdId));
            Assert.IsTrue(_game.HasActiveMinigame());

            _game.ReturnToMainMenu();

            Assert.IsFalse(_game.HasActiveMinigame());
            Assert.IsNull(_game.GetCurrentMinigameSession());
        }

        [Test]
        public void CompleteCurrentMinigame_NoActiveSession_FailsSafely()
        {
            var screenBefore = _game.CurrentScreen;

            var completed = _game.CompleteCurrentMinigame(new MinigameCompletionViewModel
            {
                Status = MinigameCompletionStatus.Success
            });

            Assert.IsFalse(completed);
            Assert.IsFalse(_game.HasActiveMinigame());
            Assert.AreEqual(screenBefore, _game.CurrentScreen);
        }

        [Test]
        public void CancelCurrentMinigame_NoActiveSession_FailsSafely()
        {
            var screenBefore = _game.CurrentScreen;

            var cancelled = _game.CancelCurrentMinigame();

            Assert.IsFalse(cancelled);
            Assert.IsFalse(_game.HasActiveMinigame());
            Assert.AreEqual(screenBefore, _game.CurrentScreen);
        }
    }
}
