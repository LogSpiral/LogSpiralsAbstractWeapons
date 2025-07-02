using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using static Microsoft.Xna.Framework.Graphics.SpriteBatch;

namespace LogSpiralsAbstractWeapons.Tools;

public unsafe static class SpriteBatchExtension
{
    extension(SpriteBatch spriteBatch)
    {
        public unsafe void VertexDraw(
            Texture2D texture,
            Vector2 position0,
            Vector2 position1,
            Vector2 position2,
            Vector2 position3,

            Vector2 texcoord0,
            Vector2 texcoord1,
            Vector2 texcoord2,
            Vector2 texcoord3,

            Color color0,
            Color color1,
            Color color2,
            Color color3,

            float depth
        )
        {
            if (spriteBatch.numSprites >= spriteBatch.vertexInfo.Length)
            {
                if (spriteBatch.vertexInfo.Length >= MAX_ARRAYSIZE)
                    spriteBatch.FlushBatch();
                else
                {
                    int newMax = Math.Min(spriteBatch.vertexInfo.Length * 2, MAX_ARRAYSIZE);
                    Array.Resize(ref spriteBatch.vertexInfo, newMax);
                    Array.Resize(ref spriteBatch.textureInfo, newMax);
                    Array.Resize(ref spriteBatch.spriteInfos, newMax);
                    Array.Resize(ref spriteBatch.sortedSpriteInfos, newMax);
                }
            }

            if (spriteBatch.sortMode == SpriteSortMode.Immediate)
            {
                int offset;
                fixed (VertexPositionColorTexture4* sprite = &spriteBatch.vertexInfo[0])
                {
                    sprite->Position0 = new Vector3(position0, depth);
                    sprite->Position1 = new Vector3(position1, depth);
                    sprite->Position2 = new Vector3(position2, depth);
                    sprite->Position3 = new Vector3(position3, depth);

                    sprite->Color0 = color0;
                    sprite->Color1 = color1;
                    sprite->Color2 = color2;
                    sprite->Color3 = color3;

                    sprite->TextureCoordinate0 = texcoord0;
                    sprite->TextureCoordinate1 = texcoord1;
                    sprite->TextureCoordinate2 = texcoord2;
                    sprite->TextureCoordinate3 = texcoord3;

                    if (spriteBatch.supportsNoOverwrite)
                        offset = spriteBatch.UpdateVertexBuffer(0, 1);
                    else
                    {
                        offset = 0;
                        spriteBatch.vertexBuffer.SetDataPointerEXT(
                            0,
                            (IntPtr)sprite,
                            VertexPositionColorTexture4.RealStride,
                            SetDataOptions.None
                        );
                    }
                }
                spriteBatch.DrawPrimitives(texture, offset, 1);
            }
            else if (spriteBatch.sortMode == SpriteSortMode.Deferred)
            {
                fixed (VertexPositionColorTexture4* sprite = &spriteBatch.vertexInfo[spriteBatch.numSprites])
                {
                    sprite->Position0 = new Vector3(position0, depth);
                    sprite->Position1 = new Vector3(position1, depth);
                    sprite->Position2 = new Vector3(position2, depth);
                    sprite->Position3 = new Vector3(position3, depth);

                    sprite->Color0 = color0;
                    sprite->Color1 = color1;
                    sprite->Color2 = color2;
                    sprite->Color3 = color3;

                    sprite->TextureCoordinate0 = texcoord0;
                    sprite->TextureCoordinate1 = texcoord1;
                    sprite->TextureCoordinate2 = texcoord2;
                    sprite->TextureCoordinate3 = texcoord3;
                }

                spriteBatch.textureInfo[spriteBatch.numSprites] = texture;
                spriteBatch.numSprites += 1;
            }
            else
            {
                throw new NotImplementedException($"Mode:{SpriteSortMode.Texture}, {SpriteSortMode.FrontToBack}, {SpriteSortMode.BackToFront} Not Support.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="texture">要绘制的贴图</param>
        /// <param name="position">绘制锚点，绘制帧上origin点所在处</param>
        /// <param name="frame">从贴图上裁切下来的绘制帧</param>
        /// <param name="color">给贴图染的颜色，默认是乘算</param>
        /// <param name="rotationStandard">贴图的标准朝向，会以这个为轴进行翻转</param>
        /// <param name="rotationOffset">缩放前旋转量</param>
        /// <param name="rotationDirection">x轴正方向朝向</param>
        /// <param name="origin">绘制帧锚点</param>
        /// <param name="scaler">缩放系数</param>
        /// <param name="flip">是否翻转</param>
        public void Draw(
            Texture2D texture,
            Vector2 position,
            Rectangle? frame,
            Color color,
            float rotationStandard,
            float rotationOffset,
            float rotationDirection,
            Vector2 origin,
            Vector2 scaler,
            bool flip)
        {
            Rectangle realFrame = frame ?? new Rectangle(0, 0, texture.Width, texture.Height);
            origin /= realFrame.Size();
            Vector2[] vecs = new Vector2[4];
            for (int i = 0; i < 4; i++)
            {
                var v = new Vector2(i % 2, i / 2);
                v += new Vector2(-origin.X, origin.Y - 1);      // 把绘制锚点平移绘制帧锚点的位置
                v *= realFrame.Size();                          // 变换到帧大小
                v = v.RotatedBy(-rotationStandard);             // 使得x正方向朝向指定标准方向
                if (flip)                                       
                    v.Y *= -1;                                  // 以标准方向为轴翻转
                v = v.RotatedBy(rotationOffset);                // 旋转偏移量
                v *= scaler;                                    // 缩放
                v = v.RotatedBy(rotationDirection);             // 椭圆朝向旋转量
                vecs[i] = v;
            }

            Vector2 startCoord = realFrame.TopLeft() / texture.Size();
            Vector2 endCoord = realFrame.BottomRight() / texture.Size();

            if (flip)
                spriteBatch.VertexDraw(texture, vecs[0] + position, vecs[2] + position, vecs[1] + position, vecs[3] + position,
                     new Vector2(startCoord.X, endCoord.Y), startCoord, endCoord, new(endCoord.X, startCoord.Y),
                    color, color, color, color, 0
                    );
            else
                spriteBatch.VertexDraw(texture, vecs[0] + position, vecs[1] + position, vecs[2] + position, vecs[3] + position,
                    new Vector2(startCoord.X, endCoord.Y), endCoord, startCoord, new(endCoord.X, startCoord.Y),
                    color, color, color, color, 0
                    );
        }

    }
}
