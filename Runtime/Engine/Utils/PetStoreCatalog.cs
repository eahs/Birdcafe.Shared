using BirdCafe.Shared.Enums;
using BirdCafe.Shared.Models.Cafe;
using System.Collections.Generic;

namespace BirdCafe.Shared.Engine.Utils
{
    public static class PetStoreCatalog
    {
        public static readonly List<PetBirdCatalogEntry> Birds = new List<PetBirdCatalogEntry>
        {
            new PetBirdCatalogEntry { Id = "budgie-blue", Name = "Blue Budgie", Rarity = PetBirdRarityTier.Common, Price = 120m, CustomerBonus = 1, FlatRevenueBonus = 2m, EffectDescription = "+1 daily guest, +$2 daily tips vibe" },
            new PetBirdCatalogEntry { Id = "cockatiel-sun", Name = "Sun Cockatiel", Rarity = PetBirdRarityTier.Uncommon, Price = 280m, CustomerBonus = 2, FlatRevenueBonus = 5m, EffectDescription = "+2 daily guests, +$5 daily revenue" },
            new PetBirdCatalogEntry { Id = "toucan-rainbow", Name = "Rainbow Toucan", Rarity = PetBirdRarityTier.Rare, Price = 700m, CustomerBonus = 4, FlatRevenueBonus = 12m, EffectDescription = "+4 daily guests, +$12 daily revenue" },
            new PetBirdCatalogEntry { Id = "shoebill-royal", Name = "Royal Shoebill", Rarity = PetBirdRarityTier.Legendary, Price = 1500m, CustomerBonus = 7, FlatRevenueBonus = 28m, EffectDescription = "+7 daily guests, +$28 daily revenue" },
            new PetBirdCatalogEntry { Id = "kakapo-ancient", Name = "Ancient Kakapo", Rarity = PetBirdRarityTier.Legendary, Price = 2200m, CustomerBonus = 10, FlatRevenueBonus = 45m, EffectDescription = "+10 daily guests, +$45 daily revenue" }
        };

        public static readonly List<PetSupplyCatalogEntry> Supplies = new List<PetSupplyCatalogEntry>
        {
            new PetSupplyCatalogEntry { Type = PetStoreSupplyType.BirdFood, Name = "Bird Food", Price = 20m, EffectDescription = "Consumed next day for +1 customer." },
            new PetSupplyCatalogEntry { Type = PetStoreSupplyType.Toys, Name = "Toys", Price = 45m, EffectDescription = "Consumed next day for +$4 revenue boost." },
            new PetSupplyCatalogEntry { Type = PetStoreSupplyType.Costumes, Name = "Costumes", Price = 80m, EffectDescription = "Consumed next day for +1 popularity at end of day." },
            new PetSupplyCatalogEntry { Type = PetStoreSupplyType.MysteryEgg, Name = "Mystery Egg Toy", Price = 350m, EffectDescription = "Expensive egg with one deterministic random reward." }
        };

        public static readonly List<PetEggRewardEntry> EggRewards = new List<PetEggRewardEntry>
        {
            new PetEggRewardEntry { Id = "buff-golden-perch", Name = "Golden Perch Buff", RewardType = "Buff", Description = "Permanent +2 daily customers.", BonusCustomers = 2 },
            new PetEggRewardEntry { Id = "toy-nebula-spinner", Name = "Nebula Spinner", RewardType = "Unique Toy", Description = "Unique toy grants permanent +$9 daily revenue.", FlatRevenueBonus = 9m, UnlockUniqueToy = true },
            new PetEggRewardEntry { Id = "costume-phoenix-cape", Name = "Phoenix Cape", RewardType = "Rare Costume", Description = "Rare costume gives permanent +$14 daily revenue and +1 daily customer.", BonusCustomers = 1, FlatRevenueBonus = 14m, UnlockRareCostume = true },
            new PetEggRewardEntry { Id = "buff-song-aura", Name = "Song Aura", RewardType = "Buff", Description = "Permanent +$7 daily revenue.", FlatRevenueBonus = 7m }
        };
    }
}
