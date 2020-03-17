using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RPG
{
	public class TitleScreen : MiniScreen
	{
		private ContentManager contentManager;
		private MouseState state;
		private Texture2D background;
		private Menu options;
		//private Button startButton;
		int mouseX, mouseY;
		private enum Phase { Title, Settings };
		private Phase phase;
		//Intentionally keep inner and outer MouseStates separate so pausing doesn't act weird
		public TitleScreen(ContentManager contentManager)
		{
			this.contentManager = contentManager;
			mouseX = 0;
			mouseY = 0;
			state = Mouse.GetState();
			background = contentManager.Load<Texture2D>("Menus/Palette");
			options = new Menu(contentManager, new string[] { "Start Game", "Settings", "Test1", "Test2" }, 4, offsetX: Game1.width / 3, offsetY: Game1.height / 2);
			phase = Phase.Title;
			//startButton = new Button(contentManager, Game1.width / 2, Game1.height / 2);
		}

		public void Unload()
		{
			background.Dispose();
		}

		public byte Update(GameTime dt, KeyboardState prevStateKb, MouseState prevStateM)
		{
			state = Mouse.GetState();
			mouseX = (int) (state.X * Game1.resMultiplier);
			mouseY = (int) (state.Y * Game1.resMultiplier);
			options.Update(dt, prevStateKb, prevStateM, mouseX, mouseY);
			//startButton.Update(x, y, state.LeftButton == ButtonState.Pressed);

			switch (phase)
			{
				case Phase.Title:
					switch (options.GetSelection(prevStateKb, prevStateM, mouseX, mouseY))
					{
						case 0:
							return 1;
						case 1:
							phase = Phase.Settings;
							options = new Menu(contentManager, new string[] { "Volume", "Palette" }, 2, offsetX: Game1.width / 3, offsetY: Game1.height / 2);
							break;
						default:
							break;
					}

					break;
				case Phase.Settings:
					if (prevStateKb.IsKeyDown(Keys.Escape) && Keyboard.GetState().IsKeyUp(Keys.Escape))
					{
						phase = Phase.Title;
						options = new Menu(contentManager, new string[] { "Start Game", "Settings", "Test1", "Test2" }, 4, offsetX: Game1.width / 3, offsetY: Game1.height / 2);
					}
					break;
			}

			return 0;
		}

		void MiniScreen.Draw(SpriteBatch sb)
		{
			Console.WriteLine("Mouse X: " + state.X);
			sb.Begin();
			//Adjust for output buffer
			sb.Draw(background, new Rectangle(0, 0, Game1.width, Game1.height), Color.White);
			options.Draw(sb);
			//startButton.Draw(sb);
			sb.End();
		}
	}
}
