﻿using Terraria;
using Terraria.ModLoader;
using TF2.Common;

namespace TF2.Content.Buffs
{
    public class HeavyCrit : ModBuff
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Heavy's Crit");
            Description.SetDefault("Grants crits");
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex) => player.GetModPlayer<TF2Player>().crit = true;
    }
}