using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics;
using LogSpiralsAbstractWeapons.Tools;
using Terraria.ModLoader.IO;
using System;
namespace LogSpiralsAbstractWeapons.Contents.NPCWhip;
public class NPCWhip : ModItem
{
    public override void SetDefaults()
    {
        // This method quickly sets the whip's properties.
        // Mouse over to see its parameters.
        Item.DefaultToWhip(ModContent.ProjectileType<NPCWhipProj>(), 20, 2, 4);
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
            .AddIngredient(ItemID.GuideVoodooDoll)
            .AddIngredient(ItemID.ClothierVoodooDoll)
            .AddIngredient(ItemID.TruffleWorm)
            .AddTile(TileID.LunarCraftingStation)
            .Register();

    }

    // Makes the whip receive melee prefixes
    public override bool MeleePrefix()
    {
        return true;
    }
}
public class NPCWhipProj : ModProjectile
{
    public override string Texture => base.Texture.Replace("Proj", "");
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.IsAWhip[Type] = true;
    }
    List<int> townNpcs = [];
    public override void SetDefaults()
    {
        // This method quickly sets the whip's properties.
        Projectile.DefaultToWhip();

        // use these to change from the vanilla defaults
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 0;
        Projectile.DamageType = DamageClass.Melee;
        foreach (var npc in Main.npc)
        {
            if (npc.townNPC && npc.active && npc.housingCategory != 1)
                townNpcs.Add(npc.type);
        }
        Projectile.WhipSettings.Segments = townNpcs.Count;
        Projectile.WhipSettings.RangeMultiplier = townNpcs.Count / 5f;
        Projectile.hostile = true;
        if (townNpcs.Count == 0)
        {
            Projectile.Kill();
            Main.NewText("[c/FF0000:在那之后一个人也没有了吗]");
        }
    }
    public override bool CanHitPlayer(Player target) => false;

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        modifiers.FinalDamage.Flat += townNpcs.Count * 5;
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
        //for (int n = 0; n < 10; n++)
        //{
        //    float factor = (Main.GlobalTimeWrappedHourly - n * 0.025f) % 0.5f * 2f;
        //    Main.spriteBatch.Draw(
        //        TextureAssets.Item[ItemID.TerraBlade].Value,
        //        new Vector2(600),
        //        null,
        //        Color.White * (1 - n * .1f),
        //        MathHelper.PiOver4,
        //        MathF.Pow(MathHelper.SmoothStep(0, 1f, factor), 2) * -MathHelper.TwoPi,
        //        MathHelper.PiOver4,
        //        new Vector2(0, 54),
        //        new Vector2(1, 2),
        //        Main.GlobalTimeWrappedHourly % 10 > 5);
        //}


        List<Vector2> list = [];
        Projectile.FillWhipControlPoints(Projectile, list);
        DrawLine(list);


        SpriteEffects flip = 0;

        Vector2 pos = list[0];

        for (int i = 0; i < list.Count - 1; i++)
        {
            var texture = TextureAssets.Npc[townNpcs[i]].Value;

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

            Main.spriteBatch.Draw(texture, pos - Main.screenPosition, new(0, 0, 40, 56), color, rotation + MathHelper.PiOver4 * 3, new Vector2(20, 56), scale, flip, 0);

            pos += diff;
        }

        return false;
    }
}
