using BirdCafe.Shared.Enums;
using BirdCafe.Shared.Models.PetStore;
using System.Collections.Generic;

namespace BirdCafe.Shared.Engine.Utils
{
    /// <summary>
    /// Static catalog data for Rick's Pet Store offerings and egg rewards.
    /// </summary>
    public static class PetStoreCatalog
    {
        public static IReadOnlyList<PetBirdDefinition> EntertainerBirds { get; } = new List<PetBirdDefinition>
        {
            new PetBirdDefinition
            {
                BirdId = "ent-budgie",
                SpeciesName = "Budgerigar",
                Price = 120m,
                Rarity = PetBirdRarity.Common,
                EffectDescription = "Small crowd favorite. +$6/day",
                DailyRevenueBonus = 6m,
                DailyPopularityBonus = 0.2f
            },
            new PetBirdDefinition
            {
                BirdId = "ent-cockatiel",
                SpeciesName = "Cockatiel",
                Price = 260m,
                Rarity = PetBirdRarity.Uncommon,
                EffectDescription = "Chirpy entertainer. +$14/day",
                DailyRevenueBonus = 14m,
                DailyPopularityBonus = 0.4f
            },
            new PetBirdDefinition
            {
                BirdId = "ent-african-grey",
                SpeciesName = "African Grey Parrot",
                Price = 520m,
                Rarity = PetBirdRarity.Rare,
                EffectDescription = "Charismatic mimic boosts repeat visits. +$32/day",
                DailyRevenueBonus = 32m,
                DailyPopularityBonus = 0.8f
            },
            new PetBirdDefinition
            {
                BirdId = "ent-hyacinth-macaw",
                SpeciesName = "Hyacinth Macaw",
                Price = 980m,
                Rarity = PetBirdRarity.Legendary,
                EffectDescription = "Rare showstopper that draws major attention. +$65/day",
                DailyRevenueBonus = 65m,
                DailyPopularityBonus = 1.4f
            },
            new PetBirdDefinition
            {
                BirdId = "ent-kakapo",
                SpeciesName = "Kakapo",
                Price = 1400m,
                Rarity = PetBirdRarity.Legendary,
                EffectDescription = "Ultra-rare real-world icon. +$95/day",
                DailyRevenueBonus = 95m,
                DailyPopularityBonus = 2.0f
            }
        };

        public static IReadOnlyList<PetSupplyDefinition> Supplies { get; } = new List<PetSupplyDefinition>
        {
            new PetSupplyDefinition { SupplyType = PetStoreSupplyType.BirdFood, DisplayName = "Bird Food", Price = 30m, EffectDescription = "Consumed on next day for +$5 cafe charm sales." },
            new PetSupplyDefinition { SupplyType = PetStoreSupplyType.Toys, DisplayName = "Toys", Price = 55m, EffectDescription = "Consumed on next day for +0.5 popularity." },
            new PetSupplyDefinition { SupplyType = PetStoreSupplyType.Costumes, DisplayName = "Costumes", Price = 90m, EffectDescription = "Permanent style inventory, +0.25 popularity/day while owned." },
            new PetSupplyDefinition { SupplyType = PetStoreSupplyType.MysteryEgg, DisplayName = "Mystery Egg Toy", Price = 300m, EffectDescription = "Expensive egg with one deterministic random reward." }
        };

        public static IReadOnlyList<EggRewardDefinition> EggRewards { get; } = new List<EggRewardDefinition>
        {
            new EggRewardDefinition { RewardId = "egg-buff-tipjar", DisplayName = "Lucky Tip Jar", RewardType = EggRewardType.Buff, Description = "Daily tips increase by +$12.", DailyRevenueBonus = 12m, DailyPopularityBonus = 0f },
            new EggRewardDefinition { RewardId = "egg-toy-oracle-whistle", DisplayName = "Oracle Whistle", RewardType = EggRewardType.UniqueToy, Description = "Unique toy boosts atmosphere (+0.8 popularity/day).", DailyRevenueBonus = 0m, DailyPopularityBonus = 0.8f },
            new EggRewardDefinition { RewardId = "egg-costume-gilded-cape", DisplayName = "Gilded Wing Cape", RewardType = EggRewardType.RareCostume, Description = "Rare costume set adds +$20 and +0.3 popularity/day.", DailyRevenueBonus = 20m, DailyPopularityBonus = 0.3f }
        };
    }
}
