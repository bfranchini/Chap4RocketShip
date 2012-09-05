using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Audio;
//using Microsoft.Xna.Framework.Content;
//using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//using Microsoft.Xna.Framework.Media;


namespace Chap4RocketShip
{
    /// <summary>
    /// Spaceship!
    /// </summary>
    internal class Spaceship
    {
        private Vector2 shipPosition = new Vector2(100.0f, 100.0f);
        private Texture2D shipTexture;
        private float shipRotation;

        //ship dimensions
        private Vector2 shipCenter;
        private int shipWidth, shipHeight;

        //variables for collision detection
        private const int SHIP = 1;
        private Color[] shipColor;

        //public properties 
        public bool Move
        {
            get { return move; }
        }
         bool move;

    //default constructor
        public void LoadContent(Game game)
        {
            //load and initialize ship
            shipTexture = game.Content.Load<Texture2D>("Images\\ship");
            shipWidth = shipTexture.Width;
            shipHeight = shipTexture.Height;
            shipCenter = new Vector2(shipWidth / 2, shipHeight / 2);
        }


        public void HandleCollisions()
        {
            //initialize color arrays used for collision detection
            //an array of type color is created of size = to the area of the sprite
            shipColor = new Color[shipTexture.Width * shipTexture.Height];
            shipTexture.GetData(shipColor);
        }

        public void draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(shipTexture, shipPosition, null, Color.White, shipRotation, shipCenter, 1.0f,
                            SpriteEffects.None, 0.0f);
        }


        private void GetInput(KeyboardState keyboardState, 
                              GamePadState gamePadState, 
                              float rotation,
                               float speed)
        {
            //handle user input
            keyboardState = Keyboard.GetState();
            gamePadState = GamePad.GetState(PlayerIndex.One);
        }

        //Rotate Ship
        private float RotateShip(GameTime gameTime)
        {
            float rotation = 0.0f;
            float speed = gameTime.ElapsedGameTime.Milliseconds / 300.0f; //speed of rotation

            if (!Move) //collision has ocurred so don't rotate ship any more
                return rotation;

            
            KeyboardState keyboard = Keyboard.GetState();
            GamePadState gamePad = GamePad.GetState(PlayerIndex.One);

            if (!gamePad.IsConnected)
            {
                if (keyboard.IsKeyDown(Keys.Right) && keyboard.IsKeyDown(Keys.Left))
                {
                    //don't rotate if right and left pressed at same time
                }
                else if (keyboard.IsKeyDown(Keys.Right)) //right
                {
                    rotation = speed;
                }
                else if (keyboard.IsKeyDown(Keys.Left)) //left
                {
                    rotation = -speed;
                }
                else //controller input
                {
                    rotation = gamePad.ThumbSticks.Left.X * speed;
                }
            }
            //update rotation based on time scale and only store between 0 & 2pi
            shipRotation += rotation;
            shipRotation = shipRotation % (MathHelper.Pi * 2.0f);
            return shipRotation;
        }
    }
}
