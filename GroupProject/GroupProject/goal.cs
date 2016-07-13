using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Maps.Tiled;
using MonoGame.Extended.ViewportAdapters;using System;using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;


namespace GroupProject
{
    class goal
    {
        Sprite sprite = new Sprite();
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public void Load(ContentManager content)
        {
            AnimatedTexture animation = new AnimatedTexture(new Vector2(0, 0), 0, 0.2f, 1);
            animation.Load(content, "goal", 6, 5);
            sprite.Add(animation, 0, -25);            sprite.Pause();
        }
    }
}
