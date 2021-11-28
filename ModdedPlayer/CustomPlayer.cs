using System;
using Boids.Items.Accessories;
using Boids.Projectiles.Minions;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Item = IL.Terraria.Item;

namespace Boids.ModdedPlayer
{
    public class CustomPlayer : ModPlayer
    {
        public int BaseMaxBoidCount = 100;
        public int MaxBoidCount = 3;
        public int NextToGo = 0;
        private readonly string[] _accessoryIncrease = {new BoidIncreaseAccessoryLVL1().Name};

        public override void PreUpdate()
        {
            FixOverflow();
            UpdateAccessories();
        }

        private void UpdateAccessories()
        {
            int additionalSlots = 0;
            for(int i=3; i<=7+Main.LocalPlayer.extraAccessorySlots; i++)
            {
                // Main.NewText(Main.LocalPlayer.armor[i].Name.Replace(" ", ""));
                // Main.NewText(_accessoryIncrease[0]);
                
                string currentAccessory = Main.LocalPlayer.armor[i].Name.Replace(" ", "");

                for (var j = 0; j < _accessoryIncrease.Length; j++)
                {
                    string checkingAccessory = _accessoryIncrease[j];
                    if (currentAccessory == checkingAccessory)
                    {
                        additionalSlots += 1;
                        break;
                    }
                }
            }

            MaxBoidCount = BaseMaxBoidCount + additionalSlots;
        }
        private void FixOverflow()
        {
            if (Main.LocalPlayer.ownedProjectileCounts[ModContent.ProjectileType<BoidMinion>()] <= MaxBoidCount)
            {
                if (Main.projectile[NextToGo].active) return;
                for (var i = Main.projectile.Length - 1; i > 0; i--)
                {
                    if (!Main.projectile[i].active) continue;
                    NextToGo = i;
                    break;
                }
            }
            else
            {
                Main.projectile[NextToGo].Kill();
            }
            
            // if (Main.LocalPlayer.ownedProjectileCounts[ModContent.ProjectileType<BoidMinion>()] > MaxBoidCount)
            // {
            //     foreach (var projectile in Main.projectile)
            //     {
            //         if (!projectile.active) continue;
            //         projectile.Kill();
            //         break;
            //     }
            // }
        }
        
    }
}