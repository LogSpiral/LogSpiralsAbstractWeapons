using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using MonoMod.Cil;
using System;
using System.Reflection;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using static System.Net.Mime.MediaTypeNames;
using Terraria.ID;
using System.Collections.Generic;
using Terraria.Localization;

namespace LogSpiralsAbstractWeapons.Contents.EZBlessing;

/*
public abstract class EZBlessing : ModItem
{
    public override string Texture => $"Terraria/Images/Item_{ItemID.FallenStar}";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
    }
    public override void SetDefaults()
    {
        Item.useTime = Item.useAnimation = 60;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.UseSound = SoundID.Item9;
        Item.mana = 100;
        Item.shoot = 1;
        Item.shootSpeed = 1;
        base.SetDefaults();
    }
    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        Main.NewText(Star.starfallBoost);
        Star.starfallBoost = 114514;
        return false;
    }
}
public class BlessingTier1 :EZBlessing
{
}*/
public class EZBlessingPlayer : ModPlayer
{
    public bool HasFaintStarlight;
    public override void ResetEffects()
    {
        HasFaintStarlight = false;
        base.ResetEffects();
    }
    public static int FindClosestFaintStarlightPlayer(Vector2 position)
    {
        float distance = float.MaxValue;
        int index = -1;
        foreach (var player in Main.player)
        {
            if (!player.active) continue;
            if (!player.GetModPlayer<EZBlessingPlayer>().HasFaintStarlight) continue;
            float currentDistance = Vector2.Distance(position, player.Center);
            if (currentDistance < distance)
            {
                distance = currentDistance;
                index = player.whoAmI;
            }
        }
        return index;
    }
}
public class EZBlessingSystem : ModSystem
{
    public static void SetCurrentBoost(float boost)
    {
        if (starFallBoostOrig == 0)
            starFallBoostOrig = Star.starfallBoost;
        if (Star.starfallBoost < boost)
            Star.starfallBoost = boost;
        HasFaintStarlightInWorld = true;
    }
    public static bool HasFaintStarlightInWorld;
    public static float starFallBoostOrig;
    public static bool HasEZBlessing;
    public override void Load()
    {
        IL_Item.DespawnIfMeetingConditions += FallenStarTransform;
        IL_WorldGen.UpdateWorld_Inner += FallenStarSpawnModify;
        IL_Projectile.VanillaAI += SoundEffectOptifine;
        IL_Projectile.AI_148_StarSpawner += FSSpawnModify;
        base.Load();
    }

    private void FSSpawnModify(ILContext il)
    {
        var cursor = new ILCursor(il);
        if (!cursor.TryGotoNext(i => i.MatchPop())) return;
        cursor.Remove();
        cursor.EmitDelegate<Action<int>>(index =>
        {
            if (HasEZBlessing)
            {
                var projectile = Main.projectile[index];
                if (Main.rand.NextBool(3))
                    projectile.hostile = true;
                projectile.velocity *= Main.rand.NextFloat(1, 3);
            }
        });
    }

    private static void SoundEffectOptifine(ILContext il)
    {
        var cursor = new ILCursor(il);
        if (!cursor.TryGotoNext(i => i.MatchLdsflda(typeof(SoundID), "Item9")))
            return;
        cursor.RemoveRange(7);
        cursor.EmitLdarg0();
        cursor.EmitDelegate<Action<Projectile>>(proj =>
        {
            SoundEngine.PlaySound(SoundID.Item9 with { MaxInstances = -1 }, proj.position);
        });
    }

    private static void FallenStarSpawnModify(ILContext il)
    {
        var cursor = new ILCursor(il);
        if (!cursor.TryGotoNext(i => i.MatchCall(typeof(Main), "get_expertMode")))
            return;
        int index = cursor.Index;
        cursor.Index += 6;
        var label = cursor.MarkLabel();
        cursor.Index = index;
        cursor.EmitLdsfld(typeof(EZBlessingSystem).GetField(nameof(HasFaintStarlightInWorld), BindingFlags.Static | BindingFlags.Public));
        cursor.EmitBrtrue(label);

        if (!cursor.TryGotoNext(i => i.MatchStloc(27)))
            return;
        cursor.EmitLdloc(25);
        cursor.EmitDelegate<Func<int, Vector2, int>>((idx, vec) =>
        {
            int i = -1;
            if (HasFaintStarlightInWorld)
                i = EZBlessingPlayer.FindClosestFaintStarlightPlayer(vec);

            return i != -1 ? i : idx;
        });
    }

    private void FallenStarTransform(ILContext il)
    {
        var cursor = new ILCursor(il);
        if (!cursor.TryGotoNext(i => i.MatchLdsfld(typeof(Main), "netMode"))) return;
        var label = cursor.MarkLabel();

        cursor.Index -= 9;
        cursor.EmitLdarg0();
        cursor.EmitDelegate<Func<Item, bool>>(
            item =>
            {
                var flag = Main.rand.NextBool(10);
                if (flag)
                {
                    SoundEngine.PlaySound(SoundID.Shimmer1);
                    int stack = item.stack;
                    item.SetDefaults(ModContent.ItemType<SpecialFallenStar>());
                    item.stack = stack;
                }
                return flag;
            });
        cursor.EmitBrtrue(label);
    }

    public override void PostUpdateEverything()
    {
        if (!HasFaintStarlightInWorld && starFallBoostOrig != 0)
        {
            Star.starfallBoost = starFallBoostOrig;
            starFallBoostOrig = 0;
        }
        HasEZBlessing = false;
        HasFaintStarlightInWorld = false;
        // Main.NewText("当前落星生成倍率：" + Star.starfallBoost);
        base.PostUpdateEverything();
    }
}

public class SpecialFallenStar : ModItem
{
    public override string Texture => $"Terraria/Images/Item_{ItemID.FallenStar}";

    public override void SetStaticDefaults()
    {
        Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 8));
        ItemID.Sets.ItemIconPulse[Item.type] = true;

        Item.ResearchUnlockCount = 1;
        base.SetStaticDefaults();
    }

    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.FallenStar);
        Item.ammo = AmmoID.None;
        Item.consumable = false;
        base.SetDefaults();
    }

    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        for (int n = 0; n < 4; n++)
            spriteBatch.Draw(TextureAssets.Item[Type].Value, Item.Center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 8) - Main.screenPosition, TextureAssets.Item[Type].Frame(1, 8, 0, Main.rand.Next(0, 8)), Color.White with { A = 0 } * .5f, rotation, new Vector2(11), 1, 0, 0);
        return true;
    }

    public override void Update(ref float gravity, ref float maxFallSpeed)
    {
        if (!Main.dayTime && !Main.remixWorld && !Item.shimmered && !Item.beingGrabbed)
        {
            for (int j = 0; j < 10; j++)
            {
                Dust.NewDust(Item.position, Item.width, Item.height, 15, Item.velocity.X, Item.velocity.Y, 150, default(Color), 1.2f);
            }

            for (int k = 0; k < 3; k++)
            {
                Gore.NewGore(Item.GetSource_FromThis(), Item.position, Item.velocity, Main.rand.Next(16, 18));
            }

            Item.active = false;
            Item.type = ItemID.None;
            Item.stack = 0;
            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, Item.whoAmI);
        }
    }
}

public class FaintStarlight : ModItem
{
    public override string Texture => $"Terraria/Images/Item_{ItemID.FallenStar}";

    public override Color? GetAlpha(Color lightColor)
    {
        return Color.MediumPurple with { A = 0 };
    }
    public override void SetStaticDefaults()
    {
        Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 8));
        ItemID.Sets.ItemIconPulse[Item.type] = true;
        Item.ResearchUnlockCount = 1;
        base.SetStaticDefaults();
    }
    public override void SetDefaults()
    {

        Item.rare = ItemRarityID.Green;
        Item.accessory = true;
        Item.value = Item.sellPrice(0, 1);
        Item.width = Item.height = 22;
        base.SetDefaults();
    }
    public override void HoldItem(Player player)
    {
        EZBlessingSystem.SetCurrentBoost(5);

        player.GetModPlayer<EZBlessingPlayer>().HasFaintStarlight = true;
        base.HoldItem(player);
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        EZBlessingSystem.SetCurrentBoost(5);

        player.GetModPlayer<EZBlessingPlayer>().HasFaintStarlight = true;
        base.UpdateAccessory(player, hideVisual);
    }
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<SpecialFallenStar>()
            .AddIngredient(ItemID.ManaCrystal)
            .AddTile(TileID.Anvils)
            .Register();
        base.AddRecipes();
    }
}

public class StarSlingshot : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
        base.SetStaticDefaults();
    }

    public override void SetDefaults()
    {
        Item.rare = ItemRarityID.Orange;
        Item.accessory = true;
        Item.value = Item.sellPrice(0, 5);
        Item.width = Item.height = 55;
        Item.useTime = 15;
        Item.useAnimation = 15;
        Item.useAmmo = AmmoID.FallenStar;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.DamageType = DamageClass.Ranged;
        Item.noUseGraphic = true;
        //Item.shoot = ProjectileID.FallingStar;
        //Item.shootSpeed = 1;
        Item.channel = true;
        Item.damage = 20;
        Item.noMelee = true;
        base.SetDefaults();
    }

    int chargeCounter;


    public override bool? UseItem(Player player)
    {
        if (player.itemAnimation is 1 or 2 && player.HasAmmo(Item))
        {

            float factor = MathHelper.Clamp(chargeCounter / 60f, 0, 1);
            factor = MathHelper.SmoothStep(0, 1, factor);
            if (player.controlUseItem)
            {
                player.itemAnimation = 2;
                chargeCounter++;
                for (int n = 0; n < 4; n++)
                    Dust.NewDustPerfect(player.Center + (factor * MathHelper.Pi + MathHelper.PiOver4 + MathHelper.PiOver2 * n).ToRotationVector2() * MathF.Exp((1 - factor) * 6), DustID.FrostStaff, default).noGravity = true;

                if (chargeCounter == 60)
                {
                    SoundEngine.PlaySound(SoundID.MaxMana with { Volume = 2f, MaxInstances = -1 }, player.Center);
                    for (int n = 0; n < 30; n++)
                        Gore.NewGore(player.Center, Main.rand.NextVector2Circular(8, 8), 16);

                }
            }
            else
            {
                if (!player.PickAmmo(Item, out _, out _, out _, out _, out _)) return base.UseItem(player);
                for (int n = 0; n < 30; n++)
                {
                    var offset = (n / 30f * MathHelper.TwoPi).ToRotationVector2() * new Vector2(Main.rand.NextFloat(16, 32), Main.rand.NextFloat(16, 32));
                    Dust.NewDustPerfect(player.Center + offset, DustID.FrostStaff, new Vector2(-offset.Y, offset.X) * .25f).noGravity = true;
                }
                SoundEngine.PlaySound(factor switch
                {
                    < .25f => SoundID.Item9,
                    > .75f => SoundID.Item74,
                    _ => SoundID.Item43
                });
                chargeCounter = 0;
                if (player.whoAmI == Main.myPlayer)
                {
                    var center = player.Center - Vector2.UnitX * player.direction * 48 - Vector2.UnitY * 96;
                    var target = Main.MouseWorld + Main.rand.NextVector2Unit() * Main.rand.NextFloat(0,4) - center;
                    Projectile.NewProjectile(player.GetSource_ItemUse(Item), center, default,
                        ShootingStar.ID(), (int)(player.GetWeaponDamage(Item) * MathHelper.Lerp(0.5f, 4f, factor)), 5, player.whoAmI, target.X, target.Y, factor);
                }
            }
        }
        return base.UseItem(player);
    }


    public override void HoldItem(Player player)
    {
        EZBlessingSystem.SetCurrentBoost(10);

        player.GetModPlayer<EZBlessingPlayer>().HasFaintStarlight = true;
        base.HoldItem(player);
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        EZBlessingSystem.SetCurrentBoost(10);

        player.GetModPlayer<EZBlessingPlayer>().HasFaintStarlight = true;
        base.UpdateAccessory(player, hideVisual);
    }
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<FaintStarlight>()
            .AddIngredient(ItemID.SlimySaddle)
            .AddIngredient(ItemID.Starfury)
            .AddTile(TileID.DemonAltar)
            .Register();
        base.AddRecipes();
    }
}
public class ShootingStar : ModProjectile
{

    internal static void DrawStarryTail(Projectile proj, Color MainColor, Color CoreColor)
    {
#if true
        Color color = new Color(255, 255, 255, CoreColor.A - proj.alpha);
        Vector2 vector = proj.velocity;
        Color color2 = MainColor;
        Vector2 spinningpoint = new Vector2(0f, -4f);
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

            Vector2 origin = new Vector2(proj.ai[0], proj.ai[1]);
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
        Vector2 origin2 = new Vector2((float)value3.Width / 2f, 10f);
        _ = Color.Cyan * 0.5f * num2;
        Vector2 vector3 = new Vector2(0f, proj.gfxOffY);
        float num5 = (float)Main.timeForVisualEffects / 60f;
        Vector2 vector4 = vector2 + vector * 0.5f;
        Color color3 = CoreColor * 0.5f * num2;
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
#else
        Texture2D value20 = TextureAssets.Projectile[proj.type].Value;
        Rectangle rectangle6 = new Rectangle(0, 0, value20.Width, value20.Height);
        Vector2 origin9 = rectangle6.Size() / 2f;
        Color alpha3 = Color.White;
        Texture2D value21 = TextureAssets.Extra[91].Value;
        Rectangle value22 = value21.Frame();
        Vector2 origin10 = new Vector2((float)value22.Width / 2f, 10f);
        Vector2 vector34 = new Vector2(0f, proj.gfxOffY);
        Vector2 spinningpoint = new Vector2(0f, -10f);
        float num189 = (float)Main.timeForVisualEffects / 60f;
        Vector2 vector35 = proj.Center + proj.velocity;
        Color color44 = Color.Blue * 0.2f;
        Color color45 = Color.White * 0.5f;
        color45.A = 0;
        float num190 = 0f;
        if (true)
        {
            color44 = MainColor;
            color45 = CoreColor;
            color45.A = 0;
            num190 = -0.1f;
        }

        Color color46 = color44;
        color46.A = 0;
        Color color47 = color44;
        color47.A = 0;
        Color color48 = color44;
        color48.A = 0;
        Main.EntitySpriteDraw(value21, vector35 - Main.screenPosition + vector34 + spinningpoint.RotatedBy((float)Math.PI * 2f * num189), value22, color46, proj.velocity.ToRotation() + (float)Math.PI / 2f, origin10, 1.5f + num190, SpriteEffects.None);
        Main.EntitySpriteDraw(value21, vector35 - Main.screenPosition + vector34 + spinningpoint.RotatedBy((float)Math.PI * 2f * num189 + (float)Math.PI * 2f / 3f), value22, color47, proj.velocity.ToRotation() + (float)Math.PI / 2f, origin10, 1.1f + num190, SpriteEffects.None);
        Main.EntitySpriteDraw(value21, vector35 - Main.screenPosition + vector34 + spinningpoint.RotatedBy((float)Math.PI * 2f * num189 + 4.1887903f), value22, color48, proj.velocity.ToRotation() + (float)Math.PI / 2f, origin10, 1.3f + num190, SpriteEffects.None);
        Vector2 vector36 = proj.Center - proj.velocity * 0.5f;
        for (float num191 = 0f; num191 < 1f; num191 += 0.5f)
        {
            float num192 = num189 % 0.5f / 0.5f;
            num192 = (num192 + num191) % 1f;
            float num193 = num192 * 2f;
            if (num193 > 1f)
                num193 = 2f - num193;

            Main.EntitySpriteDraw(value21, vector36 - Main.screenPosition + vector34, value22, color45 * num193, proj.velocity.ToRotation() + (float)Math.PI / 2f, origin10, 0.3f + num192 * 0.5f, SpriteEffects.None);
        }
#endif

    }


    public override string Texture => $"Terraria/Images/Item_{ItemID.FallenStar}";
    float Timer { get; set; }
    Vector2 TargetPos => new Vector2(Projectile.ai[0], Projectile.ai[1]);
    float ChargeFactor => Projectile.ai[2];
    float Factor => 1.8f * MathF.Pow(Timer / 120f, 2) - 0.8f * Timer / 120f;
    float DFactor => Timer / 7200f * 1.8f - 1 / 120f * 0.8f;
    public override void SetDefaults()
    {
        Projectile.timeLeft = 480;
        Projectile.extraUpdates = 3;
        Projectile.width = Projectile.height = 32;
        Projectile.tileCollide = false;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.penetrate = -1;
        base.SetDefaults();
    }
    public override void AI()
    {
        Projectile.velocity = Vector2.Lerp(-Vector2.UnitY * 960 * ChargeFactor, TargetPos + Vector2.UnitY + Vector2.UnitY * 960 * ChargeFactor, Factor) * 2 * DFactor;

        Timer++;
        if (Factor >= .75f)
            Projectile.tileCollide = true;

        Projectile.rotation += Projectile.velocity.Length() * .05f;

        for (int n = 9; n > 0; n--)
        {
            Projectile.oldPos[n] = Projectile.oldPos[n - 1];
            Projectile.oldRot[n] = Projectile.oldRot[n - 1];
        }
        Projectile.oldPos[0] = Projectile.Center;
        Projectile.oldRot[0] = Projectile.rotation;

        Projectile.knockBack = Projectile.velocity.Length();
        base.AI();
    }
    public override bool PreDraw(ref Color lightColor)
    {
        float alpha = MathHelper.Clamp(Timer / 60f, 0, 1);
        Vector2 origCen = Projectile.Center;
        int origType = Projectile.type;
        float origRotation = Projectile.rotation;
        Projectile.type = ProjectileID.SuperStar;
        for (int n = 9; n >= 0; n--)
        {
            float a = ((10 - n) * .1f) * alpha;
            Main.EntitySpriteDraw(TextureAssets.Item[ItemID.FallenStar].Value,
                Projectile.oldPos[n] - Main.screenPosition, new Rectangle(0, 0, 22, 24), Color.White with { A = 0 } * a, Projectile.oldRot[n], new Vector2(11, 12), 1, 0, 0);


            Projectile.Center = Projectile.oldPos[n];
            Projectile.rotation = Projectile.oldRot[n];
            DrawStarryTail(Projectile, Color.Blue * a * a * .2f, Color.White * a);
            //Main.instance.DrawProjWithStarryTrail(Projectile, Color.White, 0);
        }
        Projectile.type = origType;
        Projectile.rotation = origRotation;
        Projectile.Center = origCen;
        return false;
    }
    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        SoundEngine.PlaySound(SoundID.Item10);
        for (int n = 0; n < 30; n++)
            Gore.NewGorePerfect(Projectile.Center + Projectile.velocity, Main.rand.NextVector2Circular(4, 4), Main.rand.Next(16, 18));
        for (int n = 0; n < 30; n++)
            Gore.NewGorePerfect(Projectile.Center + Projectile.velocity, Main.rand.NextVector2Circular(2, 2) - oldVelocity * Main.rand.NextFloat() * .5f, Main.rand.Next(16, 18));
        return base.OnTileCollide(oldVelocity);
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        Projectile.SummonSuperStarSlash(target.Center);
        base.OnHitNPC(target, hit, damageDone);
    }
}

public class StarlightZone : ModProjectile
{
    public override string Texture => $"Terraria/Images/Item_{ItemID.FallenStar}";

    public override void AI()
    {
        var owner = Main.player[Projectile.owner];
        Projectile.Center = owner.Center - Vector2.UnitY * 540;
        int s = Projectile.ai[0] switch
        {
            0 => 30,
            1 => 45,
            2 => 10
        };
        if (Projectile.owner == Main.myPlayer && Projectile.ai[1] % s == 0)
        {

            int num11 = 3;

            for (int k = 0; k < num11; k++)
            {
                var pointPoisition = new Vector2(owner.Center.X + (float)(Main.rand.Next(201) * -owner.direction) + (Main.MouseWorld.X - owner.position.X), owner.MountedCenter.Y - 600f);
                pointPoisition.X = (pointPoisition.X * 10f + owner.Center.X) / 11f + (float)Main.rand.Next(-100, 101);
                pointPoisition.Y -= 150 * (k - 1);
                Vector2 targetVec = Main.MouseWorld - pointPoisition;
                targetVec.Y = MathF.Abs(targetVec.Y);
                targetVec.Y = MathF.Max(targetVec.Y, 20);

                targetVec = targetVec.SafeNormalize(default) * 16 + new Vector2(Main.rand.Next(-40, 41), Main.rand.Next(-40, 41)) * .03f;
                targetVec.X *= (float)Main.rand.Next(75, 150) * 0.01f;
                targetVec *= 1 + Projectile.ai[0] * .5f;
                pointPoisition.X += Main.rand.Next(-50, 51);
                int num13 = Projectile.NewProjectile(Projectile.GetProjectileSource_FromThis(), pointPoisition, targetVec, Projectile.ai[0] == 1 ? NaturalFallingStar.ID() : ZoneFallingStar.ID(), Projectile.damage, 10f, Projectile.owner);
                Main.projectile[num13].noDropItem = true;
            }

        }
        if (Projectile.ai[0] == 1)
        {
            if (owner.channel)
            {
                owner.itemTime = 2;
                owner.itemAnimation = 2;

                Projectile.timeLeft = 2;
            }
        }
        Projectile.ai[1]++;
        base.AI();
    }

    public override bool PreDraw(ref Color lightColor)
    {
        return false;
    }

    public override void SetDefaults()
    {
        Projectile.timeLeft = 180;
        Projectile.friendly = Projectile.hostile = false;
        Projectile.width = Projectile.height = 1;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;

    }
}
public class ZoneFallingStar : ModProjectile
{
    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.FallingStar}";

    public override bool PreDraw(ref Color lightColor)
    {
        Vector2 vec = Projectile.Center;
        Vector3 hsl = Main.rgbToHsl(MainColor);
        for (int n = 9; n >= 0; n--)
        {
            Projectile.Center = Projectile.oldPos[n];
            float alpha = n == 0 ? 1 : (1 - n * .1f) * .5f;

            ShootingStar.DrawStarryTail(
                Projectile,
                Main.hslToRgb(hsl + new Vector3(n * .03f, 0, 0)) with { A = 0 } * .2f * alpha,
                Color.White with { A = 0 } * .8f * alpha);

        }
        Projectile.Center = vec;


        float fac = Utils.GetLerpValue(0, 15, Projectile.ai[2], true);
        var yScaler = fac * .35f;
        var xScaler = fac * fac * 4;
        vec = origCenter - Main.screenPosition;
        var rotation = Projectile.velocity.ToRotation();
        Main.EntitySpriteDraw(ModAsset.Extra_197_Modified2.Value, vec, null, MainColor with { A = 0 } * .5f * fac * .25f, rotation, new Vector2(0, 128), new Vector2(xScaler, yScaler * 0.5f), 0, 0);
        Main.EntitySpriteDraw(ModAsset.Extra_197_Modified2.Value, vec, null, Color.White with { A = 0 } * fac * .25f, rotation, new Vector2(0, 128), new Vector2(xScaler, yScaler * 0.25f), 0, 0);

        return base.PreDraw(ref lightColor);
    }
    protected virtual Color MainColor => Color.Blue;
    public override void SetDefaults()
    {
        var projectile = Projectile;
        projectile.width = 18;
        projectile.height = 18;
        projectile.aiStyle = 5;
        projectile.friendly = true;
        projectile.penetrate = -1;
        projectile.alpha = 50;
        projectile.light = 1f;
        projectile.ignoreWater = true;
        base.SetDefaults();
    }
    public Vector2 origCenter;
    public override void AI()
    {
        Projectile.ai[2]++;
        float factor = Projectile.ai[2] / 15f;
        if (origCenter == default)
            origCenter = Projectile.Center;
        Projectile.Center = origCenter + Projectile.velocity * factor * (factor - 1) * 16;

        Projectile.tileCollide = Projectile.ai[2] > 45;
        var projectile = Projectile;
        if (projectile.ai[1] == 0f && !Collision.SolidCollision(projectile.position, projectile.width, projectile.height))
        {
            projectile.ai[1] = 1f;
            projectile.netUpdate = true;
        }

        if (projectile.ai[1] != 0f)
            projectile.tileCollide = true;

        if (projectile.soundDelay == 0)
        {
            projectile.soundDelay = 20 + Main.rand.Next(40);
            SoundEngine.PlaySound(SoundID.Item9, projectile.position);
        }

        if (projectile.localAI[0] == 0f)
            projectile.localAI[0] = 1f;

        projectile.alpha += (int)(25f * projectile.localAI[0]);
        if (projectile.alpha > 200)
        {
            projectile.alpha = 200;
            projectile.localAI[0] = -1f;
        }

        if (projectile.alpha < 0)
        {
            projectile.alpha = 0;
            projectile.localAI[0] = 1f;
        }

        projectile.rotation += (Math.Abs(projectile.velocity.X) + Math.Abs(projectile.velocity.Y)) * 0.01f * (float)projectile.direction;


        Vector2 vector10 = new Vector2(Main.screenWidth, Main.screenHeight);
        if (projectile.Hitbox.Intersects(Utils.CenteredRectangle(Main.screenPosition + vector10 / 2f, vector10 + new Vector2(400f))) && Main.rand.NextBool(6))
        {
            int num87 = Utils.SelectRandom<int>(Main.rand, 16, 17, 17, 17);
            if (Main.tenthAnniversaryWorld)
                num87 = Utils.SelectRandom<int>(Main.rand, 16, 16, 16, 17);

            Gore.NewGore(projectile.position, projectile.velocity * 0.2f, num87);
        }

        projectile.light = 0.9f;
        if (Main.rand.NextBool(20) || (Main.tenthAnniversaryWorld && Main.rand.NextBool(15)))
            Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.Enchanted_Pink, projectile.velocity.X * 0.5f, projectile.velocity.Y * 0.5f, 150, default(Color), 1.2f);


        for (int n = 9; n > 0; n--)
            projectile.oldPos[n] = projectile.oldPos[n - 1];
        projectile.oldPos[0] = projectile.Center;
        base.AI();
    }
    public override bool ShouldUpdatePosition() => false;
    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        SoundEngine.PlaySound(SoundID.Item10);
        for (int n = 0; n < 10; n++)
            Gore.NewGorePerfect(Projectile.Center + Projectile.velocity, Main.rand.NextVector2Circular(4, 4), Main.rand.Next(16, 18));
        for (int n = 0; n < 10; n++)
            Gore.NewGorePerfect(Projectile.Center + Projectile.velocity, Main.rand.NextVector2Circular(2, 2) - oldVelocity * Main.rand.NextFloat() * .5f, Main.rand.Next(16, 18));
        return base.OnTileCollide(oldVelocity);
    }
}
public class NaturalFallingStar : ZoneFallingStar
{
    protected override Color MainColor => Color.LimeGreen;
    public override void OnKill(int timeLeft)
    {
        for (int n = 0; n < 4; n++)
            Projectile.NewProjectileDirect(Projectile.GetProjectileSource_FromThis(), Projectile.Center, Main.rand.NextVector2Unit() * Main.rand.NextFloat(4),
                Main.rand.Next(569, 572), Projectile.damage / 3, Projectile.knockBack, Projectile.owner).hostile = true;
        base.OnKill(timeLeft);
    }
    public override Color? GetAlpha(Color lightColor)
    {
        lightColor = Color.Green;
        return base.GetAlpha(lightColor);
    }
}
public class StarlightBlessing : ModItem
{
    public override string Texture => $"Terraria/Images/Item_{ItemID.FallenStar}";
    public override Color? GetAlpha(Color lightColor)
    {
        return Main.DiscoColor with { A = 0 };
    }
    public override void SetStaticDefaults()
    {
        Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 8));
        ItemID.Sets.ItemIconPulse[Item.type] = true;
        Item.ResearchUnlockCount = 1;
        base.SetStaticDefaults();
    }
    public override void SetDefaults()
    {

        Item.rare = ItemRarityID.LightRed;
        Item.accessory = true;
        Item.value = Item.sellPrice(0, 10);
        Item.width = Item.height = 22;

        Item.useTime = Item.useAnimation = 15;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.shoot = StarlightZone.ID();
        Item.shootSpeed = 1f;
        base.SetDefaults();
    }
    int timer;
    public override void UpdateInventory(Player player)
    {
        timer--;
        base.UpdateInventory(player);
    }
    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (timer > 0)
            Main.NewText(Language.GetTextValue("Mods.LogSpiralsAbstractWeapons.Items.StarlightBlessing.NotNowHint"));
        else
        {
            Projectile.NewProjectile(source, player.Center, default, type, 60, 0, player.whoAmI, 0);
            timer = 600;
        }
        return false;
    }
    public override void HoldItem(Player player)
    {
        EZBlessingSystem.SetCurrentBoost(15);

        player.GetModPlayer<EZBlessingPlayer>().HasFaintStarlight = true;
        base.HoldItem(player);
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        EZBlessingSystem.SetCurrentBoost(15);

        player.GetModPlayer<EZBlessingPlayer>().HasFaintStarlight = true;
        base.UpdateAccessory(player, hideVisual);
    }
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<StarSlingshot>()
            .AddIngredient(ItemID.SoulofLight, 10)
            .AddIngredient(ItemID.SoulofNight, 10)
            .AddTile(TileID.MythrilAnvil)
            .Register();
        base.AddRecipes();
    }
}


public class NaturalStarlight : ModItem
{
    public override string Texture => $"Terraria/Images/Item_{ItemID.FallenStar}";
    public override Color? GetAlpha(Color lightColor)
    {
        return Color.Green with { A = 0 };
    }
    public override void SetStaticDefaults()
    {
        Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 8));
        ItemID.Sets.ItemIconPulse[Item.type] = true;
        Item.ResearchUnlockCount = 1;
        base.SetStaticDefaults();
    }
    public override void SetDefaults()
    {

        Item.rare = ItemRarityID.Yellow;
        Item.accessory = true;
        Item.value = Item.sellPrice(0, 12);
        Item.width = Item.height = 22;
        Item.channel = true;
        Item.useTime = Item.useAnimation = 15;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.shoot = StarlightZone.ID();
        Item.shootSpeed = 1f;
        base.SetDefaults();
    }
    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        Projectile.NewProjectile(source, player.Center, default, type, 100, 0, player.whoAmI, 1);
        return false;
    }
    public override void HoldItem(Player player)
    {
        EZBlessingSystem.SetCurrentBoost(20);

        player.GetModPlayer<EZBlessingPlayer>().HasFaintStarlight = true;
        base.HoldItem(player);
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        EZBlessingSystem.SetCurrentBoost(20);

        player.GetModPlayer<EZBlessingPlayer>().HasFaintStarlight = true;
        base.UpdateAccessory(player, hideVisual);
    }
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<StarlightBlessing>()
            .AddIngredient(ItemID.ChlorophyteBar, 10)
            .AddTile(TileID.MythrilAnvil)
            .Register();
        base.AddRecipes();
    }
}

public class TrueStarlightBlessing : ModItem
{
    public override string Texture => $"Terraria/Images/Item_{ItemID.FallenStar}";
    public override Color? GetAlpha(Color lightColor)
    {
        return Color.White with { A = 0 };
    }
    public override void SetStaticDefaults()
    {
        Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 8));
        ItemID.Sets.ItemIconPulse[Item.type] = true;
        Item.ResearchUnlockCount = 1;
        base.SetStaticDefaults();
    }
    public override void SetDefaults()
    {

        Item.rare = ItemRarityID.Red;
        Item.accessory = true;
        Item.value = Item.sellPrice(0, 50);
        Item.width = Item.height = 22;
        Item.useTime = Item.useAnimation = 15;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.shoot = StarlightZone.ID();
        Item.shootSpeed = 1f;
        base.SetDefaults();
    }
    int timer;
    public override void UpdateInventory(Player player)
    {
        timer--;
        base.UpdateInventory(player);
    }
    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (timer > 0)
            Main.NewText(Language.GetTextValue("Mods.LogSpiralsAbstractWeapons.Items.StarlightBlessing.NotNowHint"));
        else
        {
            Projectile.NewProjectile(source, player.Center, default, type, 150, 0, player.whoAmI, 2);
            timer = 300;

        }
        return false;
    }
    public override void HoldItem(Player player)
    {
        EZBlessingSystem.SetCurrentBoost(30);
        player.GetModPlayer<EZBlessingPlayer>().HasFaintStarlight = true;
        base.HoldItem(player);
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        EZBlessingSystem.SetCurrentBoost(30);
        player.GetModPlayer<EZBlessingPlayer>().HasFaintStarlight = true;
        base.UpdateAccessory(player, hideVisual);
    }
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<NaturalStarlight>()
            .AddIngredient(ItemID.FragmentSolar, 2)
            .AddIngredient(ItemID.FragmentNebula, 2)
            .AddIngredient(ItemID.FragmentStardust, 2)
            .AddIngredient(ItemID.FragmentVortex, 2)
            .AddTile(TileID.LunarCraftingStation)
            .Register();
        base.AddRecipes();
    }
}

public class EasyBlessing : ModItem
{
    public override string Texture => $"Terraria/Images/Item_{ItemID.FallenStar}";
    public override Color? GetAlpha(Color lightColor)
    {
        return Color.Gray with { A = 0 };
    }
    public override void SetStaticDefaults()
    {
        Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 8));
        ItemID.Sets.ItemIconPulse[Item.type] = true;
        Item.ResearchUnlockCount = 1;
        base.SetStaticDefaults();
    }
    public override void SetDefaults()
    {

        Item.rare = ItemRarityID.Purple;
        Item.accessory = true;
        Item.value = Item.sellPrice(591, 60, 15, 3);
        Item.width = Item.height = 22;
        base.SetDefaults();
    }
    public override void HoldItem(Player player)
    {
        EZBlessingSystem.SetCurrentBoost(50);

        player.GetModPlayer<EZBlessingPlayer>().HasFaintStarlight = true;
        EZBlessingSystem.HasEZBlessing = true;
        _isHolding = true;
        base.HoldItem(player);
    }
    bool _isHolding;

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        EZBlessingSystem.SetCurrentBoost(50);

        player.GetModPlayer<EZBlessingPlayer>().HasFaintStarlight = true;
        EZBlessingSystem.HasEZBlessing = true;
        base.UpdateAccessory(player, hideVisual);
    }
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<TrueStarlightBlessing>()
            .AddIngredient(ItemID.FallenStar, 300)
            .AddIngredient(ItemID.PlatinumCoin, 591)
            .AddIngredient(ItemID.GoldCoin, 60)
            .AddIngredient(ItemID.SilverCoin, 15)
            .AddIngredient(ItemID.CopperCoin, 3)
            .AddTile(TileID.LunarCraftingStation)
            .Register();
        base.AddRecipes();
    }
    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        if (_isHolding && !Main.dayTime)
            tooltips.Add(new TooltipLine(Mod, "ExtraHint", this.GetLocalizedValue("ExtraHint")));
        base.ModifyTooltips(tooltips);
    }
    public override void UpdateInventory(Player player)
    {
        _isHolding = false;
        base.UpdateInventory(player);
    }
}