// Copyright 2010 Giovanni Botta

// This file is part of GeomClone.

// GeomClone is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// GeomClone is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with GeomClone.  If not, see <http://www.gnu.org/licenses/>.

#region Using directives
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace GameEngine
{
    public abstract class GameObject2DAbstract
    {
        #region Constructors
        public GameObject2DAbstract(BaseGame theGame) { game = theGame; /**graphicsDeviceManager = theGame.GraphicsDeviceManager; **/ }
        #endregion

        #region Members
        BaseGame game;
        #endregion

        #region Resolution
        // assumption: all the textures are made to be drawn on a WxH screen
        //GraphicsDeviceManager graphicsDeviceManager;
        protected static readonly int referenceWidth = 1600, referenceHeight = 1200;
        //protected static int width = 800, height = 600;
        public int Width { get { return game.Width(); } }
        public int Height { get { return game.Height(); } }

        //public static void SetResolution(int w, int h)
        //{
        //    width = w;
        //    height = h;
        //}
        #endregion

        #region Draw
        public abstract void Draw(GameTime time);
        #endregion

        #region Update
        public virtual void Update(GameTime time) { }
        #endregion
    }

    public class Line2D : GameObject2D
    {
        #region Constructor
        public Line2D(BaseGame game, Vector2 position, float rotation, float width, float length, Color color)
            : base(game, "smallRect", position, rotation, length / 50.0f, width / 5.0f, color)
        { }
        #endregion
    }

    public class GameObject2D : GameObject2DAbstract
    {       

        #region Member variables
        private BaseGame game;
        private Texture2D texture;
        // origin, position, scale and bb in transformed coordinate with x,y in range 0,1
        private Vector2 origin, position;
        private float scaleX = 1.0f, scaleY = 1.0f, rotation = 0.0f;
        private Rectangle sourceRectangle;
        private Color color;

        // transform matrix in real coordinate system
        protected Matrix transform;

        protected Color[] textureData;
        #endregion

        #region Properties
        protected Rectangle BBox
        {
            get
            {
                transform =
                    Matrix.CreateTranslation(new Vector3(-origin, 0.0f)) *
                    Matrix.CreateScale(scaleX, scaleY, 1.0f) *
                    Matrix.CreateRotationZ(rotation) *
                    Matrix.CreateTranslation(new Vector3(position.X * referenceWidth, position.Y * referenceHeight, 0.0f));
                return CalculateBoundingRectangle(ref sourceRectangle, ref transform);
            }
        }
        public BaseGame TheGame { get { return game; } }
        protected Rectangle SourceRectangle { get { return sourceRectangle; } }
        protected Color TheColor { get { return color; } set { color = value; } }
        protected Texture2D Texture { get { return texture; } }
        protected float Rotation { get { return rotation; } set { rotation = value; } }
        protected float ScaleX { get { return scaleX; } set { scaleX = value; } }
        protected float ScaleY { get { return scaleY; } }
        internal Vector2 Position { get { return position; } set { position = value; } }
        protected Vector2 Origin { get { return origin; } }
        #endregion

        #region Constructor
        public GameObject2D(BaseGame theGame, string theTexture, Vector2 thePosition, float theRotation, float setScaleX, float setScaleY, Color setColor)
            : base(theGame)
        {
            game = theGame;
            texture = game.Content.Load<Texture2D>(theTexture);
            position = thePosition;
            rotation = theRotation;
            scaleX = setScaleX;
            scaleY = setScaleY;
            color = setColor;

            Initialize();
        }
        public GameObject2D(BaseGame theGame, Texture2D theTexture)
            : base(theGame)
        {
            game = theGame;
            texture = theTexture;
            position = new Vector2(0.5f, 0.5f);

            Initialize();
        }
        public void Initialize()
        {
            origin = new Vector2(texture.Width / 2 / (float)referenceWidth, texture.Height / 2 / (float)referenceHeight);           
            sourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);
            textureData = new Color[texture.Width * texture.Height];
            texture.GetData(textureData);
        }
        #endregion

        #region Draw
        public override void Draw(GameTime gameTime)
        {   
            game.spriteBatch.Draw(texture, new Vector2(position.X * Width, position.Y * Height),
                sourceRectangle, color, MathHelper.Pi / 180.0f * rotation, new Vector2(origin.X * referenceWidth, origin.Y * referenceHeight), 
                new Vector2(scaleX*Width/referenceWidth, scaleY*Height/referenceHeight), SpriteEffects.None, 0);
        }        
        #endregion

        #region Update
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
        #endregion

        #region Collisions
        public bool collidesWith(GameObject2D other)
        {
            if (BBox.Intersects(other.BBox))
            {
                // inverse of transform matrix of other object (faster than actually calculate the inverse)
                Matrix inverseTransform =
                    Matrix.CreateTranslation(new Vector3(-other.Position.X * referenceWidth, -other.Position.Y * referenceHeight, 0.0f)) *
                    Matrix.CreateRotationZ(-other.Rotation) *
                    Matrix.CreateScale(1 / other.ScaleX, 1 / other.ScaleY, 1.0f) *
                    Matrix.CreateTranslation(new Vector3(other.Origin, 0.0f));

                if (IntersectPixels(ref transform, Texture.Width, Texture.Height, ref textureData,
                    ref inverseTransform, other.Texture.Width, other.Texture.Height, ref other.textureData))
                    return true;
            }

            return false;
        }
        
        private bool IntersectPixels(
                            ref Matrix transformA, int widthA, int heightA, ref Color[] dataA,
                            ref Matrix inverseTransformB, int widthB, int heightB, ref Color[] dataB)
        {
            // Calculate a matrix which transforms from A's local space into
            // world space and then into B's local space
            Matrix transformAToB = transformA * inverseTransformB;

            // When a point moves in A's local space, it moves in B's local space with a
            // fixed direction and distance proportional to the movement in A.
            // This algorithm steps through A one pixel at a time along A's X and Y axes
            // Calculate the analogous steps in B:
            Vector2 stepX = Vector2.TransformNormal(Vector2.UnitX, transformAToB);
            Vector2 stepY = Vector2.TransformNormal(Vector2.UnitY, transformAToB);

            // Calculate the top left corner of A in B's local space
            // This variable will be reused to keep track of the start of each row
            Vector2 yPosInB = Vector2.Transform(Vector2.Zero, transformAToB);

            // For each row of pixels in A
            for (int yA = 0; yA < heightA; yA++)
            {
                // Start at the beginning of the row
                Vector2 posInB = yPosInB;

                // For each pixel in this row
                for (int xA = 0; xA < widthA; xA++)
                {
                    // Round to the nearest pixel
                    int xB = (int)Math.Round(posInB.X);
                    int yB = (int)Math.Round(posInB.Y);

                    // If the pixel lies within the bounds of B
                    if (0 <= xB && xB < widthB &&
                        0 <= yB && yB < heightB)
                    {
                        // Get the colors of the overlapping pixels
                        Color colorA = dataA[xA + yA * widthA];
                        Color colorB = dataB[xB + yB * widthB];

                        // If both pixels are not completely transparent,
                        if (colorA.A != 0 && colorB.A != 0)
                        {
                            // then an intersection has been found
                            return true;
                        }
                    }

                    // Move to the next pixel in the row
                    posInB += stepX;
                }

                // Move to the next row
                yPosInB += stepY;
            }

            // No intersection found
            return false;
        }
        
        private Rectangle CalculateBoundingRectangle(ref Rectangle rectangle, ref Matrix transform)
        {
            // Get all four corners in local space
            Vector2 leftTop = new Vector2(rectangle.Left, rectangle.Top);
            Vector2 rightTop = new Vector2(rectangle.Right, rectangle.Top);
            Vector2 leftBottom = new Vector2(rectangle.Left, rectangle.Bottom);
            Vector2 rightBottom = new Vector2(rectangle.Right, rectangle.Bottom);

            // Transform all four corners into work space
            Vector2.Transform(ref leftTop, ref transform, out leftTop);
            Vector2.Transform(ref rightTop, ref transform, out rightTop);
            Vector2.Transform(ref leftBottom, ref transform, out leftBottom);
            Vector2.Transform(ref rightBottom, ref transform, out rightBottom);

            // Find the minimum and maximum extents of the rectangle in world space
            Vector2 min = Vector2.Min(Vector2.Min(leftTop, rightTop),
                                      Vector2.Min(leftBottom, rightBottom));
            Vector2 max = Vector2.Max(Vector2.Max(leftTop, rightTop),
                                      Vector2.Max(leftBottom, rightBottom));

            // Return that as a rectangle
            return new Rectangle((int)min.X, (int)min.Y,
                                 (int)(max.X - min.X), (int)(max.Y - min.Y));
        }
        #endregion
    }
}
