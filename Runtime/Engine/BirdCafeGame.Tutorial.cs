using BirdCafe.Shared.ViewModels;
using System.Collections.Generic;

namespace BirdCafe.Shared
{
    /// <summary>
    /// Contains first-run tutorial content and tutorial navigation for
    /// <see cref="BirdCafeGame"/>.
    /// </summary>
    public partial class BirdCafeGame
    {
        /// <summary>
        /// Builds the tutorial content shown to first-time players.
        /// </summary>
        /// <returns>A UI-ready tutorial containing the introductory gameplay steps.</returns>
        public TutorialViewModel GetTutorialContent()
        {
            // Tutorial wording is returned as data so Unity, console, and future front ends can
            // render the same instructional sequence using their own presentation controls.
            return new TutorialViewModel
            {
                Title = "Your First Day at the Bird Cafe",
                Steps = new List<TutorialStep>
                {
                    new TutorialStep
                    {
                        Title = "Step 1: Start the work day",
                        Description = "We gave you starter coffee. Open the cafe and let your birds entertain customers."
                    },
                    new TutorialStep
                    {
                        Title = "Step 2: Take care of your birds at night",
                        Description = "Feed, rest, and heal birds so they are ready for tomorrow."
                    },
                    new TutorialStep
                    {
                        Title = "Step 3: Plan inventory",
                        Description = "Choose how much to sell for each day after."
                    }
                }
            };
        }

        /// <summary>
        /// Completes tutorial navigation and enters the first day-introduction screen.
        /// </summary>
        public void CompleteTutorial()
        {
            // Tutorial completion changes only navigation here; durable tutorial flags remain the
            // responsibility of the engine's meta/save workflow.
            TransitionTo(GameScreen.DayIntro);
        }
    }
}
