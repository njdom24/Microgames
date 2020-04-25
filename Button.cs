using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RPG
{
	public class Button
	{
		private Texture2D image, outline;
		private int posX, posY;
		private int height;
		private bool hovered;
		private Text overlayText;

		public Button(ContentManager contentManager, int posX, int posY, int height = 1, string text = "OK")
		{
			image = contentManager.Load<Texture2D>("OkButton");
			outline = contentManager.Load<Texture2D>("OkButton_Outline");
			this.posX = posX;
			this.posY = posY;
			hovered = false;
			this.height = height;

			overlayText = new Text(contentManager, text);
		}

		public void Draw(SpriteBatch sb)
		{
			if(hovered)
				sb.Draw(image, new Rectangle(posX, posY, image.Width, image.Height), Color.DarkOliveGreen);
			else
				sb.Draw(image, new Rectangle(posX, posY, image.Width, image.Height), Color.White);
			
			sb.Draw(outline, new Rectangle(posX, posY, image.Width, image.Height), Color.White);
			overlayText.Draw(sb, new Vector2(posX + image.Width/2 - overlayText.width/2, posY + image.Height/2 - 8*height));
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
