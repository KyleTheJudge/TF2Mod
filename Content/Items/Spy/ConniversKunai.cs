﻿using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using TF2.Content.Items.Materials;
using TF2.Content.Projectiles.Spy;
using TF2.Content.Tiles.Crafting;

namespace TF2.Content.Items.Spy
{
    public class ConniversKunai : TF2Weapon
    {
        protected override void WeaponStatistics()
        {
            SetWeaponCategory(Spy, Melee, Unique, Craft);
            SetLungeUseStyle(knife: true);
            SetWeaponDamage(damage: 40, projectile: ModContent.ProjectileType<ConniversKunaiProjectile>(), projectileSpeed: 2f, noRandomCriticalHits: true);
            SetWeaponAttackSpeed(0.8);
            SetWeaponAttackSound("TF2/Content/Sounds/SFX/knife_swing");
            SetWeaponAttackIntervals(altClick: true);
            SetWeaponPrice(weapon: 1, reclaimed: 1, scrap: 1);
        }

        protected override void WeaponDescription(List<TooltipLine> description)
        {
            AddPositiveAttribute(description);
            AddNegativeAttribute(description);
            AddNeutralAttribute(description);
        }

        protected override void WeaponPassiveUpdate(Player player) => SetPlayerHealth(player, 56);

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ScrapMetal>()
                .AddIngredient<CloakAndDagger>()
                .AddTile<CraftingAnvil>()
                .Register();
        }
    }
}