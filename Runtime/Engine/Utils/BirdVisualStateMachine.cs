using BirdCafe.Shared.Enums;
using BirdCafe.Shared.Models.Birds;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BirdCafe.Shared.Engine.Utils
{
    /// <summary>
    /// Deterministic visual-state machine used to map live bird stats to stable animation-state keys.
    /// </summary>
    public static class BirdVisualStateMachine
    {
        private const int MaxConsecutiveRepeatsBeforeForcedChange = 3;

        private static readonly Dictionary<string, BirdVisualState> OneShotEventToState = new Dictionary<string, BirdVisualState>(StringComparer.Ordinal)
        {
            [BirdAnimationEventIds.TreatGiven] = BirdVisualState.ActAcceptTreat,
            [BirdAnimationEventIds.GiftReceived] = BirdVisualState.ActGiftReceived,
            [BirdAnimationEventIds.SongPrompt] = BirdVisualState.ActChirpSing
        };

        private static readonly Dictionary<BirdVisualState, string> ExternalKeys = new Dictionary<BirdVisualState, string>
        {
            [BirdVisualState.IdleNeutral] = "idle_neutral",
            [BirdVisualState.IdleLook] = "idle_look",
            [BirdVisualState.IdleHappy] = "idle_happy",
            [BirdVisualState.IdleShift] = "idle_shift",
            [BirdVisualState.IdleSleepy] = "idle_sleepy",
            [BirdVisualState.IdleSleep] = "idle_sleep",
            [BirdVisualState.EmoExcited] = "emo_excited",
            [BirdVisualState.EmoCurious] = "emo_curious",
            [BirdVisualState.EmoSurprised] = "emo_surprised",
            [BirdVisualState.EmoProud] = "emo_proud",
            [BirdVisualState.EmoSad] = "emo_sad",
            [BirdVisualState.EmoAngry] = "emo_angry",
            [BirdVisualState.EmoLove] = "emo_love",
            [BirdVisualState.ActAcceptTreat] = "act_accept_treat",
            [BirdVisualState.ActChirpSing] = "act_chirp_sing",
            [BirdVisualState.ActGiftReceived] = "act_gift_received",
            [BirdVisualState.EmoHungry] = "emo_hungry",
            [BirdVisualState.ActPeckSearch] = "act_peck_search",
            [BirdVisualState.ActHungryPacing] = "act_hungry_pacing",
            [BirdVisualState.EmoHungryWeakChirp] = "emo_hungry_weak_chirp",
            [BirdVisualState.IdleSick] = "idle_sick",
            [BirdVisualState.EmoSickShiver] = "emo_sick_shiver",
            [BirdVisualState.EmoSickWobble] = "emo_sick_wobble",
            [BirdVisualState.ActSickCough] = "act_sick_cough"
        };

        private static readonly Dictionary<BirdAnimationMood, Dictionary<BirdVisualState, List<WeightedTransition>>> TransitionTable =
            BuildTransitions();

        /// <summary>
        /// Returns a stable external animation key used by thin rendering clients.
        /// </summary>
        public static string ToExternalKey(BirdVisualState state) => ExternalKeys[state];

        /// <summary>
        /// Returns true when the provided one-shot event id is supported.
        /// </summary>
        public static bool IsSupportedEventId(string eventId) => !string.IsNullOrWhiteSpace(eventId) && OneShotEventToState.ContainsKey(eventId);

        /// <summary>
        /// Resolves the current mood from live bird stats and stores it in runtime state.
        /// </summary>
        public static BirdAnimationMood RefreshMood(Bird bird, BirdVisualRuntimeState runtime)
        {
            var mood = BirdMoodResolver.Resolve(bird);
            runtime.CurrentMood = mood;
            return mood;
        }

        /// <summary>
        /// Advances the visual state once, consuming pending one-shots before mood-table transitions.
        /// </summary>
        public static BirdVisualState Advance(Bird bird, BirdVisualRuntimeState runtime, GameSave save)
        {
            var mood = RefreshMood(bird, runtime);

            if (!string.IsNullOrWhiteSpace(runtime.PendingOneShotEventId)
                && OneShotEventToState.TryGetValue(runtime.PendingOneShotEventId, out var oneShotState))
            {
                ApplyAdvancedState(runtime, oneShotState);
                runtime.PendingOneShotEventId = null;
                return runtime.CurrentVisualState;
            }

            var row = ResolveTransitionRow(mood, runtime.CurrentVisualState);
            var selected = SelectWeightedDeterministic(row, bird.Id, mood, runtime.CurrentVisualState, runtime.StepCounter, save.CurrentDayNumber, save.CurrentWeekNumber);

            if (selected == runtime.CurrentVisualState && runtime.ConsecutiveRepeatCount >= MaxConsecutiveRepeatsBeforeForcedChange)
            {
                selected = ForceNonRepeatSelection(row, bird.Id, mood, runtime.CurrentVisualState, runtime.StepCounter, save.CurrentDayNumber, save.CurrentWeekNumber);
            }

            ApplyAdvancedState(runtime, selected);
            return runtime.CurrentVisualState;
        }

        /// <summary>
        /// Exposes transition table structure for deterministic tests.
        /// </summary>
        public static IReadOnlyDictionary<BirdAnimationMood, Dictionary<BirdVisualState, List<WeightedTransition>>> GetTransitionTable()
            => TransitionTable;

        private static void ApplyAdvancedState(BirdVisualRuntimeState runtime, BirdVisualState selected)
        {
            runtime.ConsecutiveRepeatCount = selected == runtime.CurrentVisualState
                ? runtime.ConsecutiveRepeatCount + 1
                : 0;

            runtime.CurrentVisualState = selected;
            runtime.StepCounter++;
        }

        private static List<WeightedTransition> ResolveTransitionRow(BirdAnimationMood mood, BirdVisualState from)
        {
            var moodTable = TransitionTable[mood];
            if (moodTable.TryGetValue(from, out var row))
                return row;

            // deterministic fallback for unsupported source states inside a mood.
            // Use a mood-specific anchor row so we preserve the live mood intent.
            var fallbackState = GetMoodFallbackState(mood);
            if (moodTable.TryGetValue(fallbackState, out var fallbackRow))
                return fallbackRow;

            // hard safety fallback if a table is malformed.
            if (TransitionTable[BirdAnimationMood.Neutral].TryGetValue(BirdVisualState.IdleNeutral, out var neutralRow))
                return neutralRow;

            return new List<WeightedTransition> { new WeightedTransition(BirdVisualState.IdleNeutral, 100) };
        }

        private static BirdVisualState GetMoodFallbackState(BirdAnimationMood mood)
        {
            switch (mood)
            {
                case BirdAnimationMood.Happy:
                    return BirdVisualState.IdleHappy;
                case BirdAnimationMood.Excited:
                    return BirdVisualState.EmoExcited;
                case BirdAnimationMood.Sleepy:
                    return BirdVisualState.IdleSleepy;
                case BirdAnimationMood.Hungry:
                    return BirdVisualState.EmoHungry;
                case BirdAnimationMood.Sick:
                    return BirdVisualState.IdleSick;
                default:
                    return BirdVisualState.IdleNeutral;
            }
        }

        private static BirdVisualState SelectWeightedDeterministic(List<WeightedTransition> options, string birdId, BirdAnimationMood mood, BirdVisualState current, int step, int day, int week)
        {
            int totalWeight = options.Sum(o => o.Weight);
            int roll = StableHash(birdId, mood.ToString(), current.ToString(), step.ToString(), day.ToString(), week.ToString()) % totalWeight;

            int cumulative = 0;
            foreach (var option in options)
            {
                cumulative += option.Weight;
                if (roll < cumulative)
                    return option.State;
            }

            return options[options.Count - 1].State;
        }

        private static BirdVisualState ForceNonRepeatSelection(List<WeightedTransition> options, string birdId, BirdAnimationMood mood, BirdVisualState current, int step, int day, int week)
        {
            var alternatives = options.Where(o => o.State != current).ToList();
            if (alternatives.Count == 0)
                return current;

            return SelectWeightedDeterministic(alternatives, birdId, mood, current, step + 17, day, week);
        }

        private static int StableHash(params string[] values)
        {
            unchecked
            {
                int hash = (int)2166136261;
                for (int i = 0; i < values.Length; i++)
                {
                    var value = values[i] ?? string.Empty;
                    for (int j = 0; j < value.Length; j++)
                    {
                        hash ^= value[j];
                        hash *= 16777619;
                    }

                    hash ^= '|';
                    hash *= 16777619;
                }

                return hash == int.MinValue ? int.MaxValue : Math.Abs(hash);
            }
        }

        private static Dictionary<BirdAnimationMood, Dictionary<BirdVisualState, List<WeightedTransition>>> BuildTransitions()
        {
            return new Dictionary<BirdAnimationMood, Dictionary<BirdVisualState, List<WeightedTransition>>>
            {
                [BirdAnimationMood.Happy] = new Dictionary<BirdVisualState, List<WeightedTransition>>
                {
                    [BirdVisualState.IdleHappy] = Row((BirdVisualState.IdleHappy, 45), (BirdVisualState.IdleLook, 15), (BirdVisualState.IdleShift, 15), (BirdVisualState.ActChirpSing, 10), (BirdVisualState.EmoProud, 8), (BirdVisualState.EmoLove, 5), (BirdVisualState.ActGiftReceived, 2)),
                    [BirdVisualState.IdleLook] = Row((BirdVisualState.IdleHappy, 35), (BirdVisualState.IdleLook, 25), (BirdVisualState.IdleShift, 20), (BirdVisualState.EmoCurious, 10), (BirdVisualState.ActChirpSing, 10)),
                    [BirdVisualState.IdleShift] = Row((BirdVisualState.IdleHappy, 40), (BirdVisualState.IdleShift, 25), (BirdVisualState.IdleLook, 20), (BirdVisualState.EmoProud, 10), (BirdVisualState.ActAcceptTreat, 5)),
                    [BirdVisualState.EmoProud] = Row((BirdVisualState.IdleHappy, 55), (BirdVisualState.IdleLook, 15), (BirdVisualState.IdleShift, 15), (BirdVisualState.EmoLove, 10), (BirdVisualState.ActChirpSing, 5)),
                    [BirdVisualState.EmoLove] = Row((BirdVisualState.IdleHappy, 55), (BirdVisualState.IdleLook, 15), (BirdVisualState.ActGiftReceived, 10), (BirdVisualState.ActChirpSing, 10), (BirdVisualState.IdleShift, 10)),
                    [BirdVisualState.ActChirpSing] = Row((BirdVisualState.IdleHappy, 50), (BirdVisualState.IdleLook, 20), (BirdVisualState.EmoProud, 10), (BirdVisualState.EmoCurious, 10), (BirdVisualState.ActChirpSing, 10)),
                    [BirdVisualState.ActGiftReceived] = Row((BirdVisualState.IdleHappy, 50), (BirdVisualState.EmoLove, 20), (BirdVisualState.EmoProud, 15), (BirdVisualState.ActChirpSing, 10), (BirdVisualState.IdleLook, 5))
                },
                [BirdAnimationMood.Excited] = new Dictionary<BirdVisualState, List<WeightedTransition>>
                {
                    [BirdVisualState.EmoExcited] = Row((BirdVisualState.EmoExcited, 35), (BirdVisualState.ActChirpSing, 20), (BirdVisualState.EmoSurprised, 15), (BirdVisualState.EmoCurious, 10), (BirdVisualState.IdleHappy, 20)),
                    [BirdVisualState.ActChirpSing] = Row((BirdVisualState.EmoExcited, 30), (BirdVisualState.ActChirpSing, 20), (BirdVisualState.IdleHappy, 25), (BirdVisualState.EmoSurprised, 15), (BirdVisualState.EmoCurious, 10)),
                    [BirdVisualState.EmoSurprised] = Row((BirdVisualState.EmoExcited, 35), (BirdVisualState.IdleHappy, 20), (BirdVisualState.EmoCurious, 20), (BirdVisualState.ActChirpSing, 15), (BirdVisualState.IdleLook, 10)),
                    [BirdVisualState.EmoCurious] = Row((BirdVisualState.EmoExcited, 25), (BirdVisualState.IdleLook, 20), (BirdVisualState.ActAcceptTreat, 15), (BirdVisualState.ActChirpSing, 20), (BirdVisualState.IdleHappy, 20)),
                    [BirdVisualState.IdleHappy] = Row((BirdVisualState.EmoExcited, 30), (BirdVisualState.ActChirpSing, 20), (BirdVisualState.IdleHappy, 20), (BirdVisualState.EmoCurious, 15), (BirdVisualState.ActGiftReceived, 15))
                },
                [BirdAnimationMood.Sleepy] = new Dictionary<BirdVisualState, List<WeightedTransition>>
                {
                    [BirdVisualState.IdleSleepy] = Row((BirdVisualState.IdleSleepy, 40), (BirdVisualState.IdleSleep, 30), (BirdVisualState.IdleNeutral, 15), (BirdVisualState.IdleShift, 10), (BirdVisualState.IdleLook, 5)),
                    [BirdVisualState.IdleSleep] = Row((BirdVisualState.IdleSleep, 55), (BirdVisualState.IdleSleepy, 25), (BirdVisualState.IdleNeutral, 10), (BirdVisualState.IdleLook, 5), (BirdVisualState.IdleShift, 5)),
                    [BirdVisualState.IdleNeutral] = Row((BirdVisualState.IdleSleepy, 35), (BirdVisualState.IdleNeutral, 25), (BirdVisualState.IdleLook, 15), (BirdVisualState.IdleShift, 15), (BirdVisualState.IdleSleep, 10)),
                    [BirdVisualState.IdleLook] = Row((BirdVisualState.IdleNeutral, 30), (BirdVisualState.IdleSleepy, 35), (BirdVisualState.IdleLook, 15), (BirdVisualState.IdleShift, 10), (BirdVisualState.IdleSleep, 10)),
                    [BirdVisualState.IdleShift] = Row((BirdVisualState.IdleNeutral, 35), (BirdVisualState.IdleSleepy, 35), (BirdVisualState.IdleShift, 15), (BirdVisualState.IdleLook, 10), (BirdVisualState.IdleSleep, 5))
                },
                [BirdAnimationMood.Hungry] = new Dictionary<BirdVisualState, List<WeightedTransition>>
                {
                    [BirdVisualState.EmoHungry] = Row((BirdVisualState.EmoHungry, 25), (BirdVisualState.ActPeckSearch, 20), (BirdVisualState.ActHungryPacing, 18), (BirdVisualState.EmoHungryWeakChirp, 15), (BirdVisualState.IdleNeutral, 6), (BirdVisualState.IdleLook, 5), (BirdVisualState.EmoSad, 4), (BirdVisualState.EmoCurious, 3), (BirdVisualState.ActAcceptTreat, 3), (BirdVisualState.ActChirpSing, 1)),
                    [BirdVisualState.ActPeckSearch] = Row((BirdVisualState.EmoHungry, 18), (BirdVisualState.ActPeckSearch, 28), (BirdVisualState.ActHungryPacing, 15), (BirdVisualState.EmoHungryWeakChirp, 10), (BirdVisualState.IdleNeutral, 8), (BirdVisualState.IdleLook, 6), (BirdVisualState.EmoSad, 3), (BirdVisualState.EmoCurious, 5), (BirdVisualState.ActAcceptTreat, 5), (BirdVisualState.ActChirpSing, 2)),
                    [BirdVisualState.ActHungryPacing] = Row((BirdVisualState.EmoHungry, 20), (BirdVisualState.ActPeckSearch, 18), (BirdVisualState.ActHungryPacing, 28), (BirdVisualState.EmoHungryWeakChirp, 12), (BirdVisualState.IdleNeutral, 6), (BirdVisualState.IdleLook, 5), (BirdVisualState.EmoSad, 4), (BirdVisualState.EmoCurious, 3), (BirdVisualState.ActAcceptTreat, 3), (BirdVisualState.ActChirpSing, 1)),
                    [BirdVisualState.EmoHungryWeakChirp] = Row((BirdVisualState.EmoHungry, 25), (BirdVisualState.ActPeckSearch, 18), (BirdVisualState.ActHungryPacing, 12), (BirdVisualState.EmoHungryWeakChirp, 20), (BirdVisualState.IdleNeutral, 7), (BirdVisualState.IdleLook, 5), (BirdVisualState.EmoSad, 5), (BirdVisualState.EmoCurious, 3), (BirdVisualState.ActAcceptTreat, 4), (BirdVisualState.ActChirpSing, 1)),
                    [BirdVisualState.IdleNeutral] = Row((BirdVisualState.EmoHungry, 25), (BirdVisualState.ActPeckSearch, 18), (BirdVisualState.ActHungryPacing, 15), (BirdVisualState.EmoHungryWeakChirp, 12), (BirdVisualState.IdleNeutral, 10), (BirdVisualState.IdleLook, 7), (BirdVisualState.EmoSad, 5), (BirdVisualState.EmoCurious, 3), (BirdVisualState.ActAcceptTreat, 4), (BirdVisualState.ActChirpSing, 1)),
                    [BirdVisualState.IdleLook] = Row((BirdVisualState.EmoHungry, 22), (BirdVisualState.ActPeckSearch, 20), (BirdVisualState.ActHungryPacing, 15), (BirdVisualState.EmoHungryWeakChirp, 10), (BirdVisualState.IdleNeutral, 8), (BirdVisualState.IdleLook, 10), (BirdVisualState.EmoSad, 4), (BirdVisualState.EmoCurious, 5), (BirdVisualState.ActAcceptTreat, 4), (BirdVisualState.ActChirpSing, 2)),
                    [BirdVisualState.EmoSad] = Row((BirdVisualState.EmoHungry, 24), (BirdVisualState.ActPeckSearch, 15), (BirdVisualState.ActHungryPacing, 14), (BirdVisualState.EmoHungryWeakChirp, 17), (BirdVisualState.IdleNeutral, 7), (BirdVisualState.IdleLook, 5), (BirdVisualState.EmoSad, 10), (BirdVisualState.EmoCurious, 3), (BirdVisualState.ActAcceptTreat, 3), (BirdVisualState.ActChirpSing, 2)),
                    [BirdVisualState.EmoCurious] = Row((BirdVisualState.EmoHungry, 20), (BirdVisualState.ActPeckSearch, 25), (BirdVisualState.ActHungryPacing, 12), (BirdVisualState.EmoHungryWeakChirp, 10), (BirdVisualState.IdleNeutral, 6), (BirdVisualState.IdleLook, 8), (BirdVisualState.EmoSad, 3), (BirdVisualState.EmoCurious, 8), (BirdVisualState.ActAcceptTreat, 6), (BirdVisualState.ActChirpSing, 2)),
                    [BirdVisualState.ActAcceptTreat] = Row((BirdVisualState.EmoHungry, 20), (BirdVisualState.ActPeckSearch, 18), (BirdVisualState.ActHungryPacing, 12), (BirdVisualState.EmoHungryWeakChirp, 10), (BirdVisualState.IdleNeutral, 8), (BirdVisualState.IdleLook, 5), (BirdVisualState.EmoSad, 3), (BirdVisualState.EmoCurious, 4), (BirdVisualState.ActAcceptTreat, 15), (BirdVisualState.ActChirpSing, 5)),
                    [BirdVisualState.ActChirpSing] = Row((BirdVisualState.EmoHungry, 20), (BirdVisualState.ActPeckSearch, 15), (BirdVisualState.ActHungryPacing, 12), (BirdVisualState.EmoHungryWeakChirp, 18), (BirdVisualState.IdleNeutral, 7), (BirdVisualState.IdleLook, 6), (BirdVisualState.EmoSad, 5), (BirdVisualState.EmoCurious, 3), (BirdVisualState.ActAcceptTreat, 4), (BirdVisualState.ActChirpSing, 10))
                },
                [BirdAnimationMood.Sick] = new Dictionary<BirdVisualState, List<WeightedTransition>>
                {
                    [BirdVisualState.IdleSick] = Row((BirdVisualState.IdleSick, 30), (BirdVisualState.EmoSickShiver, 22), (BirdVisualState.EmoSickWobble, 14), (BirdVisualState.ActSickCough, 10), (BirdVisualState.IdleSleepy, 10), (BirdVisualState.IdleSleep, 6), (BirdVisualState.EmoSad, 5), (BirdVisualState.IdleNeutral, 2), (BirdVisualState.IdleLook, 1)),
                    [BirdVisualState.EmoSickShiver] = Row((BirdVisualState.IdleSick, 28), (BirdVisualState.EmoSickShiver, 25), (BirdVisualState.EmoSickWobble, 15), (BirdVisualState.ActSickCough, 12), (BirdVisualState.IdleSleepy, 8), (BirdVisualState.IdleSleep, 5), (BirdVisualState.EmoSad, 4), (BirdVisualState.IdleNeutral, 2), (BirdVisualState.IdleLook, 1)),
                    [BirdVisualState.EmoSickWobble] = Row((BirdVisualState.IdleSick, 30), (BirdVisualState.EmoSickShiver, 18), (BirdVisualState.EmoSickWobble, 20), (BirdVisualState.ActSickCough, 12), (BirdVisualState.IdleSleepy, 8), (BirdVisualState.IdleSleep, 4), (BirdVisualState.EmoSad, 5), (BirdVisualState.IdleNeutral, 2), (BirdVisualState.IdleLook, 1)),
                    [BirdVisualState.ActSickCough] = Row((BirdVisualState.IdleSick, 35), (BirdVisualState.EmoSickShiver, 20), (BirdVisualState.EmoSickWobble, 12), (BirdVisualState.ActSickCough, 8), (BirdVisualState.IdleSleepy, 10), (BirdVisualState.IdleSleep, 6), (BirdVisualState.EmoSad, 5), (BirdVisualState.IdleNeutral, 3), (BirdVisualState.IdleLook, 1)),
                    [BirdVisualState.IdleSleepy] = Row((BirdVisualState.IdleSick, 30), (BirdVisualState.EmoSickShiver, 20), (BirdVisualState.EmoSickWobble, 12), (BirdVisualState.ActSickCough, 8), (BirdVisualState.IdleSleepy, 12), (BirdVisualState.IdleSleep, 8), (BirdVisualState.EmoSad, 6), (BirdVisualState.IdleNeutral, 3), (BirdVisualState.IdleLook, 1)),
                    [BirdVisualState.IdleSleep] = Row((BirdVisualState.IdleSick, 28), (BirdVisualState.EmoSickShiver, 18), (BirdVisualState.EmoSickWobble, 10), (BirdVisualState.ActSickCough, 6), (BirdVisualState.IdleSleepy, 10), (BirdVisualState.IdleSleep, 18), (BirdVisualState.EmoSad, 5), (BirdVisualState.IdleNeutral, 4), (BirdVisualState.IdleLook, 1)),
                    [BirdVisualState.EmoSad] = Row((BirdVisualState.IdleSick, 28), (BirdVisualState.EmoSickShiver, 18), (BirdVisualState.EmoSickWobble, 15), (BirdVisualState.ActSickCough, 9), (BirdVisualState.IdleSleepy, 8), (BirdVisualState.IdleSleep, 5), (BirdVisualState.EmoSad, 12), (BirdVisualState.IdleNeutral, 4), (BirdVisualState.IdleLook, 1)),
                    [BirdVisualState.IdleNeutral] = Row((BirdVisualState.IdleSick, 30), (BirdVisualState.EmoSickShiver, 20), (BirdVisualState.EmoSickWobble, 12), (BirdVisualState.ActSickCough, 8), (BirdVisualState.IdleSleepy, 9), (BirdVisualState.IdleSleep, 5), (BirdVisualState.EmoSad, 4), (BirdVisualState.IdleNeutral, 10), (BirdVisualState.IdleLook, 2)),
                    [BirdVisualState.IdleLook] = Row((BirdVisualState.IdleSick, 28), (BirdVisualState.EmoSickShiver, 18), (BirdVisualState.EmoSickWobble, 15), (BirdVisualState.ActSickCough, 9), (BirdVisualState.IdleSleepy, 7), (BirdVisualState.IdleSleep, 4), (BirdVisualState.EmoSad, 1), (BirdVisualState.IdleNeutral, 8), (BirdVisualState.IdleLook, 10))
                },
                [BirdAnimationMood.Neutral] = new Dictionary<BirdVisualState, List<WeightedTransition>>
                {
                    [BirdVisualState.IdleNeutral] = Row((BirdVisualState.IdleNeutral, 40), (BirdVisualState.IdleLook, 25), (BirdVisualState.IdleShift, 25), (BirdVisualState.EmoCurious, 10)),
                    [BirdVisualState.IdleLook] = Row((BirdVisualState.IdleNeutral, 35), (BirdVisualState.IdleLook, 30), (BirdVisualState.IdleShift, 20), (BirdVisualState.EmoCurious, 15)),
                    [BirdVisualState.IdleShift] = Row((BirdVisualState.IdleNeutral, 35), (BirdVisualState.IdleShift, 30), (BirdVisualState.IdleLook, 20), (BirdVisualState.EmoCurious, 15)),
                    [BirdVisualState.EmoCurious] = Row((BirdVisualState.IdleLook, 30), (BirdVisualState.IdleNeutral, 30), (BirdVisualState.IdleShift, 20), (BirdVisualState.EmoCurious, 20))
                }
            };
        }

        private static List<WeightedTransition> Row(params (BirdVisualState State, int Weight)[] transitions)
        {
            return transitions.Select(t => new WeightedTransition(t.State, t.Weight)).ToList();
        }

        /// <summary>
        /// Weighted transition option used by deterministic row selection.
        /// </summary>
        public sealed class WeightedTransition
        {
            /// <summary>
            /// Initializes a weighted transition record.
            /// </summary>
            public WeightedTransition(BirdVisualState state, int weight)
            {
                State = state;
                Weight = weight;
            }

            /// <summary>
            /// Destination state.
            /// </summary>
            public BirdVisualState State { get; }

            /// <summary>
            /// Relative transition weight.
            /// </summary>
            public int Weight { get; }
        }
    }
}
