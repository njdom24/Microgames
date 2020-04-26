using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPG
{
	class ScrollingVal
	{
		private Texture2D scrollingNums, bg;
		private double numTimer;
		private int countdown;

		private int rollingHealth;
		private sbyte one, ten, hund;
		private bool movTen, movHund;

		public ScrollingVal(ContentManager contentManager, int defaultVal = 0)
		{
			scrollingNums = contentManager.Load<Texture2D>("Battle/Numbers/ScrollingNums");//5x8
			bg = contentManager.Load<Texture2D>("Menus/TransitionPlate_2");

			rollingHealth = defaultVal;
			hund = (sbyte)(rollingHealth / 100);
			ten = (sbyte)(rollingHealth / 10 % 10);
			one = (sbyte)(rollingHealth % 10);
		}

		public void Update(GameTime gameTime, int health)
		{
			rollingHealth = one + 10 * ten + 100 * hund;
			if (rollingHealth != health)
				numTimer += gameTime.ElapsedGameTime.TotalSeconds/10;

			if (numTimer > 0.01)//100 pixels per second
			{
				numTimer -= 0.01;

				if (rollingHealth != health)
				{
					countdown++;
				}
				else
					countdown = 0;

				if (countdown == 10)
				{
					movHund = false;
					movTen = false;
					countdown = 0;
					if (rollingHealth > health)
					{
						one--;
						if (one == -1)
						{
							one = 9;
							ten--;
							//movTen = true;
							if (ten == -1)
							{
								ten = 9;
								hund--;
								//movHund = true;
							}
						}
						else if (ten != 0 || hund != 0)
						{
							if (one == 0)
							{
								movTen = true;
								if (ten == 0)
									movHund = true;
							}
						}
					}
					else if (rollingHealth < health)//might not need if
					{
						one++;
						if (one == 10)
						{
							one = 0;
							ten++;
							//movTen = true;
							if (ten == 10)
							{
								ten = 0;
								hund++;
								//movHund = true;
							}
						}
						else
						{
							if (one == 9)
							{
								movTen = true;
								if (ten == 9)
									movHund = true;
							}
						}
					}
				}

			}
		}

		public void Draw(SpriteBatch sb, Vector2 pos, int health)
		{
			sb.Draw(bg, new Vector2((int)pos.X, (int)pos.Y), Color.White);

			if (rollingHealth < health)
			{
				sb.Draw(scrollingNums, new Rectangle((int)pos.X + 49, (int)pos.Y + 25, 5, 8), new Rectangle(0, one * 9 - 1 + countdown, 5, 8), Color.White);
			}
			else if (rollingHealth > health)
			{
				sb.Draw(scrollingNums, new Rectangle((int)pos.X + 49, (int)pos.Y + 25, 5, 8), new Rectangle(0, one * 9 - 1 - countdown, 5, 8), Color.White);
			}
			else
			{
				sb.Draw(scrollingNums, new Rectangle((int)pos.X + 49, (int)pos.Y + 25, 5, 8), new Rectangle(0, one * 9 - 1, 5, 8), Color.White);
			}

			if (rollingHealth < health)
			{
				if (movTen)
					sb.Draw(scrollingNums, new Rectangle((int)pos.X + 49 - 8, (int)pos.Y + 25, 5, 8), new Rectangle(0, ten * 9 - 1 + countdown, 5, 8), Color.White);
				else
					sb.Draw(scrollingNums, new Rectangle((int)pos.X + 49 - 8, (int)pos.Y + 25, 5, 8), new Rectangle(0, ten * 9 - 1, 5, 8), Color.White);

				if (movHund)
					sb.Draw(scrollingNums, new Rectangle((int)pos.X + 49 - 16, (int)pos.Y + 25, 5, 8), new Rectangle(0, hund * 9 - 1 + countdown, 5, 8), Color.White);
				else
					sb.Draw(scrollingNums, new Rectangle((int)pos.X + 49 - 16, (int)pos.Y + 25, 5, 8), new Rectangle(0, hund * 9 - 1, 5, 8), Color.White);
			}
			else if (rollingHealth > health)
			{
				if (movTen)
					sb.Draw(scrollingNums, new Rectangle((int)pos.X + 49 - 8, (int)pos.Y + 25, 5, 8), new Rectangle(0, ten * 9 - 1 - countdown, 5, 8), Color.White);
				else
					sb.Draw(scrollingNums, new Rectangle((int)pos.X + 49 - 8, (int)pos.Y + 25, 5, 8), new Rectangle(0, ten * 9 - 1, 5, 8), Color.White);

				if (movHund)
					sb.Draw(scrollingNums, new Rectangle((int)pos.X + 49 - 16, (int)pos.Y + 25, 5, 8), new Rectangle(0, hund * 9 - 1 - countdown, 5, 8), Color.White);
				else
					sb.Draw(scrollingNums, new Rectangle((int)pos.X + 49 - 16, (int)pos.Y + 25, 5, 8), new Rectangle(0, hund * 9 - 1, 5, 8), Color.White);
			}
			else
			{
				sb.Draw(scrollingNums, new Rectangle((int)pos.X + 49 - 8, (int)pos.Y + 25, 5, 8), new Rectangle(0, ten * 9 - 1, 5, 8), Color.White);
				sb.Draw(scrollingNums, new Rectangle((int)pos.X + 49 - 16, (int)pos.Y + 25, 5, 8), new Rectangle(0, hund * 9 - 1, 5, 8), Color.White);
			}
		}
	}
}
