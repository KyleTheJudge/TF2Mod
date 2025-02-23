using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace TF2.Content.Buffs
{
    public class Bleeding : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.IsATagBuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex) => player.GetModPlayer<BleedingPlayer>().lifeRegenDebuff = true;

        public override void Update(NPC npc, ref int buffIndex) => npc.GetGlobalNPC<BleedingNPC>().lifeRegenDebuff = true;
    }

    public class BleedingPlayer : ModPlayer
    {
        public bool lifeRegenDebuff;
        public float damageMultiplier = 1f;
        public int timer;

        public override void ResetEffects() => lifeRegenDebuff = false;

        public override void UpdateBadLifeRegen()
        {
            if (lifeRegenDebuff)
            {
                timer++;
                if (Player.lifeRegen > 0)
                    Player.lifeRegen = 0;
                Player.lifeRegenTime = 0;
                if (timer >= 30)
                {
                    Player.statLife -= (int)(4 * damageMultiplier);
                    CombatText.NewText(new Rectangle((int)Player.position.X, (int)Player.position.Y, Player.width, Player.height), CombatText.LifeRegen, (int)(4 * damageMultiplier), dramatic: false, dot: true);
                    if (Player.statLife <= 0)
                        Player.KillMe(PlayerDeathReason.ByCustomReason(Player.name + " bled to death."), (int)(4 * damageMultiplier), 0);
                    timer = 0;
                }
            }
            else
                timer = 0;
        }
    }

    public class BleedingNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public bool lifeRegenDebuff;
        public float damageMultiplier = 1f;
        public int timer;

        public override void ResetEffects(NPC npc) => lifeRegenDebuff = false;

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (lifeRegenDebuff)
            {
                timer++;
                if (npc.lifeRegen > 0)
                    npc.lifeRegen = 0;
                if (timer >= 30)
                {
                    npc.life -= (int)(4 * damageMultiplier);
                    CombatText.NewText(new Rectangle((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height), CombatText.LifeRegenNegative, (int)(4 * damageMultiplier), dramatic: false, dot: true);
                    npc.checkDead();
                    if (Main.netMode != NetmodeID.SinglePlayer)
                        npc.netUpdate = true;
                    timer = 0;
                }
                int dustIndex = Dust.NewDust(new Vector2(npc.position.X, npc.position.Y), npc.width, npc.height, DustID.Blood, 0f, 0f, 100, default, 3f);
                Main.dust[dustIndex].noGravity = true;
                Main.dust[dustIndex].velocity *= 5f;
            }
            else
                timer = 0;
        }

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter) => binaryWriter.Write(timer);

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader) => timer = binaryReader.ReadInt32();
    }
}