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
            _bird.IsSick = false;
            _bird.Health = 100;
            _bird.Energy = 100;
            _bird.Hunger = 100;
            _bird.Mood = 70;

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
            Assert.IsTrue(table[BirdAnimationMood.Hungry].ContainsKey(BirdVisualState.EmoHungry));
            Assert.IsTrue(table[BirdAnimationMood.Hungry].ContainsKey(BirdVisualState.ActPeckSearch));
            Assert.IsTrue(table[BirdAnimationMood.Hungry].ContainsKey(BirdVisualState.ActHungryPacing));
            Assert.IsTrue(table[BirdAnimationMood.Hungry].ContainsKey(BirdVisualState.EmoHungryWeakChirp));
            Assert.IsTrue(table[BirdAnimationMood.Sick].ContainsKey(BirdVisualState.IdleSick));
            Assert.IsTrue(table[BirdAnimationMood.Sick].ContainsKey(BirdVisualState.EmoSickShiver));
            Assert.IsTrue(table[BirdAnimationMood.Sick].ContainsKey(BirdVisualState.EmoSickWobble));
            Assert.IsTrue(table[BirdAnimationMood.Sick].ContainsKey(BirdVisualState.ActSickCough));

            Assert.IsTrue(table[BirdAnimationMood.Neutral].ContainsKey(BirdVisualState.IdleNeutral));
            Assert.IsTrue(table[BirdAnimationMood.Neutral].ContainsKey(BirdVisualState.IdleLook));
            Assert.IsTrue(table[BirdAnimationMood.Neutral].ContainsKey(BirdVisualState.IdleShift));
            Assert.IsTrue(table[BirdAnimationMood.Neutral].ContainsKey(BirdVisualState.EmoCurious));

            foreach (var mood in table.Keys)
            {
                foreach (var row in table[mood].Values)
                {
                    Assert.IsTrue(row.Count > 0);
                    Assert.IsTrue(row.All(r => r.Weight > 0));
                    Assert.AreEqual(100, row.Sum(r => r.Weight));
                    Assert.AreEqual(row.Count, row.Select(r => r.State).Distinct().Count());
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
        public void MissingRow_FallsBackToDedicatedSickAnchorRow()
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

            var legalSickFallbackTargets = BirdVisualStateMachine.GetTransitionTable()[BirdAnimationMood.Sick][BirdVisualState.IdleSick]
                .Select(t => t.State)
                .ToHashSet();

            Assert.IsTrue(legalSickFallbackTargets.Contains(runtime.CurrentVisualState));
        }


        [Test]
        public void MissingHungryRow_FallsBackToDedicatedHungryAnchorRow()
        {
            var runtime = _controller.CurrentState.BirdVisualStates.First(v => v.BirdId == _bird.Id);
            runtime.CurrentVisualState = BirdVisualState.ActGiftReceived;
            runtime.StepCounter = 0;
            _bird.IsSick = false;
            _bird.Health = 100;
            _bird.Energy = 100;
            _bird.Hunger = 10;
            _bird.Mood = 100;

            var result = _controller.BirdVisualStates.AdvanceBirdAnimationState(_bird.Id);
            Assert.IsTrue(result.IsSuccess);

            var legalHungryFallbackTargets = BirdVisualStateMachine.GetTransitionTable()[BirdAnimationMood.Hungry][BirdVisualState.EmoHungry]
                .Select(t => t.State)
                .ToHashSet();

            Assert.IsTrue(legalHungryFallbackTargets.Contains(runtime.CurrentVisualState));
        }

        [Test]
        public void NewVisualStates_HaveExactExternalKeys()
        {
            Assert.AreEqual("emo_hungry", BirdVisualStateMachine.ToExternalKey(BirdVisualState.EmoHungry));
            Assert.AreEqual("act_peck_search", BirdVisualStateMachine.ToExternalKey(BirdVisualState.ActPeckSearch));
            Assert.AreEqual("act_hungry_pacing", BirdVisualStateMachine.ToExternalKey(BirdVisualState.ActHungryPacing));
            Assert.AreEqual("emo_hungry_weak_chirp", BirdVisualStateMachine.ToExternalKey(BirdVisualState.EmoHungryWeakChirp));
            Assert.AreEqual("idle_sick", BirdVisualStateMachine.ToExternalKey(BirdVisualState.IdleSick));
            Assert.AreEqual("emo_sick_shiver", BirdVisualStateMachine.ToExternalKey(BirdVisualState.EmoSickShiver));
            Assert.AreEqual("emo_sick_wobble", BirdVisualStateMachine.ToExternalKey(BirdVisualState.EmoSickWobble));
            Assert.AreEqual("act_sick_cough", BirdVisualStateMachine.ToExternalKey(BirdVisualState.ActSickCough));
        }

        [Test]
        public void EveryVisualState_HasUniqueExternalKey()
        {
            var keys = new HashSet<string>();
            foreach (BirdVisualState state in System.Enum.GetValues(typeof(BirdVisualState)))
            {
                var key = BirdVisualStateMachine.ToExternalKey(state);
                Assert.IsFalse(string.IsNullOrWhiteSpace(key));
                Assert.IsTrue(keys.Add(key));
            }
        }

        [Test]
        public void VisualStateNumericValues_ArePersistedCompatibilityContract()
        {
            Assert.AreEqual(0, (int)BirdVisualState.IdleNeutral);
            Assert.AreEqual(1, (int)BirdVisualState.IdleLook);
            Assert.AreEqual(2, (int)BirdVisualState.IdleHappy);
            Assert.AreEqual(3, (int)BirdVisualState.IdleShift);
            Assert.AreEqual(4, (int)BirdVisualState.IdleSleepy);
            Assert.AreEqual(5, (int)BirdVisualState.IdleSleep);
            Assert.AreEqual(6, (int)BirdVisualState.EmoExcited);
            Assert.AreEqual(7, (int)BirdVisualState.EmoCurious);
            Assert.AreEqual(8, (int)BirdVisualState.EmoSurprised);
            Assert.AreEqual(9, (int)BirdVisualState.EmoProud);
            Assert.AreEqual(10, (int)BirdVisualState.EmoSad);
            Assert.AreEqual(11, (int)BirdVisualState.EmoAngry);
            Assert.AreEqual(12, (int)BirdVisualState.EmoLove);
            Assert.AreEqual(13, (int)BirdVisualState.ActAcceptTreat);
            Assert.AreEqual(14, (int)BirdVisualState.ActChirpSing);
            Assert.AreEqual(15, (int)BirdVisualState.ActGiftReceived);
            Assert.AreEqual(16, (int)BirdVisualState.EmoHungry);
            Assert.AreEqual(17, (int)BirdVisualState.ActPeckSearch);
            Assert.AreEqual(18, (int)BirdVisualState.ActHungryPacing);
            Assert.AreEqual(19, (int)BirdVisualState.EmoHungryWeakChirp);
            Assert.AreEqual(20, (int)BirdVisualState.IdleSick);
            Assert.AreEqual(21, (int)BirdVisualState.EmoSickShiver);
            Assert.AreEqual(22, (int)BirdVisualState.EmoSickWobble);
            Assert.AreEqual(23, (int)BirdVisualState.ActSickCough);
        }

        [Test]
        public void SickRows_PreferDedicatedSickStates()
        {
            foreach (var row in BirdVisualStateMachine.GetTransitionTable()[BirdAnimationMood.Sick].Values)
            {
                var sickWeight = row.Where(t => t.State.ToString().IndexOf("Sick", System.StringComparison.Ordinal) >= 0).Sum(t => t.Weight);
                var genericWeight = row.Sum(t => t.Weight) - sickWeight;
                Assert.Greater(sickWeight, genericWeight);
                Assert.GreaterOrEqual(sickWeight, 60);
            }
        }

        [Test]
        public void HungryRows_PreferDedicatedHungryStates()
        {
            var dedicatedHungryStates = new HashSet<BirdVisualState>
            {
                BirdVisualState.EmoHungry,
                BirdVisualState.ActPeckSearch,
                BirdVisualState.ActHungryPacing,
                BirdVisualState.EmoHungryWeakChirp
            };

            foreach (var row in BirdVisualStateMachine.GetTransitionTable()[BirdAnimationMood.Hungry].Values)
            {
                var hungryWeight = row.Where(t => dedicatedHungryStates.Contains(t.State)).Sum(t => t.Weight);
                var genericWeight = row.Sum(t => t.Weight) - hungryWeight;
                Assert.GreaterOrEqual(hungryWeight, 60);
                Assert.Greater(hungryWeight, genericWeight);
            }
        }

        [Test]
        public void HungryAndSickTables_RetainLegacySourcesAndDestinations()
        {
            var table = BirdVisualStateMachine.GetTransitionTable();
            var hungryLegacy = new[] { BirdVisualState.IdleNeutral, BirdVisualState.IdleLook, BirdVisualState.EmoSad, BirdVisualState.EmoCurious, BirdVisualState.ActAcceptTreat, BirdVisualState.ActChirpSing };
            var sickLegacy = new[] { BirdVisualState.IdleSleepy, BirdVisualState.IdleSleep, BirdVisualState.EmoSad, BirdVisualState.IdleNeutral, BirdVisualState.IdleLook };

            foreach (var state in hungryLegacy)
            {
                Assert.IsTrue(table[BirdAnimationMood.Hungry].ContainsKey(state));
                Assert.IsTrue(table[BirdAnimationMood.Hungry].Values.Any(row => row.Any(t => t.State == state)));
            }

            foreach (var state in sickLegacy)
            {
                Assert.IsTrue(table[BirdAnimationMood.Sick].ContainsKey(state));
                Assert.IsTrue(table[BirdAnimationMood.Sick].Values.Any(row => row.Any(t => t.State == state)));
            }
        }

        [Test]
        public void AntiStuck_ForcesChangeWhenRepeatThresholdReached()
        {
            var runtime = _controller.CurrentState.BirdVisualStates.First(v => v.BirdId == _bird.Id);
            _bird.Mood = 55;
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
        public void HungryAdvancement_IsStableForSamePersistedInputs()
        {
            var first = BuildControllerWithKnownHungryVisualSeed();
            var second = BuildControllerWithKnownHungryVisualSeed();
            var firstBird = first.CurrentState.Birds.First();
            var secondBird = second.CurrentState.Birds.First();
            var firstSequence = new List<BirdVisualState>();
            var secondSequence = new List<BirdVisualState>();

            for (int i = 0; i < 12; i++)
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

        [Test]
        public void FacadeBirdAnimationState_ProjectsSpeciesId_AndNullCostumeId()
        {
            var game = BirdCafeGame.Instance;
            game.StartNewGame("AnimSpecies", "Cafe");

            var bird = game.Controller.CurrentState.Birds.First();
            var vm = game.GetBirdAnimationState(bird.Id);

            Assert.NotNull(vm);
            Assert.AreEqual(bird.Id, vm.BirdId);
            Assert.AreEqual(bird.SpeciesId, vm.SpeciesId);
            Assert.IsNull(vm.CostumeId);
            Assert.AreEqual(BirdVisualStateMachine.ToExternalKey(vm.CurrentVisualState), vm.CurrentVisualStateKey);
        }

        [Test]
        public void FacadeBirdAnimationState_ReflectsCostumeAfterEquipFlow()
        {
            var game = BirdCafeGame.Instance;
            game.StartNewGame("AnimCostume", "Cafe");
            game.StartSimulationPlayback();
            game.FinishSimulation();
            game.Controller.CurrentState.Economy.CurrentBalance = 5000m;

            var bird = game.Controller.CurrentState.Birds.First();
            Assert.IsTrue(game.BuyPetStoreSupply(PetStoreCatalog.CostumeBandanaId, PetStoreSupplyType.Costume));
            Assert.IsTrue(game.EquipBirdCostume(bird.Id, PetStoreCatalog.CostumeBandanaId));

            var vm = game.GetBirdAnimationState(bird.Id);

            Assert.NotNull(vm);
            Assert.AreEqual(PetStoreCatalog.CostumeBandanaId, vm.CostumeId);
            Assert.AreEqual(bird.SpeciesId, vm.SpeciesId);
            Assert.AreEqual(BirdVisualStateMachine.ToExternalKey(vm.CurrentVisualState), vm.CurrentVisualStateKey);
        }

        [Test]
        public void FacadeGetAllBirdAnimationStates_ProjectsSpeciesAndCostume()
        {
            var game = BirdCafeGame.Instance;
            game.StartNewGame("AnimAll", "Cafe");
            game.StartSimulationPlayback();
            game.FinishSimulation();
            game.Controller.CurrentState.Economy.CurrentBalance = 5000m;

            var bird = game.Controller.CurrentState.Birds.First();
            Assert.IsTrue(game.BuyPetStoreSupply(PetStoreCatalog.CostumeBandanaId, PetStoreSupplyType.Costume));
            Assert.IsTrue(game.EquipBirdCostume(bird.Id, PetStoreCatalog.CostumeBandanaId));

            var allStates = game.GetAllBirdAnimationStates();
            var mappedBird = allStates.First(s => s.BirdId == bird.Id);

            Assert.AreEqual(game.Controller.CurrentState.Birds.Count, allStates.Count);
            Assert.AreEqual(bird.SpeciesId, mappedBird.SpeciesId);
            Assert.AreEqual(PetStoreCatalog.CostumeBandanaId, mappedBird.CostumeId);
            Assert.AreEqual(BirdVisualStateMachine.ToExternalKey(mappedBird.CurrentVisualState), mappedBird.CurrentVisualStateKey);
        }


        private static BirdCafeController BuildControllerWithKnownHungryVisualSeed()
        {
            var controller = BuildControllerWithKnownVisualSeed();
            var bird = controller.CurrentState.Birds.First();
            bird.IsSick = false;
            bird.Health = 100;
            bird.Energy = 100;
            bird.Hunger = 10;
            bird.Mood = 100;

            var runtime = controller.CurrentState.BirdVisualStates.First(v => v.BirdId == bird.Id);
            runtime.CurrentVisualState = BirdVisualState.EmoHungry;
            runtime.CurrentMood = BirdAnimationMood.Hungry;
            runtime.StepCounter = 5;
            runtime.ConsecutiveRepeatCount = 0;

            return controller;
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
