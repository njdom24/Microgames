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
		private Texture2D background, sprite;
		private Menu options;
		private Effect paletteShader;
		private Button backButton;

		//private Button startButton;
		int mouseX, mouseY;
		private enum Phase { Title, Settings };
		private Phase phase;

		//Order should be lightest to darkest
		public static Tuple<Vector4, Vector4, Vector4, Vector4>[] palettes =
		{
			//Default (Stylish)
			Tuple.Create(new Color(250, 231, 190).ToVector4(),//skin
						 new Color(234, 80, 115).ToVector4(),//blush
						 new Color(113, 68, 123).ToVector4(),//purple
						 new Color(176, 108, 57).ToVector4()),//brown
			
			//Deuteranopia
			Tuple.Create(new Color(225, 255, 245).ToVector4(),
						 new Color(160, 95, 190).ToVector4(),
						 new Color(107, 174, 214).ToVector4(),
						 new Color(100, 170, 45).ToVector4()),

			//Protanopia
			Tuple.Create(new Color(251, 106, 74).ToVector4(),
			             new Color(165, 15, 21).ToVector4(),
						 new Color(160, 60, 155).ToVector4(),
						 new Color(115, 15, 40).ToVector4()),

			//Tritanopia
			Tuple.Create(
						 new Color(250, 185, 45).ToVector4(),
						 new Color(7, 81, 156).ToVector4(),
						 new Color(49, 130, 189).ToVector4(),
						 new Color(105, 90, 140).ToVector4()),
			
			//Game Boy
			Tuple.Create(new Color(192, 192, 128).ToVector4(),
						 new Color(160, 160, 96).ToVector4(),
						 new Color(128, 128, 64).ToVector4(),
						 new Color(64, 64, 0).ToVector4()),

		};

		//Intentionally keep inner and outer MouseStates separate so pausing doesn't act weird
		public TitleScreen(ContentManager contentManager, Effect paletteShader)
		{
			this.contentManager = contentManager;
			this.paletteShader = paletteShader;
			mouseX = 0;
			mouseY = 0;
			state = Mouse.GetState();
			background = contentManager.Load<Texture2D>("Menus/Palette");
			sprite = contentManager.Load<Texture2D>("Menus/wiz");
			options = new Menu(contentManager, new string[] { "Start Game", "Settings", "Test1", "Test2" }, 4, offsetX: Game1.width / 3, offsetY: Game1.height / 2);
			phase = Phase.Title;

			backButton = new Button(contentManager, 4, Game1.height - 30);

			//startButton = new Button(contentManager, Game1.width / 2, Game1.height / 2);
		}

		void SetColor(int index)
		{
			Tuple<Vector4, Vector4, Vector4, Vector4> rgba = palettes[index];
			paletteShader.Parameters["col_light"].SetValue(rgba.Item1);
			paletteShader.Parameters["col_extra"].SetValue(rgba.Item2);
			paletteShader.Parameters["col_med"].SetValue(rgba.Item3);
			paletteShader.Parameters["col_dark"].SetValue(rgba.Item4);
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
							options = new Menu(contentManager, new string[] { "Volume", "Palette", null, "P1", null, "P2", null, "P3", null, "P4", null, "P5" }, 2, 40, offsetX: Game1.width / 3, offsetY: Game1.height / 2);
							break;
						default:
							break;
					}

					break;
				case Phase.Settings:
					backButton.Update(mouseX, mouseY);
					if ((prevStateKb.IsKeyDown(Keys.Escape) && Keyboard.GetState().IsKeyUp(Keys.Escape))
					    || backButton.IsPressed(prevStateM))
					{
						phase = Phase.Title;
						options = new Menu(contentManager, new string[] { "Start Game", "Settings", "Test1", "Test2" }, 4, offsetX: Game1.width / 3, offsetY: Game1.height / 2);
					}
					if (options.GetSelectionY(prevStateKb, prevStateM, mouseX, mouseY) == 1)
					{ 
						int indX = options.GetSelectionX(prevStateKb, prevStateM, mouseX, mouseY);
						if (indX > 0)
							SetColor(indX - 1);
					}
					else switch (options.GetSelection(prevStateKb, prevStateM, mouseX, mouseY))
					{
						default:
							break;
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
			sb.Draw(sprite, new Rectangle(0, Game1.height - sprite.Height, sprite.Width, sprite.Height), new Rectangle(0, 0, sprite.Width, sprite.Height), Color.White);
			options.Draw(sb);
			//startButton.Draw(sb);
			if(phase == Phase.Settings)
				backButton.Draw(sb);
			sb.End();
		}
	}
}
