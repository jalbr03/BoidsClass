using Boids.DamageClasses;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Boids.Items.Armor
{
    // The AutoloadEquip attribute automatically attaches an equip texture to this item.
    // Providing the EquipType.Body value here will result in TML expecting X_Arms.png, X_Body.png and X_FemaleBody.png sprite-sheet files to be placed next to the item's main texture.
    [AutoloadEquip(EquipType.Body)]
    public class WoodenBoidBreastplate : ModItem
    {
        public override void SetStaticDefaults() {
            base.SetStaticDefaults();
            DisplayName.SetDefault("Wooden Boid Breastplate");
            Tooltip.SetDefault("There seams to be a distant chirping," 
                               + "\nMaybe it will come closer if you get the full set.");

            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults() {
            Item.width = 18; // Width of the item
            Item.height = 18; // Height of the item
            Item.sellPrice(gold: 1); // How many coins the item is worth
            Item.rare = ItemRarityID.Green; // The rarity of the item
            Item.defense = 1; // The amount of defense the item will give when equipped
            Item.DamageType = ModContent.GetInstance<BoidDamageClass>();
        }


        // Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
        public override void AddRecipes() {
            CreateRecipe().AddIngredient(ItemID.Wood, 20)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}