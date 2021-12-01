using Microsoft.Xna.Framework;
using System;
using Boids.Buffs;
using Boids.DamageClasses;
using Boids.General;
using Boids.ModdedPlayer;
using Boids.Projectiles.Minions;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Boids.Items.weapons
{
    public class StaffOfBoids : ModItem
	{
		private float _usingTime = 0f;
		private const float MaxUsingTime = 0.1f;

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Staff of Boids");
			Tooltip.SetDefault("Take control and make those mindless Boids Fight for YOU!");
	
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the player target anywhere on the whole screen while using a controller
			ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
		}

		public override void SetDefaults()
		{
			// Item.damage = 0;
			// Item.knockBack = 3f;
			Item.mana = 0; // mana cost
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 15;
			Item.useAnimation = 15;
			Item.useStyle = ItemUseStyleID.Swing; // how the player's arm moves when using the item
			Item.value = Item.sellPrice(silver: 10);
			Item.rare = ItemRarityID.White;
			Item.UseSound = SoundID.Item44; // What sound should play when using the item
			Item.autoReuse = true;
			

			Item.noMelee = true; // this item doesn't do any melee damage
			// Item.DD2Summon = false;
			// Item.DamageType = ModContent.GetInstance<BoidDamageClass>(); 
			// Item.buffType = ModContent.BuffType<BoidWallBuff>();

			// Item.shoot = ModContent.ProjectileType<BoidWall>(); // This item creates the minion projectile
			
		}

		// public override bool CanUseItem(Player player) {
		// 	Main.NewText("click!");
		// 	Main.NewText("player.altFunctionUse " + player.altFunctionUse);
		// 	// if (player.altFunctionUse == 2)
		// 	// {
		// 	// 	Item.autoReuse = true;
		// 	// 	Main.NewText("right click");
		// 	// }else
		// 	// {
		// 	// 	Item.autoReuse = false;
		// 	// 	Main.NewText("left click");
		// 	// 	// item.shoot = /*projectile on left click*/;
		// 	// }
		//
		// 	return base.CanUseItem(player);
		// }

		// public override bool? UseItem(Player player)
		// {
		// 	Main.NewText("click!");
		// 	Main.NewText("player.altFunctionUse " + player.altFunctionUse);
		// 	if (player.altFunctionUse == 2)
		// 	{
		// 		// Item.autoReuse = true;
		// 		// Main.NewText("right click");
		// 	}
		// 	else
		// 	{
		// 		// Item.autoReuse = false;
		// 		// Main.NewText("left click");
		// 		// item.shoot = /*projectile on left click*/;
		// 	}
		//
		// 	return base.UseItem(player);
		// }

		public override void HoldItem(Player player)
		{
			// Main.NewText("PullBoids " + Main.LocalPlayer.GetModPlayer<CustomPlayer>().PullBoids);
			if (_usingTime > 0)
			{
				_usingTime -= 1.0f/Main.frameRate;
				return;
			}

			_usingTime = MaxUsingTime;
			if (Main.mouseLeft)
			{
				player.AddBuff(ModContent.BuffType<BoidWallBuff>(), 2);
				var position = Main.MouseWorld;
				Projectile.NewProjectile(player.GetProjectileSource_Item(Item), position.X, position.Y, 0, 0,
					ModContent.ProjectileType<BoidWall>(), 0, 0, Main.myPlayer);
				// var projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, Main.myPlayer);
			}
			
			if (Main.mouseRight && player.statMana >= Item.mana)
			{
				player.statMana -= Item.mana;
				Main.LocalPlayer.GetModPlayer<CustomPlayer>().PullBoids = true;
			}
			else
			{
				Main.LocalPlayer.GetModPlayer<CustomPlayer>().PullBoids = false;
			}
		}

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			// Here you can change where the minion is spawned. Most vanilla minions spawn at the cursor position
			position = Main.MouseWorld;
		}

		// public override void Update(ref float gravity, ref float maxFallSpeed)
		// {
		// 	
		// }

		public override bool Shoot(Player player, ProjectileSource_Item_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			// if(player.altFunctionUse == 2)
			// {
			// 	// Main.NewText("right click");
			// }else{
			// 	Main.NewText("left click");
			// }
			
			// player.AddBuff(ModContent.BuffType<BoidWallBuff>(), 2);
			// Main.NewText("left click");
			// // Projectile.NewProjectile(player.GetProjectileSource_Item(Item), position.X, position.Y, 0, 0,
			// // 	ModContent.ProjectileType<BoidWall>(), 10, 1, Main.myPlayer);
			// var projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, Main.myPlayer);
			// projectile.originalDamage = Item.damage;
			
			return false;
			
		}
		
		// public override bool AltFunctionUse(Player player)
		// {
		// 	Main.NewText("right click");
		// 	player.GetModPlayer<CustomPlayer>().PullBoids = true;
		// 	return true;
		// }

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.Wood)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
}