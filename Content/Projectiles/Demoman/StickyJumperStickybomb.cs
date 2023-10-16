﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Achievements;
using Terraria.ID;
using Terraria.ModLoader;
using TF2.Common;

namespace TF2.Content.Projectiles.Demoman
{
    public class StickyJumperStickybomb : ModProjectile
    {
        public int Time
        {
            get => (int)Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        private const int maxPower = 5;
        private float velocityX;
        private float velocityY;

        public bool Stick
        {
            get => Projectile.ai[0] == 1f;
            set => Projectile.ai[0] = value ? 1f : 0f;
        }

        public override void SetStaticDefaults() => DisplayName.SetDefault("Stickybomb");

        public override void SetDefaults()
        {
            Projectile.width = 32;                  // The width of projectile hitbox
            Projectile.height = 32;                 // The height of projectile hitbox
            Projectile.aiStyle = 0;                 // The ai style of the projectile, please reference the source code of Terraria
            Projectile.friendly = false;            // Can the projectile deal damage to enemies?
            Projectile.hostile = false;             // Can the projectile deal damage to the player?
            Projectile.penetrate = -1;              // How many monsters the projectile can penetrate. (OnTileCollide below also decrements penetrate for bounces as well)
            Projectile.timeLeft = 3600;             // The live time for the projectile (60 = 1 second, so 600 is 10 seconds)
            Projectile.alpha = 0;                   // The transparency of the projectile, 255 for completely transparent. (aiStyle 1 quickly fades the projectile in) Make sure to delete this if you aren't using an aiStyle that fades in. You'll wonder why your projectile is invisible.
            Projectile.light = 0f;                  // How much light emit around the projectile
            Projectile.ignoreWater = true;          // Does the projectile's speed be influenced by water?
            Projectile.tileCollide = true;          // Can the projectile collide with tiles?
            Projectile.extraUpdates = 1;            // Set to above 0 if you want the projectile to update multiple time in a frame
            Projectile.scale = 0.5f;                // Change projectile size
            Projectile.Name = "Stickybomb";
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Stick = false;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            velocityX = Projectile.velocity.X;
            velocityY = Projectile.velocity.Y;
            oldVelocity.X = 0;
            oldVelocity.Y = 0;
            Projectile.velocity = new Vector2(0, 0);
            Stick = true;
            return false;
        }


        public override bool PreDraw(ref Color lightColor)
        {
            Main.instance.LoadProjectile(Projectile.type);
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            // Redraw the projectile with the color not influenced by light
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Vector2 drawPos = Projectile.oldPos[i] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }

            return true;
        }


        public override bool PreAI()
        {
            if (Time >= maxPower && Stick == false)
            {
                Projectile.velocity.Y = Projectile.velocity.Y + 0.2f;
                Time = maxPower; // set ai1 to maxTicks continuously
            }
            return true;
        }
        public override void AI()
        {
            Time++;
            if (Projectile.owner == Main.myPlayer && Projectile.timeLeft == 0)
            {
                Projectile.tileCollide = false;
                // Set to transparent. This projectile technically lives as transparent for about 3 frames
                Projectile.alpha = 255;
                // Change the hitbox size, centered about the original projectile center. This makes the projectile damage enemies during the explosion.
                Projectile.position = Projectile.Center;
                Projectile.width = 250;
                Projectile.height = 250;
                Projectile.Center = Projectile.position;
                if (TF2.FindPlayer(Projectile, 250f))
                {
                    velocityX *= 10f;
                    velocityY *= 10f;
                    velocityX = Utils.Clamp(velocityX, -15f, 15f);
                    velocityY = Utils.Clamp(velocityY, -15f, 15f);
                    Main.player[Projectile.owner].velocity -= new Vector2(velocityX, velocityY);
                }
            }
            if (!Stick && Projectile.timeLeft != 0)
                StartingAI();
            else
                GroundAI();
        }

        private void StartingAI()
        {
            if (!Stick)
                Projectile.timeLeft = 3600;
            if (Projectile.timeLeft == 0)
            {
                Stick = true;
                Projectile.velocity = new Vector2(0, 0);
            }
            // Make sure to set the rotation accordingly to the velocity, and add some to work around the sprite's rotation
            // Please notice the MathHelper usage, offset the rotation by 90 degrees (to radians because rotation uses radians) because the sprite's rotation is not aligned!
            Projectile.rotation = Projectile.position.ToRotation() + MathHelper.ToRadians(90f);
        }

        private void GroundAI()
        {
            if (Projectile.timeLeft > 0)
                Projectile.timeLeft = 3600;
        }

        public override void Kill(int timeLeft) => Explode();

        private void Explode()
        {
            // Play explosion sound
            SoundEngine.PlaySound(new SoundStyle("TF2/Content/Sounds/SFX/sticky_jumper_explode1"), Projectile.Center);
            // Smoke Dust spawn
            for (int i = 0; i < 50; i++)
            {
                int dustIndex = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 2f);
                Main.dust[dustIndex].velocity *= 1.4f;
            }
            // Fire Dust spawn
            for (int i = 0; i < 80; i++)
            {
                int dustIndex = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 3f);
                Main.dust[dustIndex].noGravity = true;
                Main.dust[dustIndex].velocity *= 5f;
                dustIndex = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 2f);
                Main.dust[dustIndex].velocity *= 3f;
            }
            /*
            // Large Smoke Gore spawn
            for (int g = 0; g < 2; g++)
            {
                var projectileSource = Projectile.GetSource_FromThis();
                int goreIndex = GoreID.Smoke1;
                Gore.NewGore(projectileSource, Projectile.position, new Vector2(Main.rand.Next(61, 64), Main.rand.Next(61, 64)), goreIndex);
                Main.gore[goreIndex].scale = 1.5f;
                Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X + 1.5f;
                Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y + 1.5f;
                goreIndex = GoreID.Smoke2;
                Gore.NewGore(projectileSource, Projectile.position, new Vector2(Main.rand.Next(61, 64), Main.rand.Next(61, 64)), goreIndex);
                Main.gore[goreIndex].scale = 1.5f;
                Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X - 1.5f;
                Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y + 1.5f;
                goreIndex = GoreID.Smoke3;
                Gore.NewGore(projectileSource, Projectile.position, new Vector2(Main.rand.Next(61, 64), Main.rand.Next(61, 64)), goreIndex);
                Main.gore[goreIndex].scale = 1.5f;
                Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X + 1.5f;
                Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y - 1.5f;
                goreIndex = GoreID.Smoke1;
                Gore.NewGore(projectileSource, Projectile.position, new Vector2(Main.rand.Next(61, 64), Main.rand.Next(61, 64)), goreIndex);
                Main.gore[goreIndex].scale = 1.5f;
                Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X - 1.5f;
                Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y - 1.5f;
            }
            */
            // reset size to normal width and height.
            Projectile.position.X = Projectile.position.X + Projectile.width / 2;
            Projectile.position.Y = Projectile.position.Y + Projectile.height / 2;
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.position.X = Projectile.position.X - Projectile.width / 2;
            Projectile.position.Y = Projectile.position.Y - Projectile.height / 2;

            // TODO, tmodloader helper method

            int explosionRadius = 3;
 
            int minTileX = (int)(Projectile.position.X / 16f - explosionRadius);
            int maxTileX = (int)(Projectile.position.X / 16f + explosionRadius);
            int minTileY = (int)(Projectile.position.Y / 16f - explosionRadius);
            int maxTileY = (int)(Projectile.position.Y / 16f + explosionRadius);
            if (minTileX < 0)
            {
                minTileX = 0;
            }
            if (maxTileX > Main.maxTilesX)
            {
                maxTileX = Main.maxTilesX;
            }
            if (minTileY < 0)
            {
                minTileY = 0;
            }
            if (maxTileY > Main.maxTilesY)
            {
                maxTileY = Main.maxTilesY;
            }
            if (ModContent.GetInstance<TF2Config>().Explosions)
            {
                bool canKillWalls = false;
                for (int x = minTileX; x <= maxTileX; x++)
                {
                    for (int y = minTileY; y <= maxTileY; y++)
                    {
                        float diffX = Math.Abs(x - Projectile.position.X / 16f);
                        float diffY = Math.Abs(y - Projectile.position.Y / 16f);
                        double distance = Math.Sqrt((double)(diffX * diffX + diffY * diffY));
                        if (distance < explosionRadius && Main.tile[x, y] != null && Main.tile[x, y].WallType == 0)
                        {
                            canKillWalls = true;
                            break;
                        }
                    }
                }
                AchievementsHelper.CurrentlyMining = true;
                for (int i = minTileX; i <= maxTileX; i++)
                {
                    for (int j = minTileY; j <= maxTileY; j++)
                    {
                        float diffX = Math.Abs(i - Projectile.position.X / 16f);
                        float diffY = Math.Abs(j - Projectile.position.Y / 16f);
                        double distanceToTile = Math.Sqrt((double)(diffX * diffX + diffY * diffY));
                        if (distanceToTile < explosionRadius)
                        {
                            bool canKillTile = true;
                            if (Main.tile[i, j] != null && Main.tile[i, j].HasTile)
                            {
                                canKillTile = true;
                                if (Main.tileDungeon[Main.tile[i, j].TileType] || Main.tile[i, j].TileType == 88 || Main.tile[i, j].TileType == 21 || Main.tile[i, j].TileType == 26 || Main.tile[i, j].TileType == 107 || Main.tile[i, j].TileType == 108 || Main.tile[i, j].TileType == 111 || Main.tile[i, j].TileType == 226 || Main.tile[i, j].TileType == 237 || Main.tile[i, j].TileType == 221 || Main.tile[i, j].TileType == 222 || Main.tile[i, j].TileType == 223 || Main.tile[i, j].TileType == 211 || Main.tile[i, j].TileType == 404)
                                {
                                    canKillTile = false;
                                }
                                if (!Main.hardMode && Main.tile[i, j].TileType == 58)
                                {
                                    canKillTile = false;
                                }
                                if (!TileLoader.CanExplode(i, j))
                                {
                                    canKillTile = false;
                                }
                                if (canKillTile)
                                {
                                    WorldGen.KillTile(i, j, false, false, false);
                                    if (!Main.tile[i, j].HasTile && Main.netMode != NetmodeID.SinglePlayer)
                                    {
                                        NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, i, j, 0f, 0, 0, 0);
                                    }
                                }
                            }
                            if (canKillTile)
                            {
                                for (int x = i - 1; x <= i + 1; x++)
                                {
                                    for (int y = j - 1; y <= j + 1; y++)
                                    {
                                        if (Main.tile[x, y] != null && Main.tile[x, y].WallType > 0 && canKillWalls && WallLoader.CanExplode(x, y, Main.tile[x, y].WallType))
                                        {
                                            WorldGen.KillWall(x, y, false);
                                            if (Main.tile[x, y].WallType == 0 && Main.netMode != NetmodeID.SinglePlayer)
                                            {
                                                NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 2, x, y, 0f, 0, 0, 0);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                AchievementsHelper.CurrentlyMining = false;
            }
        }
    }
}