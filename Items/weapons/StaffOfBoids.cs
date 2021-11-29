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
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Boids.Items.weapons
{
    public class StaffOfBoids : ModItem
	{
		
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Staff of Boids");
			Tooltip.SetDefault("Take control and make those mindless Boids Fight for YOU!"); // TODO: chang this!
	
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the player target anywhere on the whole screen while using a controller
			ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
		}

		public override void SetDefaults()
		{
			Item.damage = 30;
			Item.knockBack = 3f;
			Item.mana = 0; // mana cost
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 5;
			Item.useAnimation = 5;
			Item.useStyle = ItemUseStyleID.Swing; // how the player's arm moves when using the item
			Item.value = Item.sellPrice(gold: 30);
			Item.rare = ItemRarityID.White;
			Item.UseSound = SoundID.Item44; // What sound should play when using the item
			Item.autoReuse = true;

			Item.noMelee = true; // this item doesn't do any melee damage
			Item.DD2Summon = false;
			Item.DamageType = ModContent.GetInstance<BoidDamageClass>(); 
			Item.buffType = ModContent.BuffType<BoidWallBuff>();

			Item.shoot = ModContent.ProjectileType<BoidWall>(); // This item creates the minion projectile
			
		}

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			// Here you can change where the minion is spawned. Most vanilla minions spawn at the cursor position
			position = Main.MouseWorld;
		}
	
		public override bool Shoot(Player player, ProjectileSource_Item_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			// This is needed so the buff that keeps your minion alive and allows you to despawn it properly applies
			player.AddBuff(Item.buffType, 2);
	
			// Minions have to be spawned manually, then have originalDamage assigned to the damage of the summon item
			var projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, Main.myPlayer);
			projectile.originalDamage = Item.damage;
	
			// Since we spawned the projectile manually already, we do not need the game to spawn it for ourselves anymore, so return false
			return false;
		}

		public override bool AltFunctionUse(Player player)
		{
			player.statMana = 0;
			player.GetModPlayer<CustomPlayer>().PullBoids = true;
			return true;
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.Wood)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
}