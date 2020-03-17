using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RPG;

namespace RPG
{
	public class BetweenGames : MiniScreen
	{
		private readonly int stairWidth = 500;
		private readonly int stairHeight = 325;

		private Texture2D stairClimber;
		private double timer;
		private int iterations;
		
		public BetweenGames(ContentManager contentManager)
		{
			stairClimber = contentManager.Load<Texture2D>("Map/TransitionAnim");
			timer = 0.0;
			iterations = 0;
		}

		void MiniScreen.Unload()
		{ 
		}

		byte MiniScreen.Update(GameTime dt, KeyboardState prevStateKb, MouseState prevStateM)
		{
			timer += dt.ElapsedGameTime.TotalSeconds * 22;
			if (iterations > 6)
				return 255;
			return 1;
		}

		void MiniScreen.Draw(SpriteBatch sb)
		{
			if (timer > 15)
			{
				timer = 0;
				iterations++;
			}

			int frameX = (int)timer % 5;
			int frameY = (int)timer / 5;

			sb.Begin();
			sb.Draw(stairClimber, new Rectangle(0, 0, Game1.width, Game1.height), new Rectangle(frameX * stairWidth, frameY * stairHeight, stairWidth, stairHeight), Color.White);
			sb.End();
		}
	}
}
