using Boids.ModdedPlayer;
using Microsoft.Xna.Framework;
using On.Terraria.Localization;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;

namespace Boids.Items.Accessories
{
    public class BoidIncreaseAccessoryLVL1 : ModItem
    {
        public override void SetStaticDefaults() {
            Tooltip.SetDefault("+1 max boids");
            DisplayName.SetDefault("Boid Increase Accessory LVL1");

            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
        
        public override void SetDefaults() {
            Item.width = 24;
            Item.height = 28;
            Item.value = Item.buyPrice(10);
            Item.rare = ItemRarityID.Green;
            Item.accessory = true;
        }

        // public override void EquipFrameEffects(Player player, EquipType type)
        // {
        //     player.GetModPlayer<CustomPlayer>().MaxBoidCount++;
        // }
        
        // public override void Load()
        // {
        //     Main.LocalPlayer.GetModPlayer<CustomPlayer>().MaxBoidCount++;
        // }
        //
        // public override void Unload()
        // {
        //     Main.LocalPlayer.GetModPlayer<CustomPlayer>().MaxBoidCount--;
        // }

        // public override void UpdateAccessory(Player player, bool hideVisual)
        // {
        //     Main.LocalPlayer.GetModPlayer<CustomPlayer>().BoidCount++;
        // }
        
        public override void AddRecipes() {
            CreateRecipe()
                .AddIngredient(ItemID.DirtBlock)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}