using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.Graphics;

namespace LogSpiralsAbstractWeapons.Contents.RandomRainbow;

public class RandomRainbowStaff : ModItem
{

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.RainbowRod);
        Item.shoot = RandomRainbowPortal.ID();
        Item.damage = Item.damage * 3 / 2;
        base.SetDefaults();
    }
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.RainbowRod)
            .AddIngredient(ItemID.LunarBar, 20)
            .AddIngredient(ItemID.FragmentNebula, 10)
            .AddIngredient(ItemID.FragmentSolar, 10)
            .AddIngredient(ItemID.FragmentStardust, 10)
            .AddIngredient(ItemID.FragmentVortex, 10)
            .AddTile(TileID.LunarCraftingStation)
            .Register();
        base.AddRecipes();
    }
    private Vector2 TargetPoint;

    public override void HoldItem(Player player)
    {
        if (TargetPoint != default)
            Dust.NewDustPerfect(TargetPoint, DustID.Clentaminator_Cyan).noGravity = true;
        base.HoldItem(player);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (TargetPoint != default)
        {
            Projectile.NewProjectile(source, TargetPoint, default, type, damage, knockback, player.whoAmI, Main.MouseWorld.X, Main.MouseWorld.Y);
            TargetPoint = default;
        }
        else
            TargetPoint = Main.MouseWorld;
        return false;
    }
}

public class RandomRainbowProjectile : ModProjectile
{
    public override string Texture => $"Terraria/Images/Item_{ItemID.RainbowRod}";

    public override bool PreDraw(ref Color lightColor)
    {
        default(RainbowRodDrawer).Draw(Projectile);
        return false;
    }

    public override void AI()
    {
        Projectile.velocity += Main.rand.NextVector2Unit() * Main.rand.NextFloat(16);
        //Projectile.Center = Vector2.Lerp(Projectile.Center, new Vector2(Projectile.ai[0], Projectile.ai[1]), 1 - Projectile.timeLeft / 30f);
        float factor = Utils.GetLerpValue(10, 70, Projectile.timeLeft, true);
        float iFactor = 1 - factor;
        if (_origCenter == default) _origCenter = Projectile.Center;
        if (_controlCenter == default) _controlCenter = (Projectile.Center + TargetCenter) * .5f + Main.rand.NextVector2Unit() * 64;
        _controlCenter += Projectile.velocity;

        //Projectile.rotation = Projectile.velocity.ToRotation();
        for (int n = 9; n > 0; n--)
        {
            Projectile.oldPos[n] = Projectile.oldPos[n - 1];
            Projectile.oldRot[n] = Projectile.oldRot[n - 1];
        }
        Projectile.Center = _origCenter * factor * factor + 2 * factor * iFactor * _controlCenter + TargetCenter * iFactor * iFactor;
        if (Projectile.oldPos[0] != default)
            Projectile.rotation = (Projectile.Center - Projectile.oldPos[0]).ToRotation();
        Projectile.oldPos[0] = Projectile.Center;
        Projectile.oldRot[0] = Projectile.rotation;
        base.AI();
    }
    Vector2 _controlCenter;
    Vector2 _origCenter;
    Vector2 TargetCenter => new Vector2(Projectile.ai[0], Projectile.ai[1]);
    public override bool ShouldUpdatePosition() => false;
    public override void SetDefaults()
    {
        Projectile.timeLeft = 70;
        Projectile.friendly = true;
        Projectile.width = Projectile.height = 1;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.ignoreWater = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 0;
        base.SetDefaults();
    }
}


public class RandomRainbowPortal : ModProjectile
{
    public override bool PreDraw(ref Color lightColor)
    {
        var _fac = Utils.GetLerpValue(300, 270, MathF.Abs(Projectile.timeLeft - 300), true);
        _fac = MathHelper.SmoothStep(0, 1, _fac);
        float rotation = Main.GlobalTimeWrappedHourly * 5f;
        float scale = 2f * _fac;
        SpriteEffects dir = 0;
        Color mainColor = Main.DiscoColor;

        Vector2 center = Projectile.Center - Main.screenPosition;

        Color colorVortex = mainColor * 0.8f;
        colorVortex.A /= 2;
        Color color1 = Color.Lerp(mainColor, Color.Black, 0.5f);
        color1.A = mainColor.A;
        float sinValue = 0.95f + (rotation * 0.75f).ToRotationVector2().Y * 0.1f;
        color1 *= sinValue;
        float scale1 = 0.6f + scale * 0.6f * sinValue;
        Texture2D voidTex = ModAsset.RandomRainbowPortal2.Value;
        Vector2 voidOrigin = voidTex.Size() / 2f;
        Texture2D vortexTex = ModAsset.RandomRainbowPortal.Value;//TextureAssets.Projectile[ProjectileID.DD2ApprenticeStorm].Value;//;
        if (Projectile.ai[2] == 0)
        {
            Main.EntitySpriteDraw(voidTex, center, null, color1, -rotation + 0.35f, voidOrigin, scale1, dir ^ SpriteEffects.FlipHorizontally, 0);
            Main.EntitySpriteDraw(voidTex, center, null, mainColor, -rotation, voidOrigin, scale, dir ^ SpriteEffects.FlipHorizontally, 0);
            Main.EntitySpriteDraw(voidTex, center, null, mainColor * 0.8f, rotation * 0.5f, voidOrigin, scale * 0.9f, dir, 0);
            Main.EntitySpriteDraw(vortexTex, center, null, colorVortex, -rotation * 0.7f, vortexTex.Size() * .5f, scale, dir ^ SpriteEffects.FlipHorizontally, 0);
            Main.EntitySpriteDraw(vortexTex, center, null, Color.White with { A = 0 }, -rotation * 1.4f, vortexTex.Size() * .5f, scale * .85f, dir ^ SpriteEffects.FlipHorizontally, 0);
        }
        else
        {
            //center = new Vector2(Projectile.ai[0], Projectile.ai[1]) - Main.screenPosition;
            Main.EntitySpriteDraw(voidTex, center, null, color1, rotation - 0.35f, voidOrigin, scale1, dir ^ SpriteEffects.FlipHorizontally, 0);
            Main.EntitySpriteDraw(voidTex, center, null, mainColor, rotation, voidOrigin, scale, dir ^ SpriteEffects.FlipHorizontally, 0);
            Main.EntitySpriteDraw(voidTex, center, null, mainColor * 0.8f, -rotation * 0.5f, voidOrigin, scale * 0.9f, dir, 0);
        }


        return false;
    }

    public override void AI()
    {
        if (Projectile.ai[2] == 0)
        {
            if (Projectile.timeLeft == 600)
            {
                Projectile.NewProjectileDirect(Projectile.GetProjectileSource_FromThis(), new Vector2(Projectile.ai[0], Projectile.ai[1]), default,
                    Type, Projectile.damage, Projectile.knockBack, Projectile.owner, 0, 0, 1);
            }
            if (MathF.Abs(Projectile.timeLeft - 300) < 240)
            {
                Projectile.NewProjectile(Projectile.GetProjectileSource_FromThis(), Projectile.Center, Main.rand.NextVector2Unit() * 16,
        RandomRainbowProjectile.ID(), Projectile.damage, Projectile.knockBack, Projectile.owner, Projectile.ai[0], Projectile.ai[1]);
            }
        }


        base.AI();
    }

    public override void SetDefaults()
    {
        Projectile.hide = true;
        Projectile.timeLeft = 600;
        Projectile.width = Projectile.height = 1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        base.SetDefaults();
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        behindProjectiles.Add(index);
        base.DrawBehind(index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);
    }
}