﻿using System.Collections.Generic;
using Terraria.ModLoader;
using TF2.Content.Projectiles.Spy;

namespace TF2.Content.Items.Spy
{
    public class Sapper : TF2Weapon
    {
        protected override void WeaponStatistics()
        {
            SetWeaponCategory(Spy, PDA2, Stock, Starter);
            SetWeaponSize();
            SetThrowableUseStyle(focus: true);
            SetWeaponDamage(damage: 25, projectile: ModContent.ProjectileType<SapperProjectile>(), projectileSpeed: 25f);
            SetWeaponAttackSpeed(1, hide: true);
            SetWeaponAttackSound("TF2/Content/Sounds/SFX/melee_swing");
            SetUtilityWeapon();
        }

        protected override void WeaponDescription(List<TooltipLine> description) => AddNeutralAttribute(description);
    }
}