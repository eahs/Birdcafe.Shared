using BirdCafe.Shared.Enums;
using BirdCafe.Shared.Models.Birds;
using System.Collections.Generic;

namespace BirdCafe.Shared.Engine.Utils
{
    /// <summary>
    /// Static definitions used by Pete's Pet Store.
    /// </summary>
    public static class PetStoreCatalog
    {
        public const string BirdFoodItemId = "BirdFoodBag";
        public const string BirdFoodSeedMixItemId = "BirdFood_SeedMix";
        public const string BirdFoodFruitMedleyItemId = "BirdFood_FruitMedley";
        public const string BirdFoodNutriPelletsItemId = "BirdFood_NutriPellets";
        public const string ToyFeatherWandId = "Toy_FeatherWand";
        public const string ToyBellOrbId = "Toy_BellOrb";
        public const string CostumeBandanaId = "Costume_Bandana";
        public const string CostumeRoyalCapeId = "Costume_RoyalCape";
        public const string SpecialEggToyItemId = "SpecialEggToy";

        public const decimal BirdFoodPrice = 18m;
        public const decimal BirdFoodSeedMixPrice = 18m;
        public const decimal BirdFoodFruitMedleyPrice = 20m;
        public const decimal BirdFoodNutriPelletsPrice = 22m;
        public const decimal FeatherWandPrice = 45m;
        public const decimal BellOrbPrice = 70m;
        public const decimal BandanaPrice = 50m;
        public const decimal RoyalCapePrice = 110m;
        public const decimal SpecialEggToyPrice = 300m;

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
                Reliability = 13,
                PreferredFoods = new List<BirdFoodType> { BirdFoodType.SeedMix }
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
                Reliability = 17,
                PreferredFoods = new List<BirdFoodType> { BirdFoodType.FruitMedley }
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
                Reliability = 21,
                PreferredFoods = new List<BirdFoodType> { BirdFoodType.NutriPellets, BirdFoodType.FruitMedley }
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
                Reliability = 28,
                PreferredFoods = new List<BirdFoodType> { BirdFoodType.NutriPellets }
            }
        };

        public static readonly List<PetStoreSupplyDefinition> SupplyOffers = new List<PetStoreSupplyDefinition>
        {
            new PetStoreSupplyDefinition
            {
                ItemId = BirdFoodSeedMixItemId,
                DisplayName = "Seed Mix",
                SupplyType = PetStoreSupplyType.BirdFood,
                CategoryText = "Bird Food",
                Price = BirdFoodSeedMixPrice,
                EffectText = "Basic food for flock birds. Feeding uses stored inventory only.",
                ExpenseCategory = ExpenseCategory.FoodAndSupplies,
                BirdFoodType = BirdFoodType.SeedMix
            },
            new PetStoreSupplyDefinition
            {
                ItemId = BirdFoodFruitMedleyItemId,
                DisplayName = "Fruit Medley",
                SupplyType = PetStoreSupplyType.BirdFood,
                CategoryText = "Bird Food",
                Price = BirdFoodFruitMedleyPrice,
                EffectText = "Preferred by fruit-loving birds; boosts trust faster when matched.",
                ExpenseCategory = ExpenseCategory.FoodAndSupplies,
                BirdFoodType = BirdFoodType.FruitMedley
            },
            new PetStoreSupplyDefinition
            {
                ItemId = BirdFoodNutriPelletsItemId,
                DisplayName = "Nutri Pellets",
                SupplyType = PetStoreSupplyType.BirdFood,
                CategoryText = "Bird Food",
                Price = BirdFoodNutriPelletsPrice,
                EffectText = "Dense nutrition favored by high-performance birds.",
                ExpenseCategory = ExpenseCategory.FoodAndSupplies,
                BirdFoodType = BirdFoodType.NutriPellets
            },
            new PetStoreSupplyDefinition
            {
                ItemId = ToyFeatherWandId,
                DisplayName = "Feather Wand",
                SupplyType = PetStoreSupplyType.Toy,
                CategoryText = "Toy",
                Price = FeatherWandPrice,
                EffectText = "Owned toy collection item.",
                ExpenseCategory = ExpenseCategory.ToysAndActivities
            },
            new PetStoreSupplyDefinition
            {
                ItemId = ToyBellOrbId,
                DisplayName = "Bell Orb",
                SupplyType = PetStoreSupplyType.Toy,
                CategoryText = "Toy",
                Price = BellOrbPrice,
                EffectText = "Owned toy collection item.",
                ExpenseCategory = ExpenseCategory.ToysAndActivities
            },
            new PetStoreSupplyDefinition
            {
                ItemId = CostumeBandanaId,
                DisplayName = "Cafe Bandana",
                SupplyType = PetStoreSupplyType.Costume,
                CategoryText = "Costume",
                Price = BandanaPrice,
                EffectText = "Owned costume unlock.",
                ExpenseCategory = ExpenseCategory.UpgradesAndCustomization
            },
            new PetStoreSupplyDefinition
            {
                ItemId = CostumeRoyalCapeId,
                DisplayName = "Royal Cape",
                SupplyType = PetStoreSupplyType.Costume,
                CategoryText = "Costume",
                Price = RoyalCapePrice,
                EffectText = "Rare costume unlock.",
                ExpenseCategory = ExpenseCategory.UpgradesAndCustomization
            },
            new PetStoreSupplyDefinition
            {
                ItemId = SpecialEggToyItemId,
                DisplayName = "Special Egg Toy",
                SupplyType = PetStoreSupplyType.SpecialEggToy,
                CategoryText = "Special Egg Toy",
                Price = SpecialEggToyPrice,
                EffectText = "Open to receive one deterministic reward.",
                ExpenseCategory = ExpenseCategory.Miscellaneous
            }
        };

        public static BirdSpeciesOffer FindBirdOffer(string speciesId)
        {
            return BirdOffers.Find(b => b.SpeciesId == speciesId);
        }

        public static List<PetStoreSupplyDefinition> GetSupplyOffers()
        {
            return SupplyOffers;
        }

        public static PetStoreSupplyDefinition FindSupplyOffer(string itemId, PetStoreSupplyType supplyType)
        {
            if (supplyType == PetStoreSupplyType.BirdFood && itemId == BirdFoodItemId)
            {
                itemId = BirdFoodSeedMixItemId;
            }

            return SupplyOffers.Find(offer => offer.ItemId == itemId && offer.SupplyType == supplyType);
        }

        public static string BuildBirdName(string speciesName, int dayNumber, int existingBirdCount)
        {
            return $"{speciesName} #{dayNumber}-{existingBirdCount + 1}";
        }

        public static BirdFoodType? GetFoodTypeForItem(string itemId)
        {
            var offer = FindSupplyOffer(itemId, PetStoreSupplyType.BirdFood);
            return offer?.BirdFoodType;
        }
    }
}
