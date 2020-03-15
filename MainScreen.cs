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

		private RenderTarget2D mainTarget, internalTarget, lastFrame;
		private GraphicsDevice graphicsDevice;
		private Texture2D screenshot, lio;
		private Effect transition;
		private double timer;
		private Song song;
		private KeyboardState prevStateKb;
		private MouseState prevStateM;

		int currentFlashes, maxFlashes;

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

			this.graphicsDevice = graphicsDevice;
			internalTarget = new RenderTarget2D(graphicsDevice, Game1.width, Game1.height, false, SurfaceFormat.Color, DepthFormat.None, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);
			lastFrame = new RenderTarget2D(graphicsDevice, Game1.width, Game1.height, false, SurfaceFormat.Color, DepthFormat.None, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);
			mainTarget = final;
			//microgame = new Microgame(contentManager);
			//microgame = new Battle(contentManager, internalTarget, graphicsDevice, pp);
			screenshot = contentManager.Load<Texture2D>("Map/Transition");
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
					byte result = microgame.Update(dt, prevStateKb, prevStateM);

					if (result == 255)
					{
						curPhase = Phase.Transition;
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
				graphicsDevice.SetRenderTarget(lastFrame);
				//microgame.Draw(sb);
				sb.Begin();
				sb.Draw(mainTarget, new Rectangle(0, 0, Game1.width, Game1.height), new Rectangle(0, 0, Game1.width, Game1.height), Color.White);
				sb.End();
				sb.Begin(blendState: BlendState.Opaque);

				//Sidebars
				sb.Draw(lio, new Rectangle(0, 0, 1, Game1.height), new Rectangle(0, 0, Game1.width, Game1.height), Color.Transparent);
				sb.Draw(lio, new Rectangle(Game1.width - 1, 0, 1, Game1.height), new Rectangle(0, 0, Game1.width, Game1.height), Color.Transparent);

				sb.End();
				graphicsDevice.SetRenderTarget(mainTarget);
			}


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

						//Move from transition into the next game
						if (blackBar > Game1.height / 2)
						{ 
							//Reset timer variables for next time
							currentFlashes = 1;
							maxFlashes = 7;
							timer = 0.2;

							curPhase = Phase.InGame;
						}
							

						//sb.Draw(lio, new Rectangle(0, 0, Game1.width, Game1.height), Color.White);
						sb.Draw(internalTarget, new Rectangle(0, 0, Game1.width, Game1.height), Color.White);

						//Top and bottom bars
						sb.Draw(lio, new Rectangle(0, blackBar + Game1.height / 2, Game1.width, Game1.height), new Rectangle(0, 0, Game1.width, Game1.height), Color.Black);
						sb.Draw(lio, new Rectangle(0, -blackBar - Game1.height / 2, Game1.width, Game1.height), new Rectangle(0, 0, Game1.width, Game1.height), Color.Black);

						sb.End();

						sb.Begin(SpriteSortMode.Immediate);
						graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
						transition.CurrentTechnique.Passes[0].Apply();
						//sb.Draw(internalTarget, new Rectangle(0, 0, Game1.width, Game1.height), Color.White);

						sb.Draw(lastFrame, new Rectangle(-1, 0, Game1.width + 2, Game1.height), Color.White);
						sb.End();


					}
					else
					{
						sb.Begin(SpriteSortMode.Immediate);
						int flashCol = (int)(timer * 1700);

						sb.Draw(lastFrame, new Rectangle(0, 0, Game1.width, Game1.height), new Color(flashCol, flashCol, flashCol));

						//Draw black bars to the sides of lastFrame
						//TODO: Bake these into lastFrame
						//sb.Draw(lio, new Rectangle(0, 0, 1, Game1.height), new Rectangle(0, 0, Game1.width, Game1.height), Color.Black);
						//sb.Draw(lio, new Rectangle(Game1.width-1, 0, 1, Game1.height), new Rectangle(0, 0, Game1.width, Game1.height), Color.Black);

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