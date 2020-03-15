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
	class Icon
	{
		private Texture2D smallBorder;
		private Texture2D largeBorder;
		private Texture2D image;
		private Effect tint;
		private string name;

		public Icon(ContentManager cm, string name)
		{
			smallBorder = cm.Load<Texture2D>("Battle/Icons/BorderSmall");
			largeBorder = cm.Load<Texture2D>("Battle/Icons/BorderLarge");
			image = cm.Load<Texture2D>("Battle/Icons/" + name);
			tint = cm.Load<Effect>("Battle/Icons/IconChange");

			if(name.Length < 6)
			{
				for (int i = 0; i < 5 - name.Length; i++)
					name = " " + name;
			}
			this.name = name;
		}

		public void Draw(SpriteBatch sb, int x, int y)
		{
			sb.Draw(largeBorder, new Rectangle(x, y, largeBorder.Width, largeBorder.Height), Color.White);
			sb.Draw(image, new Rectangle(x + 2, y + 2, image.Width, image.Height), Color.White);
			/*
			else
			{
				sb.Draw(largeBorder, new Rectangle(x, y, largeBorder.Width, largeBorder.Height), Color.White);
				sb.Draw(image, new Rectangle(x + 2, y + 2, image.Width, image.Height), Color.White);
			}

			sb.End();
			*/
		}

		public void DrawDesaturated(SpriteBatch sb, int x, int y)
		{
			sb.Draw(smallBorder, new Rectangle(x + 1, y + 1, smallBorder.Width, smallBorder.Height), Color.White);
			//tint.CurrentTechnique.Passes[0].Apply();
			sb.Draw(image, new Rectangle(x + 1 + 1, y + 1 + 1, image.Width, image.Height), Color.White);
		}

		public string GetName()
		{
			return name;
		}

	}
}
