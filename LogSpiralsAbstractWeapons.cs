global using Terraria;
global using Terraria.ID;
global using Terraria.ModLoader;

namespace LogSpiralsAbstractWeapons;

// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
public class LogSpiralsAbstractWeapons : Mod
{
}
public static class ProjectileHelper
{
    extension<T>(T proj) where T : ModProjectile
    {
        public static int ID() => ModContent.ProjectileType<T>();
    }
}