using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using TF2.Common;
using TF2.Content.Items.Materials;
using TF2.Content.Projectiles;
using TF2.Content.Tiles.Crafting;

namespace TF2.Content.Items.Spy
{
    public class LEtranger : TF2Weapon
    {
        protected override void WeaponStatistics()
        {
            SetWeaponCategory(Spy, Secondary, Unique, Craft);
            SetWeaponSize(50, 25);
            SetGunUseStyle(focus: true, automatic: true);
            SetWeaponDamage(damage: 32, projectile: ModContent.ProjectileType<Bullet>());
            SetWeaponAttackSpeed(0.5);
            SetWeaponAttackSound("TF2/Content/Sounds/SFX/letranger_shoot");
            SetWeaponAttackIntervals(maxAmmo: 6, reloadTime: 1.133, usesMagazine: true, reloadSoundPath: "TF2/Content/Sounds/SFX/revolver_reload");
            SetWeaponPrice(weapon: 1, reclaimed: 1);
        }

        protected override void WeaponDescription(List<TooltipLine> description)
        {
            AddPositiveAttribute(description);
            AddNegativeAttribute(description);
        }

        protected override void WeaponPassiveUpdate(Player player) => player.GetModPlayer<LEtrangerPlayer>().lEtrangerEquipped = true;

        protected override void WeaponPostFireProjectile(Player player, int projectile) => Main.projectile[projectile].GetGlobalProjectile<TF2ProjectileBase>().lEtrangerProjectile = true;

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<DeadRinger>()
                .AddIngredient<ReclaimedMetal>()
                .AddTile<CraftingAnvil>()
                .Register();
        }
    }

    public class LEtrangerPlayer : ModPlayer
    {
        public bool lEtrangerEquipped;

        public override void ResetEffects() => lEtrangerEquipped = false;
    }
}