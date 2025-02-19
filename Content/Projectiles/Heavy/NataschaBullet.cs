﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TF2.Content.Buffs;

namespace TF2.Content.Projectiles.Heavy
{
    public class NataschaBullet : Bullet
    {
        public override string Texture => "TF2/Content/Projectiles/Bullet";

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<NataschaDebuff>(), 1);

        public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(BuffID.Slow, 1);
    }
}