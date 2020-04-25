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
	class Icon
	{
		public int texOffset;
		public string name;

		public Icon(int pos, string name)
		{
			texOffset = pos * 42;
			this.name = name;
		}
	}

	class Icons
	{
		private Effect desaturate;
		private Texture2D iconSet;
		private int width;
		private int height;
		private int count;
		private int x;
		private int y;

		private int index;
		private int mouseX, mouseY;
		private Selector selector;
		private Icon[] icons;

		public Icons(ContentManager contentManager, bool shuffle, int width = 42, int height = 42, int count = 3)
		{
			desaturate = contentManager.Load<Effect>("Battle/Icons/IconChange");
			iconSet = contentManager.Load<Texture2D>("Battle/Icons/BattleHudNew");
			this.width = width;
			this.height = height;
			this.count = count;
			x = (Game1.width - 48*3 ) / 2;
			y = Game1.height - 48;

			index = 0;

			icons = new Icon[]
			{
				new Icon(0, "Earth"),
				new Icon(1, "Fire"),
				new Icon(2, "Water")
			};

			if (shuffle)
			{
				Random rnd = new Random();
				Shuffle(rnd, icons);
			}

			selector = new Selector(3, names: new string[] { icons[0].name, icons[1].name, icons[2].name });
		}

		//Fisher-Yates Algorithm
		public static void Shuffle<T>(Random rng, T[] array)
		{
			int n = array.Length;
			while (n > 1)
			{
				int k = rng.Next(n--);
				T temp = array[n];
				array[n] = array[k];
				array[k] = temp;
			}
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

		public string GetMagic()
		{
			return icons[selector.GetIndex()].name;
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
			//48 is the icon dimensions w/ border
			//42 is w/o border
			//3 is the "padding" created by the border pixels

			//Draw selected icon + border
			sb.Draw(iconSet, new Rectangle(48 * selected + x, y, 48, 48), new Rectangle(14, 42+42, 48, 48), Color.White);//Border
			sb.Draw(iconSet, new Rectangle(48 * selected + 3 + x, 3 + y, 42, 42), new Rectangle(icons[selected].texOffset, 0, 42, 42), Color.White);//Icon

			//Draw icons before selected
			for(int i = 0; i < selected; i++)
			{
				sb.Draw(iconSet, new Rectangle(48 * i + 3 + x, 3 + y, 42, 42), new Rectangle(icons[i].texOffset, 42, 42, 42), Color.White);
			}
			//Draw icons after selected
			for(int i = count-1; i > selected; i--)
			{
				sb.Draw(iconSet, new Rectangle(48 * i + 3 + x, 3 + y, 42, 42), new Rectangle(icons[i].texOffset, 42, 42, 42), Color.White);
			}
		}
	}
}
