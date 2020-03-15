
using FarseerPhysics;
using FarseerPhysics.DebugView;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.TextureAtlases;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Graphics;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RPG
{
	class Map : MiniScreen
	{
		private Effect effect;
		private List<MapEntity> entityList;
		private DebugViewXNA debugView;
		private KeyboardState prevState;//used by both npcs and textboxes
		private Hud hud;
		private NPC[] npcs;
		private int tileWidth;
		private int tileHeight;
		private int width;
		private int height;
		private GraphicsDevice g;
		private Texture2D debug;
		//private Texture2D light;
		private ContentManager cont;
		private List<Body> blocks;
		private List<Vector2> blockDims;//x, y, width, height

		private Menu menu;

		private TiledMap tMap;
		private TiledMapRenderer mapRenderer;

		private bool speaking;
		private World world;
		public Player player;
		//private NPC npc;
		private List<Vector2> verticesList;
		private NPC talkingNPC;
		private Camera2D camera;

		public Camera2D Camera
		{
			get
			{
				return camera;
			}
		}

		public Map(GraphicsDevice pDevice, ContentManager content, int pTileWidth, int pTileHeight, int pWidth, int pHeight)
		{
			ConvertUnits.SetDisplayUnitToSimUnitRatio(100);
			entityList = new List<MapEntity>();
			verticesList = new List<Vector2>();
			//light = content.Load<Texture2D>("lightmask");
			effect = content.Load<Effect>("File");
			//effect.Parameters["lightMask"].SetValue(light);
			prevState = Keyboard.GetState();
			hud = new Hud(new string[] { "Undertale is bad,\nand so am I." }, content);
			speaking = false;
			blocks = new List<Body>();
			tileWidth = pTileWidth;
			tileHeight = pTileHeight;
			width = pWidth;
			height = pHeight;
			g = pDevice;
			cont = content;

			menu = new Menu(content, null);

			camera = new Camera2D(pDevice);
			debug = content.Load<Texture2D>("overworld_gutter");

			tMap = content.Load<TiledMap>("Map/Tazmily/Tazmily");
			mapRenderer = new TiledMapRenderer(pDevice);

			world = new World(Vector2.Zero);
			//world.ContactManager.OnBroadphaseCollision += BroadphaseHandler;
			//world.ContactManager.EndContact += EndContactHandler;
			debugView = new DebugViewXNA(world);
			debugView.LoadContent(pDevice, content);
			//debugView.AppendFlags(DebugViewFlags.DebugPanel);
			//debugView.AppendFlags(DebugViewFlags.PolygonPoints);
			//debugView.AppendFlags(DebugViewFlags.ContactPoints);
			//debugView.AppendFlags(DebugViewFlags.AABB);
			debugView.AppendFlags(DebugViewFlags.Shape);
			debugView.DefaultShapeColor = Color.Green;

			//camera.Position = player.body.Position;
			//Console.WriteLine("Scunt: " + tMap.ObjectLayers.Count);
			player = new Player(world, content, 16, 23);
			//npc = new NPC(world, content, colArray, 2, false, 16, 14, new string[] {"Weebs are worse\nthan fortnite\ngamers.", "Where's the lie?" });

			//Body b1 = BodyFactory.CreateRectangle(world, 3, 3, 1);

			blockDims = new List<Vector2>();
			MakeCollisionBodies();

			npcs = new NPC[] {
				//new NPC(world, content, player, 1, false, 12, 15, prevState, new string[] { "@You wanna see the special?\n@It's a detective story. Some kind of <Columbo>\n  knock-off.\n@Well, ya interested or not?", "@Get outta here." }),
				new NPC(world, content, player, 1, false, 12, 15, prevState, new string[] { "@Your name Steve Foley? Of course it is.\n  You are great. Fantastic.\n@Good job Steve.", "@Get outta here." }),
				//new NewNPC(world, content, player, 0, true, 18, 18, new string[] {"help"}, 4, 1)
			};
			entityList.Add(player);
			foreach (NPC n in npcs)
				entityList.Add(n);
		}

		void MiniScreen.Unload()
		{
			effect.Dispose();
			debug.Dispose();
			tMap.Dispose();
		}

		private void BroadphaseHandler(ref FixtureProxy fp1, ref FixtureProxy fp2)
		{
		}
		
		private void EndContactHandler(Contact contact)//unfinished
		{
			NPC tempNPC;
			if (contact.FixtureA.Body.UserData is NPC)
				tempNPC = (NPC)contact.FixtureA.Body.UserData;
			else if (contact.FixtureB.Body.UserData is NPC)
				tempNPC = (NPC)contact.FixtureB.Body.UserData;
			else
				return;

			tempNPC.ReapplyVelocity();
		}

		public void HandleInput(GameTime gameTime)
		{
			if (Keyboard.GetState().IsKeyDown(Keys.A) && prevState.IsKeyUp(Keys.A))
				hud.finishMessage();
		}

		public void DrawDebug(SpriteBatch sb)
		{
			for (int i = 0; i < blocks.Count; i++)
			{
				Body b = blocks[i];
				//sb.Draw(debug, new Rectangle(0, 0, 16, 16), new Rectangle(0, 0, 16, 16), Color.White);
				sb.Draw(debug, new Rectangle((int)b.Position.X - ((int)blockDims[i].X - 1) * 8, (int)b.Position.Y - ((int)blockDims[i].Y - 1) * 8, 16 * (int)blockDims[i].X, 16 * (int)blockDims[i].Y), new Rectangle(0, 0, 16, 16), Color.White);
			}
		}
		public void MakeCollisionBodies()
		{
			for (int i = 0; i < tMap.ObjectLayers[0].Objects.Length; i++)//Collision Layer
			{
				int width = (int)tMap.ObjectLayers[0].Objects[i].Size.Width;
				int height = (int)tMap.ObjectLayers[0].Objects[i].Size.Height;
				int x = (int)tMap.ObjectLayers[0].Objects[i].Position.X + width/2;
				int y = (int)tMap.ObjectLayers[0].Objects[i].Position.Y + height/2;

				//Body b = (BodyFactory.CreateRectangle(world, 16*3, 16*2, 0.2f, new Vector2(32 + 8*2, 32 + 8)));
				blocks.Add(BodyFactory.CreateRectangle(world, ConvertUnits.ToSimUnits(width), ConvertUnits.ToSimUnits(height), 0, new Vector2(ConvertUnits.ToSimUnits(x), ConvertUnits.ToSimUnits(y))));
				blocks[blocks.Count - 1].BodyType = BodyType.Static;
				blocks[blocks.Count - 1].UserData = null;
				blockDims.Add(new Vector2(width, height));
				//sb.Draw(debug, new Rectangle((int)b.Position.X - (width-1)*8, (int)b.Position.Y - (height-1)*8, 16*width, 16*height), new Rectangle(0, 0, 16, 16), Color.White);
			}
			//parsePolys("Triangles", true);
			parsePolys("Spangles", false);
		}

		void MiniScreen.Draw(SpriteBatch pSb)
		{
			mapRenderer.Draw(tMap.GetLayer("Ground"), camera.GetViewMatrix());
			pSb.Begin(transformMatrix: camera.GetViewMatrix(), samplerState: SamplerState.PointClamp);//SpriteSortMode.Immediate required for pixel shader

			//pSb.Begin();
			//pSb.Draw(debug, new Rectangle(0, 0, debug.Width, debug.Height), Color.White);
			//mapRenderer.Draw(tMap, camera.GetViewMatrix());
			foreach(MapEntity entity in entityList)
			{
				entity.Draw(pSb);
			}
			/*
			foreach (NPC n in npcs)
				n.Draw(pSb);
			player.Draw(pSb);
			*/
			//g.SamplerStates[0] = SamplerState.PointClamp;

			Vector2 tilePos = Vector2.Zero;

			//DrawDebug(pSb);
			pSb.End();
			mapRenderer.Draw(tMap.GetLayer("Hills"), camera.GetViewMatrix());
			pSb.Begin(samplerState: SamplerState.PointClamp);// effect: effect);
			//effect.CurrentTechnique.Passes[1].Apply();
			if (speaking)
				hud.Draw(pSb);

			//menu.Draw(pSb);
			pSb.End();

			Matrix proj = Matrix.CreateOrthographicOffCenter(0f, Game1.width, Game1.height, 0f, 0f, 1f);
			Matrix view = camera.GetViewMatrix();
			//debugView.RenderDebugData(ref proj, ref view);
		}

		byte MiniScreen.Update(GameTime gameTime, KeyboardState prevStateKb, MouseState prevStateM)
		{
			if(speaking)
				hud.Update(gameTime, prevState);
			if (hud.IsWaiting() || hud.isFinished() && hud.visible)
				talkingNPC.CloseMouth();

			HandleInput(gameTime);

			mapRenderer.Update(tMap, gameTime);

			menu.Update(gameTime, prevState, Mouse.GetState(), 0, 0);

			if (!speaking)
			{
				world.Step((float)gameTime.ElapsedGameTime.TotalSeconds);
				player.Update(gameTime, false);
				foreach (NPC n in npcs)
				{
					if(Keyboard.GetState().IsKeyDown(Keys.Space) && prevState.IsKeyUp(Keys.Space))
						if (n.body.LinearVelocity == Vector2.Zero && n.touchingPlayer)
						{
							//Make a check here to ensure the player is facing the NPC
							if (player.getStateH() == Player.HorizontalState.Left && n.touchingRight || player.getStateH() == Player.HorizontalState.Right && n.touchingLeft || player.getStateV() == Player.VerticalState.Up && n.touchingDown || player.getStateV() == Player.VerticalState.Down && n.touchingUp)
							{
								talkingNPC = n;
								n.speaking = true;
								n.FacePlayer(player.getStateH(), player.getStateV());
								speaking = true;
								hud = new Hud(n.messages, cont, n.textWidth, n.textHeight);
							}
						}

					n.Update(gameTime);
				}
				
			}
			else
			{
				player.Update(gameTime, true);
				foreach (NPC n in npcs)
					n.Update(gameTime);
				if (hud.messageComplete())
				{
					speaking = false;
					talkingNPC.ResetSpeaking();
					//foreach (NPC n in npcs)
						//n.ResetSpeaking();
				}
			}


			//camera.Position = new Vector2(player.body.Position.X - 400 / 2, player.body.Position.Y - 240 / 2 + 16);
			prevState = Keyboard.GetState();
			AdjustCamera();
			InsertionSortEntities();

			return 1;
		}

		private void InsertionSortEntities()
		{
			for (int i = 0; i < entityList.Count - 1; i++)
			{
				for (int j = i + 1; j > 0; j--)
				{
					if (entityList[j - 1] > entityList[j])
					{
						MapEntity temp = entityList[j - 1];
						entityList[j - 1] = entityList[j];
						entityList[j] = temp;
					}
				}
			}
		}

		private void AdjustCamera()
		{
			Vector2 tempPos = new Vector2((int)(ConvertUnits.ToDisplayUnits(player.body.Position.X) - Game1.width/2), (int)(ConvertUnits.ToDisplayUnits(player.body.Position.Y) - Game1.height/2 + 6 - 13));

			if (TooFarUp())
				tempPos.Y = 0;
			else if (TooFarDown())
				tempPos.Y = tMap.HeightInPixels - Game1.height;
			if (TooFarLeft())
				tempPos.X = 0;
			else if (TooFarRight())
				tempPos.X = tMap.WidthInPixels - Game1.width;

			//camera.Position = Vector2.Lerp(camera.Position, tempPos, 0.1f);
			camera.Position = tempPos;
		}
		private bool TooFarLeft()
		{
			return (ConvertUnits.ToDisplayUnits(player.body.Position.X) - Game1.width / 2 < 0);
		}

		private bool TooFarRight()
		{
			return (ConvertUnits.ToDisplayUnits(player.body.Position.X) - Game1.width / 2 + Game1.width > tMap.WidthInPixels);
		}

		private bool TooFarUp()
		{
			return (ConvertUnits.ToDisplayUnits(player.body.Position.Y) - Game1.height / 2 + 6 - 13 < 0);
		}

		private bool TooFarDown()
		{
			return (ConvertUnits.ToDisplayUnits(player.body.Position.Y) - Game1.height / 2 + 6 - 13 + Game1.height > tMap.HeightInPixels);
		}

		private void parsePolys(string group, bool isSensor)
		{
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "MonoRPG-GL.Properties.Tazmily.tmx";
            string s;
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();
                Console.WriteLine("I eat children: \n" + result);
                s = result;
            }
           

            //var bleh = Properties.Resource.Tazmily;
            //string s = System.Text.Encoding.Default.GetString(Properties.Resource.Tazmily);
            //string s = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<map version=\"1.0\" tiledversion=\"1.1.5\" orientation=\"orthogonal\" renderorder=\"right-down\" width=\"48\" height=\"48\" tilewidth=\"16\" tileheight=\"16\" infinite=\"0\" nextobjectid=\"73\">\n <tileset firstgid=\"1\" name=\"Tazmily\" tilewidth=\"16\" tileheight=\"16\" tilecount=\"1936\" columns=\"8\">\n  <image source=\"fulltileset.PNG\" trans=\"ff00ff\" width=\"128\" height=\"3872\"/>\n </tileset>\n <layer name=\"Ground\" width=\"48\" height=\"48\">\n  <data encoding=\"csv\">\n2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,\n2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,\n2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,\n2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,\n2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,\n2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,\n2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,\n2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,\n2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,\n2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,\n2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,\n2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,\n2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,13,2,2,2,2,\n2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,50,2,2,2,2,2,\n2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,142,2,2,2,2,2,\n2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,142,154,2,2,2,2,\n2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,142,154,2,2,2,2,\n2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,142,154,2,2,2,2,\n2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,142,154,2,2,2,2,\n2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,142,154,2,2,2,2,\n2,2,2,2,2,2,2,2,2,2,74,74,74,74,74,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,858,858,858,2,142,155,2,2,2,2,\n2,2,2,2,2,2,2,2,2,67,2,2,2,2,2,65,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,877,876,877,2,142,2,2,2,2,2,\n2,2,2,2,2,2,2,2,2,67,2,2,2,2,2,65,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,885,884,885,2,142,2,2,2,2,2,\n2,2,2,2,2,2,2,2,2,67,2,2,2,2,2,65,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,889,858,858,858,891,142,2,2,2,2,2,\n2,2,2,2,2,2,2,2,2,67,2,2,2,2,2,65,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,897,866,866,866,899,142,2,2,2,2,2,\n2,2,2,2,2,2,2,2,2,67,2,2,2,2,2,65,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,147,2,2,2,2,2,\n2,2,2,2,2,2,2,2,2,2,10,10,10,10,10,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,147,169,162,2,2,30,2,2,\n2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,154,118,119,116,114,117,119,119,\n2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,154,125,127,127,135,135,127,127,\n2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,154,2,121,123,2,2,121,123,\n2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,154,2,2,2,2,2,2,2,\n2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,154,2,2,2,2,2,2,2,\n2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,\n2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,\n2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,49,2,2,2,2,\n2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,\n2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,421,2,2,2,2,\n2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,421,2,2,2,2,\n2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,425,2,2,2,2,\n2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,\n2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,\n2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,\n2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,\n2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,\n2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,\n2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,\n2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,\n2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2\n</data>\n </layer>\n <layer name=\"Hills\" width=\"48\" height=\"48\">\n  <data encoding=\"csv\">\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,820,817,818,819,824,0,0,0,0,0,0,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,820,821,825,826,827,823,824,0,0,0,0,0,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,828,829,825,826,827,831,832,0,0,0,0,0,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,836,829,825,826,827,831,840,54,14,0,0,0,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,836,829,825,826,827,831,840,143,21,0,0,0,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,836,829,825,826,827,831,840,0,26,37,0,0,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,836,829,825,826,827,831,840,0,0,33,0,0,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,836,829,825,826,827,831,840,0,9,45,0,0,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,836,837,841,842,843,839,840,0,17,0,3,0,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,844,857,849,850,851,859,848,0,33,0,0,0,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,852,865,861,862,863,867,856,9,45,0,0,0,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,860,873,869,870,871,875,864,17,0,0,0,0,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,860,881,886,887,888,883,864,25,0,3,0,0,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,860,0,894,895,896,0,864,33,0,0,0,3,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,868,0,0,0,0,0,872,41,16,15,16,16,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,49,22,23,24,22,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,57,46,47,48,10,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,17,2,2,2,2,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,722,722,723,33,2,2,2,2,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,25,2,3,2,2,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,41,29,2,2,2,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,417,37,2,2,2,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,425,33,2,2,2,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,25,2,3,2,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,417,33,2,2,2,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,57,46,45,2,2,2,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,17,2,2,2,2,2,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,33,2,2,2,2,2,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,9,45,2,2,2,2,2,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,9,61,2,2,3,2,2,2,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,57,46,47,48,10,47,48,10,47,48,10,47,48,10,61,2,2,2,2,2,2,2,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,17,2,2,2,2,2,2,2,2,2,2,2,2,3,2,2,2,2,2,2,2,2,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,33,2,2,2,2,2,2,3,2,2,2,2,2,2,2,2,2,2,2,3,2,2,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,33,2,2,3,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,\n0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,33,2,2,2,2,2,2,2,2,2,3,2,2,2,2,2,2,2,2,2,2,2\n</data>\n </layer>\n <objectgroup name=\"BoxCollisions\">\n  <object id=\"2\" x=\"576\" y=\"208\" width=\"112\" height=\"192\"/>\n  <object id=\"5\" x=\"688\" y=\"480\" width=\"16\" height=\"144\"/>\n  <object id=\"8\" x=\"656\" y=\"688\" width=\"32\" height=\"16\"/>\n  <object id=\"12\" x=\"672\" y=\"624\" width=\"16\" height=\"64\"/>\n  <object id=\"13\" x=\"704\" y=\"480\" width=\"64\" height=\"16\"/>\n  <object id=\"14\" x=\"688\" y=\"400\" width=\"80\" height=\"16\"/>\n  <object id=\"15\" x=\"688\" y=\"336\" width=\"16\" height=\"64\"/>\n  <object id=\"16\" x=\"704\" y=\"288\" width=\"16\" height=\"48\"/>\n  <object id=\"17\" x=\"720\" y=\"240\" width=\"16\" height=\"48\"/>\n  <object id=\"18\" x=\"704\" y=\"224\" width=\"16\" height=\"32\"/>\n  <object id=\"19\" x=\"688\" y=\"208\" width=\"16\" height=\"32\"/>\n  <object id=\"20\" x=\"416\" y=\"704\" width=\"16\" height=\"64\"/>\n  <object id=\"22\" x=\"432\" y=\"704\" width=\"224\" height=\"16\"/>\n </objectgroup>\n <objectgroup name=\"Spangles\">\n  <object id=\"56\" x=\"577.636\" y=\"201.818\">\n   <polygon points=\"0,0 37.6364,-38.1818 70.9091,-38.1818 109.091,0.181818\"/>\n  </object>\n  <object id=\"57\" x=\"576\" y=\"400\">\n   <polygon points=\"0,0 -48,-48 0,-96\"/>\n  </object>\n  <object id=\"66\" x=\"144\" y=\"336\">\n   <polygon points=\"0,0 16,-16 96,-16 112,0 112,80 96,96 16,96 0,80\"/>\n  </object>\n  <object id=\"67\" x=\"144\" y=\"336\">\n   <polygon points=\"0,0 16,-16 96,-16 112,0 112,80 96,96 16,96 0,80\"/>\n  </object>\n </objectgroup>\n</map>";

            string line;
			int lastIndex = 0;
			lastIndex = s.IndexOf(group);
			
			string x = "Blargle";
			string y = "Flargle";
			s = s.Substring(lastIndex);//Go to objectgroup line
			s = s.Substring(0, s.IndexOf("</objectgroup>"));
			s = s.Substring(s.IndexOf('\n') + 1);//Go to object line
			List<List<Vector2>> myList = new List<List<Vector2>>();
			while (s.Contains("<object"))
			{
				
				string p1 = "";
				string p2 = "";
				myList.Add(new List<Vector2>());
				//Console.WriteLine("barles");
				s = s.Substring(s.IndexOf("x=\""));
				s = s.Substring(s.IndexOf("\"") + 1);
				//Console.WriteLine(s);
				x = s.Substring(0, s.IndexOf('\"'));

				s = s.Substring(s.IndexOf("y=\""));
				s = s.Substring(s.IndexOf("\"") + 1);
				y = s.Substring(0, s.IndexOf('\"'));
				s = s.Substring(s.IndexOf("polygon"));
				s = s.Substring(s.IndexOf('\"') + 1);
				float offX = float.Parse(x, CultureInfo.InvariantCulture.NumberFormat);
				float offY = float.Parse(y, CultureInfo.InvariantCulture.NumberFormat);
				line = s.Substring(0, s.IndexOf('\n'));
				//Console.WriteLine(line);
				do
				{
					p1 = line.Substring(0, line.IndexOf(','));
					line = line.Substring(line.IndexOf(',') + 1);
					p2 = line.Substring(0, line.IndexOf(' '));
					line = line.Substring(line.IndexOf(' ') + 1);
					myList[myList.Count - 1].Add(ConvertUnits.ToSimUnits(float.Parse(p1, CultureInfo.InvariantCulture.NumberFormat) + offX, float.Parse(p2, CultureInfo.InvariantCulture.NumberFormat)+offY));


				} while (line.IndexOf(' ') != -1);
				p1 = line.Substring(0, line.IndexOf(','));
				line = line.Substring(line.IndexOf(',') + 1);
				p2 = line.Substring(0, line.IndexOf('\"'));
				myList[myList.Count - 1].Add(ConvertUnits.ToSimUnits(float.Parse(p1, CultureInfo.InvariantCulture.NumberFormat)+offX, float.Parse(p2, CultureInfo.InvariantCulture.NumberFormat)+offY));
				s = s.Substring(s.IndexOf("</object>") + 9);
			}
			Body tempBody;
			foreach (List<Vector2> list in myList)
			{
				tempBody = BodyFactory.CreatePolygon(world, new FarseerPhysics.Common.Vertices(list), 0.1f);
				tempBody.IsSensor = isSensor;
				tempBody.UserData = group;
			}


		}
	}
}
