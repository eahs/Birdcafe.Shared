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

        /// <summary>
        /// Shared list of bird offers consumed by store UIs and purchase validation logic.
        /// </summary>
        public static readonly List<BirdSpeciesOffer> BirdOffers = new List<BirdSpeciesOffer>
        {
            new BirdSpeciesOffer
            {
                SpeciesId = "budgie",
                DisplayName = "Buddy",
                Rarity = BirdRarity.Common,
                Price = 120m,
                FlavorDescription = "Friendly helper who loves to entertain.",
                Productivity = 22,
                Friendliness = 16,
                Reliability = 13,
                PreferredFoods = new List<BirdFoodType> { BirdFoodType.SeedMix }
            },
            new BirdSpeciesOffer
            {
                SpeciesId = "cockatiel",
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
                SpeciesId = "lorikeet",
                DisplayName = "Keet",
                Rarity = BirdRarity.Rare,
                Price = 520m,
                FlavorDescription = "A sweet but rare attention-grabber that keeps the cafe lively.",
                Productivity = 35,
                Friendliness = 24,
                Reliability = 21,
                PreferredFoods = new List<BirdFoodType> { BirdFoodType.NutriPellets, BirdFoodType.FruitMedley }
            },
            new BirdSpeciesOffer
            {
                SpeciesId = "kingfisher",
                DisplayName = "King Kiwi",
                Rarity = BirdRarity.Exotic,
                Price = 900m,
                FlavorDescription = "Exotic little favorite that draws extra attention.",
                Productivity = 42,
                Friendliness = 30,
                Reliability = 28,
                PreferredFoods = new List<BirdFoodType> { BirdFoodType.NutriPellets }
            }
        };

        /// <summary>
        /// Shared list of supply offers, including reward-only definitions used for inventory display.
        /// </summary>
        public static readonly List<PetStoreSupplyDefinition> SupplyOffers = new List<PetStoreSupplyDefinition>
        {
            new PetStoreSupplyDefinition
            {
                ItemId = BirdFoodType.SeedMix.ToString(),
                DisplayName = "Seed Mix",
                SupplyType = PetStoreSupplyType.BirdFood,
                CategoryText = "Bird Food",
                Price = BirdFoodSeedMixPrice,
                EffectText = "Basic food for flock birds. Feeding uses stored inventory only.",
                ExpenseCategory = ExpenseCategory.FoodAndSupplies,
                BirdFoodType = BirdFoodType.SeedMix,
                Buyable = true
            },
            new PetStoreSupplyDefinition
            {
                ItemId = BirdFoodType.FruitMedley.ToString(),
                DisplayName = "Fruit Medley",
                SupplyType = PetStoreSupplyType.BirdFood,
                CategoryText = "Bird Food",
                Price = BirdFoodFruitMedleyPrice,
                EffectText = "Preferred by fruit-loving birds; boosts trust faster when matched.",
                ExpenseCategory = ExpenseCategory.FoodAndSupplies,
                BirdFoodType = BirdFoodType.FruitMedley,
                Buyable = true
            },
            new PetStoreSupplyDefinition
            {
                ItemId = BirdFoodType.NutriPellets.ToString(),
                DisplayName = "Nutri Pellets",
                SupplyType = PetStoreSupplyType.BirdFood,
                CategoryText = "Bird Food",
                Price = BirdFoodNutriPelletsPrice,
                EffectText = "Dense nutrition favored by high-performance birds.",
                ExpenseCategory = ExpenseCategory.FoodAndSupplies,
                BirdFoodType = BirdFoodType.NutriPellets,
                Buyable = true
            },
            new PetStoreSupplyDefinition
            {
                ItemId = ToyFeatherWandId,
                DisplayName = "Feather Wand",
                SupplyType = PetStoreSupplyType.Toy,
                CategoryText = "Toy",
                Price = FeatherWandPrice,
                EffectText = "Owned toy collection item.",
                ExpenseCategory = ExpenseCategory.ToysAndActivities,
                Buyable = true
            },
            new PetStoreSupplyDefinition
            {
                ItemId = ToyBellOrbId,
                DisplayName = "Bell Orb",
                SupplyType = PetStoreSupplyType.Toy,
                CategoryText = "Toy",
                Price = BellOrbPrice,
                EffectText = "Owned toy collection item.",
                ExpenseCategory = ExpenseCategory.ToysAndActivities,
                Buyable = true
            },
            
            new PetStoreSupplyDefinition
            {
                ItemId = CostumeBandanaId,
                DisplayName = "Cafe Bandana",
                SupplyType = PetStoreSupplyType.Costume,
                CategoryText = "Costume",
                Price = BandanaPrice,
                EffectText = "Owned costume unlock.",
                ExpenseCategory = ExpenseCategory.UpgradesAndCustomization,
                Buyable = true  
            },
            
            new PetStoreSupplyDefinition
            {
                ItemId = CostumeRoyalCapeId,
                DisplayName = "Royal Cape",
                SupplyType = PetStoreSupplyType.Costume,
                CategoryText = "Costume",
                Price = RoyalCapePrice,
                EffectText = "Rare costume unlock.",
                ExpenseCategory = ExpenseCategory.UpgradesAndCustomization,
                Buyable = true
            },
            new PetStoreSupplyDefinition
            {
                ItemId = SpecialEggToyItemId,
                DisplayName = "Special Egg Toy",
                SupplyType = PetStoreSupplyType.SpecialEggToy,
                CategoryText = "Special Egg Toy",
                Price = SpecialEggToyPrice,
                EffectText = "Open to receive one deterministic reward.",
                ExpenseCategory = ExpenseCategory.Miscellaneous,
                Buyable = true
            },

            //Egg Rewards
            new PetStoreSupplyDefinition
            {
                ItemId = "Toy_StarlightSpinner",
                DisplayName = "Starlight Spinner",
                SupplyType = PetStoreSupplyType.Toy,
                CategoryText = "Toy",
                Price = 0m,
                EffectText = "Unique toy added to your collection.",
                ExpenseCategory = ExpenseCategory.ToysAndActivities,
                Buyable = false
            },
            new PetStoreSupplyDefinition
            {
                ItemId = "Costume_GoldenVest",
                DisplayName = "Golden Vest",
                SupplyType = PetStoreSupplyType.Costume,
                CategoryText = "Costume",
                Price = 0m,
                EffectText = "Rare costume unlock.",
                ExpenseCategory = ExpenseCategory.UpgradesAndCustomization,
                Buyable = false
            },
        };

        /// <summary>
        /// Resolves a bird offer by species id.
        /// </summary>
        /// <param name="speciesId">Stable species id stored in save data and purchase actions.</param>
        /// <returns>The matching offer definition, or <see langword="null"/> when no offer exists.</returns>
        public static BirdSpeciesOffer FindBirdOffer(string speciesId)
        {
            return BirdOffers.Find(b => b.SpeciesId == speciesId);
        }

        /// <summary>
        /// Returns the shared supply catalog used by both UI listings and purchase validation.
        /// </summary>
        public static List<PetStoreSupplyDefinition> GetSupplyOffers()
        {
            return SupplyOffers;
        }

        /// <summary>
        /// Resolves a supply offer by item id and supply category.
        /// </summary>
        public static PetStoreSupplyDefinition FindSupplyOffer(string itemId, PetStoreSupplyType supplyType)
        {
            return SupplyOffers.Find(offer => offer.ItemId == itemId && offer.SupplyType == supplyType);
        }

        /// <summary>
        /// Builds a deterministic generated name for newly purchased birds.
        /// </summary>
        /// <remarks>
        /// The day and roster count suffixes help keep names readable and reduce collisions in save files.
        /// </remarks>
        public static string BuildBirdName(string speciesName, int dayNumber, int existingBirdCount)
        {
            return $"{speciesName}";
        }

        /// <summary>
        /// Maps a bird-food item id back to its food subtype.
        /// </summary>
        /// <returns>
        /// The mapped food type for bird-food catalog entries; otherwise <see langword="null"/>.
        /// </returns>
        public static BirdFoodType? GetFoodTypeForItem(string itemId)
        {
            var offer = FindSupplyOffer(itemId, PetStoreSupplyType.BirdFood);
            return offer?.BirdFoodType;
        }
    }
}
