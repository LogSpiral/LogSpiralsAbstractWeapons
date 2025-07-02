using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics;
namespace LogSpiralsAbstractWeapons.Contents.DestroyerWhip;
public class DestroyerWhip : ModItem
{
    public override void SetDefaults()
    {
        // This method quickly sets the whip's properties.
        // Mouse over to see its parameters.
        Item.DefaultToWhip(ModContent.ProjectileType<DestroyerWhipProj>(), 20, 2, 4);
        Item.rare = ItemRarityID.Purple;
        Item.channel = true;
        Item.damage = 40;
        Item.DamageType = DamageClass.Melee;
        Item.useTime = Item.useAnimation = 120;
    }


    // Makes the whip receive melee prefixes
    public override bool MeleePrefix()
    {
        return true;
    }
}
public class DestroyerWhipLoot : GlobalNPC
{
    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        if (npc.type is NPCID.SkeletronPrime or NPCID.TheDestroyer or NPCID.Retinazer or NPCID.Spazmatism)
            npcLoot.Add(ItemDropRule.ByCondition(new Conditions.MechdusaKill(), ModContent.ItemType<DestroyerWhip>()));
        base.ModifyNPCLoot(npc, npcLoot);
    }
}
public class DestroyerWhipProj : ModProjectile
{
    public override string Texture => base.Texture.Replace("Proj", "");
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.IsAWhip[Type] = true;
    }
    public override void SetDefaults()
    {
        // This method quickly sets the whip's properties.
        Projectile.DefaultToWhip();

        // use these to change from the vanilla defaults
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 5;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.WhipSettings.Segments = 62;
        Projectile.WhipSettings.RangeMultiplier = 2.5f;
        Projectile.ownerHitCheck = false;
    }
    public override bool CanHitPlayer(Player target) => false;

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
        if (Main.rand.NextBool((int)Projectile.ai[2] + 1)) 
        {
            Projectile.ai[2]++;
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Main.rand.NextVector2Unit() * 16, ModContent.ProjectileType<ProbeLaser>(),
                Projectile.damage, Projectile.knockBack, Projectile.owner, target.whoAmI);
        }

    }

    // This method draws a line between all points of the whip, in case there's empty space between the sprites.
    private static void DrawLine(List<Vector2> list)
    {
        Texture2D texture = TextureAssets.FishingLine.Value;
        Rectangle frame = texture.Frame();
        Vector2 origin = new Vector2(frame.Width / 2, 2);

        Vector2 pos = list[0];
        for (int i = 0; i < list.Count - 1; i++)
        {
            Vector2 element = list[i];
            Vector2 diff = list[i + 1] - element;

            float rotation = diff.ToRotation() - MathHelper.PiOver2;
            Color color = Lighting.GetColor(element.ToTileCoordinates(), Color.White);
            Vector2 scale = new Vector2(1, (diff.Length() + 2) / frame.Height);

            Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, SpriteEffects.None, 0);

            pos += diff;
        }
    }
    private float RealTimer
    {
        get => Projectile.ai[1];
        set => Projectile.ai[1] = value;
    }
    public override void PostAI()
    {
        RealTimer++;
        Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out int _, out float _);
        float factor = RealTimer / timeToFlyOut;
        Projectile.ai[0] = MathF.Pow(factor, 4) * timeToFlyOut * .8f + .2f * factor * timeToFlyOut;

        if (MathF.Abs(Projectile.ai[0] - timeToFlyOut / 2f) < 4f)
        {
            SoundEngine.PlaySound(SoundID.Item153, Projectile.Center);
        }
        base.PostAI();
    }
    public override bool PreDraw(ref Color lightColor)
    {

        List<Vector2> list = [];
        Projectile.FillWhipControlPoints(Projectile, list);
        DrawLine(list);


        SpriteEffects flip = 0;

        Vector2 pos = list[0];

        for (int i = 0; i < list.Count - 1; i++)
        {
            bool isBody = false;
            Texture2D texture;
            if (i == 0)
                texture = TextureAssets.Npc[NPCID.TheDestroyerTail].Value;
            else if (i == list.Count - 2)
                texture = TextureAssets.Npc[NPCID.TheDestroyer].Value;
            else
            {
                texture = TextureAssets.Npc[NPCID.TheDestroyerBody].Value;
                isBody = true;
            }

            float scale = 1;
            if (i == list.Count - 2)
            {
                // For a more impactful look, this scales the tip of the whip up when fully extended, and down when curled up.
                Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out int _, out float _);
                float t = Projectile.ai[0] / timeToFlyOut;
                scale = MathHelper.Lerp(0.5f, 1.5f, Utils.GetLerpValue(0.1f, 0.7f, t, true) * Utils.GetLerpValue(0.9f, 0.7f, t, true));
            }

            Vector2 element = list[i];
            Vector2 diff = list[i + 1] - element;

            float rotation = diff.ToRotation() - MathHelper.PiOver4; // This projectile's sprite faces down, so PiOver2 is used to correct rotation.
            Color color = Lighting.GetColor(element.ToTileCoordinates());
            if (isBody)
            {
                Main.EntitySpriteDraw(texture, pos - Main.screenPosition, new(0, 102, 50, 102), color, rotation + MathHelper.PiOver4 * 3,
    new Vector2(20, 56), scale, flip, 0);

                Main.EntitySpriteDraw(texture, pos - Main.screenPosition, new(0, 0, 50, 102), color * (MathF.Cos(Main.GlobalTimeWrappedHourly * 15 - i * .5f) * .5f + .5f), rotation + MathHelper.PiOver4 * 3,
    new Vector2(20, 56), scale, flip, 0);
            }
            else
            {
                Main.EntitySpriteDraw(texture, pos - Main.screenPosition, new(0, 0, 50, 102), color, rotation + MathHelper.PiOver4 * 3,
                    new Vector2(20, 56), scale, flip, 0);

            }

            pos += diff;
        }
        return false;
    }
}

public class ProbeLaser : ModProjectile
{
    public override string Texture => $"Terraria/Images/NPC_{NPCID.Probe}";
    public override void SetDefaults()
    {
        Projectile.timeLeft = 60;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.width = Projectile.height = 1;
        Projectile.tileCollide = false;
        Projectile.aiStyle = -1;
        Projectile.ignoreWater = true;
        Projectile.friendly = false;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 5;
        base.SetDefaults();
    }
    NPC Target
    {
        get => Main.npc[(int)Projectile.ai[0]];
        set => Projectile.ai[0] = value.whoAmI;
    }
    public override void AI()
    {
        if (Projectile.timeLeft < 20) 
        {
            Projectile.velocity = default;
            Projectile.friendly = true;
            return;
        }
        if (!Target.active || Target.friendly || Vector2.Distance(Target.Center, Projectile.Center) > 512)
        {
            foreach (var npc in Main.npc)
            {
                if (!npc.friendly && npc.CanBeChasedBy() && Vector2.Distance(npc.Center, Projectile.Center) < 432)
                {
                    Target = npc;
                    break;
                }
            }
        }
        else
        {
            Vector2 unit = Target.Center - Projectile.Center;
            Projectile.rotation = unit.ToRotation();
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, -unit.SafeNormalize(default) * Utils.GetLerpValue(25, 40, Projectile.timeLeft, true) * 4, .25f);
        }
        base.AI();
    }
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        float facL = 1 - Projectile.timeLeft / 20f;
        var xScaler = 5 * MathHelper.Clamp(facL * 2, 0, 1);
        float point = 0f;
        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), 
            Projectile.Center, Projectile.Center + Projectile.rotation.ToRotationVector2() * xScaler * 256, 32, ref point);
    }
    public override bool PreDraw(ref Color lightColor)
    {
        var rotation = Projectile.rotation;
        var flag = MathF.Abs(rotation) < MathF.PI / 2;
        var t = Projectile.timeLeft;
        Main.EntitySpriteDraw(TextureAssets.Npc[NPCID.Probe].Value, Projectile.Center - Main.screenPosition, null, lightColor * Utils.GetLerpValue(30, 20, MathF.Abs(t - 30)), flag ? rotation : MathHelper.Pi + rotation, new Vector2(16), 1, flag ? SpriteEffects.FlipHorizontally : SpriteEffects.None);

        if (t < 40)
        {
            var unit = rotation.ToRotationVector2();
            Vector2 center = Projectile.Center - Main.screenPosition + unit * 12;
            if (t < 20)
            {
                float facL = 1 - Projectile.timeLeft / 20f;
                var xScaler = 5 * MathHelper.Clamp(facL * 2, 0, 1);
                var yScaler = MathF.Sin(MathF.PI * MathF.Sqrt(facL));
                Main.EntitySpriteDraw(ModAsset.Extra_197_Modified.Value, center, null, Color.Red with { A = 0 } * .5f, rotation, new Vector2(0, 128), new Vector2(xScaler, yScaler * 0.5f), 0, 0);
                Main.EntitySpriteDraw(ModAsset.Extra_197_Modified.Value, center, null, Color.White with { A = 0 }, rotation, new Vector2(0, 128), new Vector2(xScaler, yScaler * 0.25f), 0, 0);
            }
            float fac1 = Utils.GetLerpValue(40, 30, t, true);

            float fac2 = Utils.GetLerpValue(30, 20, t, true);
            float scaler = fac1 + fac2 * .25f;
            Main.EntitySpriteDraw(TextureAssets.Extra[98].Value, center, null, Color.Red with { A = 0 } * fac1, rotation, new Vector2(36), scaler, 0, 0);
            Main.EntitySpriteDraw(TextureAssets.Extra[98].Value, center, null, Color.White with { A = 0 } * fac1, rotation, new Vector2(36), scaler * .75f, 0, 0);


            Main.EntitySpriteDraw(TextureAssets.Extra[174].Value, center, null, Color.Pink with { A = 0 } * fac2, rotation, new Vector2(64), (1 - fac2) * 2, 0, 0);
        }
        return false;
    }
}
