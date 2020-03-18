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

		public Icons(ContentManager contentManager, int width = 12, int height = 12, int count = 4)
		{
			desaturate = contentManager.Load<Effect>("Battle/Icons/IconChange");
			iconSet = contentManager.Load<Texture2D>("Battle/Icons/BattleHud");
			test = contentManager.Load<Texture2D>("Battle/Icons/Attack");
			this.width = width;
			this.height = height;
			this.count = count;
			x = (Game1.width - 64 - 2) / 2;
			y = Game1.height - 25;
			selector = new Selector(4, names: new string[] { "Attack", "Bag", "PSI", "Run" });
			index = 0;
		}

		public void Update(KeyboardState prevStateKb, MouseState prevStateM)
		{
			selector.Update(prevStateKb);

			MouseState state = Mouse.GetState();
			mouseX = (int)(state.X * Game1.resMultiplier);
			mouseY = (int)(state.Y * Game1.resMultiplier);

			index = (mouseX - x) / 16;

			if(!state.Equals(prevStateM) && mouseY > y && mouseY < y+16)
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
			//16 is the icon dimensions w/ border
			//12 is w/o border

			//2 is the "padding" created by the border pixels

			//Draw selected icon + border
			sb.Draw(iconSet, new Rectangle(16 * selected + x, y, 16, 16), new Rectangle(14, 12+12, 16, 16), Color.White);//Border
			sb.Draw(iconSet, new Rectangle(16 * selected + 2 + x, 2 + y, 12, 12), new Rectangle(12 * selected, 0, 12, 12), Color.White);//Icon

			//Draw borders before selected
			for (int i = 0; i < selected; i++)
			{
				sb.Draw(iconSet, new Rectangle(16 * i + 1 + x, 1 + y, 14, 14), new Rectangle(0, 12+12, 14, 14), Color.White);
			}
			//Draw borders after selected
			for (int i = count - 1; i > selected; i--)
			{
				sb.Draw(iconSet, new Rectangle(16 * i + 1 + x, 1 + y, 14, 14), new Rectangle(0, 12+12, 14, 14), Color.White);
			}

			//Draw icons before selected
			for(int i = 0; i < selected; i++)
			{
				sb.Draw(iconSet, new Rectangle(16 * i + 2 + x, 2 + y, 12, 12), new Rectangle(12 * i, 12, 12, 12), Color.White);
			}
			//Draw icons after selected
			for(int i = count-1; i > selected; i--)
			{
				sb.Draw(iconSet, new Rectangle(16 * i + 2 + x, 2 + y, 12, 12), new Rectangle(12 * i, 12, 12, 12), Color.White);
			}
		}
	}
}
