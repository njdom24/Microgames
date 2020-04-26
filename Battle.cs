using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RPG
{
	class Battle : MiniScreen
	{
		private Enemy flasher;
		private double flashTimer;
		private float timerMult;
		private byte flashCounter;

		private Enemy enemy;
		private Battler travis;

		private Texture2D blackRect;
		private Texture2D background;
		private Texture2D background2;
		private Texture2D youWon;
		private bool victory;
		private bool exitReady;
		private double victoryTimer;
		private Color victoryColor, flashColor;
		
		private Effect effect;
		private Effect flash;
		private double bgTimer;
		private double waiterTimer;
		
		private RenderTarget2D firstEffect, final;
		private GraphicsDevice graphicsDevice;
		private Hud text;
		private Hud commandName;
		private ContentManager content;
		private int MultiSampleCount;
		private Icons options;

		private World world;
		private Combatant waiter;

		private int offsetHeightTop;
		private int offsetHeightBottom;

		private float secondsPerBeat;
		private double combatTimer;
		private double threshHold;
		private Texture2D combatIndicator;
		private Texture2D palette;

		private int playerMove, enemyMove;

		private enum Phase {IntroPhase, PlayerPhase, SelectTarget, EnemyPhase, AttackPhase, AnimPhase, BlinkPhase, PlayerDeathPhase, EnemyDeathPhase, YouWin};
		private Phase curPhase;
		private double turnWaiter;

		private Animation magicAnim;
		private Texture2D magic;
		private double animTimer;
		private double darkenTimer;
		private bool enemyDraw;
		private byte flashCount, toReturn;
		private Color magicColor;
		private int enemyType;

		private bool deathMessageDisplayed;

		public Battle(ContentManager contentManager, RenderTarget2D final, GraphicsDevice graphicsDevice, PresentationParameters pp, int score)
		{
			//Generates 0, 1, 2
			enemyType = new Random().Next(0, 3);

			exitReady = false;
			curPhase = Phase.IntroPhase;
			effect = contentManager.Load<Effect>("Battle/BattleBG");
			effect.CurrentTechnique = effect.Techniques[0];
			flash = contentManager.Load<Effect>("Battle/SpriteFlash");
			combatTimer = 0;
			threshHold = 0.15;
			combatIndicator = contentManager.Load<Texture2D>("Battle/Icons/Attack");
			youWon = contentManager.Load<Texture2D>("Battle/Icons/YouWon");
			victoryColor = Color.White;
			flashColor = Color.White;
			secondsPerBeat = 0.5f;
			world = new World(ConvertUnits.ToSimUnits(0,Game1.width));
			waiter = null;
			options = new Icons(contentManager, score >= 20);//shuffle spells only after 20 wins
			blackRect = new Texture2D(graphicsDevice, 1, 1);
			blackRect.SetData(new Color[] { Color.Black });

			travis = new Battler(contentManager, world);
			enemy = new Enemy(contentManager, world, secondsPerBeat, enemyType, threshHold);
			enemyDraw = true;

			MultiSampleCount = pp.MultiSampleCount;
			palette = contentManager.Load<Texture2D>("Battle/003Palette");
			effect.Parameters["palette"].SetValue(palette);
			effect.Parameters["paletteWidth"].SetValue((float)palette.Width);
			effect.Parameters["time"].SetValue((float)bgTimer);
			flash.Parameters["time"].SetValue((float)flashTimer);
			firstEffect = new RenderTarget2D(graphicsDevice, Game1.width, Game1.height, false, SurfaceFormat.Color, DepthFormat.None, MultiSampleCount, RenderTargetUsage.DiscardContents);
			content = contentManager;

			background = contentManager.Load<Texture2D>("Battle/005_GRN");
			background2 = content.Load<Texture2D>("Battle/Yellow");
			magic = contentManager.Load<Texture2D>("Battle/Effects/PkFireA");
			magicAnim = new Animation(0, 24);
			bgTimer = 0;

			this.final = final;//required for scaling
			this.graphicsDevice = graphicsDevice;
			text = new Hud(new string[] { "@" + enemy.GetStageName() + " draws near!" }, content, 22, 2, posY: 3, canClose: true);
			//text.finishText();
			commandName = new Hud(new string[] { options.GetSelectedName() }, content, 6, 0, Game1.width / 3 - 50, 2, canClose: false, centered: true);
			offsetHeightBottom = text.getHeight();
			offsetHeightTop = 32;
			flashCounter = 1;
			timerMult = 1;

			magicColor = Color.White;

			darkenTimer = 1;
			toReturn = 1;
		}

		public string GetEnemyName()
		{
			return enemy.GetName();
		}

		public override string ToString()
		{
			return "Battle";
		}

		void MiniScreen.Unload()
		{
			effect.Dispose();
			flash.Dispose();
			combatIndicator.Dispose();
			youWon.Dispose();
			palette.Dispose();
			background.Dispose();
			background2.Dispose();
			magic.Dispose();
		}

		public void ChangeTarget(RenderTarget2D target)
		{
			final = target;
		}

		public void DrawBackground(SpriteBatch sb)
		{
			graphicsDevice.SetRenderTarget(firstEffect);
			sb.Begin(sortMode: SpriteSortMode.Immediate);
			//effect.Techniques[1].Passes[0].Apply();
			effect.CurrentTechnique.Passes[0].Apply();
			graphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
			sb.Draw(background, new Rectangle(0, 0, Game1.width, Game1.height), Color.White);//Draw to texture
			sb.End();

			graphicsDevice.SetRenderTarget(final);
			sb.Begin(samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.Immediate, blendState: BlendState.Opaque);
			//Console.WriteLine("Count: " + flash.Techniques.Count);

			sb.Draw(firstEffect, new Rectangle(0, 0, Game1.width, Game1.height), Color.White);
			//sb.Draw(secondEffect, new Rectangle(0, 0, 400, 240), Color.White * 0f);

			sb.End();
		}

		private void DrawHud(SpriteBatch sb)
		{
			sb.Begin(samplerState: SamplerState.PointClamp);
			sb.Draw(blackRect, new Rectangle(0, Game1.height - 35, Game1.width, 35), Color.Black);
			sb.Draw(blackRect, new Rectangle(0, 0, Game1.width, 32), Color.Black);
			travis.Draw(sb);
			if (curPhase == Phase.PlayerPhase)
			{
				if (waiter == null)
				{ 
					commandName.Draw(sb);
					//Draw spell icons
					options.Draw(sb, options.GetIndex());
				}
			}

			if (combatTimer > secondsPerBeat - threshHold)// && combatTimer < 0.6)
			{
				if (combatTimer > secondsPerBeat + threshHold)
					combatTimer = threshHold;
			}
			sb.End();

			if (options.IndexChanged())
				commandName = new Hud(new string[] { options.GetSelectedName() }, content, 6, 0, Game1.width / 3 - 50, 2, canClose: false, centered: true);
		}

		void MiniScreen.Draw(SpriteBatch sb)
		{
			if (effect.IsDisposed)
				return;

			DrawBackground(sb);
			DrawHud(sb);

			sb.Begin(samplerState: SamplerState.PointClamp);

			if(enemyDraw)
				enemy.Draw(sb, bgTimer, flashColor, offsetHeightTop, offsetHeightBottom);
			
			sb.End();
			sb.Begin(samplerState: SamplerState.PointWrap);

			if(!victory || turnWaiter > 0.4)
				text.Draw(sb);

			if(victory && turnWaiter > 0.4)
			{
				sb.Draw(youWon, new Rectangle((Game1.width - 102)/2, 18, 102, 10), new Rectangle(0, 0, 102, 10), victoryColor);
			}

			if(curPhase == Phase.AnimPhase && !text.visible)
			{
				int frame = magicAnim.getFrame();
				sb.Draw(magic, new Rectangle(0, 0, Game1.width, Game1.height), new Rectangle((frame % 4) * Game1.width, (frame / 4)*Game1.height, Game1.width, Game1.height), magicColor);
			}
			sb.End();
		}

		byte MiniScreen.Update(GameTime gameTime, KeyboardState prevStateKb, MouseState prevStateM)
		{
			if (flasher != null)
			{
				if ((flashCounter & 1) == 0)
					flashTimer -= gameTime.ElapsedGameTime.TotalSeconds * timerMult*4.2;
				else
					flashTimer += gameTime.ElapsedGameTime.TotalSeconds * timerMult*4.2;
				if(flasher.health > 0)//Attack Flash
					flash.Parameters["time"].SetValue((float)(flashTimer + 0.1));
				else//Dying flash
					flash.Parameters["time"].SetValue((float)(flashTimer/1.1 + 0.01));//the added value controls the offset, multiplication controls transition speed

				if (flashTimer > 0.2)// || flashTimer < 0)
				{
					flashTimer = 0.2;
					flashCounter++;
					//flashColor = Color.Red;
				}
				else if(flashTimer < 0)
				{
					flashTimer = 0;
					flashCounter++;
					//flashColor = Color.Blue;
				}
				if (flashCounter > 11)
				{
					flasher = null;
					flashTimer = 0;
					flashCounter = 1;
					//flashColor = Color.Green;
				}

				switch (flashCounter)
				{
					case 0:
						flashColor = Color.Red;
						break;
					case 1:
						//Final color
						flashColor = Color.Transparent;
						break;
					case 2:
						flashColor = Color.Transparent;
						break;
					case 3:
						flashColor = new Color(128, 128, 64);
						break;
					case 4:
						flashColor = new Color(192, 192, 128);
						break;
					case 5:
						flashColor = new Color(192, 192, 128);
						break;
					case 6:
						flashColor = new Color(192, 192, 128);
						break;
					case 7:
						flashColor = new Color(128, 128, 64);
						break;
					case 8:
						flashColor = new Color(128, 128, 64);
						break;
					case 9:
						flashColor = new Color(128, 128, 64);
						break;
					case 10:
						flashColor = Color.Black;
						break;
					default:
						flashColor = Color.Transparent;
						break;
				}
			}

			enemy.Update(gameTime);

			travis.Update(gameTime, Keyboard.GetState());

			world.Step((float)gameTime.ElapsedGameTime.TotalSeconds);
			combatTimer += gameTime.ElapsedGameTime.TotalSeconds;
			waiterTimer += gameTime.ElapsedGameTime.TotalSeconds;
			turnWaiter += gameTime.ElapsedGameTime.TotalSeconds;

			commandName.finishMessage();
			commandName.Update(gameTime, prevStateKb, prevStateM);
			UpdateBackground(gameTime);

			if (curPhase == Phase.EnemyDeathPhase)
			{
				if (flasher == null)//Executed after flashing finishes
				{
					//Renders an extra frame at the end so the sprite is invisible during transition
					if (exitReady)
						return 255;
					if (enemy.IsKilled() && !deathMessageDisplayed)
					{
						exitReady = true;
						//return 255;
						text = new Hud(new string[] { enemy.deathMessage }, content, 30, 2, posY: 3, canClose: true);
						text.finishMessage();
						text.visible = false;
						deathMessageDisplayed = true;
					}
					enemy.Kill();
				}
				Console.WriteLine("flash: " + flashCounter);

				if(text.messageComplete())
					if(flashCounter == 1)
					{
						text = new Hud(new string[] { "" }, content, 30, 2, posY: 3, canClose: false);
						text.finishMessage();
						//text.finishText();
						//text.visible = false;
						victory = true;
						turnWaiter = 0;
					}
			}


			if (turnWaiter > 0.2)
			{
				if (text.messageComplete())
				{
					if (waiter != null)
					{
						if (waiter.attacked)
						{
							if (waiter is Enemy)
							{
								if (waiter.IsDone(gameTime, combatTimer))
								{
									waiter.ForceFinish();
									waiter = null;
									turnWaiter = 0;
								}
							}
							else
							{
								if (waiter.IsDone(gameTime, waiterTimer))
								{
									waiterTimer = 0;
									waiter.ForceFinish();
									waiter = null;
									turnWaiter = 0;
								}
							}
						}
						else
						{
							waiter.TakeDamage(10, combatTimer);
							waiter.attacked = true;
						}
					}
					else
						advanceBattle(gameTime, prevStateKb, prevStateM);
				}
				//else
				//text.Update(gameTime, prevState);

				if (victory && turnWaiter > 0.5)
					return 255;
				
			}
			text.Update(gameTime, prevStateKb, prevStateM);

			//prevState = Keyboard.GetState();

			//if (prevStateKb.IsKeyDown(Keys.Q) && Keyboard.GetState().IsKeyUp(Keys.Q))
			//	return 255;

			return toReturn;
		}

		private void advanceBattle(GameTime gameTime, KeyboardState prevStateKb, MouseState prevStateM)
		{
			//Console.WriteLine("Calling advance!");
			switch (curPhase)
			{
				case Phase.IntroPhase:
					if(text.messageComplete())
						curPhase = Phase.PlayerPhase;
					break;
				case Phase.PlayerPhase:
					if (waiter == null)
					{
						options.Update(prevStateKb, prevStateM);
						//selector.Update(prevState);
					}
						

					if ((Keyboard.GetState().IsKeyDown(Keys.Space) && prevStateKb.IsKeyUp(Keys.Space)) ||
					    (prevStateM.LeftButton == ButtonState.Pressed && Mouse.GetState().LeftButton == ButtonState.Released))
					{
						switch (options.GetMagic())
						{
							//Earth
							case "Earth":
								//playerMove = 0;
								playerMove = 1;
								magic = content.Load<Texture2D>("Battle/Effects/PkFireA_GRN");
								toReturn = 254;
								magicColor = Color.Purple;

								curPhase = Phase.EnemyPhase;
								break;
							//Fire
							case "Fire":
								playerMove = 1;
								magic = content.Load<Texture2D>("Battle/Effects/PkFireA_GRN");
								toReturn = 254;
								magicColor = Color.White;

								curPhase = Phase.EnemyPhase;
								break;
							//Water
							case "Water":
								playerMove = 1;
								magic = content.Load<Texture2D>("Battle/Effects/PkFireA_GRN");
								toReturn = 254;
								magicColor = Color.LightGray;

								curPhase = Phase.EnemyPhase;
								break;
						}
					}
					break;
				case Phase.EnemyPhase:
					enemyMove = 0;
					curPhase = Phase.AttackPhase;
					break;
				case Phase.AttackPhase:
					if(enemy.health <= 0)
					{
						if (enemyType == 0 && magicColor.Equals(Color.White) || enemyType == 1 && magicColor.Equals(Color.Purple) || enemyType == 2 && magicColor.Equals(Color.LightGray))
						{
							deathMessageDisplayed = false;
							curPhase = Phase.EnemyDeathPhase;
							flasher = enemy;
							enemy.ChangeToWhite();
							flashTimer = -0.5;
							flashCounter = 1;
							timerMult = 0.5f;
						}
						else
						{
							toReturn = 2;
						}
						//text = new Hud(new string[] { "@The enemy dissipates into hollow armor." }, content, 30, 2, posY: 3, canClose: true);
					}
					else
					if(playerMove != -1)
					{
						if (playerMove == 0)
						{
							enemy.attacked = false;
							//text = new Hud(new string[] { "@Travis's attack!" }, content, 30, 2, posY: 3, canClose: true);
							//enemy.TakeDamage(1, combatTimer);
							waiter = enemy;
							//playerMove = -1;
						}
						else if(playerMove == 1)
						{
							//text = new Hud(new string[] { "@Travis tried PK Fire [!" }, content, 30, 2, posY: 3, canClose: true);
							//text.finishText();
							Console.WriteLine("GetFrame: " + magicAnim.getFrame());
							magicAnim.resetStart();
							darkenTimer = 1;
							curPhase = Phase.AnimPhase;
							//if (magicAnim.getFrame() == 25)
							//playerMove = -1;
							//playerMove = -1;
							//TODO: Something
							enemy.DecreaseHealth(5);
						}
						playerMove = -1;
					}
					else if(enemyMove == 0)
					{
						flasher = enemy;
						flashTimer = 0;

						travis.attacked = false;
						text = new Hud(new string[] { "@enemy's attack!" }, content, 30, 2, posY: 3, canClose: true);
						//travis.TakeDamage(1, combatTimer);
						waiter = travis;
						enemyMove = -1;
						timerMult = 1;
						//flashCounter = 1;
						//flashTimer = 0;
					}
					else
					{
						curPhase = Phase.PlayerPhase;
					}
					break;
				case Phase.EnemyDeathPhase:
					//Refer to the if statement in the Update() function

					break;
				case Phase.AnimPhase:
					Console.WriteLine("anim");

					if (magicAnim.getFrame() == magicAnim.frameCount)
					{
						Console.WriteLine("Skadoosh");
						if (darkenTimer < 1)
							darkenTimer += gameTime.ElapsedGameTime.TotalSeconds * 4;
						else//end phase
						{
							//curPhase = Phase.AttackPhase;
							//magic.Dispose();
							curPhase = Phase.BlinkPhase;
							enemyDraw = false;
							magicAnim.resetStart();
							animTimer = 0;
						}
					}
					else
					{
						if (darkenTimer > 0.5)
							darkenTimer -= gameTime.ElapsedGameTime.TotalSeconds * 5;

						animTimer += gameTime.ElapsedGameTime.TotalSeconds;

						if (animTimer > 0.05)
						{
							animTimer -= 0.05;
							magicAnim.advanceFrame();
						}
					}
					break;
				case Phase.BlinkPhase:
					animTimer += gameTime.ElapsedGameTime.TotalSeconds;

					if(animTimer > 0.07)
					{
						flashCount++;
						animTimer -= 0.07;
						if(flashCount < 11)
							enemyDraw = !enemyDraw;
					}

					if (flashCount == 11)
					{
						//flashCount = 0;
						enemyDraw = true;
						//curPhase = Phase.AttackPhase;
					}
					else if(flashCount == 17)
					{
						flashCount = 0;
						curPhase = Phase.AttackPhase;
					}
					break;
				default:
					break;
			}
		}

		private void UpdateBackground(GameTime gameTime)
		{
			bgTimer += gameTime.ElapsedGameTime.TotalSeconds;
			if (bgTimer > Math.PI * 2)
			{
				//bgTimer -= Math.PI*2;
				//Console.WriteLine("Timer reset");
			}
			effect.Parameters["time"].SetValue((float)bgTimer);
		}
	}
}
