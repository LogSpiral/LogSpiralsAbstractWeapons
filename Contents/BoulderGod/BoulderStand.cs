using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;
using static LogSpiralsAbstractWeapons.Contents.BoulderGod.BoulderGodHelper;
namespace LogSpiralsAbstractWeapons.Contents.BoulderGod;
public class BoulderStand : ModItem
{
    public override void SetStaticDefaults()
    {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<BoulderStandTile>());
        Item.width = 30;
        Item.height = 12;
        Item.maxStack = 99;
        Item.rare = ItemRarityID.Purple;
    }
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.StoneBlock, 500)
            .AddIngredient(ItemID.Boulder, 10)
            .AddIngredient(ItemID.DD2ElderCrystalStand)
            .AddTile(TileID.DemonAltar)
            /*.Register()*/;

        base.AddRecipes();
    }
}
public class BoulderStandTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileLavaDeath[Type] = false;
        TileID.Sets.FramesOnKillWall[Type] = true;
        TileID.Sets.HasOutlines[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style5x4);
        TileObjectData.newTile.Width = 5;
        TileObjectData.newTile.Height = 2;
        TileObjectData.newTile.Origin = new Point16(2, 1);
        TileObjectData.newTile.CoordinateHeights = new int[2] { 16, 16 };
        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(28, 28, 28), Language.GetText("Mods.LogSpiralsAbstractWeapons.Items.BoulderStand.DisplayName"));
        DustType = DustID.Stone;
    }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;
    public override bool RightClick(int i, int j)
    {
        if (NPC.AnyNPCs(ModContent.NPCType<BoulderGod>()))
        {
            BoulderMessage("FightingHint");
            return true;
        }
        if (Main.LocalPlayer.HeldItem.type == ModContent.ItemType<BoulderBoulder>())
        {
            HandleBoulderGodSpawn(i, j);
        }
        else
        {
            BoulderMessage("ItemHint");
        }
        return true;
    }
    public static void HandleBoulderGodSpawn(int i, int j)
    {
        if (hintTimer > 0)
        {
            goto label;
        }
        var tile = Main.tile[i, j];
        i -= tile.TileFrameX / 16 - 2;
        j -= tile.TileFrameY / 16 - 2;

        #region 物块检测

        #region 边界检测
        if (j < 150)
        {
            BoulderMessage("YLimit");
            return;
        }
        else if (j < 124 + Main.maxTilesY / 20)
        {
            BoulderMessage("SpaceHint");
            hintTimer = 300;
            return;
        }
        if (i < 150 || i > Main.maxTilesX - 150)
        {
            BoulderMessage("XLimit");
            return;
        }
        #endregion


        int tileCount = 0;
        for (int x = -100, y = 0; x < 101 && y < 124; x++, y++)
        {
            var currentTile = Main.tile[i + x, j - 1 - y];
            if (currentTile.HasTile && Main.tileSolid[currentTile.type])
            {
                tileCount++;
                if (y < 6)
                {
                    BoulderMessage("CleanBrenaHint");
                    return;
                }
            }
        }
        if (tileCount > 201 * 124 / 4)
        {
            BoulderMessage("TooManyTilesHint");
            return;
        }
        else if (tileCount > 0)
        {
            BoulderMessage("SomeTilesHint");
            hintTimer = 300;
            return;
        }

        #endregion


        label:
        /*for (int x = -101; x <= 101; x++)
        {
            var ctile = Main.tile[i + x, j];
            if (!ctile.HasTile)
            {
                ctile.HasTile = true;
                ctile.type = (ushort)ModContent.TileType<BrenaTile>();
            }
            ctile = Main.tile[i + x, j-125];
            if (!ctile.HasTile)
            {
                ctile.HasTile = true;
                ctile.type = (ushort)ModContent.TileType<BrenaTile>();
            }
        }
        for (int y = 1; y < 125; y++) 
        {
            var ctile = Main.tile[i + 101, j - y];
            if (!ctile.HasTile)
            {
                ctile.HasTile = true;
                ctile.type = (ushort)ModContent.TileType<BrenaTile>();
            }
            ctile = Main.tile[i - 101, j - y];
            if (!ctile.HasTile)
            {
                ctile.HasTile = true;
                ctile.type = (ushort)ModContent.TileType<BrenaTile>();
            }
        }*/
        NPC.NewNPC(new EntitySource_TileInteraction(Main.LocalPlayer, i, j), i * 16 + 8, j * 16 + 8 - 96, ModContent.NPCType<BoulderGod>());
        SoundEngine.PlaySound(SoundID.Zombie104);

    }
    public override void MouseOver(int i, int j)
    {
        Player localPlayer = Main.LocalPlayer;
        localPlayer.noThrow = 2;
        localPlayer.cursorItemIconEnabled = true;
        localPlayer.cursorItemIconID = ItemID.Boulder;
        base.MouseOver(i, j);
    }
    public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
    {
        const int FrameWidth = 5 * 18;
        const int FrameHeight = 2 * 18;
        // Since this tile does not have the hovering part on its sheet, we have to animate it ourselves
        // Therefore we register the top-left of the tile as a "special point"
        // This allows us to draw things in SpecialDraw
        if (drawData.tileFrameX % FrameWidth == 0 && drawData.tileFrameY % FrameHeight == 0)
        {
            Main.instance.TilesRenderer.AddSpecialLegacyPoint(i, j);
        }
    }
    static bool BoulderGodExists;
    static int checkTimer;
    static int animationTimer;
    static int hintTimer;
    public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
    {
        hintTimer--;
        checkTimer++;
        if (checkTimer >= 60)
        {
            BoulderGodExists = NPC.AnyNPCs(ModContent.NPCType<BoulderGod>());
            checkTimer = 0;
        }
        //BoulderGodExists = true;
        animationTimer += BoulderGodExists ? 1 : -1;
        animationTimer = Math.Clamp(animationTimer, 0, 60);
        if (animationTimer == 0) return;
        Vector2 offScreen = new Vector2(Main.offScreenRange);
        if (Main.drawToScreen)
        {
            offScreen = Vector2.Zero;
        }
        const int FrameWidth = 5 * 18;
        const int FrameHeight = 2 * 18;
        // Take the tile, check if it actually exists
        Point p = new Point(i, j);
        Tile tile = Main.tile[p.X, p.Y];
        if (tile == null || !tile.HasTile)
        {
            return;
        }

        // Get the initial draw parameters
        Texture2D texture = TextureAssets.Item[ItemID.Boulder].Value;

        int frameY = tile.TileFrameX / FrameWidth; // Picks the frame on the sheet based on the placeStyle of the item
        Rectangle frame = texture.Frame(1, 1, 0, frameY);

        Vector2 origin = frame.Size() / 2f;
        Vector2 worldPos = p.ToWorldCoordinates(40f, 64f);

        Color color = Lighting.GetColor(p.X, p.Y) * (animationTimer / 60f);

        bool direction = tile.TileFrameY / FrameHeight != 0; // This is related to the alternate tile data we registered before
        SpriteEffects effects = direction ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

        // Some math magic to make it smoothly move up and down over time
        const float TwoPi = (float)Math.PI * 2f;
        float offset = (float)Math.Sin(Main.GlobalTimeWrappedHourly * TwoPi / 5f);
        Vector2 drawPos = worldPos + offScreen - Main.screenPosition + new Vector2(0f, -96f) + new Vector2(0f, offset * 4f);

        // Draw the main texture
        spriteBatch.Draw(texture, drawPos, frame, color, 0f, origin, 1.5f, effects, 0f);

        // Draw the periodic glow effect
        float scale = (float)Math.Sin(Main.GlobalTimeWrappedHourly * TwoPi / 2f) * 0.3f + 0.7f;
        Color effectColor = color;
        effectColor.A = 0;
        effectColor = effectColor * 0.1f * scale;
        for (float num5 = 0f; num5 < 1f; num5 += 355f / (678f * (float)Math.PI))
        {
            spriteBatch.Draw(texture, drawPos + (TwoPi * num5).ToRotationVector2() * (6f + offset * 2f), frame, effectColor, 0f, origin, 1.5f, effects, 0f);
        }
    }
}
