using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.GameContent;

namespace LogSpiralsAbstractWeapons.Contents.SausageSword;

public class SausageSword : ModItem
{
    // 来自世纪小花，赞美花花
    public static void ConductBetterItemLocation(Player player)
    {
        float xoffset = 6;
        float yoffset = -10;
        if (player.itemAnimation < player.itemAnimationMax * .333)
            yoffset = 4f;
        else if (player.itemAnimation >= player.itemAnimationMax * .666)
            xoffset = -4f;
        player.itemLocation.X = player.Center.X + xoffset * player.direction;
        player.itemLocation.Y = player.MountedCenter.Y + yoffset;
        if (player.gravDir < 0)
            player.itemLocation.Y = player.Center.Y * 2 - player.itemLocation.Y;
    }
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
    }
    public override void UseStyle(Player player, Rectangle heldItemFrame) => ConductBetterItemLocation(player);
    public override void SetDefaults()
    {
        Item.damage = 30;
        Item.knockBack = 1;
        Item.DamageType = DamageClass.Melee;
        Item.useTime = Item.useAnimation = 15;
        Item.width = 252;
        Item.height = 239;
        Item.value = Item.sellPrice(114, 514);
        Item.useStyle = ItemUseStyleID.Swing;
        Item.UseSound = SoundID.Item2;
        Item.rare = ItemRarityID.Orange;
        base.SetDefaults();
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.GrilledSquirrel)
            .AddIngredient(ItemID.ChickenNugget, 4)
            .AddIngredient(ItemID.Bacon, 2)
            .AddIngredient(ItemID.Hotdog)
            .AddIngredient(ItemID.BBQRibs)
            .AddIngredient(ItemID.RoastedDuck)
            .AddIngredient(ItemID.MonsterLasagna)
            .AddTile(TileID.CookingPots)
            .Register();

        base.AddRecipes();
    }

    public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
    {
        player.AddBuff(Main.rand.Next(10) switch
        {
            < 2 => BuffID.WellFed3,
            < 5 => BuffID.WellFed2,
            _ => BuffID.WellFed
        }, Main.rand.Next(114));
        base.OnHitNPC(player, target, hit, damageDone);
    }
}

public class SausageSwordUltra : ModItem
{
    public override void UseStyle(Player player, Rectangle heldItemFrame) => SausageSword.ConductBetterItemLocation(player);
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults()
    {
        Item.damage = 80;
        Item.knockBack = 1;
        Item.DamageType = DamageClass.Melee;
        Item.useTime = Item.useAnimation = 15;
        Item.width = 252;
        Item.height = 239;
        Item.value = Item.sellPrice(1145, 14);
        Item.useStyle = ItemUseStyleID.Swing;
        Item.UseSound = SoundID.Item2;
        Item.rare = ItemRarityID.Yellow;
        base.SetDefaults();
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<SausageSword>()
            .AddIngredient(ItemID.ChlorophyteBar, 20)
            .AddTile(TileID.CookingPots)
            .Register();

        base.AddRecipes();
    }

    public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
    {
        player.AddBuff(Main.rand.Next(10) switch
        {
            < 2 => BuffID.WellFed,
            < 5 => BuffID.WellFed2,
            _ => BuffID.WellFed3
        }, Main.rand.Next(114514));
        base.OnHitNPC(player, target, hit, damageDone);
    }
}