﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Maps.Tiled;
using MonoGame.Extended.ViewportAdapters;
using System;
using System.Collections.Generic;


namespace GroupProject
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Player player = null;

        Camera2D camera = null;
        TiledMap map = null;
        TiledTileLayer collisionLayer;
        TiledTileLayer spikes;
        TiledTileLayer Collums;
        TiledTileLayer Ladder;

        const int STATE_SPLASH = 0;
        const int STATE_GAME = 1;
        const int STATE_GAMEOVER = 2;
        int gameState = STATE_SPLASH;
        int score = 0;
        Texture2D medal = null;

        List<Robot> robots = new List<Robot>();
        Sprite goal = null;

        public static int tile = 64;
        // abitrary choice for 1m (1 tile = 1 meter)
        public static float meter = tile;
        // very exaggerated gravity (6x)
        public static float gravity = meter * 9.8f * 6.0f;
        // max vertical speed (10 tiles/sec horizontal, 15 tiles/sec vertical)
        public static Vector2 maxVelocity = new Vector2(meter * 10, meter * 15);
        // horizontal acceleration - take 1/2 second to reach max velocity
        public static float acceleration = maxVelocity.X * 2;
        // horizontal friction - take 1/6 second to stop from max velocity
        public static float friction = maxVelocity.X * 6;
        // (a large) instantaneous jump impulse
        public static float jumpImpulse = meter * 1500;

        List<Robot> Robots = new List<Robot>();

        public int ScreenWidth
        {
            get
            {
                return graphics.GraphicsDevice.Viewport.Width;
            }
        }
        public int ScreenHeight
        {
            get
            {
                return graphics.GraphicsDevice.Viewport.Height;
            }
        }

        


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        private void CheckCollisions()
        {
            List<Robot> deadRobots = new List<Robot>();
            foreach (Robot e in robots)
            {
                if (IsColliding(player.Bounds, e.Bounds) == true)
                {
                    if (player.IsJumping && player.Velocity.Y > 0)
                    {
                        player.JumpOnCollision();
                        deadRobots.Add(e);
                        score = score + 1;
                    }
                    else if (player.isAttacking()) // we're doing the knife animation
                    {
                        deadRobots.Add(e);
                        score = score + 1;
                    }
                    else
                    {
                        gameState = STATE_GAMEOVER;
                    }
                }
            }
            foreach (Robot dead in deadRobots)
                robots.Remove(dead);
        }



        private bool IsColliding(Rectangle rect1, Rectangle rect2)
        {
            if (rect1.X + rect1.Width < rect2.X ||
            rect1.X > rect2.X + rect2.Width ||
            rect1.Y + rect1.Height < rect2.Y ||
            rect1.Y > rect2.Y + rect2.Height)
            {
                // these two rectangles are not colliding
                return false;
            }
            // else, the two AABB rectangles overlap, therefore collision
            return true;
        }


        protected override void Initialize()
        {
            player = new Player(this);

            base.Initialize();
        }

        
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            player.Load(Content);

            medal = Content.Load<Texture2D>("Medal");

            var viewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice,
            ScreenWidth, ScreenHeight);
            camera = new Camera2D(viewportAdapter);
            camera.Position = new Vector2(0, ScreenHeight);

            map = Content.Load<TiledMap>("level1");
            foreach (TiledTileLayer layer in map.TileLayers)
            {
                if (layer.Name == "Collisions")
                    collisionLayer = layer;
            }

            foreach (TiledObjectGroup group in map.ObjectGroups)
            {
                if (group.Name == "Robot")
                {
                    foreach (TiledObject obj in group.Objects)
                    {
                        Robot robot = new Robot(this);
                        robot.Load(Content);
                        robot.Position = new Vector2(obj.X, obj.Y);
                        robots.Add(robot);
                    }
                }
                if (group.Name == "goal")
                {
                    TiledObject obj = group.Objects[0];
                    if (obj != null)
                    {
                        AnimatedTexture anim = new AnimatedTexture(
                        new Vector2(0, 0), 0, 0.5f , 1);
                        anim.Load(Content, "goal", 1, 1);
                        goal = new Sprite();
                        goal.Add(anim, 0, -85);
                        goal.position = new Vector2(obj.X, obj.Y);
                    }
                }
            }


            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            player.Update(deltaTime);

            camera.Position = player.Position - new Vector2(ScreenWidth / 2, ScreenHeight / 2);

            foreach (Robot e in robots)
            {
                e.Update(deltaTime);
            }

            switch (gameState)
            {
                case STATE_SPLASH:
                    UpdateSplashState(deltaTime);
                    break;
                case STATE_GAME:
                    UpdateGameState(deltaTime);
                    break;
                case STATE_GAMEOVER:
                    UpdateGameOverState(deltaTime);
                    break;
            }

            CheckCollisions();

            base.Update(gameTime);
        }

        private void UpdateSplashState(float deltaTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Enter) == true)
            {
                gameState = STATE_GAME;
            }
        }
        private void UpdateGameOverState(float deltaTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Enter) == true)
            {
                gameState = STATE_SPLASH;

            }
        }
        protected void UpdateGameState(float deltaTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic her
            player.Update(deltaTime);
            foreach (Robot e in robots)
            {
                e.Update(deltaTime);
            }

            camera.Position = player.Position - new Vector2(ScreenWidth / 2, ScreenHeight / 2);

            CheckCollisions();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            var transformMatrix = camera.GetViewMatrix();

            spriteBatch.Begin(transformMatrix: transformMatrix);

            map.Draw(spriteBatch);

            foreach (Robot e in robots)
            {
                e.Draw(spriteBatch);
            }

            player.Draw(spriteBatch);
            goal.Draw(spriteBatch);

            spriteBatch.End();

            spriteBatch.Begin();
            for (int i = 0; i < score; i++)
            {
                spriteBatch.Draw(medal, new Vector2(10 + i * 10,
               20), Color.White);
            }

            spriteBatch.End();


            base.Draw(gameTime);
        }
        public int PixelToTile(float pixelCoord)
        {
            return (int)Math.Floor(pixelCoord / tile);
        }
        public int TileToPixel(int tileCoord)
        {
            return tile * tileCoord;
        }
        public int CellAtPixelCoord(Vector2 pixelCoords)
        {
            if (pixelCoords.X < 0 ||
           pixelCoords.X > map.WidthInPixels || pixelCoords.Y < 0)
                return 1;
            // let the player drop of the bottom of the screen (this means death)
            if (pixelCoords.Y > map.HeightInPixels)
                return 0;
            return CellAtTileCoord(
           PixelToTile(pixelCoords.X), PixelToTile(pixelCoords.Y));
        }
        public int CellAtTileCoord(int tx, int ty)
        {
            if (tx < 0 || tx >= map.Width || ty < 0)
                return 1;
            // let the player drop of the bottom of the screen (this means death)
            if (ty >= map.Height)
                return 0;
            TiledTile tile = collisionLayer.GetTile(tx, ty);
            return tile.Id;
        }
      
    }
}
