﻿using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using TF2.Content.Projectiles.Heavy;

namespace TF2.Content.Items.Heavy
{
    public class KillingGlovesOfBoxing : TF2Weapon
    {
        protected override void WeaponStatistics()
        {
            SetWeaponCategory(Heavy, Melee, Unique, Unlock);
            SetLungeUseStyle();
            SetWeaponDamage(damage: 65, projectile: ModContent.ProjectileType<KillingGlovesOfBoxingProjectile>(), projectileSpeed: 2f);
            SetWeaponAttackSpeed(0.96);
            SetWeaponAttackSound("TF2/Content/Sounds/SFX/melee_swing");
            SetWeaponAttackIntervals(altClick: true);
        }

        protected override void WeaponDescription(List<TooltipLine> description)
        {
            AddPositiveAttribute(description);
            AddNegativeAttribute(description);
        }

        protected override void WeaponAttackAnimation(Player player) => Item.noUseGraphic = true;
    }
}