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

		private Texture2D background;
		private Texture2D wizZoom;
		private double animTimer;
		private double timer;
		
		public BetweenGames(ContentManager contentManager)
		{
			background = contentManager.Load<Texture2D>("Menus/TransBG_GRN");
			wizZoom = contentManager.Load<Texture2D>("Menus/wizZOOM");
			animTimer = 0.0;
			timer = 0.0;
		}

		void MiniScreen.Unload()
		{ 
		}

		byte MiniScreen.Update(GameTime dt, KeyboardState prevStateKb, MouseState prevStateM)
		{
			timer += dt.ElapsedGameTime.TotalSeconds * 3;
			animTimer += dt.ElapsedGameTime.TotalSeconds * 40;
			if (animTimer > 4*40)
				return 255;
			return 1;
		}

		void MiniScreen.Draw(SpriteBatch sb)
		{
			sb.Begin(samplerState: SamplerState.PointWrap);

			sb.Draw(background, new Rectangle(0, 0, Game1.width, Game1.height), new Rectangle((int)animTimer, 0, Game1.width, Game1.height), Color.White);
			double heightOffset = Math.Sin(timer);

			heightOffset *= 20;

			Console.WriteLine("timeroffset: " + heightOffset);
			sb.Draw(wizZoom, new Rectangle((Game1.width - wizZoom.Width)/2, (Game1.height - wizZoom.Height) / 2 + (int)heightOffset, wizZoom.Width, wizZoom.Height), new Rectangle(0, 0, wizZoom.Width, wizZoom.Height), Color.White);
			sb.End();
		}
	}
}
