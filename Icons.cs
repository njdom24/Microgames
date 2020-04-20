using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;

namespace RPG
{
	class Icons
	{
		private Effect desaturate;
		private Texture2D iconSet;
		private Texture2D test;
		private int width;
		private int height;
		private int count;
		private int x;
		private int y;

		private int index; //testing, delete later

		private int mouseX, mouseY;

		private Selector selector;

		public Icons(ContentManager contentManager, int width = 42, int height = 42, int count = 3)
		{
			desaturate = contentManager.Load<Effect>("Battle/Icons/IconChange");
			iconSet = contentManager.Load<Texture2D>("Battle/Icons/BattleHudNew");
			test = contentManager.Load<Texture2D>("Battle/Icons/Attack");
			this.width = width;
			this.height = height;
			this.count = count;
			x = (Game1.width - 48*3 ) / 2;
			y = Game1.height - 48;
			selector = new Selector(3, names: new string[] { "Attack", "Bag", "PSI" });
			index = 0;
		}

		public void Update(KeyboardState prevStateKb, MouseState prevStateM)
		{
			selector.Update(prevStateKb);

			MouseState state = Mouse.GetState();
			mouseX = (int)(state.X * Game1.resMultiplier);
			mouseY = (int)(state.Y * Game1.resMultiplier);

			index = (mouseX - x) / 48;

			if(!state.Equals(prevStateM) && mouseY > y && mouseY < y+48)
				selector.SetIndex(index);
		}

		public int GetIndex()
		{
			return selector.GetIndex();
		}

		public string GetSelectedName()
		{
			return selector.GetName();
		}

		public bool IndexChanged()
		{
			return selector.IndexChanged();
		}

		public void Draw(SpriteBatch sb, int selected)//sb starts initialized with no effect
		{
			//48 is the icon dimensions w/ border -> 48
			//42 is w/o border -> 42

			//2 is the "padding" created by the border pixels -> 3

			//Draw selected icon + border
			sb.Draw(iconSet, new Rectangle(48 * selected + x, y, 48, 48), new Rectangle(14, 42+42, 48, 48), Color.White);//Border
			sb.Draw(iconSet, new Rectangle(48 * selected + 3 + x, 3 + y, 42, 42), new Rectangle(42 * selected, 0, 42, 42), Color.White);//Icon

			//Draw borders before selected
			for (int i = 0; i < selected; i++)
			{
				sb.Draw(iconSet, new Rectangle(48 * i + 1 + x, 1 + y, 14, 14), new Rectangle(0, 42+42, 14, 14), Color.White);
			}
			//Draw borders after selected
			for (int i = count - 1; i > selected; i--)
			{
				sb.Draw(iconSet, new Rectangle(48 * i + 1 + x, 1 + y, 14, 14), new Rectangle(0, 42+42, 14, 14), Color.White);
			}

			//Draw icons before selected
			for(int i = 0; i < selected; i++)
			{
				sb.Draw(iconSet, new Rectangle(48 * i + 3 + x, 3 + y, 42, 42), new Rectangle(42 * i, 42, 42, 42), Color.White);
			}
			//Draw icons after selected
			for(int i = count-1; i > selected; i--)
			{
				sb.Draw(iconSet, new Rectangle(48 * i + 3 + x, 3 + y, 42, 42), new Rectangle(42 * i, 42, 42, 42), Color.White);
			}
		}
	}
}
