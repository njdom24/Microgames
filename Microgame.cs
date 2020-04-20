using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace RPG
{
	class Microgame : MiniScreen
	{
		private KeyboardState prevState;
		private Texture2D background;

		public Microgame(ContentManager contentManager)
		{
			prevState = Keyboard.GetState();
			background = contentManager.Load<Texture2D>("Battle/005");
		}

		byte MiniScreen.Update(GameTime dt, KeyboardState prevStateKb, MouseState prevStateM)
		{
			prevState = Keyboard.GetState();

			return 1;
		}

		public void HandleInput(GameTime dt)
		{
			if (Keyboard.GetState().IsKeyDown(Keys.A) && prevState.IsKeyUp(Keys.A))
				Console.WriteLine("yay!");
		}

		void MiniScreen.Draw(SpriteBatch sb)
		{
			sb.Begin();
			sb.Draw(background, new Rectangle(0, 0, Game1.width, Game1.height), Color.White);
			//Rectangle rec = new Rectangle(0, 0,,)
			sb.End();
		}

		void MiniScreen.Unload()
		{
			
		}

	}
}