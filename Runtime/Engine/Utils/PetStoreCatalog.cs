using BirdCafe.Shared.Enums;
using BirdCafe.Shared.Models.Birds;
using System;
using System.Collections.Generic;

namespace BirdCafe.Shared.Engine.Utils
{
    /// <summary>
    /// Static definitions used by Rick's Pet Store.
    /// </summary>
    public static class PetStoreCatalog
    {
        public static readonly List<BirdSpeciesOffer> BirdOffers = new List<BirdSpeciesOffer>
        {
            new BirdSpeciesOffer
            {
                SpeciesId = "Budgerigar",
                DisplayName = "Budgerigar",
                Rarity = BirdRarity.Common,
                Price = 120m,
                FlavorDescription = "Friendly starter flock bird. Solid cafe helper.",
                Productivity = 22,
                Friendliness = 16,
                Reliability = 13
            },
            new BirdSpeciesOffer
            {
                SpeciesId = "Cockatiel",
                DisplayName = "Cockatiel",
                Rarity = BirdRarity.Uncommon,
                Price = 260m,
                FlavorDescription = "Charismatic whistler that improves customer vibes.",
                Productivity = 27,
                Friendliness = 22,
                Reliability = 17
            },
            new BirdSpeciesOffer
            {
                SpeciesId = "HyacinthMacaw",
                DisplayName = "Hyacinth Macaw",
                Rarity = BirdRarity.Rare,
                Price = 520m,
                FlavorDescription = "Large rare macaw with excellent service pace.",
                Productivity = 35,
                Friendliness = 24,
                Reliability = 21
            },
            new BirdSpeciesOffer
            {
                SpeciesId = "PalmCockatoo",
                DisplayName = "Palm Cockatoo",
                Rarity = BirdRarity.Exotic,
                Price = 900m,
                FlavorDescription = "A rare real-world species. Elite all-around cafe performer.",
                Productivity = 42,
                Friendliness = 30,
                Reliability = 28
            }
        };

        public const string BirdFoodItemId = "BirdFoodBag";
        public const string ToyFeatherWandId = "Toy_FeatherWand";
        public const string ToyBellOrbId = "Toy_BellOrb";
        public const string CostumeBandanaId = "Costume_Bandana";
        public const string CostumeRoyalCapeId = "Costume_RoyalCape";
        public const decimal BirdFoodPrice = 18m;
        public const decimal FeatherWandPrice = 45m;
        public const decimal BellOrbPrice = 70m;
        public const decimal BandanaPrice = 50m;
        public const decimal RoyalCapePrice = 110m;
        public const decimal SpecialEggToyPrice = 300m;

        public static BirdSpeciesOffer FindBirdOffer(string speciesId)
        {
            return BirdOffers.Find(b => b.SpeciesId == speciesId);
        }

        public static string BuildBirdName(string speciesName, int dayNumber, int existingBirdCount)
        {
            return $"{speciesName} #{dayNumber}-{existingBirdCount + 1}";
        }

        public static ExpenseCategory GetCategoryForSupply(PetStoreSupplyType supplyType)
        {
            return supplyType switch
            {
                PetStoreSupplyType.BirdFood => ExpenseCategory.FoodAndSupplies,
                PetStoreSupplyType.Toy => ExpenseCategory.ToysAndActivities,
                PetStoreSupplyType.Costume => ExpenseCategory.UpgradesAndCustomization,
                _ => ExpenseCategory.Miscellaneous
            };
        }
    }
}
