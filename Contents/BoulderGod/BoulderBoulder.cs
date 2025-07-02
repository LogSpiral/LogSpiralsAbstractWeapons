using Terraria.Audio;

namespace LogSpiralsAbstractWeapons.Contents.BoulderGod;

public class BoulderBoulder : ModItem
{
    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.Boulder}";

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
        ItemID.Sets.SortingPriorityBossSpawns[Type] = 12;
    }

    public override void SetDefaults()
    {
        Item.rare = ItemRarityID.Purple;
        Item.width = 20;
        Item.height = 20;
        Item.maxStack = 1;
        base.SetDefaults();
    }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
    {
        itemGroup = ContentSamples.CreativeHelper.ItemGroup.BossSpawners;
    }


    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Boulder, 10)
            .AddTile(TileID.DemonAltar)
            /*.Register()*/;
        base.AddRecipes();
    }
}