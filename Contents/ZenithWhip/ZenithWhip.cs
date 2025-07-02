using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Localization;
using FinalFractalProfile = Terraria.Graphics.FinalFractalHelper.FinalFractalProfile;
namespace LogSpiralsAbstractWeapons.Contents.ZenithWhip
{
    public class ZenithWhip : ModItem
    {
        public override void SetDefaults()
        {
            // This method quickly sets the whip's properties.
            // Mouse over to see its parameters.
            Item.DefaultToWhip(ModContent.ProjectileType<ZenithWhipProj>(), 20, 2, 4);
            Item.rare = ItemRarityID.Purple;
            Item.channel = true;
            Item.damage = 500;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = Item.useAnimation = 60;
        }

        // Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Zenith)
                .AddTile(TileID.LunarCraftingStation)
                .Register();

        }

        // Makes the whip receive melee prefixes
        public override bool MeleePrefix()
        {
            return true;
        }
    }
    public class ZenithWhipProj : ModProjectile
    {
        private static KeyValuePair<int, FinalFractalProfile>[] _fractalProfiles = [.. FinalFractalHelper._fractalProfiles];

        public override void SetStaticDefaults()
        {
            // This makes the projectile use whip collision detection and allows flasks to be applied to it.
            ProjectileID.Sets.IsAWhip[Type] = true;
        }

        public override void SetDefaults()
        {
            // This method quickly sets the whip's properties.
            Projectile.DefaultToWhip();

            // use these to change from the vanilla defaults
            Projectile.WhipSettings.Segments = _fractalProfiles.Length;
            Projectile.WhipSettings.RangeMultiplier = 3f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 0;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.ownerHitCheck = true;
        }

        private float Timer
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        private float RealTimer
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        Vector2[][] oldNodePositions;
        float[][] oldNodeRotations;

        public override void PostAI()
        {
            RealTimer++;
            Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out int _, out float _);
            float factor = RealTimer / timeToFlyOut;
            Timer = MathHelper.SmoothStep(0, timeToFlyOut - 2, factor);
            if (RealTimer > timeToFlyOut + 30)
                Timer = timeToFlyOut;
            if (RealTimer == (float)(int)(timeToFlyOut / 2f))
            {
                SoundEngine.PlaySound(SoundID.Item153, Projectile.Center);
            }
            // Timer = factor * timeToFlyOut * (.5f + .5f * MathF.Cos(factor * MathHelper.Pi * 2));

            Projectile.WhipSettings.RangeMultiplier += 1 / 60f;

            if (Main.netMode != NetmodeID.Server)
            {
                List<Vector2> list = [];
                Projectile.FillWhipControlPoints(Projectile, list);

                oldNodePositions ??= new Vector2[21][];
                oldNodeRotations ??= new float[21][];

                Vector2 pos = list[0];

                for (int i = 0; i < list.Count - 1; i++)
                {
                    var currentPosArray = oldNodePositions[i] ??= new Vector2[30];
                    var currentRotArray = oldNodeRotations[i] ??= new float[30];

                    Vector2 element = list[i];
                    Vector2 diff = list[i + 1] - element;

                    for (int j = 29; j > 0; j--)
                    {
                        currentPosArray[j] = currentPosArray[j - 1];
                        currentRotArray[j] = currentRotArray[j - 1];
                    }
                    currentRotArray[0] = diff.ToRotation() + MathHelper.PiOver2;
                    currentPosArray[0] = pos;
                    pos += diff;
                }
            }
            base.PostAI();
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
            //Projectile.damage = (int)(Projectile.damage * 0.75f); // Multihit penalty. Decrease the damage the more enemies the whip hits.
        }

        // This method draws a line between all points of the whip, in case there's empty space between the sprites.
        private static void DrawLine(List<Vector2> list, float alpha)
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
                Color color = Lighting.GetColor(element.ToTileCoordinates(), Color.White) * alpha;
                Vector2 scale = new Vector2(1, (diff.Length() + 2) / frame.Height);

                Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, SpriteEffects.None, 0);

                pos += diff;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {


            Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out int _, out float _);

            float aScaler = Utils.GetLerpValue(0, 25, RealTimer, true) * Utils.GetLerpValue(25, 0, RealTimer - timeToFlyOut, true);

            List<Vector2> list = [];
            Projectile.FillWhipControlPoints(Projectile, list);
            DrawLine(list, aScaler);

            //Main.DrawWhip_WhipBland(Projectile, list);
            // The code below is for custom drawing.
            // If you don't want that, you can remove it all and instead call one of vanilla's DrawWhip methods, like above.
            // However, you must adhere to how they draw if you do.

            SpriteEffects flip = 0;//Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            //Texture2D texture = ModContent.Request<Texture2D>($"{nameof(LogSpiralsAbstractWeapons)}/Contents/ZenithWhip/ZenithWhipProj2").Value;//TextureAssets.Projectile[Type].Value;

            Vector2 pos = list[0];

            for (int i = 0; i < list.Count - 1; i++)
            {
                var currentPosArray = oldNodePositions[i] ??= new Vector2[30];
                var currentRotArray = oldNodeRotations[i] ??= new float[30];


                var profile = _fractalProfiles[i];
                Main.instance.LoadItem(profile.Key);
                Texture2D texture = TextureAssets.Item[profile.Key].Value;
                Vector2 origin = texture.Size() * Vector2.UnitY;
                float scale = 1;
                if (i == list.Count - 2)
                {
                    // For a more impactful look, this scales the tip of the whip up when fully extended, and down when curled up.
                    float t = Timer / timeToFlyOut;
                    scale = MathHelper.Lerp(0.5f, 1.5f, Utils.GetLerpValue(0.1f, 0.7f, t, true) * Utils.GetLerpValue(0.9f, 0.7f, t, true));
                }

                Vector2 element = list[i];
                Vector2 diff = list[i + 1] - element;

                float rotation = diff.ToRotation() - MathHelper.PiOver2; // This projectile's sprite faces down, so PiOver2 is used to correct rotation.

                if (RealTimer < timeToFlyOut + 15) 
                {
                    var dummyAI1 = Projectile.ai[1];
                    var dummyPos = Projectile.oldPos;
                    var dummyRot = Projectile.oldRot;
                    Projectile.ai[1] = profile.Key;
                    Projectile.oldPos = currentPosArray;
                    Projectile.oldRot = currentRotArray;
                    default(FinalFractalHelper).Draw(Projectile);
                    Projectile.ai[1] = dummyAI1;
                    Projectile.oldPos = dummyPos;
                    Projectile.oldRot = dummyRot;
                }


                //Main.EntitySpriteDraw(texture, pos - Main.screenPosition, null, color, rotation + MathHelper.PiOver4 * 3, origin, scale, flip, 0);
                for (int u = 0; u < 4; u++)
                    Main.EntitySpriteDraw(
                        texture,
                        (u == 0 ? pos : currentPosArray[u * 7]) - Main.screenPosition,
                        null,
                        Lighting.GetColor(currentPosArray[u * 7].ToTileCoordinates()) * ((1 - u * .25f) * aScaler),
                        currentRotArray[u * 7] + MathHelper.PiOver4 * 7,
                        origin,
                        scale,
                        flip,
                        0);



                pos += diff;
            }
            return false;
        }

    }
}
