
using System;
using BirdCafe.Shared.Models.Birds;

namespace BirdCafe.Shared.Engine.Utils
{
    /// <summary>
    /// Factory class for creating Bird instances.
    /// Encapsulates default values so they aren't scattered in MetaManager.
    /// </summary>
    public static class BirdFactory
    {
        /// <summary>
        /// Creates the specific starter bird for a new game.
        /// </summary>
        /// <returns>A fully initialized Bird object.</returns>
        public static Bird CreateStarterBird()
        {
            return new Bird 
            { 
                Name = "Peep", 
                SpeciesId = "Sparrow_Standard",
                PrimaryColorHex = "#FFCC00",
                Productivity = 20,
                Energy = 100,
                Mood = 80,
                Hunger = 100,
                Friendliness = 15,
                Reliability = 10
            };
        }
    }
}