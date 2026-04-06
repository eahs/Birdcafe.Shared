using BirdCafe.Shared;
using BirdCafe.Shared.Enums;
using BirdCafe.Shared.ViewModels;
using NUnit.Framework;
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
        public void TryStartCareMinigame_Play_UsesConfiguredDefaultMinigame()
        {
            var started = _game.TryStartCareMinigame(_birdId, CareActionIds.Play);

            Assert.IsTrue(started);
            var session = _game.GetCurrentMinigameSession();
            Assert.NotNull(session);
            Assert.AreEqual(CareActionIds.Play, session.PendingRewardActionId);
            Assert.IsTrue(session.WasStartedFromCare);
            Assert.AreEqual(MinigameId.Flappy, session.Minigame);
        }

        [Test]
        public void CompleteCurrentMinigame_Success_AppliesPlayRewardExactlyOnce()
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
            var returnScreen = _game.GetCurrentMinigameSession().ReturnScreen;

            var completed = _game.CompleteCurrentMinigame(new MinigameCompletionViewModel
            {
                Status = MinigameCompletionStatus.Success,
                WasSuccessful = true,
                Score = 100,
                ResultMessage = "Success"
            });

            Assert.IsTrue(completed);
            Assert.AreEqual(beforeMood + 20f, bird.Mood);
            Assert.AreEqual(beforeEnergy - 5f, bird.Energy);
            Assert.AreEqual(beforeStress - 10f, bird.Stress);
            Assert.IsFalse(_game.HasActiveMinigame());
            Assert.IsNull(_game.GetCurrentMinigameSession());
            Assert.AreEqual(returnScreen, _game.CurrentScreen);
        }

        [Test]
        public void CompleteCurrentMinigame_Failure_DoesNotApplyReward()
        {
            var bird = _game.Controller.CurrentState.Birds.First(b => b.Id == _birdId);
            bird.Mood = 10;
            bird.Energy = 50;
            bird.Stress = 50;

            Assert.IsTrue(_game.TryStartCareMinigame(_birdId, CareActionIds.Play));
            var returnScreen = _game.GetCurrentMinigameSession().ReturnScreen;

            var completed = _game.CompleteCurrentMinigame(new MinigameCompletionViewModel
            {
                Status = MinigameCompletionStatus.Failure,
                WasSuccessful = false,
                Score = 5,
                ResultMessage = "Failed"
            });

            Assert.IsTrue(completed);
            Assert.AreEqual(10f, bird.Mood);
            Assert.AreEqual(50f, bird.Energy);
            Assert.AreEqual(50f, bird.Stress);
            Assert.IsFalse(_game.HasActiveMinigame());
            Assert.AreEqual(returnScreen, _game.CurrentScreen);
        }

        [Test]
        public void CancelCurrentMinigame_DoesNotApplyReward()
        {
            var bird = _game.Controller.CurrentState.Birds.First(b => b.Id == _birdId);
            bird.Mood = 10;
            bird.Energy = 50;
            bird.Stress = 50;

            Assert.IsTrue(_game.TryStartCareMinigame(_birdId, CareActionIds.Play));
            var returnScreen = _game.GetCurrentMinigameSession().ReturnScreen;

            var cancelled = _game.CancelCurrentMinigame();

            Assert.IsTrue(cancelled);
            Assert.AreEqual(10f, bird.Mood);
            Assert.AreEqual(50f, bird.Energy);
            Assert.AreEqual(50f, bird.Stress);
            Assert.IsFalse(_game.HasActiveMinigame());
            Assert.AreEqual(returnScreen, _game.CurrentScreen);
        }

        [Test]
        public void CompleteCurrentMinigame_NoActiveSession_FailsSafely()
        {
            var screenBefore = _game.CurrentScreen;

            var completed = _game.CompleteCurrentMinigame(new MinigameCompletionViewModel
            {
                Status = MinigameCompletionStatus.Success,
                WasSuccessful = true
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
