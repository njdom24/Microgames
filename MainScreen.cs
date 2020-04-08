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

		private RenderTarget2D mainTarget, lastFrame, bufferTarget;
		private GraphicsDevice graphicsDevice;
		private Texture2D lio, stairClimber, pauseScreen;
		private Menu pauseMenu;

		private Effect transition, paletteShader;
		private double timer;
		private Song song;
		private KeyboardState prevStateKb;
		private MouseState prevStateM;

		int currentFlashes, maxFlashes;

		private enum Phase { MainMenu, InGame, Transition, BetweenGames, Paused };
		private Phase curPhase, prevPhase;
		private bool fromGame;
		private Random random;

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
			bufferTarget = new RenderTarget2D(graphicsDevice, Game1.width, Game1.height, false, SurfaceFormat.Color, DepthFormat.None, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);
			mainTarget = final;

			lio = contentManager.Load<Texture2D>("Menus/TransitionTest");
			//lio = contentManager.Load<Texture2D>("Battle/BackgroundL");
			stairClimber = contentManager.Load<Texture2D>("Map/TransitionAnim");
			pauseScreen = contentManager.Load<Texture2D>("Menus/PauseScreen");

			transition = contentManager.Load<Effect>("Map/transitions");
			transition.Parameters["time"].SetValue((float)timer);

			paletteShader = contentManager.Load<Effect>("Battle/BattleBG");
			paletteShader.Parameters["col_light"].SetValue(new Color(192, 192, 128).ToVector4());
			paletteShader.Parameters["col_extra"].SetValue(new Color(160, 160, 96).ToVector4());
			paletteShader.Parameters["col_med"].SetValue(new Color(128, 128, 64).ToVector4());
			paletteShader.Parameters["col_dark"].SetValue(new Color(64, 64, 0).ToVector4());

			song = contentManager.Load<Song>("Map/pkmnbtl2");
			//MediaPlayer.Play(song);
			prevStateKb = Keyboard.GetState();
			prevStateM = Mouse.GetState();

			microgame = new TitleScreen(cm, paletteShader);

			random = new Random();

			pauseMenu = new Menu(contentManager, new string[] { "Return to Title", "Volume", "Palette", null, null, "P1", null, null, "P2", null, null, "P3", null, null, "P4", null, null, "P5" }, 3, 69, offsetX: Game1.width / 4, offsetY: Game1.height / 2, defaultSpacingX: 75);
		}

		void SetColor(int index)
		{
			Tuple<Vector4, Vector4, Vector4, Vector4> rgba = TitleScreen.palettes[index];
			paletteShader.Parameters["col_light"].SetValue(rgba.Item1);
			paletteShader.Parameters["col_extra"].SetValue(rgba.Item2);
			paletteShader.Parameters["col_med"].SetValue(rgba.Item3);
			paletteShader.Parameters["col_dark"].SetValue(rgba.Item4);
		}

		MiniScreen ChooseGame()
		{
			int num = random.Next(0,2);
			//return new Galaga(cm, bufferTarget, graphicsDevice);
			//return new Galaga(cm, bufferTarget, graphicsDevice, pp);
			switch (num)
			{
				case 0:
					return new Battle(cm, bufferTarget, graphicsDevice, pp);
				case 1:
					return new FallingApples(cm, graphicsDevice);
				default:
					return new Galaga(cm, bufferTarget, graphicsDevice, pp);
			}
		}

		void Screen.Update(GameTime dt)
		{
			if (curPhase != Phase.Paused && curPhase != Phase.MainMenu && Keyboard.GetState().IsKeyUp(Keys.Escape) && prevStateKb.IsKeyDown(Keys.Escape))
			{
				prevPhase = curPhase;
				curPhase = Phase.Paused;
			}
			else switch (curPhase) 
			{
				case Phase.MainMenu:
					switch (microgame.Update(dt, prevStateKb, prevStateM))
					{
						case 1:
							microgame.Unload();
							cm.Dispose();
							cm = new ContentManager(contentManager.ServiceProvider);
							cm.RootDirectory = contentManager.RootDirectory;
							//microgame = new Battle(cm, bufferTarget, graphicsDevice, pp);
							//microgame = new FallingApples(cm, graphicsDevice);
							microgame = ChooseGame();
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

						//microgame = new Battle(cm, bufferTarget, graphicsDevice, pp);
						//microgame = new FallingApples(cm, graphicsDevice);
						microgame = ChooseGame();
						curPhase = Phase.Transition;
						fromGame = false;
					}

					break;
				case Phase.Paused:
					Console.WriteLine("am paused");
					if (Keyboard.GetState().IsKeyUp(Keys.Escape) && prevStateKb.IsKeyDown(Keys.Escape))
					{
						curPhase = prevPhase;
					}
					else
					{ 
						MouseState state = Mouse.GetState();
						int mouseX = (int)(state.X * Game1.resMultiplier);
						int mouseY = (int)(state.Y * Game1.resMultiplier);
						pauseMenu.Update(dt, prevStateKb, prevStateM, mouseX, mouseY);

						if (pauseMenu.GetSelectionY(prevStateKb, prevStateM, mouseX, mouseY) == 2)
						{
							int indX = pauseMenu.GetSelectionX(prevStateKb, prevStateM, mouseX, mouseY);
							if (indX > 0)
							{
								Console.WriteLine("tryna set color");
								SetColor(indX - 1);
							}
						}
						//pauseMenu.GetSelection(prevStateKb, prevStateM, mouseX, mouseY);
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
				graphicsDevice.SetRenderTarget(bufferTarget);
				if (microgame is Battle)
				{
					((Battle)microgame).ChangeTarget(bufferTarget);
				}
			}



			switch (curPhase)
			{
				case Phase.MainMenu:
					graphicsDevice.SetRenderTarget(bufferTarget);
					microgame.Draw(sb);

					break;
				//Only needed for Battle
				case Phase.InGame:
					graphicsDevice.SetRenderTarget(bufferTarget);
					microgame.Draw(sb);
					//microgame.Draw(sb);
					break;
				case Phase.Transition:
					//Battle uses nested render targets
					graphicsDevice.SetRenderTarget(bufferTarget);
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

						sb.End();
						microgame.Draw(sb);
						sb.Begin();


						//Top and bottom bars, don't draw during transition screen
						if (!(microgame is BetweenGames))
						{ 
							sb.Draw(lio, new Rectangle(0, blackBar + Game1.height / 2, Game1.width, Game1.height / 2), new Rectangle(0, Game1.height/2, Game1.width, Game1.height/2), Color.White);
							sb.Draw(lio, new Rectangle(0, -blackBar , Game1.width, Game1.height / 2), new Rectangle(0, 0, Game1.width, Game1.height/2), Color.White);
						}

						sb.End();

						sb.Begin(SpriteSortMode.Immediate);
						graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
						transition.CurrentTechnique.Passes[0].Apply();
						//sb.Draw(internalTarget, new Rectangle(0, 0, Game1.width, Game1.height), Color.White);

						sb.Draw(lastFrame, new Rectangle(0, 0, Game1.width, Game1.height), new Rectangle(1, 0, Game1.width, Game1.height), Color.White);
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
					graphicsDevice.SetRenderTarget(bufferTarget);
					microgame.Draw(sb);

					break;
					
				case Phase.Paused:
					graphicsDevice.SetRenderTarget(bufferTarget);
					microgame.Draw(sb);
					break;
			}
			graphicsDevice.SetRenderTarget(mainTarget);
			sb.Begin(SpriteSortMode.Immediate);

			if (curPhase != Phase.Paused)
			{
				paletteShader.Techniques[1].Passes[0].Apply();
				sb.Draw(bufferTarget, new Rectangle(0, 0, Game1.width, Game1.height), new Rectangle(0, 0, Game1.width, Game1.height), Color.White);
				sb.End();
			}

			else
			{
				paletteShader.Techniques[3].Passes[0].Apply();
				sb.Draw(bufferTarget, new Rectangle(0, 0, Game1.width, Game1.height), new Rectangle(0, 0, Game1.width, Game1.height), Color.White);
				sb.End();

				sb.Begin(SpriteSortMode.Immediate);
				paletteShader.Techniques[1].Passes[0].Apply();
				sb.Draw(pauseScreen, new Rectangle(0, 0, pauseScreen.Width, pauseScreen.Height), Color.White);
				sb.End();

				graphicsDevice.SetRenderTarget(bufferTarget);
				graphicsDevice.Clear(Color.Transparent);
				sb.Begin(blendState:BlendState.AlphaBlend);
				//The text is becoming white because the shader overrides it. Need to render the text to a buffer first
				pauseMenu.Draw(sb);
				sb.End();

				graphicsDevice.SetRenderTarget(mainTarget);
				sb.Begin(SpriteSortMode.Immediate);
				paletteShader.Techniques[1].Passes[0].Apply();
				sb.Draw(bufferTarget, new Rectangle(0, 0, Game1.width, Game1.height), new Rectangle(0, 0, Game1.width, Game1.height), Color.White);
				sb.End();
			}
			




		}

	}
}