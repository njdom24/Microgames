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
	public class Pellet
	{
		private Body body;
		private double heightTimer;
		private int hPos;

		public Pellet(World world, int hPos = 80)
		{
			heightTimer = Game1.height - 45;
			this.hPos = hPos;

			body = BodyFactory.CreateRectangle(world, 3, 5, 0, new Vector2(hPos, (int)heightTimer));
			body.BodyType = BodyType.Static;
			body.IgnoreGravity = true;
			body.IsSensor = true;
			body.UserData = this;
		}

		public bool Update(double dt)
		{
			heightTimer -= dt;

			body.SetTransform(new Vector2(hPos, (float)(heightTimer)), 0);

			if (body.Position.Y < -10)
				return true;
			return false;
		}

		public void Destroy(World world)
		{
			//world.RemoveBody(body);
			body.Dispose();
		}

		public Vector2 GetPos()
		{
			return new Vector2(body.Position.X - 1, body.Position.Y);
		}
	}

	public class Alien
	{
		private Body body;
		private int hPos;
		private Texture2D sprite;

		public Alien(World world, Texture2D sprite, int hPos = 80)
		{
			this.hPos = hPos;
			this.sprite = sprite;

			body = BodyFactory.CreateRectangle(world, sprite.Width, sprite.Height, 0, new Vector2(hPos, 30));
			body.BodyType = BodyType.Static;
			body.IgnoreGravity = true;
			body.IsSensor = true;
			body.UserData = this;
		}

		public bool Update(double dt)
		{
			body.SetTransform(new Vector2(hPos, 30), 0);

			return false;
		}

		public void Destroy(World world)
		{
			//world.RemoveBody(body);
			body.Dispose();
		}

		public void Draw(SpriteBatch sb)
		{
			Vector2 pos = GetPos();
			sb.Draw(sprite, new Rectangle((int)pos.X, (int)pos.Y, sprite.Width, sprite.Height), new Rectangle(0, 0, sprite.Width, sprite.Height), Color.Red);
		}

		public Vector2 GetPos()
		{
			return new Vector2(body.Position.X - sprite.Width/2, body.Position.Y - sprite.Height/2);
		}

		public float GetRotation()
		{
			return body.Rotation;
		}
	}

	public class Galaga : MiniScreen
	{
		private Texture2D background, ship;
		private int shipWidth;
		private double mouseX;
		private int maxVelocity;
		private int mousePos;
		private bool usingKeyboard;

		private double timer;

		private World world;
		private Body shipBody;

		private Random random;
		private List<Pellet> pellets;
		private List<Alien> aliens;

		public Galaga (ContentManager contentManager, GraphicsDevice pDevice)
		{
			background = contentManager.Load<Texture2D>("Corneria_gutter");
			ship = contentManager.Load<Texture2D>("Galaga/Ship");
			shipWidth = ship.Width - 4;
			timer = 0.0;
			maxVelocity = 200;
			ConvertUnits.SetDisplayUnitToSimUnitRatio(10);
			world = new World(Vector2.Zero);

			shipBody = BodyFactory.CreateRectangle(world, shipWidth, (int)(ship.Height * 0.9), 0, new Vector2(Game1.width / 2, Game1.height - ship.Height - 6));
			shipBody.BodyType = BodyType.Static;
			shipBody.IgnoreGravity = true;
			shipBody.IsSensor = true;
			shipBody.UserData = "ship";

			world.ContactManager.OnBroadphaseCollision += BroadphaseHandler;

			random = new Random();
			usingKeyboard = true;

			pellets = new List<Pellet>();

			aliens = new List<Alien>();
			aliens.Add(new Alien(world, ship, 20));
			aliens.Add(new Alien(world, ship, Game1.width / 2));
			aliens.Add(new Alien(world, ship, Game1.width - 20));
		}

		private void BroadphaseHandler(ref FixtureProxy fp1, ref FixtureProxy fp2)
		{
			if (fp1.Fixture.Body.UserData is Alien && fp2.Fixture.Body.UserData is Pellet)
			{
				Alien a = (Alien)fp1.Fixture.Body.UserData;
				a.Destroy(world);
				aliens.Remove(a);

				Pellet p = (Pellet)fp2.Fixture.Body.UserData;
				p.Destroy(world);
				pellets.Remove(p);
			}
			else if (fp2.Fixture.Body.UserData is Alien && fp1.Fixture.Body.UserData is Pellet)
			{
				Alien a = (Alien)fp2.Fixture.Body.UserData;
				a.Destroy(world);
				aliens.Remove(a);

				Pellet p = (Pellet)fp1.Fixture.Body.UserData;
				p.Destroy(world);
				pellets.Remove(p);
			}
		}

		public void Draw(SpriteBatch sb)
		{
			sb.Begin();
			sb.Draw(background, new Rectangle(0, 0, Game1.width, Game1.height), new Rectangle(220, 48, 3, 3), new Color(30, 30, 30));//Sourcing white background from a placeholder spritesheet

			sb.Draw(ship, new Rectangle((int)shipBody.Position.X - shipWidth / 2, (int)shipBody.Position.Y - 12, shipWidth, ship.Height), new Rectangle(0, 0, shipWidth, ship.Height), Color.White);

			foreach (Pellet p in pellets)
			{
				Vector2 pos = p.GetPos();
				sb.Draw(ship, new Rectangle((int)pos.X, (int)pos.Y, 3, 5), new Rectangle(28, 19, 3, 5), Color.White);
			}

			foreach (Alien a in aliens)
				a.Draw(sb);

			sb.End();
		}

		public void Unload()
		{
			ship.Dispose();
		}

		public byte Update(GameTime dt, KeyboardState prevStateKb, MouseState prevStateM)
		{
			timer += dt.ElapsedGameTime.TotalSeconds * 2;

			//Don't need widthOffset due to body position being centered around the body
			int widthOffset = shipWidth / 2;

			MouseState mState = Mouse.GetState();
			KeyboardState kbState = Keyboard.GetState();

			if (kbState.IsKeyDown(Keys.Right) || kbState.IsKeyDown(Keys.Left))
				usingKeyboard = true;
			else if (mState.Position != prevStateM.Position)
			{
				usingKeyboard = false;
				mouseX = shipBody.Position.X;
			}

			if (usingKeyboard)
			{
				Vector2 pos = shipBody.Position;

				if (kbState.IsKeyDown(Keys.Right))
				{
					shipBody.SetTransform(new Vector2((float)(pos.X + maxVelocity * dt.ElapsedGameTime.TotalSeconds), Game1.height - ship.Height - 6), 0);
				}
				else if (kbState.IsKeyDown(Keys.Left))
				{
					shipBody.SetTransform(new Vector2((float)(pos.X - maxVelocity * dt.ElapsedGameTime.TotalSeconds), Game1.height - ship.Height - 6), 0);
				}
			}
			else
			{
				mousePos = (int)(mState.X * Game1.resMultiplier);

				if (mouseX <= Game1.width && mousePos - mouseX > 5)
					mouseX += maxVelocity * dt.ElapsedGameTime.TotalSeconds;
				else if (mouseX >= shipWidth / 2 && mousePos - mouseX < -5)
					mouseX -= maxVelocity * dt.ElapsedGameTime.TotalSeconds;

				shipBody.SetTransform(new Vector2((float)mouseX, Game1.height - ship.Height - 6), 0);
			}

			if ((kbState.IsKeyDown(Keys.Space) && prevStateKb.IsKeyUp(Keys.Space)) || 
			    (prevStateM.LeftButton == ButtonState.Pressed && Mouse.GetState().LeftButton == ButtonState.Released))
			{
				pellets.Add(new Pellet(world, (int)shipBody.Position.X));
			}

			double pelletDt = dt.ElapsedGameTime.TotalSeconds * 200;
			for (int i = 0; i < pellets.Count; i++)
			{
				if (pellets[i] != null && (pellets[i].Update(pelletDt)))
					pellets.RemoveAt(i);
			}

			for (int i = 0; i < aliens.Count; i++)
			{
				if (aliens[i] != null)
					aliens[i].Update(dt.ElapsedGameTime.TotalSeconds);
			}

			world.Step((float)dt.ElapsedGameTime.TotalSeconds);

			return 1;
		}
	}
}
