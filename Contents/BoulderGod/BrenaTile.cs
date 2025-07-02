using Microsoft.Xna.Framework;

namespace LogSpiralsAbstractWeapons.Contents.BoulderGod;

public class BrenaTile : ModTile
{
    public override void SetStaticDefaults()
    {
        MinPick = int.MaxValue;
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = true;
        Main.tileBlockLight[Type] = true;
        AddMapEntry(new Color(128, 0, 0), CreateMapEntryName());
    }

    public override bool CanKillTile(int i, int j, ref bool blockDamaged)
    {
        return false;
    }

    public override bool CanExplode(int i, int j)
    {
        return false;
    }

    public override void NearbyEffects(int i, int j, bool closer)
    {
        if (closer && !NPC.AnyNPCs(ModContent.NPCType<BoulderGod>()))
        {
            WorldGen.KillTile(i, j, false, false, false);
            if (!Main.tile[i, j].HasTile && Main.netMode != NetmodeID.SinglePlayer)
                NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, (float)i, (float)j, 0f, 0, 0, 0);
        }
    }

}
