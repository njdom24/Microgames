using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace RPG
{
	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class Game1 : Game
	{
		private GraphicsDeviceManager manager;
		private Screen currentScreen;
		private SpriteBatch sb;
		private SpriteBatch render;
		private RenderTarget2D original;
		private RenderTarget2D nearest;
		private RenderTarget2D bilinear;
		private Point largestScale;
		public static readonly int width = 320;//400;
		public static readonly int height = 180;//240;
		public static float resMultiplier;

		public Game1()
		{
			manager = new GraphicsDeviceManager(this);
			manager.GraphicsProfile = GraphicsProfile.HiDef;
			IsFixedTimeStep = false;
			IsMouseVisible = true;
			Window.IsBorderless = true;
			Window.Title = "FF";
			manager.IsFullScreen = true;

			resMultiplier = (float)width / (float)System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
			int scale = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width / width;//400 for full, 800 for half
			//scale = 1;
			//Window.Position = new Point(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width / 2 - 200 * scale, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height / 2 - 120 * scale);

			//Window.Position = new Point(-width/2, -height/2);
			manager.PreferredBackBufferWidth = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width / 1;//400 * scale;
			manager.PreferredBackBufferHeight = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height / 1;//240 * scale;
			//manager.PreferredBackBufferWidth = 400;//400 * scale;
			//manager.PreferredBackBufferHeight = 240;//240 * scale;
			largestScale.X = width * scale;
			largestScale.Y = height * scale;

			Console.WriteLine("Scale: " + scale);

			Content.RootDirectory = "Content";

		}
		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			//GraphicsDevice.GraphicsProfile = GraphicsProfile.HiDef;
			GraphicsDevice.DepthStencilState = DepthStencilState.Default;
			PresentationParameters pp = GraphicsDevice.PresentationParameters;
			original = new RenderTarget2D(GraphicsDevice, Game1.width, Game1.height, false, SurfaceFormat.Color, DepthFormat.None, pp.MultiSampleCount, RenderTargetUsage.PreserveContents);
			nearest = new RenderTarget2D(GraphicsDevice, largestScale.X, largestScale.Y, false, SurfaceFormat.Color, DepthFormat.None, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);
			bilinear = new RenderTarget2D(GraphicsDevice, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height, false, SurfaceFormat.Color, DepthFormat.None, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);
			//nearest = bilinear = original;
			//currentScreen = new OldMap(GraphicsDevice, Content, 16, 16, 10, 10);
			//currentScreen = new Battle(Content, original, GraphicsDevice, pp);
			FileStream fs = File.Open("save.txt", System.IO.FileMode.OpenOrCreate);
			currentScreen = new MainScreen(Content, original, GraphicsDevice, pp, fs, this);
			//currentScreen = new Map(GraphicsDevice, Content, 48, 48, 10, 10);
			//currentScreen = new Environment();
			sb = new SpriteBatch(GraphicsDevice);
			render = new SpriteBatch(GraphicsDevice);
			//currentScreen = new TestScreen(Content);

			//tileset = Content.Load<Texture2D>("Corneria_gutter");
			base.Initialize();

			//light = Content.Load<Texture2D>("lightmask");
			//effect = Content.Load<Effect>("File");
			//effect.Parameters["lightMask"].SetValue(light);
			Console.WriteLine(Window.ClientBounds.Width);
			Console.WriteLine(Window.ClientBounds.Height);
			Console.WriteLine(largestScale.X);
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>  
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			//sb = new SpriteBatch(GraphicsDevice);

			// TODO: use this.Content to load your game content here
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// game-specific content.
		/// </summary>
		protected override void UnloadContent()
		{
			// TODO: Unload any non ContentManager content here
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected void HandleInput(GameTime gameTime)
		{
			//if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
			//	Exit();

			/*
			if (Keyboard.GetState().IsKeyDown(Keys.B))
			{
				Content.Unload();
				//Content = null;
				Content = new Microsoft.Xna.Framework.Content.ContentManager(this.Services);
				Content.RootDirectory = "Content";
				currentScreen = new OldMap(GraphicsDevice, Content, 16, 16, 10, 10);
			}
			*/

		}
		protected override void Update(GameTime gameTime)
		{
			//HandleInput(gameTime);
			currentScreen.Update(gameTime);

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.SetRenderTarget(original);
			//GraphicsDevice.Clear(Color.CornflowerBlue);
			GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;

			currentScreen.Draw(sb);

			myScale();
		}

		//Scales to nearest as high as accuracy allows, then scales the rest with bilinear and as-necessary black bars
		private void myScale()
		{
			Rectangle dst;
			float outputAspect = manager.PreferredBackBufferWidth / (float)manager.PreferredBackBufferHeight;
			float preferredAspect = Game1.width / (float)Game1.height;
			if (outputAspect <= preferredAspect)
			{
				// output is taller than it is wider, bars on top/bottom
				int presentHeight = (int)((manager.PreferredBackBufferWidth / preferredAspect) + 0.5f);
				int barHeight = (manager.PreferredBackBufferHeight - presentHeight) / 2;
				dst = new Rectangle(0, barHeight, manager.PreferredBackBufferWidth, presentHeight);
			}
			else
			{
				// output is wider than it is tall, bars left/right
				int presentWidth = (int)((manager.PreferredBackBufferHeight * preferredAspect) + 0.5f);
				int barWidth = (manager.PreferredBackBufferWidth - presentWidth) / 2;
				dst = new Rectangle(barWidth, 0, presentWidth, manager.PreferredBackBufferHeight);
			}

			//Scale original to highest point
			GraphicsDevice.SetRenderTarget(nearest);
			sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);
			sb.Draw(original, new Rectangle(0, 0, largestScale.X, largestScale.Y), Color.White);
			sb.End();

			//Scale highest point to full resolution
			GraphicsDevice.SetRenderTarget(bilinear);
			sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);
			sb.Draw(nearest, dst, Color.White);
			sb.End();

			//Render full resolution image
			GraphicsDevice.SetRenderTarget(null);
			sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);
			sb.Draw(bilinear, new Rectangle(0, 0, manager.PreferredBackBufferWidth, manager.PreferredBackBufferHeight), Color.White);
			sb.End();
		}

		private void scaleToDisplay()
		{
			float outputAspect = Window.ClientBounds.Width / (float)Window.ClientBounds.Height;
			float preferredAspect = width / (float)height;
			Rectangle dst;
			if (outputAspect <= preferredAspect)
			{
				// output is taller than it is wider, bars on top/bottom
				int presentHeight = (int)((Window.ClientBounds.Width / preferredAspect) + 0.5f);
				int barHeight = (Window.ClientBounds.Height - presentHeight) / 2;
				dst = new Rectangle(0, barHeight, Window.ClientBounds.Width, presentHeight);
			}
			else
			{
				// output is wider than it is tall, bars left/right
				int presentWidth = (int)((Window.ClientBounds.Height * preferredAspect) + 0.5f);
				int barWidth = (Window.ClientBounds.Width - presentWidth) / 2;
				dst = new Rectangle(barWidth, 0, presentWidth, Window.ClientBounds.Height);
			}
			GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 1.0f, 0);
			sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);
			sb.Draw(original, dst, Color.White);
			sb.End();
		}

		/*
        private void DrawLayer(int index, SpriteBatch batch)
        {
            int tileWidth = 16;
            int tileHeight = 16;

            Debug.WriteLine(tMap.TileLayers.Count);
            for (var i = 0; i < tMap.TileLayers[index].Tiles.Count; i++)
            {
                //Get the identification of the tile
                int gid = tMap.TileLayers[index].Tiles[i].GlobalIdentifier;

                // Empty tile, do nothing
                if (gid == 0) { }
                else
                {
                    int tileFrame = gid - 1;
                    int column = tileFrame % (tileset.Width / tileWidth);//tileset.width
                    int row = tileFrame / (tileset.Height / tileHeight);

                    float x = (i % tMap.Width) * tMap.TileWidth;
                    float y = (float)Math.Floor(i / (double)tMap.Width) * tMap.TileHeight;

                    //Put all the data together in a new rectangle
                    Rectangle tilesetRec = new Rectangle(tileWidth * column, tileHeight * row, tileWidth, tileHeight);

                    //Draw the tile that is within the tilesetRec
                    batch.Draw(tileset, new Rectangle((int)x, (int)y, tileWidth, tileHeight), tilesetRec, Color.White);
                }
            }
            
        }*/
	}
}