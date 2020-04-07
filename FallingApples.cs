using System;
using System.Collections;
using System.Collections.Generic;
using FarseerPhysics;
using FarseerPhysics.DebugView;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace RPG
{
	public class Apple
	{
		private Body body;
		private double heightTimer;
		private int hPos;

		public Apple(World world, Texture2D sprite, int hPos = 80)
		{
			body = BodyFactory.CreateRectangle(world, sprite.Width, sprite.Width, 0, new Vector2(hPos, 0));
			body.BodyType = BodyType.Static;
			body.IgnoreGravity = true;
			body.IsSensor = true;
			body.UserData = this;

			heightTimer = 0.0;
			this.hPos = hPos;
		}

		public bool Update(double dt)
		{
			heightTimer += dt;

			float rotation = 0;
			if(heightTimer > 0.2)
				rotation = (float)(body.Rotation + dt * heightTimer * 2);
			body.SetTransform(new Vector2(hPos, (float)(4 * heightTimer * heightTimer * heightTimer)), rotation);
			//body.Rotation = (float)(body.Rotation + dt*20);

			if (body.Position.Y > Game1.height)
				return true;
			return false;
		}

		public void Destroy(World world)
		{
			//world.RemoveBody(body);
			body.Dispose();
		}

		public Vector2 GetPos ()
		{
			return body.Position;
		}

		public float GetRotation()
		{
			return body.Rotation;
		}
	}

	public class FallingApples : MiniScreen
	{
		private Texture2D background, basket, hat, apple;
		private double mouseX;
		private double spawnTimer;
		private int maxVelocity;
		private int mousePos;
		private int collectedCount;
		private bool usingKeyboard;

		private double timer;

		private World world;
		private Body basketBody;

		private List<Apple> apples;
		private Random random;


		public FallingApples(ContentManager contentManager, GraphicsDevice pDevice)
		{
			background = contentManager.Load<Texture2D>("Corneria_gutter");
			hat = contentManager.Load<Texture2D>("FallingApples/wizJUNP_Back");
			basket = contentManager.Load<Texture2D>("FallingApples/wizJUNP");
			apple = contentManager.Load<Texture2D>("FallingApples/Apple");
			timer = 0.0;
			spawnTimer = 0.0;
			maxVelocity = 200;
			ConvertUnits.SetDisplayUnitToSimUnitRatio(10);
			world = new World(Vector2.Zero);

			basketBody = BodyFactory.CreateRectangle(world, basket.Width, (int)(basket.Height/8), 0, new Vector2(Game1.width/2, Game1.height - basket.Height / 2 - 18));
			basketBody.BodyType = BodyType.Static;
			basketBody.IgnoreGravity = true;
			basketBody.IsSensor = true;
			basketBody.UserData = "Basket";

			world.ContactManager.OnBroadphaseCollision += BroadphaseHandler;

			apples = new List<Apple>();
			apples.Add(new Apple(world, apple, 50));

			random = new Random();
			collectedCount = 0;
			usingKeyboard = true;
		}

		private void BroadphaseHandler(ref FixtureProxy fp1, ref FixtureProxy fp2)
		{
			if (fp1.Fixture.Body.UserData is Apple && fp2.Fixture.Body.UserData.Equals("Basket"))
			{
				Apple a = (Apple)fp1.Fixture.Body.UserData;
				a.Destroy(world);
				Console.WriteLine("A: " + collectedCount);
				apples.Remove((Apple)fp1.Fixture.Body.UserData);
				collectedCount++;
			}
			else if (fp2.Fixture.Body.UserData is Apple && fp1.Fixture.Body.UserData.Equals("Basket"))
			{
				Apple a = (Apple)fp2.Fixture.Body.UserData;
				a.Destroy(world);
				Console.WriteLine("B: " + collectedCount);
				apples.Remove((Apple)fp2.Fixture.Body.UserData);
				apples.Remove((Apple)fp2.Fixture.Body.UserData);
				collectedCount++;
			}
		}

		public void Draw(SpriteBatch sb)
		{
			sb.Begin();
			sb.Draw(background, new Rectangle(0, 0, Game1.width, Game1.height), new Rectangle(220, 48, 3, 3), new Color(30,30,30));//Sourcing white background from a placeholder spritesheet

			sb.Draw(hat, new Rectangle((int)basketBody.Position.X - basket.Width / 2, (int)basketBody.Position.Y - basket.Height / 8 - 20, basket.Width, basket.Height), new Rectangle(0, 0, basket.Width, basket.Height), Color.White);

			foreach (Apple a in apples)
			{
				Vector2 pos = a.GetPos();
				float rotation = a.GetRotation();

				sb.Draw(apple, new Rectangle((int)pos.X, (int)pos.Y, apple.Width*2, apple.Height*2), new Rectangle(0, 0, apple.Width, apple.Height), Color.White, rotation, new Vector2(apple.Width/2, apple.Height/2), SpriteEffects.None, 1);
			}


			sb.Draw(basket, new Rectangle((int)basketBody.Position.X - basket.Width/2, (int)basketBody.Position.Y - basket.Height/8 - 20, basket.Width, basket.Height), new Rectangle(0, 0, basket.Width, basket.Height), Color.White);
			sb.End();
		}

		public void Unload()
		{
			basket.Dispose();
		}

		public byte Update(GameTime dt, KeyboardState prevStateKb, MouseState prevStateM)
		{
			timer += dt.ElapsedGameTime.TotalSeconds * 2;

			//Don't need widthOffset due to body position being centered around the body
			int widthOffset = basket.Width / 2;

			MouseState mState = Mouse.GetState();
			KeyboardState kbState = Keyboard.GetState();

			if (kbState.IsKeyDown(Keys.Right) || kbState.IsKeyDown(Keys.Left))
				usingKeyboard = true;
			else if (mState.Position != prevStateM.Position)
			{
				usingKeyboard = false;
				mouseX = basketBody.Position.X;
			}
			
			if (usingKeyboard)
			{
				Vector2 pos = basketBody.Position;

				if (kbState.IsKeyDown(Keys.Right))
				{
					basketBody.SetTransform(new Vector2((float)(pos.X + maxVelocity * dt.ElapsedGameTime.TotalSeconds), Game1.height - basket.Height/2 - 18), 0);
				}
				else if (kbState.IsKeyDown(Keys.Left))
				{
					basketBody.SetTransform(new Vector2((float)(pos.X - maxVelocity * dt.ElapsedGameTime.TotalSeconds), Game1.height - basket.Height/2 - 18), 0);
				}
			}
			else
			{
				mousePos = (int)(mState.X * Game1.resMultiplier);

				if (mouseX <= Game1.width && mousePos - mouseX > 5)
					mouseX += maxVelocity * dt.ElapsedGameTime.TotalSeconds;
				else if (mouseX >= basket.Width / 2 && mousePos - mouseX < -5)
					mouseX -= maxVelocity * dt.ElapsedGameTime.TotalSeconds;

				basketBody.SetTransform(new Vector2((float)mouseX, Game1.height - basket.Height/2 - 18), 0);
			}
			//TODO: MOVE THE DRAW UP


			for (int i = 0; i < apples.Count; i++)
			{
				if (apples[i] != null && (apples[i].Update(dt.ElapsedGameTime.TotalSeconds * 2)))
					apples.RemoveAt(i);
			}

			spawnTimer += dt.ElapsedGameTime.TotalSeconds;

			if (spawnTimer > 1)
			{
				spawnTimer = 0;
				int spawnPoint = random.Next(20, Game1.width - 20);
				apples.Add(new Apple(world, apple, spawnPoint));
			}

			if (collectedCount == 3)
			{
				return 255;
			}

			world.Step((float)dt.ElapsedGameTime.TotalSeconds);

			return 1;
		}
	}
}
