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
	class Icons
	{
		private Effect desaturate;
		private Texture2D iconSet;
		private int width;
		private int height;
		private int count;
		private int x;
		private int y;

		public Icons(ContentManager contentManager, int width = 12, int height = 12, int count = 4, int x = 4, int y = 4)
		{
			desaturate = contentManager.Load<Effect>("Battle/Icons/IconChange");
			iconSet = contentManager.Load<Texture2D>("Battle/Icons/BattleHud");
			this.width = width;
			this.height = height;
			this.count = count;
			this.x = x;
			this.y = y;
		}

		public void Draw(SpriteBatch sb, int selected)//sb starts initialized with no effect
		{
			//sb.Draw(iconSet, new Rectangle(14 * selected, 0, 14, 14), new Rectangle(0, 12, 14, 14), Color.White);
			sb.Draw(iconSet, new Rectangle(16 * selected + x, y, 16, 16), new Rectangle(14, 12, 16, 16), Color.White);
			sb.Draw(iconSet, new Rectangle(16 * selected + 2 + x, 2 + y, 12, 12), new Rectangle(12 * selected, 0, 12, 12), Color.White);
			for (int i = 0; i < selected; i++)
			{
				sb.Draw(iconSet, new Rectangle(16 * i + 1 + x, 1 + y, 14, 14), new Rectangle(0, 12, 14, 14), Color.White);
			}
			for (int i = count - 1; i > selected; i--)
			{
				sb.Draw(iconSet, new Rectangle(16 * i + 1 + x, 1 + y, 14, 14), new Rectangle(0, 12, 14, 14), Color.White);
			}
			sb.End();

			sb.Begin(effect: desaturate);
			for(int i = 0; i < selected; i++)
			{
				sb.Draw(iconSet, new Rectangle(16 * i + 2 + x, 2 + y, 12, 12), new Rectangle(12 * i, 0, 12, 12), Color.White);
			}
			for(int i = count-1; i > selected; i--)
			{
				sb.Draw(iconSet, new Rectangle(16 * i + 2 + x, 2 + y, 12, 12), new Rectangle(12 * i, 0, 12, 12), Color.White);
			}
		}
	}
}
