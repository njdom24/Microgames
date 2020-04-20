using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RPG
{
	class Battler : Combatant
	{
		private Texture2D portrait;
		private Texture2D text;
		private Texture2D scrollingNums;
		private Vector2 nameOffset;

		private int posX, posY;
		private Body body;
		private float lastForce;
		private double fullTimer;
		private double moveTimer;
		private Text name;
		private Vector2 pos;
		//private int nameOffset;

		public Battler(ContentManager contentManager, World world)
		{
			maxHealth = 123;
			health = maxHealth;
			maxMagic = 69;
			magic = maxMagic;

			portrait = contentManager.Load<Texture2D>("Battle/Player");
			text = contentManager.Load<Texture2D>("Textbox/Text");
			scrollingNums = contentManager.Load<Texture2D>("Battle/Numbers/ScrollingNums");//5x8
			posX = (Game1.width - portrait.Width) / 2;
			posY = Game1.height - 52;
			pos.X = posX;
			pos.Y = posY;
			//posY = Game1.height / 2;
			Console.WriteLine(posX + ", " + posY);
			body = new Body(world, ConvertUnits.ToSimUnits(posX, posY));
			body.BodyType = BodyType.Dynamic;
			body.IgnoreGravity = true;
			lastForce = 130f;
			name = new Text(contentManager, "Spells");
			name.SetColor(Color.Black);

			nameOffset = new Vector2((Game1.width - name.width) / 2, Game1.height - 40);
			//nameWidth = letterPos[letterPos.Length - 1];
			//Console.WriteLine("NameWidth: " + nameWidth);
		}
		public override void ForceFinish()
		{
			lastForce = 130f;
			moveTimer = 0;
			body.ResetDynamics();
			body.SetTransform(ConvertUnits.ToSimUnits(posX, posY), 0);
		}

		public override bool IsDone(GameTime gameTime, double combatTimer)
		{
			fullTimer += gameTime.ElapsedGameTime.TotalSeconds;
			if (fullTimer > 1)
			{
				fullTimer = 0;
				ForceFinish();
				return true;
			}
			else
			{
				moveTimer += gameTime.ElapsedGameTime.TotalSeconds;

				if(moveTimer > 0.1f)
				{
					lastForce *= -0.9f;
					body.ResetDynamics();
					body.ApplyForce(new Vector2(0, lastForce));
					
					//body.LinearVelocity = -body.LinearVelocity;
					moveTimer = 0;
				}
			}
			//throw new NotImplementedException();
			return false;
		}

		public override void TakeDamage(int damage, double combatTimer)
		{
			Console.WriteLine("K Y K Y");
			health -= damage;
			//body.ResetDynamics();
			body.ApplyForce(new Vector2(0, lastForce));
			//body.LinearVelocity = ConvertUnits.ToSimUnits(0, 150);
			//throw new NotImplementedException();
		}

		

		public void Draw(SpriteBatch sb)
		{
			sb.Draw(portrait, new Rectangle((int)posX, (int)posY, portrait.Width, portrait.Height), new Rectangle(0, 0, portrait.Width, portrait.Height), Color.White);

			//name.Draw(sb, nameOffset);
		}

		public void Update(GameTime gameTime, KeyboardState state)
		{

		}
	}
}
