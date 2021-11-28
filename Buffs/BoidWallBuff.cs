using Microsoft.Xna.Framework;
using System;
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

namespace Boids.Buffs
{
    public class BoidWallBuff : ModBuff
    {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Wall");
            Description.SetDefault("GET AWAY BOID!");

            Main.buffNoSave[Type] = true; // This buff won't save when you exit the world
            Main.buffNoTimeDisplay[Type] = true; // The time remaining won't display on this buff
        }

        public override void Update(Player player, ref int buffIndex) {
            // If the minions exist reset the buff time, otherwise remove the buff from the player
            if (player.ownedProjectileCounts[ModContent.ProjectileType<BoidWall>()] > 0) {
                player.buffTime[buffIndex] = 18000;
            }
            else {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
		
    }
}