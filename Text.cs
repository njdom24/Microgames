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
	class Text
	{
		private Texture2D text;
		//private Texture2D background;
		private Color color;

		private int[] lengthRef;
		private int[] letterPos;
		private Point[] locations;

		public int width;
		//private bool highlighted;
		private int highlightWidth;

		public Text(ContentManager contentManager, string message, int customWidth = -1) : this(contentManager.Load<Texture2D>("Textbox/Text"), message, customWidth)
		{
			
		}
		public Text(Texture2D font, string message, int customWidth = -1)
		{
			lengthRef = new int[] { 2, 2, 3, 2, 5, 9, 7, 2, 3, 3, 3, 5, 2, 2, 2, 4, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 2, 3, 3, 5, 3, 4, 5, 6, 5, 5, 5, 4, 4, 5, 5, 1, 4, 5, 4, 7, 5, 5, 5, 5, 5, 5, 5, 5, 6, 7, 5, 5, 4, 5, 4, 6, 4, 5, 1, 4, 4, 4, 4, 4, 3, 4, 4, 1, 2, 4, 1, 7, 4, 4, 4, 4, 3, 4, 3, 4, 5, 7, 4, 4, 4, 2, 5, 2, 6, 7, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255 };
			letterPos = new int[message.Length];
			locations = new Point[message.Length];

			text = font;// contentManager.Load<Texture2D>("Textbox/Text");
			//background = contentManager.Load<Texture2D>("HighlightColor");

			for (int i = 0; i < message.Length; i++)
			{
				char letter = message.ElementAt(i);
				int line, off;
				line = (letter - ' ') / 16;
				off = (letter - ' ') % 16;
				locations[i] = new Point(16 * off, 16 * line);
				width += lengthRef[message.ElementAt(i) - ' '] + 1;
				if (i < message.Length - 1)
				{
					letterPos[i + 1] = lengthRef[message.ElementAt(i) - ' '] + 1;
					if (i > 0)
						letterPos[i + 1] += letterPos[i];
				}
			}

			color = Color.White;

			//Custom highlight width
			if (customWidth == -1)
				highlightWidth = width + 1;
			else
				highlightWidth = customWidth;
		}

		public void Draw(SpriteBatch sb, Vector2 pos)
		{
			for (int i = 0; i < locations.Length; i++)
			{
				sb.Draw(text, new Rectangle((int)(letterPos[i] + pos.X), (int) pos.Y, 16, 16), new Rectangle(locations[i].X, locations[i].Y, 16, 16), color);
			}
		}

		public void SetColor(Color c)
		{
			color = c;
		}
	}
}
