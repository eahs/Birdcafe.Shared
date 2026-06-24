using BirdCafe.Shared.Enums;
using BirdCafe.Shared.Models.Simulation;
using BirdCafe.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BirdCafe.Shared
{
    /// <summary>
    /// Contains day-introduction, simulation execution, and timeline-projection operations for
    /// <see cref="BirdCafeGame"/>.
    /// </summary>
    public partial class BirdCafeGame
    {
        /// <summary>
        /// Caches the current day's deterministic simulation result for repeated UI playback.
        /// </summary>
        /// <remarks>
        /// This value is intentionally facade-only. The authoritative historical result remains in
        /// save-state after the simulation manager records it.
        /// </remarks>
        private DaySimulationResult _cachedSimResult;

        /// <summary>
        /// Builds the day-introduction view model shown before simulation playback.
        /// </summary>
        /// <returns>A UI-ready summary of the day, player, cafe, and popularity.</returns>
        public DayIntroViewModel GetDayIntro()
        {
            var state = _controller.CurrentState;

            // The view model flattens domain state so front ends do not need to understand the save
            // model's internal object graph.
            return new DayIntroViewModel
            {
                DayNumber = state.CurrentDayNumber,
                DayName = state.CurrentDayName.ToString(),
                CafeName = state.Cafe.CafeName,
                Popularity = (int)state.Cafe.Popularity,
                Message = $"Good morning <#008DD4>{state.Profile.DisplayName}</color>! Today is {state.CurrentDayName}, day <#6c18a3>{state.CurrentDayNumber}</color>. Let's make it a great day at {state.Cafe.CafeName}. Good luck!"
            };
        }

        /// <summary>
        /// Starts the current day's simulation or reuses its cached result for replay.
        /// </summary>
        /// <returns><see langword="true"/> when playback can begin; otherwise <see langword="false"/>.</returns>
        public bool StartSimulationPlayback()
        {
            // Replaying the current day must not run the simulation a second time. Re-execution
            // would duplicate state mutations, sales, ledger entries, and bird-stat changes.
            if (_cachedSimResult != null
                && _cachedSimResult.DayNumber == _controller.CurrentState.CurrentDayNumber)
            {
                TransitionTo(GameScreen.DaySimulation);
                return true;
            }

            // SimulationManager owns phase validation, deterministic randomness, and all mutations.
            var result = _controller.Simulation.RunDaySimulation();
            if (!result.IsSuccess)
            {
                FireToast(result.UserMessage);
                return false;
            }

            // Cache the manager's completed result solely for timeline playback and evening summary.
            _cachedSimResult = (DaySimulationResult)result.Payload;
            TransitionTo(GameScreen.DaySimulation);
            return true;
        }

        /// <summary>
        /// Projects simulation timeline records into display-ready playback events.
        /// </summary>
        /// <returns>Timeline events ordered in the same sequence as the simulation result.</returns>
        public List<UiTimelineEvent> GetDayTimeline()
        {
            if (_cachedSimResult == null)
            {
                // Returning an empty list keeps callers simple and avoids forcing every UI to add a
                // separate null-state branch before rendering a timeline.
                return new List<UiTimelineEvent>();
            }

            float simDuration = _controller.CurrentState.Config.DayDurationSeconds;
            TimeSpan startOfDay = TimeSpan.FromHours(7);
            double realHoursOpen = 8.0;

            return _cachedSimResult.Timeline.Select(t =>
            {
                // Convert compressed simulation seconds into a friendly in-world clock time. This
                // is display math only and does not affect deterministic simulation outcomes.
                double pct = t.TimeSeconds / simDuration;
                TimeSpan eventTime = startOfDay.Add(TimeSpan.FromHours(realHoursOpen * pct));
                string timeString = DateTime.Today.Add(eventTime).ToString("hh:mm tt");

                // A missing bird id can be valid for cafe-wide events, so provide readable fallback
                // text rather than exposing null to the UI.
                string birdName = _controller.CurrentState.Birds
                    .FirstOrDefault(b => b.Id == t.BirdId)?.Name ?? "Unknown";

                // Prefer the explicit simulation reason. Older or simpler event producers may omit
                // it, in which case the facade creates a concise description for playback.
                string description = t.ReasonCode;
                if (string.IsNullOrEmpty(description))
                {
                    description = t.EventType.ToString();

                    if (t.EventType == SimulationTimelineEventType.CustomerArrived && t.Product.HasValue)
                    {
                        description = $"Arrived wanting {t.Product}";
                    }

                    if (t.EventType == SimulationTimelineEventType.ServiceCompleted && t.MoneyDelta > 0)
                    {
                        description = $"Served {t.Product} (+${t.MoneyDelta:F2})";
                    }
                }

                return new UiTimelineEvent
                {
                    TimeSeconds = t.TimeSeconds,
                    FormattedTime = timeString,
                    EventType = t.EventType.ToString(),
                    Description = description,
                    BirdName = birdName,
                    IconId = t.Product.HasValue ? t.Product.Value.ToString() : null,
                    MoneyDelta = t.MoneyDelta,
                    PopularityDelta = t.PopularityDelta
                };
            }).ToList();
        }

        /// <summary>
        /// Advances from completed simulation playback into evening progression when valid.
        /// </summary>
        public void FinishSimulation()
        {
            // The manager verifies that the simulation is complete and performs the authoritative
            // phase transition. The facade changes screens only after that operation succeeds.
            var result = _controller.Simulation.AdvanceFromSimulation();
            if (result.IsSuccess)
            {
                TransitionTo(GameScreen.Hub);
            }
        }
    }
}
