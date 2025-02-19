﻿using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TF2.Content.Buffs;

namespace TF2.Content.Items.Scout
{
    public class BonkAtomicPunch : TF2Weapon
    {
        protected override void WeaponStatistics()
        {
            SetWeaponCategory(Scout, Secondary, Unique, Unlock);
            SetWeaponSize(19, 40);
            SetDrinkUseStyle();
            SetWeaponAttackSpeed(1.2, hide: true);
            SetWeaponAttackSound(SoundID.Item3);
            SetUtilityWeapon();
        }

        protected override void WeaponDescription(List<TooltipLine> description) => AddNeutralAttribute(description);

        public override bool WeaponCanBeUsed(Player player) => !player.HasBuff<DrinkCooldown>() && !player.GetModPlayer<RadioactivePlayer>().radiationBuff;

        protected override bool? WeaponOnUse(Player player)
        {
            player.AddBuff(ModContent.BuffType<Radiation>(), 480);
            player.AddBuff(ModContent.BuffType<DrinkCooldown>(), 1320);
            return true;
        }
    }
}