using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework.Media;
using System.IO;
using System.Text;

namespace RPG
{
	class MainScreen : Screen
	{
		private ContentManager contentManager, cm;
		private PresentationParameters pp;
		private MiniScreen microgame;

		private RenderTarget2D mainTarget, lastFrame, bufferTarget;
		private GraphicsDevice graphicsDevice;
		private Texture2D lio, stairClimber, pauseScreen, continueIcon, countdown, controls;
		private Menu pauseMenu;
		private Hud introText;
		private bool controlsShown;

		private Effect transition, paletteShader;
		private double timer, countdownTimer;
		private Song song;
		private KeyboardState prevStateKb;
		private MouseState prevStateM;

		int currentFlashes, maxFlashes;

		private enum Phase { Introduction, FinalMessage, MainMenu, InGame, Transition, BetweenGames, Paused };
		private Phase curPhase, prevPhase;
		private bool fromGame;
		private int continues, score;
		private Random random;

		private FileStream fs;

		public MainScreen(ContentManager contentManager, RenderTarget2D final, GraphicsDevice graphicsDevice, PresentationParameters pp, FileStream fs)
		{
			this.contentManager = contentManager;
			this.fs = fs;
			cm = new ContentManager(contentManager.ServiceProvider);
			cm.RootDirectory = contentManager.RootDirectory;
			this.pp = pp;

			//Console.WriteLine("BYTE: " + fs.ReadByte());
			if (fs.ReadByte().Equals(49))
				curPhase = Phase.MainMenu;
			else
				curPhase = Phase.Introduction;
			
			fromGame = false;
			//curPhase = Phase.BetweenGames;
			currentFlashes = 7;
			maxFlashes = 7;
			timer = 0.2;
			countdownTimer = 0.0;
			controlsShown = false;

			this.graphicsDevice = graphicsDevice;

			lastFrame = new RenderTarget2D(graphicsDevice, Game1.width + 2, Game1.height, false, SurfaceFormat.Color, DepthFormat.None, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);
			bufferTarget = new RenderTarget2D(graphicsDevice, Game1.width, Game1.height, false, SurfaceFormat.Color, DepthFormat.None, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);
			mainTarget = final;

			lio = contentManager.Load<Texture2D>("Menus/TransitionTest");
			//lio = contentManager.Load<Texture2D>("Battle/BackgroundL");
			stairClimber = contentManager.Load<Texture2D>("Map/TransitionAnim");
			pauseScreen = contentManager.Load<Texture2D>("Menus/PauseScreen");
			continueIcon = contentManager.Load<Texture2D>("Menus/TransitionButton");
			countdown = contentManager.Load<Texture2D>("Menus/Countdown");
			controls = contentManager.Load<Texture2D>("Menus/Controls");

			transition = contentManager.Load<Effect>("Map/transitions");
			transition.Parameters["time"].SetValue((float)timer);

			paletteShader = contentManager.Load<Effect>("Battle/BattleBG");
			paletteShader.Parameters["col_light"].SetValue(new Color(250, 231, 190).ToVector4());
			paletteShader.Parameters["col_extra"].SetValue(new Color(234, 80, 115).ToVector4());
			paletteShader.Parameters["col_med"].SetValue(new Color(113, 68, 123).ToVector4());
			paletteShader.Parameters["col_dark"].SetValue(new Color(176, 108, 57).ToVector4());

			song = contentManager.Load<Song>("Map/pkmnbtl2");
			continues = 3;
			score = 0;
			//MediaPlayer.Play(song);
			prevStateKb = Keyboard.GetState();
			prevStateM = Mouse.GetState();

			microgame = new TitleScreen(cm, paletteShader);

			random = new Random();

			introText = new Hud(new string[] { "Welcome to Microgames!\nClick the mouse or press the spacebar to continue.", "Because this is your first time, continue to see\nthe controls." }, cm, 30, 2, posY: -1, canClose: true);

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
			//return new Battle(cm, bufferTarget, graphicsDevice, pp);
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
				case Phase.Introduction:
					introText.Update(dt, prevStateKb, prevStateM);

					if (introText.messageComplete())
					{
						if (!controlsShown)
						{
							controlsShown = true;
							break;
						}
						else if (prevStateKb.IsKeyUp(Keys.Space) && Keyboard.GetState().IsKeyDown(Keys.Space) || (prevStateM.LeftButton == ButtonState.Pressed && Mouse.GetState().LeftButton == ButtonState.Released))
						{
							curPhase = Phase.FinalMessage;
							introText = new Hud(new string[] {"You can revisit that screen in Settings.\nHave fun!" }, cm, 30, 2, posY: -1, canClose: true);
						}
					}
					break;
				case Phase.FinalMessage:
					introText.Update(dt, prevStateKb, prevStateM);
					if (introText.messageComplete())
					{ 
						//TODO: Make an exit button
						if (prevStateKb.IsKeyUp(Keys.Space) && Keyboard.GetState().IsKeyDown(Keys.Space) || (prevStateM.LeftButton == ButtonState.Pressed && Mouse.GetState().LeftButton == ButtonState.Released))
						{
							byte[] bytes = Encoding.UTF8.GetBytes("1\n");
							fs.Write(bytes, 0, bytes.Length);
							fs.Close();

							curPhase = Phase.MainMenu;
						}
					}
					break;
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

					if(result != 254)
						countdownTimer += dt.ElapsedGameTime.TotalSeconds;

					//Won game (or time is up but animations are still playing)
					if (result == 255)
					{
						score++;
						countdownTimer = 0.0;
						microgame.Unload();
						cm.Dispose();
						cm = new ContentManager(contentManager.ServiceProvider);
						cm.RootDirectory = contentManager.RootDirectory;

						if (continues <= 2)
						{
							//Regain a life
							microgame = new BetweenGames(cm, score, continues, false, true);
							continues++;
						}
						else
							microgame = new BetweenGames(cm, score, continues, false);

						curPhase = Phase.Transition;
						fromGame = true;
					}
					//Lost game
					else if (countdownTimer >= 8 || result == 2)
					{
						countdownTimer = 0.0;

						microgame.Unload();
						cm.Dispose();
						cm = new ContentManager(contentManager.ServiceProvider);
						cm.RootDirectory = contentManager.RootDirectory;

						continues--;
						microgame = new BetweenGames(cm, score, continues, true);

						curPhase = Phase.Transition;
						fromGame = true;
						//BetweenGames phase will handle switching to TitleScreen on 0 continues
					}

					break;
				case Phase.BetweenGames:
					if (microgame.Update(dt, prevStateKb, prevStateM) == 255)
					{
						microgame.Unload();
						cm.Dispose();
						cm = new ContentManager(contentManager.ServiceProvider);
						cm.RootDirectory = contentManager.RootDirectory;

						if (continues > 0)
							microgame = ChooseGame();
						else
						{
							continues = 3;
							microgame = new TitleScreen(cm, paletteShader);
						}
		
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

						switch (pauseMenu.GetSelectionY(prevStateKb, prevStateM, mouseX, mouseY))
						{ 
							case 0:
								if (Mouse.GetState().LeftButton == ButtonState.Released && prevStateM.LeftButton == ButtonState.Pressed)
								{ 
									microgame.Unload();
									cm.Dispose();
									cm = new ContentManager(contentManager.ServiceProvider);
									cm.RootDirectory = contentManager.RootDirectory;

									microgame = new TitleScreen(cm, paletteShader);
									//microgame = new Battle(cm, mainTarget, graphicsDevice, pp);

									curPhase = Phase.Transition;
									fromGame = true;
								}
								break;
							case 2:
								int indX = pauseMenu.GetSelectionX(prevStateKb, prevStateM, mouseX, mouseY);
								if (indX > 0)
								{
									Console.WriteLine("tryna set color");
									SetColor(indX - 1);
								}
								break;
							default:
								break;	
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

				//Top/bottom bars
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
				case Phase.Introduction:
					
					graphicsDevice.SetRenderTarget(bufferTarget);
					microgame.Draw(sb);
					if (!introText.messageComplete())
					{
						sb.Begin();
						introText.Draw(sb);
						sb.End();
					}

					break;
				case Phase.FinalMessage:
					graphicsDevice.SetRenderTarget(bufferTarget);
					microgame.Draw(sb);

					sb.Begin();
					introText.Draw(sb);
					sb.End();
					break;	
				case Phase.MainMenu:
					graphicsDevice.SetRenderTarget(bufferTarget);
					microgame.Draw(sb);

					break;
				//Only needed for Battle
				case Phase.InGame:
					graphicsDevice.SetRenderTarget(bufferTarget);
					microgame.Draw(sb);
					sb.Begin();
					int offset = countdownTimer >= 8 ? 7*48 : ((int)countdownTimer) * 48;
					sb.Draw(countdown, new Rectangle(0, 0, 48, countdown.Height), new Rectangle(offset, 0, 48, countdown.Height), Color.White);
					sb.End();
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

			if (curPhase == Phase.Introduction || curPhase == Phase.FinalMessage)
			{
				Console.WriteLine("FSAFSAF");
				paletteShader.Techniques[3].Passes[0].Apply();
				sb.Draw(bufferTarget, new Rectangle(0, 0, Game1.width, Game1.height), new Rectangle(0, 0, Game1.width, Game1.height), Color.White);
				sb.End();

				sb.Begin(SpriteSortMode.Immediate);
				paletteShader.Techniques[1].Passes[0].Apply();

				sb.End();

				graphicsDevice.SetRenderTarget(bufferTarget);
				graphicsDevice.Clear(Color.Transparent);
				sb.Begin(blendState: BlendState.AlphaBlend);
				//The text is becoming white because the shader overrides it. Need to render the text to a buffer first

				
				sb.End();

				graphicsDevice.SetRenderTarget(mainTarget);
				sb.Begin(SpriteSortMode.Immediate);
				paletteShader.Techniques[1].Passes[0].Apply();
				sb.Draw(bufferTarget, new Rectangle(0, 0, Game1.width, Game1.height), new Rectangle(0, 0, Game1.width, Game1.height), Color.White);
				if (introText.messageComplete())
				{
					sb.Draw(controls, new Rectangle(0, 0, controls.Width, controls.Height), Color.White);
				}
				else
				{
					//sb.Begin();
					introText.Draw(sb);
					//sb.End();
				}
				sb.End();
			}
			else if (curPhase != Phase.Paused)
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