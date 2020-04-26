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
		private Texture2D lio, stairClimber, pauseScreen, areYouSure, continueIcon, countdown, controls;
		private Menu pauseMenu;
		private Hud introText;
		private bool controlsShown, controlHint, controlRevisit, exitConfirm;

		private Effect transition, paletteShader;
		private double timer, countdownTimer, timerMult, betweenGamesOffset;
		private Color countdownColor;
		private Song song;
		private KeyboardState prevStateKb;
		private MouseState prevStateM;

		int currentFlashes, maxFlashes;

		private enum Phase { Introduction, PracticeUnlock, FinalMessage, MainMenu, InGame, Transition, BetweenGames, Paused };
		private Phase curPhase, prevPhase;
		private bool fromGame, practiceMode, practiceUnlocked, practiceIntro;
		private int continues, score;
		private Random random;
		private int lastGame, timesRepeated;

		private FileStream fs;
		private Game game;
		private Button exitButton, pauseButton, resumeButton, noButton, yesButton, backButton;

		public MainScreen(ContentManager contentManager, RenderTarget2D final, GraphicsDevice graphicsDevice, PresentationParameters pp, FileStream fs, Game game)
		{
			this.game = game;
			this.contentManager = contentManager;
			this.fs = fs;
			cm = new ContentManager(contentManager.ServiceProvider);
			cm.RootDirectory = contentManager.RootDirectory;
			this.pp = pp;
			paletteShader = contentManager.Load<Effect>("Battle/BattleBG");
			introText = new Hud(new string[] { "Welcome to Microgames!\nClick the mouse or press the spacebar to continue.", "Because this is your first time, continue to see\nthe controls." }, cm, 30, 2, posY: -1, canClose: true);
			if (fs.ReadByte().Equals('P'))
			{
				curPhase = Phase.MainMenu;
				fs.Close();
				practiceUnlocked = GetSaveElem("Practice") == 1;
				SetColor(GetSaveElem("Palette"));//Restore palette from save
				introText.finishText();
			}
			else
			{
				paletteShader.Parameters["col_light"].SetValue(new Color(250, 231, 190).ToVector4());
				paletteShader.Parameters["col_extra"].SetValue(new Color(234, 80, 115).ToVector4());
				paletteShader.Parameters["col_med"].SetValue(new Color(113, 68, 123).ToVector4());
				paletteShader.Parameters["col_dark"].SetValue(new Color(176, 108, 57).ToVector4());

				practiceUnlocked = false;
				curPhase = Phase.Introduction;
			}
			
			fromGame = false;
			exitConfirm = false;
			//curPhase = Phase.BetweenGames;
			currentFlashes = 7;
			maxFlashes = 7;
			timer = 0.2;
			countdownTimer = 0.0;
			timerMult = 1.0;
			betweenGamesOffset = 0.0;
			countdownColor = Color.White;

			controlsShown = false;
			controlRevisit = false;
			controlHint = false;

			this.graphicsDevice = graphicsDevice;
			practiceMode = false;

			lastFrame = new RenderTarget2D(graphicsDevice, Game1.width + 2, Game1.height, false, SurfaceFormat.Color, DepthFormat.None, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);
			bufferTarget = new RenderTarget2D(graphicsDevice, Game1.width, Game1.height, false, SurfaceFormat.Color, DepthFormat.None, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);
			mainTarget = final;

			lio = contentManager.Load<Texture2D>("Menus/TransitionTest");
			stairClimber = contentManager.Load<Texture2D>("Map/TransitionAnim");
			pauseScreen = contentManager.Load<Texture2D>("Menus/PauseScreen");
			areYouSure = contentManager.Load<Texture2D>("Menus/AreYouSure");
			continueIcon = contentManager.Load<Texture2D>("Menus/TransitionButton");
			countdown = contentManager.Load<Texture2D>("Menus/Countdown");

			if (Environment.OSVersion.Platform.ToString().Contains("Win"))
				controls = contentManager.Load<Texture2D>("Menus/Controls");
			else
				controls = contentManager.Load<Texture2D>("Menus/Controls_Unix");

			transition = contentManager.Load<Effect>("Map/transitions");
			transition.Parameters["time"].SetValue((float)timer);

			song = contentManager.Load<Song>("Map/pkmnbtl2");
			continues = 3;
			score = 0;
			//MediaPlayer.Play(song);
			prevStateKb = Keyboard.GetState();
			prevStateM = Mouse.GetState();

			lastGame = -1;
			timesRepeated = 0;

			practiceIntro = false;

			microgame = new TitleScreen(cm, paletteShader, practiceUnlocked);

			random = new Random();

			exitButton = new Button(contentManager, 4, 1, text: "OK");
			pauseButton = new Button(contentManager, 4, 1, text: "Pause");
			resumeButton = new Button(contentManager, 4, 1, text: "Resume");

			noButton = new Button(contentManager, Game1.width/2 - 60, Game1.height/2 + 20, text: "No");
			yesButton = new Button(contentManager, Game1.width/2 + 20, Game1.height/2 + 20, text: "Yes");
			backButton = new Button(contentManager, 4, 1, text: "Back");

			pauseMenu = new Menu(contentManager, new string[] { "Palette", "Return to Title", "P1", null, "P2", null, "P3", null, "P4", null, "P5", null }, 2, 69, offsetX: Game1.width / 4, offsetY: Game1.height / 2, defaultSpacingX: 75);
		}

		int GetSaveElem(string elem)
		{
			elem += ": ";
			StreamReader file = new StreamReader("save.txt");
			string fileText = file.ReadToEnd();
			file.Close();

			int elemIndex = fileText.IndexOf(elem, 0, StringComparison.CurrentCulture) + elem.Length;
			int endIndex = fileText.IndexOf('\n', elemIndex);
			if (endIndex == -1)
				endIndex = fileText.Length - 1;

			return int.Parse(fileText.Substring(elemIndex, endIndex - elemIndex));
		}

		public static void UpdateSaveElem(string elem, int val)
		{
			elem += ": ";
			StreamReader file = new StreamReader("save.txt");
			string fileText = file.ReadToEnd();
			file.Close();

			StreamWriter outFile = new StreamWriter("save.txt");

			int beforeText = fileText.IndexOf(elem, 0, StringComparison.CurrentCulture);
			int afterText = fileText.IndexOf('\n', beforeText);
			if (afterText == -1)
				afterText = fileText.Length - 1;
			int afterIndex = beforeText + elem.Length;

			string outputText = fileText.Substring(0, beforeText) + elem + val + fileText.Substring(afterText);
			outFile.Write(outputText);

			outFile.Close();
		}

		double GetLossRatio(string game)
		{
			game += ": ";

			StreamReader file = new StreamReader("save.txt");
			string fileText = file.ReadToEnd();
			file.Close();

			int afterIndex = fileText.IndexOf(game, 0, StringComparison.CurrentCulture) + game.Length;
			int afterWins = fileText.IndexOf('W', afterIndex);
			string wins = fileText.Substring(afterIndex, afterWins - afterIndex);

			int lossIndex = fileText.IndexOf('L', afterWins + 2);
			string losses = fileText.Substring(afterWins + 2, lossIndex - afterWins - 2);
			double winCount = int.Parse(wins);
			double lossCount = int.Parse(losses);

			return lossCount / (winCount + lossCount);
		}

		//TODO: Make other stuff use this
		MiniScreen GetMostLost()
		{
			if (GetLossRatio("Battle") > GetLossRatio("Apples"))
			{
				Battle b = new Battle(cm, bufferTarget, graphicsDevice, pp, score);
				lio = score < 10 ? lio = contentManager.Load<Texture2D>("Battle/Enemies/Explainer_" + b.GetEnemyName())
					: contentManager.Load<Texture2D>("Menus/TransitionTest");
				
				return b;
			}
			else
			{
				lio = contentManager.Load<Texture2D>("Menus/TransitionApples");
				return new FallingApples(cm, score);
			}
		}

		void UpdateSaveGame(string game, bool won)
		{
			game += ": ";
			StreamReader file = new StreamReader("save.txt");
			string fileText = file.ReadToEnd();
			file.Close();

			StreamWriter outFile = new StreamWriter("save.txt");

			int afterIndex = fileText.IndexOf(game, 0, StringComparison.CurrentCulture) + game.Length;
			int afterWins = fileText.IndexOf('W', afterIndex);
			string wins = fileText.Substring(afterIndex, afterWins - afterIndex);
			string losses = fileText.Substring(afterWins + 2, fileText.IndexOf('L', afterWins + 2) - afterWins - 2);
			int winCount = int.Parse(wins);
			int lossCount = int.Parse(losses);

			if (won)
				winCount++;
			else
				lossCount++;

			string beforeEdit = fileText.Substring(0, afterIndex);
			string afterEdit = fileText.Substring(fileText.IndexOf('L', afterWins + 2));

			outFile.Write(beforeEdit + winCount + "W " + lossCount + afterEdit);
			outFile.Close();
		}

		void SetColor(int index)
		{
			UpdateSaveElem("Palette", index);
			Tuple<Vector4, Vector4, Vector4, Vector4> rgba = TitleScreen.palettes[index];
			paletteShader.Parameters["col_light"].SetValue(rgba.Item1);
			paletteShader.Parameters["col_extra"].SetValue(rgba.Item2);
			paletteShader.Parameters["col_med"].SetValue(rgba.Item3);
			paletteShader.Parameters["col_dark"].SetValue(rgba.Item4);
		}

		MiniScreen ChooseGame()
		{
			if (practiceMode)
			{
				return GetMostLost();
			}

			//Generates 0, 1
			int num = random.Next(0,2);

			switch (num)
			{
				case 0:
				{
					if (lastGame == 0)
					{
						timesRepeated++;
						if (timesRepeated >= 2)
							goto case 1;
					}
					else
						timesRepeated = 0;

					lastGame = 0;

					Battle b = new Battle(cm, bufferTarget, graphicsDevice, pp, score);
					lio = score < 20 ? lio = contentManager.Load<Texture2D>("Battle/Enemies/Explainer_" + b.GetEnemyName())
						: contentManager.Load<Texture2D>("Menus/TransitionTest");
					countdownColor = Color.White;

					return b;

				}
				case 1:
				{
					if (lastGame == 1)
					{
						timesRepeated++;
						if (timesRepeated >= 2)
							goto case 0;
					}
					else
						timesRepeated = 0;

					lastGame = 1;
					lio = score < 20 ? lio = contentManager.Load<Texture2D>("Menus/TransitionApples_2")
						: contentManager.Load<Texture2D>("Menus/TransitionApples");
					countdownColor = Color.Purple;

					return new FallingApples(cm, score);
				}
				default:
					return new Galaga(cm, bufferTarget, graphicsDevice, pp);
			}
		}

		void Screen.Update(GameTime dt)
		{
			if (!prevStateKb.GetPressedKeys().Equals(Keyboard.GetState().GetPressedKeys()))
				game.IsMouseVisible = false;

			MouseState state = Mouse.GetState();

			if (prevStateM.X != state.X || prevStateM.Y != state.Y)
				game.IsMouseVisible = true;

			int mouseX = (int)(state.X * Game1.resMultiplier);
			int mouseY = (int)(state.Y * Game1.resMultiplier);

			pauseButton.Update(mouseX, mouseY);
			if ((curPhase == Phase.Transition || curPhase == Phase.BetweenGames) && 
			    ((Keyboard.GetState().IsKeyUp(Keys.Escape) && prevStateKb.IsKeyDown(Keys.Escape))
			     || (curPhase != Phase.Paused && pauseButton.IsPressed(prevStateM))))
			{
				prevStateM = state;
				prevPhase = curPhase;
				curPhase = Phase.Paused;
				pauseMenu.SetSelectionY(0);
				//timer = 0.2;
			}
			else switch (curPhase) 
			{
				case Phase.Introduction:
					exitButton.Update(mouseX, mouseY);
					introText.Update(dt, prevStateKb, prevStateM);

					if (controlHint)
					{ 
						if ((prevStateKb.IsKeyUp(Keys.Escape) && Keyboard.GetState().IsKeyDown(Keys.Escape)) || exitButton.IsPressed(prevStateM))
						{
							controlHint = false;
							introText.finishMessage();
								introText.finishText();
							if (controlRevisit)
								curPhase = Phase.MainMenu;
							else
							{
								curPhase = Phase.FinalMessage;
								introText = new Hud(new string[] { "You can revisit that screen in Settings.\nHave fun!" }, cm, 30, 2, posY: -1, canClose: true);
							}
						}
					}
					if (introText.messageComplete())
					{
						if (controlHint)
						{
							controlHint = false;
							//exitButton.Update(mouseX, mouseY);

							if ((prevStateKb.IsKeyUp(Keys.Escape) && Keyboard.GetState().IsKeyDown(Keys.Escape)) || exitButton.IsPressed(prevStateM))
							{
								if (controlRevisit)
									curPhase = Phase.MainMenu;
								else
								{
									curPhase = Phase.FinalMessage;
									introText = new Hud(new string[] { "You can revisit that screen in Settings.\nHave fun!" }, cm, 30, 2, posY: -1, canClose: true);
								}
							}

						}
						else if (!controlsShown)
						{
							controlsShown = true;
							break;
						}
						else
						{
							//exitButton.Update(mouseX, mouseY);

							//Hit the wrong buttons to continue
							if ((prevStateKb.IsKeyUp(Keys.Space) && Keyboard.GetState().IsKeyDown(Keys.Space)) ||
								!exitButton.IsPressed(prevStateM) && (prevStateM.LeftButton == ButtonState.Pressed && Mouse.GetState().LeftButton == ButtonState.Released))
							{
								controlHint = true;
								introText = new Hud(new string[] { "You can click the button in the upper left corner\nor hit the Escape key to exit this screen." }, cm, 30, 2, posY: -1, canClose: true);
							}
							else if ((prevStateKb.IsKeyUp(Keys.Escape) && Keyboard.GetState().IsKeyDown(Keys.Escape)) || exitButton.IsPressed(prevStateM))
							{
								if (controlRevisit)
									curPhase = Phase.MainMenu;
								else
								{
									curPhase = Phase.FinalMessage;
									introText = new Hud(new string[] { "You can revisit that screen in Settings.\nHave fun!" }, cm, 30, 2, posY: -1, canClose: true);
								}
							}
						}
					}
					else
					{ 
					}
						break;
				case Phase.FinalMessage:
					if (controlRevisit)
					{
						curPhase = Phase.MainMenu;
						break;
					}
					introText.Update(dt, prevStateKb, prevStateM);
					if (introText.messageComplete())
					{ 
						//TODO: Make an exit button
						if (prevStateKb.IsKeyUp(Keys.Space) && Keyboard.GetState().IsKeyDown(Keys.Space) || (prevStateM.LeftButton == ButtonState.Pressed && Mouse.GetState().LeftButton == ButtonState.Released))
						{
							byte[] bytes = Encoding.UTF8.GetBytes("Palette: 0\nPractice: 0\nBattle: 0W 0L\nApples: 0W 0L\n");
							fs.Write(bytes, 0, bytes.Length);
							fs.Close();

							curPhase = Phase.MainMenu;
						}
					}
					break;
				case Phase.PracticeUnlock:
					introText.Update(dt, prevStateKb, prevStateM);
						if (introText.messageComplete())
						{
							//TODO: Make an exit button
							if (prevStateKb.IsKeyUp(Keys.Space) && Keyboard.GetState().IsKeyDown(Keys.Space) || (prevStateM.LeftButton == ButtonState.Pressed && Mouse.GetState().LeftButton == ButtonState.Released))
							{
								UpdateSaveElem("Practice", 1);
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
							practiceMode = false;
							microgame = ChooseGame();
							//microgame = new TitleScreen(cm);
							curPhase = Phase.Transition;
							break;
						case 2:
							microgame.Unload();
							cm.Dispose();
							cm = new ContentManager(contentManager.ServiceProvider);
							cm.RootDirectory = contentManager.RootDirectory;
							practiceMode = true;
							microgame = GetMostLost();
							//microgame = new TitleScreen(cm);
							curPhase = Phase.Transition;
							break;
						case 3:
							game.Exit();
							break;
						case 4:
							controlRevisit = true;
							curPhase = Phase.Introduction;
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

					if (result != 254)
						countdownTimer += dt.ElapsedGameTime.TotalSeconds * timerMult;

					//Won game
					if (result == 255)
					{
						UpdateSaveGame(microgame.ToString(), true);
						score++;
						if (score == 10)
							timerMult = 1.25;
						else if (score == 20)
							timerMult = 1.5;
						
						countdownTimer = 0.0;
						microgame.Unload();
						cm.Dispose();
						cm = new ContentManager(contentManager.ServiceProvider);
						cm.RootDirectory = contentManager.RootDirectory;

						if (continues <= 2)
						{
							if (score % 10 == 0)
							{
								microgame = new BetweenGames(cm, score, betweenGamesOffset, pauseButton, continues, false, true);
								continues++;
							}
							else
								microgame = new BetweenGames(cm, score, betweenGamesOffset, pauseButton, continues, false, false);
						}
						else
							microgame = new BetweenGames(cm, score, betweenGamesOffset, pauseButton, continues, false);

						curPhase = Phase.Transition;
						fromGame = true;
					}
					//Lost game
					else if (countdownTimer >=11 || result == 2)
					{
						UpdateSaveGame(microgame.ToString(), false);
						countdownTimer = 0.0;

						microgame.Unload();
						cm.Dispose();
						cm = new ContentManager(contentManager.ServiceProvider);
						cm.RootDirectory = contentManager.RootDirectory;

						continues--;
						microgame = new BetweenGames(cm, score, betweenGamesOffset, pauseButton, continues, true);

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
						{
							betweenGamesOffset = ((BetweenGames)microgame).GetAnimOffset();
							microgame = ChooseGame();
						}
						else
						{
							//Out of continues
							score = 0;
							timerMult = 1.0;
							timer = 0.2;
							betweenGamesOffset = 0.0;
							lio = contentManager.Load<Texture2D>("Menus/TransitionQuit");
							continues = 3;

							//First time getting a game over
							if (!practiceUnlocked)
							{
								practiceIntro = true;
								practiceUnlocked = true;
								introText = new Hud(new string[] { "You've unlocked practice mode!\nYou can play the games you've lost more frequently.", "Check it out in the menu!" }, cm, 30, 2, posY: -1, canClose: true);
							}
							microgame = new TitleScreen(cm, paletteShader, practiceUnlocked);
						}
		
						curPhase = Phase.Transition;
						fromGame = false;
					}

					break;
				case Phase.Paused:
					resumeButton.Update(mouseX, mouseY);
					if (!exitConfirm && (Keyboard.GetState().IsKeyUp(Keys.Escape) && prevStateKb.IsKeyDown(Keys.Escape) || resumeButton.IsPressed(prevStateM)))
					{
						prevStateM = state;
						curPhase = prevPhase;
					}
					else
					{ 
						pauseMenu.Update(dt, prevStateKb, prevStateM, mouseX, mouseY);

						if (!exitConfirm)
							switch (pauseMenu.GetSelectionY(prevStateKb, prevStateM, mouseX, mouseY))
							{
								case 0:
									int indX = pauseMenu.GetSelectionX(prevStateKb, prevStateM, mouseX, mouseY);
									if (indX > 0)
									{
										Console.WriteLine("tryna set color");
										SetColor(indX - 1);
									}
									break;
								case 1:
									if (Mouse.GetState().LeftButton == ButtonState.Released && prevStateM.LeftButton == ButtonState.Pressed ||
										Keyboard.GetState().IsKeyUp(Keys.Space) && prevStateKb.IsKeyDown(Keys.Space))
									{
										exitConfirm = true;
									}
									break;

								default:
									break;
							}
						else
						{
							//Check for OK, No
							yesButton.Update(mouseX, mouseY);
							noButton.Update(mouseX, mouseY);
							backButton.Update(mouseX, mouseY);

							if (yesButton.IsPressed(prevStateM) || (prevStateKb.IsKeyDown(Keys.Enter)) && Keyboard.GetState().IsKeyUp(Keys.Enter))
							{
								microgame.Unload();
								cm.Dispose();
								cm = new ContentManager(contentManager.ServiceProvider);
								cm.RootDirectory = contentManager.RootDirectory;

								lio = contentManager.Load<Texture2D>("Menus/TransitionQuit");
								if (score < 10)
									timer = 1.8;
								else
									timer = 1.4;
								betweenGamesOffset = 0.0;
								microgame = new TitleScreen(cm, paletteShader, practiceUnlocked);
								//microgame = new Battle(cm, mainTarget, graphicsDevice, pp);

								curPhase = Phase.Transition;
								fromGame = true;
							}
							else if (noButton.IsPressed(prevStateM) || yesButton.IsPressed(prevStateM) ||
							         ((prevStateKb.IsKeyDown(Keys.Escape)) && Keyboard.GetState().IsKeyUp(Keys.Escape)))
							{
								exitConfirm = false;
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
				case Phase.PracticeUnlock:
					graphicsDevice.SetRenderTarget(bufferTarget);
					microgame.Draw(sb);

					sb.Begin();
					introText.Draw(sb);
					sb.End();
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
					int offset = countdownTimer >= 11 ? 7*48 : ((int)countdownTimer) * 48;
					sb.Draw(countdown, new Rectangle(2, 2, 48, countdown.Height), new Rectangle(offset, 0, 48, countdown.Height), countdownColor);
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

						//1.8 is the amount of seconds the preview is active
						int blackBar = (int)(Math.Pow(2, (timer - 3.3 / timerMult) * 14));

						//Move from transition into the next game
						if (blackBar > Game1.height / 2)
						{ 
							//Reset timer variables for next time
							currentFlashes = 7;
							maxFlashes = 7;
							timer = 0.2;

							if (microgame is TitleScreen)
							{
								if (practiceIntro)
									curPhase = Phase.PracticeUnlock;
								else
									curPhase = Phase.MainMenu;
							}
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

			if (curPhase == Phase.Introduction || curPhase == Phase.FinalMessage || curPhase == Phase.PracticeUnlock)
			{
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
				if (introText.messageComplete() || controlHint)
					exitButton.Draw(sb);
				
				sb.End();

				graphicsDevice.SetRenderTarget(mainTarget);
				sb.Begin(SpriteSortMode.Immediate);
				paletteShader.Techniques[1].Passes[0].Apply();
				sb.Draw(bufferTarget, new Rectangle(0, 0, Game1.width, Game1.height), new Rectangle(0, 0, Game1.width, Game1.height), Color.White);
				if (introText.messageComplete())
				{
					sb.Draw(controls, new Rectangle(0, 0, controls.Width, controls.Height), Color.White);

				}
				else if (controlHint)
				{ 
					sb.Draw(controls, new Rectangle(0, 0, controls.Width, controls.Height), Color.White);
					introText.Draw(sb);
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
				if (exitConfirm)
				{ 
					sb.Draw(areYouSure, new Rectangle(0, 0, pauseScreen.Width, pauseScreen.Height), Color.White);
				}
				else
					sb.Draw(pauseScreen, new Rectangle(0, 0, pauseScreen.Width, pauseScreen.Height), Color.White);
				sb.End();

				graphicsDevice.SetRenderTarget(bufferTarget);
				graphicsDevice.Clear(Color.Transparent);
				sb.Begin(blendState:BlendState.AlphaBlend);
				//The text is becoming white because the shader overrides it. Need to render the text to a buffer first
				if (exitConfirm)
				{
					backButton.Draw(sb);
					yesButton.Draw(sb);
					noButton.Draw(sb);
				}
				else
				{
					pauseMenu.Draw(sb);
					resumeButton.Draw(sb);
				}
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