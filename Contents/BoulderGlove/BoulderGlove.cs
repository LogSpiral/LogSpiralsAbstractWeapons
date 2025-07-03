using LogSpiralsAbstractWeapons.Contents.EZBlessing;
using LogSpiralsAbstractWeapons.Tools;
using Microsoft.Xna.Framework;
using System;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
namespace LogSpiralsAbstractWeapons.Contents.BoulderGlove;

public class BoulderGlove : ModItem
{
    public override string Texture => $"Terraria/Images/Item_{ItemID.MechanicalGlove}";

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
        base.SetStaticDefaults();
    }

    public override void SetDefaults()
    {
        Item.rare = ItemRarityID.Green;
        Item.damage = 40;
        Item.DamageType = DamageClass.Throwing;
        Item.width = 22;
        Item.height = 28;
        Item.useTime = Item.useAnimation = 30;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.channel = true;
        Item.noMelee = true;
        Item.value = Item.sellPrice(0, 5);
        base.SetDefaults();
    }
    public override void UseStyle(Player player, Rectangle heldItemFrame)
    {
        float factor = player.itemAnimation / (float)player.itemAnimationMax;

        float rotation = MathHelper.PiOver4;
        if (player.itemAnimation > 5)
        {
            float fac = Utils.GetLerpValue(1, 1 / 3f, factor, true);
            float k = 1;
            if (fac < .5f)
                k = MathHelper.SmoothStep(0.25f, 1f, Utils.GetLerpValue(0, 0.5f, fac));
            else if (fac < .75f)
                k = MathHelper.SmoothStep(1f, 0.5f, Utils.GetLerpValue(0.5f, 0.75f, fac));
            else
                k = MathHelper.SmoothStep(0.5f, 0.75f, Utils.GetLerpValue(0.75f, 1f, fac));

            rotation = MathHelper.Pi * k;
        }
        else
        {
            float fac = Utils.GetLerpValue(1 / 3f, 0, factor, true);
            fac = MathF.Pow(fac, 3);
            rotation = MathHelper.Lerp(MathHelper.PiOver4 * 3, MathHelper.PiOver4 * -3, fac);
        }
        rotation *= -1;
        player.itemRotation = rotation + MathHelper.PiOver2;
        player.itemRotation *= player.direction;
        player.itemLocation = player.Center - (rotation + MathHelper.PiOver2).ToRotationVector2() * 12;// ;
        if (player.direction < 0)
            player.itemLocation += Vector2.UnitX * 16;
        //player.itemLocation -= rotation.ToRotationVector2() * 11 * new Vector2(player.direction, 1);
        player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, player.itemRotation - MathHelper.Pi);
        //player.itemLocation = default;
        base.UseStyle(player, heldItemFrame);
    }

    int chargeCounter;


    public override bool? UseItem(Player player)
    {
        if (player.itemAnimation is 10 or 11)
        {

            float factor = MathHelper.Clamp(chargeCounter / 60f, 0, 1);
            factor = MathHelper.SmoothStep(0, 1, factor);
            if (player.controlUseItem)
            {
                player.itemAnimation = 11;
                chargeCounter++;
                float k = MathF.Exp((1 - factor) * 4);
                for (int n = 0; n < Math.Min(4, k * .5f - 1); n++)
                    Dust.NewDustPerfect(player.Center + (factor * MathHelper.Pi + MathHelper.PiOver4 + MathHelper.PiOver2 * n).ToRotationVector2() * k, DustID.Stone, default).noGravity = true;
                if (chargeCounter == 60)
                {
                    SoundEngine.PlaySound(SoundID.MaxMana with { Volume = 2f, MaxInstances = -1 }, player.Center);

                    for (int n = 0; n < 15; n++)
                        Dust.NewDustPerfect(player.Center, DustID.FrostStaff, Main.rand.NextVector2Unit() * Main.rand.NextFloat(2, 16)).noGravity = true;
                }
            }
        }
        if (player.itemAnimation == 1)
        {
            float factor = MathHelper.Clamp(chargeCounter / 60f, 0, 1);
            factor = MathHelper.SmoothStep(0, 1, factor);
            for (int n = 0; n < 5; n++)
            {
                var offset = (n / 30f * MathHelper.TwoPi).ToRotationVector2() * new Vector2(Main.rand.NextFloat(16, 32), Main.rand.NextFloat(16, 32));
                Dust.NewDustPerfect(player.Center + offset, DustID.Stone, new Vector2(-offset.Y, offset.X) * .25f).noGravity = true;
            }
            chargeCounter = 0;
            if (player.whoAmI == Main.myPlayer)
            {
                var center = player.Center;
                var target = Main.MouseWorld - center;
                Projectile.NewProjectile(player.GetSource_ItemUse(Item), center, target.SafeNormalize(default) * MathHelper.Lerp(8f, 64f, factor) + Main.rand.NextVector2Unit() * Main.rand.NextFloat(4),
                    BouncyBoulder.ID(), (int)(player.GetWeaponDamage(Item) * MathHelper.Lerp(0.5f, 4f, factor)), 5, player.whoAmI, 0, 0, 1);
            }
        }
        return base.UseItem(player);
    }


    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.StoneBlock, 500)
            .AddIngredient(ItemID.Boulder, 10)
            .AddIngredient(ItemID.TitanGlove)
            .AddTile(TileID.DemonAltar)
            .Register();
    }
}
public class BouncyBoulder : ModProjectile
{
    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.Boulder}";

    public override bool PreDraw(ref Color lightColor)
    {
        var tex = TextureAssets.Projectile[Type].Value;
        for (int n = 9; n >= 0; n--) 
        {
            if (oldDrawDatas[n].Y is float bounceFac && bounceFac is 0) continue;
            Main.spriteBatch.Draw(
                tex,
                Projectile.oldPos[n] - Main.screenPosition,
                null,
                lightColor * MathF.Pow(1 - n * .1f,3.0f) * (n == 0 ? 1f : .5f),
                0,
                Projectile.oldRot[n],
                oldDrawDatas[n].X,
                new Vector2(16),
                new Vector2(bounceFac, .5f / bounceFac + .5f),
                false);
        }

        return false;
    }
    readonly Vector2[] oldDrawDatas = new Vector2[10];
    public override void AI()
    {
        Projectile.velocity += Vector2.UnitY;
        Projectile.rotation += Vector2.Dot(Vector2.One, Projectile.velocity) * .0125f;
        float v = Projectile.velocity.Length();
        if (Projectile.ai[2] == 0) Projectile.ai[2] = 1;
        float assistFac = 0f;
        if (Projectile.velocity != default && Projectile.oldVelocity != default)
        {
            assistFac = 1 - Vector2.Dot(Projectile.velocity, Projectile.oldVelocity) / v / Projectile.oldVelocity.Length();
            assistFac *= .5f;
            assistFac = MathHelper.Lerp(assistFac, 1, MathF.Exp(-MathF.Pow(Projectile.velocity.Y / 12f, 2)));
            //assistFac = MathF.Pow(assistFac, 0.25f);
        }
        Projectile.ai[2] =
            MathHelper.Lerp(Projectile.ai[2],
            MathHelper.Lerp(MathF.Log(v + 1) * .35f + 1, 1, assistFac),
            MathHelper.Lerp(0.02f + 0.02f / (1 + 16 / v), 1, assistFac));

        Projectile.ai[1] = Utils.AngleLerp(Projectile.ai[1], Projectile.velocity.ToRotation(), .5f);
        Projectile.knockBack = v;
        //Main.NewText((Projectile.ai[2],assistFac));
        base.AI();
    }
    public override void PostAI()
    {
        for (int n = 9; n > 0; n--)
        {
            Projectile.oldPos[n] = Projectile.oldPos[n - 1];
            Projectile.oldRot[n] = Projectile.oldRot[n - 1];
            oldDrawDatas[n] = oldDrawDatas[n - 1];
        }
        Projectile.oldPos[0] = Projectile.Center;
        Projectile.oldRot[0] = Projectile.rotation;
        oldDrawDatas[0] = new Vector2(Projectile.ai[1], Projectile.ai[2]);
        base.PostAI();
    }
    public override void SetDefaults()
    {
        Projectile.tileCollide = true;
        Projectile.width = Projectile.height = 16;
        Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Throwing;
        Projectile.ignoreWater = true;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 5;
        base.SetDefaults();
    }

    public override bool OnTileCollide(Vector2 lastVelocity)
    {
        Projectile.ai[2] = .5f / (1 + lastVelocity.Length() * .25f) + .5f;
        for (int n = 0; n < 15; n++)
            Collision.HitTiles(Projectile.Center, lastVelocity * .5f, 16, 16);
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
    }
}
