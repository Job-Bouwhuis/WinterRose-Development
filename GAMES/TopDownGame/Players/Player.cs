using Cyotek.Drawing.BitmapFont;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopDownGame.Inventories;
using WinterRose;
using WinterRose.Monogame;
using WinterRose.Monogame.DamageSystem;
using WinterRose.Monogame.ModdingSystem;
using WinterRose.Monogame.Weapons;

namespace TopDownGame.Players
{
    [RequireComponent<Vitality>]
    public class Player : ObjectBehavior
    {
        /// <summary>
        /// The players inventory
        /// </summary>
        [WFInclude]
        public Inventory Inventory { get; private set; }
        /// <summary>
        /// The players name
        /// </summary>
        [WFInclude]
        public string Name { get; set; }
        /// <summary>
        /// The player ID
        /// </summary>
        [WFInclude]
        public long ID { get; set; }
        /// <summary>
        /// The mods for the player
        /// </summary>
        [WFInclude]
        public ModContainer<Player> ModContainer { get; } = new();
        /// <summary>
        /// The players vitality component
        /// </summary>
        [WFInclude]
        public Vitality Vitality { get; set; }

        /// <summary>
        /// The players primary weapon equipped
        /// </summary>
        public Weapon? PrimaryWeapon { get; set; } = null;

        private Player() // for serialization
        { }

        public Player(string name)
        {
            Name = name;
            string invAssetName = name + "_Inv";

            if (!AssetDatabase.AssetExists(invAssetName))
                AssetDatabase.GetOrDeclareAsset<Inventory>(invAssetName);
            Inventory = AssetDatabase.LoadAsset<Inventory>(invAssetName);
        }

        protected override void Awake()
        {
            Vitality = FetchComponent<Vitality>()!;
        }

        protected override void Update() => Inventory.ValidateItemStacks();
    }
}
