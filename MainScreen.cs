using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework.Media;

namespace RPG
{
	class MainScreen : Screen
	{
		private ContentManager contentManager;
		private PresentationParameters pp;
		private MiniScreen microgame;

		private World world;
		private RenderTarget2D mainTarget, internalTarget;
		private GraphicsDevice graphicsDevice;
		private Texture2D pkmn, lio;
		private Effect transition;
		private double timer;
		private Song song;
		private KeyboardState prevStateKb;
		private MouseState prevStateM;

		int currentFlashes, maxFlashes;
		bool darken;

		private enum Phase { MainMenu, InGame, Transition };
		private Phase curPhase;


		public MainScreen(ContentManager contentManager, RenderTarget2D final, GraphicsDevice graphicsDevice, PresentationParameters pp)
		{
			this.contentManager = contentManager;
			this.pp = pp;

			curPhase = Phase.MainMenu;
			currentFlashes = 1;
			maxFlashes = 7;
			timer = 0.2;
			darken = true;
			this.graphicsDevice = graphicsDevice;
			internalTarget = new RenderTarget2D(graphicsDevice, Game1.width, Game1.height, false, SurfaceFormat.Color, DepthFormat.None, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);
			mainTarget = final;
			//microgame = new Microgame(contentManager);
			//microgame = new Battle(contentManager, internalTarget, graphicsDevice, pp);
			pkmn = contentManager.Load<Texture2D>("Map/Transition");
			microgame = new TitleScreen(contentManager);
			lio = contentManager.Load<Texture2D>("Map/Hand");
			transition = contentManager.Load<Effect>("Map/transitions");
			transition.Parameters["time"].SetValue((float)timer);
			song = contentManager.Load<Song>("Map/pkmnbtl2");
			//MediaPlayer.Play(song);
			prevStateKb = Keyboard.GetState();
			prevStateM = Mouse.GetState();
		}

		void Screen.Update(GameTime dt)
		{
			switch (curPhase) 
			{
				case Phase.MainMenu:
					switch (microgame.Update(dt, prevStateKb, prevStateM))
					{
						case 1:
							microgame.Unload();
							microgame = new Battle(contentManager, internalTarget, graphicsDevice, pp);
							curPhase = Phase.Transition;
							break;
					}
					break;
				case Phase.Transition:
					if ((currentFlashes & 1) == 0)
						timer += dt.ElapsedGameTime.TotalSeconds;
					else
						timer -= dt.ElapsedGameTime.TotalSeconds;

					transition.Parameters["time"].SetValue((float)timer * 3);
					break;
				case Phase.InGame:
					microgame.Update(dt, prevStateKb, prevStateM);
					break;
					
			}

			//microgame.Update(dt, prevStateKb, prevStateM);
			prevStateKb = Keyboard.GetState();
			prevStateM = Mouse.GetState();
		}

		public void HandleInput(GameTime gameTime)
		{
			//Code to exit game?
		}

		void Screen.Draw(SpriteBatch sb)
		{
			//Render microgame internally
			//graphicsDevice.SetRenderTarget(internalTarget);
			microgame.Draw(sb);

			switch (curPhase)
			{
				case Phase.InGame:
					graphicsDevice.SetRenderTarget(mainTarget);
					sb.Begin();
					sb.Draw(internalTarget, new Rectangle(0, 0, Game1.width, Game1.height), Color.White);
					sb.End();
					break;
				case Phase.Transition:
					//Battle uses nested render targets
					graphicsDevice.SetRenderTarget(mainTarget);

					if (currentFlashes > maxFlashes)
					{
						sb.Begin();
						int blackBar = (int)(Math.Pow(2, (timer - 1.4) * 9));

						if (blackBar > Game1.height / 2)
							curPhase = Phase.InGame;

						sb.Draw(lio, new Rectangle(0, 0, Game1.width, Game1.height), Color.White);
						sb.Draw(internalTarget, new Rectangle(0, 0, Game1.width, Game1.height), Color.White);
						sb.Draw(lio, new Rectangle(0, blackBar + Game1.height / 2, Game1.width, Game1.height), new Rectangle(0, 0, Game1.width, Game1.height), Color.Black);
						sb.Draw(lio, new Rectangle(0, -blackBar - Game1.height / 2, Game1.width, Game1.height), new Rectangle(0, 0, Game1.width, Game1.height), Color.Black);

						sb.End();

						sb.Begin(SpriteSortMode.Immediate);
						graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
						transition.CurrentTechnique.Passes[0].Apply();
						//sb.Draw(internalTarget, new Rectangle(0, 0, Game1.width, Game1.height), Color.White);
						sb.Draw(pkmn, new Rectangle(-1, 0, Game1.width + 2, Game1.height), Color.White);
						sb.End();


					}
					else
					{
						sb.Begin(SpriteSortMode.Immediate);
						int flashCol = (int)(timer * 1700);

						sb.Draw(pkmn, new Rectangle(-1, 0, Game1.width + 2, Game1.height), new Color(flashCol, flashCol, flashCol));
						sb.End();

						if (currentFlashes == maxFlashes)
						{
							timer = 0;
							currentFlashes++;
						}
						else
						{
							if (flashCol > 255 && (currentFlashes & 1) == 0)
							{
								currentFlashes++;
							}
							else if (flashCol < 0 && currentFlashes <= maxFlashes && (currentFlashes & 1) == 1)
							{
								currentFlashes++;
							}
						}


					}
					break;
			}

			if (curPhase == Phase.MainMenu)
			{

			}
			else
			{
				
			}

			//new Color(Color.Gray, 255);


		}

	}
}