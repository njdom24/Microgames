using FarseerPhysics;
using FarseerPhysics.Dynamics;
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

namespace RPG
{
	class Enemy : Combatant
	{
		private Body body;
		private Texture2D sprite;
		private double moveTimer;
		private int offsetTop;
		private int offsetBottom;
		private float lastForce;
		private double moveDuration;
		private double secondsPerBeat;
		private double threshHold;
		private bool noteHit;
		private int noteCount;
		private Texture2D hitEffects;
		private Body[] noteBodies;
		private Vector2[] notePositions;
		private bool[] visibility;
		private Vector2 defaultPos;
		private int centerX;
		private int centerY;

		private Vector2 notePos;
		private double noteTimer;

		private int verticalRadius = 15;
		private int verticalRadiusFinal = 180;//120
		private int horizontalRadius = 80;
		private int horizontalRadiusFinal = 250;//200
		private bool increaseRadius = false;
		private double radiusTimer = 0;

		private bool killed;

		private int[] visOrder;

		private double piTimer;
		private double hitTimer;
		private Random offsetter;
		private int hitX;
		private int hitY;

		private Color color;
		public string deathMessage;
		private string name;

		public Enemy(ContentManager contentManager, World world, double secondsPerBeat, int enemyType, double threshHold = 0, int offsetTop = 0, int offsetBottom = 0)
		{
			color = Color.White;
			offsetter = new Random();
			hitX = 0;
			hitY = 0;
			hitTimer = 0;

			if (enemyType == 0)
			{
				sprite = contentManager.Load<Texture2D>("Battle/Enemies/Borowstree");
				name = "Borowstree";
			}
			else
			{
				sprite = contentManager.Load<Texture2D>("Battle/Enemies/Borowater");
				name = "Borowater";
			}
			
			hitEffects = contentManager.Load<Texture2D>("Battle/Icons/HitEffects");
			piTimer = 0;
			noteBodies = new Body[15];
			notePositions = new Vector2[noteBodies.Length];
			visOrder = new int[] { 10, 2, 5, 13, 7, 14, 9, 1, 8, 3, 11, 6, 12, 4, 0 };//, 8 };
			visibility = new bool[noteBodies.Length];
			killed = false;
			noteTimer = 0;
			notePos = Vector2.Zero;
			defaultPos = new Vector2((Game1.width - sprite.Width) / 2, (Game1.height - offsetBottom + offsetTop - sprite.Height) / 2);
			noteHit = true;
			noteCount = 0;
			this.secondsPerBeat = secondsPerBeat;
			this.threshHold = threshHold;
			moveDuration = 0.05;
			lastForce = 450;
			this.offsetTop = offsetTop;
			this.offsetBottom = offsetBottom;
			body = new Body(world, new Vector2((Game1.width - sprite.Width) / 2, (Game1.height - offsetBottom + offsetTop - sprite.Height) / 2));
			body.SetTransform(new Vector2((Game1.width - sprite.Width) / 2, (Game1.height - offsetBottom + offsetTop - sprite.Height) / 2), 0);
			body.BodyType = BodyType.Dynamic;
			body.IgnoreGravity = true;
			body.Mass = 0.1f;
			health = 2;
			moveTimer = 0.05;
			deathMessage = "@The knight dissipates into hollow armor.";

			centerX = (int)defaultPos.X + (sprite.Width - 10) / 2 + 6;
			centerY = (int)defaultPos.Y + (sprite.Height - 15) / 2 - 30;

			for (int i = 0; i < noteBodies.Length; i++)
			{
				noteBodies[i] = new Body(world, new Vector2(centerX + horizontalRadius * (float)Math.Cos(i * Math.PI / 8), centerY - verticalRadius * ((float)Math.Sin(i * Math.PI / 8) - (float)Math.Cos(i * Math.PI / 8))), 0);
				noteBodies[i].BodyType = BodyType.Dynamic;
				//notePositions[i] = new Vector2(centerX + horizontalRadius * (float)Math.Cos(i * Math.PI / 8), centerY - verticalRadius * ((float)Math.Sin(i * Math.PI / 8) - (float)Math.Cos(i * Math.PI / 8)));
			}
		}

		public string GetName()
		{
			return name;
		}

		//Turns every colored pixel of the sprite white
		//Intended for use prior to flashing to avoid tinting
		public void ChangeToWhite()
		{ 
			Color[] tcolor = new Color[sprite.Width * sprite.Height];
			sprite.GetData<Color>(tcolor);

			for (int i = 0; i < tcolor.Length; i++)
			{
				if (tcolor[i].A != 0)
					tcolor[i] = Color.White;
			}
			sprite.SetData<Color>(tcolor);
		}

		public void Draw(SpriteBatch sb, double piTimer, Color flashColor, int offsetTop = 0, int offsetBottom = 0)
		{
			sb.Draw(sprite, new Rectangle((int)body.Position.X, (int)body.Position.Y - 22, sprite.Width, sprite.Height), flashColor);
			if(hitTimer > 0)//Draw hit marker
			{
				sb.Draw(hitEffects, new Rectangle(centerX + hitX, centerY + 27/2 + hitY, 27, 27), new Rectangle(10, 0, 27, 27), Color.White);
			}
			for (int i = noteBodies.Length-1; i >= 0; i--)//Draw music note
			{
				if (visibility[i])
				{
					sb.Draw(hitEffects, new Rectangle((int)ConvertUnits.ToDisplayUnits(noteBodies[i].Position.X), (int)ConvertUnits.ToDisplayUnits(noteBodies[i].Position.Y), 10, 15), new Rectangle(0, 0, 10, 15), Color.White);
				}
			}
			//sb.Draw(musicNote, new Rectangle((int)notePos.X, (int)notePos.Y, musicNote.Width, musicNote.Height), Color.White);
		}

		public void UpdateNotes()
		{
			for (int i = 0; i < noteBodies.Length; i++)
			{
				if(increaseRadius)
					noteBodies[i].SetTransform(ConvertUnits.ToSimUnits(centerX + horizontalRadius * (float)Math.Cos((Math.PI / 8 * i) + 1.4*piTimer), centerY - verticalRadius * ((float)Math.Sin((Math.PI / 8 * i) + 1.4*piTimer))), 0);
				else
					noteBodies[i].SetTransform(ConvertUnits.ToSimUnits(centerX + horizontalRadius * (float)Math.Cos((Math.PI / ((noteBodies.Length) / 2) * i) + piTimer), centerY - verticalRadius * ((float)Math.Sin((Math.PI / ((noteBodies.Length) / 2) * i) + piTimer) - (float)Math.Cos((Math.PI / ((noteBodies.Length) / 2) * i) + piTimer / 2))), 0);
			}

			//notePos = new Vector2((int)(centerX + horizontalRadius * Math.Cos(seconds)), (int)(centerY - verticalRadius * Math.Sin(seconds)));
		}

		public void Update(GameTime gameTime)
		{
			radiusTimer += gameTime.ElapsedGameTime.TotalSeconds;
			piTimer += gameTime.ElapsedGameTime.TotalSeconds / secondsPerBeat * Math.PI;
			if(hitTimer > 0)
				hitTimer -= gameTime.ElapsedGameTime.TotalSeconds;
			if(increaseRadius)
			{   //one's prolly gonna increase faster
				UpdateNotes();

				if (horizontalRadiusFinal > horizontalRadius)
				{
					horizontalRadius += (int)(500*gameTime.ElapsedGameTime.TotalSeconds);//+= (int)(1000 * gameTime.ElapsedGameTime.TotalSeconds);//USE A RADIUS TIMER
					verticalRadius += (int)(400 * gameTime.ElapsedGameTime.TotalSeconds);//+= (int)(1000 * gameTime.ElapsedGameTime.TotalSeconds);
				}
				else
				{
					visibility = new bool[noteBodies.Length];
					increaseRadius = false;
					horizontalRadius = 80;
					verticalRadius = 15;
				}
			}
			//UpdateNodes();
		}

		public override void ForceFinish()
		{
			lastForce = 450;
			moveTimer = moveDuration;
			body.ResetDynamics();
			body.SetTransform(defaultPos, 0);
		}

		public void DecreaseHealth(int amt)//animates a number of decreased health
		{
			health -= amt;
		}

		private bool FinishCombo()
		{
			//visibility = new bool[16];
			if(noteCount == noteBodies.Length)
			{
				radiusTimer = 0;
				increaseRadius = true;
				for (int i = 0; i < noteBodies.Length; i++)
				{
					//noteBodies[i].IgnoreGravity = true;
					noteBodies[i].ResetDynamics();
				}
				//increase radius
			}
			else
				for(int i = 0; i < noteBodies.Length; i++)
				{
					//noteBodies[i].IgnoreGravity = false;
					noteBodies[i].ResetDynamics();
					//noteBodies[i].LinearDamping = 0;
					//noteBodies[i].ApplyForce(new Vector2(9000*(noteBodies[i].Position.X - centerX), 50*(noteBodies[i].Position.Y - centerY)));
					noteBodies[i].ApplyForce(ConvertUnits.ToSimUnits(noteCount*50*(ConvertUnits.ToDisplayUnits(noteBodies[i].Position.X) - centerX), noteCount*40*(ConvertUnits.ToDisplayUnits(noteBodies[i].Position.Y)-centerY)));
					//give it an initial velocity or something
				}
			return true;
		}

		public override bool IsDone(GameTime gameTime, double combatTimer)
		{
			UpdateNotes();
			if (noteCount == noteBodies.Length)
				return FinishCombo();

			moveTimer -= gameTime.ElapsedGameTime.TotalSeconds;
			
			if (combatTimer > secondsPerBeat + threshHold)//if time window ends
			{
				if (noteHit)
					noteHit = false;
				else
					return FinishCombo();
			}

			//Uncomment to allow rhythm-based gameplay (Out of scope at the moment)
			/*
			if ((Keyboard.GetState().IsKeyDown(Keys.Space) && prevStateKb.IsKeyUp(Keys.Space)) ||
				(prevStateM.LeftButton == ButtonState.Pressed && Mouse.GetState().LeftButton == ButtonState.Released))
			{
				if (combatTimer > secondsPerBeat - threshHold)//during time window
				{
					if (noteHit)
						return FinishCombo();

					AdditionalDamage();
				}
				else//exit when a beat is missed
					return FinishCombo();
			}
			*/

			//if (timer < 0)
			//return true;

			if (Math.Abs(lastForce) < 20)
			{
				ForceFinish();
				lastForce = 0;
			}
			else if (moveTimer < 0)
			{
				Jostle(-0.75f);
				//moveDuration = moveDuration / 1.2;
				moveTimer = moveDuration;
			}

			return false;
		}

		public void Kill()
		{
			killed = true;
			color = Color.Transparent;
		}

		public bool IsKilled()
		{
			return killed;
		}

		private void MakeVisible()
		{
			/*
			int lowestIndex = 0;
			double lowestHeight = 9;
			for (int i = 0; i < notePositions.Length; i++)
			{
				if (notePositions[i].Y > lowestHeight)
				{
					lowestIndex = i;
					lowestHeight = notePositions[i].Y;
				}
			}
			visibility[lowestIndex] = true;
			*/
			//visibility[12] = true;
			hitTimer = 0.05;
			hitX = offsetter.Next(-10, 10);
			hitY = offsetter.Next(-10, 10);
			if(noteCount > 0)
				visibility[visOrder[noteCount-1]] = true;
			//Console.WriteLine("NoteCount: " + noteCount);
		}

		public override void TakeDamage(int damage, double combatTimer)
		{
			visibility = new bool[noteBodies.Length];
			noteCount = 0;
			Jostle(-2.5f);
			piTimer = combatTimer;
			health -= damage;
			noteHit = (combatTimer > secondsPerBeat - threshHold);

			MakeVisible();
			increaseRadius = false;
			horizontalRadius = 80;
			verticalRadius = 15;
		}

		private void AdditionalDamage()
		{
			ForceFinish();
			Jostle(-1.5f);
			health--;
			noteHit = true;
			noteCount++;
			piTimer += 0.2;

			MakeVisible();
		}

		private void Jostle(float multiplier)
		{
			body.ResetDynamics();
			body.ApplyForce(new Vector2(lastForce, 0));
			lastForce *= multiplier;
		}
	}
}
