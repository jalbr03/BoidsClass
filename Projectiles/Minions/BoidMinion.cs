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
	public class BoidMinion : ModProjectile
	{
		private Random random = new Random();
		private const float MaxDist = 15f;
		private const float WallMaxDist = 65f;
		private const float SeeingRange = 100f;
		
		private const float BoidAvoid = 30f;
		private const float WallAvoid = 70f;
		private const float Alignment = 1f;
		private const float Cohesion = 0.01f;
		private const float Speed = 3f;

		private float tpBuffer = 24f;
		
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Boids");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 1;
			// This is necessary for right-click targeting
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;

			Main.projPet[Projectile.type] = true; // Denotes that this projectile is a pet or minion

			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true; // This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
			// ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true; // Make the cultist resistant to this projectile, as it's resistant to all homing projectiles.
		}

		public sealed override void SetDefaults() {
			Projectile.width = 16;
			Projectile.height = 16;
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
			vectorToIdlePosition = Vector2.Zero; //Projectile.Center;
			distanceToIdlePosition = 0; //vectorToIdlePosition.Length();

			var spawnchance = random.Next(0, 100);

			if (spawnchance > 50) return;

			var avrageVelocityX = 0f;
			var avrageVelocityY = 0f;

			var avragePositionX = 0f;
			var avragePositionY = 0f;

			var avragePushX = 0f;
			var avragePushY = 0f;
			var neighbors = 0;
			var walls = 0;

			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile other = Main.projectile[i];

				if (i != Projectile.whoAmI && other.active && other.owner == Projectile.owner)
				{
					var dist = Projectile.position.Distance(other.position);

					var isWall = other.type == ModContent.ProjectileType<BoidWall>();

					if (dist < MaxDist || isWall && dist < WallMaxDist)
					{
						var distToOther = ExtraMath.PointDistance(Projectile.position, other.position);
						var difx = Projectile.position.X - other.position.X;
						var dify = Projectile.position.Y - other.position.Y;
						avragePushX += (float) (difx / distToOther) * (isWall ? WallAvoid : BoidAvoid);
						avragePushY += (float) (dify / distToOther) * (isWall ? WallAvoid : BoidAvoid);
					}

					if (isWall)
					{
						walls++;
						continue;
					}

					if (dist <= SeeingRange)
					{
						neighbors++;
						avrageVelocityX += other.velocity.X;
						avrageVelocityY += other.velocity.Y;

						avragePositionX += other.position.X;
						avragePositionY += other.position.Y;
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
				
				targetVelX += (avrageVelocityX - Projectile.velocity.X) * Alignment;
				targetVelY += (avrageVelocityY - Projectile.velocity.Y) * Alignment;

				var midX = avragePositionX - Projectile.position.X;
				var midY = avragePositionY - Projectile.position.Y;
				targetVelX += (midX - Projectile.velocity.X) * Cohesion;
				targetVelY += (midY - Projectile.velocity.Y) * Cohesion;
			}

			if (neighbors != 0 || walls != 0)
			{
				avragePushX /= neighbors + walls;
				avragePushY /= neighbors + walls;
				
				targetVelX += avragePushX;
				targetVelY += avragePushY;
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
				Projectile.velocity.X += (targetVelX*Speed - Projectile.velocity.X) / 20f;
				Projectile.velocity.Y += (targetVelY*Speed - Projectile.velocity.Y) / 20f;
			}
		}

		private void SearchForTargets(Player owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter)
		{
			// Starting search distance
			distanceFromTarget = 700f;
			targetCenter = Projectile.position;
			foundTarget = true;
			Projectile.friendly = foundTarget;
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
			int frameSpeed = 0;
			Projectile.frame = 0;

			//Projectile.frameCounter++;

			if (Projectile.frameCounter >= frameSpeed) {
				Projectile.frameCounter = 0;
				// Projectile.frame++;

				if (Projectile.frame >= Main.projFrames[Projectile.type]) {
					Projectile.frame = 0;
				}
			}

			// Some visuals here
			Lighting.AddLight(Projectile.Center, Color.White.ToVector3() * 0.78f);
		}
	}
}
