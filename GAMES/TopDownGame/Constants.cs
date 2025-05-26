using Microsoft.Xna.Framework;
using System;
using WinterRose.Monogame;

namespace TopDownGame
{
    public static class Constants
    {
        public const string RARITY_COMMON_NAME = "Common";
        public const string RARITY_UNCOMMON_NAME = "Uncommon";
        public const string RARITY_RARE_NAME = "Rare";
        public const string RARITY_LEGENDARY_NAME = "Legendary";
        public const string RARITY_EXOTIC_NAME = "Exotic";
        public const string RARITY_UNIQUE_NAME = "Unique";
        public const string RARITY_QUEST_NAME = "Quest";
        public const string RARITY_WHIMSICAL_NAME = "Dajuska";

        public static readonly Rarity CommonRarity;
        public static readonly Rarity UncommonRarity;
        public static readonly Rarity RareRarity;
        public static readonly Rarity LegendaryRarity;
        public static readonly Rarity ExoticRarity;
        public static readonly Rarity UniqueRarity;
        public static readonly Rarity QuestRarity;
        public static readonly Rarity WhimsicalRarity;

        static Constants()
        {
            CommonRarity = Rarity.CreateRarity(0, RARITY_COMMON_NAME, new Color(200, 200, 200));
            UncommonRarity = Rarity.CreateRarity(1, RARITY_UNCOMMON_NAME, new Color(30, 255, 30));
            RareRarity = Rarity.CreateRarity(2, RARITY_RARE_NAME, new Color(0, 112, 221));
            LegendaryRarity = Rarity.CreateRarity(3, RARITY_LEGENDARY_NAME, new Color(255, 128, 0));
            ExoticRarity = Rarity.CreateRarity(4, RARITY_EXOTIC_NAME, new Color(200, 0, 200));
            UniqueRarity = Rarity.CreateRarity(5, RARITY_UNIQUE_NAME, new Color(255, 255, 100));
            QuestRarity = Rarity.CreateRarity(6, RARITY_QUEST_NAME, new Color(200, 160, 0));
            WhimsicalRarity = Rarity.CreateRarity(7, RARITY_WHIMSICAL_NAME, new Color(255, 182, 193));
        }

        internal static void Init() { }
    }
}
