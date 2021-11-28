using Boids.DamageClasses;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.GameContent.Creative;

namespace Boids.Items.Armor
{
	// The AutoloadEquip attribute automatically attaches an equip texture to this item.
	// Providing the EquipType.Legs value here will result in TML expecting a X_Legs.png file to be placed next to the item's main texture.
	[AutoloadEquip(EquipType.Legs)]
	public class WoodenBoidLeggings : ModItem
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Wooden Boid leggings");
			Tooltip.SetDefault("There seams to be a distant chirping," 
			                   + "\nJust get the full set we'll see what comes.");

			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults() {
			Item.width = 18; // Width of the item
			Item.height = 18; // Height of the item
			Item.sellPrice(gold: 1); // How many coins the item is worth
			Item.rare = ItemRarityID.Green; // The rarity of the item
			Item.defense = 5; // The amount of defense the item will give when equipped
			Item.DamageType = ModContent.GetInstance<BoidDamageClass>();
		}

		public override void UpdateEquip(Player player) {
			player.moveSpeed += 0.05f; // Increase the movement speed of the player
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe().AddIngredient(ItemID.Wood, 20)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
}
