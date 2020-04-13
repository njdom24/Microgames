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
		private Texture2D background;
		private Texture2D wizZoom, continueIcon, lostIcon;
		private double animTimer;
		private double timer, fallTimer;
		public int continues, lastDrawnContinues;
		private Rectangle[] positions;
		private bool regain;
		//If continues != lastDrawnContinues, that means it was just lost and must be animated

		public BetweenGames(ContentManager contentManager, int continues = 3, bool lost = true, bool regain = false)
		{
			background = contentManager.Load<Texture2D>("Menus/TransBG_GRN");
			wizZoom = contentManager.Load<Texture2D>("Menus/wizZOOM");
			continueIcon = contentManager.Load<Texture2D>("Menus/TransitionButton");
			lostIcon = contentManager.Load<Texture2D>("Menus/TransitionButton_Lost");
			animTimer = 0.0;
			timer = 0.0;
			fallTimer = 0.0;
			this.regain = regain;
			this.continues = continues;

			if (lost)
				lastDrawnContinues = continues + 1;
			else
				lastDrawnContinues = continues;

			positions =  new Rectangle[3];
			int center = (Game1.width - continueIcon.Width) / 2;

			positions[0] = new Rectangle(center - continueIcon.Width * 2, Game1.height - continueIcon.Height, continueIcon.Width, continueIcon.Height);
			positions[1] = new Rectangle(center, Game1.height - continueIcon.Height, continueIcon.Width, continueIcon.Height);
			positions[2] = new Rectangle(center + continueIcon.Width * 2, Game1.height - continueIcon.Height, continueIcon.Width, continueIcon.Height);
		}

		void MiniScreen.Unload()
		{ 
		}

		byte MiniScreen.Update(GameTime dt, KeyboardState prevStateKb, MouseState prevStateM)
		{
			timer += dt.ElapsedGameTime.TotalSeconds * 3;
			animTimer += dt.ElapsedGameTime.TotalSeconds * 40;

			if(timer > 6)
				if (regain)
					fallTimer -= dt.ElapsedGameTime.TotalSeconds * 20;
				else
					fallTimer += dt.ElapsedGameTime.TotalSeconds * 20;
			
			if (animTimer > 4*40)
				return 255;
			return 1;
		}

		public void DrawContinues(SpriteBatch sb)
		{
			int center = (Game1.width - continueIcon.Width) / 2;

			for (int i = 0; i < continues; i++)
			{
				sb.Draw(continueIcon, positions[i], new Rectangle(0, 0, continueIcon.Width, continueIcon.Height), Color.White);
			}


			if (lastDrawnContinues > continues)
			{
				int x = positions[lastDrawnContinues - 1].X;
				sb.Draw(lostIcon, new Rectangle(x, Game1.height - continueIcon.Height + (int)(fallTimer * 10), continueIcon.Width, continueIcon.Height), new Rectangle(0, 0, continueIcon.Width, continueIcon.Height), Color.White);
			}
			else if (regain)
			{
				int x = positions[continues].X;
				int expectedHeight = positions[continues].Y;
				if ((int)(Game1.height * 1.3) + (int)(fallTimer * 10) <= expectedHeight)
					sb.Draw(continueIcon, new Rectangle(x, expectedHeight, continueIcon.Width, continueIcon.Height), new Rectangle(0, 0, continueIcon.Width, continueIcon.Height), Color.White);
				else
					sb.Draw(continueIcon, new Rectangle(x, (int)(Game1.height*1.3) + (int)(fallTimer * 10), continueIcon.Width, continueIcon.Height), new Rectangle(0, 0, continueIcon.Width, continueIcon.Height), Color.White);
			}
		}

		void MiniScreen.Draw(SpriteBatch sb)
		{
			sb.Begin(samplerState: SamplerState.PointWrap);

			sb.Draw(background, new Rectangle(0, 0, Game1.width, Game1.height), new Rectangle((int)animTimer, 0, Game1.width, Game1.height), Color.White);
			double heightOffset = Math.Sin(timer);

			heightOffset *= 20;

			Console.WriteLine("timeroffset: " + heightOffset);
			sb.Draw(wizZoom, new Rectangle((Game1.width - wizZoom.Width)/2, (Game1.height - wizZoom.Height) / 2 + (int)heightOffset, wizZoom.Width, wizZoom.Height), new Rectangle(0, 0, wizZoom.Width, wizZoom.Height), Color.White);

			if (animTimer < 4 * 39.5)
				DrawContinues(sb);
			
			sb.End();
		}
	}
}
