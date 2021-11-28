using System;
using Boids.Buffs;
using Boids.DamageClasses;
using Boids.ModdedPlayer;
using Boids.Projectiles.Minions;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.GameContent.Creative;

namespace Boids.Items.Armor
{
	// The AutoloadEquip attribute automatically attaches an equip texture to this item.
	// Providing the EquipType.Head value here will result in TML expecting a X_Head.png file to be placed next to the item's main texture.
	[AutoloadEquip(EquipType.Head)]
	public class WoodenBoidHelmet : ModItem
	{
		private Random random = new Random();
		
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Wooden Boid Helmet");
			Tooltip.SetDefault("There seams to be a distant chirping," 
			                   + "\nGet the full set and see what comes.");

			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 3;
			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the player target anywhere on the whole screen while using a controller
			ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
		}

		public override void SetDefaults() {
			Item.width = 18; // Width of the item
			Item.height = 18; // Height of the item
			Item.sellPrice(gold: 1); // How many coins the item is worth
			Item.rare = ItemRarityID.Green; // The rarity of the item
			Item.defense = 5; // The amount of defense the item will give when equipped
			Item.buffType = ModContent.BuffType<BoidMinionBuff>();
			Item.DamageType = ModContent.GetInstance<BoidDamageClass>();
		}

		// IsArmorSet determines what armor pieces are needed for the setbonus to take effect
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<WoodenBoidBreastplate>() && legs.type == ModContent.ItemType<WoodenBoidLeggings>();
		}

		// UpdateArmorSet allows you to give set bonuses to the armor.
		public override void UpdateArmorSet(Player player) {
			player.setBonus = "Here come the Boids"; // This is the setbonus tooltip

			var spawnchance = random.Next(0, 100);

			if (spawnchance > 3) return;

			if (Main.LocalPlayer.ownedProjectileCounts[ModContent.ProjectileType<BoidMinion>()] >=
			    player.GetModPlayer<CustomPlayer>().MaxBoidCount) return;
			
			var isTop = random.Next(0, 2);
			Main.NewText(isTop);
			var x = 0;
			var y = 0;
			var xSpeed = (float) random.NextDouble()*2-1;
			var ySpeed = (float) random.NextDouble()*2-1;
				
			Main.NewText("xspeed" + xSpeed);
			Main.NewText("yspeed" + ySpeed);
				
			if (isTop == 1)
			{
				x = random.Next(0, Main.screenWidth) + (int) player.position.X;
				y = Main.screenHeight/2 + (int) player.position.Y-1;
			}
			else
			{
				x = Main.screenWidth/2 + (int) player.position.X-1;
				y = random.Next(0, Main.screenHeight) + (int) player.position.Y;
			}
			player.AddBuff(Item.buffType, 2);
				
			Projectile.NewProjectile(player.GetProjectileSource_Item(Item), x, y, xSpeed, ySpeed,
				ModContent.ProjectileType<BoidMinion>(), 0, 0, Main.myPlayer, 0f, 0f);
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe().AddIngredient(ItemID.Wood, 20)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
}
