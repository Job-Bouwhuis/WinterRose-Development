using Microsoft.Xna.Framework;
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

        public static Rarity CommonRarity => Rarity.CreateRarity(0, RARITY_COMMON_NAME, new Color(200, 200, 200));
        public static Rarity UncommonRarity => Rarity.CreateRarity(1, RARITY_UNCOMMON_NAME, new Color(30, 255, 30));
        public static Rarity RareRarity => Rarity.CreateRarity(2, RARITY_RARE_NAME, new Color(0, 112, 221));
        public static Rarity LegendaryRarity => Rarity.CreateRarity(3, RARITY_LEGENDARY_NAME, new Color(255, 128, 0));
        public static Rarity ExoticRarity => Rarity.CreateRarity(4, RARITY_EXOTIC_NAME, new Color(200, 0, 200));
        public static Rarity UniqueRarity => Rarity.CreateRarity(5, RARITY_UNIQUE_NAME, new Color(255, 255, 100));
        public static Rarity QuestRarity => Rarity.CreateRarity(6, RARITY_QUEST_NAME, new Color(200, 160, 0));
        public static Rarity WhimsicalRarity => Rarity.CreateRarity(7, RARITY_WHIMSICAL_NAME, new Color(255, 182, 193));
    }
}
