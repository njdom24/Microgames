using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;

namespace RPG
{
	interface Screen
	{
		void Update(GameTime dt);
		void Draw(SpriteBatch sb);
	}

	interface MiniScreen
	{
		byte Update(GameTime dt, KeyboardState prevStateKb, MouseState prevStateM);
		void Draw(SpriteBatch sb);
		void Unload();
	}
}
