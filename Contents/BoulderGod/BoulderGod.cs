using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;

namespace LogSpiralsAbstractWeapons.Contents.BoulderGod;

[AutoloadBossHead]
public class BoulderGod : ModNPC
{
    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.Boulder}";
    public override string BossHeadTexture => $"Terraria/Images/Projectile_{ProjectileID.Boulder}";

    public override void SetDefaults()
    {
        NPC.damage = 50;
        NPC.lifeMax = 3000;
        NPC.aiStyle = -1;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.friendly = false;
        NPC.width = NPC.height = 200;
        NPC.boss = true;
        NPC.knockBackResist = 0;
        base.SetDefaults();
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<BoulderGlove.BoulderGlove>()));
        base.ModifyNPCLoot(npcLoot);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        spriteBatch.Draw(TextureAssets.Npc[Type].Value, NPC.Center - Main.screenPosition, null, drawColor, NPC.rotation, new Vector2(16), 8, 0, 0);
        return false;
    }
    int Stage
    {
        get => (int)NPC.ai[0];
        set => NPC.ai[0] = value;
    }
    int Timer
    {
        get => (int)NPC.ai[1];
        set => NPC.ai[1] = value;
    }
    public override void AI()
    {
        switch (Stage)
        {
            case 0:
                {
                    Stage0_Platform();
                    break;
                }
        }
        base.AI();
    }
    public override void OnSpawn(IEntitySource source)
    {
        if (source is EntitySource_TileInteraction interaction)
        {
            NPC.ai[2] = interaction.TileCoords.X;
            NPC.ai[3] = interaction.TileCoords.Y;
        }
        base.OnSpawn(source);
    }
    void Stage0_Platform()
    {
        int t = ++Timer;
        int centerCoordX = (int)NPC.ai[2];
        int centerCoordY = (int)NPC.ai[3];
        if (Timer <= 102)
        {
            PlaceBrena(centerCoordX - t, centerCoordY);
            PlaceBrena(centerCoordX + t, centerCoordY);
        }
        else if (Timer - 102 <= 125)
        {
            t -= 102;
            PlaceBrena(centerCoordX - 102, centerCoordY - t);
            PlaceBrena(centerCoordX + 102, centerCoordY - t);
        }
        else if (Timer - 227 <= 102)
        {
            t -= 227;
            PlaceBrena(centerCoordX - 102 + t, centerCoordY - 125);
            PlaceBrena(centerCoordX + 102 - t, centerCoordY - 125);
        }
        else
        {
            Stage = 1;
            Timer = 0;
        }
    }
    static void PlaceBrena(int x, int y)
    {
        var ctile = Main.tile[x, y];
        if (!ctile.HasTile)
        {
            ctile.HasTile = true;
            ctile.type = (ushort)ModContent.TileType<BrenaTile>();
            WorldGen.TileFrame(x, y);
        }
    }
}