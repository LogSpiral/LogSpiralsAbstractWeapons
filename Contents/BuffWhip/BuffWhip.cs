using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics;
using LogSpiralsAbstractWeapons.Tools;
using System;
namespace LogSpiralsAbstractWeapons.Contents.BuffWhip;
public class BuffWhip : ModItem
{
    public override void SetDefaults()
    {
        // This method quickly sets the whip's properties.
        // Mouse over to see its parameters.
        Item.DefaultToWhip(ModContent.ProjectileType<BuffWhipProj>(), 20, 2, 4);
        Item.rare = ItemRarityID.Purple;
        Item.channel = true;
        Item.damage = 50;
        Item.DamageType = DamageClass.Melee;
        Item.useTime = Item.useAnimation = 30;
    }

    // Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.RedPotion)
            .AddTile(TileID.LunarCraftingStation)
            .Register();

    }

    // Makes the whip receive melee prefixes
    public override bool MeleePrefix()
    {
        return true;
    }
}
public class BuffWhipProj : ModProjectile
{
    public override string Texture => base.Texture.Replace("Proj","");
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.IsAWhip[Type] = true;
    }
    List<int> buffs = [];
    public override void SetDefaults()
    {
        // This method quickly sets the whip's properties.
        Projectile.DefaultToWhip();

        // use these to change from the vanilla defaults
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 0;
        Projectile.DamageType = DamageClass.Melee;
        for (int n = 0; n < BuffLoader.BuffCount; n++)
            if (Main.debuff[n])
                buffs.Add(n);
        Projectile.WhipSettings.Segments = buffs.Count;
        Projectile.WhipSettings.RangeMultiplier = buffs.Count / 5f;
        Projectile.hostile = true;
    }
    public override bool CanHitPlayer(Player target) => false;

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        foreach (var buff in buffs)
            if(Main.rand.NextBool(10))
            target.AddBuff(buff, Main.rand.Next(30, 300));

        base.ModifyHitNPC(target, ref modifiers);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
        //Projectile.damage = (int)(Projectile.damage * 0.75f); // Multihit penalty. Decrease the damage the more enemies the whip hits.
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

    public override bool PreDraw(ref Color lightColor)
    {

        List<Vector2> list = [];
        Projectile.FillWhipControlPoints(Projectile, list);
        DrawLine(list);


        SpriteEffects flip = 0;

        Vector2 pos = list[0];

        for (int i = 0; i < list.Count - 1; i++)
        {
            var texture = TextureAssets.Buff[buffs[i]].Value;
            
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

            float rotation = diff.ToRotation(); // This projectile's sprite faces down, so PiOver2 is used to correct rotation.
            Color color = Lighting.GetColor(element.ToTileCoordinates());

            float cValue = MathF.Cos(-i * .25f + Main.GlobalTimeWrappedHourly * 2);
            Main.spriteBatch.Draw(texture, pos - Main.screenPosition, null, color, Main.GlobalTimeWrappedHourly * 4, 0, rotation, new Vector2(16), new Vector2(1, MathF.Abs(cValue)) * scale, cValue < 0);
            //Main.EntitySpriteDraw(texture, pos - Main.screenPosition, null, color, rotation + MathHelper.PiOver4 * 2, new Vector2(16,16), scale, flip, 0);

            pos += diff;
        }
        return false;
    }
}
