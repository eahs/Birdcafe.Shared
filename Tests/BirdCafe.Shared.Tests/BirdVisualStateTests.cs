using BirdCafe.Shared.Engine;
using BirdCafe.Shared.Engine.Utils;
using BirdCafe.Shared.Enums;
using BirdCafe.Shared.Models.Birds;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace BirdCafe.Shared.Tests
{
    public class BirdVisualStateTests
    {
        private BirdCafeController _controller;
        private Bird _bird;

        [SetUp]
        public void Setup()
        {
            _controller = new BirdCafeController();
            _controller.Meta.StartNewGame("VisualTester", "Cafe");
            _bird = _controller.CurrentState.Birds.First();
        }

        [Test]
        public void MoodResolver_SickOverridesExcitedAndHappy()
        {
            _bird.IsSick = true;
            _bird.Mood = 100;
            _bird.Health = 100;
            _bird.Energy = 100;
            _bird.Hunger = 100;

            Assert.AreEqual(BirdAnimationMood.Sick, BirdMoodResolver.Resolve(_bird));
        }

        [Test]
        public void MoodResolver_SleepyOverridesHappy()
        {
            _bird.IsSick = false;
            _bird.Health = 100;
            _bird.Mood = 90;
            _bird.Hunger = 100;
            _bird.Energy = 10;

            Assert.AreEqual(BirdAnimationMood.Sleepy, BirdMoodResolver.Resolve(_bird));
        }

        [Test]
        public void MoodResolver_HungryOverridesHappy()
        {
            _bird.IsSick = false;
            _bird.Health = 100;
            _bird.Mood = 90;
            _bird.Energy = 100;
            _bird.Hunger = 10;

            Assert.AreEqual(BirdAnimationMood.Hungry, BirdMoodResolver.Resolve(_bird));
        }

        [Test]
        public void MoodResolver_FallsBackToNeutral()
        {
            _bird.IsSick = false;
            _bird.Health = 55;
            _bird.Mood = 55;
            _bird.Energy = 55;
            _bird.Hunger = 55;

            Assert.AreEqual(BirdAnimationMood.Neutral, BirdMoodResolver.Resolve(_bird));
        }

        [Test]
        public void TransitionSelection_AlwaysUsesLegalRowOption()
        {
            var runtime = _controller.CurrentState.BirdVisualStates.First(v => v.BirdId == _bird.Id);
            runtime.CurrentMood = BirdAnimationMood.Happy;
            runtime.CurrentVisualState = BirdVisualState.IdleHappy;

            var legal = BirdVisualStateMachine.GetTransitionTable()[BirdAnimationMood.Happy][BirdVisualState.IdleHappy]
                .Select(t => t.State)
                .ToHashSet();

            for (int i = 0; i < 30; i++)
            {
                runtime.CurrentVisualState = BirdVisualState.IdleHappy;
                var result = _controller.BirdVisualStates.AdvanceBirdAnimationState(_bird.Id);
                Assert.IsTrue(result.IsSuccess);
                Assert.IsTrue(legal.Contains(runtime.CurrentVisualState));
            }
        }

        [Test]
        public void TransitionTable_ContainsRequiredRowsForAllMoods()
        {
            var table = BirdVisualStateMachine.GetTransitionTable();

            Assert.IsTrue(table[BirdAnimationMood.Happy].ContainsKey(BirdVisualState.IdleHappy));
            Assert.IsTrue(table[BirdAnimationMood.Excited].ContainsKey(BirdVisualState.EmoExcited));
            Assert.IsTrue(table[BirdAnimationMood.Sleepy].ContainsKey(BirdVisualState.IdleSleepy));
            Assert.IsTrue(table[BirdAnimationMood.Hungry].ContainsKey(BirdVisualState.IdleNeutral));
            Assert.IsTrue(table[BirdAnimationMood.Sick].ContainsKey(BirdVisualState.IdleSleepy));

            Assert.IsTrue(table[BirdAnimationMood.Neutral].ContainsKey(BirdVisualState.IdleNeutral));
            Assert.IsTrue(table[BirdAnimationMood.Neutral].ContainsKey(BirdVisualState.IdleLook));
            Assert.IsTrue(table[BirdAnimationMood.Neutral].ContainsKey(BirdVisualState.IdleShift));
            Assert.IsTrue(table[BirdAnimationMood.Neutral].ContainsKey(BirdVisualState.EmoCurious));

            foreach (var mood in table.Keys)
            {
                foreach (var row in table[mood].Values)
                {
                    Assert.IsTrue(row.Count > 0);
                    Assert.AreEqual(100, row.Sum(r => r.Weight));
                }
            }
        }

        [Test]
        public void MissingRuntimeState_IsBackfilledOnQuery()
        {
            _controller.CurrentState.BirdVisualStates = new List<BirdVisualRuntimeState>();

            var result = _controller.BirdVisualStates.GetBirdAnimationState(_bird.Id);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, _controller.CurrentState.BirdVisualStates.Count);
            Assert.AreEqual(_bird.Id, ((BirdVisualRuntimeState)result.Payload).BirdId);
        }

        [Test]
        public void NewGame_InitializesRuntimeStateForStarterBirds()
        {
            Assert.AreEqual(_controller.CurrentState.Birds.Count, _controller.CurrentState.BirdVisualStates.Count);
            Assert.AreEqual(_bird.Id, _controller.CurrentState.BirdVisualStates[0].BirdId);
        }

        [Test]
        public void OneShotEvent_IsMappedAndConsumed()
        {
            var trigger = _controller.BirdVisualStates.TriggerBirdAnimationEvent(_bird.Id, BirdAnimationEventIds.TreatGiven);
            Assert.IsTrue(trigger.IsSuccess);

            var advance = _controller.BirdVisualStates.AdvanceBirdAnimationState(_bird.Id);
            Assert.IsTrue(advance.IsSuccess);

            var runtime = _controller.CurrentState.BirdVisualStates.First(v => v.BirdId == _bird.Id);
            Assert.AreEqual(BirdVisualState.ActAcceptTreat, runtime.CurrentVisualState);
            Assert.IsNull(runtime.PendingOneShotEventId);
        }

        [Test]
        public void MissingRow_FallsBackToMoodAnchorRow_NotNeutralRow()
        {
            var runtime = _controller.CurrentState.BirdVisualStates.First(v => v.BirdId == _bird.Id);
            runtime.CurrentVisualState = BirdVisualState.ActGiftReceived; // not present in Sick table
            runtime.StepCounter = 0;
            _bird.IsSick = true;
            _bird.Health = 100;
            _bird.Energy = 100;
            _bird.Hunger = 100;

            var result = _controller.BirdVisualStates.AdvanceBirdAnimationState(_bird.Id);
            Assert.IsTrue(result.IsSuccess);

            var legalSickFallbackTargets = BirdVisualStateMachine.GetTransitionTable()[BirdAnimationMood.Sick][BirdVisualState.IdleSleepy]
                .Select(t => t.State)
                .ToHashSet();

            Assert.IsTrue(legalSickFallbackTargets.Contains(runtime.CurrentVisualState));
        }

        [Test]
        public void AntiStuck_ForcesChangeWhenRepeatThresholdReached()
        {
            var runtime = _controller.CurrentState.BirdVisualStates.First(v => v.BirdId == _bird.Id);
            _bird.Mood = 60;
            _bird.Health = 60;
            _bird.Energy = 60;
            _bird.Hunger = 60;

            int selfSelectingStep = -1;
            for (int step = 0; step < 200; step++)
            {
                runtime.CurrentVisualState = BirdVisualState.IdleNeutral;
                runtime.StepCounter = step;
                runtime.ConsecutiveRepeatCount = 0;

                _controller.BirdVisualStates.AdvanceBirdAnimationState(_bird.Id);
                if (runtime.CurrentVisualState == BirdVisualState.IdleNeutral)
                {
                    selfSelectingStep = step;
                    break;
                }
            }

            Assert.GreaterOrEqual(selfSelectingStep, 0);

            runtime.CurrentVisualState = BirdVisualState.IdleNeutral;
            runtime.StepCounter = selfSelectingStep;
            runtime.ConsecutiveRepeatCount = 3;
            _controller.BirdVisualStates.AdvanceBirdAnimationState(_bird.Id);

            Assert.AreNotEqual(BirdVisualState.IdleNeutral, runtime.CurrentVisualState);
        }

        [Test]
        public void DeterministicAdvancement_IsStableForSamePersistedInputs()
        {
            var first = BuildControllerWithKnownVisualSeed();
            var second = BuildControllerWithKnownVisualSeed();

            var firstBird = first.CurrentState.Birds.First();
            var secondBird = second.CurrentState.Birds.First();

            var firstSequence = new List<BirdVisualState>();
            var secondSequence = new List<BirdVisualState>();

            for (int i = 0; i < 10; i++)
            {
                first.BirdVisualStates.AdvanceBirdAnimationState(firstBird.Id);
                second.BirdVisualStates.AdvanceBirdAnimationState(secondBird.Id);

                firstSequence.Add(first.CurrentState.BirdVisualStates.First(v => v.BirdId == firstBird.Id).CurrentVisualState);
                secondSequence.Add(second.CurrentState.BirdVisualStates.First(v => v.BirdId == secondBird.Id).CurrentVisualState);
            }

            CollectionAssert.AreEqual(firstSequence, secondSequence);
        }

        [Test]
        public void MoodRefresh_IsImmediateAfterCareStatChange()
        {
            _controller.SetPhaseForTests(GamePhase.EveningLoop);
            _controller.CurrentState.PetStore.AddFood(BirdFoodType.SeedMix, 1);
            _bird.Hunger = 5;

            var hungryBefore = _controller.BirdVisualStates.GetBirdAnimationState(_bird.Id);
            Assert.AreEqual(BirdAnimationMood.Hungry, ((BirdVisualRuntimeState)hungryBefore.Payload).CurrentMood);

            var careResult = _controller.Care.PerformCareAction(_bird.Id, CareActionIds.Feed);
            Assert.IsTrue(careResult.IsSuccess);

            var afterFeed = _controller.BirdVisualStates.GetBirdAnimationState(_bird.Id);
            Assert.AreNotEqual(BirdAnimationMood.Hungry, ((BirdVisualRuntimeState)afterFeed.Payload).CurrentMood);
        }

        [Test]
        public void InvalidBirdId_ReturnsFailure_ForCommandAndQuery()
        {
            var query = _controller.BirdVisualStates.GetBirdAnimationState("missing-id");
            var advance = _controller.BirdVisualStates.AdvanceBirdAnimationState("missing-id");
            var trigger = _controller.BirdVisualStates.TriggerBirdAnimationEvent("missing-id", BirdAnimationEventIds.TreatGiven);

            Assert.IsFalse(query.IsSuccess);
            Assert.IsFalse(advance.IsSuccess);
            Assert.IsFalse(trigger.IsSuccess);
            Assert.AreEqual("BirdNotFound", query.ErrorCode);
            Assert.AreEqual("BirdNotFound", advance.ErrorCode);
            Assert.AreEqual("BirdNotFound", trigger.ErrorCode);
        }

        private static BirdCafeController BuildControllerWithKnownVisualSeed()
        {
            var controller = new BirdCafeController();
            controller.Meta.StartNewGame("Deterministic", "Cafe");

            var bird = controller.CurrentState.Birds.First();
            bird.Id = "bird-deterministic";
            bird.IsSick = false;
            bird.Health = 100;
            bird.Energy = 100;
            bird.Hunger = 100;
            bird.Mood = 80;

            controller.CurrentState.CurrentDayNumber = 4;
            controller.CurrentState.CurrentWeekNumber = 2;
            controller.CurrentState.BirdVisualStates = new List<BirdVisualRuntimeState>();
            controller.BirdVisualStates.EnsureRuntimeStateForAllBirds();

            return controller;
        }
    }
}
