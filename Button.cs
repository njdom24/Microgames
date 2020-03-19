using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RPG
{
	public class Button
	{
		private Texture2D image;
		private int posX, posY;
		private bool hovered;

		public Button(ContentManager contentManager, int posX, int posY)
		{
			image = contentManager.Load<Texture2D>("OkButton");
			this.posX = posX;
			this.posY = posY;
			hovered = false;
		}

		public void Draw(SpriteBatch sb)
		{
			if(hovered)
				sb.Draw(image, new Rectangle(posX, posY, image.Width, image.Height), Color.DimGray);
			else
				sb.Draw(image, new Rectangle(posX, posY, image.Width, image.Height), Color.White);
		}

		//x and y are mouse coordinates
		//Prev unpressed, now pressed -> pressed
		//Prev pressed, now unpressed -> clicked
		public void Update(int x, int y)
		{
			hovered = (x > posX && x < image.Width + posX) && (y > posY && y < image.Height + posY);
		}

		public bool IsPressed(MouseState prevStateM)
		{
			bool pressed = prevStateM.LeftButton == ButtonState.Pressed && Mouse.GetState().LeftButton == ButtonState.Released;
			return hovered && pressed;
		}
	}
}
