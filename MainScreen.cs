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
		private ContentManager contentManager, cm;
		private PresentationParameters pp;
		private MiniScreen microgame;

		private RenderTarget2D mainTarget, lastFrame;
		private GraphicsDevice graphicsDevice;
		private Texture2D lio, stairClimber;
		private Effect transition;
		private double timer;
		private Song song;
		private KeyboardState prevStateKb;
		private MouseState prevStateM;

		private readonly int stairWidth = 500;
		private readonly int stairHeight = 325;

		int currentFlashes, maxFlashes;

		private enum Phase { MainMenu, InGame, Transition, BetweenGames };
		private Phase curPhase;
		private bool fromGame;


		public MainScreen(ContentManager contentManager, RenderTarget2D final, GraphicsDevice graphicsDevice, PresentationParameters pp)
		{
			this.contentManager = contentManager;
			cm = new ContentManager(contentManager.ServiceProvider);
			cm.RootDirectory = contentManager.RootDirectory;
			this.pp = pp;

			curPhase = Phase.MainMenu;
			fromGame = false;
			//curPhase = Phase.BetweenGames;
			currentFlashes = 7;
			maxFlashes = 7;
			timer = 0.2;

			this.graphicsDevice = graphicsDevice;

			lastFrame = new RenderTarget2D(graphicsDevice, Game1.width + 2, Game1.height, false, SurfaceFormat.Color, DepthFormat.None, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);
			mainTarget = final;

			microgame = new TitleScreen(cm);
			lio = contentManager.Load<Texture2D>("Map/Hand");
			stairClimber = contentManager.Load<Texture2D>("Map/TransitionAnim");

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
							cm.Dispose();
							cm = new ContentManager(contentManager.ServiceProvider);
							cm.RootDirectory = contentManager.RootDirectory;
							microgame = new Battle(cm, mainTarget, graphicsDevice, pp);
							//microgame = new TitleScreen(cm);
							curPhase = Phase.Transition;
							break;
					}
					break;
				case Phase.Transition:
					if ((currentFlashes & 1) == 0)
						timer += dt.ElapsedGameTime.TotalSeconds;
					else
						timer -= dt.ElapsedGameTime.TotalSeconds;

					transition.Parameters["time"].SetValue((float)timer * 4);

					//Lets the transition animation start while borders are still animating
					if (microgame is BetweenGames && currentFlashes > maxFlashes)
						microgame.Update(dt, prevStateKb, prevStateM);
					
					break;
				case Phase.InGame:
					byte result = microgame.Update(dt, prevStateKb, prevStateM);

					if (result == 255)
					{
						microgame.Unload();
						cm.Dispose();
						cm = new ContentManager(contentManager.ServiceProvider);
						cm.RootDirectory = contentManager.RootDirectory;

						microgame = new BetweenGames(cm);
						//microgame = new Battle(cm, mainTarget, graphicsDevice, pp);

						curPhase = Phase.Transition;
						fromGame = true;
					}

					break;
				case Phase.BetweenGames:
					if (microgame.Update(dt, prevStateKb, prevStateM) == 255)
					{
						microgame.Unload();
						cm.Dispose();
						cm = new ContentManager(contentManager.ServiceProvider);
						cm.RootDirectory = contentManager.RootDirectory;

						microgame = new Battle(cm, mainTarget, graphicsDevice, pp);

						curPhase = Phase.Transition;
						fromGame = false;
					}

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

			//Render black background, then render the last frame in the middle, leaving 2 1-pixel borders on the sides
			if (curPhase != Phase.Transition)
			{
				if (microgame is Battle)
				{
					((Battle)microgame).ChangeTarget(lastFrame);
				}
				graphicsDevice.SetRenderTarget(lastFrame);
				//microgame.Draw(sb);
				microgame.Draw(sb);
				sb.Begin(blendState: BlendState.Opaque);

				//Sidebars
				sb.Draw(lio, new Rectangle(0, 0, 1, Game1.height), new Rectangle(0, 0, Game1.width, Game1.height), Color.Transparent);
				sb.Draw(lio, new Rectangle(Game1.width + 1, 0, 1, Game1.height), new Rectangle(0, 0, Game1.width, Game1.height), Color.Transparent);

				sb.End();
				graphicsDevice.SetRenderTarget(mainTarget);
				if (microgame is Battle)
				{
					((Battle)microgame).ChangeTarget(mainTarget);
				}
			}



			switch (curPhase)
			{
				case Phase.MainMenu:
					graphicsDevice.SetRenderTarget(mainTarget);
					microgame.Draw(sb);

					break;
				//Only needed for Battle
				case Phase.InGame:
					graphicsDevice.SetRenderTarget(mainTarget);
					microgame.Draw(sb);
					//microgame.Draw(sb);
					break;
				case Phase.Transition:
					//Battle uses nested render targets
					graphicsDevice.SetRenderTarget(mainTarget);
					microgame.Draw(sb);

					if (currentFlashes > maxFlashes)
					{
						sb.Begin();
						int blackBar = (int)(Math.Pow(2, (timer - 1.4) * 14));

						//Move from transition into the next game
						if (blackBar > Game1.height / 2)
						{ 
							//Reset timer variables for next time
							currentFlashes = 7;
							maxFlashes = 7;
							timer = 0.2;


							if (microgame is TitleScreen)
								curPhase = Phase.MainMenu;
							else if (fromGame)
							{
								fromGame = false;
								curPhase = Phase.BetweenGames;
							}
							else
								curPhase = Phase.InGame;
							sb.End();
							//microgame.Draw(sb);
							sb.Begin();
						}


						//sb.Draw(lio, new Rectangle(0, 0, Game1.width, Game1.height), Color.White);
						//sb.Draw(internalTarget, new Rectangle(0, 0, Game1.width, Game1.height), Color.White);


						sb.End();
						microgame.Draw(sb);
						sb.Begin();


						//Top and bottom bars
						sb.Draw(lio, new Rectangle(0, blackBar + Game1.height / 2, Game1.width, Game1.height), new Rectangle(0, 0, Game1.width, Game1.height), Color.Black);
						sb.Draw(lio, new Rectangle(0, -blackBar - Game1.height / 2, Game1.width, Game1.height), new Rectangle(0, 0, Game1.width, Game1.height), Color.Black);

						sb.End();

						sb.Begin(SpriteSortMode.Immediate);
						graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
						transition.CurrentTechnique.Passes[0].Apply();
						//sb.Draw(internalTarget, new Rectangle(0, 0, Game1.width, Game1.height), Color.White);

						sb.Draw(lastFrame, new Rectangle(0, 0, Game1.width, Game1.height), new Rectangle(1, 0, Game1.width, Game1.height), Color.Red);
						sb.End();


					}
					else
					{
						sb.Begin(SpriteSortMode.Immediate);
						int flashCol = (int)(timer * 1700);

						sb.Draw(lastFrame, new Rectangle(0, 0, Game1.width, Game1.height), new Rectangle(1, 0, Game1.width, Game1.height), new Color(flashCol, flashCol, flashCol));

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
				case Phase.BetweenGames:
					graphicsDevice.SetRenderTarget(mainTarget);
					microgame.Draw(sb);

					break;
			}

			//new Color(Color.Gray, 255);


		}

	}
}