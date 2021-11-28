using Microsoft.Xna.Framework;
using System;
using Boids.Buffs;
using Boids.DamageClasses;
using Boids.General;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Boids.Projectiles.Minions
{
	// This file contains all the code necessary for a minion
	// - ModItem - the weapon which you use to summon the minion with
	// - ModBuff - the icon you can click on to despawn the minion
	// - ModProjectile - the minion itself

	// It is not recommended to put all these classes in the same file. For demonstrations sake they are all compacted together so you get a better overwiew.
	// To get a better understanding of how everything works together, and how to code minion AI, read the guide: https://github.com/tModLoader/tModLoader/wiki/Basic-Minion-Guide
	// This is NOT an in-depth guide to advanced minion AI

	// public class BoidMinionBuff : ModBuff
	// {
	// 	public override void SetStaticDefaults() {
	// 		DisplayName.SetDefault("Boid");
	// 		Description.SetDefault("Fly with the Boids!");
	//
	// 		Main.buffNoSave[Type] = true; // This buff won't save when you exit the world
	// 		Main.buffNoTimeDisplay[Type] = true; // The time remaining won't display on this buff
	// 	}
	//
	// 	public override void Update(Player player, ref int buffIndex) {
	// 		// If the minions exist reset the buff time, otherwise remove the buff from the player
	// 		if (player.ownedProjectileCounts[ModContent.ProjectileType<BoidMinion>()] > 0) {
	// 			player.buffTime[buffIndex] = 18000;
	// 		}
	// 		else {
	// 			player.DelBuff(buffIndex);
	// 			buffIndex--;
	// 		}
	// 	}
	// 	
	// }

	// public class StaffOfBoids : ModItem
	// {
	// 	
	// 	public override void SetStaticDefaults() {
	// 		DisplayName.SetDefault("Boid test");
	// 		Tooltip.SetDefault("Summons a Boid to fly with you!"); // TODO: chang this!
	//
	// 		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	// 		ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the player target anywhere on the whole screen while using a controller
	// 		ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
	// 	}
	//
	// 	public override void SetDefaults()
	// 	{
	// 		Item.damage = 30;
	// 		Item.knockBack = 3f;
	// 		Item.mana = 0; // mana cost
	// 		Item.width = 32;
	// 		Item.height = 32;
	// 		Item.useTime = 1;
	// 		Item.useAnimation = 1;
	// 		Item.useStyle = ItemUseStyleID.Swing; // how the player's arm moves when using the item
	// 		Item.value = Item.sellPrice(gold: 30);
	// 		Item.rare = ItemRarityID.White;
	// 		Item.UseSound = SoundID.Item44; // What sound should play when using the item
	//
	// 		// These below are needed for a minion weapon
	// 		Item.noMelee = true; // this item doesn't do any melee damage
	// 		Item.DD2Summon = false;
	// 		Item.DamageType = ModContent.GetInstance<BoidDamageClass>(); // Makes the damage register as summon. If your item does not have any damage type, it becomes true damage (which means that damage scalars will not affect it). Be sure to have a damage type
	// 		Item.buffType = ModContent.BuffType<BoidMinionBuff>();
	// 		// No buffTime because otherwise the item tooltip would say something like "1 minute duration"
	// 		
	// 		Item.shoot = ModContent.ProjectileType<BoidMinion>(); // This item creates the minion projectile
	// 	}
	//
	// 	// public override bool? UseItem(Player player)
	// 	// {
	// 	// 	return true;
	// 	// }
	//
	// 	public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
	// 		// Here you can change where the minion is spawned. Most vanilla minions spawn at the cursor position
	// 		position = Main.MouseWorld;
	// 	}
	//
	// 	public override bool Shoot(Player player, ProjectileSource_Item_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
	// 		// This is needed so the buff that keeps your minion alive and allows you to despawn it properly applies
	// 		player.AddBuff(Item.buffType, 2);
	//
	// 		// Minions have to be spawned manually, then have originalDamage assigned to the damage of the summon item
	// 		var projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, Main.myPlayer);
	// 		projectile.originalDamage = Item.damage;
	//
	// 		// Since we spawned the projectile manually already, we do not need the game to spawn it for ourselves anymore, so return false
	// 		return false;
	// 	}
	//
	// 	// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
	// 	public override void AddRecipes() {
	// 		CreateRecipe()
	// 			.AddIngredient(ItemID.DirtBlock)
	// 			.AddTile(TileID.WorkBenches)
	// 			.Register();
	// 	}
	// }

	// This minion shows a few mandatory things that make it behave properly. 
	// Its attack pattern is simple: If an enemy is in range of 43 tiles, it will fly to it and deal contact damage
	// If the player targets a certain NPC with right-click, it will fly through tiles to it
	// If it isn't attacking, it will float near the player with minimal movement
	public class BoidMinion : ModProjectile
	{
		private const float MaxDist = 16f;
		private const float MaxForce = 50f;
		private const float SeeingRange = 144f;
		private const float SeeingAngle = 1.2f;
		private const float Speed = 3f;

		private float tpBuffer = 24f;
		// private float tpXmax = 1024/2;
		// private float tpYmax = 1024/2;
		// private float tpBuffer = 26;
		
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Example Minion");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 4;
			// This is necessary for right-click targeting
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;

			Main.projPet[Projectile.type] = true; // Denotes that this projectile is a pet or minion

			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true; // This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
			// ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true; // Make the cultist resistant to this projectile, as it's resistant to all homing projectiles.
		}

		public sealed override void SetDefaults() {
			Projectile.width = 18;
			Projectile.height = 28;
			Projectile.tileCollide = false; // Makes the minion go through tiles freely

			// These below are needed for a minion weapon
			Projectile.friendly = true; // Only controls if it deals damage to enemies on contact (more on that later)
			//Projectile.minion = true; // Declares this as a minion (has many effects)
			Projectile.DamageType = ModContent.GetInstance<BoidDamageClass>(); // Declares the damage type (needed for it to deal damage)
			// Projectile.minionSlots = 0f; // Amount of slots this minion occupies from the total minion slots available to the player (more on that later)
			Projectile.penetrate = -1; // Needed so the minion doesn't despawn on collision with enemies or tiles
		}

		// Here you can decide if your minion breaks things like grass or pots
		public override bool? CanCutTiles() {
			return false;
		}

		// This is mandatory if your minion deals contact damage (further related stuff in AI() in the Movement region)
		public override bool MinionContactDamage() {
			return true;
		}
		
		// The AI of this minion is split into multiple methods to avoid bloat. This method just passes values between calls actual parts of the AI.
		public override void AI() {
			Player owner = Main.player[Projectile.owner];
			
			if (!CheckActive(owner)) {
				return;
			}

			GeneralBehavior(owner, out Vector2 vectorToIdlePosition, out float distanceToIdlePosition);
			SearchForTargets(owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter);
			Movement(foundTarget, distanceFromTarget, targetCenter, distanceToIdlePosition, vectorToIdlePosition);
			Visuals();
		}

		// This is the "active check", makes sure the minion is alive while the player is alive, and despawns if not
		private bool CheckActive(Player owner) {
			if (owner.dead || !owner.active) {
				owner.ClearBuff(ModContent.BuffType<BoidMinionBuff>());

				return false;
			}

			if (owner.HasBuff(ModContent.BuffType<BoidMinionBuff>())) {
				Projectile.timeLeft = 2;
			}

			return true;
		}

		private void GeneralBehavior(Player owner, out Vector2 vectorToIdlePosition, out float distanceToIdlePosition)
		{
			vectorToIdlePosition = Vector2.Zero;//Projectile.Center;
			distanceToIdlePosition = 0;//vectorToIdlePosition.Length();

			
			
			// if (Math.Abs(Projectile.position.X - owner.position.X) > tpXmax+tpBuffer*2)
			// {
			// 	Projectile.position.X = owner.position.X-tpXmax-tpBuffer;
			// } 
			// else if (Math.Abs(Projectile.position.X - owner.position.X) < tpBuffer*2)
			// {
			// 	Projectile.position.X = owner.position.X+tpXmax+tpBuffer;
			// }
			//
			// if (Math.Abs(Projectile.position.Y - owner.position.Y) > tpYmax+tpBuffer*2)
			// {
			// 	Projectile.position.Y = owner.position.Y-tpYmax-tpBuffer;
			// } 
			// else if (Math.Abs(Projectile.position.Y - owner.position.Y) < tpBuffer*2)
			// {
			// 	Projectile.position.Y = owner.position.Y+tpYmax+tpBuffer;
			// }
			
			// Fix overlap with other minions
			
			var avrageVelocityX = 0f;
			var avrageVelocityY = 0f;
			
			var avragePositionX = 0f;
			var avragePositionY = 0f;

			var avragePushX = 0f;
			var avragePushY = 0f;
			var neighbors = 0;
			// var dir = (float) Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X);
			//dir = dir - Math.Sign(dir) * (float) Math.PI;
			
			// var dirMouse = (float) Math.Atan2(Main.MouseWorld.Y-Projectile.position.Y, Main.MouseWorld.X-Projectile.position.X);
			// Main.NewText("tomouse "+ExtraMath.AngleDifference(dir, dirMouse));
			// Main.NewText("dirMouse "+dirMouse);
			
			for (int i = 0; i < Main.maxProjectiles; i++) {
				Projectile other = Main.projectile[i];
				
				if (i != Projectile.whoAmI && other.active && other.owner == Projectile.owner)
				{
					var dist = Projectile.position.Distance(other.position);
					
					// var xdist = Projectile.position.X - other.position.X;
					// var ydist = Projectile.position.Y - other.position.Y;
					//
					// var xsign = Math.Sign(xdist);
					// var ysign = Math.Sign(ydist);
					//
					// xdist = Math.Abs(xdist);
					// ydist = Math.Abs(ydist);
					// var dir2 = (float) Math.Atan2(other.position.Y-Projectile.position.Y, other.position.X-Projectile.position.X);
					
					// double angleDif = ExtraMath.AngleDifference(dir, dir2);
					if (dist <= SeeingRange)
					{
						neighbors++;
						avrageVelocityX += other.velocity.X;
						avrageVelocityY += other.velocity.Y;

						avragePositionX += other.position.X;
						avragePositionY += other.position.Y;
					}

					if (dist < MaxDist)
					{
						double distToOther = ExtraMath.PointDistance(Projectile.position, other.position);
						double difx = Projectile.position.X - other.position.X+Math.Sign(other.position.X)*0.01;
						double dify = Projectile.position.Y - other.position.Y+Math.Sign(other.position.Y)*0.01;
						avragePushX += (float) (difx / distToOther);
						avragePushY += (float) (dify / distToOther);
						// avragePushX += (MaxDist / (xdist + MaxDist / (MaxForce + 1f)) - 1f)*xsign;
						// avragePushY += (MaxDist / (ydist + MaxDist / (MaxForce + 1f)) - 1f)*ysign;
					}
				}
			}

			var targetVelX = Projectile.velocity.X;
			var targetVelY = Projectile.velocity.Y;
			
			if (neighbors != 0)
			{
				avrageVelocityX /= neighbors;
				avrageVelocityY /= neighbors;
				
				avragePositionX /= neighbors;
				avragePositionY /= neighbors;
				
				avragePushX /= neighbors;
				avragePushY /= neighbors;

				// Main.NewText("avragePushX "+avragePushX);
				// Main.NewText("avragePushY "+avragePushY);
				// Main.NewText(Projectile.velocity.X);
				// Main.NewText(avrageVelocityX);
				// Main.NewText((Projectile.velocity.X - avrageVelocityX) / 100);
				
				targetVelX += (avrageVelocityX - Projectile.velocity.X) / 1f;
				targetVelY += (avrageVelocityY - Projectile.velocity.Y) / 1f;
				
				var midX = avragePositionX - Projectile.position.X;
				var midY = avragePositionY - Projectile.position.Y;
				targetVelX += (midX - Projectile.velocity.X) * 0.01f;
				targetVelY += (midY - Projectile.velocity.Y) * 0.01f;

				targetVelX += avragePushX*30;
				targetVelY += avragePushY*30;
			}

			// TODO: keep this code
			// if (owner.position.Distance(Projectile.position) > tpMax)
			// {
			// 	targetVelX += (owner.Center.X - Projectile.position.X)/100f;
			// 	targetVelY += (owner.Center.Y - Projectile.position.Y)/100f;
			// }

			var maxX = Main.screenWidth / 2;
			var maxY = Main.screenHeight / 2;
			var left = owner.position.X + maxX;
			var right = owner.position.X - maxX;
			var top = owner.position.Y + maxY;
			var bottom = owner.position.Y - maxY;

			if (Projectile.position.X > left + tpBuffer)
			{
				Projectile.position.X = right;
			}
			else if(Projectile.position.X < right - tpBuffer)
			{
				Projectile.position.X = left;
			}
			
			if (Projectile.position.Y > top + tpBuffer)
			{
				Projectile.position.Y = bottom;
			}
			else if(Projectile.position.Y < bottom - tpBuffer)
			{
				Projectile.position.Y = top;
			}
			
			var num = 1f / (float) Math.Sqrt(targetVelX * targetVelX + targetVelY * targetVelY);
			targetVelX *= num;
			targetVelY *= num;

			if(!float.IsPositiveInfinity(Math.Abs(num)))
			{
				// Projectile.velocity.X = targetVelX*2;
				// Projectile.velocity.Y = targetVelY*2;
				Projectile.velocity.X += (targetVelX*Speed - Projectile.velocity.X) / 20f;
				Projectile.velocity.Y += (targetVelY*Speed - Projectile.velocity.Y) / 20f;
			}
		}

		private void SearchForTargets(Player owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter)
		{
			// Starting search distance
			distanceFromTarget = 700f;
			targetCenter = Projectile.position;
			foundTarget = false;
			return;
			// This code is required if your minion weapon has the targeting feature
			if (owner.HasMinionAttackTargetNPC) {
				NPC npc = Main.npc[owner.MinionAttackTargetNPC];
				float between = Vector2.Distance(npc.Center, Projectile.Center);

				// Reasonable distance away so it doesn't target across multiple screens
				if (between < 2000f) {
					distanceFromTarget = between;
					targetCenter = npc.Center;
					foundTarget = true;
				}
			}

			if (!foundTarget) {
				// This code is required either way, used for finding a target
				for (int i = 0; i < Main.maxNPCs; i++) {
					NPC npc = Main.npc[i];

					if (npc.CanBeChasedBy()) {
						float between = Vector2.Distance(npc.Center, Projectile.Center);
						bool closest = Vector2.Distance(Projectile.Center, targetCenter) > between;
						bool inRange = between < distanceFromTarget;
						bool lineOfSight = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height);
						// Additional check for this specific minion behavior, otherwise it will stop attacking once it dashed through an enemy while flying though tiles afterwards
						// The number depends on various parameters seen in the movement code below. Test different ones out until it works alright
						bool closeThroughWall = between < 100f;

						if (((closest && inRange) || !foundTarget) && (lineOfSight || closeThroughWall)) {
							distanceFromTarget = between;
							targetCenter = npc.Center;
							foundTarget = true;
						}
					}
				}
			}

			// friendly needs to be set to true so the minion can deal contact damage
			// friendly needs to be set to false so it doesn't damage things like target dummies while idling
			// Both things depend on if it has a target or not, so it's just one assignment here
			// You don't need this assignment if your minion is shooting things instead of dealing contact damage
			Projectile.friendly = foundTarget;
		}

		private void Movement(bool foundTarget, float distanceFromTarget, Vector2 targetCenter, float distanceToIdlePosition, Vector2 vectorToIdlePosition)
		{
			return;
			// Default movement parameters (here for attacking)
			float speed = 8f;
			float inertia = 20f;

			if (foundTarget) {
				// Minion has a target: attack (here, fly towards the enemy)
				if (distanceFromTarget > 40f) {
					// The immediate range around the target (so it doesn't latch onto it when close)
					Vector2 direction = targetCenter - Projectile.Center;
					direction.Normalize();
					direction *= speed;

					Projectile.velocity = (Projectile.velocity * (inertia - 1) + direction) / inertia;
				}
			}
			else {
				// Minion doesn't have a target: return to player and idle
				if (distanceToIdlePosition > 600f) {
					// Speed up the minion if it's away from the player
					speed = 12f;
					inertia = 60f;
				}
				else {
					// Slow down the minion if closer to the player
					speed = 4f;
					inertia = 80f;
				}

				if (distanceToIdlePosition > 20f) {
					// The immediate range around the player (when it passively floats about)

					// This is a simple movement formula using the two parameters and its desired direction to create a "homing" movement
					vectorToIdlePosition.Normalize();
					vectorToIdlePosition *= speed;
					Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
				}
				else if (Projectile.velocity == Vector2.Zero) {
					// If there is a case where it's not moving at all, give it a little "poke"
					Projectile.velocity.X = -0.15f;
					Projectile.velocity.Y = -0.05f;
				}
			}
		}

		private void Visuals() {
			// So it will lean slightly towards the direction it's moving
			var dir = (float) Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X);
			// Main.NewText("dir "+dir);
			Projectile.rotation = dir;//Projectile.velocity.X * 0.05f;

			// This is a simple "loop through all frames from top to bottom" animation
			int frameSpeed = 5;

			Projectile.frameCounter++;

			if (Projectile.frameCounter >= frameSpeed) {
				Projectile.frameCounter = 0;
				Projectile.frame++;

				if (Projectile.frame >= Main.projFrames[Projectile.type]) {
					Projectile.frame = 0;
				}
			}

			// Some visuals here
			Lighting.AddLight(Projectile.Center, Color.White.ToVector3() * 0.78f);
		}
	}
}
