using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Items;

namespace LogSpiralsAbstractWeapons.Contents.StarBoulderCannon;

public class StarBoulderCannon : ModItem
{
    public override string Texture => $"Terraria/Images/Item_{ItemID.StarCannon}";

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults()
    {
        Item.autoReuse = true;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.useAnimation = 12;
        Item.useTime = 12;
        Item.width = 50;
        Item.height = 18;
        Item.shoot = ModContent.ProjectileType<StarBoulderFlyingProj>();
        Item.useAmmo = AmmoID.FallenStar;
        Item.UseSound = SoundID.Zombie104;
        Item.knockBack = 3f;
        Item.damage = 55;
        Item.shootSpeed = 14f;
        Item.noMelee = true;
        Item.value = 500000;
        Item.rare = ItemRarityID.Purple;
        Item.DamageType = DamageClass.Ranged;
        if (Item.Variant == ItemVariants.RebalancedVariant)
        {
            Item.damage = (int)(Item.damage * 0.9);
            Item.useTime = (int)(Item.useTime * 1.1);
        }
        base.SetDefaults();
    }

    public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        spriteBatch.Draw(TextureAssets.Projectile[ProjectileID.StarCannonStar].Value, position, null, Color.White, DateTime.Now.Millisecond / 250f * MathHelper.TwoPi, new(11, 12), scale, 0, 0);
        base.PostDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);
    }

    public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
    {
        spriteBatch.Draw(TextureAssets.Projectile[ProjectileID.StarCannonStar].Value, Item.Center - Main.screenPosition, null, Color.White, DateTime.Now.Millisecond / 250f * MathHelper.TwoPi, new(11, 12), scale, 0, 0);
        base.PostDrawInWorld(spriteBatch, lightColor, alphaColor, rotation, scale, whoAmI);
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.StarCannon)
            .AddIngredient<BoulderGlove.BoulderGlove>()
            .AddTile(TileID.DemonAltar)
            .Register();
        base.AddRecipes();
    }
}

public class StarBoulderFlyingProj : ModProjectile
{
    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.FallingStar}";

    public override void AI()
    {
        base.AI();
    }

    public override void SetDefaults()
    {
        Projectile.width = 18;
        Projectile.height = 18;
        Projectile.aiStyle = 5;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.alpha = 50;
        Projectile.light = 1f;
        Projectile.DamageType = DamageClass.Ranged;
        base.SetDefaults();
    }

    public override void OnKill(int timeLeft)
    {
        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity * .5f, ModContent.ProjectileType<FallenStarBoulderProjectile>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
        base.OnKill(timeLeft);
    }

    private static void DrawProjWithStarryTrail(Projectile proj, Color projectileColor, SpriteEffects dir)
    {
        Color color = new(255, 255, 255, projectileColor.A - proj.alpha);
        Vector2 vector = proj.velocity;
        Color color2 = Color.Blue * 0.1f;
        Vector2 spinningpoint = new(0f, -4f);
        float num = 0f;
        float t = vector.Length();
        float num2 = Utils.GetLerpValue(3f, 5f, t, clamped: true);
        bool flag = true;
        if (proj.type == 856 || proj.type == 857)
        {
            vector = proj.position - proj.oldPos[1];
            float num3 = vector.Length();
            if (num3 == 0f)
                vector = Vector2.UnitY;
            else
                vector *= 5f / num3;

            Vector2 origin = new(proj.ai[0], proj.ai[1]);
            Vector2 center = Main.player[proj.owner].Center;
            float lerpValue = Utils.GetLerpValue(0f, 120f, origin.Distance(center), clamped: true);
            float num4 = 90f;
            if (proj.type == 857)
            {
                num4 = 60f;
                flag = false;
            }

            float lerpValue2 = Utils.GetLerpValue(num4, num4 * (5f / 6f), proj.localAI[0], clamped: true);
            float lerpValue3 = Utils.GetLerpValue(0f, 120f, proj.Center.Distance(center), clamped: true);
            lerpValue *= lerpValue3;
            lerpValue2 *= Utils.GetLerpValue(0f, 15f, proj.localAI[0], clamped: true);
            color2 = Color.HotPink * 0.15f * (lerpValue2 * lerpValue);
            if (proj.type == 857)
                color2 = proj.GetFirstFractalColor() * 0.15f * (lerpValue2 * lerpValue);

            spinningpoint = new Vector2(0f, -2f);
            float lerpValue4 = Utils.GetLerpValue(num4, num4 * (2f / 3f), proj.localAI[0], clamped: true);
            lerpValue4 *= Utils.GetLerpValue(0f, 20f, proj.localAI[0], clamped: true);
            num = -0.3f * (1f - lerpValue4);
            num += -1f * Utils.GetLerpValue(15f, 0f, proj.localAI[0], clamped: true);
            num *= lerpValue;
            num2 = lerpValue2 * lerpValue;
        }

        Vector2 vector2 = proj.Center + vector;
        Texture2D value = TextureAssets.Projectile[proj.type].Value;
        _ = new Rectangle(0, 0, value.Width, value.Height).Size() / 2f;
        Texture2D value2 = TextureAssets.Extra[91].Value;
        Rectangle value3 = value2.Frame();
        Vector2 origin2 = new(value3.Width / 2f, 10f);
        _ = Color.Cyan * 0.5f * num2;
        Vector2 vector3 = new(0f, proj.gfxOffY);
        float num5 = (float)Main.timeForVisualEffects / 60f;
        Vector2 vector4 = vector2 + vector * 0.5f;
        Color color3 = Color.White * 0.5f * num2;
        color3.A = 0;
        Color color4 = color2 * num2;
        color4.A = 0;
        Color color5 = color2 * num2;
        color5.A = 0;
        Color color6 = color2 * num2;
        color6.A = 0;
        float num6 = vector.ToRotation();
        Main.EntitySpriteDraw(value2, vector4 - Main.screenPosition + vector3 + spinningpoint.RotatedBy((float)Math.PI * 2f * num5), value3, color4, num6 + (float)Math.PI / 2f, origin2, 1.5f + num, SpriteEffects.None);
        Main.EntitySpriteDraw(value2, vector4 - Main.screenPosition + vector3 + spinningpoint.RotatedBy((float)Math.PI * 2f * num5 + (float)Math.PI * 2f / 3f), value3, color5, num6 + (float)Math.PI / 2f, origin2, 1.1f + num, SpriteEffects.None);
        Main.EntitySpriteDraw(value2, vector4 - Main.screenPosition + vector3 + spinningpoint.RotatedBy((float)Math.PI * 2f * num5 + 4.1887903f), value3, color6, num6 + (float)Math.PI / 2f, origin2, 1.3f + num, SpriteEffects.None);
        Vector2 vector5 = vector2 - vector * 0.5f;
        for (float num7 = 0f; num7 < 1f; num7 += 0.5f)
        {
            float num8 = num5 % 0.5f / 0.5f;
            num8 = (num8 + num7) % 1f;
            float num9 = num8 * 2f;
            if (num9 > 1f)
                num9 = 2f - num9;

            Main.EntitySpriteDraw(value2, vector5 - Main.screenPosition + vector3, value3, color3 * num9, num6 + (float)Math.PI / 2f, origin2, 0.3f + num8 * 0.5f, SpriteEffects.None);
        }

        if (flag)
        {
            float rotation = proj.rotation + proj.localAI[1];
            float globalTimeWrappedHourly = Main.GlobalTimeWrappedHourly;
            globalTimeWrappedHourly %= 5f;
            globalTimeWrappedHourly /= 2.5f;
            if (globalTimeWrappedHourly >= 1f)
                globalTimeWrappedHourly = 2f - globalTimeWrappedHourly;

            globalTimeWrappedHourly = globalTimeWrappedHourly * 0.5f + 0.5f;
            Vector2 position = proj.Center - Main.screenPosition;
            Main.instance.LoadItem(75);
            Texture2D value4 = TextureAssets.Item[75].Value;
            Rectangle rectangle = value4.Frame(1, 8);
            Main.EntitySpriteDraw(origin: rectangle.Size() / 2f, texture: value4, position: position, sourceRectangle: rectangle, color: color, rotation: rotation, scale: proj.scale, effects: SpriteEffects.None);
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        SpriteEffects dir = SpriteEffects.None;
        var proj = Projectile;
        if (proj.spriteDirection == -1)
            dir = SpriteEffects.FlipHorizontally;

        if (proj.velocity.X > 0f)
            dir ^= SpriteEffects.FlipHorizontally;

        proj.type = ProjectileID.StarCannonStar;
        DrawProjWithStarryTrail(proj, Color.White, dir);
        proj.type = Type;
        return base.PreDraw(ref lightColor);
    }
}

public class FallenStarBoulderProjectile : ModProjectile
{
    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.FallingStar}";

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 24;
        Projectile.tileCollide = true;
        Projectile.penetrate = -1;
        Projectile.friendly = true;
        Projectile.hostile = true;
        Projectile.ignoreWater = true;
        Projectile.aiStyle = -1;
        ProjectileID.Sets.TrailCacheLength[Type] = 10;
        base.SetDefaults();
    }

    public override bool PreDraw(ref Color lightColor)
    {
        int m = Projectile.oldPos.Length;
        for (int n = m - 1; n >= 0; n--)
        {
            float fac = Utils.GetLerpValue(0, m - 1, n);
            float scaler = MathHelper.Lerp(1, 0.2f, fac);
            Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.oldPos[n] - Main.screenPosition + Main.rand.NextVector2Unit() * MathHelper.Lerp(0, Main.rand.NextFloat(0, 8), fac), null, Main.hslToRgb((fac - Main.GlobalTimeWrappedHourly) % 1, 1, 0.95f) with { A = 0 } * scaler * MathHelper.Clamp(Projectile.localAI[0] / m, 0, 1), Projectile.oldRot[n], new Vector2(12), scaler * 1.2f, 0, 0);//
        }
        Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, new Vector2(12), 1f, 0, 0);//
        return false;
    }

    public override void AI()
    {
        Projectile.localAI[0]++;
        if (Projectile.ai[0] != 0f && Projectile.velocity.Y <= 0f && Projectile.velocity.X == 0f)
        {
            float num218 = 0.5f;
            int i2 = (int)((Projectile.position.X - 8f) / 16f);
            int num219 = (int)(Projectile.position.Y / 16f);
            bool flag9 = false;
            bool flag10 = false;
            if (WorldGen.SolidTile(i2, num219) || WorldGen.SolidTile(i2, num219 + 1))
                flag9 = true;

            i2 = (int)((Projectile.position.X + Projectile.width + 8f) / 16f);
            if (WorldGen.SolidTile(i2, num219) || WorldGen.SolidTile(i2, num219 + 1))
                flag10 = true;

            if (flag9)
            {
                Projectile.velocity.X = num218;
            }
            else if (flag10)
            {
                Projectile.velocity.X = 0f - num218;
            }
            else
            {
                i2 = (int)((Projectile.position.X - 8f - 16f) / 16f);
                num219 = (int)(Projectile.position.Y / 16f);
                flag9 = false;
                flag10 = false;
                if (WorldGen.SolidTile(i2, num219) || WorldGen.SolidTile(i2, num219 + 1))
                    flag9 = true;

                i2 = (int)((Projectile.position.X + Projectile.width + 8f + 16f) / 16f);
                if (WorldGen.SolidTile(i2, num219) || WorldGen.SolidTile(i2, num219 + 1))
                    flag10 = true;

                if (flag9)
                {
                    Projectile.velocity.X = num218;
                }
                else if (flag10)
                {
                    Projectile.velocity.X = 0f - num218;
                }
                else
                {
                    i2 = (int)((Projectile.position.X - 8f - 32f) / 16f);
                    num219 = (int)(Projectile.position.Y / 16f);
                    flag9 = false;
                    flag10 = false;
                    if (WorldGen.SolidTile(i2, num219) || WorldGen.SolidTile(i2, num219 + 1))
                        flag9 = true;

                    i2 = (int)((Projectile.position.X + Projectile.width + 8f + 32f) / 16f);
                    if (WorldGen.SolidTile(i2, num219) || WorldGen.SolidTile(i2, num219 + 1))
                        flag10 = true;

                    if (!flag9 && !flag10)
                    {
                        if ((int)(Projectile.Center.X / 16f) % 2 == 0)
                            flag9 = true;
                        else
                            flag10 = true;
                    }

                    if (flag9)
                        Projectile.velocity.X = num218;
                    else if (flag10)
                        Projectile.velocity.X = 0f - num218;
                }
            }
        }

        Projectile.rotation += Projectile.velocity.X * 0.06f;
        Projectile.ai[0] = 1f;
        if (Projectile.velocity.Y > 16f)
            Projectile.velocity.Y = 16f;

        if (Projectile.velocity.Y <= 6f)
        {
            if (Projectile.velocity.X > 0f && Projectile.velocity.X < 7f)
                Projectile.velocity.X += 0.05f;

            if (Projectile.velocity.X < 0f && Projectile.velocity.X > -7f)
                Projectile.velocity.X -= 0.05f;
        }
        Projectile.velocity.Y += 0.3f;

        if (Projectile.wet)
            Projectile.Kill();

        for (int n = Projectile.oldPos.Length - 1; n > 0; n--)
        {
            Projectile.oldPos[n] = Projectile.oldPos[n - 1];
            Projectile.oldRot[n] = Projectile.oldRot[n - 1];
        }
        Projectile.oldPos[0] = Projectile.Center;
        Projectile.oldRot[0] = Projectile.rotation;
    }

    public override void OnKill(int timeLeft)
    {
        var index = Item.NewItem(Projectile.GetSource_DropAsItem(), (int)Projectile.position.X, (int)Projectile.position.Y, Projectile.width, Projectile.height, 75);
        if (Main.netMode == NetmodeID.MultiplayerClient && index >= 0)
            NetMessage.SendData(MessageID.SyncItem, -1, -1, null, index, 1f);
        base.OnKill(timeLeft);
    }

    public override bool OnTileCollide(Vector2 lastVelocity)
    {
        float num36 = Math.Abs(lastVelocity.X);
        float num37 = Math.Abs(lastVelocity.Y);
        float num38 = 0.95f;
        float num39 = 0.95f;
        if (num36 < 0.5f)
            num38 = 0.1f;
        else if (num36 < 0.75f)
            num38 = 0.25f;
        else if (num36 < 1f)
            num38 = 0.5f;

        if (num37 < 0.5f)
            num39 = 0.1f;
        else if (num37 < 0.75f)
            num39 = 0.25f;
        else if (num37 < 1f)
            num39 = 0.5f;

        bool flag12 = false;
        if (Projectile.velocity.Y != lastVelocity.Y)
        {
            if (Math.Abs(lastVelocity.Y) > 5f)
                flag12 = true;

            Projectile.velocity.Y = (0f - lastVelocity.Y) * num39;
        }

        if (Projectile.velocity.X != lastVelocity.X)
        {
            if (Math.Abs(lastVelocity.X) > 5f)
                flag12 = true;

            Projectile.velocity.X = (0f - lastVelocity.X) * num38;
        }

        if (flag12)
        {
            Projectile.localAI[1] += 1f;
            Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);

            //Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ProjectileID.RollingCactus, 35, 10f, Main.myPlayer);
        }

        if (Projectile.velocity.Length() < 0.1f && Projectile.localAI[0] > 50f)
            Projectile.Kill();

        if (Projectile.localAI[1] > 20f)
            Projectile.Kill();
        return false;
        if (Projectile.velocity.Y != lastVelocity.Y && lastVelocity.Y > 5f)
        {
            Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            Projectile.velocity.Y = (0f - lastVelocity.Y) * 0.2f;
        }

        if (Projectile.velocity.X != lastVelocity.X)
            Projectile.Kill();
        return false;
    }
}