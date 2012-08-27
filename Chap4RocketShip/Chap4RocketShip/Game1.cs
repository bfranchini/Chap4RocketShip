using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Chap4RocketShip
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private Texture2D rockTexture, backgroundTexture;
       
        private Vector2 rockPosition = new Vector2(200.0f, 100.0f);
        private Vector2 backgroundPosition = new Vector2(0.0f, 0.0f);
        private float rockRotation;

        //rock dimensions
        private Vector2 rockCenter;
        private int rockWidth, rockHeight;

        //background dimensions
        private Vector2 backgroundCenter;
        private int backgroundWidth, backgroundHeight;

        //motion tracking variables
        private float rockRotationSpeed;
        private Vector2 rockSpeed;
        private bool move = true;

        //variables for collision detection
        private const int ROCK = 0;
        
        private Color[] rockColor;
        

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            rockSpeed = new Vector2(.16f, .16f); //.16f;  //continuous rock lateral speed (Rock is always moving)
            rockRotationSpeed = 0.3f; //continuous rock rotational speed
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            
            rockTexture = Content.Load<Texture2D>("Images\\asteroid");
            backgroundTexture = Content.Load<Texture2D>("Images\\B1_stars");
            rockWidth = rockTexture.Width;
            rockHeight = rockTexture.Height;
            
            backgroundWidth = Window.ClientBounds.Width;
            backgroundHeight = Window.ClientBounds.Height;

            rockCenter = new Vector2(rockWidth / 2, rockWidth / 2);
            
            backgroundCenter = new Vector2(backgroundWidth / 2, backgroundHeight / 2);

            //initialize color arrays used for collision detection
            //an array of type color is created of size = to the area of the sprite
            rockColor = new Color[rockTexture.Width * rockTexture.Height];
            //rock texture data is passed to rockColor array
            rockTexture.GetData(rockColor);
            
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            //call movement and rotation functions here to ensure consistency each frame
            //  UpdateAsteroid(gameTime);
            RotateShip(gameTime);
            MoveShip(gameTime);
            CheckCollisions();
            resetGame();
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //Vector2 bgTexturePos = new Vector2(Window.ClientBounds.Width,Window.ClientBounds.Height);
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend); //start drawing 2d images
            spriteBatch.Draw(backgroundTexture, backgroundPosition, null, Color.White, 0.0f, backgroundCenter, 1.0f,
                             SpriteEffects.None, 1.0f);
            spriteBatch.Draw(rockTexture, rockPosition, null, Color.White, rockRotation, rockCenter, 1.0f,
                             SpriteEffects.None, 0.0f);
           
            spriteBatch.End(); //stop drawing 2d images
            base.Draw(gameTime);
        }


        //This function returns a rectangle that contains the margins of the safe viewing area
        private Rectangle TitleSaferegion(int spriteWidth, int spriteHeight)
        {
#if Xbox
    //some televisions only show 80% of the window
        Vector2 start  = new Vector2(); //starting pixel X & Y
        const float MARGIN  = 0.2f; //only 80% visible on xbox 360

        start.X = graphics.GraphicsDevice.Viewport.Width * MARGIN/2.0f;
        start.Y = graphics.GraphicsDevice.Viewport.Height*(1 - MARGIN/2.0f); //ensure image drawn in safe region on all sides

        return new Rectangle(
            (int)start.X,                    //surrounding safe area
            (int)start.Y,                    //top, left, width, height                          
            (int)(1.0f-MARGIN)*Window.ClientBounds.Width-spriteWidth, 
            (int)(1.0f-MARGIN)*Window.ClientBounds.Height-spriteHeight;
#endif
            //show entire region on the pc or zune
            return new Rectangle(0, 0, Window.ClientBounds.Width - spriteWidth, Window.ClientBounds.Height - spriteHeight);

            //0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height
        }

        //this function updates the asteroid's position and orientation. this method is called for each frame
        private void UpdateAsteroid(GameTime gametime)
        {
            //time between frames
            float timeLapse = (float)gametime.ElapsedGameTime.Milliseconds;

            if (move == true)
            {
                //asteriod centered at the middle of the image
                Rectangle safeArea = TitleSaferegion(rockWidth / 2, rockHeight / 2);

                //asteroid right edge exceeds right window edge
                if (rockPosition.X > safeArea.Right)
                {
                    rockPosition.X = safeArea.Right; //move rock back
                    rockSpeed.X *= -1.0f; //reverse rock direction
                }
                //asteroid left edge precedes left window edge
                else if (rockPosition.X - rockCenter.X < 0)
                {
                    rockPosition.X = rockCenter.X;
                    rockSpeed.X *= -1.0f;
                }
                //asteroid top edge exceeds top window edge
                else if (rockPosition.Y < 0)
                {
                    rockPosition.Y = safeArea.Top; //move rock back
                    rockSpeed.Y *= -1.0f; //reverse direction
                }
                //asteroid bottom edge exceeds safe area
                else if (rockPosition.Y > safeArea.Bottom)
                {
                    rockPosition.Y = safeArea.Bottom;
                    rockSpeed.Y *= -1.0f;
                }
                //asteroid within window bounds so update rockPosition
                else
                {
                    rockPosition += rockSpeed * timeLapse;
                }

                //scale radians by time between frames so rotation is uniform rate on all systems
                //mod by 2pi to keep cumulative rock rotation between 0 and 2pi at all times
                //the angle of a circle is 360 degrees or 2pi radians
                const float SCALE = 50.0f;
                rockRotation += rockRotationSpeed * timeLapse / SCALE;
                rockRotation = rockRotation % (MathHelper.Pi * 2.0f);
            }
        }


        private void MoveShip(GameTime gameTime)
        {
            const float SCALE = 20.0f; //scale determines how fast ship moves when key is pressed. 20.0f is original
            float speed = gameTime.ElapsedGameTime.Milliseconds / 100.0f;
            //elapsed time in ms over 100...why 100? The 100 also affects how fast ship moves

            KeyboardState keyboard = Keyboard.GetState(); //user input
            GamePadState gamePad = GamePad.GetState(PlayerIndex.One);
            Rectangle safeArea = TitleSaferegion(shipWidth / 2, shipHeight / 2);
            if (move && !gamePad.IsConnected) //keyboard is being used to move ship
            {


                if (keyboard.IsKeyDown(Keys.Down) && keyboard.IsKeyDown(Keys.Up))
                {
                    //up and down pressed at same time so don't move
                }

                else if (keyboard.IsKeyDown(Keys.Up)) //forwards
                {

                    shipPosition.X += (float)Math.Sin(shipRotation) * speed * SCALE;
                    //sin(thetha) * hypotenuse(speed) = opposite(which is x) p.47 in book
                    shipPosition.Y -= (float)Math.Cos(shipRotation) * speed * SCALE;
                    //cos(theta) * hypotenuse(speed) = adjacent(which is y)

                }
                else if (keyboard.IsKeyDown(Keys.Down)) //backwards
                {
                    // if (shipPosition.X < safeArea.Right && shipPosition.X > safeArea.Left && shipPosition.Y > safeArea.Top && shipPosition.Y < safeArea.Bottom)
                    //{
                    shipPosition.X -= (float)Math.Sin(shipRotation) * speed * SCALE;
                    shipPosition.Y += (float)Math.Cos(shipRotation) * speed * SCALE;
                    //}
                }
            }

            else if (move) //gamepad
            {
                shipPosition.X += (float)Math.Sin(shipRotation) * gamePad.ThumbSticks.Left.Y * speed * SCALE;
                //on the gamepad, y can be a positive or a negative value so only 2 statements are needed
                shipPosition.Y -= (float)Math.Cos(shipRotation) * gamePad.ThumbSticks.Left.Y * speed * SCALE;
            }

            //Prevent ship from going out of bounds. I think better way to do this would be collision detection? Currently 
            //half of the ship goes out of bounds before it's stopped.
            if (shipPosition.X < safeArea.Left)
            {
                //ship is outside left or right bounds of the screen so don't move.
                shipPosition.X = safeArea.Left;
            }
            else if (shipPosition.X > safeArea.Right)
            {
                shipPosition.X = safeArea.Right;
            }
            //ship is outside top or bottom bounds of screen so don't move
            else if (shipPosition.Y < safeArea.Top)
            {
                shipPosition.Y = safeArea.Top;
            }
            else if (shipPosition.Y > safeArea.Bottom)
            {
                shipPosition.Y = safeArea.Bottom;
            }
        }
        private void CheckCollisions()
        {
            Matrix shipTransform, rockTransform;
            Rectangle shipRectangle, rockRectangle;

            //transform the rectangles which surround each sprite
            rockTransform = Transform(rockCenter, rockRotation, rockPosition);
            rockRectangle = TransformRectangle(rockTransform, rockWidth, rockHeight);

            shipTransform = Transform(shipCenter, shipRotation, shipPosition);
            shipRectangle = TransformRectangle(shipTransform, shipWidth, shipHeight);

            //collision checking
            if (rockRectangle.Intersects(shipRectangle)) //rough collision check
            {
                if (PixelCollision(rockTransform, rockWidth, rockHeight, ROCK,
                                   shipTransform, shipWidth, shipHeight, SHIP))
                    move = false;
            }
        }

        public bool PixelCollision(Matrix transformA, int pixelWidthA, int pixelHeightA, int A,
                                   Matrix transformB, int pixelWidthB, int pixelHeightB, int B)
        {
            //set A transformation relative to B. B remains at x=0, y=0
            Matrix AtoB = transformA * Matrix.Invert(transformB);

            //generate a perpendicular vectors to each rectangle side
            Vector2 columnStep, rowStep, rowStartPosition;
            columnStep = Vector2.TransformNormal(Vector2.UnitX, AtoB);
            rowStep = Vector2.TransformNormal(Vector2.UnitY, AtoB);

            //calculate the top left corner of A
            rowStartPosition = Vector2.Transform(Vector2.Zero, AtoB);

            //search each row of pixels in A. start at the top and move down
            for (int rowA = 0; rowA < pixelHeightA; rowA++)
            {
                //begin at the left
                Vector2 pixelPositionA = rowStartPosition;

                //for each column in the row(move left to right)
                for (int colA = 0; colA < pixelWidthA; colA++)
                {
                    //get the pixel position
                    int X = (int)Math.Round(pixelPositionA.X);
                    int Y = (int)Math.Round(pixelPositionA.Y);

                    //if the pixel is within the bounds of b
                    if (X >= 0 && X < pixelWidthB && Y >= 0 && Y < pixelHeightB)
                    {
                        //get colors of overlapping pixels
                        Color colorA = PixelColor(A, colA + rowA * pixelWidthA);
                        Color ColorB = PixelColor(B, X + Y * pixelWidthB);

                        //if both pixels are not completely transparent, collision
                        if (colorA.A != 0 && ColorB.A != 0)
                        {
                            return true;
                        }
                    }
                    // move to the next pixel in the row of a
                    pixelPositionA += columnStep;
                }
                //move to the next row of A
                rowStartPosition += rowStep;
            }
            return false; //no collision
        }


        //this method is used in checking for collisions between bounding rectangles
        //each corner of each rectangle is transformed. Then a new rectangle is generated
        //using top left vertex from the newly transformed corners and the x and y distance
        //of opposite corner
        public static Rectangle TransformRectangle(Matrix transform, int width, int height)
        {
            //get each corner of texture
            Vector2 leftTop = new Vector2(0.0f, 0.0f);
            Vector2 rightTop = new Vector2(width, 0.0f);
            Vector2 leftBottom = new Vector2(0.0f, height);
            Vector2 rightBottom = new Vector2(width, height);

            //transform each corner
            Vector2.Transform(ref leftTop, ref transform, out leftTop);
            Vector2.Transform(ref rightTop, ref transform, out rightTop);
            Vector2.Transform(ref leftBottom, ref transform, out leftBottom);
            Vector2.Transform(ref rightBottom, ref transform, out rightBottom);

            //find the minimum and maximum corners
            Vector2 min = Vector2.Min(Vector2.Min(leftTop, rightTop),
                                      Vector2.Min(leftBottom, rightBottom));
            Vector2 max = Vector2.Max(Vector2.Max(leftTop, rightTop),
                                      Vector2.Max(leftBottom, rightBottom));

            //return transformed rectangle
            return new Rectangle((int)min.X, (int)min.Y, (int)(max.X - min.X), (int)(max.Y - min.Y));
        }


        //this method generates a cumulative transformation matrix according to sprites'
        // current rotation and position. Sprite is first shifted to origin, then z rotation
        //and translation is performed on x and y
        //this method is used when checking for bounding rectangles
        public Matrix Transform(Vector2 center, float rotation, Vector2 position)
        {
            //move to origin, scale(if desired), rotate, translate
            return Matrix.CreateTranslation(new Vector3(-center, 0.0f)) *
                //add scaling here if desired
                   Matrix.CreateRotationZ(rotation) *
                   Matrix.CreateTranslation(new Vector3(position, 0.0f));
        }

        //gets specific color for given pixel in given sprite. Used for collision detection
        public Color PixelColor(int objectNum, int pixelNum)
        {
            switch (objectNum)
            {
                case ROCK:
                    return rockColor[pixelNum];
                case SHIP:
                    return shipColor[pixelNum];
            }
            return Color.White;
        }

        //reset objects to their original state
        public void resetGame()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                shipPosition = new Vector2(100.0f, 100.0f);
                shipRotation = 0.0f;
                rockPosition = new Vector2(200.0f, 100.0f);
                Initialize();
                move = true;
            }
        }

    }
}
