﻿using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using TF2.Common;
using TF2.Content.Buffs;
using TF2.Content.NPCs;
using TF2.Content.Projectiles;

namespace TF2.Content.Items.Spy
{
    public class DeadRinger : TF2Weapon
    {
        protected override void WeaponStatistics()
        {
            SetWeaponCategory(Spy, PDA, Unique, Unlock);
            SetUtilityWeapon();
        }

        protected override void WeaponDescription(List<TooltipLine> description)
        {
            AddHeader(description);
            AddPositiveAttribute(description);
            AddNegativeAttribute(description);
        }

        public override bool WeaponCanBeUsed(Player player) => false;

        protected override void WeaponPassiveUpdate(Player player) => player.GetModPlayer<FeignDeathPlayer>().deadRingerEquipped = true;
    }

    public class FeignDeathPlayer : ModPlayer
    {
        public bool deadRingerEquipped;
        public bool feignDeath;
        public float cloakMeter;
        public float cloakMeterMax = 840;
        public int timer;
        public int timer2;
        private bool playDecloakingSound;
        public bool fullCloak;

        public override void OnRespawn() => cloakMeter = cloakMeterMax;

        public override void PostNurseHeal(NPC nurse, int health, bool removeDebuffs, int price) => cloakMeter = cloakMeterMax;

        public override void ResetEffects()
        {
            deadRingerEquipped = false;
            feignDeath = false;
            Player.opacityForAnimation = 1f;
            cloakMeterMax = 840f;
        }

        public override void PostUpdate()
        {
            if (Player.GetModPlayer<LEtrangerPlayer>().lEtrangerEquipped)
                cloakMeterMax += 336f;
            if (Player.GetModPlayer<YourEternalRewardPlayer>().yourEternalRewardEquipped)
                cloakMeterMax -= 280f;
            if (!Player.GetModPlayer<TF2Player>().initializedClass)
                cloakMeter = cloakMeterMax;
            if (playDecloakingSound && deadRingerEquipped && cloakMeter <= 0)
            {
                SoundEngine.PlaySound(new SoundStyle("TF2/Content/Sounds/SFX/spy_cloak"), Player.Center);
                playDecloakingSound = false;
            }
            fullCloak = cloakMeter == cloakMeterMax;
            if (deadRingerEquipped)
                timer++;
            else
            {
                Player.ClearBuff(ModContent.BuffType<FeignDeath>());
                cloakMeter = 0;
                timer = 0;
            }          
            if (!feignDeath)
                timer2 = 0;
            if (!feignDeath && deadRingerEquipped && timer >= 3)
            {
                cloakMeter += 2.1f;
                cloakMeter = Utils.Clamp(cloakMeter, 0, cloakMeterMax);
                timer = 0;
            }
            else if (Player.HasBuff(ModContent.BuffType<FeignDeath>()))
            {
                timer2++;
                cloakMeter--;
                cloakMeter = Utils.Clamp(cloakMeter, 0, cloakMeterMax);
                int buffIndex = Player.FindBuffIndex(ModContent.BuffType<FeignDeath>());
                Player.buffTime[buffIndex] = Convert.ToInt32(cloakMeter);
                Player.opacityForAnimation = 0.5f;
                playDecloakingSound = true;
                timer = 0;
                if (timer2 <= 180)
                {
                    Player.GetModPlayer<TF2Player>().damageReduction += 0.65f;
                    TF2Weapon.SetPlayerSpeed(Player, 200);
                }
            }
        }

        #region Cloak Drain On Attack

        public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (feignDeath && Player.HasBuff<FeignDeath>())
                cloakMeter -= 150f;
        }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (proj.GetGlobalProjectile<TF2ProjectileBase>().spawnedFromNPC) return;
            if (feignDeath && Player.HasBuff<FeignDeath>())
                cloakMeter -= 150f;
            else if (Player.GetModPlayer<LEtrangerPlayer>().lEtrangerEquipped && proj.GetGlobalProjectile<TF2ProjectileBase>().lEtrangerProjectile)
                cloakMeter += 176f;
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (!modifiers.PvP) return;
            Player opponent = Main.player[modifiers.DamageSource.SourcePlayerIndex];
            if (opponent.GetModPlayer<FeignDeathPlayer>().feignDeath && opponent.HasBuff<FeignDeath>())
                opponent.GetModPlayer<FeignDeathPlayer>().cloakMeter -= 150f;
        }

        public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
        {
            if (!hurtInfo.PvP) return;
            Player opponent = Main.player[proj.owner];
            if (opponent.GetModPlayer<FeignDeathPlayer>().feignDeath && opponent.HasBuff<FeignDeath>())
                opponent.GetModPlayer<FeignDeathPlayer>().cloakMeter -= 150f;
            else if (opponent.GetModPlayer<LEtrangerPlayer>().lEtrangerEquipped && proj.GetGlobalProjectile<TF2ProjectileBase>().lEtrangerProjectile)
                opponent.GetModPlayer<FeignDeathPlayer>().cloakMeter += 176f;
        }

        #endregion Cloak Drain On Attack

        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (Player.HeldItem.ModItem is DeadRinger && deadRingerEquipped)
            {
                if (cloakMeter >= cloakMeterMax)
                    cloakMeter /= 2f;
                else if (feignDeath && Player.HasBuff<FeignDeath>() && Player.GetModPlayer<TF2Player>().cloakImmuneTime <= 0)
                {
                    cloakMeter -= 60f;
                    Player.GetModPlayer<TF2Player>().cloakImmuneTime += 30;
                    SoundEngine.PlaySound(new SoundStyle("TF2/Content/Sounds/SFX/cloak_hit"), Player.Center);
                }
                info.Damage = (int)(info.Damage * 0.25f);
                Player.AddBuff(ModContent.BuffType<FeignDeath>(), 420);
                if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    IEntitySource source = Player.GetSource_FromThis();
                    int npc = NPC.NewNPC(source, (int)Player.position.X, (int)Player.position.Y, ModContent.NPCType<SpyNPC>(), Player.whoAmI);
                    SpyNPC spawnedModNPC = (SpyNPC)Main.npc[npc].ModNPC;
                    spawnedModNPC.npcOwner = Player.whoAmI;
                    NetMessage.SendData(MessageID.SyncNPC, number: npc);
                }
                else
                {
                    Player.stealth = 1000f;
                    Player.stealthTimer = (int)cloakMeter;
                }
                for (int i = 0; i < Player.MaxBuffs; i++)
                {
                    int buffTypes = Player.buffType[i];
                    if (Main.debuff[buffTypes] && Player.buffTime[i] > 0 && !BuffID.Sets.NurseCannotRemoveDebuff[buffTypes] && !TF2BuffBase.cooldownBuff[buffTypes])
                    {
                        Player.DelBuff(i);
                        i = -1;
                    }
                }
            }
            return feignDeath;
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (KeybindSystem.Cloak.JustPressed && deadRingerEquipped && Player.HasBuff<FeignDeath>())
            {
                SoundEngine.PlaySound(new SoundStyle("TF2/Content/Sounds/SFX/spy_cloak"), Player.Center);
                Player.ClearBuff(ModContent.BuffType<FeignDeath>());
            }
        }
    }
}